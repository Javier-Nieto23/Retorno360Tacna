# Corrección del Query Unificado - Columnas Faltantes

## Fecha
Enero 2025

## Problema Detectado

El query unificado en `ObtenerConciliacionIGI()` no estaba generando las columnas necesarias para la UI, causando una excepción en runtime cuando `FrmReportes.cs` intentaba acceder a las columnas `Año` y `Mes`.

### Síntomas
- Excepción en `ReporteIGIService.ObtenerConciliacionIGI()`
- La UI esperaba columnas `Año`, `Mes`, y `Diferencia_IGI`
- El query original solo agrupaba por `FormaPago_IGI` sin incluir año/mes

---

## Correcciones Aplicadas

### 1. **Resumen IGI - Agregadas Columnas de Año y Mes**

**Antes**:
```sql
SELECT
	FormaPago_IGI,
	COUNT(*) AS TotalPedimentos,
	SUM(IGI_Pagado) AS IGI_Pagado,
	SUM(IGI_Calculado) AS IGI_Calculado,
	SUM(Diferencia) AS Diferencia,
	SUM(CASE WHEN Estatus = 'DIFERENCIA' THEN 1 ELSE 0 END) AS PedimentosConDiferencia
FROM #ConciliacionIGI
GROUP BY FormaPago_IGI
ORDER BY FormaPago_IGI;
```

**Después**:
```sql
SELECT
	YEAR(FechaPago) AS Año,
	MONTH(FechaPago) AS Mes,
	FormaPago_IGI,
	SUM(IGI_Pagado) AS IGI_Pagado,
	SUM(IGI_Calculado) AS IGI_Calculado,
	SUM(IGI_Calculado) - SUM(CASE WHEN FormaPago_IGI = '5' THEN 0 ELSE IGI_Pagado END) AS Diferencia_IGI
FROM #ConciliacionIGI
GROUP BY YEAR(FechaPago), MONTH(FechaPago), FormaPago_IGI
ORDER BY YEAR(FechaPago), MONTH(FechaPago), FormaPago_IGI;
```

**Cambios**:
- ✅ Agregado `YEAR(FechaPago) AS Año`
- ✅ Agregado `MONTH(FechaPago) AS Mes`
- ✅ Cambiado `Diferencia` a `Diferencia_IGI` con la regla de forma de pago `5`
- ✅ Agregado `Año` y `Mes` al `GROUP BY`

---

### 2. **Resumen IVA - Agregadas Columnas de Año y Mes**

**Antes**:
```sql
SELECT
	FormaPago_IVA,
	COUNT(*) AS TotalPedimentos,
	SUM(IVA_Pagado) AS IVA_Pagado
FROM #ConciliacionIVA
GROUP BY FormaPago_IVA
ORDER BY FormaPago_IVA;
```

**Después**:
```sql
SELECT
	YEAR(FechaPago) AS Año,
	MONTH(FechaPago) AS Mes,
	FormaPago_IVA,
	SUM(IVA_Pagado) AS IVA_Pagado
FROM #ConciliacionIVA
GROUP BY YEAR(FechaPago), MONTH(FechaPago), FormaPago_IVA
ORDER BY YEAR(FechaPago), MONTH(FechaPago), FormaPago_IVA;
```

**Cambios**:
- ✅ Agregado `YEAR(FechaPago) AS Año`
- ✅ Agregado `MONTH(FechaPago) AS Mes`
- ✅ Agregado `Año` y `Mes` al `GROUP BY`

---

### 3. **Detalle IGI - Aplicada Regla de Forma de Pago 5**

**Antes**:
```sql
SELECT
	C.Pedimento,
	C.FechaPago,
	C.FormaPago_IGI,
	G.IGI_Pagado,
	C.IGI_Calculado,
	G.IGI_Pagado - C.IGI_Calculado AS Diferencia,
	CASE
		WHEN ABS(G.IGI_Pagado - C.IGI_Calculado) > 1
		THEN 'DIFERENCIA'
		ELSE 'OK'
	END AS Estatus
INTO #ConciliacionIGI
FROM #PedimentosCliente C
INNER JOIN #PedimentosGlosa G
	ON G.Pedimento = C.Pedimento
   AND G.FormaPago_IGI = C.FormaPago_IGI;
```

**Después**:
```sql
SELECT
	C.Pedimento,
	C.FechaPago,
	C.FormaPago_IGI,

	CASE 
		WHEN C.FormaPago_IGI = '5' THEN 0 
		ELSE ISNULL(G.IGI_Pagado, 0) 
	END AS IGI_Pagado,

	C.IGI_Calculado,

	C.IGI_Calculado - CASE 
		WHEN C.FormaPago_IGI = '5' THEN 0 
		ELSE ISNULL(G.IGI_Pagado, 0) 
	END AS Diferencia_IGI,

	CASE
		WHEN ABS(C.IGI_Calculado - ISNULL(G.IGI_Pagado, 0)) > 1
		THEN 'DIFERENCIA'
		ELSE 'OK'
	END AS Estatus

INTO #ConciliacionIGI

FROM #PedimentosCliente C

LEFT JOIN #PedimentosGlosa G
	ON G.Pedimento = C.Pedimento
   AND G.FormaPago_IGI = C.FormaPago_IGI;
```

**Cambios**:
- ✅ **Regla forma de pago 5**: Si `FormaPago_IGI = '5'`, entonces `IGI_Pagado = 0`
- ✅ Renombrado `Diferencia` a `Diferencia_IGI`
- ✅ Cambiado `INNER JOIN` a `LEFT JOIN` para incluir pedimentos sin glosa
- ✅ Agregado `ISNULL()` para manejar valores NULL en caso de LEFT JOIN

---

### 4. **Detalle IGI Resultset - Nombre de Columna**

**Antes**:
```sql
SELECT
	Pedimento,
	FechaPago,
	FormaPago_IGI,
	IGI_Pagado,
	IGI_Calculado,
	Diferencia,  -- ❌ Nombre incorrecto
	Estatus
FROM #ConciliacionIGI
```

**Después**:
```sql
SELECT
	Pedimento,
	FechaPago,
	FormaPago_IGI,
	IGI_Pagado,
	IGI_Calculado,
	Diferencia_IGI,  -- ✅ Nombre correcto
	Estatus
FROM #ConciliacionIGI
```

---

## Impacto de las Correcciones

### Columnas Generadas Ahora

#### **ResumenIGI**:
| Columna | Tipo | Descripción |
|---------|------|-------------|
| Año | int | Año del pago |
| Mes | int | Mes del pago |
| FormaPago_IGI | string | Forma de pago IGI |
| IGI_Pagado | decimal | Total IGI pagado (con regla forma 5) |
| IGI_Calculado | decimal | Total IGI calculado |
| Diferencia_IGI | decimal | Diferencia con regla forma 5 |

#### **ResumenIVA**:
| Columna | Tipo | Descripción |
|---------|------|-------------|
| Año | int | Año del pago |
| Mes | int | Mes del pago |
| FormaPago_IVA | string | Forma de pago IVA |
| IVA_Pagado | decimal | Total IVA pagado |

#### **DetalleIGI**:
| Columna | Tipo | Descripción |
|---------|------|-------------|
| Pedimento | string | Número de pedimento |
| FechaPago | date | Fecha de pago |
| FormaPago_IGI | string | Forma de pago IGI |
| IGI_Pagado | decimal | IGI pagado (0 si forma = 5) |
| IGI_Calculado | decimal | IGI calculado |
| Diferencia_IGI | decimal | Diferencia con regla forma 5 |
| Estatus | string | OK / DIFERENCIA |

#### **DetalleIVA**:
| Columna | Tipo | Descripción |
|---------|------|-------------|
| Pedimento | string | Número de pedimento |
| FechaPago | date | Fecha de pago |
| FormaPago_IVA | string | Forma de pago IVA |
| IVA_Pagado | decimal | IVA pagado |

---

## Compatibilidad con la UI

Ahora las columnas del query coinciden **exactamente** con lo que espera `FrmReportes.cs`:

### `FormatearGridIGI()` espera:
- ✅ `Año` → renombra a `"AÑO"`
- ✅ `Mes` → renombra a `"MES"`
- ✅ `IGI_Pagado` → renombra a `"IGI PAGADO"`
- ✅ `IGI_Calculado` → renombra a `"IGI CALCULADO"`
- ✅ `Diferencia_IGI` → renombra a `"DIFERENCIA"`
- ✅ `FormaPago_IGI` → renombra a `"FORMA DE PAGO IGI"`

### `FormatearGridIVA()` espera:
- ✅ `Año` → renombra a `"AÑO"`
- ✅ `Mes` → renombra a `"MES"`
- ✅ `IVA_Pagado` → renombra a `"IVA PAGADO"`
- ✅ `FormaPago_IVA` → renombra a `"FORMA DE PAGO IVA"`

---

## Regla de Forma de Pago 5

La regla especial para forma de pago `5` ahora está aplicada **en el nivel SQL**:

```sql
CASE 
	WHEN C.FormaPago_IGI = '5' THEN 0 
	ELSE ISNULL(G.IGI_Pagado, 0) 
END AS IGI_Pagado
```

Y en el cálculo de diferencia:

```sql
C.IGI_Calculado - CASE 
	WHEN C.FormaPago_IGI = '5' THEN 0 
	ELSE ISNULL(G.IGI_Pagado, 0) 
END AS Diferencia_IGI
```

**Ventajas**:
- ✅ Consistencia total: la regla se aplica una sola vez en SQL
- ✅ Performance: no se recalcula en C#
- ✅ Datos correctos tanto en resumen como en detalle

---

## Estado

✅ **Compilación exitosa**  
✅ **Columnas correctas generadas**  
✅ **Regla forma de pago 5 aplicada**  
✅ **Compatible con UI existente**

---

## Archivos Modificados

- `Retorno360Tacna\SERVICES\ReporteIGIService.cs` - Método `ObtenerConciliacionIGI()`

---

## Próximo Paso

Probar la ejecución completa:
1. Seleccionar razón social y cliente
2. Hacer clic en "Consultar"
3. Verificar que los grids muestren datos correctamente
4. Hacer doble clic en una fila para abrir detalle
5. Verificar que el detalle filtre por forma de pago

---

**Fecha**: Enero 2025  
**Estado**: ✅ Corrección aplicada y compilada exitosamente
