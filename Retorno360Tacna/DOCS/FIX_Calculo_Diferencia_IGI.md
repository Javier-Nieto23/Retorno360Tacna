# Fix: Cálculo de Diferencia IGI con Forma de Pago 5

## Cambio Implementado

Se agregó el cálculo de la diferencia entre IGI Calculado e IGI Pagado con lógica especial para la forma de pago '5', alineando el comportamiento con la lógica existente en `ReporteIGIService_Extension.cs`.

## Lógica de Negocio

### Fórmula General
```
Diferencia_IGI = IGI_Calculado - IGI_Pagado
```

### Regla Especial: Forma de Pago '5'
Cuando `FormaPago_IGI = '5'`:
- El `IGI_Pagado` se ajusta a **0** tanto para cálculo como para visualización
- La diferencia se calcula como `Diferencia_IGI = 0 - IGI_Calculado`

### Pseudocódigo
```csharp
if (FormaPago_IGI == "5")
{
	IGI_Pagado = 0;
	Diferencia_IGI = 0 - IGI_Calculado;
}
else
{
	Diferencia_IGI = IGI_Calculado - IGI_Pagado;
}
```

## Ejemplos

### Ejemplo 1: Forma de Pago Normal (0, 21, etc.)
```
IGI_Pagado:     $1,500.00
IGI_Calculado:  $1,450.00
FormaPago_IGI:  "0"

→ Diferencia_IGI = $1,450.00 - $1,500.00 = -$50.00
```

### Ejemplo 2: Forma de Pago '5' (Exento/Sin pago)
```
IGI_Pagado:     $1,500.00 (valor original en glosa)
IGI_Calculado:  $1,450.00
FormaPago_IGI:  "5"

→ IGI_Pagado = $0.00 (ajustado)
→ Diferencia_IGI = $0.00 - $1,450.00 = -$1,450.00
```

### Ejemplo 3: Sobrepago
```
IGI_Pagado:     $1,400.00
IGI_Calculado:  $1,500.00
FormaPago_IGI:  "0"

→ Diferencia_IGI = $1,500.00 - $1,400.00 = $100.00 (positivo = faltante)
```

### Ejemplo 4: Pago exacto
```
IGI_Pagado:     $1,450.00
IGI_Calculado:  $1,450.00
FormaPago_IGI:  "21"

→ Diferencia_IGI = $1,450.00 - $1,450.00 = $0.00
```

## Interpretación de la Diferencia

- **Diferencia POSITIVA** (ej: +$100): El IGI calculado es MAYOR que el pagado → **Falta pagar**
- **Diferencia NEGATIVA** (ej: -$50): El IGI pagado es MAYOR que el calculado → **Sobrepago**
- **Diferencia CERO**: El pago coincide exactamente con lo calculado → **Correcto**

## Estructura de la Tabla IGI

**Columnas actualizadas:**
```
Año              (int)
Mes              (int)
IGI_Pagado       (decimal)    ← Valor real pagado (sin ajustar)
IGI_Calculado    (decimal)    ← Valor calculado del sistema
Diferencia_IGI   (decimal)    ← NUEVA COLUMNA: diferencia con ajuste por forma de pago
FormaPago_IGI    (string)     ← '0', '5', '21', etc.
```

## Código Modificado

### 1. Definición de Columnas (Línea ~75-81)
```csharp
var tablaIGI = new System.Data.DataTable();
tablaIGI.Columns.Add("Año", typeof(int));
tablaIGI.Columns.Add("Mes", typeof(int));
tablaIGI.Columns.Add("IGI_Pagado", typeof(decimal));
tablaIGI.Columns.Add("IGI_Calculado", typeof(decimal));
tablaIGI.Columns.Add("Diferencia_IGI", typeof(decimal));  // ← NUEVA
tablaIGI.Columns.Add("FormaPago_IGI", typeof(string));
```

### 2. Cálculo de Diferencia (Línea ~378-396)
```csharp
foreach (var row in listaJoinIGI.OrderBy(r => r.Año).ThenBy(r => r.Mes).ThenBy(r => r.Forma))
{
	decimal igiPagado = row.IGI_Pagado;
	decimal igiCalculado = row.IGI_Calculado;
	decimal diferencia;

	// Si la forma de pago es '5', el IGI_Pagado se considera 0 para el cálculo de diferencia
	if (row.Forma == "5")
	{
		diferencia = 0m - igiCalculado;  // Diferencia = 0 - IGI_Calculado
		igiPagado = 0m;  // Mostrar 0 en la columna IGI_Pagado
	}
	else
	{
		diferencia = igiCalculado - igiPagado;  // Diferencia = IGI_Calculado - IGI_Pagado
	}

	tablaIGI.Rows.Add(row.Año, row.Mes, igiPagado, igiCalculado, diferencia, row.Forma);
}
```

## Consistencia con ReporteIGIService_Extension.cs

Esta implementación es **consistente** con la lógica ya existente en `ReporteIGIService_Extension.cs` (líneas 96-106):

```csharp
if (!string.IsNullOrWhiteSpace(grp.FormaPago) && grp.FormaPago.Trim() == "5")
{
	diferencia = 0m - igiCalculado;
	igiPagado = 0m;
}
else
{
	diferencia = igiCalculado - igiPagado;
}
```

Ahora ambos métodos (`ObtenerResumenTablasPorBase` y `ConvertirADataTableIGI`) aplican la **misma fórmula**.

## Razón del Cambio

La forma de pago '5' representa un caso especial (generalmente exención o suspensión de pago) donde aunque el sistema haya registrado un monto en `IGI_Pagado`, **fiscalmente no se considera como pago efectivo**. Por lo tanto, para el cálculo de diferencias y auditoría, se debe tratar como si el pago fuera $0.

## Impacto en la UI

La interfaz `FrmReportes.cs` ahora mostrará una columna adicional `Diferencia_IGI` en el `DataGridView` que permite visualizar:

- ✅ **Diferencias positivas**: Cuando se pagó más de lo calculado
- ⚠️ **Diferencias negativas**: Cuando se pagó menos (o cero en caso de forma de pago 5)
- ✔️ **Diferencias cero**: Cuando el pago coincide exactamente con lo calculado

## Verificación

Para verificar el cálculo:

1. Consultar un pedimento con `FormaPago_IGI = '5'`
2. Revisar que:
   - `IGI_Pagado` muestra el valor original
   - `Diferencia_IGI` = 0 - `IGI_Calculado` (negativo)

3. Consultar un pedimento con `FormaPago_IGI != '5'`
4. Revisar que:
   - `Diferencia_IGI` = `IGI_Pagado` - `IGI_Calculado`

## Archivos Modificados

- `Retorno360Tacna\SERVICES\ReporteIGIService.cs`
  - Línea ~80: Agregada columna `Diferencia_IGI`
  - Línea ~378-385: Lógica de cálculo con ajuste por forma de pago

## Compilación

✅ Build exitoso sin errores

## Siguiente Paso

Actualizar `FrmReportes.cs` para:
1. Configurar el formato de la columna `Diferencia_IGI` (moneda con 2 decimales)
2. Opcional: Aplicar formato condicional (color rojo para negativo, verde para positivo)
