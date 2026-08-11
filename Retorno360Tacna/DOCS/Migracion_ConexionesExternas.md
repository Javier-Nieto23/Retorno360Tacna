# 🔄 Migración a Sistema de Conexiones Externas (RAZONXTABLA)

## 📋 Resumen de Cambios

Se reemplazó el sistema de `GestorConexiones` (auto-descubrimiento) por un **sistema basado en metadatos** de la tabla `RAZONXTABLA`.

---

## 🎯 Nuevo Sistema

### Antes (GestorConexiones):
```
┌──────────────────────────────────────┐
│ RetornoService                       │
├──────────────────────────────────────┤
│ ConfigurarConexionesSecundarias()    │
│  ├─ ObtenerBasesDatosDeServidor()    │
│  ├─ ObtenerBasesDatosDesdeTablaRazon│
│  └─ AgregarConexionSecundaria()      │
│                                      │
│ GestorConexiones                     │
│  ├─ Dictionary<servidor, bases[]>    │
│  ├─ ObtenerConexion(baseDatos)       │
│  └─ EsConexionSecundaria(baseDatos)  │
└──────────────────────────────────────┘
```

### Ahora (RAZONXTABLA + Conexiones):
```
┌──────────────────────────────────────┐
│ RetornoService                       │
├──────────────────────────────────────┤
│ ObtenerConexionExterna(baseDatos)    │
│  └─ Lee RAZONXTABLA:                 │
│      ├─ ConnExterna ('S', 'N', NULL) │
│      └─ IdConexion → Conexiones      │
│                                      │
│ ObtenerConexionParaBaseDatos(BD)     │
│  ├─ Si ConnExterna = 'S'             │
│  │   └─ Usa conexión de tabla        │
│  └─ Si ConnExterna ≠ 'S'             │
│      └─ Usa conexión principal       │
│                                      │
│ Cache: Dictionary<BD, ConexionInfo>  │
└──────────────────────────────────────┘
```

---

## 📊 Estructura de Tablas

### RAZONXTABLA (Actualizada)
```sql
CREATE TABLE RAZONXTABLA (
    IdRazon INT,
    NOMBRE_RAZON VARCHAR(255),
    DB VARCHAR(100),
    ConnExterna VARCHAR(1),    -- 🆕 'S', 'N' o NULL
    IdConexion INT,            -- 🆕 FK a tabla Conexiones
    ...
)
```

**Ejemplos**:
| IdRazon | NOMBRE_RAZON | DB | ConnExterna | IdConexion |
|---------|--------------|-----|-------------|------------|
| 1 | MAM DE LA FRONTERA SA DE CV | SEERT_Able | NULL | NULL |
| 2 | BAJA TEK SA DE CV | SEERT_Adela | NULL | NULL |
| 15 | VIDRIOS MUNDIALES SA DE CV | SEERT_VIDRIOS | S | 1002 |

### Conexiones
```sql
CREATE TABLE Conexiones (
    IdConexion INT PRIMARY KEY,
    NombreConexion VARCHAR(100),
    Servidor VARCHAR(50),      -- IP o nombre del servidor
    UsuarioSQL VARCHAR(50),
    PasswordSQL VARCHAR(100),
    TipoMotor VARCHAR(50),
    Activo BIT
)
```

**Ejemplos**:
| IdConexion | NombreConexion | Servidor | UsuarioSQL | PasswordSQL | Activo |
|------------|----------------|----------|------------|-------------|--------|
| 1 | TJ-SQLSRVR03 | 172.20.20.26 | MedTiempos | T3ch4dm1n | 1 |
| 2 | TJ-SQLSRVR19 | 172.20.20.26 | mjnieto | admin1234 | 1 |
| 1002 | tj-sqlsrv04 | 172.20.21.33 | jnieto | admin1234 | 1 |

---

## 🔧 Cambios en el Código

### Archivos Eliminados
- ❌ `Retorno360Tacna\CNX\GestorConexiones.cs`
- ❌ `Retorno360Tacna\CNX\GestorConexionesEjemplos.cs`
- ❌ `Retorno360Tacna\DOCS\GestorConexiones_Guia.md`

### Archivos Nuevos
- ✅ `Retorno360Tacna\MODELS\ConexionExternaInfo.cs` - Modelo para conexión externa

### Archivos Modificados
- ✅ `Retorno360Tacna\SERVICES\RetornoService.cs` - Reescrito completamente

---

## 🔍 Métodos Principales

### 1. `ObtenerConexionExterna(baseDatos)`
```csharp
// Lee RAZONXTABLA y Conexiones
// Devuelve ConexionExternaInfo con:
//   - TieneConexionExterna (bool)
//   - IdConexion (int?)
//   - Servidor, UsuarioSQL, PasswordSQL
//   - Cache interno para performance
```

### 2. `ObtenerConexionParaBaseDatos(baseDatos)`
```csharp
// Devuelve SqlConnection apropiada
// Si ConnExterna = 'S':
//   → Usa conexión de tabla Conexiones
// Si ConnExterna ≠ 'S':
//   → Usa conexión principal
```

### 3. `ObtenerServidorDeBaseDatos(baseDatos)`
```csharp
// Devuelve nombre del servidor donde está la BD
// Útil para diagnósticos y logs
```

### 4. `LimpiarCacheConexiones()`
```csharp
// Limpia el cache interno
// Útil si se modifican datos en RAZONXTABLA
```

---

## ✅ Ventajas del Nuevo Sistema

| Aspecto | Antes (GestorConexiones) | Ahora (RAZONXTABLA) |
|---------|--------------------------|---------------------|
| **Configuración** | Hardcodeada en código | Metadata en BD |
| **Flexibilidad** | Requiere recompilar | Cambio en BD solamente |
| **Mantenimiento** | Modificar código C# | SQL UPDATE/INSERT |
| **Nuevos servidores** | Editar `ConfigurarConexionesSecundarias()` | INSERT en Conexiones |
| **Nuevas razones** | Re-descubrir bases | UPDATE RAZONXTABLA |
| **Performance** | Auto-descubrimiento al inicio | Cache + consulta bajo demanda |
| **Escalabilidad** | Limitado por código | Ilimitado (BD) |
| **Diagnóstico** | Logs complejos | Simple: SELECT FROM RAZONXTABLA |

---

## 📝 Cómo Agregar una Nueva Conexión Externa

### Paso 1: Registrar el Servidor
```sql
INSERT INTO RetornoMaster.dbo.Conexiones 
(IdConexion, NombreConexion, Servidor, UsuarioSQL, PasswordSQL, TipoMotor, Activo)
VALUES 
(1003, 'Servidor Nuevo', '172.20.21.50', 'usuario', 'password', 'SQL Server', 1);
```

### Paso 2: Marcar Razón Social como Externa
```sql
UPDATE RetornoMaster.dbo.RAZONXTABLA
SET 
    ConnExterna = 'S',
    IdConexion = 1003
WHERE 
    DB = 'SEERT_NUEVARAZON';
```

### Paso 3: Verificar
```sql
SELECT 
    R.IdRazon,
    R.NOMBRE_RAZON,
    R.DB,
    R.ConnExterna,
    R.IdConexion,
    C.Servidor,
    C.UsuarioSQL
FROM RAZONXTABLA R
LEFT JOIN Conexiones C ON R.IdConexion = C.IdConexion
WHERE R.DB = 'SEERT_NUEVARAZON';
```

### Paso 4: (Opcional) Limpiar Cache
```csharp
// En el código, si es necesario:
retornoService.LimpiarCacheConexiones();
```

---

## 🐛 Troubleshooting

### Error: "Los datos de conexión están incompletos"
```
CAUSA: IdConexion apunta a registro inexistente o inválido en Conexiones
SOLUCIÓN:
  1. Verificar que existe el IdConexion en tabla Conexiones
  2. Verificar que Activo = 1
  3. Verificar que Servidor, UsuarioSQL, PasswordSQL no son NULL
```

### Error: Login attempt failed
```
CAUSA: Credenciales incorrectas en tabla Conexiones
SOLUCIÓN:
  1. Verificar usuario SQL existe en el servidor
  2. Verificar contraseña correcta
  3. Ejecutar SQL_Configurar_Servidor_Secundario.sql
```

### Performance lenta al calcular retornos
```
CAUSA: Cache no está funcionando
SOLUCIÓN:
  1. Verificar que ObtenerConexionExterna() guarda en cache
  2. Si modificaste RAZONXTABLA, ejecutar LimpiarCacheConexiones()
```

### Razón social no usa conexión externa cuando debería
```
CAUSA: ConnExterna no está marcado como 'S'
SOLUCIÓN:
  UPDATE RAZONXTABLA 
  SET ConnExterna = 'S', IdConexion = [ID_CORRECTO]
  WHERE DB = '[BASE_DATOS]';
```

---

## 🔄 Migración de Datos Existentes

### Si ya tienes servidores secundarios configurados:

```sql
-- Paso 1: Verificar qué bases están en servidor secundario
SELECT DISTINCT R.DB
FROM RAZONXTABLA R
WHERE R.DB IN (
    -- Lista de bases conocidas en servidor secundario
    'SEERT_VIDRIOS'
);

-- Paso 2: Registrar el servidor secundario en Conexiones (si no existe)
IF NOT EXISTS (SELECT 1 FROM Conexiones WHERE Servidor = '172.20.21.33')
BEGIN
    INSERT INTO Conexiones 
    (IdConexion, NombreConexion, Servidor, UsuarioSQL, PasswordSQL, TipoMotor, Activo)
    VALUES 
    (1002, 'tj-sqlsrv04', '172.20.21.33', 'jnieto', 'admin1234', 'SQL Server', 1);
END

-- Paso 3: Actualizar RAZONXTABLA para marcar conexiones externas
UPDATE RAZONXTABLA
SET 
    ConnExterna = 'S',
    IdConexion = 1002
WHERE 
    DB = 'SEERT_VIDRIOS';

-- Paso 4: Verificar migración
SELECT 
    R.IdRazon,
    R.NOMBRE_RAZON,
    R.DB,
    R.ConnExterna,
    C.Servidor,
    C.UsuarioSQL
FROM RAZONXTABLA R
LEFT JOIN Conexiones C ON R.IdConexion = C.IdConexion
WHERE R.ConnExterna = 'S';
```

---

## 📊 Diagnóstico y Monitoreo

### Consulta de diagnóstico completa:
```sql
SELECT 
    R.IdRazon,
    R.NOMBRE_RAZON,
    R.DB,
    CASE 
        WHEN R.ConnExterna = 'S' THEN 'Externa'
        ELSE 'Principal'
    END AS TipoConexion,
    COALESCE(C.Servidor, '172.20.20.26') AS Servidor,
    COALESCE(C.UsuarioSQL, 'MedTiempos') AS Usuario,
    CASE 
        WHEN C.Activo = 1 OR C.Activo IS NULL THEN 'Activo'
        ELSE 'Inactivo'
    END AS Estado
FROM RAZONXTABLA R
LEFT JOIN Conexiones C ON R.IdConexion = C.IdConexion
ORDER BY TipoConexion, R.NOMBRE_RAZON;
```

### Validar integridad:
```sql
-- Verificar razones con ConnExterna = 'S' pero sin IdConexion
SELECT *
FROM RAZONXTABLA
WHERE ConnExterna = 'S' AND IdConexion IS NULL;

-- Verificar razones con IdConexion pero ConnExterna != 'S'
SELECT *
FROM RAZONXTABLA
WHERE ConnExterna != 'S' AND IdConexion IS NOT NULL;

-- Verificar IdConexion inválidos
SELECT R.*
FROM RAZONXTABLA R
LEFT JOIN Conexiones C ON R.IdConexion = C.IdConexion
WHERE R.ConnExterna = 'S' AND C.IdConexion IS NULL;
```

---

## 🎯 Resultado Final

✅ **Sistema simplificado** - Menos código, más configuración  
✅ **Basado en metadatos** - Cambios sin recompilación  
✅ **Escalable** - Agregar servidores = INSERT en BD  
✅ **Mantenible** - Configuración centralizada en RAZONXTABLA  
✅ **Performance** - Cache interno evita consultas repetidas  
✅ **Diagnóstico fácil** - SQL simple para verificar configuración  

---

**Fecha**: Enero 2026  
**Versión**: 3.0 - Sistema de Conexiones Externas Basado en Metadata
