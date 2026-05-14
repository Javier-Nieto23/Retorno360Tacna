# Corrección de IVA en Multi-Servidor

## 📋 Problema Identificado

El método `ObtenerDatosAgrupadosMultiServidor` (usado cuando las bases están en **servidores diferentes**) no estaba calculando correctamente el **IVA_Pagado** porque:

1. **Mezclaba IGI e IVA en una sola consulta**
2. **Aplicaba el mismo filtro de `FormaPago_IGI` para buscar tanto IGI como IVA**
3. **No seguía la lógica del query original** que especifica:
   - Para **IGI**: JOIN con filtro por `Pedimento + FormaPago_IGI`
   - Para **IVA**: JOIN con filtro **solo por `Pedimento`** (sin FormaPago)

---

## ❌ Código Anterior (INCORRECTO)

### **Problema 1: Una sola consulta para IGI e IVA**

```csharp
// ❌ INCORRECTO - Mezclaba IGI e IVA en un solo registro
var datosGlosa = ObtenerDatosGlosaAgrupadosParaPedimento(
    baseDatosGlosa,
    pedimento.Aduana,
    pedimento.Patente,
    pedimento.Folio,
    pedimento.FechaPago,
    pedimento.FormaPago_IGI,  // ❌ Usaba FormaPago_IGI para buscar IVA también
    conexionGlosa
);

// Creaba UN SOLO reporte con IGI e IVA juntos
var reporte = new ReporteIGIPagado
{
    IGI_Pagado = datosGlosa.IGI_Pagado,
    IGI_Calculado = pedimento.IGI_Calculado,
    IVA_Pagado = datosGlosa.IVA_Pagado,  // ❌ IVA podía estar mal
    FormaPago_IGI = datosGlosa.FormaPago_IGI,
    FormaPago_IVA = datosGlosa.FormaPago_IVA,
};
```

### **Problema 2: Query de glosa filtraba por FormaPago_IGI**

```sql
-- ❌ INCORRECTO - Aplicaba el mismo filtro para IGI e IVA
WHERE TR.Gl_Pedimento = @Folio
    AND TR.Gl_Aduana = @Aduana
    AND TR.Gl_Patente = @Patente
    AND TR.Gl_TOper = 1
    AND TR.Gl_OrigenZipGlosa = 'S'
    AND TR.Gl_FPagoAdvalorem = @FormaPagoIGI  -- ❌ Filtraba ambos por FormaPago_IGI
```

---

## ✅ Solución Implementada

### **Separación de consultas IGI e IVA**

Ahora el método `ObtenerDatosAgrupadosMultiServidor` hace **DOS consultas separadas**:

#### **PASO 1: Obtener pedimentos únicos**
```csharp
// ✅ Obtener lista de pedimentos únicos (sin duplicados)
var pedimentosUnicos = new Dictionary<string, (int IdPedimento, DateTime? FechaPago)>();
```

#### **PASO 2: Consulta de IGI (con FormaPago_IGI)**
```csharp
// ✅ Obtener datos de IGI agrupados por Pedimento + FormaPago_IGI
var datosIGI = ObtenerDatosIGIAgrupadosMultiServidor(baseDatosPedimentos, fechaInicio, fechaFin, conexionPedimentos);

foreach (var datoIGI in datosIGI)
{
    // Buscar en glosa filtrando por FormaPago_IGI
    var datosGlosaIGI = ObtenerDatosGlosaIGIAgrupadosParaPedimento(
        baseDatosGlosa,
        datoIGI.Aduana,
        datoIGI.Patente,
        datoIGI.Folio,
        datoIGI.FechaPago,
        datoIGI.FormaPago_IGI,  // ✅ Filtra por FormaPago_IGI
        conexionGlosa
    );

    // Crear reporte SOLO con datos de IGI
    var reporte = new ReporteIGIPagado
    {
        IGI_Pagado = datosGlosaIGI.IGI_Pagado,
        IGI_Calculado = datoIGI.IGI_Calculado,
        IVA_Pagado = 0,  // Sin IVA en este reporte
        FormaPago_IGI = datosGlosaIGI.FormaPago_IGI,
        FormaPago_IVA = string.Empty,
    };
    resultados.Add(reporte);
}
```

#### **PASO 3: Consulta de IVA (solo por Pedimento)**
```csharp
// ✅ Para cada pedimento único, buscar IVA SIN filtrar por FormaPago
foreach (var kvp in pedimentosUnicos)
{
    // Buscar en glosa SIN filtrar por FormaPago_IGI
    var datosGlosaIVA = ObtenerDatosGlosaIVAAgrupadosParaPedimento(
        baseDatosGlosa,
        aduana,
        patente,
        folio,
        fechaPago,
        conexionGlosa  // ✅ NO recibe FormaPago_IGI
    );

    // Crear reporte SOLO con datos de IVA
    var reporte = new ReporteIGIPagado
    {
        IGI_Pagado = 0,  // Sin IGI en este reporte
        IGI_Calculado = 0,
        IVA_Pagado = datosGlosaIVA.IVA_Pagado,
        FormaPago_IGI = string.Empty,
        FormaPago_IVA = datosGlosaIVA.FormaPago_IVA,
    };
    resultados.Add(reporte);
}
```

---

## 🔍 Nuevos Métodos Auxiliares

### **1. `ObtenerDatosIGIAgrupadosMultiServidor`**
- Obtiene datos de IGI agrupados por `Pedimento + FormaPago_IGI`
- Calcula `IGI_Calculado` con la fórmula correcta
- Incluye `FormaPago_IGI` en el GROUP BY

### **2. `ObtenerDatosGlosaIGIAgrupadosParaPedimento`**
- Busca en TR_GLOSA filtrando por `Pedimento + FormaPago_IGI`
- Solo permite formas de pago `'0'` y `'5'` para IGI
- Valida que `IGI_Pagado > 0`

### **3. `ObtenerDatosGlosaIVAAgrupadosParaPedimento`**
- Busca en TR_GLOSA filtrando **solo por `Pedimento`** (sin FormaPago_IGI)
- Solo permite formas de pago `'0'` y `'21'` para IVA
- Valida que `IVA_Pagado > 0`

---

## 📊 Comparación Final

| Aspecto | Mismo Servidor | Multi-Servidor (Antes) | Multi-Servidor (Ahora) | Estado |
|---------|----------------|------------------------|------------------------|---------|
| **Consulta IGI con FormaPago_IGI** | ✅ Sí | ❌ Mezclado | ✅ **Sí (separado)** | ✅ **Unificado** |
| **Consulta IVA solo por Pedimento** | ✅ Sí | ❌ Con FormaPago_IGI | ✅ **Sí (sin FormaPago)** | ✅ **Unificado** |
| **Reportes separados IGI/IVA** | ✅ 2 result sets | ❌ 1 registro mixto | ✅ **2 registros separados** | ✅ **Unificado** |
| **Filtro FormaPago IGI: 0,5** | ✅ Sí | ✅ Sí | ✅ Sí | ✅ **Igual** |
| **Filtro FormaPago IVA: 0,21** | ✅ Sí | ❌ Usaba 0,5 | ✅ **Ahora Sí (0,21)** | ✅ **Unificado** |

---

## ✅ Resultado

Ahora **ambos métodos (mismo servidor y multi-servidor)** calculan el IVA de la misma manera:

1. ✅ **IGI**: INNER JOIN entre `@PedimentosGLOSAIGI` y `@PedimentosCLIENTE` filtrando por `Pedimento + FormaPago_IGI`
2. ✅ **IVA**: INNER JOIN entre `@PedimentosGLOSAIVA` y `@PedimentosCLIENTE` filtrando **solo por `Pedimento`**
3. ✅ Se generan **dos registros separados** (uno para IGI, otro para IVA) tal como lo hace el query original
4. ✅ Formas de pago correctas: `'0','5'` para IGI y `'0','21'` para IVA

---

## 📝 Fecha de Actualización
**Fecha:** 2025-01-XX
**Modificado por:** Sistema de validación de queries IGI/IVA
