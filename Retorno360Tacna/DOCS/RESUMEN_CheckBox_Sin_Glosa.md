# ✅ Resumen: CheckBox "Sin Validación TR_GLOSA"

## 🎯 ¿Qué se agregó?

Un **CheckBox** en el reporte de IGI que permite consultar:

- **☐ Desmarcado (Default):** Solo pedimentos cargados en TR_GLOSA
- **☑ Marcado:** TODOS los pedimentos del cliente (sin validar si están en Glosa)

---

## 📊 Diferencias

| Característica | **CON Validación** | **SIN Validación** |
|----------------|-------------------|-------------------|
| **Pedimentos** | Solo en TR_GLOSA (~62) | Todos (~250+) |
| **IGI_Pagado** | Valor real de Glosa | `0` |
| **IVA_Pagado** | Valor real de Glosa | `0` |
| **FormaPago** | Códigos reales | Vacío |
| **Estatus** | SI CARGADO / NO CARGADO | NO VALIDADO |
| **IGI_Calculado** | ✅ Calculado | ✅ Calculado |

---

## 💡 Casos de Uso

### **1. Auditoría de Glosa (Default - Sin marcar)**
Ver solo pedimentos procesados y sus datos reales de pago.

### **2. Análisis Completo (Marcar CheckBox)**
- Identificar pedimentos NO cargados en Glosa
- Ver operaciones completas del cliente
- Proyectar IGI teórico de todos los pedimentos

---

## 🔧 Implementación

### **UI:**
```
[Razón Social ▼]  [Cliente ▼]
[Fecha Inicio 📅] [Fecha Fin 📅]
☐ Sin validación de TR_GLOSA (Todos los pedimentos)
                              [🔍 Consultar]
```

### **Código:**
```csharp
// Servicio con parámetro opcional
reporteService.GenerarReporteIGI(
    baseDatos, 
    fechaInicio, 
    fechaFin, 
    sinValidacionGlosa: chkSinGlosa.Checked // ← NUEVO
);
```

---

## 📝 Queries

### **CON Validación:**
```sql
LEFT JOIN TR_GLOSA TR
    ON TR.Gl_Pedimento = DP.Pim_Folio
    AND ... (múltiples condiciones)
WHERE 
    TR.Gl_FecPagoReal BETWEEN @FechaInicio AND @FechaFin
    AND (TR.Gl_FPagoIVA IN ('5','21') OR TR.Gl_FPagoAdvalorem IN ('5','21'))
```

### **SIN Validación:**
```sql
-- NO hace JOIN con TR_GLOSA
FROM Di_Pedimento DP
INNER JOIN Di_PedimentoDet DI ...
INNER JOIN Ca_Farancelaria FRA ...
WHERE 
    CONVERT(DATE, IIF(...)) BETWEEN @FechaInicio AND @FechaFin
    -- Sin filtro de formas de pago
```

---

## ✅ Archivos Modificados

- `FrmReportes.Designer.cs` - CheckBox agregado
- `FrmReportes.cs` - Lógica para pasar parámetro
- `ReporteIGIService.cs` - Método con sobrecarga

---

**Build:** ✅ Exitoso  
**Estado:** Listo para usar  
**Versión:** 3.0.6
