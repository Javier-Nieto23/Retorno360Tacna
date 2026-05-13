# CHANGELOG - v2.5.1

## 🐛 Corrección de Bug Crítico: Escalado DPI en FrmRetorno

### Problema
Las gráficas en el formulario `FrmRetorno` no se alineaban correctamente cuando el usuario configuraba el escalado de Windows al 125%, 150% o 175%, resultando en gráficas desplazadas, cortadas o superpuestas con otros controles.

### Solución
Se implementó detección automática de DPI y escalado proporcional de todos los controles de gráficas.

### Archivos Modificados
- `Retorno360Tacna/FORMS/FrmRetorno.cs`
  - Método `AjustarControles()` reescrito para detectar DPI actual
  - Cálculo dinámico de posiciones basado en `Graphics.DpiX`
  - Reposicionamiento automático de botones de navegación

- `README.md`
  - Versión actualizada a v2.5.1
  - Nueva característica "Soporte Multi-DPI" documentada
  - Sección de solución de problemas expandida con fix de DPI

- `DOCS/FIX_DPI_SCALING.md` (nuevo)
  - Documentación técnica completa del fix
  - Ejemplos de código y tabla de escalado
  - Referencias y mejoras futuras

### Cambios Técnicos

#### Antes
```csharp
// Posiciones fijas sin considerar DPI
panelGraficaColumnas.Location = new Point(CHART_LEFT, CHART_TOP);
panelGraficaColumnas.Width = Math.Max(350, nuevoAncho);
```

#### Después
```csharp
// Detección de DPI y escalado proporcional
using (Graphics g = this.CreateGraphics())
{
    float dpiScale = g.DpiX / 96.0f; // Detectar escalado
    int chartLeft = (int)((RESULTS_WIDTH + MARGIN * 2) * dpiScale);
    int chartTop = (int)(CHART_TOP * dpiScale);

    panelGraficaColumnas.Location = new Point(chartLeft, chartTop);
    panelGraficaColumnas.Size = new Size(anchoGrafica, altoGrafica);

    // Reposicionar controles internos
    btnSiguienteColumnas.Location = new Point(anchoGrafica - 45, 5);
}
```

### Impacto
- ✅ Compatible con escalados: 100%, 125%, 150%, 175%, 200%
- ✅ Mejora significativa en pantallas de alta resolución (4K, UHD)
- ✅ No requiere configuración del usuario
- ✅ Experiencia consistente en cualquier monitor

### Testing
- ✅ Build exitoso sin errores
- ✅ Probado en escalado 100% (96 DPI)
- ✅ Probado en escalado 125% (120 DPI) - caso reportado
- ✅ Probado en escalado 150% (144 DPI)
- ✅ Probado en escalado 175% y 200%

### Versión
- **Anterior**: v2.5.0
- **Nueva**: v2.5.1
- **Fecha**: Enero 2025

---

## 📦 Instrucciones de Despliegue

1. Compilar el proyecto actualizado
2. Generar instalador con Advanced Installer
3. Actualizar link de descarga: https://digizen.tacna.net/index.php/s/NqeekQR2MrtkH3x
4. Notificar a usuarios sobre el fix de DPI
5. Actualizar número de versión en "Acerca de"

---

## 🔗 Referencias
- Documentación técnica: `DOCS/FIX_DPI_SCALING.md`
- README actualizado: `README.md`
- Código modificado: `Retorno360Tacna/FORMS/FrmRetorno.cs`
