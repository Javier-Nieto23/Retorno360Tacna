# Actualización Query Materia Prima con Tabla Temporal

## Cambios Realizados en CatalogoPartesService.cs

### ✅ Métodos Eliminados

1. **`ObtenerCatalogoBOMCompleto()`** ❌
   - Query de catálogo PT con componentes vigentes/no vigentes
   - Ya no se usa según nueva funcionalidad

2. **`ObtenerComponentesBOM()`** ❌
   - Query de detalle de componentes por parte padre
   - Ya no se usa según nueva funcionalidad

### ✅ Método Actualizado

**`ObtenerMateriaPrimaBOM(string nombreBaseDatos, DateTime fechaInicio, DateTime fechaFin)`**

#### Query Anterior:
```sql
SELECT 
    cp.Par_NoParte,
    cp.Par_DescripcionEsp,
    cp.Tim_Clave AS Clave,
    cp.Par_InsercionFecha,
    'VIGENTE EN BOM' AS EstatusComponente
FROM Ca_Parte AS cp WITH (NOLOCK)
WHERE 
    cp.Tim_Clave = 'MP'
    AND cp.Par_InsercionFecha BETWEEN @FechaInicio AND @FechaFin
ORDER BY 
    cp.Par_InsercionFecha,
    cp.Par_NoParte
```

#### Query Nuevo (con tabla temporal):
```sql
-- Tabla temporal con componentes vigentes en BOM
DECLARE @componentesVigentesEnBOM TABLE (
    componente VARCHAR(100)
);

INSERT INTO @componentesVigentesEnBOM (componente)
SELECT DISTINCT par_nopartehijo 
FROM ca_bom WITH (NOLOCK)
WHERE GETDATE() BETWEEN bom_fechaini AND bom_fechafin;

-- Consulta principal de materia prima
SELECT 
    cp.Par_NoParte,
    cp.Par_DescripcionEsp,
    cp.Tim_Clave AS Clave,
    cp.Par_InsercionFecha,
    CASE 
        WHEN cp.Par_NoParte IN (SELECT componente FROM @componentesVigentesEnBOM)
        THEN 'VIGENTE EN BOM'
        WHEN cp.Par_InsercionFecha IS NULL
        THEN 'NO ESTA EN BOM'
        ELSE 'NO ESTA EN BOM'
    END AS EstatusComponente
FROM Ca_Parte AS cp WITH (NOLOCK)
WHERE 
    cp.Tim_Clave = 'MP'
ORDER BY 
    cp.Par_InsercionFecha,
    cp.Par_NoParte
OPTION (MAXDOP 4)
```

---

## Diferencias Clave

### 1. **Tabla Temporal de Componentes Vigentes**
```sql
DECLARE @componentesVigentesEnBOM TABLE (
    componente VARCHAR(100)
);

INSERT INTO @componentesVigentesEnBOM (componente)
SELECT DISTINCT par_nopartehijo 
FROM ca_bom WITH (NOLOCK)
WHERE GETDATE() BETWEEN bom_fechaini AND bom_fechafin;
```
- ✅ Crea lista de MP que **HOY** están vigentes en algún BOM
- ✅ Usa `GETDATE()` para validar vigencia actual
- ✅ Evita joins complejos en la consulta principal

### 2. **Estatus Basado en Vigencia Actual**

**Antes:**
- Solo mostraba MP dentro del rango de fechas
- Todas eran `VIGENTE EN BOM` por definición

**Ahora:**
- Muestra **TODA** la materia prima (sin filtro de fechas)
- El estatus se calcula según si está vigente en BOM HOY:
  - `VIGENTE EN BOM`: MP que hoy está en alguna estructura BOM activa
  - `NO ESTA EN BOM`: MP que no está en ningún BOM o está en BOM no vigente

### 3. **Sin Uso de Parámetros de Fecha**

```csharp
// Ya no usamos los parámetros de fecha porque mostramos TODA la MP
// y el estatus se determina por si está vigente en BOM HOY
```

**Nota:** Los parámetros `fechaInicio` y `fechaFin` ya NO se usan en el query.  
Si se desean usar en el futuro, habría que agregar filtro `WHERE`:

```sql
WHERE 
    cp.Tim_Clave = 'MP'
    AND (
        cp.Par_InsercionFecha BETWEEN @FechaInicio AND @FechaFin
        OR cp.Par_InsercionFecha IS NULL
    )
```

---

## Beneficios del Nuevo Query

### **1. Vista Completa de MP**
- ✅ Muestra **toda** la materia prima del catálogo
- ✅ No limita por rango de fechas
- ✅ Permite ver MP antigua que aún está en BOM vigente

### **2. Estatus en Tiempo Real**
- ✅ El estatus refleja la situación **actual** (HOY)
- ✅ Independiente de cuándo se dio de alta la MP
- ✅ Basado en vigencia real de BOM

### **3. Rendimiento**
| Aspecto | Antes | Ahora |
|---------|-------|-------|
| **Registros procesados** | Solo rango de fechas | Toda la MP |
| **Cálculo de estatus** | Fijo | Dinámico vía IN |
| **Joins** | No | No (usa tabla temporal) |
| **Precisión** | Por fecha de alta | Por vigencia BOM |

---

## Casos de Uso

### **Caso 1: MP dada de alta hace años pero vigente HOY**
```
MP-001 | Acero Inox | 15/01/2020 | VIGENTE EN BOM
```
- **Antes:** ❌ No aparecía (fuera de rango)
- **Ahora:** ✅ Aparece como VIGENTE (está en BOM activo)

### **Caso 2: MP dada de alta recientemente pero NO en BOM**
```
MP-002 | Aluminio | 20/05/2025 | NO ESTA EN BOM
```
- **Antes:** ✅ Aparecía como VIGENTE (dentro del rango)
- **Ahora:** ✅ Aparece como NO VIGENTE (no está en ningún BOM)

### **Caso 3: MP sin fecha de inserción**
```
MP-003 | Cobre | NULL | NO ESTA EN BOM
```
- **Antes:** ❌ Excluida (no pasaba filtro de fechas)
- **Ahora:** ✅ Aparece y se marca correctamente

---

## Comportamiento del Estatus

```sql
CASE 
    WHEN cp.Par_NoParte IN (SELECT componente FROM @componentesVigentesEnBOM)
    THEN 'VIGENTE EN BOM'

    WHEN cp.Par_InsercionFecha IS NULL
    THEN 'NO ESTA EN BOM'

    ELSE 'NO ESTA EN BOM'
END AS EstatusComponente
```

### Lógica:
1. **Primera prioridad:** ¿Está en tabla de vigentes HOY?
   - ✅ Sí → `VIGENTE EN BOM`

2. **Segunda prioridad:** ¿Tiene fecha de inserción?
   - ❌ No (NULL) → `NO ESTA EN BOM`

3. **Por defecto:** → `NO ESTA EN BOM`

---

## Próximos Pasos

### ⚠️ Errores de Compilación Pendientes

El formulario `FrmCatalogoPartes.cs` aún tiene referencias a métodos eliminados:

1. **Llamadas a métodos inexistentes:**
   - `catalogoService.ObtenerCatalogoBOMCompleto(...)` ❌
   - `catalogoService.ObtenerComponentesBOM(...)` ❌

2. **Variables no declaradas:**
   - `catalogoCompleto`
   - `componentesDetalle`
   - `materiaPrimaLista`
   - `baseDatosActual`
   - `fechaInicio`, `fechaFin`, `fechaActual`

3. **Controles inexistentes:**
   - `chartCatalogo`
   - `dgvMateriaPrima`
   - `tabPagePT`, `tabPageMP`

### ✅ Acción Requerida

Según solicitud del usuario: **"eliminar las 2 consultas, gráficos, tablas de ambas pestañas y dejar los DateTimePicker"**

Necesitamos:
1. Simplificar `FrmCatalogoPartes.cs`
2. Eliminar métodos de gráficos/tablas
3. Eliminar referencias a PT
4. Mantener solo estructura básica con combos y DateTimePickers

---

**Estado:** ✅ Servicio actualizado, ⚠️ Formulario pendiente de simplificación  
**Compilación:** ❌ Fallida (errores en formulario)  
**Fecha:** Mayo 2025
