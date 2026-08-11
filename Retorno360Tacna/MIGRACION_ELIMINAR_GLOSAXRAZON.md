# 🔄 MIGRACIÓN: De GLOSAXRAZON a NOM_TABLARAZON

## 📋 RESUMEN DEL CAMBIO

Se eliminaron todas las validaciones y consultas que usaban la tabla `GLOSAXRAZON` y se migraron para usar exclusivamente `RAZONXTABLA` y `NOM_TABLARAZON`.

---

## 🚨 PROBLEMA ANTERIOR

El sistema tenía una **duplicidad de fuentes de datos** para obtener las bases de datos asociadas a una razón social:

1. **GLOSAXRAZON** - Tabla obsoleta que se usaba para obtener las bases de datos
2. **NOM_TABLARAZON** - Tabla actual que contiene las bases de datos seleccionables

Esto causaba:
- ❌ Inconsistencias si las dos tablas no estaban sincronizadas
- ❌ Confusión sobre cuál es la fuente de verdad
- ❌ Mantenimiento duplicado de datos
- ❌ Posibles errores si GLOSAXRAZON no estaba actualizada

---

## ✅ SOLUCIÓN IMPLEMENTADA

### Cambio 1: Método Obsoleto Comentado

**Método eliminado**: `ObtenerBasesDatosDeGlosaxRazon(string razonSocial)`

```csharp
// ❌ MÉTODO OBSOLETO: Usaba GLOSAXRAZON
// private List<string> ObtenerBasesDatosDeGlosaxRazon(string razonSocial)
// {
//     string sql = "SELECT DB FROM GLOSAXRAZON WHERE RAZON = @Razon";
//     ...
// }
```

**Razón de eliminación**:
- Consultaba tabla obsoleta `GLOSAXRAZON`
- Buscaba por `RAZON` (nombre de razón social) en lugar de `IdRazon`
- Duplicaba funcionalidad ya existente

---

### Cambio 2: Uso de Método Existente

**Método ya existente** (público): `ObtenerBasesDatosRazon(int idRazon)`

```csharp
public List<string> ObtenerBasesDatosRazon(int idRazon)
{
    string sql = "SELECT NOMBRE_TABLA FROM NOM_TABLARAZON WHERE IdRazon = @IdRazon ORDER BY NOMBRE_TABLA";
    // ... resto del código
}
```

**Ventajas**:
- ✅ Usa tabla actual `NOM_TABLARAZON`
- ✅ Busca por `IdRazon` (más eficiente y preciso)
- ✅ Ya existe y está en uso en otros lugares
- ✅ Método público, puede ser reutilizado

---

### Cambio 3: Actualización de `CalcularRetornoPorRazonSocial`

**Antes**:
```csharp
// Obtener TODAS las bases de datos asociadas a esta razón en GLOSAXRAZON
List<string> basesDatos = ObtenerBasesDatosDeGlosaxRazon(razonInfo.NombreRazon);

if (!basesDatos.Any())
{
    throw new Exception($"No se encontraron bases de datos en GLOSAXRAZON para la razón social: {razonInfo.NombreRazon}");
}
```

**Ahora**:
```csharp
// ✅ NUEVO: Obtener TODAS las bases de datos asociadas a esta razón desde NOM_TABLARAZON
// Ya no se usa GLOSAXRAZON, ahora todo se maneja desde RAZONXTABLA y NOM_TABLARAZON
List<string> basesDatos = ObtenerBasesDatosRazon(idRazon);

if (!basesDatos.Any())
{
    throw new Exception($"No se encontraron bases de datos en NOM_TABLARAZON para la razón social: {razonInfo.NombreRazon} (IdRazon: {idRazon})");
}
```

**Mejoras**:
- ✅ Usa `idRazon` directamente (más eficiente, no necesita buscar por nombre)
- ✅ Mensaje de error más claro (indica IdRazon)
- ✅ Fuente única de datos: `NOM_TABLARAZON`

---

## 📊 FLUJO DE DATOS ANTES Y DESPUÉS

### ❌ FLUJO ANTERIOR

```
Usuario selecciona razón social
    ↓
ObtenerRazonSocial(idRazon) → RAZONXTABLA
    ↓
Obtiene: IdRazon, NombreRazon, BaseDatosOrigen (glosa)
    ↓
ObtenerBasesDatosDeGlosaxRazon(NombreRazon) → GLOSAXRAZON ❌
    ↓
Query: SELECT DB FROM GLOSAXRAZON WHERE RAZON = @Razon
    ↓
Obtiene lista de bases de datos
```

**Problema**: Usa dos fuentes diferentes (`RAZONXTABLA` y `GLOSAXRAZON`)

---

### ✅ FLUJO ACTUAL

```
Usuario selecciona razón social
    ↓
ObtenerRazonSocial(idRazon) → RAZONXTABLA
    ↓
Obtiene: IdRazon, NombreRazon, BaseDatosOrigen (glosa)
    ↓
ObtenerBasesDatosRazon(idRazon) → NOM_TABLARAZON ✅
    ↓
Query: SELECT NOMBRE_TABLA FROM NOM_TABLARAZON WHERE IdRazon = @IdRazon
    ↓
Obtiene lista de bases de datos
```

**Ventaja**: Fuente única y consistente (`RAZONXTABLA` + `NOM_TABLARAZON`)

---

## 🎯 ESTRUCTURA DE TABLAS

### RAZONXTABLA (Tabla maestra de razones sociales)
```sql
IdRazon         INT           -- ID único de la razón social
NOMBRE_RAZON    VARCHAR       -- Nombre de la razón social
DB              VARCHAR       -- Base de datos ORIGEN/GLOSA (TR_Glosa)
ConnExterna     CHAR(1)       -- 'S' si tiene conexión externa
IdConexion      INT           -- FK a tabla Conexiones
```

**Propósito**: Define las razones sociales y su base de datos de glosa (origen)

---

### NOM_TABLARAZON (Tabla de bases de datos seleccionables)
```sql
IdTabla         INT           -- ID único de la tabla
IdRazon         INT           -- FK a RAZONXTABLA
NOMBRE_TABLA    VARCHAR       -- Nombre de la base de datos
ConnExterna     CHAR(1)       -- 'S' si tiene conexión externa
IdConexion      INT           -- FK a tabla Conexiones
```

**Propósito**: Lista TODAS las bases de datos asociadas a cada razón social (Di_Pedimento/De_Pedimento)

---

### ~~GLOSAXRAZON~~ (Tabla obsoleta - YA NO SE USA)
```sql
RAZON           VARCHAR       -- Nombre de la razón social
DB              VARCHAR       -- Base de datos
```

**Estado**: ❌ **OBSOLETA** - Ya no se consulta en el código

---

## 🔍 VALIDACIÓN DE CAMBIOS

### Verificación de compilación
```
get_errors(["Retorno360Tacna\\SERVICES\\RetornoService.cs"])
Resultado: ✅ Sin errores
```

### Búsqueda de referencias a GLOSAXRAZON
```
code_search(["GLOSAXRAZON"])
Resultado: Solo en código comentado ✅
```

---

## 📝 ESCENARIOS DE PRUEBA

### Escenario 1: Cálculo de retorno por razón social

**Datos de entrada**:
- IdRazon: 17 (ejemplo)
- FechaInicio: 2024-01-01
- FechaFin: 2024-12-31

**Flujo**:
1. `ObtenerRazonSocial(17)` → Lee `RAZONXTABLA`
   - Obtiene: NombreRazon, BaseDatosOrigen (glosa)

2. `ObtenerBasesDatosRazon(17)` → Lee `NOM_TABLARAZON`
   - Query: `SELECT NOMBRE_TABLA FROM NOM_TABLARAZON WHERE IdRazon = 17`
   - Obtiene: Lista de todas las bases de datos de esa razón

3. Para cada base de datos:
   - `CalcularPorBaseDatos(baseDatos, fechaInicio, fechaFin, incluirMateriaPrima)`
   - Acumula: importaciones, exportaciones

4. Calcula porcentaje de retorno total

**Resultado esperado**: ✅ Funciona correctamente con datos de `NOM_TABLARAZON`

---

### Escenario 2: Base de datos no encontrada

**Datos de entrada**:
- IdRazon: 999 (no existe en NOM_TABLARAZON)

**Resultado esperado**:
```
Exception: "No se encontraron bases de datos en NOM_TABLARAZON 
           para la razón social: [Nombre] (IdRazon: 999)"
```

✅ Mensaje claro que indica la tabla correcta y el IdRazon

---

## 🏁 ESTADO FINAL

| Componente | Estado Anterior | Estado Actual |
|------------|----------------|---------------|
| `GLOSAXRAZON` | ❌ En uso | ✅ **Obsoleta (no se consulta)** |
| `ObtenerBasesDatosDeGlosaxRazon` | ❌ Método activo | ✅ **Comentado** |
| `ObtenerBasesDatosRazon` | ✅ Existía | ✅ **Ahora se usa** |
| `CalcularRetornoPorRazonSocial` | ❌ Usaba GLOSAXRAZON | ✅ **Usa NOM_TABLARAZON** |
| Fuente de datos | ❌ Múltiples (RAZONXTABLA + GLOSAXRAZON) | ✅ **Única (RAZONXTABLA + NOM_TABLARAZON)** |

---

## ✅ VENTAJAS DEL CAMBIO

1. **Fuente única de verdad**
   - Todas las consultas ahora usan `RAZONXTABLA` + `NOM_TABLARAZON`
   - No hay riesgo de inconsistencias

2. **Más eficiente**
   - Búsqueda por `IdRazon` (índice) en lugar de `NombreRazon` (texto)
   - Queries más rápidas

3. **Más mantenible**
   - Una tabla menos que sincronizar
   - Código más simple y claro

4. **Mejor trazabilidad**
   - Los logs y errores ahora indican `IdRazon`
   - Más fácil debuggear problemas

5. **Consistente con el resto del sistema**
   - Otros métodos ya usaban `NOM_TABLARAZON`
   - Ahora todo el sistema es consistente

---

## 🔗 ARCHIVOS RELACIONADOS

- `AUDITORIA_VALIDACION_CONNEXTERNA.md` - Auditoría de conexiones
- `FIX_CALCULAR_POR_BASEDATOS_CONNEXION.md` - Fix anterior de conexiones
- `RESUMEN_AUDITORIA_CONNEXTERNA.md` - Resumen de auditoría

---

**Fecha de migración**: 2025-01-XX  
**Archivo modificado**: `Retorno360Tacna\SERVICES\RetornoService.cs`  
**Tabla obsoleta**: `GLOSAXRAZON` (ya no se consulta)  
**Tablas activas**: `RAZONXTABLA` + `NOM_TABLARAZON`  
**Tipo de cambio**: Eliminación de dependencia obsoleta  
**Verificación**: ✅ Sin errores de compilación  
**Versión**: .NET 10
