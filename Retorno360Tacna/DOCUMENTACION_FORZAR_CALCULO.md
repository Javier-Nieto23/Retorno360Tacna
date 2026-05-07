# Funcionalidad: Forzar Cálculo de Retorno

## Descripción

Se ha agregado una nueva opción en el formulario de **Cálculo de Retorno** que permite realizar el cálculo incluso cuando no existan pedimentos de importación o exportación cargados como ZIP en la glosa.

## Ubicación

**Formulario:** `FrmRetorno.cs`  
**Checkbox:** "Forzar cálculo (omitir validaciones)"  
**Ubicación en UI:** Debajo del checkbox "Calcular por Razón Social"

## Funcionalidad

### Comportamiento Normal (checkbox desmarcado)

Cuando el checkbox **"Forzar cálculo"** está **desmarcado** (comportamiento por defecto), el sistema realiza las siguientes validaciones:

1. **Solo Exportaciones**: Si `importado == 0` y `exportado > 0`
   - ❌ Muestra error: "No se encontraron importaciones cargadas como ZIP..."

2. **Solo Importaciones**: Si `exportado == 0` y `importado > 0`
   - ❌ Muestra error: "No se encontraron exportaciones cargadas como ZIP..."

3. **Sin Pedimentos**: Si `importado == 0` y `exportado == 0`
   - ❌ Muestra error: "No se encontraron pedimentos (ni importaciones ni exportaciones)..."

### Comportamiento con Forzar Cálculo (checkbox marcado)

Cuando el checkbox **"Forzar cálculo"** está **marcado**, el sistema:

✅ **Omite todas las validaciones** de existencia de pedimentos  
✅ **Realiza el cálculo** con los valores obtenidos (aunque sean 0)  
✅ **Permite continuar** incluso si solo hay importaciones o solo exportaciones  
✅ **Muestra mensaje especial** indicando que se omitieron las validaciones

## Casos de Uso

### Caso 1: Solo hay Importaciones
```
Importado: $100,000.00
Exportado: $0.00
```
- **Sin forzar**: ❌ Error - "No se encontraron exportaciones..."
- **Con forzar**: ✅ Calcula - Porcentaje: 0% (mensaje especial)

### Caso 2: Solo hay Exportaciones
```
Importado: $0.00
Exportado: $50,000.00
```
- **Sin forzar**: ❌ Error - "No se encontraron importaciones..."
- **Con forzar**: ✅ Calcula - Porcentaje: 0% (mensaje especial)

### Caso 3: No hay ningún pedimento
```
Importado: $0.00
Exportado: $0.00
```
- **Sin forzar**: ❌ Error - "No se encontraron pedimentos..."
- **Con forzar**: ✅ Calcula - Porcentaje: 0% (mensaje especial)

### Caso 4: Hay ambos (funcionamiento normal)
```
Importado: $100,000.00
Exportado: $75,000.00
```
- **Sin forzar**: ✅ Calcula - Porcentaje: 75%
- **Con forzar**: ✅ Calcula - Porcentaje: 75% (mensaje especial)

## Mensaje al Usuario

Cuando se utiliza **"Forzar cálculo"**, el sistema muestra:

```
✓ Cálculo completado exitosamente.

NOTA: Se omitieron las validaciones de pedimentos. 
El cálculo se realizó con los datos disponibles.
```

## Implementación Técnica

### Archivo: `RetornoService.cs`

**Método modificado:**
```csharp
public ResultadoRetorno CalcularRetorno(
    int idRazon,
    string baseDatosSeleccionada,
    DateTime fechaInicio,
    DateTime fechaFin,
    bool incluirMateriaPrima,
    bool forzarCalculo = false)  // 👈 Nuevo parámetro
```

**Lógica de validación:**
```csharp
// Validar que existan tanto importaciones como exportaciones
// A menos que se fuerce el cálculo
if (!forzarCalculo)
{
    if (importado == 0 && exportado > 0)
    {
        throw new Exception("No se encontraron importaciones...");
    }

    if (exportado == 0 && importado > 0)
    {
        throw new Exception("No se encontraron exportaciones...");
    }

    if (importado == 0 && exportado == 0)
    {
        throw new Exception("No se encontraron pedimentos...");
    }
}
// Si forzarCalculo == true, se salta toda esta validación
```

### Archivo: `FrmRetorno.cs`

**Llamada al servicio:**
```csharp
resultado = await Task.Run(() => retornoService.CalcularRetorno(
    idRazonSeleccionada,
    baseDatosSeleccionada,
    dtpFechaInicio.Value,
    dtpFechaFin.Value,
    chkMateriaPrima.Checked,
    chkForzarCalculo.Checked  // 👈 Checkbox de forzar
));
```

## Consideraciones Importantes

⚠️ **Uso Responsable**: Esta opción debe usarse solo cuando:
- Se está en proceso de depuración
- Se desea analizar datos incompletos
- Se tiene certeza de que la falta de pedimentos es esperada

⚠️ **Resultados**: Los resultados con "Forzar cálculo" pueden no ser representativos de la operación real si faltan datos.

⚠️ **No afecta**: Esta opción **NO** afecta el cálculo por razón social (`chkCalRazon`), que ya tiene su propia lógica de cálculo sin validación cruzada.

## Interacción con otras opciones

| Opción | Calcular por Razón Social | Forzar Cálculo | Comportamiento |
|--------|---------------------------|----------------|----------------|
| 1 | ❌ Desmarcado | ❌ Desmarcado | Validación normal + validación cruzada |
| 2 | ❌ Desmarcado | ✅ Marcado | Sin validación de existencia de pedimentos |
| 3 | ✅ Marcado | ❌ Desmarcado | Sin validación cruzada (usa todos los pedimentos de TR_Glosa) |
| 4 | ✅ Marcado | ✅ Marcado | Sin validación cruzada (el checkbox forzar no tiene efecto) |

## Versión

- **Implementado**: Mayo 2026
- **Versión del sistema**: .NET 10
- **Archivos modificados**:
  - `SERVICES/RetornoService.cs`
  - `FORMS/FrmRetorno.cs`
  - `FORMS/FrmRetorno.Designer.cs`
