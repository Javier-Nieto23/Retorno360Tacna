# Implementación: Query Unificado de Conciliación IGI/IVA

## Descripción

Se implementó un nuevo método `ObtenerConciliacionIGI()` que ejecuta un query unificado de conciliación y devuelve 4 DataTables correspondientes a:

1. **DetalleIGI**: Conciliación detallada por pedimento (IGI)
2. **ResumenIGI**: Resumen agrupado por forma de pago (IGI)
3. **DetalleIVA**: Conciliación detallada por pedimento (IVA)
4. **ResumenIVA**: Resumen agrupado por forma de pago (IVA)

## Estructura de Datos

### 1. DetalleIGI (#ConciliacionIGI)
```
Columnas:
- Pedimento (string)
- FechaPago (DateTime)
- FormaPago_IGI (string)
- IGI_Pagado (decimal)
- IGI_Calculado (decimal)
- Diferencia (decimal) = IGI_Pagado - IGI_Calculado
- Estatus (string) = 'DIFERENCIA' si ABS(diferencia) > 1, sino 'OK'
```

### 2. ResumenIGI (Agrupación)
```
Columnas:
- FormaPago_IGI (string)
- TotalPedimentos (int)
- IGI_Pagado (decimal) = SUM
- IGI_Calculado (decimal) = SUM
- Diferencia (decimal) = SUM
- PedimentosConDiferencia (int)
```

### 3. DetalleIVA (#ConciliacionIVA)
```
Columnas:
- Pedimento (string)
- FechaPago (DateTime)
- FormaPago_IVA (string)
- IVA_Pagado (decimal)
```

### 4. ResumenIVA (Agrupación)
```
Columnas:
- FormaPago_IVA (string)
- TotalPedimentos (int)
- IVA_Pagado (decimal) = SUM
```

## Flujo del Query

### 1. Tablas Temporales

#### #PedimentosCliente
- Extrae pedimentos del cliente con IGI_Calculado
- Join: `Di_Pedimento` → `Di_PedimentoDet` → `Ca_Farancelaria`
- Fórmula: `ROUND((Pid_ValorAdu * Fra_AdvGral) / 100.0, 0)`

#### #PedimentosGlosa
- Extrae pedimentos de TR_GLOSA con IGI_Pagado
- Filtros:
  - `Gl_TOper = 1`
  - `Gl_OrigenZipGlosa = 'S'`
  - `Gl_FPagoAdvalorem IN ('0','5')`

#### #ConciliacionIGI
- JOIN de #PedimentosCliente con #PedimentosGlosa
- Condiciones: `Pedimento` y `FormaPago_IGI`
- Calcula diferencia y estatus

#### #PedimentosGlosaIVA
- Extrae IVA de TR_GLOSA
- Filtros:
  - `Gl_TOper = 1`
  - `Gl_OrigenZipGlosa = 'S'`
  - `Gl_FPagoIVA IN ('0','21')`
  - `HAVING SUM(Gl_ImporteIVA) > 0`

#### #ConciliacionIVA
- Pedimentos de glosa IVA que existen en cliente
- JOIN con `SELECT DISTINCT Pedimento FROM #PedimentosCliente`

### 2. Resultsets Devueltos (en orden)

1. **SELECT * FROM #ConciliacionIGI** → DetalleIGI
2. **SELECT FormaPago_IGI, ... GROUP BY** → ResumenIGI
3. **SELECT * FROM #ConciliacionIVA** → DetalleIVA
4. **SELECT FormaPago_IVA, ... GROUP BY** → ResumenIVA

## Manejo de Conexiones

### Conexión Cliente
```csharp
var conexionCliente = ObtenerConexionParaBaseDatos(baseDatos);
var infoCliente = ObtenerConexionExterna(baseDatos);
```

### Conexión Glosa
```csharp
int idRazon = ObtenerIdRazonDesdeBaseDatos(baseDatos);
var razonSocial = ObtenerRazonSocial(idRazon);
string baseGlosa = razonSocial.BaseDatosOrigen; // Campo DB de RAZONXTABLA
```

### Construcción de Nombres de Base

Si la conexión es externa:
```csharp
string bdClienteCompleto = $"[{servidor}].{QuoteName(baseDatos)}";
```

Si es local:
```csharp
string bdClienteCompleto = QuoteName(baseDatos);
```

## Ejemplo de Uso

```csharp
var servicio = new ReporteIGIService(conexionInfo);
var resultado = servicio.ObtenerConciliacionIGI("SEERT_Able", fechaInicio, fechaFin);

// Grid principal IGI: mostrar ResumenIGI
dgvReporteIGI.DataSource = resultado.ResumenIGI;

// Grid principal IVA: mostrar ResumenIVA
dgvReporteIVA.DataSource = resultado.ResumenIVA;

// Al hacer clic en una fila del ResumenIGI:
string formaPago = row["FormaPago_IGI"].ToString();
var detalleFiltrado = resultado.DetalleIGI.AsEnumerable()
	.Where(r => r.Field<string>("FormaPago_IGI") == formaPago)
	.CopyToDataTable();

// Abrir ventana de detalle con detalleFiltrado
```

## Ventajas del Nuevo Enfoque

✅ **Query único**: Una sola llamada a la base de datos  
✅ **Consistencia**: Mismos criterios para cliente y glosa  
✅ **Performance**: Tablas temporales en servidor, menos tráfico de red  
✅ **Detalle disponible**: Sin necesidad de re-consultar para ver el detalle  
✅ **Conciliación automática**: JOIN directo entre cliente y glosa  

## Próximos Pasos

1. Actualizar `ObtenerResumenTablasPorBase()` para usar este método
2. Modificar `FrmReportes.cs` para:
   - Mostrar ResumenIGI y ResumenIVA en los grids
   - Implementar evento de doble clic para abrir detalle filtrado
3. Crear o adaptar ventana de detalle de pedimentos para mostrar DetalleIGI/DetalleIVA filtrado

## Archivos Modificados

- `Retorno360Tacna\SERVICES\ReporteIGIService.cs`
  - Nueva clase: `ResultadoConciliacion`
  - Nuevo método: `ObtenerConciliacionIGI()`

## Notas Técnicas

- **Timeout**: 300 segundos (5 minutos) para permitir ejecución de queries complejos
- **Tablas temporales**: Se limpian al final del script con `DROP TABLE IF EXISTS`
- **Encoding**: El query usa `N''` para strings Unicode
- **Diferencia**: Fórmula `IGI_Pagado - IGI_Calculado` (positivo = sobrepago, negativo = falta)
- **Estatus**: Tolerancia de ±$1 para considerar "OK"
