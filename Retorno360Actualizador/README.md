# Actualizador Retorno 360

Este es el actualizador automático para Retorno 360 que descarga la última versión desde GitHub.

## Características

- Descarga automática de la última versión desde GitHub
- Barra de progreso en tiempo real
- Instalación automática en el directorio del ejecutable
- Sobreescritura de archivos existentes
- Interfaz gráfica simple y clara

## Cómo Funciona

1. El actualizador se conecta a GitHub usando la API de Octokit
2. Busca el último release del repositorio `Javier-Nieto23/Retorno360Tacna`
3. Descarga el archivo ZIP del release
4. Extrae los archivos en un directorio temporal
5. Copia todos los archivos al directorio de instalación (sobrescribiendo)
6. Limpia los archivos temporales
7. Muestra mensaje de éxito

## Cómo Crear un Release en GitHub

Para que el actualizador funcione, debes crear releases en GitHub con los siguientes pasos:

### 1. Preparar los archivos

Compila tu proyecto en modo Release:

```bash
dotnet publish "C:\Users\jnieto\source\repos\Retorno360Tacna\Retorno360Tacna\Retorno360Tacna.csproj" -c Release -o "./publish"
```

### 2. Crear archivo ZIP

Comprime la carpeta `publish` en un archivo ZIP. Puedes nombrarlo:
- `Retorno360Tacna-v1.0.0.zip`
- `Retorno360-Release.zip`
- Cualquier nombre que termine en `.zip`

### 3. Crear el Release en GitHub

1. Ve a tu repositorio: https://github.com/Javier-Nieto23/Retorno360Tacna
2. Haz clic en "Releases" (en el menú lateral derecho)
3. Haz clic en "Create a new release"
4. Completa los siguientes campos:
   - **Tag version**: v1.0.0 (incrementa según la versión)
   - **Release title**: Versión 1.0.0 - Descripción breve
   - **Describe this release**: Changelog detallado de cambios
5. **IMPORTANTE**: Arrastra el archivo ZIP a la sección "Attach binaries"
6. Marca como "Latest release" si es la versión más reciente
7. Haz clic en "Publish release"

### 4. Estructura del Release

El actualizador buscará automáticamente:
- El **último** release publicado
- El **primer** archivo `.zip` en los assets del release

## Ubicación del Actualizador

El ejecutable `Retorno360Actualizador.exe` debe estar en la **misma carpeta** que el ejecutable principal `Retorno360Tacna.exe`.

Estructura de carpetas recomendada:
```
C:\Retorno360\
├── Retorno360Tacna.exe
├── Retorno360Actualizador.exe
├── (otros archivos DLL y dependencias)
└── ...
```

## Integración con la Aplicación Principal

Para llamar al actualizador desde tu aplicación principal, puedes agregar un botón o menú:

```csharp
private void btnActualizar_Click(object sender, EventArgs e)
{
    try
    {
        string actualizadorPath = Path.Combine(
            Path.GetDirectoryName(Application.ExecutablePath) ?? string.Empty,
            "Retorno360Actualizador.exe"
        );

        if (File.Exists(actualizadorPath))
        {
            Process.Start(actualizadorPath);
            Application.Exit(); // Cerrar la aplicación antes de actualizar
        }
        else
        {
            MessageBox.Show("No se encontró el actualizador.",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error al iniciar el actualizador: {ex.Message}",
            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
```

## Compilación

Para compilar el actualizador:

```bash
cd Retorno360Actualizador
dotnet build -c Release
```

El ejecutable se generará en:
`Retorno360Actualizador\bin\Release\net10.0-windows\Retorno360Actualizador.exe`

## Requisitos

- .NET 10 Runtime
- Conexión a Internet
- Acceso al repositorio de GitHub (público o con token de acceso)

## Notas Importantes

1. El actualizador NO se sobrescribe a sí mismo durante la actualización
2. Los archivos en uso pueden no actualizarse (requieren reinicio)
3. Se recomienda cerrar completamente la aplicación principal antes de actualizar
4. El progreso de descarga se muestra en MB descargados
5. Los errores se muestran en rojo en la interfaz

## Troubleshooting

### "No se encontraron versiones disponibles"
- Verifica que el repositorio tenga al menos un release publicado
- Verifica que el release esté marcado como publicado (no draft)

### "No se encontró archivo ZIP en el release"
- Asegúrate de haber subido un archivo `.zip` al release
- Verifica que el archivo esté en la sección de Assets del release

### "Error de conexión"
- Verifica tu conexión a Internet
- Verifica que puedas acceder a https://github.com
- Si el repo es privado, necesitarás configurar autenticación

## Changelog

### Versión 1.0.0
- Implementación inicial
- Descarga automática desde GitHub
- Barra de progreso
- Instalación automática
- Interfaz de usuario
