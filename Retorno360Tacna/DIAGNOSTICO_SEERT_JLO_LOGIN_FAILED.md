# 🐛 DIAGNÓSTICO: Login Failed con SEERT_Jlo

## 📋 CASO REPORTADO

### Configuración
```
Razón Social: MAM
Base de datos seleccionada: SEERT_Jlo (servidor externo)
Base de datos glosa (MAM): SEERT_Able (probablemente servidor principal)
```

### Error
```
Cannot open database "SEERT_Jlo" requested by the login. The login failed.
Login failed for user 'MedTiempos'.
```

### Stack Trace
```
at RetornoService.ValidarPedimentosCruzados(string, string, DateTime, DateTime)
```

---

## 🔍 ANÁLISIS DEL PROBLEMA

### El flujo esperado según el usuario:

1. ✅ Validar en `NOM_TABLARAZON` la base seleccionada y a qué servidor pertenece
2. ✅ Validar en `RAZONXTABLA` la razón seleccionada y la base que tiene la Glosa
3. ✅ Comparar los servidores de la razón y la base seleccionada
4. ⚠️ Si son diferentes → usar método de conexión externa
5. ⚠️ Tomar los datos de la base seleccionada y almacenarlos en variables
6. ⚠️ Usar esas variables en el query con el que calculamos el porcentaje en TR_Glosa

### El problema detectado:

El error ocurre en `ValidarPedimentosCruzados`, **NO** en `ValidarPedimentosCruzadosMultiServidor`.

Esto significa que la validación está determinando **INCORRECTAMENTE** que ambas bases están en la misma conexión, por lo que:

1. ❌ Llama a la ruta de JOIN directo (`ValidarPedimentosCruzados`)
2. ❌ Se conecta con las credenciales de `SEERT_Jlo`
3. ❌ Intenta ejecutar un JOIN entre `SEERT_Jlo` y `SEERT_Able`
4. ❌ Falla porque `SEERT_Able` está en otro servidor o requiere otras credenciales

---

## 🔎 POSIBLES CAUSAS

### Causa 1: Servidor de SEERT_Able es NULL

Si `SEERT_Able` no tiene configuración en `RAZONXTABLA.ConnExterna` o `IdConexion`, entonces:

```csharp
var conexionBaseOrigen = ObtenerConexionExterna(baseDatosOrigen);
// conexionBaseOrigen.Servidor = NULL
// conexionBaseOrigen.IdConexion = NULL

string servidorOrigen = conexionBaseOrigen.Servidor ?? conexionInfo.Servidor ?? "Servidor Principal";
// servidorOrigen = conexionInfo.Servidor (172.20.20.26 - servidor principal)
```

Y si `SEERT_Jlo` también tiene un problema similar, ambas quedan con "Servidor Principal" y la validación falla.

---

### Causa 2: Servidor de SEERT_Jlo es NULL (no configurado en NOM_TABLARAZON)

Si `SEERT_Jlo` no está correctamente configurado en `NOM_TABLARAZON`:

```csharp
var conexionBaseSeleccionada = ObtenerConexionDesdeNomTablaRazon(baseDatosSeleccionada);
// conexionBaseSeleccionada.Servidor = NULL
// conexionBaseSeleccionada.IdConexion = NULL

string servidorSeleccionada = conexionBaseSeleccionada.Servidor ?? conexionInfo.Servidor ?? "Servidor Principal";
// servidorSeleccionada = conexionInfo.Servidor (172.20.20.26)
```

Resultado:
```
servidorSeleccionada = "172.20.20.26" (por fallback)
servidorOrigen = "172.20.20.26" (por fallback)
→ mismoServidor = true ❌ (INCORRECTO!)
```

---

### Causa 3: SEERT_Able no está en RAZONXTABLA

Si la búsqueda en `ObtenerConexionExterna(baseDatosOrigen)` no encuentra `SEERT_Able` en `RAZONXTABLA`:

```csharp
// PASO 1: Buscar en RAZONXTABLA
string sql = @"SELECT ... FROM RAZONXTABLA WHERE DB = @BaseDatos";
// No encuentra nada

// PASO 2: Buscar en NOM_TABLARAZON (fallback)
string sqlNomTablaRazon = @"SELECT ... FROM NOM_TABLARAZON WHERE NOMBRE_TABLA = @BaseDatos";
// Puede o no encontrar
```

Si no encuentra en ninguna de las dos, retorna un objeto con todo en NULL.

---

## 📊 DIAGNÓSTICO CON LOGGING MEJORADO

He agregado logging adicional que mostrará:

```
🔍 VALIDAR PEDIMENTOS CRUZADOS
   ═══════════════════════════════════════════════════════════
   📋 BASE SELECCIONADA (Di_Pedimento/De_Pedimento):
      • Nombre: SEERT_Jlo
      • Tabla origen: NOM_TABLARAZON
      • Conexión externa: Sí/No
      • IdConexion: NULL/número
      • Servidor resuelto: NULL/IP  ← NUEVO
      • Servidor final: IP          ← NUEVO
      • Usuario: usuario

   📊 BASE ORIGEN/GLOSA (TR_Glosa):
      • Nombre: SEERT_Able
      • Tabla origen: RAZONXTABLA
      • Conexión externa: Sí/No
      • IdConexion: NULL/número
      • Servidor resuelto: NULL/IP  ← NUEVO
      • Servidor final: IP          ← NUEVO
      • Usuario: usuario
   ═══════════════════════════════════════════════════════════
```

Los campos **"Servidor resuelto"** y **"Servidor final"** te dirán:
- **Servidor resuelto**: Lo que viene de `Conexiones` (puede ser NULL)
- **Servidor final**: Lo que se usa después del fallback `?? conexionInfo.Servidor`

---

## 🔧 PASOS PARA DIAGNOSTICAR

### Paso 1: Ejecuta el cálculo de nuevo

Con el logging mejorado, ejecuta el cálculo y copia **COMPLETO** el output del Debug.

### Paso 2: Verifica la configuración en base de datos

Ejecuta estas queries en SQL Server:

```sql
-- 1. Verificar SEERT_Jlo en NOM_TABLARAZON
SELECT 
    N.IdTabla,
    N.IdRazon,
    N.NOMBRE_TABLA,
    N.ConnExterna,
    N.IdConexion,
    C.NombreConexion,
    C.Servidor,
    C.UsuarioSQL
FROM NOM_TABLARAZON N
LEFT JOIN Conexiones C ON N.IdConexion = C.IdConexion
WHERE N.NOMBRE_TABLA = 'SEERT_Jlo';

-- 2. Verificar SEERT_Able en RAZONXTABLA
SELECT 
    R.IdRazon,
    R.NOMBRE_RAZON,
    R.DB,
    R.ConnExterna,
    R.IdConexion,
    C.NombreConexion,
    C.Servidor,
    C.UsuarioSQL
FROM RAZONXTABLA R
LEFT JOIN Conexiones C ON R.IdConexion = C.IdConexion
WHERE R.DB = 'SEERT_Able';

-- 3. Verificar razón MAM
SELECT 
    R.IdRazon,
    R.NOMBRE_RAZON,
    R.DB,
    R.ConnExterna,
    R.IdConexion
FROM RAZONXTABLA R
WHERE R.NOMBRE_RAZON LIKE '%MAM%';
```

### Paso 3: Análisis esperado

Para que funcione correctamente, deberías ver:

**SEERT_Jlo** (en NOM_TABLARAZON):
```
NOMBRE_TABLA: SEERT_Jlo
ConnExterna: S
IdConexion: [número, no NULL]
Servidor: [IP del servidor externo, no NULL]
```

**SEERT_Able** (en RAZONXTABLA):
```
DB: SEERT_Able
ConnExterna: N (o NULL, si usa servidor principal)
IdConexion: NULL o 1
Servidor: 172.20.20.26 (o NULL si usa principal)
```

Si ambos servidores son **diferentes**, entonces `realmenteMismaConexion = false` y debería llamar a `ValidarPedimentosCruzadosMultiServidor`.

---

## 🎯 SOLUCIONES POSIBLES

### Solución 1: Configuración incorrecta

Si `SEERT_Jlo` no está bien configurado en `NOM_TABLARAZON`:

```sql
-- Agregar/actualizar configuración
UPDATE NOM_TABLARAZON
SET ConnExterna = 'S',
    IdConexion = [ID del servidor externo]
WHERE NOMBRE_TABLA = 'SEERT_Jlo';
```

### Solución 2: SEERT_Able no está en RAZONXTABLA

Si `SEERT_Able` no existe como `DB` en `RAZONXTABLA`:

```sql
-- Verificar qué DB tiene la razón MAM
SELECT IdRazon, NOMBRE_RAZON, DB
FROM RAZONXTABLA
WHERE NOMBRE_RAZON LIKE '%MAM%';

-- Si el DB es diferente a SEERT_Able, ese es el problema
```

### Solución 3: Forzar validación de servidor NULL

Si el problema es que ambos servidores quedan en NULL y se resuelven al mismo fallback, necesitamos mejorar la lógica:

```csharp
// ✅ MEJORAR: Considerar NULL como "no comparable"
if (string.IsNullOrEmpty(conexionBaseSeleccionada.Servidor) || 
    string.IsNullOrEmpty(conexionBaseOrigen.Servidor))
{
    // Si alguno es NULL, no podemos garantizar que sean el mismo servidor
    // FORZAR uso de multi-servidor por seguridad
    realmenteMismaConexion = false;
}
```

---

## 📝 INFORMACIÓN REQUERIDA DEL USUARIO

Por favor, proporciona:

1. **Output completo del Debug** con el logging mejorado
2. **Resultado de las 3 queries SQL** arriba
3. **Confirmación de**:
   - ¿En qué servidor está realmente `SEERT_Jlo`?
   - ¿En qué servidor está realmente `SEERT_Able`?
   - ¿Qué base de datos usa la razón `MAM` para la glosa?

Con esta información podremos identificar exactamente dónde está fallando la configuración o la lógica.

---

**Fecha**: 2025-01-XX  
**Caso**: Login failed con SEERT_Jlo  
**Usuario reportante**: Usuario del sistema  
**Estado**: 🔍 En diagnóstico  
**Archivo modificado**: `RetornoService.cs` (logging mejorado)
