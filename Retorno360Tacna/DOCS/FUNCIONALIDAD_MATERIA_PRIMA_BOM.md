# Funcionalidad de Materia Prima en Catálogo BOM

## Resumen de Cambios

Se agregó una nueva funcionalidad al módulo de **Catálogo de Partes** que permite consultar la **Materia Prima (MP)** no agregada a la estructura BOM, utilizando un rango de fechas.

---

## 🎯 Características Implementadas

### 1. **Sistema de Pestañas (TabControl)**
Se implementó un control de pestañas que permite alternar entre dos vistas:

- **Pestaña "Producto Terminado (PT)"**: Muestra el gráfico BOM de productos terminados (funcionalidad existente)
- **Pestaña "Materia Prima (MP)"**: Muestra una tabla con la materia prima y su estatus en BOM

### 2. **Nuevo Modelo: MateriaPrimaBOM**
Se creó el modelo `MateriaPrimaBOM.cs` con las siguientes propiedades:
```csharp
- Par_NoParte: Número de parte de la materia prima
- Par_DescripcionEsp: Descripción en español
- Clave: Tipo (MP)
- Par_InsercionFecha: Fecha de inserción
- EstatusComponente: Estado según rango de fechas (VIGENTE EN BOM / NO ESTA EN BOM)
```

### 3. **Nuevo Servicio: ObtenerMateriaPrimaBOM**
Se agregó el método en `CatalogoPartesService.cs`:
```csharp
public List<MateriaPrimaBOM> ObtenerMateriaPrimaBOM(string nombreBaseDatos, DateTime fechaInicio, DateTime fechaFin)
```

**Query utilizado:**
```sql
SELECT 
    cp.Par_NoParte,
    cp.Par_DescripcionEsp,
    cp.Tim_Clave AS Clave,
    cp.Par_InsercionFecha,
    CASE 
        WHEN cp.Par_InsercionFecha BETWEEN @FechaInicio AND @FechaFin
        THEN 'VIGENTE EN BOM'
        WHEN cp.Par_InsercionFecha IS NULL
        THEN 'NO ESTA EN BOM'
        ELSE 'NO ESTA EN BOM'
    END AS EstatusComponente
FROM Ca_Parte AS cp WITH (NOLOCK)
WHERE 
    cp.Tim_Clave = 'MP'
ORDER BY 
    cp.Par_InsercionFecha,
    cp.Par_NoParte
OPTION (MAXDOP 4)
```

### 4. **Interfaz de Usuario**

#### **Tabla de Materia Prima (DataGridView)**
Configuración visual:
- Columnas personalizadas con ancho optimizado
- Formato de fecha: `dd/MM/yyyy`
- Colores condicionales por estatus:
  - **Verde**: VIGENTE EN BOM
  - **Rojo**: NO ESTA EN BOM
- Fuente en negritas para la columna de estatus
- Estilo de encabezados con fondo azul

#### **Panel de Resumen**
Al consultar MP, se muestran estadísticas:
- Total de Materia Prima
- Cantidad vigente en BOM
- Cantidad no agregada a BOM

---

## 🔧 Flujo de Funcionamiento

### Consulta de Producto Terminado (PT)
1. Usuario selecciona la pestaña **"Producto Terminado (PT)"**
2. Hace clic en **"Consultar"**
3. Se ejecuta `ConsultarProductoTerminadoAsync()`
4. Muestra el gráfico BOM completo
5. Habilita botones **"Exportar"** y **"Ver Detalle"**

### Consulta de Materia Prima (MP)
1. Usuario selecciona la pestaña **"Materia Prima (MP)"**
2. Configura el rango de fechas (Fecha Inicio - Fecha Fin)
3. Hace clic en **"Consultar"**
4. Se ejecuta `ConsultarMateriaPrimaAsync()`
5. Muestra la tabla de materia prima con estatus
6. Habilita botón **"Exportar"**
7. **Deshabilita** botón **"Ver Detalle"** (no aplica para MP)

---

## 📊 Métodos Nuevos

### **FrmCatalogoPartes.cs**

#### `ConfigurarDataGridMP()`
Configura el DataGridView de Materia Prima con columnas, estilos y formato.

#### `ConsultarProductoTerminadoAsync()`
Método asíncrono que ejecuta la consulta de Producto Terminado.

#### `ConsultarMateriaPrimaAsync()`
Método asíncrono que ejecuta la consulta de Materia Prima usando el rango de fechas.

#### `MostrarMateriaPrima()`
Renderiza la lista de materia prima en el grid, aplica colores condicionales y actualiza estadísticas.

---

## 🎨 Cambios en el Designer

### Controles Agregados:
- `tabControlCatalogo`: Control principal de pestañas
- `tabPagePT`: Pestaña de Producto Terminado
- `tabPageMP`: Pestaña de Materia Prima
- `dgvMateriaPrima`: DataGridView para mostrar MP

### Reorganización:
- El `chartCatalogo` y `panelNavegacionGrafico` se movieron dentro de `tabPagePT`
- El `dgvMateriaPrima` se colocó dentro de `tabPageMP`
- Se actualizó el `Dock` de los controles para adaptarse al TabControl

---

## ⚡ Mejoras de Rendimiento

### Ejecución Asíncrona
Todas las consultas se ejecutan de forma asíncrona con `Task.Run()` para:
- Evitar bloqueo de la UI
- Mantener la animación del panel de carga
- Mejorar la experiencia del usuario

### Optimización de Queries
- Uso de `WITH (NOLOCK)` para evitar bloqueos
- `OPTION (MAXDOP 4)` para limitar paralelismo
- `CommandTimeout = 300` segundos para bases grandes

---

## 🔍 Uso del Rango de Fechas

### Producto Terminado (PT)
- Solo usa **Fecha Fin** como `fechaActual`
- Valida vigencia de BOM contra esa fecha

### Materia Prima (MP)
- Usa **Fecha Inicio** y **Fecha Fin** como rango
- Determina si la MP fue agregada en ese periodo
- Clasifica según:
  - `VIGENTE EN BOM`: Inserción dentro del rango
  - `NO ESTA EN BOM`: Fuera del rango o fecha nula

---

## ✅ Validaciones y Mensajes

### Panel de Carga
- Mensaje dinámico según tipo de consulta y base de datos
- Para MP: "Consultando materia prima... Por favor espere"
- Para bases grandes: "Esto puede tardar varios minutos"

### Sin Resultados
- PT: Muestra mensaje con la fecha consultada
- MP: Muestra mensaje con el rango de fechas

---

## 📝 Archivos Modificados

1. **Retorno360Tacna\MODELS\MateriaPrimaBOM.cs** *(nuevo)*
2. **Retorno360Tacna\SERVICES\CatalogoPartesService.cs**
3. **Retorno360Tacna\FORMS\FrmCatalogoPartes.cs**
4. **Retorno360Tacna\FORMS\FrmCatalogoPartes.Designer.cs**

---

## 🚀 Próximas Mejoras Sugeridas

- [ ] Exportar tabla de Materia Prima a Excel
- [ ] Agregar filtro de búsqueda en el DataGridView de MP
- [ ] Implementar paginación si hay más de 10,000 registros de MP
- [ ] Agregar opción de detalle para MP (similar a PT)
- [ ] Implementar gráfico estadístico para MP

---

**Fecha de implementación:** Mayo 2025  
**Compilación:** ✅ Exitosa  
**Pruebas:** Pendientes en entorno de producción
