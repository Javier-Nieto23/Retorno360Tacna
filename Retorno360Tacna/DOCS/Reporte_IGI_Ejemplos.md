# 📘 Reporte IGI Pagado - Ejemplos de Uso

## 🎯 Ejemplos Prácticos

### 📌 Ejemplo 1: Consulta Básica

**Escenario:** Consultar IGI pagado de SEERT en enero 2026

**Pasos:**
1. Abrir sistema → Click en "Reportes de Retorno" (botón Button3 será renombrado)
2. Seleccionar filtros:
   - **Razón Social:** SEERT
   - **Cliente:** SEERT_OPERACIONES
   - **Fecha Inicio:** 01/01/2026
   - **Fecha Fin:** 31/01/2026
3. Click en **🔍 Consultar**

**Resultado Esperado:**
```
DataGridView muestra:
┌──────────────┬────────────┬─────────────┬────────────┬──────────────┬─────────────┐
│ Base Datos   │ Pedimento  │ Fecha Pago  │ IGI Pagado │ IGI Calculado│ Diferencia  │
├──────────────┼────────────┼─────────────┼────────────┼──────────────┼─────────────┤
│ SEERT_OPE... │ 06-3496... │ 15/01/2026  │ $50,000.00 │ $49,850.00   │ $150.00     │
│ SEERT_OPE... │ 06-3496... │ 18/01/2026  │ $75,200.00 │ $75,000.00   │ $200.00     │
│ ...          │ ...        │ ...         │ ...        │ ...          │ ...         │
└──────────────┴────────────┴─────────────┴────────────┴──────────────┴─────────────┘

Panel Resumen:
📊 Total Pedimentos: 45 | 💰 IGI Pagado: $1,234,567.89 | 
🧮 IGI Calculado: $1,200,000.00 | 📈 Diferencia: $34,567.89 | 
💵 IVA Pagado: $567,890.12
```

---

### 📌 Ejemplo 2: Base de Datos en Servidor Externo

**Escenario:** Consultar VIDRIOS (servidor secundario 172.20.21.33)

**Código Interno (automático):**
```csharp
// 1. Usuario selecciona VIDRIOS
var baseDatos = "SEERT_VIDRIOS";

// 2. Sistema detecta conexión externa
var conexionExterna = reporteService.ObtenerConexionExterna(baseDatos);
// conexionExterna.TieneConexionExterna = true
// conexionExterna.Servidor = "172.20.21.33"

// 3. Sistema crea conexión al servidor secundario
var conexion = reporteService.ObtenerConexionParaBaseDatos(baseDatos);
// Conexión apunta a 172.20.21.33

// 4. Ejecuta query en servidor externo
var resultados = reporteService.GenerarReporteIGI(baseDatos, fechaInicio, fechaFin);
```

**Usuario NO nota diferencia:**
- Mismo flujo de consulta
- Mismos resultados
- Sistema rutea transparentemente

---

### 📌 Ejemplo 3: Cálculo de IGI

**Datos de Entrada (por partida):**
```
Valor Aduana: $100,000.00
Tasa IGI: 15%
```

**Proceso:**
```csharp
// Método en ReporteIGIService
private decimal CalcularIGI(decimal valorAduana, decimal tasaIGI)
{
    // Formula: ROUND((ValorAduana * TasaIGI) / 100, 0)
    decimal igiCalculado = (100000 * 15) / 100;
    // igiCalculado = 15000.00

    return Math.Round(igiCalculado, 0);
    // Retorna: 15,000.00
}
```

**Agrupación por Pedimento:**
```csharp
// Pedimento con 3 partidas
Partida 1: IGI = $15,000.00
Partida 2: IGI = $8,500.00
Partida 3: IGI = $12,300.00

// IGI Total del Pedimento
IGI_Calculado = $35,800.00
```

---

### 📌 Ejemplo 4: Filtrado por Forma de Pago

**Condición SQL:**
```sql
WHERE (
    TR.Gl_FPagoIVA IN ('5','21')      -- Transferencia o Compensación
    OR TR.Gl_FPagoAdvalorem IN ('5','21')
)
```

**Código de Conversión:**
```csharp
// Modelo ReporteIGIPagado
public string FormaPagoIGI_Descripcion => ObtenerDescripcionFormaPago(FormaPago_IGI);

private string ObtenerDescripcionFormaPago(string codigo)
{
    return codigo switch
    {
        "5" => "Transferencia",
        "21" => "Compensación",
        _ => codigo
    };
}
```

**Resultado en DataGridView:**
```
Forma Pago IGI: Transferencia    (en lugar de "5")
Forma Pago IVA: Compensación     (en lugar de "21")
```

---

### 📌 Ejemplo 5: Resumen Estadístico

**Código del Servicio:**
```csharp
public ResumenIGI GenerarResumen(List<ReporteIGIPagado> reportes)
{
    return new ResumenIGI
    {
        TotalIGI_Pagado = reportes.Sum(r => r.IGI_Pagado),
        // Suma: $1,234,567.89

        TotalIGI_Calculado = reportes.Sum(r => r.IGI_Calculado),
        // Suma: $1,200,000.00

        TotalIVA_Pagado = reportes.Sum(r => r.IVA_Pagado),
        // Suma: $567,890.12

        TotalPedimentos = reportes.Count,
        // Conteo: 45

        PedimentosCargadosGlosa = reportes.Count(r => r.EstatusGlosa == "SI CARGADO")
        // Conteo: 42
    };
}
```

**Propiedad Calculada:**
```csharp
public decimal DiferenciaTotal => TotalIGI_Pagado - TotalIGI_Calculado;
// $1,234,567.89 - $1,200,000.00 = $34,567.89
```

---

## 🧪 Casos de Prueba

### ✅ Test Case 1: Sin Datos

**Input:**
- Base Datos: SEERT_OPERACIONES
- Fecha Inicio: 01/06/2026
- Fecha Fin: 30/06/2026

**Proceso:**
```csharp
var reportes = reporteService.GenerarReporteIGI("SEERT_OPERACIONES", 
    new DateTime(2026, 6, 1), 
    new DateTime(2026, 6, 30));

// reportes.Count = 0
```

**Output:**
```
DataGridView: Vacío
Panel Resumen: "Sin resultados para los filtros seleccionados"
lblProgreso: "No se encontraron registros"
```

---

### ✅ Test Case 2: Fecha Inválida

**Input:**
- Fecha Inicio: 31/01/2026
- Fecha Fin: 01/01/2026  ← Error

**Validación en btnConsultar_Click:**
```csharp
if (dtpFechaInicio.Value > dtpFechaFin.Value)
{
    MessageBox.Show(
        "La fecha inicial no puede ser mayor a la fecha final", 
        "Validación", 
        MessageBoxButtons.OK, 
        MessageBoxIcon.Warning
    );
    return; // No ejecuta consulta
}
```

**Output:**
```
MessageBox: "La fecha inicial no puede ser mayor a la fecha final"
```

---

### ✅ Test Case 3: Error de Conexión

**Escenario:** Base de datos no accesible

**Manejo de Error:**
```csharp
try
{
    reporteActual = await Task.Run(() =>
        reporteService.GenerarReporteIGI(baseDatos, fechaInicio, fechaFin)
    );
}
catch (Exception ex)
{
    MessageBox.Show(
        $"Error al generar el reporte:\n{ex.Message}",
        "Error",
        MessageBoxButtons.OK,
        MessageBoxIcon.Error
    );
    lblProgreso.Text = "Error al generar reporte";
    lblResumenInfo.Text = "Error en la consulta";
}
```

**Output:**
```
MessageBox: "Error al generar el reporte: Cannot open database..."
```

---

## 🔄 Flujo Completo de Datos

### 1️⃣ **Carga Inicial del Formulario**

```csharp
private void FrmReportes_Load(object sender, EventArgs e)
{
    // 1. Configurar fechas por defecto
    dtpFechaInicio.Value = new DateTime(2026, 1, 1);  // Primer día del mes
    dtpFechaFin.Value = DateTime.Now;                 // Hoy

    // 2. Cargar razones sociales
    CargarRazonesSociales();
    // → Llama a reporteService.ObtenerRazonesSociales()
    // → Consulta: SELECT DISTINCT IdRazon, Razon, DB FROM RAZONXTABLA

    // 3. Configurar DataGridView
    ConfigurarDataGridView();
    // → AutoSizeColumnsMode, ReadOnly, SelectionMode, etc.
}
```

---

### 2️⃣ **Selección de Razón Social**

```csharp
private async void cmbRazonSocial_SelectedIndexChanged(object sender, EventArgs e)
{
    var razonSeleccionada = (RazonSocial)cmbRazonSocial.SelectedItem;
    // razonSeleccionada.NombreRazon = "SEERT"

    var basesDatos = await Task.Run(() =>
        reporteService.ObtenerBasesDatosRazon("SEERT")
    );
    // Consulta: SELECT DISTINCT DB FROM RAZONXTABLA WHERE Razon = 'SEERT'
    // Retorna: ["SEERT_OPERACIONES", "SEERT_VIDRIOS", ...]

    cmbCliente.DataSource = basesDatos;
}
```

---

### 3️⃣ **Consulta del Reporte**

```csharp
private async void btnConsultar_Click(object sender, EventArgs e)
{
    // 1. Validaciones
    if (cmbRazonSocial.SelectedItem == null) return;
    if (cmbCliente.SelectedItem == null) return;
    if (dtpFechaInicio.Value > dtpFechaFin.Value) return;

    // 2. Deshabilitar controles
    btnConsultar.Enabled = false;

    // 3. Ejecutar consulta
    await GenerarReporte();

    // 4. Rehabilitar controles
    btnConsultar.Enabled = true;
}

private async Task GenerarReporte()
{
    string baseDatos = "SEERT_OPERACIONES";
    DateTime fechaInicio = new DateTime(2026, 1, 1);
    DateTime fechaFin = new DateTime(2026, 1, 31);

    // Ejecutar en background
    reporteActual = await Task.Run(() =>
        reporteService.GenerarReporteIGI(baseDatos, fechaInicio, fechaFin)
    );

    // Mostrar resultados
    MostrarResultados();
}
```

---

### 4️⃣ **Generación del Reporte (Servicio)**

```csharp
public List<ReporteIGIPagado> GenerarReporteIGI(
    string baseDatos, 
    DateTime fechaInicio, 
    DateTime fechaFin)
{
    // 1. Obtener conexión apropiada
    var conexion = ObtenerConexionParaBaseDatos(baseDatos);
    // → Revisa RAZONXTABLA.ConnExterna
    // → Si 'S': usa servidor externo
    // → Si 'N' o NULL: usa servidor principal

    // 2. Ejecutar query SQL
    using var cn = conexion.ObtenerConexion();
    using var cmd = new SqlCommand(sqlQuery, cn);
    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);
    cn.Open();

    // 3. Leer datos y calcular IGI
    var pedimentosTemp = new Dictionary<int, ReporteIGIPagado>();

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        int idPedimento = reader.GetInt32(0);

        if (!pedimentosTemp.ContainsKey(idPedimento))
        {
            // Crear nuevo registro de pedimento
            pedimentosTemp[idPedimento] = new ReporteIGIPagado
            {
                IdPedimento = idPedimento,
                Pedimento = reader.GetString(1),
                FechaPago = reader.GetDateTime(2),
                IGI_Pagado = reader.GetDecimal(3),
                // ...
            };
        }

        // Calcular IGI de esta partida
        decimal valorAduana = reader.GetDecimal(4);
        decimal tasaIGI = reader.GetDecimal(5);
        decimal igiPartida = CalcularIGI(valorAduana, tasaIGI);

        // Acumular IGI calculado
        pedimentosTemp[idPedimento].IGI_Calculado += igiPartida;
    }

    return pedimentosTemp.Values.ToList();
}
```

---

### 5️⃣ **Mostrar Resultados**

```csharp
private void MostrarResultados()
{
    // 1. Convertir a DataTable
    var dataTable = reporteService.ConvertirADataTable(reporteActual);

    // 2. Asignar a DataGridView
    dgvReporte.DataSource = dataTable;

    // 3. Formatear columnas
    FormatearColumnas();
    // → IGI Pagado: Format = "C2"
    // → Fecha Pago: Format = "dd/MM/yyyy"

    // 4. Generar resumen
    var resumen = reporteService.GenerarResumen(reporteActual);
    MostrarResumen(resumen);

    // 5. Actualizar progreso
    lblProgreso.Text = $"Consulta completada: {reporteActual.Count} registros";
}
```

---

## 🎓 Tips de Desarrollo

### 💡 Tip 1: Agregar Columna Calculada

**Escenario:** Agregar "% Diferencia IGI"

```csharp
// En ConvertirADataTable()
dt.Columns.Add("% Diferencia", typeof(decimal));

foreach (var reporte in reportes)
{
    decimal porcentaje = reporte.IGI_Calculado != 0
        ? (reporte.DiferenciaIGI / reporte.IGI_Calculado) * 100
        : 0;

    dt.Rows.Add(
        reporte.BaseDatos,
        reporte.Pedimento,
        // ...
        porcentaje  // Nueva columna
    );
}
```

---

### 💡 Tip 2: Exportar a Excel

```csharp
// Agregar botón "Exportar"
private void btnExportar_Click(object sender, EventArgs e)
{
    if (!reporteActual.Any())
    {
        MessageBox.Show("No hay datos para exportar");
        return;
    }

    var dataTable = reporteService.ConvertirADataTable(reporteActual);

    // Usar librería EPPlus o similar
    ExportarAExcel(dataTable, "Reporte_IGI_Pagado.xlsx");
}
```

---

### 💡 Tip 3: Filtrar DataGridView

```csharp
// Agregar TextBox de búsqueda
private void txtBuscar_TextChanged(object sender, EventArgs e)
{
    if (dgvReporte.DataSource is DataTable dt)
    {
        string filtro = txtBuscar.Text;
        dt.DefaultView.RowFilter = $"Pedimento LIKE '%{filtro}%'";
    }
}
```

---

## 🎯 Casos de Uso Avanzados

### 🔧 Uso 1: Reporte Consolidado (Futuro)

```csharp
// En FrmReportes (extensión futura)
private async Task GenerarReporteConsolidado()
{
    var basesDatos = new List<string>
    {
        "SEERT_OPERACIONES",
        "SEERT_VIDRIOS",
        "OTRA_BASE"
    };

    var progreso = new Progress<string>(mensaje =>
    {
        lblProgreso.Text = mensaje;
    });

    reporteActual = await Task.Run(() =>
        reporteService.GenerarReporteConsolidado(
            basesDatos,
            dtpFechaInicio.Value,
            dtpFechaFin.Value,
            progreso
        )
    );

    MostrarResultados();
}
```

**Output en lblProgreso:**
```
Consultando base de datos: SEERT_OPERACIONES...
✓ SEERT_OPERACIONES: 25 registros
Consultando base de datos: SEERT_VIDRIOS...
✓ SEERT_VIDRIOS: 15 registros
Consultando base de datos: OTRA_BASE...
✓ OTRA_BASE: 10 registros
```

---

### 🔧 Uso 2: Crear Nuevo Reporte (Pedimentos)

```csharp
// 1. Modelo
public class ReportePedimentos : ReporteBase
{
    public string NumPedimento { get; set; }
    public string Tipo { get; set; }
    public decimal ValorComercial { get; set; }
}

// 2. Servicio
public class ReportePedimentosService : ReporteServiceBase
{
    public ReportePedimentosService(ConexionInfo conexion) 
        : base(conexion) { }

    public List<ReportePedimentos> Generar(
        string baseDatos, 
        DateTime fechaInicio, 
        DateTime fechaFin)
    {
        var conexion = ObtenerConexionParaBaseDatos(baseDatos);
        // Ya tiene ruteo multi-servidor automático

        string sql = @"
            SELECT 
                Pim_Folio,
                Pim_TipoOper,
                Pim_ValorDls
            FROM Di_Pedimento
            WHERE Pim_FechaPago BETWEEN @FechaInicio AND @FechaFin";

        using var cn = conexion.ObtenerConexion();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
        cmd.Parameters.AddWithValue("@FechaFin", fechaFin);
        cn.Open();

        var resultados = new List<ReportePedimentos>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            resultados.Add(new ReportePedimentos
            {
                NumPedimento = reader.GetString(0),
                Tipo = reader.GetInt32(1) == 1 ? "Importación" : "Exportación",
                ValorComercial = reader.GetDecimal(2)
            });
        }

        return resultados;
    }
}

// 3. Uso en formulario
var service = new ReportePedimentosService(conexionActual);
var razones = service.ObtenerRazonesSociales();  // ← Heredado
var bases = service.ObtenerBasesDatosRazon("SEERT");  // ← Heredado
var reportes = service.Generar("SEERT_OPERACIONES", inicio, fin);  // ← Propio
```

---

## 📚 Recursos Adicionales

- **Documentación Completa:** `Reporte_IGI_Pagado_Documentacion.md`
- **Resumen Ejecutivo:** `Reporte_IGI_RESUMEN.md`
- **Código Fuente:** 
  - `MODELS/ReporteIGI.cs`
  - `SERVICES/ReporteServiceBase.cs`
  - `SERVICES/ReporteIGIService.cs`
  - `FORMS/FrmReportes.cs`

---

**Fecha:** Enero 2026  
**Versión:** 3.0  
**Sistema:** Retorno 360° Tacna  
**Desarrollado por:** Javier Nieto
