# 🔧 Actualización: Validación de Origen ZIP en TR_GLOSA

## 📋 Descripción General

Se actualizó el query del reporte IGI **CON validación de TR_GLOSA** para incluir la verificación del campo `Gl_OrigenZipGlosa`, que indica si el pedimento fue cargado desde un archivo ZIP en la Glosa.

---

## 🎯 Objetivo

Identificar claramente qué pedimentos provienen de archivos ZIP cargados en TR_GLOSA, ya que estos son los que tienen información completa y confiable para el cálculo de impuestos.

---

## ✅ Cambios Implementados

### **1. Modelo de Datos Actualizado**

#### **ReporteIGIPagado.cs:**

```csharp
public class ReporteIGIPagado : ReporteBase
{
    // ... propiedades existentes ...

    public string EstatusOrigen { get; set; } = string.Empty; // ← NUEVO

    // ...
}
```

**Valores posibles:**
- `"ZIP"` - Pedimento cargado desde archivo ZIP en Glosa
- `"NO ZIP"` - Pedimento cargado manualmente o por otro medio
- `"N/A"` - No aplica (cuando no se valida contra Glosa)

---

### **2. Query CON Validación de TR_GLOSA**

#### **Campo Agregado en SELECT:**

```sql
CASE 
    WHEN TR.Gl_OrigenZipGlosa = 'S' THEN 'ZIP'
    ELSE 'NO ZIP'
END AS EstatusOrigen
```

#### **Filtro Agregado en WHERE:**

```sql
AND (TR.Gl_OrigenZipGlosa = 'S' OR TR.Gl_OrigenZipGlosa IS NULL)
```

**Explicación:**
- `TR.Gl_OrigenZipGlosa = 'S'` → Pedimentos cargados desde ZIP
- `TR.Gl_OrigenZipGlosa IS NULL` → Pedimentos sin registro en Glosa (aún no cargados)

---

### **3. Cambios Adicionales en el Query**

#### **a) Formato del Pedimento:**

**Antes:**
```sql
Adu_AduanaSecc+'-'+Gl_Patente+'-'+Gl_Pedimento AS Pedimento
```

**Ahora:**
```sql
DP.Pim_Folio AS Pedimento
```

**Razón:** Usar directamente el folio del pedimento desde `Di_Pedimento` para mayor consistencia.

---

#### **b) Fecha de Pago:**

**Antes:**
```sql
WHERE CONVERT(DATE,TR.Gl_FecPagoReal) BETWEEN @FechaInicio AND @FechaFin
```

**Ahora:**
```sql
WHERE CONVERT(DATE, IIF(DP.CLP_CLAVE = 'R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago)) BETWEEN @FechaInicio AND @FechaFin
```

**Razón:** Considerar si el pedimento es de régimen R1 o normal para tomar la fecha correcta.

---

#### **c) Filtro de Formas de Pago:**

**Antes:**
```sql
AND (
    TR.Gl_FPagoIVA IN ('5','21') 
    OR TR.Gl_FPagoAdvalorem IN ('5','21')
)
```

**Ahora:**
```sql
AND (
    TR.Gl_FPagoIVA IN ('5','21') 
    OR TR.Gl_FPagoAdvalorem IN ('5','21')
    OR TR.Gl_Pedimento IS NULL
)
```

**Razón:** Incluir también pedimentos que NO están en Glosa (`TR.Gl_Pedimento IS NULL`) para detectar pendientes de carga.

---

## 📊 Query Completo Actualizado

### **CON Validación de TR_GLOSA:**

```sql
SELECT 
    DI.Pim_Consecutivo AS iDPedimento,
    DP.Pim_Folio AS Pedimento,
    TR.Gl_FecPagoReal AS FechaPago,
    TR.Gl_ImporteADvalorem AS IGI_Pagado,
    DI.Pid_ValorAdu AS ValorAduana,
    FRA.Fra_AdvGral AS TasaIGI,
    TR.Gl_ImporteIVA AS IVA_Pagado,
    TR.Gl_FPagoAdvalorem AS FormaPago_IGI,
    TR.Gl_FPagoIVA AS FormaPago_IVA,
    CASE 
        WHEN TR.Gl_Pedimento IS NOT NULL THEN 'SI CARGADO'
        ELSE 'NO CARGADO'
    END AS EstatusGlosa,
    CASE 
        WHEN TR.Gl_OrigenZipGlosa = 'S' THEN 'ZIP'
        ELSE 'NO ZIP'
    END AS EstatusOrigen
FROM Di_Pedimento DP
INNER JOIN Di_PedimentoDet DI
    ON DI.Pim_Consecutivo = DP.Pim_Consecutivo
LEFT JOIN TR_GLOSA TR
    ON TR.Gl_Pedimento = DP.Pim_Folio
    AND TR.Gl_Aduana = DP.Adu_AduanaSecc
    AND TR.Gl_Patente = DP.AgP_Patente
    AND YEAR(IIF(DP.CLP_CLAVE= 'R1',DP.Pim_FechaPagoR1,DP.Pim_FechaPago)) = YEAR(CONVERT(DATE,TR.Gl_FecPagoReal))
    AND DI.Pid_Secuencia = TR.GL_SEC
    AND TR.Gl_TOper = 1
INNER JOIN Ca_Farancelaria FRA
    ON FRA.Fra_Fraccion = IIF(LEFT(DI.Fra_Fraccion,2)= '98', DI.Fra_FraccionORIG,DI.Fra_Fraccion) 
    AND FRA.Pai_Clave = 'MEX' 
    AND FRA.Fra_TipoOper = 0
WHERE 
    CONVERT(DATE, IIF(DP.CLP_CLAVE = 'R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago)) BETWEEN @FechaInicio AND @FechaFin
    AND (
        TR.Gl_FPagoIVA IN ('5','21') 
        OR TR.Gl_FPagoAdvalorem IN ('5','21')
        OR TR.Gl_Pedimento IS NULL
    )
    AND (TR.Gl_OrigenZipGlosa = 'S' OR TR.Gl_OrigenZipGlosa IS NULL)
```

---

### **SIN Validación de TR_GLOSA:**

```sql
SELECT 
    DI.Pim_Consecutivo AS iDPedimento,
    DP.Adu_AduanaSecc+'-'+DP.AgP_Patente+'-'+DP.Pim_Folio AS Pedimento,
    CONVERT(DATE, IIF(DP.CLP_CLAVE = 'R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago)) AS FechaPago,
    DI.Pid_ValorAdu AS ValorAduana,
    FRA.Fra_AdvGral AS TasaIGI,
    0 AS IGI_Pagado,
    0 AS IVA_Pagado,
    '' AS FormaPago_IGI,
    '' AS FormaPago_IVA,
    'NO VALIDADO' AS EstatusGlosa,
    'N/A' AS EstatusOrigen
FROM Di_Pedimento DP
INNER JOIN Di_PedimentoDet DI
    ON DI.Pim_Consecutivo = DP.Pim_Consecutivo
INNER JOIN Ca_Farancelaria FRA
    ON FRA.Fra_Fraccion = IIF(LEFT(DI.Fra_Fraccion,2)= '98', DI.Fra_FraccionORIG, DI.Fra_Fraccion) 
    AND FRA.Pai_Clave = 'MEX' 
    AND FRA.Fra_TipoOper = 0
WHERE 
    CONVERT(DATE, IIF(DP.CLP_CLAVE = 'R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago)) BETWEEN @FechaInicio AND @FechaFin
```

---

## 🔍 Comparación de Resultados

### **Ejemplo de Datos:**

| IdPedimento | Pedimento | FechaPago | IGI_Pagado | Estatus Glosa | **Estatus Origen** |
|-------------|-----------|-----------|------------|---------------|-------------------|
| 8986 | 6006491 | 01/14/2026 | 1074.00 | SI CARGADO | **ZIP** ✅ |
| 8987 | 6006490 | 01/15/2026 | 922.00 | SI CARGADO | **ZIP** ✅ |
| 8988 | 6010493 | 01/20/2026 | 507.00 | SI CARGADO | **NO ZIP** ⚠️ |
| 8989 | 6010492 | 01/21/2026 | 0.00 | NO CARGADO | **NO ZIP** ⚠️ |

---

## 📈 Casos de Uso

### **Caso 1: Pedimento Cargado desde ZIP**

```
Estatus Glosa: "SI CARGADO"
Estatus Origen: "ZIP"
```

**Interpretación:**
- ✅ Pedimento en TR_GLOSA
- ✅ Cargado desde archivo ZIP
- ✅ Datos completos y confiables
- ✅ Ideal para auditoría

---

### **Caso 2: Pedimento Cargado Manualmente**

```
Estatus Glosa: "SI CARGADO"
Estatus Origen: "NO ZIP"
```

**Interpretación:**
- ✅ Pedimento en TR_GLOSA
- ⚠️ NO cargado desde ZIP (manual u otra fuente)
- ⚠️ Puede tener datos incompletos
- ⚠️ Requiere validación adicional

---

### **Caso 3: Pedimento NO en Glosa**

```
Estatus Glosa: "NO CARGADO"
Estatus Origen: "NO ZIP"
```

**Interpretación:**
- ❌ Pedimento NO está en TR_GLOSA
- ❌ No hay datos de pago
- ❌ Pendiente de carga

---

### **Caso 4: Consulta SIN Validación**

```
Estatus Glosa: "NO VALIDADO"
Estatus Origen: "N/A"
```

**Interpretación:**
- ℹ️ No se validó contra TR_GLOSA
- ℹ️ Solo cálculo teórico desde Di_Pedimento
- ℹ️ No hay información de origen

---

## 🎨 Visualización en el Reporte

### **DataGridView (Ocultar Columna):**

```csharp
// En FrmReportes.FormatearColumnas()
dgvReporte.Columns["EstatusOrigen"].Visible = false;
```

**O mostrarla:**
```csharp
dgvReporte.Columns["EstatusOrigen"].Visible = true;
dgvReporte.Columns["EstatusOrigen"].HeaderText = "Origen";
dgvReporte.Columns["EstatusOrigen"].Width = 80;
```

---

## 🧪 Pruebas

### **Test 1: Verificar Campo en Resultados**

```csharp
var reportes = reporteService.GenerarReporteIGI("SEERT_Able", fechaInicio, fechaFin, sinValidacionGlosa: false);

foreach (var reporte in reportes)
{
    Console.WriteLine($"Pedimento: {reporte.Pedimento} | Glosa: {reporte.EstatusGlosa} | Origen: {reporte.EstatusOrigen}");
}
```

**Resultado esperado:**
```
Pedimento: 6006491 | Glosa: SI CARGADO | Origen: ZIP
Pedimento: 6006490 | Glosa: SI CARGADO | Origen: ZIP
Pedimento: 6010493 | Glosa: SI CARGADO | Origen: NO ZIP
Pedimento: 6010492 | Glosa: NO CARGADO | Origen: NO ZIP
```

---

### **Test 2: Filtrar Solo ZIP**

```csharp
var reportesZip = reportes.Where(r => r.EstatusOrigen == "ZIP").ToList();
Console.WriteLine($"Pedimentos desde ZIP: {reportesZip.Count}");
```

---

### **Test 3: Detectar Pedimentos NO ZIP**

```csharp
var reportesNoZip = reportes.Where(r => r.EstatusOrigen == "NO ZIP" && r.EstatusGlosa == "SI CARGADO").ToList();
Console.WriteLine($"Pedimentos cargados manualmente: {reportesNoZip.Count}");
```

---

## 📊 Estadísticas Adicionales en Resumen

### **Opcional: Agregar al ResumenIGI**

```csharp
public class ResumenIGI
{
    // ... propiedades existentes ...

    public int PedimentosDesdeZip { get; set; }
    public int PedimentosManuales { get; set; }

    public decimal PorcentajeZip => TotalPedimentos > 0 
        ? (decimal)PedimentosDesdeZip / TotalPedimentos * 100 
        : 0;
}
```

**Cálculo:**
```csharp
public ResumenIGI GenerarResumen(List<ReporteIGIPagado> reportes)
{
    return new ResumenIGI
    {
        // ... existentes ...
        PedimentosDesdeZip = reportes.Count(r => r.EstatusOrigen == "ZIP"),
        PedimentosManuales = reportes.Count(r => r.EstatusOrigen == "NO ZIP" && r.EstatusGlosa == "SI CARGADO")
    };
}
```

---

## 🔄 Flujo de Validación

```
┌─────────────────────────────────────────────────┐
│  Pedimento en Di_Pedimento                      │
└──────────────────┬──────────────────────────────┘
                   │
                   ▼
         ┌─────────────────────┐
         │ ¿Existe en TR_GLOSA?│
         └────────┬────────────┘
                  │
         ┌────────┴────────┐
         │                 │
         ▼                 ▼
    ┌────────┐        ┌────────────┐
    │   SÍ   │        │     NO     │
    └───┬────┘        └─────┬──────┘
        │                   │
        ▼                   ▼
┌──────────────────┐   ┌──────────────────┐
│ ¿Gl_OrigenZipGlosa│   │ EstatusGlosa:   │
│      = 'S'?      │   │  "NO CARGADO"    │
└───┬──────────────┘   │ EstatusOrigen:   │
    │                  │  "NO ZIP"        │
┌───┴──┐               └──────────────────┘
│      │
▼      ▼
SÍ     NO

EstatusGlosa:       EstatusGlosa:
"SI CARGADO"       "SI CARGADO"
EstatusOrigen:      EstatusOrigen:
"ZIP" ✅           "NO ZIP" ⚠️
```

---

## ⚠️ Consideraciones Importantes

### **1. Campo `Gl_OrigenZipGlosa` en TR_GLOSA**

**Valores posibles:**
- `'S'` → Cargado desde ZIP
- `'N'` → No cargado desde ZIP
- `NULL` → Sin información (pedimento no está en Glosa)

**Manejo en el CASE:**
```sql
CASE 
    WHEN TR.Gl_OrigenZipGlosa = 'S' THEN 'ZIP'
    ELSE 'NO ZIP'  -- Cubre 'N' y NULL
END
```

---

### **2. Impacto en el Filtro WHERE**

**Antes:**
```sql
AND (TR.Gl_FPagoIVA IN ('5','21') OR TR.Gl_FPagoAdvalorem IN ('5','21'))
```

**Ahora:**
```sql
AND (
    TR.Gl_FPagoIVA IN ('5','21') 
    OR TR.Gl_FPagoAdvalorem IN ('5','21')
    OR TR.Gl_Pedimento IS NULL  -- ← Incluye NO CARGADOS
)
AND (TR.Gl_OrigenZipGlosa = 'S' OR TR.Gl_OrigenZipGlosa IS NULL)  -- ← Solo ZIP o pendientes
```

**Resultado:**
- ✅ Trae pedimentos desde ZIP (`Gl_OrigenZipGlosa = 'S'`)
- ✅ Trae pedimentos NO en Glosa (`Gl_OrigenZipGlosa IS NULL`)
- ❌ **Excluye** pedimentos cargados manualmente (`Gl_OrigenZipGlosa = 'N'`)

---

### **3. Consistencia con RetornoService**

Este cambio es consistente con la lógica usada en `RetornoService.ObtenerImportacionesValidadas()` y `ObtenerExportacionesValidadas()`:

```csharp
WHERE TR.Gl_OrigenZipGlosa = 'S'
```

---

## ✅ Beneficios

### **1. Calidad de Datos:**
- ✅ Identificar pedimentos con datos completos (ZIP)
- ✅ Detectar cargas manuales que pueden tener errores

### **2. Auditoría:**
- ✅ Rastrear origen de la información
- ✅ Validar integridad de datos

### **3. Reportes:**
- ✅ Filtrar solo información confiable (desde ZIP)
- ✅ Generar estadísticas de calidad de carga

---

## 📝 Archivos Modificados

```
Retorno360Tacna/
├── MODELS/
│   └── ReporteIGI.cs                      [MODIFICADO]
│       └── ReporteIGIPagado
│           └── + EstatusOrigen
│
├── SERVICES/
│   └── ReporteIGIService.cs               [MODIFICADO]
│       ├── GenerarReporteIGIConGlosa()
│       │   ├── + SELECT EstatusOrigen
│       │   ├── + WHERE Gl_OrigenZipGlosa
│       │   └── + Lectura EstatusOrigen
│       │
│       └── GenerarReporteIGISinGlosa()
│           ├── + SELECT 'N/A' AS EstatusOrigen
│           └── + EstatusOrigen = "N/A"
│
└── DOCS/
    └── FIX_Validacion_OrigenZIP.md        [NUEVO - Este archivo]
```

---

## 🔗 Referencias

- Query original proporcionado por el usuario
- Adaptación a la arquitectura existente
- Consistencia con `RetornoService.cs` (`Gl_OrigenZipGlosa = 'S'`)

---

**Fecha de Implementación:** Enero 2026  
**Versión:** 3.0.7  
**Sistema:** Retorno 360° Tacna  
**Tipo:** Mejora - Validación de origen de datos  
**Estado:** ✅ IMPLEMENTADO Y COMPILADO  
**Build:** ✅ Exitoso
