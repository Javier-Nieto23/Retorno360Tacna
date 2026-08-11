# Filtros de Formas de Pago - Reporte IGI

## 📋 Resumen de Cambios

Se corrigió un problema donde el método `ObtenerDatosAgrupadosMultiServidor` estaba agregando pedimentos adicionales que **NO debían estar** en el reporte según el query original.

---

## ❌ Problema Identificado

### **Antes (INCORRECTO):**

En el método multi-servidor había un **filtro adicional en C#** que permitía agregar pedimentos extras:

```csharp
// ❌ FILTRO INCORRECTO - Agregaba pedimentos con FormaPago_IGI == "21"
if (reporte.FormaPago_IGI == "0" || reporte.FormaPago_IGI == "5" || reporte.FormaPago_IGI == "21" ||
    reporte.FormaPago_IVA == "0" || reporte.FormaPago_IVA == "21")
{
    resultados.Add(reporte);
}
```

Este filtro **NO coincide con el query original**, que especifica:
- Para **IGI**: Solo formas de pago `'0'` y `'5'`
- Para **IVA**: Solo formas de pago `'0'` y `'21'`

---

## ✅ Solución Implementada

### **Ahora (CORRECTO):**

1. **Eliminamos el filtro adicional de C#** en `ObtenerDatosAgrupadosMultiServidor`
2. **Agregamos el filtro directamente en el SQL** de `ObtenerDatosGlosaAgrupadosParaPedimento`:

```sql
WHERE TR.Gl_Pedimento = @Folio
    AND TR.Gl_Aduana = @Aduana
    AND TR.Gl_Patente = @Patente
    AND TR.Gl_TOper = 1
    AND TR.Gl_OrigenZipGlosa = 'S'
    AND TR.Gl_FPagoAdvalorem = @FormaPagoIGI
    AND TR.Gl_FPagoAdvalorem IN ('0','5')  -- ✅ Solo formas de pago permitidas para IGI
    AND (@FechaPago IS NULL OR YEAR(CONVERT(DATE, TR.Gl_FecPagoReal)) = YEAR(@FechaPago))
HAVING SUM(ISNULL(TR.Gl_ImporteADvalorem,0)) > 0  -- ✅ Solo si hay IGI pagado
```

3. **Validamos en C# solo por coincidencia y datos válidos**:

```csharp
// ✅ Solo valida que coincidan y que haya datos de glosa
if (datosGlosa.FormaPago_IGI == pedimento.FormaPago_IGI && !string.IsNullOrEmpty(datosGlosa.Pedimento))
{
    resultados.Add(reporte);
}
```

---

## 📊 Formas de Pago Permitidas (según Query Original)

### **Para Reporte IGI:**
- **Forma de pago `'0'`**: Efectivo
- **Forma de pago `'5'`**: Transferencia electrónica de fondos

### **Para Reporte IVA:**
- **Forma de pago `'0'`**: Efectivo
- **Forma de pago `'21'`**: Compensación

---

## 🔍 Comparación: Mismo Servidor vs Multi-Servidor

| Aspecto | Mismo Servidor | Multi-Servidor | Estado |
|---------|----------------|----------------|---------|
| **Query filtra por FormaPago IN ('0','5')** | ✅ Sí | ✅ **Ahora Sí** | ✅ **Unificado** |
| **Filtro adicional en C#** | ❌ No (correcto) | ❌ **Ahora No** (antes sí) | ✅ **Unificado** |
| **Solo agrega pedimentos que coinciden** | ✅ INNER JOIN | ✅ **Validación en C#** | ✅ **Unificado** |
| **Excluye pedimentos sin IGI pagado** | ✅ HAVING > 0 | ✅ **HAVING > 0** | ✅ **Unificado** |

---

## ✅ Resultado

Ahora **ambos métodos (mismo servidor y multi-servidor)** filtran de la misma manera:

1. ✅ Solo pedimentos con **FormaPago_IGI en ('0','5')**
2. ✅ Solo pedimentos que **coinciden entre cliente y glosa** (por pedimento + FormaPago_IGI)
3. ✅ Solo pedimentos con **IGI_Pagado > 0**
4. ✅ Solo pedimentos de **TR_GLOSA con OrigenZipGlosa = 'S'**

**No se agregan pedimentos adicionales fuera de lo solicitado en el query original.**

---

## 📝 Fecha de Actualización
**Fecha:** $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
**Modificado por:** Sistema de validación de queries IGI
