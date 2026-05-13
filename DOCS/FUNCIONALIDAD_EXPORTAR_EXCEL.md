# Funcionalidad: Exportar Reportes a Excel

## Descripción General
Esta funcionalidad permite exportar las tablas de reportes IGI e IVA generadas en el formulario `FrmReportes` a un archivo Excel (.xlsx), facilitando el análisis offline y el compartir la información con otros usuarios.

## Características Principales

### 1. Botón de Exportación
- **Ubicación**: Panel de filtros, junto al botón "Generar PDF"
- **Color**: Verde (#2E7D32) para distinguirlo del botón PDF (rojo)
- **Estado inicial**: Deshabilitado
- **Se habilita**: Después de generar un reporte exitoso con datos

### 2. Exportación de Datos

#### Hojas del Archivo Excel
El archivo generado contiene **2 hojas**:

1. **Hoja "Reporte IGI"**
   - Contiene todos los datos de la tabla de IGI
   - Columnas incluidas:
     - MES
     - IGI PAGADO
     - IGI CALCULADO
     - DIFERENCIA
     - FORMA DE PAGO IGI
   - Encabezados con color azul (#2980B9)

2. **Hoja "Reporte IVA"**
   - Contiene todos los datos de la tabla de IVA
   - Columnas incluidas:
     - MES
     - IVA PAGADO
     - FORMA DE PAGO IVA
   - Encabezados con color verde (#27AE60)

### 3. Formato del Archivo

#### Encabezados
- Texto en **negrita**
- Fondo de color (azul para IGI, verde para IVA)
- Texto blanco
- Alineación centrada

#### Datos
- Valores monetarios con formato de moneda: `$#,##0.00`
- Columnas ajustadas automáticamente al contenido
- Datos exactamente como se muestran en las tablas del formulario

#### Nombre del Archivo
Patrón: `Reporte_IGI_IVA_YYYYMMDD_HHmmss.xlsx`

Ejemplo: `Reporte_IGI_IVA_20240515_143022.xlsx`

## Flujo de Uso

1. **Generar Reporte**
   - Seleccionar razón social y cliente (o usar modo sin glosa)
   - Seleccionar rango de fechas
   - Hacer clic en "Consultar"
   - Esperar a que se carguen los datos

2. **Exportar a Excel**
   - Verificar que el botón "Excel" esté habilitado
   - Hacer clic en el botón "Excel"
   - Seleccionar ubicación y nombre del archivo en el diálogo
   - Confirmar guardado

3. **Abrir Archivo**
   - El sistema pregunta si desea abrir el archivo
   - Seleccionar "Sí" para abrir inmediatamente con Excel
   - O "No" para abrir manualmente más tarde

## Validaciones

### Validaciones Previas
- ✅ Debe existir un reporte generado con datos
- ✅ Si no hay datos, muestra mensaje: "No hay datos de reporte para exportar"

### Manejo de Errores
- Captura errores de escritura de archivo
- Muestra mensaje detallado en caso de error
- Restaura estado del botón después de error

## Dependencias Técnicas

### Paquete NuGet
```xml
<PackageReference Include="ClosedXML" Version="0.105.0" />
```

### Namespace Requerido
```csharp
using ClosedXML.Excel;
```

## Archivos Modificados

### FrmReportes.Designer.cs
- Agregado botón `btnExportarExcel`
- Configuración visual y eventos

### FrmReportes.cs
- Método `btnExportarExcel_Click()`
- Habilitación/deshabilitación del botón en `MostrarResultados()` y `FrmReportes_Load()`
- Using de `ClosedXML.Excel`

## Ventajas sobre PDF

1. **Editable**: Los usuarios pueden modificar y analizar los datos
2. **Filtrable**: Se pueden aplicar filtros y ordenamientos en Excel
3. **Calculable**: Permite crear fórmulas adicionales
4. **Formato estándar**: Compatible con múltiples aplicaciones
5. **Ligero**: Archivos más pequeños que PDF equivalentes

## Casos de Uso

### 1. Análisis Detallado
Usuario exporta los datos a Excel para:
- Crear gráficas personalizadas
- Aplicar filtros complejos
- Realizar cálculos adicionales

### 2. Presentaciones
Usuario incluye las tablas exportadas en:
- Informes ejecutivos
- Presentaciones PowerPoint
- Documentos Word

### 3. Compartir Datos
Usuario envía el archivo Excel a:
- Contadores
- Gerentes
- Auditores externos

## Mejoras Futuras

### Posibles Extensiones
- [ ] Agregar hoja de resumen con totales
- [ ] Incluir gráficas de los reportes
- [ ] Permitir seleccionar qué columnas exportar
- [ ] Exportar datos de detalle de pedimentos
- [ ] Agregar filtros avanzados pre-exportación
- [ ] Exportar múltiples períodos en una sola hoja

### Optimizaciones
- [ ] Agregar barra de progreso para exportaciones grandes
- [ ] Implementar exportación asíncrona
- [ ] Caché de última exportación para re-exportar rápidamente

## Notas Técnicas

### Rendimiento
- La exportación es síncrona (bloquea UI temporalmente)
- Para reportes típicos (< 1000 filas), la exportación es instantánea
- El botón se deshabilita durante la exportación

### Seguridad
- No se validan permisos de escritura antes de mostrar diálogo
- El sistema operativo maneja los permisos de archivo
- No se almacena información sensible en el archivo

### Compatibilidad
- Formato: Office Open XML (.xlsx)
- Compatible con:
  - Microsoft Excel 2007 y superior
  - LibreOffice Calc
  - Google Sheets (importación)
  - Numbers (macOS)
