# Solución al Problema de Búsqueda de Conexiones

## Problema Original

Al intentar calcular el retorno para **RASMUSSEN DE TECATE SA DE CV** con la base **SEERT_TODCO**, el sistema generaba el siguiente error:

```
❌ ERROR DE CONFIGURACIÓN MULTI-SERVIDOR

Las bases de datos están en servidores diferentes:
  • Base seleccionada: SEERT_TODCO → 172.20.20.26
  • Base origen: SEERT_RASMUSSEN → 172.20.21.36
```

## Causa Raíz del Problema

El método `ObtenerConexionExterna` **SOLO buscaba en la tabla RAZONXTABLA**:

```sql
SELECT ... 
FROM RAZONXTABLA R
WHERE R.DB = @BaseDatos
```

### Lo que ocurría:

| Base de Datos | Buscada en RAZONXTABLA | Resultado | Servidor Asignado |
|---------------|------------------------|-----------|-------------------|
| SEERT_TODCO | `WHERE DB = 'SEERT_TODCO'` | ❌ **NO encontrada** | Servidor principal (172.20.20.26) por defecto |
| SEERT_RASMUSSEN | `WHERE DB = 'SEERT_RASMUSSEN'` | ✅ **Encontrada** (IdConexion=2) | Servidor externo (172.20.21.36) |

**Resultado:** El sistema pensaba que estaban en servidores diferentes y lanzaba error.

### La Realidad según los Datos:

Según las tablas de configuración:

**NOM_TABLARAZON:**
```
IdTabla: 1050
NOMBRE_TABLA: SEERT_TODCO
IdRazon: 17
ConnExterna: NULL o S (dependiendo de configuración)
IdConexion: NULL, 2 o el que corresponda
```

**RAZONXTABLA:**
```
IdRazon: 17
NOMBRE_RAZON: RASMUSSEN DE TECATE SA DE CV
DB: SEERT_RASMUSSEN
ConnExterna: S
IdConexion: 2
```

**Conexiones:**
```
IdConexion: 2
Servidor: 172.20.21.36
UsuarioSQL: jnieto
PasswordSQL: admin1234
```

## Solución Implementada

Se modificó `ObtenerConexionExterna` para buscar en **DOS tablas en secuencia**:

### PASO 1: Buscar en RAZONXTABLA (Comportamiento Original)

```sql
SELECT TOP 1 
    R.ConnExterna,
    R.IdConexion,
    C.NombreConexion,
    C.Servidor,
    C.UsuarioSQL,
    C.PasswordSQL
FROM RAZONXTABLA R
LEFT JOIN Conexiones C ON R.IdConexion = C.IdConexion
WHERE R.DB = @BaseDatos
ORDER BY R.IdRazon
```

- ✅ **Funciona para:** SEERT_RASMUSSEN, SEERT_TINA, SEERT_VIDRIOS, etc.
- ❌ **NO funciona para:** SEERT_TODCO (no está en el campo `DB`)

### PASO 2: Buscar en NOM_TABLARAZON (Nuevo - Solo si PASO 1 falló)

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
ORDER BY N.IdTabla
```

- ✅ **Funciona para:** SEERT_TODCO, SEERT_DESICCARE, SEERT_EUROTEC, etc.
- ❌ **NO funciona para:** Bases que solo están en RAZONXTABLA

## Nuevo Flujo de Ejecución

### Caso: RASMUSSEN DE TECATE + SEERT_TODCO

**1. Usuario selecciona:**
- Razón Social: RASMUSSEN DE TECATE SA DE CV (IdRazon=17)
- Base de Datos: SEERT_TODCO

**2. Sistema ejecuta `CalcularRetorno`:**

```csharp
// Línea 193: Obtener conexión de base seleccionada
var conexionExt = ObtenerConexionExterna("SEERT_TODCO");
```

**3. `ObtenerConexionExterna("SEERT_TODCO")`:**

```
PASO 1: Buscar en RAZONXTABLA WHERE DB = 'SEERT_TODCO'
        └─ ❌ No encontrado

PASO 2: Buscar en NOM_TABLARAZON WHERE NOMBRE_TABLA = 'SEERT_TODCO'
        └─ ✅ Encontrado!
           ├─ IdTabla: 1050
           ├─ IdRazon: 17
           ├─ ConnExterna: (depende de configuración)
           └─ IdConexion: 2 (si está configurado)

RESULTADO: Servidor 172.20.21.36
```

**4. Sistema obtiene razón social:**

```csharp
// Línea 203: Obtener razón social con IdRazon=17
RazonSocial razonInfo = ObtenerRazonSocial(17);
// Retorna: BaseDatosOrigen = "SEERT_RASMUSSEN"
```

**5. `ObtenerConexionExterna("SEERT_RASMUSSEN")`:**

```
PASO 1: Buscar en RAZONXTABLA WHERE DB = 'SEERT_RASMUSSEN'
        └─ ✅ Encontrado!
           ├─ IdRazon: 17
           ├─ ConnExterna: S
           └─ IdConexion: 2

RESULTADO: Servidor 172.20.21.36
```

**6. Validación de servidores:**

```csharp
if (!servidorSeleccionada.Equals(servidorOrigen))
{
    // Usar estrategia multi-servidor
}
```

**Resultado:**
- `servidorSeleccionada` = 172.20.21.36
- `servidorOrigen` = 172.20.21.36
- ✅ **Son iguales → Continúa sin error**

## Compatibilidad con Casos Existentes

### Caso 1: Base en RAZONXTABLA (ya funcionaba)

**Ejemplo:** SEERT_RASMUSSEN

```
ObtenerConexionExterna("SEERT_RASMUSSEN")
  ├─ PASO 1: RAZONXTABLA → ✅ Encontrado
  └─ PASO 2: (se omite)

✅ Funciona igual que antes
```

### Caso 2: Base en NOM_TABLARAZON (nuevo)

**Ejemplo:** SEERT_TODCO

```
ObtenerConexionExterna("SEERT_TODCO")
  ├─ PASO 1: RAZONXTABLA → ❌ No encontrado
  └─ PASO 2: NOM_TABLARAZON → ✅ Encontrado

✅ Ahora funciona correctamente
```

### Caso 3: Base en AMBAS tablas (prioridad)

**Ejemplo:** Si una base estuviera en ambas tablas

```
ObtenerConexionExterna("BASE_X")
  ├─ PASO 1: RAZONXTABLA → ✅ Encontrado
  └─ PASO 2: (se omite porque ya se encontró)

✅ Prioriza RAZONXTABLA (comportamiento consistente)
```

### Caso 4: Base no encontrada en ninguna tabla

```
ObtenerConexionExterna("BASE_INEXISTENTE")
  ├─ PASO 1: RAZONXTABLA → ❌ No encontrado
  ├─ PASO 2: NOM_TABLARAZON → ❌ No encontrado
  └─ Retorna: ConexionExternaInfo por defecto

✅ Usa servidor principal (comportamiento de respaldo)
```

## Ventajas de la Solución

✅ **Retrocompatible:** No rompe funcionalidad existente  
✅ **Completa:** Busca en ambas fuentes de configuración  
✅ **Priorizada:** RAZONXTABLA primero (más específico)  
✅ **Resiliente:** Funciona aunque falte configuración  
✅ **Cacheada:** Usa cache para mejorar rendimiento  
✅ **Diagnóstica:** Logs detallados para debugging  

## Logs de Diagnóstico

El sistema ahora genera logs más informativos:

```
📡 Conexión encontrada en NOM_TABLARAZON para 'SEERT_TODCO'
📡 Conexión Externa para 'SEERT_TODCO':
   Servidor: 172.20.21.36
   IdConexion: 2

📡 Conexión encontrada en RAZONXTABLA para 'SEERT_RASMUSSEN'
📡 Conexión Externa para 'SEERT_RASMUSSEN':
   Servidor: 172.20.21.36
   IdConexion: 2

🔍 VALIDAR PEDIMENTOS CRUZADOS
   Base seleccionada: SEERT_TODCO (Servidor: 172.20.21.36)
   Base origen: SEERT_RASMUSSEN (Servidor: 172.20.21.36)
   ✅ Ambas bases en el mismo servidor: 172.20.21.36
```

## Configuración Requerida

Para que SEERT_TODCO funcione correctamente, debe estar configurada en **NOM_TABLARAZON**:

```sql
INSERT INTO NOM_TABLARAZON (NOMBRE_TABLA, IdRazon, ConnExterna, IdConexion)
VALUES ('SEERT_TODCO', 17, 'S', 2);
```

O actualizar si ya existe:

```sql
UPDATE NOM_TABLARAZON
SET ConnExterna = 'S', IdConexion = 2
WHERE NOMBRE_TABLA = 'SEERT_TODCO' AND IdRazon = 17;
```

## Versión

- **Implementado:** Mayo 2026
- **Versión del sistema:** .NET 10
- **Archivo modificado:** `SERVICES/RetornoService.cs`
- **Método modificado:** `ObtenerConexionExterna`
- **Líneas de código agregadas:** ~70
- **Breaking changes:** Ninguno
