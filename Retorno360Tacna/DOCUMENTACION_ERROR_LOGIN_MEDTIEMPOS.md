# Error de Autenticación: Login failed for user 'MedTiempos'

## Error Completo

```
Cannot open database "SEERT_TODCO" requested by the login. The login failed.
Login failed for user 'MedTiempos'.
```

## Causa del Error

El error indica que el sistema está intentando conectarse al servidor externo **172.20.21.36** usando las credenciales del servidor principal:

- **Usuario incorrecto:** MedTiempos (servidor principal: 172.20.20.26)
- **Usuario correcto:** jnieto (servidor externo: 172.20.21.36)

## ¿Por qué ocurre esto?

El método `ObtenerConexionExterna` busca la configuración de **SEERT_TODCO** en:

1. **RAZONXTABLA** → ❌ No encontrado (porque SEERT_TODCO no está en el campo `DB`)
2. **NOM_TABLARAZON** → ⚠️ Encontrado PERO sin `IdConexion` configurado (o NULL)

Cuando `IdConexion` es NULL, el sistema usa la **conexión principal** por defecto:

```csharp
if (conexionExt.UsarConexionPrincipal)  // IdConexion == NULL
{
    // Usar conexión principal (MedTiempos)
    Conexion conexion = new Conexion(
        conexionInfo.Servidor,      // 172.20.20.26
        conexionInfo.UsuarioSQL,    // MedTiempos
        conexionInfo.PasswordSQL,
        baseDatos
    );
}
```

Luego intenta ejecutar:
```sql
FROM [SEERT_TODCO].dbo.Di_Pedimento
```

Pero SEERT_TODCO está en el servidor **172.20.21.36**, no en el principal, por lo que:
- ❌ **MedTiempos** no tiene acceso a SEERT_TODCO en el servidor externo
- ❌ Error de login

## Solución

### Opción 1: Verificar y Corregir Configuración en Base de Datos

**Ejecutar el script:** `SQL_Scripts/Verificar_Configuracion_SEERT_TODCO.sql`

Este script:
1. ✅ Verifica la configuración actual
2. ✅ Muestra qué está mal
3. ✅ Proporciona script de corrección (comentado)

**Configuración esperada en NOM_TABLARAZON:**

```sql
UPDATE NOM_TABLARAZON
SET 
    ConnExterna = 'S',
    IdConexion = 2
WHERE NOMBRE_TABLA = 'SEERT_TODCO' 
  AND IdRazon = 17;
```

**Configuración esperada en Conexiones:**

```sql
-- IdConexion = 2 debe tener:
Servidor: 172.20.21.36
UsuarioSQL: jnieto
PasswordSQL: admin1234
Activo: 1
```

### Opción 2: Verificar Desde la Aplicación

La aplicación ahora genera un mensaje de error más detallado:

```
Error al obtener pedimentos de SEERT_TODCO:
Mensaje SQL: Cannot open database "SEERT_TODCO"...

Configuración detectada:
  • Base de datos: SEERT_TODCO
  • Tiene conexión externa: False
  • IdConexion: NULL
  • Servidor configurado: Servidor principal

SOLUCIÓN:
Verifica que la base de datos 'SEERT_TODCO' tenga configurado correctamente:
1. ConnExterna = 'S' en NOM_TABLARAZON
2. IdConexion con el servidor correcto en NOM_TABLARAZON
3. Usuario/contraseña correctos en la tabla Conexiones
```

## Configuración Correcta Completa

### Tabla: NOM_TABLARAZON

| Campo | Valor | Descripción |
|-------|-------|-------------|
| IdTabla | 1050 | ID único de la tabla |
| NOMBRE_TABLA | SEERT_TODCO | Nombre de la base de datos |
| IdRazon | 17 | RASMUSSEN DE TECATE SA DE CV |
| **ConnExterna** | **'S'** | ✅ Indica que usa conexión externa |
| **IdConexion** | **2** | ✅ Apunta al servidor externo |

### Tabla: RAZONXTABLA

| Campo | Valor | Descripción |
|-------|-------|-------------|
| IdRazon | 17 | RASMUSSEN DE TECATE SA DE CV |
| NOMBRE_RAZON | RASMUSSEN DE TECATE SA DE CV | Nombre completo |
| DB | SEERT_RASMUSSEN | Base de datos de la glosa |
| **ConnExterna** | **'S'** | ✅ Indica que usa conexión externa |
| **IdConexion** | **2** | ✅ Apunta al servidor externo |

### Tabla: Conexiones

| Campo | Valor | Descripción |
|-------|-------|-------------|
| **IdConexion** | **2** | ID de la conexión externa |
| NombreConexion | TJ-SQL2019-03 | Nombre descriptivo |
| **Servidor** | **172.20.21.36** | ✅ IP del servidor externo |
| **UsuarioSQL** | **jnieto** | ✅ Usuario correcto |
| **PasswordSQL** | **admin1234** | ✅ Contraseña correcta |
| TipoMotor | SQLServer | Motor de base de datos |
| Activo | 1 | Conexión activa |

## Flujo Correcto Esperado

Una vez corregida la configuración:

```
1. Usuario selecciona RASMUSSEN + SEERT_TODCO
   ↓
2. ObtenerConexionExterna("SEERT_TODCO")
   ├─ Busca en RAZONXTABLA → No encontrado
   ├─ Busca en NOM_TABLARAZON → ✅ Encontrado
   │  └─ ConnExterna='S', IdConexion=2
   ├─ Busca en Conexiones → ✅ Encontrado
   │  └─ Servidor=172.20.21.36, Usuario=jnieto
   └─ Retorna: Conexión al servidor externo
   ↓
3. ObtenerConexionExterna("SEERT_RASMUSSEN")
   ├─ Busca en RAZONXTABLA → ✅ Encontrado
   │  └─ ConnExterna='S', IdConexion=2
   ├─ Busca en Conexiones → ✅ Encontrado
   │  └─ Servidor=172.20.21.36, Usuario=jnieto
   └─ Retorna: Conexión al servidor externo
   ↓
4. Validación de servidores
   ├─ SEERT_TODCO → 172.20.21.36
   ├─ SEERT_RASMUSSEN → 172.20.21.36
   └─ ✅ MISMO SERVIDOR → Continúa sin error
   ↓
5. Ejecuta consultas con usuario correcto (jnieto)
   └─ ✅ LOGIN EXITOSO
```

## Verificación Rápida en SQL

```sql
-- ¿SEERT_TODCO tiene IdConexion configurado?
SELECT NOMBRE_TABLA, IdRazon, ConnExterna, IdConexion
FROM NOM_TABLARAZON
WHERE NOMBRE_TABLA = 'SEERT_TODCO' AND IdRazon = 17;

-- ¿El IdConexion apunta al servidor correcto?
SELECT IdConexion, Servidor, UsuarioSQL
FROM Conexiones
WHERE IdConexion = 2;
```

**Resultado esperado:**
```
NOMBRE_TABLA: SEERT_TODCO
ConnExterna: S
IdConexion: 2

Servidor: 172.20.21.36
UsuarioSQL: jnieto
```

## Pasos de Solución

1. ✅ **Ejecutar script de verificación**
   ```sql
   -- Archivo: SQL_Scripts/Verificar_Configuracion_SEERT_TODCO.sql
   ```

2. ✅ **Si la verificación falla, descomentar sección de corrección**
   ```sql
   -- Descomenta: /* APLICANDO CORRECCIONES */
   ```

3. ✅ **Ejecutar nuevamente el script**

4. ✅ **Reiniciar la aplicación**

5. ✅ **Volver a intentar el cálculo**

## Mejoras Implementadas en el Código

### 1. Búsqueda Dual
- Ahora busca en **RAZONXTABLA** y **NOM_TABLARAZON**
- Cubre todas las fuentes de configuración

### 2. Mensaje de Error Mejorado
- Muestra la configuración detectada
- Indica exactamente qué falta
- Proporciona pasos de solución

### 3. Logs de Diagnóstico
```
🔌 Conexión abierta para SEERT_TODCO
   Servidor: 172.20.21.36
   Base de datos: SEERT_TODCO
```

## Resultado Final

Una vez aplicada la corrección:

✅ **SEERT_TODCO** → IdConexion=2 → Servidor 172.20.21.36 → Usuario jnieto  
✅ **SEERT_RASMUSSEN** → IdConexion=2 → Servidor 172.20.21.36 → Usuario jnieto  
✅ **Mismo servidor** → Sin error de multi-servidor  
✅ **Usuario correcto** → Sin error de login  
✅ **Cálculo de retorno exitoso** 🎉

## Versión

- **Implementado:** Mayo 2026
- **Archivos modificados:** `RetornoService.cs`
- **Script creado:** `SQL_Scripts/Verificar_Configuracion_SEERT_TODCO.sql`
