# Funcionalidad: Detalle de Pedimentos en FrmReportes

## 📋 Descripción

Se ha implementado una funcionalidad que permite visualizar el detalle de cada pedimento al hacer **doble clic** en una fila del reporte de IGI o IVA en el formulario `FrmReportes`.

## ✨ Características

### Vista de Detalle
Cuando el usuario hace doble clic en una fila del reporte (ya sea IGI o IVA), se abre una ventana modal que muestra:

#### Para Reporte IGI:
- **Fecha** del pedimento
- **Número de Pedimento**
- **IGI Pagado**
- **IGI Calculado**
- **Diferencia** (IGI Pagado - IGI Calculado)
- **Forma de Pago IGI**

#### Para Reporte IVA:
- **Fecha** del pedimento
- **Número de Pedimento**
- **IVA Pagado**
- **Forma de Pago IVA**

### Ejemplo de Uso

1. Usuario genera un reporte de IGI/IVA para un período
2. El reporte muestra datos agrupados por mes y forma de pago
3. Usuario hace **doble clic** en la fila "enero 2026 - Forma de Pago 0"
4. Se abre una ventana con el detalle de **todos los pedimentos** de enero 2026 con forma de pago 0

## 🎨 Interfaz de Usuario

### FrmDetallePedimentos

Ventana modal con las siguientes características:

- **Header azul** con título descriptivo
- **Botón de cerrar** (✖) en la esquina superior derecha
- **Grid de detalle** con todos los pedimentos filtrados
- **Resumen** mostrando la cantidad total de pedimentos
- **Formato de moneda** en las columnas de importes
- **Colores en diferencias**:
  - Verde para diferencias positivas
  - Rojo para diferencias negativas
- **Filas alternadas** con color de fondo para mejor legibilidad
- **Arrastrable** haciendo clic en el header

### Diseño Visual

```
┌────────────────────────────────────────────────────┐
│  Detalle de Pedimentos - IGI | enero 2026 | FP-0 ✖│
├────────────────────────────────────────────────────┤
│  Total Pedimentos: 15                              │
├────────────────────────────────────────────────────┤
│ FECHA      │ PEDIMENTO   │ IGI PAGADO │ IGI CALC │
├────────────┼─────────────┼────────────┼──────────┤
│ 01/14/2026 │ 11158000000 │ $11158.00  │ $0.00    │
│ 01/15/2026 │ 00000000000 │ $21.00     │ $0.00    │
│ 01/15/2026 │ 00000000000 │ $0.00      │ $0.00    │
│ ...        │ ...         │ ...        │ ...      │
└────────────────────────────────────────────────────┘
```

## 🔧 Implementación Técnica

### Archivos Creados

1. **`FrmDetallePedimentos.cs`**
   - Lógica del formulario de detalle
   - Filtrado de pedimentos por mes y forma de pago
   - Formateo de columnas y colores

2. **`FrmDetallePedimentos.Designer.cs`**
   - Diseño visual del formulario
   - Controles: DataGridView, Panel, Labels, Button

### Archivos Modificados

1. **`FrmReportes.cs`**
   - Agregados eventos `CellDoubleClick` para ambos grids
   - Métodos `DgvReporteIGI_CellDoubleClick` y `DgvReporteIVA_CellDoubleClick`
   - Configuración de cursor como "Hand" en los grids

### Flujo de Datos

```
Usuario hace doble clic en fila
        ↓
Obtener MES y FORMA DE PAGO de la fila seleccionada
        ↓
Filtrar reporteActual (List<ReporteIGIPagado>)
        ↓
Crear FrmDetallePedimentos con pedimentos filtrados
        ↓
Mostrar ventana modal con detalle
```

### Código Clave

#### Evento de Doble Clic (IGI)
```csharp
private void DgvReporteIGI_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
{
    if (e.RowIndex < 0) return;

    DataGridViewRow row = dgvReporteIGI.Rows[e.RowIndex];
    string mes = row.Cells["MES"]?.Value?.ToString() ?? "";
    string formaPago = row.Cells["FORMA DE PAGO IGI"]?.Value?.ToString() ?? "";

    using (var frmDetalle = new FrmDetallePedimentos(reporteActual, mes, formaPago, "IGI"))
    {
        frmDetalle.ShowDialog(this);
    }
}
```

#### Filtrado de Pedimentos
```csharp
var pedimentosFiltrados = pedimentos
    .Where(p => p.FechaPago.HasValue &&
                p.FechaPago.Value.ToString("MMMM yyyy") == mesSeleccionado &&
                p.FormaPago_IGI == formaPago)
    .OrderBy(p => p.FechaPago)
    .ThenBy(p => p.Pedimento);
```

## 📊 Datos Mostrados

### Ejemplo de Datos de Entrada (Agrupados)

| MES          | IGI PAGADO | IGI CALCULADO | DIFERENCIA | FORMA DE PAGO IGI |
|--------------|------------|---------------|------------|-------------------|
| enero 2026   | $24,500.00 | $12,300.00    | $12,200.00 | 0                 |
| enero 2026   | $45,000.00 | $40,000.00    | $5,000.00  | 5                 |

### Ejemplo de Datos de Detalle (al hacer doble clic en la primera fila)

| FECHA      | PEDIMENTO    | IGI PAGADO | IGI CALCULADO | DIFERENCIA | FORMA DE PAGO |
|------------|--------------|------------|---------------|------------|---------------|
| 01/14/2026 | 11158.000000 | $11,158.00 | $0.00         | $11,158.00 | 0             |
| 01/15/2026 | 00000.000000 | $21.00     | $0.00         | $21.00     | 0             |
| 01/15/2026 | 00000.000000 | $0.00      | $0.00         | $0.00      | 0             |
| 01/15/2026 | 00000.000000 | $134.00    | $0.00         | $134.00    | 0             |
| ...        | ...          | ...        | ...           | ...        | ...           |

## 🎯 Beneficios

### Para el Usuario
- ✅ Acceso rápido al detalle de pedimentos
- ✅ Validación de datos agrupados
- ✅ Identificación de pedimentos con diferencias
- ✅ Exportación visual clara y organizada
- ✅ No requiere navegación adicional

### Para el Sistema
- ✅ Reutilización de datos ya cargados (`reporteActual`)
- ✅ No consulta adicional a la base de datos
- ✅ Ventana modal ligera y rápida
- ✅ Código modular y reutilizable

## 🔍 Validaciones

- ✅ Verifica que `e.RowIndex >= 0` (no es header)
- ✅ Valida que MES y FORMA DE PAGO no sean nulos/vacíos
- ✅ Manejo de excepciones con mensajes de error claros
- ✅ Filtrado seguro con LINQ

## 🚀 Mejoras Futuras Posibles

1. **Exportar a Excel**
   - Botón para exportar el detalle de pedimentos a Excel

2. **Búsqueda en el Detalle**
   - TextBox para filtrar pedimentos por número

3. **Ordenamiento**
   - Permitir ordenar por cualquier columna

4. **Estadísticas Adicionales**
   - Totales, promedios, máximo/mínimo

5. **Resaltado de Anomalías**
   - Destacar pedimentos con diferencias significativas

6. **Clic Simple vs Doble Clic**
   - Configuración de usuario para preferencia de interacción

## 📚 Referencias

- Modelo: `ReporteIGIPagado` en `MODELS/ReporteIGI.cs`
- Servicio: `ReporteIGIService` y `ReporteIGIService_Extension`
- Formulario Principal: `FrmReportes.cs`
- Formulario de Detalle: `FrmDetallePedimentos.cs`

## ✅ Checklist de Implementación

- [x] Crear `FrmDetallePedimentos.cs` y `.Designer.cs`
- [x] Agregar eventos `CellDoubleClick` en `FrmReportes`
- [x] Implementar filtrado de pedimentos por mes y forma de pago
- [x] Formatear columnas con estilos y colores
- [x] Configurar ventana modal arrastrable
- [x] Agregar botón de cerrar funcional
- [x] Mostrar resumen con total de pedimentos
- [x] Colorear diferencias (verde/rojo)
- [x] Build exitoso sin errores
- [x] Documentación completa

---

**Versión**: v2.5.1  
**Fecha de Implementación**: Enero 2025  
**Desarrollador**: Equipo Retorno360 Tacna  
**Estado**: ✅ Implementado y Funcional
