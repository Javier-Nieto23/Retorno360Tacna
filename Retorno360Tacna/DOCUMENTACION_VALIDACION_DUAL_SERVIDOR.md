# Documentación: Validación Dual de Servidor para Cálculo de Retorno

## Resumen

Se implementó una lógica de validación dual que verifica en qué servidores se encuentran tanto la base de datos seleccionada como la base de datos de la glosa (TR_Glosa), permitiendo determinar si son externas o locales y aplicar la estrategia de consulta apropiada.

---

## Problema Original

El sistema anteriormente asumía que todas las bases de datos estaban en el mismo servidor o no diferenciaba correctamente entre:
- **Base de datos seleccionada** (contiene `Di_Pedimento` y `De_Pedimento`)
- **Base de datos de glosa** (contiene `TR_Glosa`)

Esto causaba errores de login cuando:
1. La base seleccionada estaba en un servidor externo
2. La glosa estaba en otro servidor diferente
3. Se intentaba hacer un JOIN directo entre bases en servidores distintos

**Ejemplo del error:**
```
Cannot open database "SEERT_TODCO" requested by the login.
Login failed for user 'MedTiempos'.
```

---

## Solución Implementada

### 1. Nuevo Método: `ObtenerConexionDesdeNomTablaRazon`

Este método consulta la tabla `NOM_TABLARAZON` para obtener la información de conexión de la **base de datos seleccionada**.

```csharp
private ConexionExternaInfo ObtenerConexionDesdeNomTablaRazon(string baseDatos)
```

**Consulta SQL:**
```sql
SELECT TOP 1 
    N.ConnExterna,
    N.IdConexion,
    C.NombreConexion,
    C.Servidor,
    C.UsuarioSQL,
    C.PasswordSQL
FROM NOM_TABLARAZON N
LEFT JOIN Conexiones C ON N.IdConexion = C.IdConexion
WHERE N.NOMBRE_TABLA = @BaseDatos
```

**Características:**
- ✅ Busca en `NOM_TABLARAZON` (tabla de bases seleccionables)
- ✅ Hace JOIN con `Conexiones` usando `IdConexion`
- ✅ Devuelve información del servidor externo si existe
- ✅ Usa caché independiente (clave: `"NOM_{baseDatos}"`)
- ✅ Retorna conexión principal si no hay configuración externa

---

### 2. Validación Dual en `ValidarPedimentosCruzados`

El método ahora obtiene información de conexión de **dos fuentes diferentes**:

```csharp
// PASO 1: Base seleccionada desde NOM_TABLARAZON
var conexionBaseSeleccionada = ObtenerConexionDesdeNomTablaRazon(baseDatosSeleccionada);
string servidorSeleccionada = conexionBaseSeleccionada.Servidor ?? conexionInfo.Servidor ?? "Servidor Principal";

// PASO 2: Base de glosa desde RAZONXTABLA
var conexionBaseOrigen = ObtenerConexionExterna(baseDatosOrigen);
string servidorOrigen = conexionBaseOrigen.Servidor ?? conexionInfo.Servidor ?? "Servidor Principal";
```

**Comparación de Servidores:**
```csharp
if (!servidorSeleccionada.Equals(servidorOrigen, StringComparison.OrdinalIgnoreCase))
{
    // Servidores DIFERENTES → Usar estrategia multi-servidor
    return ValidarPedimentosCruzadosMultiServidor(...);
}
else
{
    // MISMO servidor → JOIN optimizado en SQL
    // Ejecutar query directo con JOIN entre bases
}
```

---

### 3. Estrategia Multi-Servidor Mejorada

Cuando los servidores son diferentes, se aplica la siguiente lógica:

```csharp
private List<PedimentoComparacion> ValidarPedimentosCruzadosMultiServidor(
    string baseDatosSeleccionada,
    string baseDatosOrigen,
    DateTime fechaInicio,
    DateTime fechaFin)
{
    // Obtener información de conexión de ambas bases
    var conexionBaseSeleccionada = ObtenerConexionDesdeNomTablaRazon(baseDatosSeleccionada);
    var conexionBaseOrigen = ObtenerConexionExterna(baseDatosOrigen);

    // PASO 1: Consulta a la base seleccionada (con su conexión)
    var pedimentosBaseSeleccionada = ObtenerPedimentosDeBaseDatos(
        baseDatosSeleccionada, 
        conexionBaseSeleccionada, 
        fechaInicio, 
        fechaFin
    );

    // PASO 2: Consulta a la glosa (con su conexión)
    var pedimentosGlosa = ObtenerPedimentosDeGlosa(
        baseDatosOrigen, 
        conexionBaseOrigen, 
        fechaInicio, 
        fechaFin
    );

    // PASO 3: Validación cruzada en memoria
    // Comparar en .NET en lugar de JOIN en SQL
}
```

---

### 4. Métodos Actualizados con Parámetro de Conexión

Los métodos ahora reciben explícitamente la información de conexión:

```csharp
private List<PedimentoComparacion> ObtenerPedimentosDeBaseDatos(
    string baseDatos,
    ConexionExternaInfo conexionExternaInfo,  // ← Nueva firma
    DateTime fechaInicio,
    DateTime fechaFin)

private List<PedimentoComparacion> ObtenerPedimentosDeGlosa(
    string baseDatos,
    ConexionExternaInfo conexionExternaInfo,  // ← Nueva firma
    DateTime fechaInicio,
    DateTime fechaFin)
```

Esto permite que cada método use la conexión correcta según el origen de la configuración.

---

## Flujo de Decisión

```
┌─────────────────────────────────────────────────────┐
│ Usuario selecciona Razón Social y Base de Datos    │
└─────────────────┬───────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────┐
│ ValidarPedimentosCruzados()                         │
├─────────────────────────────────────────────────────┤
│ 1. ObtenerConexionDesdeNomTablaRazon(base_selec)   │
│    → Consulta NOM_TABLARAZON + Conexiones          │
│    → Obtiene: Servidor A, Usuario A, Pass A        │
│                                                     │
│ 2. ObtenerConexionExterna(base_glosa)              │
│    → Consulta RAZONXTABLA + Conexiones             │
│    → Obtiene: Servidor B, Usuario B, Pass B        │
│                                                     │
│ 3. Comparar Servidor A vs Servidor B               │
└─────────────┬───────────────────────────────────────┘
              │
      ┌───────┴───────┐
      │               │
      ▼               ▼
┌─────────────┐  ┌──────────────────┐
│ A == B      │  │ A != B           │
│ (MISMO)     │  │ (DIFERENTES)     │
└──────┬──────┘  └────────┬─────────┘
       │                  │
       ▼                  ▼
┌──────────────┐  ┌────────────────────────────┐
│ JOIN directo │  │ Multi-servidor:            │
│ en SQL       │  │ 1. Query a Servidor A      │
│              │  │ 2. Query a Servidor B      │
│ ✅ Rápido    │  │ 3. Comparar en memoria     │
│ ✅ Eficiente │  │                            │
└──────────────┘  │ ✅ Evita errores de login  │
                  │ ✅ Soporta bases externas  │
                  └────────────────────────────┘
```

---

## Tablas Involucradas

### NOM_TABLARAZON
Define las bases de datos seleccionables por el usuario:

| Campo         | Descripción                                      |
|---------------|--------------------------------------------------|
| `IdTabla`     | ID único de la tabla                             |
| `NOMBRE_TABLA`| Nombre de la base de datos (ej: SEERT_TODCO)    |
| `ConnExterna` | 'S' = externa, 'N' o NULL = local               |
| `IdConexion`  | FK a tabla `Conexiones` (servidor, usuario, pwd) |

### RAZONXTABLA
Define qué base de datos contiene la glosa de cada razón social:

| Campo         | Descripción                               |
|---------------|-------------------------------------------|
| `IdRazon`     | FK a NOM_RAZONR                           |
| `DB`          | Nombre de la base con TR_Glosa            |
| `ConnExterna` | 'S' = externa, 'N' o NULL = local        |
| `IdConexion`  | FK a tabla `Conexiones`                   |

### Conexiones
Almacena credenciales y configuración de servidores:

| Campo            | Descripción                        |
|------------------|------------------------------------|
| `IdConexion`     | ID único                           |
| `NombreConexion` | Descripción de la conexión         |
| `Servidor`       | IP o nombre del servidor SQL       |
| `UsuarioSQL`     | Usuario de SQL Server              |
| `PasswordSQL`    | Contraseña del usuario SQL         |

---

## Escenarios de Uso

### Escenario 1: Base y Glosa en el Mismo Servidor
```
Razón: RASMUSSEN DE TECATE SA DE CV
Base seleccionada: SEERT_RASMUSSEN
Base de glosa: SEERT_RASMUSSEN

NOM_TABLARAZON:
  SEERT_RASMUSSEN → IdConexion = NULL → Servidor Principal

RAZONXTABLA:
  RASMUSSEN → SEERT_RASMUSSEN → IdConexion = NULL → Servidor Principal

✅ RESULTADO: Mismo servidor → JOIN optimizado
```

### Escenario 2: Base Externa, Glosa Local
```
Razón: TRANSPORTADORA TODO CONTINENTE SA DE CV
Base seleccionada: SEERT_TODCO
Base de glosa: (base de glosa configurada en RAZONXTABLA)

NOM_TABLARAZON:
  SEERT_TODCO → IdConexion = 2 → Servidor: 172.20.21.36

RAZONXTABLA:
  IdRazon 17 → DB: xxx → IdConexion = NULL → Servidor Principal

⚠️ RESULTADO: Servidores diferentes → Multi-servidor
```

### Escenario 3: Ambas Externas, Mismo Servidor
```
NOM_TABLARAZON:
  BASE_A → IdConexion = 3 → Servidor: 192.168.1.100

RAZONXTABLA:
  RAZON_X → BASE_B → IdConexion = 3 → Servidor: 192.168.1.100

✅ RESULTADO: Mismo servidor externo → JOIN optimizado
```

### Escenario 4: Ambas Externas, Servidores Diferentes
```
NOM_TABLARAZON:
  BASE_A → IdConexion = 3 → Servidor: 192.168.1.100

RAZONXTABLA:
  RAZON_X → BASE_B → IdConexion = 4 → Servidor: 192.168.1.200

⚠️ RESULTADO: Servidores diferentes → Multi-servidor
```

---

## Logs de Diagnóstico

El sistema genera logs detallados en modo DEBUG:

```
🔍 VALIDAR PEDIMENTOS CRUZADOS
   ═══════════════════════════════════════════════════════════
   📋 BASE SELECCIONADA (Di_Pedimento/De_Pedimento):
      • Nombre: SEERT_TODCO
      • Tabla origen: NOM_TABLARAZON
      • Conexión externa: Sí
      • IdConexion: 2
      • Servidor: 172.20.21.36

   📊 BASE ORIGEN/GLOSA (TR_Glosa):
      • Nombre: SEERT_PRINCIPAL
      • Tabla origen: RAZONXTABLA
      • Conexión externa: No
      • IdConexion: NULL (usa servidor principal)
      • Servidor: 172.20.21.35
   ═══════════════════════════════════════════════════════════
   ⚠️ ADVERTENCIA: Las bases de datos están en SERVIDORES DIFERENTES
      • Base seleccionada: SEERT_TODCO → 172.20.21.36
      • Base glosa: SEERT_PRINCIPAL → 172.20.21.35
   ✅ Usando estrategia alternativa: Consultas separadas + validación en memoria
```

---

## Ventajas de la Implementación

1. **✅ Flexibilidad Total**
   - Soporta bases en servidor principal
   - Soporta bases en servidores externos
   - Soporta combinaciones mixtas

2. **✅ Optimización Automática**
   - JOIN directo cuando es posible (mismo servidor)
   - Validación en memoria solo cuando es necesario

3. **✅ Evita Errores de Login**
   - Cada conexión usa las credenciales correctas
   - No se intenta acceder a bases con credenciales incorrectas

4. **✅ Mantenibilidad**
   - Lógica centralizada y clara
   - Logs detallados para diagnóstico
   - Caché independiente para cada tabla de configuración

5. **✅ Compatibilidad**
   - Funciona con configuraciones existentes
   - No rompe comportamiento previo
   - Backward compatible

---

## Configuración Requerida

Para que el sistema funcione correctamente, asegúrate de:

1. **NOM_TABLARAZON**: Todas las bases seleccionables deben tener:
   - `NOMBRE_TABLA` correcto
   - `ConnExterna = 'S'` si está en servidor externo
   - `IdConexion` apuntando a `Conexiones` si es externa

2. **RAZONXTABLA**: Todas las razones sociales deben tener:
   - `DB` con el nombre de la base de glosa
   - `ConnExterna = 'S'` si está en servidor externo
   - `IdConexion` apuntando a `Conexiones` si es externa

3. **Conexiones**: Todos los `IdConexion` referenciados deben existir con:
   - `Servidor` (IP o nombre)
   - `UsuarioSQL` válido
   - `PasswordSQL` correcto

---

## Cambios en el Código

### Archivos Modificados
- `Retorno360Tacna\SERVICES\RetornoService.cs`

### Nuevos Métodos
- `ObtenerConexionDesdeNomTablaRazon(string baseDatos)`
- Sobrecarga de `ObtenerConexionParaBaseDatos(string baseDatos, ConexionExternaInfo conexionExt)`

### Métodos Modificados
- `ValidarPedimentosCruzados(...)` - Agregada validación dual de servidores
- `ValidarPedimentosCruzadosMultiServidor(...)` - Ahora pasa conexiones explícitas
- `ObtenerPedimentosDeBaseDatos(...)` - Nueva firma con `ConexionExternaInfo`
- `ObtenerPedimentosDeGlosa(...)` - Nueva firma con `ConexionExternaInfo`

---

## Compilación

✅ **Resultado:** Exitoso (0 errores, 38 advertencias de otros archivos)

```bash
dotnet build "Retorno360Tacna\Retorno360Tacna.csproj" --no-incremental
```

---

## Próximos Pasos Recomendados

1. **Verificar configuración de tablas:**
   - Ejecutar script `Verificar_Configuracion_SEERT_TODCO.sql`
   - Validar que todos los `IdConexion` existan en `Conexiones`

2. **Pruebas:**
   - Caso: Base y glosa en servidor principal
   - Caso: Base externa, glosa local
   - Caso: Ambas externas, mismo servidor
   - Caso: Ambas externas, servidores diferentes

3. **Monitoreo:**
   - Revisar logs de DEBUG durante cálculos de retorno
   - Verificar que las conexiones se enruten correctamente
   - Validar tiempos de respuesta en estrategia multi-servidor

---

**Fecha de implementación:** 2024  
**Versión:** 1.0  
**Estado:** ✅ Compilado y listo para pruebas
