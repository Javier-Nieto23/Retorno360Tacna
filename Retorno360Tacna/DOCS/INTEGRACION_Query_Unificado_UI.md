# Integración del Query Unificado de Conciliación en la UI

## Fecha
2025-01-XX

## Objetivo
Integrar el nuevo método `ObtenerConciliacionIGI()` en la interfaz de usuario `FrmReportes.cs` para mostrar resúmenes IGI/IVA en las grillas principales y abrir detalles filtrados por forma de pago al hacer doble clic en una fila.

---

## Cambios Realizados

### 1. **Nuevos Campos de Clase en `FrmReportes.cs`**

Se agregaron dos campos privados para almacenar las tablas de detalle:

```csharp
// Tablas de detalle para mostrar al hacer doble clic
private System.Data.DataTable? detalleIGIActual;
private System.Data.DataTable? detalleIVAActual;
```

**Propósito**: Mantener en memoria los detalles completos (pedimentos nivel partida) para mostrarlos filtrados cuando el usuario haga doble clic en un resumen.

---

### 2. **Modificación del Método `GenerarReporte()` - Base Específica (CON Glosa)**

**Antes**:
```csharp
var resultado = await Task.Run(() => reporteService.ObtenerResumenTablasPorBase(baseDatos, fechaInicio, fechaFin));
tablaIGI = resultado.IGI;
tablaIVA = resultado.IVA;
```

**Después**:
```csharp
var resultado = await Task.Run(() => reporteService.ObtenerConciliacionIGI(baseDatos, fechaInicio, fechaFin));

// Guardar las tablas de detalle para uso posterior (doble clic)
detalleIGIActual = resultado.DetalleIGI;
detalleIVAActual = resultado.DetalleIVA;

// Mostrar solo los RESÚMENES en los grids
tablaIGI = resultado.ResumenIGI;
tablaIVA = resultado.ResumenIVA;
```

**Cambio clave**:
- Se cambió de `ObtenerResumenTablasPorBase()` (método viejo) a `ObtenerConciliacionIGI()` (nuevo método unificado).
- El nuevo método retorna un objeto `ResultadoConciliacion` con 4 tablas:
  - `DetalleIGI` (pedimentos a nivel partida con IGI)
  - `ResumenIGI` (agrupado por Año, Mes, FormaPago_IGI)
  - `DetalleIVA` (pedimentos a nivel partida con IVA)
  - `ResumenIVA` (agrupado por Año, Mes, FormaPago_IVA)
- Los **resúmenes** se muestran en pantalla; los **detalles** se guardan para abrirse en ventana modal al hacer doble clic.

---

### 3. **Modificación del Método `GenerarReporte()` - Múltiples Bases (SIN Glosa)**

**Antes**:
- Iteraba sobre todas las bases y llamaba `ObtenerResumenTablasPorBase()`.
- Agregaba manualmente las columnas `IGI_Pagado`, `IGI_Calculado`, `IVA_Pagado` sin calcular diferencia.
- No guardaba detalles.

**Después**:
- Itera sobre todas las bases y llama `ObtenerConciliacionIGI()` para cada base.
- Agrega columnas de resumen incluyendo `Diferencia_IGI`.
- Combina (`Merge`) todas las tablas de detalle en dos tablas acumuladas: `todosDetallesIGI` y `todosDetallesIVA`.
- Retorna una tupla con 4 elementos: `(resIGI, resIVA, todosDetallesIGI, todosDetallesIVA)`.

```csharp
// Agregador de detalles
var todosDetallesIGI = new System.Data.DataTable();
var todosDetallesIVA = new System.Data.DataTable();
bool primeraIteracion = true;

foreach (var baseDb in bases)
{
	var conciliacion = reporteService.ObtenerConciliacionIGI(baseDb, fechaInicio, fechaFin);

	// Agregar resúmenes (con Diferencia_IGI)
	...

	// Combinar detalles
	if (primeraIteracion)
	{
		todosDetallesIGI = conciliacion.DetalleIGI.Copy();
		todosDetallesIVA = conciliacion.DetalleIVA.Copy();
		primeraIteracion = false;
	}
	else
	{
		todosDetallesIGI.Merge(conciliacion.DetalleIGI);
		todosDetallesIVA.Merge(conciliacion.DetalleIVA);
	}
}
```

**Resultado**:
- Ahora en modo "Sin Glosa" también se tienen detalles completos y se calcula correctamente la diferencia IGI.

---

### 4. **Renombrado de Columnas en `FormatearGridIGI()` y `FormatearGridIVA()`**

Se agregó lógica para renombrar las columnas del nuevo formato SQL a los nombres esperados por el UI:

**Grid IGI**:
```csharp
if (dgvReporteIGI.Columns["Año"] != null)
	dgvReporteIGI.Columns["Año"].HeaderText = "AÑO";

if (dgvReporteIGI.Columns["Mes"] != null)
	dgvReporteIGI.Columns["Mes"].HeaderText = "MES";

if (dgvReporteIGI.Columns["IGI_Pagado"] != null)
	dgvReporteIGI.Columns["IGI_Pagado"].HeaderText = "IGI PAGADO";

if (dgvReporteIGI.Columns["IGI_Calculado"] != null)
	dgvReporteIGI.Columns["IGI_Calculado"].HeaderText = "IGI CALCULADO";

if (dgvReporteIGI.Columns["Diferencia_IGI"] != null)
	dgvReporteIGI.Columns["Diferencia_IGI"].HeaderText = "DIFERENCIA";

if (dgvReporteIGI.Columns["FormaPago_IGI"] != null)
	dgvReporteIGI.Columns["FormaPago_IGI"].HeaderText = "FORMA DE PAGO IGI";
```

**Grid IVA**:
```csharp
if (dgvReporteIVA.Columns["Año"] != null)
	dgvReporteIVA.Columns["Año"].HeaderText = "AÑO";

if (dgvReporteIVA.Columns["Mes"] != null)
	dgvReporteIVA.Columns["Mes"].HeaderText = "MES";

if (dgvReporteIVA.Columns["IVA_Pagado"] != null)
	dgvReporteIVA.Columns["IVA_Pagado"].HeaderText = "IVA PAGADO";

if (dgvReporteIVA.Columns["FormaPago_IVA"] != null)
	dgvReporteIVA.Columns["FormaPago_IVA"].HeaderText = "FORMA DE PAGO IVA";
```

**Propósito**: Mantener compatibilidad con el código existente de formateo (colores, formato moneda, etc.).

---

### 5. **Nuevo Formulario `FrmDetalleConciliacion`**

Se creó un nuevo formulario modal para mostrar los detalles filtrados por forma de pago.

**Archivos**:
- `Retorno360Tacna\FORMS\FrmDetalleConciliacion.cs`
- `Retorno360Tacna\FORMS\FrmDetalleConciliacion.Designer.cs`

**Características**:
- Recibe una `DataTable` de detalle (IGI o IVA), la forma de pago seleccionada, y el tipo de reporte ("IGI" o "IVA").
- Filtra la tabla por la columna `FormaPago_IGI` o `FormaPago_IVA`.
- Muestra un `DataGridView` con los pedimentos filtrados.
- Botón **Exportar** para guardar el detalle en Excel.
- Botón **Cerrar** para cerrar la ventana.

**Código relevante** (filtrado):
```csharp
string columnaFormaPago = tipoReporte == "IGI" ? "FormaPago_IGI" : "FormaPago_IVA";

var filasFiltradas = detalleOriginal.AsEnumerable()
	.Where(r => r[columnaFormaPago]?.ToString()?.Trim() == formaPago?.Trim());

if (filasFiltradas.Any())
{
	detalleFiltrado = filasFiltradas.CopyToDataTable();
}
else
{
	detalleFiltrado = detalleOriginal.Clone(); // Tabla vacía con misma estructura
}
```

---

### 6. **Modificación de Eventos de Doble Clic**

**Antes**:
- `DgvReporteIGI_CellDoubleClick()` y `DgvReporteIVA_CellDoubleClick()` llamaban a un método de servicio para obtener detalles desde la DB y abrían `FrmDetallePedimentos` (basado en lista `List<ReporteIGIPagado>`).

**Después**:
- Ambos eventos ahora usan las tablas de detalle ya cargadas en memoria (`detalleIGIActual`, `detalleIVAActual`).
- Filtran por **forma de pago** directamente desde la fila seleccionada.
- Abren el nuevo formulario `FrmDetalleConciliacion` pasando la tabla completa y la forma de pago.

**Código IGI**:
```csharp
private void DgvReporteIGI_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
{
	if (e.RowIndex < 0) return;
	if (modalAbierto) return;
	if (ultimaFilaClickeada == e.RowIndex) return;

	try
	{
		modalAbierto = true;
		ultimaFilaClickeada = e.RowIndex;

		DataGridViewRow row = dgvReporteIGI.Rows[e.RowIndex];
		string formaPago = row.Cells["FORMA DE PAGO IGI"]?.Value?.ToString() ?? "";

		if (string.IsNullOrEmpty(formaPago))
		{
			MessageBox.Show("No se pudo obtener la información de forma de pago.", ...);
			return;
		}

		if (detalleIGIActual == null || detalleIGIActual.Rows.Count == 0)
		{
			MessageBox.Show("No hay datos de detalle disponibles.", ...);
			return;
		}

		var frmDetalle = new FrmDetalleConciliacion(detalleIGIActual, formaPago, "IGI");
		frmDetalle.ShowDialog(this);
		frmDetalle.Dispose();
	}
	catch (Exception ex)
	{
		MessageBox.Show($"Error al mostrar el detalle: {ex.Message}", ...);
	}
	finally
	{
		Task.Delay(300).ContinueWith(_ => { ... });
	}
}
```

**Código IVA**: Idéntico, pero usando `detalleIVAActual` y `"IVA"`.

**Ventaja**:
- Ya no se hace una consulta adicional a la base de datos al hacer doble clic.
- Los detalles ya están en memoria, filtrados instantáneamente por forma de pago.
- Mejor rendimiento y experiencia de usuario.

---

## Flujo de Datos Completo

### Consulta con Validación de Glosa (una base específica)
1. Usuario selecciona **Razón Social** y **Cliente** (base de datos).
2. Hace clic en **Consultar**.
3. Se ejecuta `reporteService.ObtenerConciliacionIGI(baseDatos, fechaInicio, fechaFin)`.
4. El servicio ejecuta el query unificado SQL con tablas temporales.
5. Retorna `ResultadoConciliacion` con 4 tablas.
6. UI guarda `DetalleIGI` y `DetalleIVA` en campos privados.
7. UI muestra `ResumenIGI` y `ResumenIVA` en los grids.
8. Usuario hace **doble clic** en una fila del grid.
9. Se abre `FrmDetalleConciliacion` con la tabla de detalle filtrada por forma de pago.

### Consulta Sin Validación de Glosa (todas las bases de una razón social)
1. Usuario selecciona **Razón Social** y marca el checkbox **Sin Glosa**.
2. Hace clic en **Consultar**.
3. Se ejecuta un loop sobre todas las bases de esa razón social.
4. Para cada base, se llama `reporteService.ObtenerConciliacionIGI(baseDb, fechaInicio, fechaFin)`.
5. Los resúmenes se agregan en memoria (sumando IGI_Pagado, IGI_Calculado, Diferencia_IGI, IVA_Pagado).
6. Los detalles se combinan usando `DataTable.Merge()`.
7. UI muestra resúmenes agregados y guarda detalles combinados.
8. Usuario hace **doble clic** → mismo flujo de detalle.

---

## Ventajas de la Nueva Arquitectura

1. **Un solo query SQL** por consulta (en lugar de múltiples consultas separadas para resumen y detalle).
2. **Mejor rendimiento**: Tablas temporales indexadas en SQL Server.
3. **Diferencia IGI calculada directamente en SQL** con la regla especial de forma de pago `5`.
4. **Detalles ya cargados en memoria**: no se requiere consulta adicional al abrir detalle.
5. **Código más limpio**: Toda la lógica de conciliación está en el servicio, no en la UI.
6. **Fácil de exportar**: El detalle filtrado ya es una `DataTable`, lista para ClosedXML.

---

## Archivos Modificados

- `Retorno360Tacna\FORMS\FrmReportes.cs`
  - Campos `detalleIGIActual`, `detalleIVAActual`
  - Método `GenerarReporte()` (ambos casos: con y sin glosa)
  - Métodos `FormatearGridIGI()` y `FormatearGridIVA()`
  - Métodos `DgvReporteIGI_CellDoubleClick()` y `DgvReporteIVA_CellDoubleClick()`

## Archivos Nuevos

- `Retorno360Tacna\FORMS\FrmDetalleConciliacion.cs`
- `Retorno360Tacna\FORMS\FrmDetalleConciliacion.Designer.cs`

---

## Estado de Compilación

✅ **Compilación exitosa**

---

## Próximos Pasos Recomendados

1. **Pruebas de integración**:
   - Probar con diferentes razones sociales.
   - Probar con y sin validación de glosa.
   - Verificar que los detalles se filtren correctamente por forma de pago.
   - Probar exportación a Excel desde el detalle.

2. **Considerar deprecar métodos antiguos**:
   - `ObtenerResumenTablasPorBase()` ya no se usa en la UI.
   - `ObtenerDetallePorBase()` y `ObtenerDetallePorRazonSocial()` ya no se usan en los eventos de doble clic.
   - Se pueden marcar como obsoletos o eliminar si no se usan en otros lugares.

3. **Mejorar UX**:
   - Agregar indicador de progreso en el detalle si la tabla es muy grande.
   - Agregar botón de búsqueda/filtro adicional en `FrmDetalleConciliacion`.

---

## Conclusión

La integración del query unificado en la UI está completa. El sistema ahora muestra resúmenes en los grids principales y permite abrir detalles filtrados por forma de pago al hacer doble clic. Los detalles ya están en memoria, lo que mejora el rendimiento y la experiencia del usuario.
