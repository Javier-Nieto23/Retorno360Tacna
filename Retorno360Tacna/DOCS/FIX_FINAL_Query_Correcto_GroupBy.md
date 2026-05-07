# 🔧 Actualización FINAL: Query Correcto con GROUP BY

## ⚠️ Cambio Importante

Se actualizó el query del reporte IGI para usar el **query correcto** proporcionado por el usuario, que incluye diferencias críticas respecto al anterior.

---

## 🆚 Diferencias Clave vs Query Anterior

### **1. Filtro de ZIP movido al JOIN**

#### ❌ **Antes (Incorrecto):**
```sql
LEFT JOIN TR_GLOSA TR
    ON TR.Gl_Pedimento = DP.Pim_Folio
    AND ...
WHERE 
    ...
    AND (TR.Gl_OrigenZipGlosa = 'S' OR TR.Gl_OrigenZipGlosa IS NULL)
```

#### ✅ **Ahora (Correcto):**
```sql
LEFT JOIN TR_GLOSA TR
    ON TR.Gl_Pedimento = DP.Pim_Folio
    AND ...
    AND TR.Gl_OrigenZipGlosa = 'S'  -- ← Filtro directamente en el JOIN
WHERE 
    ...
    -- Sin filtro adicional de ZIP en WHERE
```

**Impacto:**
- ✅ Solo trae pedimentos **cargados desde ZIP**
- ❌ NO trae pedimentos cargados manualmente (`Gl_OrigenZipGlosa = 'N'`)
- ❌ NO trae pedimentos pendientes (`Gl_OrigenZipGlosa IS NULL`)

---

### **2. GROUP BY para Agregación**

#### ❌ **Antes (Sin GROUP BY):**
```sql
SELECT 
    DI.Pid_ValorAdu AS ValorAduana,
    FRA.Fra_AdvGral AS TasaIGI,
    ...
FROM ...
-- Sin GROUP BY
```

**Problema:** Traía múltiples filas por pedimento, y se calculaba IGI en C# línea por línea.

#### ✅ **Ahora (Con GROUP BY):**
```sql
SELECT 
    SUM(ROUND((DI.Pid_ValorAdu * FRA.Fra_AdvGral) / 100,0)) AS IGI_Calculado,  -- ← Agregación en SQL
    ...
FROM ...
GROUP BY  
    DI.Pim_Consecutivo,
    Adu_AduanaSecc+'-'+Gl_Patente+'-'+Gl_Pedimento,
    TR.Gl_FecPagoReal,
    TR.Gl_ImporteADvalorem,
    TR.Gl_ImporteIVA,
    TR.Gl_FPagoAdvalorem,
    TR.Gl_FPagoIVA,
    CASE WHEN TR.Gl_Pedimento IS NOT NULL THEN 'SI CARGADO' ELSE 'NO CARGADO' END,
    CASE WHEN TR.Gl_OrigenZipGlosa = 'S' THEN 'ZIP' ELSE 'NO ZIP' END
```

**Impacto:**
- ✅ **Suma** el IGI calculado de todas las partidas **en SQL**
- ✅ Retorna **1 fila por pedimento** (agrupado)
- ✅ Más eficiente (menos procesamiento en C#)

---

### **3. Forma de Pago IGI**

#### ❌ **Antes:**
```sql
TR.Gl_FPagoAdvalorem IN ('5','21')
```

#### ✅ **Ahora:**
```sql
TR.Gl_FPagoAdvalorem IN ('0','21')  -- ← Cambio de '5' a '0'
```

**Impacto:**
- Ahora incluye formas de pago `'0'` y `'21'`
- Antes incluía `'5'` y `'21'`

---

### **4. Cálculo de IGI**

#### ❌ **Antes (C#):**
```csharp
decimal valorAduana = reader.GetDecimal(4);
decimal tasaIGI = reader.GetDecimal(5);
reporte.IGI_Calculado = CalcularIGI(valorAduana, tasaIGI);  // Calculado en C#
```

#### ✅ **Ahora (SQL):**
```sql
SUM(ROUND((DI.Pid_ValorAdu * FRA.Fra_AdvGral) / 100,0)) AS IGI_Calculado
```

```csharp
IGI_Calculado = reader.GetDecimal(4)  // Ya calculado y sumado desde SQL
```

**Impacto:**
- ✅ Mejor rendimiento (cálculo en base de datos)
- ✅ Consistencia garantizada (misma fórmula siempre)
- ✅ Menos código en C#

---

### **5. Formato del Pedimento**

#### ❌ **Antes:**
```sql
DP.Pim_Folio AS Pedimento
```
Resultado: `6006491`

#### ✅ **Ahora:**
```sql
Adu_AduanaSecc+'-'+Gl_Patente+'-'+Gl_Pedimento AS Pedimento
```
Resultado: `400-3621-6006491`

**Impacto:**
- ✅ Formato completo del pedimento con aduana y patente
- ✅ Más descriptivo para el usuario

---

### **6. Fecha en WHERE**

#### ❌ **Antes:**
```sql
WHERE 
    CONVERT(DATE, IIF(DP.CLP_CLAVE = 'R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago)) BETWEEN @FechaInicio AND @FechaFin
```

#### ✅ **Ahora:**
```sql
WHERE 
    CONVERT(DATE,TR.Gl_FecPagoReal) BETWEEN @FechaInicio AND @FechaFin
```

**Impacto:**
- Usa directamente la fecha de TR_GLOSA
- Más simple y directo
- Como el filtro ZIP está en el JOIN, siempre habrá registro en TR_GLOSA

---

### **7. Eliminación de Condiciones Redundantes**

#### ❌ **Antes:**
```sql
WHERE 
    ...
    AND (
        TR.Gl_FPagoIVA IN ('5','21') 
        OR TR.Gl_FPagoAdvalorem IN ('5','21')
        OR TR.Gl_Pedimento IS NULL  -- ← Incluía NO CARGADOS
    )
```

#### ✅ **Ahora:**
```sql
WHERE 
    ...
    AND (
        TR.Gl_FPagoIVA IN ('5','21') 
        OR TR.Gl_FPagoAdvalorem IN ('0','21')
        -- NO incluye TR.Gl_Pedimento IS NULL
    )
```

**Impacto:**
- Como el ZIP está en el JOIN, nunca habrá `TR.Gl_Pedimento IS NULL`
- Query más limpio y directo

---

## 📊 Query Final Completo

```sql
SELECT 
    DI.Pim_Consecutivo AS iDPedimento,
    Adu_AduanaSecc+'-'+Gl_Patente+'-'+Gl_Pedimento AS Pedimento,
    TR.Gl_FecPagoReal AS FechaPago,
    TR.Gl_ImporteADvalorem AS IGI_Pagado,
    SUM(ROUND((DI.Pid_ValorAdu * FRA.Fra_AdvGral) / 100,0)) AS IGI_Calculado,
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
    AND TR.Gl_OrigenZipGlosa = 'S'
INNER JOIN Ca_Farancelaria FRA
    ON FRA.Fra_Fraccion = IIF(LEFT(DI.Fra_Fraccion,2)= '98', DI.Fra_FraccionORIG,DI.Fra_Fraccion) 
    AND FRA.Pai_Clave = 'MEX' 
    AND FRA.Fra_TipoOper = 0
WHERE 
    CONVERT(DATE,TR.Gl_FecPagoReal) BETWEEN @FechaInicio AND @FechaFin
    AND (
        TR.Gl_FPagoIVA IN ('5','21') 
        OR TR.Gl_FPagoAdvalorem IN ('0','21')
    )
GROUP BY  
    DI.Pim_Consecutivo,
    Adu_AduanaSecc+'-'+Gl_Patente+'-'+Gl_Pedimento,
    TR.Gl_FecPagoReal,
    TR.Gl_ImporteADvalorem,
    TR.Gl_ImporteIVA,
    TR.Gl_FPagoAdvalorem,
    TR.Gl_FPagoIVA,
    CASE 
        WHEN TR.Gl_Pedimento IS NOT NULL THEN 'SI CARGADO'
        ELSE 'NO CARGADO'
    END,
    CASE 
        WHEN TR.Gl_OrigenZipGlosa = 'S' THEN 'ZIP'
        ELSE 'NO ZIP'
    END
```

---

## 🔄 Cambios en Código C#

### **Lectura de Resultados:**

```csharp
var reporte = new ReporteIGIPagado
{
    BaseDatos = baseDatos,
    IdPedimento = reader.GetInt32(0),
    Pedimento = reader.GetString(1),                    // ← Formato completo
    FechaPago = LeerFechaPago(reader, 2),
    IGI_Pagado = reader.GetDecimal(3),
    IGI_Calculado = reader.GetDecimal(4),               // ← Ya calculado y sumado desde SQL
    IVA_Pagado = reader.GetDecimal(5),
    FormaPago_IGI = reader.GetString(6),
    FormaPago_IVA = reader.GetString(7),
    EstatusGlosa = reader.GetString(8),
    EstatusOrigen = reader.GetString(9)
};

// NO se calcula IGI en C#, ya viene desde SQL
resultados.Add(reporte);
```

### **Método `CalcularIGI()` ya NO se usa en este query**

El método sigue existiendo pero solo se usa en el query **SIN validación de Glosa**.

---

## 📈 Resultado Esperado

### **Antes (Sin GROUP BY):**

```
IdPedimento | Pedimento | IGI_Calculado | Filas
8986        | 6006491   | 0.00          | 1
8986        | 6006491   | 709.00        | 2
8986        | 6006491   | 1074.00       | 3
8986        | 6006491   | 3562.00       | 4
8986        | 6006491   | 11158.00      | 5
Total: 5 filas para el mismo pedimento
```

### **Ahora (Con GROUP BY):**

```
IdPedimento | Pedimento          | IGI_Calculado | Filas
8986        | 400-3621-6006491   | 16503.00      | 1 ← Suma de todas las partidas
Total: 1 fila por pedimento
```

---

## ⚠️ Impacto en Conteo de Registros

### **Antes:**
```
Pedimento 8986 → 5 partidas → 5 filas en C#
Pedimento 8987 → 21 partidas → 21 filas en C#
Total: ~62 filas
```

### **Ahora:**
```
Pedimento 8986 → 5 partidas → 1 fila en C# (IGI sumado)
Pedimento 8987 → 21 partidas → 1 fila en C# (IGI sumado)
Total: ~4 filas (1 por pedimento único)
```

---

## ✅ Ventajas del Nuevo Query

1. **Rendimiento:**
   - ✅ Menor cantidad de registros transferidos de SQL a C#
   - ✅ Cálculo y suma en base de datos (más rápido)

2. **Precisión:**
   - ✅ Fórmula SQL garantizada consistente
   - ✅ Suma correcta de todas las partidas

3. **Mantenibilidad:**
   - ✅ Menos código en C#
   - ✅ Lógica centralizada en SQL

4. **Calidad:**
   - ✅ Solo pedimentos desde ZIP (datos confiables)
   - ✅ Formato completo del pedimento

---

## 🧪 Validación

### **Test 1: Verificar GROUP BY**

```sql
-- Ejecutar directamente en SQL
SELECT 
    DI.Pim_Consecutivo,
    COUNT(*) AS CantidadPartidas,
    SUM(ROUND((DI.Pid_ValorAdu * FRA.Fra_AdvGral) / 100,0)) AS IGI_Total
FROM ...
GROUP BY DI.Pim_Consecutivo
```

**Resultado esperado:** 1 fila por `Pim_Consecutivo` con suma total.

---

### **Test 2: Comparar Totales**

```csharp
// Query anterior (sin GROUP BY)
var reporteDetallado = GenerarReporteAnterior(...);
var totalDetallado = reporteDetallado.Sum(r => r.IGI_Calculado);

// Query nuevo (con GROUP BY)
var reporteAgrupado = GenerarReporteIGIConGlosa(...);
var totalAgrupado = reporteAgrupado.Sum(r => r.IGI_Calculado);

// Deben ser iguales
Assert.AreEqual(totalDetallado, totalAgrupado);
```

---

### **Test 3: Verificar Filtro ZIP**

```csharp
var reportes = GenerarReporteIGIConGlosa(...);

// TODOS deben tener EstatusOrigen = "ZIP"
var todosZip = reportes.All(r => r.EstatusOrigen == "ZIP");
Assert.IsTrue(todosZip);
```

---

## 📝 Archivos Modificados

```
Retorno360Tacna/
└── SERVICES/
    └── ReporteIGIService.cs                    [MODIFICADO]
        └── GenerarReporteIGIConGlosa()
            ├── Query actualizado con GROUP BY
            ├── Filtro ZIP en JOIN
            ├── IGI_Calculado desde SQL
            ├── Formato completo de pedimento
            └── FormaPago_IGI con '0' en lugar de '5'
```

---

## 🔗 Consistencia con RetornoService

El filtro de ZIP en el JOIN es consistente con:

```csharp
// RetornoService.cs
WHERE TR.Gl_OrigenZipGlosa = 'S'
```

Usado en:
- `ObtenerImportacionesValidadas()`
- `ObtenerExportacionesValidadas()`
- **ReporteIGIConGlosa()** ✅ (ahora)

---

## ⚙️ Notas Técnicas

### **¿Por qué GROUP BY incluye los CASE?**

SQL requiere que **todas las columnas** del SELECT que **no** están en funciones agregadas (SUM, COUNT, etc.) estén en el GROUP BY.

```sql
SELECT 
    DI.Pim_Consecutivo,                        -- ← En GROUP BY
    SUM(...) AS IGI_Calculado,                 -- ← Agregación (no va en GROUP BY)
    CASE WHEN ... THEN ... END AS EstatusGlosa -- ← En GROUP BY (expresión no agregada)
FROM ...
GROUP BY 
    DI.Pim_Consecutivo,
    CASE WHEN ... THEN ... END  -- ← Repetir el CASE completo
```

---

**Fecha de Actualización:** Enero 2026  
**Versión:** 3.0.8  
**Sistema:** Retorno 360° Tacna  
**Tipo:** Corrección Crítica - Query con GROUP BY  
**Estado:** ✅ IMPLEMENTADO Y COMPILADO  
**Build:** ✅ Exitoso
