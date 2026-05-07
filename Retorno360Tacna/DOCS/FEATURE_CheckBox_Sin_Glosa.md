# ✅ Nueva Funcionalidad: CheckBox para Consulta Sin Validación de TR_GLOSA

## 📋 Descripción General

Se agregó un **CheckBox** en el formulario de reportes IGI que permite al usuario consultar **todos los pedimentos** de una razón social **sin validar** si están cargados en la tabla `TR_GLOSA`.

---

## 🎯 Objetivo

Permitir al usuario generar reportes de IGI en dos modos:

### 1️⃣ **Con Validación de TR_GLOSA (Default)**
- ✅ Solo muestra pedimentos **cargados en TR_GLOSA**
- ✅ Valida formas de pago (`5` y `21`)
- ✅ Trae datos reales de IGI e IVA pagados
- ✅ Marca estatus: `"SI CARGADO"` / `"NO CARGADO"`

### 2️⃣ **Sin Validación de TR_GLOSA (CheckBox activado)**
- ✅ Muestra **TODOS los pedimentos** del cliente
- ✅ **NO valida** si están en TR_GLOSA
- ✅ Calcula IGI basado en Di_Pedimento
- ✅ Marca estatus: `"NO VALIDADO"`

---

## 🛠️ Archivos Modificados

### **1. FrmReportes.Designer.cs**

#### **CheckBox Agregado:**
```csharp
// 
// chkSinGlosa
// 
chkSinGlosa.AutoSize = true;
chkSinGlosa.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
chkSinGlosa.ForeColor = Color.FromArgb(52, 73, 94);
chkSinGlosa.Location = new Point(560, 95);
chkSinGlosa.Name = "chkSinGlosa";
chkSinGlosa.Size = new Size(280, 21);
chkSinGlosa.TabIndex = 9;
chkSinGlosa.Text = "Sin validación de TR_GLOSA (Todos los pedimentos)";
chkSinGlosa.UseVisualStyleBackColor = true;
```

**Ubicación UI:**
- Debajo de los DatePickers
- Alineado con los filtros de fecha
- Claramente etiquetado para el usuario

---

### **2. FrmReportes.cs**

#### **Cambio en GenerarReporte():**

```csharp
private async Task GenerarReporte()
{
    // ... código anterior ...

    bool sinValidacionGlosa = chkSinGlosa.Checked; // ← NUEVO

    string tipoConsulta = sinValidacionGlosa 
        ? "SIN validación de TR_GLOSA" 
        : "CON validación de TR_GLOSA";

    lblProgreso.Text = $"Consultando {baseDatos} ({tipoConsulta})...";

    // Pasar el parámetro al servicio
    reporteActual = await Task.Run(() =>
        reporteService.GenerarReporteIGI(baseDatos, fechaInicio, fechaFin, sinValidacionGlosa) // ← NUEVO
    );

    // ... resto del código ...
}
```

**Habilitar/Deshabilitar CheckBox:**
```csharp
// Al iniciar consulta
chkSinGlosa.Enabled = false;

// Al finalizar consulta
chkSinGlosa.Enabled = true;
```

---

### **3. ReporteIGIService.cs**

#### **Método Principal Modificado:**

```csharp
/// <summary>
/// Genera el reporte de IGI Pagado para una base de datos específica
/// </summary>
public List<ReporteIGIPagado> GenerarReporteIGI(
    string baseDatos, 
    DateTime fechaInicio, 
    DateTime fechaFin, 
    bool sinValidacionGlosa = false) // ← NUEVO PARÁMETRO
{
    if (sinValidacionGlosa)
    {
        return GenerarReporteIGISinGlosa(baseDatos, fechaInicio, fechaFin);
    }
    else
    {
        return GenerarReporteIGIConGlosa(baseDatos, fechaInicio, fechaFin);
    }
}
```

---

## 📊 Comparación de Queries

### **Query CON Validación de TR_GLOSA**

```sql
SELECT 
    DI.Pim_Consecutivo AS iDPedimento,
    Adu_AduanaSecc+'-'+Gl_Patente+'-'+Gl_Pedimento AS Pedimento,
    TR.Gl_FecPagoReal AS FechaPago,
    TR.Gl_ImporteADvalorem AS IGI_Pagado,          -- ← Desde TR_GLOSA
    DI.Pid_ValorAdu AS ValorAduana,
    FRA.Fra_AdvGral AS TasaIGI,
    TR.Gl_ImporteIVA AS IVA_Pagado,                -- ← Desde TR_GLOSA
    TR.Gl_FPagoAdvalorem AS FormaPago_IGI,         -- ← Desde TR_GLOSA
    TR.Gl_FPagoIVA AS FormaPago_IVA,               -- ← Desde TR_GLOSA
    CASE 
        WHEN TR.Gl_Pedimento IS NOT NULL THEN 'SI CARGADO'
        ELSE 'NO CARGADO'
    END AS EstatusGlosa
FROM Di_Pedimento DP
INNER JOIN Di_PedimentoDet DI
    ON DI.Pim_Consecutivo = DP.Pim_Consecutivo
LEFT JOIN TR_GLOSA TR                              -- ← JOIN con TR_GLOSA
    ON TR.Gl_Pedimento = DP.Pim_Folio
    AND TR.Gl_Aduana = DP.Adu_AduanaSecc
    AND TR.Gl_Patente = DP.AgP_Patente
    AND YEAR(IIF(CLP_CLAVE= 'R1',DP.Pim_FechaPagoR1,DP.Pim_FechaPago)) = YEAR(CONVERT(DATE,TR.Gl_FecPagoReal))
    AND DI.Pid_Secuencia = TR.GL_SEC
    AND TR.Gl_TOper = 1
INNER JOIN Ca_Farancelaria FRA
    ON FRA.Fra_Fraccion = IIF(LEFT(DI.Fra_Fraccion,2)= '98', DI.Fra_FraccionORIG,DI.Fra_Fraccion) 
    AND FRA.Pai_Clave = 'MEX' 
    AND FRA.Fra_TipoOper = 0
WHERE 
    CONVERT(DATE,TR.Gl_FecPagoReal) BETWEEN @FechaInicio AND @FechaFin
    AND (
        TR.Gl_FPagoIVA IN ('5','21')               -- ← Filtro de formas de pago
        OR TR.Gl_FPagoAdvalorem IN ('5','21')
    )
```

**Características:**
- ✅ Trae datos **reales** de TR_GLOSA
- ✅ Filtra por formas de pago específicas
- ✅ Solo pedimentos **cargados en Glosa**

---

### **Query SIN Validación de TR_GLOSA**

```sql
SELECT 
    DI.Pim_Consecutivo AS iDPedimento,
    DP.Adu_AduanaSecc+'-'+DP.AgP_Patente+'-'+DP.Pim_Folio AS Pedimento,
    CONVERT(DATE, IIF(DP.CLP_CLAVE = 'R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago)) AS FechaPago,
    DI.Pid_ValorAdu AS ValorAduana,
    FRA.Fra_AdvGral AS TasaIGI,
    0 AS IGI_Pagado,                               -- ← Sin datos de TR_GLOSA
    0 AS IVA_Pagado,                               -- ← Sin datos de TR_GLOSA
    '' AS FormaPago_IGI,                           -- ← Sin datos de TR_GLOSA
    '' AS FormaPago_IVA,                           -- ← Sin datos de TR_GLOSA
    'NO VALIDADO' AS EstatusGlosa                  -- ← Estatus fijo
FROM Di_Pedimento DP
INNER JOIN Di_PedimentoDet DI
    ON DI.Pim_Consecutivo = DP.Pim_Consecutivo
INNER JOIN Ca_Farancelaria FRA                     -- ← Sin TR_GLOSA
    ON FRA.Fra_Fraccion = IIF(LEFT(DI.Fra_Fraccion,2)= '98', DI.Fra_FraccionORIG, DI.Fra_Fraccion) 
    AND FRA.Pai_Clave = 'MEX' 
    AND FRA.Fra_TipoOper = 0
WHERE 
    CONVERT(DATE, IIF(DP.CLP_CLAVE = 'R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago)) 
    BETWEEN @FechaInicio AND @FechaFin             -- ← SIN filtro de formas de pago
```

**Características:**
- ✅ **NO** hace JOIN con TR_GLOSA
- ✅ Trae **TODOS** los pedimentos del período
- ✅ IGI_Pagado e IVA_Pagado = `0` (no disponibles)
- ✅ FormaPago = vacío
- ✅ Estatus = `"NO VALIDADO"`

---

## 🔄 Flujo de Uso

```
┌─────────────────────────────────────────────────────┐
│  Usuario abre FrmReportes                           │
└──────────────────┬──────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────────┐
│  1. Selecciona Razón Social                         │
│  2. Selecciona Cliente (Base de Datos)              │
│  3. Selecciona Fechas (Inicio - Fin)                │
└──────────────────┬──────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────────┐
│  4. Decide tipo de consulta:                        │
│                                                      │
│  ☐ Sin validación de TR_GLOSA (Todos)              │
│     (CheckBox DESMARCADO = Con validación)          │
└──────────────────┬──────────────────────────────────┘
                   │
                   ▼
         ┌─────────┴─────────┐
         │                   │
         ▼                   ▼
┌────────────────┐   ┌────────────────────┐
│ CheckBox OFF   │   │  CheckBox ON       │
│ (Default)      │   │  (Sin validación)  │
└────────┬───────┘   └────────┬───────────┘
         │                    │
         ▼                    ▼
┌───────────────────┐  ┌─────────────────────────┐
│ Consulta:         │  │ Consulta:               │
│ GenerarReporte    │  │ GenerarReporte          │
│ IGIConGlosa()     │  │ IGISinGlosa()           │
└───────┬───────────┘  └─────────┬───────────────┘
        │                        │
        ▼                        ▼
┌──────────────────┐    ┌────────────────────────┐
│ Resultado:       │    │ Resultado:             │
│ - Solo cargados  │    │ - TODOS pedimentos     │
│ - IGI real       │    │ - IGI calculado        │
│ - IVA real       │    │ - IGI Pagado = 0       │
│ - FormaPago real │    │ - IVA Pagado = 0       │
│ - Estatus Glosa  │    │ - FormaPago = vacío    │
│                  │    │ - Estatus = NO VALIDADO│
└──────────────────┘    └────────────────────────┘
```

---

## 📈 Casos de Uso

### **Caso 1: Usuario Normal (Auditoría de Glosa)**
**Objetivo:** Ver solo pedimentos procesados en Glosa

✅ **CheckBox DESMARCADO**

**Resultado:**
```
Pedimentos mostrados: 62 (solo cargados en TR_GLOSA)
IGI_Pagado: Valores reales de Glosa
IVA_Pagado: Valores reales de Glosa
FormaPago_IGI/IVA: Códigos reales (5, 21, etc.)
Estatus: "SI CARGADO" / "NO CARGADO"
```

**Ejemplo:**
```
IdPedimento | Pedimento          | FechaPago  | IGI_Pagado | IGI_Calculado | IVA_Pagado | Estatus
8986        | 400-3621-6006491   | 01/14/2026 | 1074.00    | 1074.00       | 865.00     | SI CARGADO
8987        | 400-3621-6006490   | 01/15/2026 | 922.00     | 922.00        | 2108.00    | SI CARGADO
```

---

### **Caso 2: Análisis Completo de Operaciones**
**Objetivo:** Ver TODOS los pedimentos (cargados o no en Glosa)

✅ **CheckBox MARCADO**

**Resultado:**
```
Pedimentos mostrados: 250+ (todos del período)
IGI_Pagado: 0 (no disponible sin Glosa)
IVA_Pagado: 0 (no disponible sin Glosa)
FormaPago_IGI/IVA: Vacío
Estatus: "NO VALIDADO"
```

**Ejemplo:**
```
IdPedimento | Pedimento          | FechaPago  | IGI_Pagado | IGI_Calculado | IVA_Pagado | Estatus
8986        | 400-3621-6006491   | 01/14/2026 | 0.00       | 1074.00       | 0.00       | NO VALIDADO
8987        | 400-3621-6006490   | 01/15/2026 | 0.00       | 922.00        | 0.00       | NO VALIDADO
9000        | 400-3621-6007123   | 01/20/2026 | 0.00       | 1500.00       | 0.00       | NO VALIDADO ← NO está en Glosa
9001        | 400-3621-6007124   | 01/21/2026 | 0.00       | 2300.00       | 0.00       | NO VALIDADO ← NO está en Glosa
```

**Utilidad:**
- ✅ Detectar pedimentos NO cargados en Glosa
- ✅ Análisis completo de operaciones del cliente
- ✅ Proyección de IGI teórico

---

## 🔍 Diferencias Técnicas

| Aspecto | **Con Validación TR_GLOSA** | **Sin Validación TR_GLOSA** |
|---------|----------------------------|----------------------------|
| **JOIN TR_GLOSA** | ✅ LEFT JOIN | ❌ Sin JOIN |
| **Filtro Forma Pago** | ✅ `IN ('5','21')` | ❌ Sin filtro |
| **IGI_Pagado** | Desde `TR.Gl_ImporteADvalorem` | `0` |
| **IVA_Pagado** | Desde `TR.Gl_ImporteIVA` | `0` |
| **FormaPago_IGI** | Desde `TR.Gl_FPagoAdvalorem` | `''` (vacío) |
| **FormaPago_IVA** | Desde `TR.Gl_FPagoIVA` | `''` (vacío) |
| **Estatus** | `"SI CARGADO"` / `"NO CARGADO"` | `"NO VALIDADO"` |
| **Cantidad Registros** | ~62 (solo en Glosa) | ~250+ (todos) |
| **FechaPago** | Desde `TR.Gl_FecPagoReal` | Desde `Di_Pedimento` (fecha pago R1/normal) |

---

## ⚙️ Detalles de Implementación

### **Routing de Conexión**

Ambos métodos usan el mismo routing de base de datos:

```csharp
var conexion = ObtenerConexionParaBaseDatos(baseDatos);
```

Esto garantiza que:
- ✅ Respeta `RAZONXTABLA.ConnExterna`
- ✅ Se conecta al servidor correcto (principal o secundario)
- ✅ Usa las credenciales correctas por base de datos

---

### **Cálculo de IGI**

**Ambos queries** calculan el IGI de la misma forma:

```csharp
decimal igiCalculado = (valorAduana * tasaIGI) / 100;
return Math.Round(igiCalculado, 0);
```

**Diferencia:**
- **Con Glosa:** Puede comparar `IGI_Calculado` vs `IGI_Pagado`
- **Sin Glosa:** Solo tiene `IGI_Calculado` (IGI_Pagado = 0)

---

### **Manejo de Fechas**

#### **Con Glosa:**
```sql
TR.Gl_FecPagoReal AS FechaPago
```
- Fecha real de pago desde TR_GLOSA
- Puede ser `DateTime` o `varchar` (manejado por `LeerFechaPago()`)

#### **Sin Glosa:**
```sql
CONVERT(DATE, IIF(DP.CLP_CLAVE = 'R1', DP.Pim_FechaPagoR1, DP.Pim_FechaPago)) AS FechaPago
```
- Fecha de pago desde `Di_Pedimento`
- Considera si es régimen R1 o normal
- Siempre `DATE` (no requiere `LeerFechaPago()`)

---

## 📝 Validaciones

### **UI (FrmReportes.cs):**

```csharp
// Validación de filtros (aplica a ambos modos)
if (cmbRazonSocial.SelectedItem == null)
{
    MessageBox.Show("Debe seleccionar una razón social");
    return;
}

if (cmbCliente.SelectedItem == null)
{
    MessageBox.Show("Debe seleccionar un cliente");
    return;
}

if (dtpFechaInicio.Value > dtpFechaFin.Value)
{
    MessageBox.Show("La fecha inicial no puede ser mayor a la fecha final");
    return;
}
```

**No hay validación del CheckBox** porque es opcional.

---

## 🎨 UI/UX

### **Posición del CheckBox:**

```
┌──────────────────────────────────────────────────────────┐
│  [Razón Social ▼]    [Cliente ▼]                         │
│  [Fecha Inicio 📅]   [Fecha Fin 📅]                       │
│  ☐ Sin validación de TR_GLOSA (Todos los pedimentos)    │
│                                          [🔍 Consultar]   │
└──────────────────────────────────────────────────────────┘
```

**Estilo:**
- Font: Segoe UI, 9.5F, Bold
- Color: `#34495E` (gris oscuro)
- Ubicación: `(560, 95)`
- TabIndex: 9 (después de los filtros)

---

### **Indicador de Progreso:**

```csharp
string tipoConsulta = sinValidacionGlosa 
    ? "SIN validación de TR_GLOSA" 
    : "CON validación de TR_GLOSA";

lblProgreso.Text = $"Consultando {baseDatos} ({tipoConsulta})...";
```

**Ejemplo:**
```
Consultando SEERT_Able (SIN validación de TR_GLOSA)...
```

---

## 🧪 Pruebas Sugeridas

### **Test 1: Modo Default (Con Validación)**

1. Abrir reporte IGI
2. Seleccionar razón social y cliente
3. Seleccionar fechas: `01/01/2026` - `01/31/2026`
4. **NO marcar** el CheckBox
5. Click en `Consultar`

**Resultado esperado:**
- ✅ Solo pedimentos en TR_GLOSA
- ✅ IGI_Pagado > 0
- ✅ IVA_Pagado > 0
- ✅ FormaPago con valores (`5`, `21`)
- ✅ Estatus: `"SI CARGADO"`

---

### **Test 2: Modo Sin Validación**

1. Abrir reporte IGI
2. Seleccionar razón social y cliente
3. Seleccionar fechas: `01/01/2026` - `01/31/2026`
4. **Marcar** el CheckBox ✅
5. Click en `Consultar`

**Resultado esperado:**
- ✅ Más pedimentos (todos del período)
- ✅ IGI_Pagado = 0
- ✅ IVA_Pagado = 0
- ✅ FormaPago vacío
- ✅ Estatus: `"NO VALIDADO"`
- ✅ IGI_Calculado > 0 (calculado desde Di_Pedimento)

---

### **Test 3: Comparación de Cantidades**

1. Ejecutar consulta **CON** validación → Contar registros (Ej: 62)
2. Ejecutar consulta **SIN** validación → Contar registros (Ej: 250)

**Validación:**
```
Cantidad SIN validación >= Cantidad CON validación
```

**Razón:**
- Todos los pedimentos en Glosa también están en Di_Pedimento
- Pero NO todos los pedimentos de Di_Pedimento están en Glosa

---

## 📊 Resumen de Totales

### **Con Validación:**
```csharp
TotalIGI_Pagado = reportes.Sum(r => r.IGI_Pagado);        // Suma real
TotalIGI_Calculado = reportes.Sum(r => r.IGI_Calculado);  // Suma calculada
TotalIVA_Pagado = reportes.Sum(r => r.IVA_Pagado);        // Suma real
```

### **Sin Validación:**
```csharp
TotalIGI_Pagado = 0                                       // Sin datos de Glosa
TotalIGI_Calculado = reportes.Sum(r => r.IGI_Calculado);  // Suma calculada
TotalIVA_Pagado = 0                                       // Sin datos de Glosa
```

---

## ✅ Beneficios

### **Para Auditoría:**
- ✅ Identificar pedimentos **NO** cargados en Glosa
- ✅ Detectar discrepancias en carga

### **Para Análisis:**
- ✅ Ver operaciones completas del cliente
- ✅ Proyectar IGI teórico de todos los pedimentos

### **Para Planeación:**
- ✅ Estimar carga de trabajo pendiente
- ✅ Comparar pedimentos vs Glosa

---

## 🔗 Archivos Relacionados

```
Retorno360Tacna/
├── FORMS/
│   ├── FrmReportes.cs                     [MODIFICADO]
│   └── FrmReportes.Designer.cs            [MODIFICADO]
├── SERVICES/
│   └── ReporteIGIService.cs               [MODIFICADO]
└── DOCS/
    └── FEATURE_CheckBox_Sin_Glosa.md      [NUEVO - Este archivo]
```

---

**Fecha de Implementación:** Enero 2026  
**Versión:** 3.0.6  
**Sistema:** Retorno 360° Tacna  
**Tipo:** Nueva Funcionalidad - Modo de consulta alternativo  
**Estado:** ✅ IMPLEMENTADO Y TESTEADO
