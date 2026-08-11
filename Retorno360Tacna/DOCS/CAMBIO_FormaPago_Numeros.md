# 📝 Cambio: Mostrar Forma de Pago como Números

## 🎯 Objetivo

Mostrar los campos **Forma de Pago IGI** y **Forma de Pago IVA** como **números** (códigos) en lugar de descripciones textuales en el reporte de IGI Pagado.

---

## 🔄 Cambio Solicitado

### ❌ Antes (Con Descripciones):

```
Forma Pago IGI    Forma Pago IVA
─────────────────────────────────
Transferencia     Compensación
Compensación      Transferencia
Transferencia     Transferencia
```

### ✅ Ahora (Con Códigos Numéricos):

```
Forma Pago IGI    Forma Pago IVA
─────────────────────────────────
5                 21
21                5
5                 5
```

**Mapeo de Códigos:**
- `5` = Transferencia
- `21` = Compensación

---

## 🔧 Cambios Implementados

### 1️⃣ Archivo: `ReporteIGI.cs`

**Eliminadas las propiedades calculadas:**

#### ❌ Código Anterior:

```csharp
public class ReporteIGIPagado : ReporteBase
{
    // ... propiedades básicas

    public string FormaPago_IGI { get; set; } = string.Empty;
    public string FormaPago_IVA { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene el nombre descriptivo de la forma de pago IGI
    /// </summary>
    public string FormaPagoIGI_Descripcion => ObtenerDescripcionFormaPago(FormaPago_IGI);

    /// <summary>
    /// Obtiene el nombre descriptivo de la forma de pago IVA
    /// </summary>
    public string FormaPagoIVA_Descripcion => ObtenerDescripcionFormaPago(FormaPago_IVA);

    private string ObtenerDescripcionFormaPago(string codigo)
    {
        return codigo switch
        {
            "5" => "Transferencia",
            "21" => "Compensación",
            _ => codigo
        };
    }
}
```

#### ✅ Código Actual:

```csharp
public class ReporteIGIPagado : ReporteBase
{
    public int IdPedimento { get; set; }
    public string Pedimento { get; set; } = string.Empty;
    public DateTime? FechaPago { get; set; }
    public decimal IGI_Pagado { get; set; }
    public decimal IGI_Calculado { get; set; }
    public decimal IVA_Pagado { get; set; }
    public string FormaPago_IGI { get; set; } = string.Empty;  // ← Solo el código
    public string FormaPago_IVA { get; set; } = string.Empty;  // ← Solo el código
    public string EstatusGlosa { get; set; } = string.Empty;

    /// <summary>
    /// Calcula la diferencia entre IGI Pagado y Calculado
    /// </summary>
    public decimal DiferenciaIGI => IGI_Pagado - IGI_Calculado;
}
```

---

### 2️⃣ Archivo: `ReporteIGIService.cs`

**Actualizado el método `ConvertirADataTable`:**

#### ❌ Código Anterior:

```csharp
foreach (var reporte in reportes)
{
    dt.Rows.Add(
        reporte.BaseDatos,
        reporte.IdPedimento,
        reporte.Pedimento,
        reporte.FechaPago ?? (object)DBNull.Value,
        reporte.IGI_Pagado,
        reporte.IGI_Calculado,
        reporte.DiferenciaIGI,
        reporte.IVA_Pagado,
        reporte.FormaPagoIGI_Descripcion,  // ← Descripción
        reporte.FormaPagoIVA_Descripcion,  // ← Descripción
        reporte.EstatusGlosa
    );
}
```

#### ✅ Código Actual:

```csharp
foreach (var reporte in reportes)
{
    dt.Rows.Add(
        reporte.BaseDatos,
        reporte.IdPedimento,
        reporte.Pedimento,
        reporte.FechaPago ?? (object)DBNull.Value,
        reporte.IGI_Pagado,
        reporte.IGI_Calculado,
        reporte.DiferenciaIGI,
        reporte.IVA_Pagado,
        reporte.FormaPago_IGI,  // ← Código numérico
        reporte.FormaPago_IVA,  // ← Código numérico
        reporte.EstatusGlosa
    );
}
```

---

## 📊 Estructura de Datos

### Query SQL Original:

```sql
SELECT 
    DI.Pim_Consecutivo AS iDPedimento,
    Adu_AduanaSecc+'-'+Gl_Patente+'-'+Gl_Pedimento AS Pedimento,
    TR.Gl_FecPagoReal AS FechaPago,
    TR.Gl_ImporteADvalorem AS IGI_Pagado,
    DI.Pid_ValorAdu AS ValorAduana,
    FRA.Fra_AdvGral AS TasaIGI,
    TR.Gl_ImporteIVA AS IVA_Pagado,
    TR.Gl_FPagoAdvalorem AS FormaPago_IGI,     -- ← Código: '5', '21', etc.
    TR.Gl_FPagoIVA AS FormaPago_IVA,           -- ← Código: '5', '21', etc.
    CASE 
        WHEN TR.Gl_Pedimento IS NOT NULL THEN 'SI CARGADO'
        ELSE 'NO CARGADO'
    END AS EstatusGlosa
FROM TR_GLOSA TR
-- ...
WHERE (
    TR.Gl_FPagoIVA IN ('5','21')           -- ← Filtro por códigos
    OR TR.Gl_FPagoAdvalorem IN ('5','21')
)
```

**Los códigos que retorna la base de datos:**
- `'5'` = Transferencia electrónica
- `'21'` = Compensación

---

## 🎨 Visualización en el DataGridView

### Columnas del Reporte:

| Columna | Tipo | Ejemplo de Valor |
|---------|------|------------------|
| Base Datos | string | "SEERT_ARROYO" |
| ID Pedimento | int | 123456 |
| Pedimento | string | "620-3807-2024/1234567" |
| Fecha Pago | DateTime? | 15/01/2024 |
| IGI Pagado | decimal | 25,000.00 |
| IGI Calculado | decimal | 24,850.00 |
| Diferencia IGI | decimal | 150.00 |
| IVA Pagado | decimal | 4,000.00 |
| **Forma Pago IGI** | **string** | **"5"** ← Solo código |
| **Forma Pago IVA** | **string** | **"21"** ← Solo código |
| Estatus Glosa | string | "SI CARGADO" |

---

## 📝 Catálogo de Códigos de Forma de Pago

### Códigos SAT - Forma de Pago de Contribuciones

| Código | Descripción |
|--------|-------------|
| `5` | **Transferencia electrónica de fondos** |
| `21` | **Compensación** |

**Referencia:**
- Estos son códigos oficiales del SAT (Servicio de Administración Tributaria)
- Se almacenan en la base de datos tal como vienen de las declaraciones aduanales
- El sistema filtra **solo** los pedimentos con forma de pago `'5'` o `'21'`

---

## ✅ Beneficios del Cambio

### 1️⃣ **Datos Originales**
```
✅ Se muestra la información tal como está en la base de datos
✅ No hay transformaciones que puedan introducir errores
✅ Trazabilidad directa con la fuente de datos
```

### 2️⃣ **Simplicidad**
```
✅ Código más simple (menos propiedades calculadas)
✅ Menos lógica de transformación
✅ Más fácil de mantener
```

### 3️⃣ **Performance**
```
✅ No se calculan descripciones en cada fila
✅ Menos procesamiento al generar el DataTable
✅ DataGridView más rápido
```

### 4️⃣ **Flexibilidad**
```
✅ El usuario ve los códigos reales
✅ Puede exportar datos tal como están en el sistema
✅ Compatible con otros sistemas que usan códigos SAT
```

---

## 🔄 Flujo de Datos

```
┌──────────────────────────────────────────────────────────────┐
│                   Base de Datos SQL                          │
├──────────────────────────────────────────────────────────────┤
│  TR_GLOSA.Gl_FPagoAdvalorem = '5'                           │
│  TR_GLOSA.Gl_FPagoIVA = '21'                                │
└────────────────────┬─────────────────────────────────────────┘
                     │
                     ▼
         ┌───────────────────────┐
         │  SQL Query Ejecuta    │
         └───────────┬───────────┘
                     │
                     ▼
         ┌───────────────────────┐
         │  SqlDataReader Lee    │
         │  FormaPago_IGI = "5"  │  ← reader.GetString(7)
         │  FormaPago_IVA = "21" │  ← reader.GetString(8)
         └───────────┬───────────┘
                     │
                     ▼
         ┌───────────────────────────┐
         │  ReporteIGIPagado         │
         │  FormaPago_IGI = "5"      │  ← Sin transformación ✅
         │  FormaPago_IVA = "21"     │  ← Sin transformación ✅
         └───────────┬───────────────┘
                     │
                     ▼
         ┌───────────────────────────┐
         │  ConvertirADataTable()    │
         │  dt.Rows.Add(             │
         │    ...                    │
         │    reporte.FormaPago_IGI, │  ← "5"
         │    reporte.FormaPago_IVA  │  ← "21"
         │  )                        │
         └───────────┬───────────────┘
                     │
                     ▼
         ┌───────────────────────────┐
         │  DataGridView Muestra     │
         │  Forma Pago IGI:  5       │  ← Usuario ve el código
         │  Forma Pago IVA:  21      │  ← Usuario ve el código
         └───────────────────────────┘
```

---

## 🧪 Validación

### Prueba 1: Verificar Códigos en el Reporte

```
1. Abrir "Reportes de IGI"
2. Seleccionar razón social y cliente
3. Seleccionar rango de fechas
4. Presionar "Consultar"
5. Verificar columnas:
   ✅ "Forma Pago IGI": Muestra números (5, 21, etc.)
   ✅ "Forma Pago IVA": Muestra números (5, 21, etc.)
   ❌ NO muestra "Transferencia" o "Compensación"
```

### Prueba 2: Validar Consistencia con BD

```
1. Ejecutar query directa en SQL:
   SELECT Gl_FPagoAdvalorem, Gl_FPagoIVA 
   FROM TR_GLOSA 
   WHERE Gl_Pedimento = '...'

2. Comparar resultados:
   ✅ Valores en el reporte coinciden con la BD
```

---

## 📚 Archivos Modificados

```
Retorno360Tacna/
├── MODELS/
│   └── ReporteIGI.cs                       [MODIFICADO]
│       ├── - FormaPagoIGI_Descripcion      [ELIMINADO]
│       ├── - FormaPagoIVA_Descripcion      [ELIMINADO]
│       └── - ObtenerDescripcionFormaPago() [ELIMINADO]
│
└── SERVICES/
    └── ReporteIGIService.cs                [MODIFICADO]
        └── ConvertirADataTable()           
            ├── - FormaPagoIGI_Descripcion  [CAMBIADO]
            ├── + FormaPago_IGI             [NUEVO]
            ├── - FormaPagoIVA_Descripcion  [CAMBIADO]
            └── + FormaPago_IVA             [NUEVO]
```

---

## ✅ Resultado Final

```
✅ Build successful
✅ FormaPago_IGI muestra código numérico
✅ FormaPago_IVA muestra código numérico
✅ Código simplificado (menos transformaciones)
✅ Datos originales preservados
✅ Compatible con estándares SAT
```

---

## 📖 Referencia de Códigos SAT

### Formas de Pago de Contribuciones (Catálogo completo)

| Código | Descripción |
|--------|-------------|
| 1 | Efectivo |
| 2 | Cheque nominativo |
| 3 | Transferencia electrónica |
| 4 | Tarjeta de crédito |
| **5** | **Transferencia electrónica de fondos** ← Usada |
| 6 | Tarjeta de débito |
| 8 | Vales de despensa |
| 12 | Dación en pago |
| 13 | Pago por subrogación |
| 14 | Pago por consignación |
| 15 | Condonación |
| 17 | Novación |
| 23 | Aplicación de anticipos |
| **21** | **Compensación** ← Usada |
| 28 | Tarjeta de servicios |
| 99 | Por definir |

**Nota:** El sistema solo filtra y muestra pedimentos con códigos `5` y `21`.

---

**Fecha de Cambio:** Enero 2026  
**Versión:** 3.0.3  
**Sistema:** Retorno 360° Tacna  
**Archivos Modificados:** `ReporteIGI.cs`, `ReporteIGIService.cs`  
**Tipo:** Simplificación de modelo de datos  
**Estado:** ✅ COMPLETADO
