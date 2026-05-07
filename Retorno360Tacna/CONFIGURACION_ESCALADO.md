# Sistema de Escalado de Interfaz - Retorno 360 Tacna

## Descripción

El sistema de escalado de interfaz permite que la aplicación mantenga una apariencia visual consistente independientemente de la configuración de zoom de Windows (100%, 125%, 150%, etc.).

## Características

### 1. Configuración de Escala
La aplicación permite seleccionar entre diferentes escalas:
- **100%** - Tamaño estándar (recomendado para pantallas Full HD con zoom de Windows al 100%)
- **125%** - Aumenta todos los elementos un 25% (recomendado para pantallas Full HD con zoom de Windows al 125%)
- **150%** - Aumenta todos los elementos un 50% (recomendado para pantallas 4K)
- **175%** - Aumenta todos los elementos un 75%
- **200%** - Duplica el tamaño de todos los elementos

### 2. Elementos Escalados

El sistema escala automáticamente:
- ✅ **Fuentes** - Todos los textos se ajustan proporcionalmente
- ✅ **Botones** - Tamaño y posición de todos los botones
- ✅ **Imágenes e Iconos** - Las imágenes en botones y PictureBox
- ✅ **Controles** - Todos los controles (TextBox, ComboBox, DataGridView, etc.)
- ✅ **Padding y Margin** - Espaciado interno y externo de controles
- ✅ **DataGridView** - Altura de filas y columnas

### 3. Acceso a la Configuración

1. En el menú lateral, haga clic en **⚙ Configuración** (ubicado arriba de "Cerrar Sesión")
2. Seleccione la escala deseada en el desplegable "Escala de Interfaz"
3. Haga clic en **Vista Previa** para ver información sobre los cambios
4. Haga clic en **Guardar** para aplicar los cambios
5. La aplicación se reiniciará automáticamente para aplicar la nueva escala

### 4. Archivo de Configuración

La configuración se guarda en:
```
%AppData%\Retorno360Tacna\config.txt
```

Formato del archivo:
```
EscalaUI=1.25
```

### 5. Uso Recomendado

| Zoom de Windows | Escala Recomendada | Descripción |
|-----------------|-------------------|-------------|
| 100% | 100% | Configuración estándar |
| 125% | 125% | Compensación para zoom de Windows al 125% |
| 150% | 150% | Para pantallas de alta resolución |

### 6. Implementación Técnica

El escalado se aplica mediante el servicio `ConfiguracionService`:

```csharp
// Obtener la escala configurada
decimal escala = ConfiguracionService.ObtenerEscalaUI();

// Aplicar escala a un formulario
ConfiguracionService.AplicarEscalaFormulario(this, escala);
```

El sistema escala recursivamente todos los controles del formulario, ajustando:
- Tamaño y posición
- Fuentes
- Imágenes
- Padding y margin
- Configuraciones específicas de DataGridView

### 7. Notas Importantes

- ⚠️ **Reinicio requerido**: Después de cambiar la escala, la aplicación debe reiniciarse
- ⚠️ **Compatibilidad**: El escalado se aplica al cargar cada formulario
- ⚠️ **Imágenes**: Las imágenes se redimensionan proporcionalmente
- ⚠️ **Performance**: El escalado se realiza una sola vez al cargar el formulario

### 8. Solución de Problemas

**Problema**: Los elementos se ven muy grandes o muy pequeños
- **Solución**: Ajuste la escala en Configuración → Escala de Interfaz

**Problema**: Los cambios no se aplican
- **Solución**: Asegúrese de hacer clic en "Guardar" y reiniciar la aplicación

**Problema**: Algunos elementos no escalan correctamente
- **Solución**: Verifique que el control esté agregado al formulario antes de aplicar el escalado

## Desarrollo

Para desarrolladores que quieran extender el sistema:

### Agregar Escalado a un Nuevo Formulario

```csharp
private void MiFormulario_Load(object sender, EventArgs e)
{
    // Aplicar escalado de UI
    decimal escala = SERVICES.ConfiguracionService.ObtenerEscalaUI();
    if (escala != 1.0m)
    {
        SERVICES.ConfiguracionService.AplicarEscalaFormulario(this, escala);
    }

    // ... resto del código de carga
}
```

### Estructura de Archivos

- `SERVICES/ConfiguracionService.cs` - Lógica de escalado
- `MODELS/ConfiguracionUsuario.cs` - Modelo de datos
- `FORMS/FrmConfiguracion.cs` - Interfaz de configuración
- `FORMS/MainMenu.cs` - Aplicación del escalado en el menú principal

## Versión

Sistema implementado en: 2026
Última actualización: Mayo 2026
