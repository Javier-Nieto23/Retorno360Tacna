# Integración del Actualizador en MainMenu

## Opción 1: Agregar botón de actualización en el menú lateral

### 1. Agregar el botón en MainMenu.Designer.cs

Busca la sección donde se declaran los botones del menú y agrega:

```csharp
private Button btnActualizar;
```

Luego en el método `InitializeComponent()`, agrega la configuración del botón:

```csharp
// 
// btnActualizar
// 
btnActualizar.Cursor = Cursors.Hand;
btnActualizar.Dock = DockStyle.Bottom;
btnActualizar.FlatAppearance.BorderSize = 0;
btnActualizar.FlatStyle = FlatStyle.Flat;
btnActualizar.Font = new Font("Segoe UI", 11F);
btnActualizar.ForeColor = Color.White;
btnActualizar.Image = Properties.Resources.update_icon; // Opcional: icono de actualización
btnActualizar.ImageAlign = ContentAlignment.MiddleRight;
btnActualizar.Location = new Point(0, 620);
btnActualizar.Name = "btnActualizar";
btnActualizar.Padding = new Padding(20, 0, 0, 0);
btnActualizar.Size = new Size(250, 60);
btnActualizar.TabIndex = 6;
btnActualizar.Text = "Buscar Actualización";
btnActualizar.TextAlign = ContentAlignment.MiddleLeft;
btnActualizar.UseVisualStyleBackColor = true;
btnActualizar.Click += btnActualizar_Click;
btnActualizar.MouseEnter += MenuButton_MouseEnter;
btnActualizar.MouseLeave += MenuButton_MouseLeave;

// Agregar al panel lateral
panelMenu.Controls.Add(btnActualizar);
```

### 2. Agregar el manejador de eventos en MainMenu.cs

```csharp
private void btnActualizar_Click(object sender, EventArgs e)
{
    DialogResult resultado = MessageBox.Show(
        "¿Desea buscar actualizaciones?\n\n" +
        "La aplicación se cerrará y se iniciará el actualizador.",
        "Buscar Actualizaciones",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question
    );

    if (resultado == DialogResult.Yes)
    {
        IniciarActualizador();
    }
}

private void IniciarActualizador()
{
    try
    {
        string directorioActual = Path.GetDirectoryName(
            System.Reflection.Assembly.GetExecutingAssembly().Location
        ) ?? string.Empty;

        string rutaActualizador = Path.Combine(directorioActual, "Retorno360Actualizador.exe");

        if (File.Exists(rutaActualizador))
        {
            // Iniciar el actualizador
            System.Diagnostics.Process.Start(rutaActualizador);

            // Cerrar la aplicación completamente
            System.Windows.Forms.Application.Exit();
        }
        else
        {
            MessageBox.Show(
                "No se encontró el actualizador.\n\n" +
                $"Ubicación esperada: {rutaActualizador}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            $"Error al iniciar el actualizador:\n\n{ex.Message}",
            "Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        );
    }
}
```

## Opción 2: Agregar en el menú de Configuración

Si prefieres agregar la opción de actualización dentro del formulario de configuración:

### En FrmConfiguracion.Designer.cs

```csharp
private Button btnBuscarActualizacion;

// En InitializeComponent()
btnBuscarActualizacion.Location = new Point(30, 400);
btnBuscarActualizacion.Name = "btnBuscarActualizacion";
btnBuscarActualizacion.Size = new Size(200, 40);
btnBuscarActualizacion.TabIndex = 10;
btnBuscarActualizacion.Text = "Buscar Actualización";
btnBuscarActualizacion.UseVisualStyleBackColor = true;
btnBuscarActualizacion.Click += btnBuscarActualizacion_Click;

this.Controls.Add(btnBuscarActualizacion);
```

### En FrmConfiguracion.cs

```csharp
private void btnBuscarActualizacion_Click(object sender, EventArgs e)
{
    DialogResult resultado = MessageBox.Show(
        "¿Desea buscar actualizaciones?\n\n" +
        "La aplicación se cerrará y se iniciará el actualizador.",
        "Buscar Actualizaciones",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question
    );

    if (resultado == DialogResult.Yes)
    {
        try
        {
            string directorioActual = Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location
            ) ?? string.Empty;

            string rutaActualizador = Path.Combine(directorioActual, "Retorno360Actualizador.exe");

            if (File.Exists(rutaActualizador))
            {
                System.Diagnostics.Process.Start(rutaActualizador);
                System.Windows.Forms.Application.Exit();
            }
            else
            {
                MessageBox.Show(
                    "No se encontró el actualizador.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error al iniciar el actualizador:\n\n{ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}
```

## Opción 3: Verificación automática al inicio

Para verificar actualizaciones automáticamente al iniciar la aplicación:

### En MainMenu.cs, modifica MainMenu_Load:

```csharp
private async void MainMenu_Load(object sender, EventArgs e)
{
    // ... código existente ...

    // Verificar actualizaciones en segundo plano (opcional)
    await VerificarActualizacionesDisponibles();
}

private async Task VerificarActualizacionesDisponibles()
{
    try
    {
        // Esto es opcional - solo verifica si hay una nueva versión
        // sin descargar automáticamente

        var client = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("Retorno360Tacna"));
        var releases = await client.Repository.Release.GetAll("Javier-Nieto23", "Retorno360Tacna");

        if (releases.Count > 0)
        {
            var latestRelease = releases[0];
            var versionActual = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            // Comparar versiones y notificar si hay una nueva disponible
            // (Implementación depende de cómo manejas las versiones)
        }
    }
    catch
    {
        // Ignorar errores de verificación silenciosamente
    }
}
```

## Distribución

Al publicar tu aplicación, asegúrate de incluir:

1. `Retorno360Tacna.exe` (aplicación principal)
2. `Retorno360Actualizador.exe` (actualizador)
3. Todas las DLLs necesarias para ambos ejecutables
4. `Octokit.dll` (requerida por el actualizador)

## Notas Importantes

- El actualizador debe estar en la **misma carpeta** que el ejecutable principal
- La aplicación principal debe **cerrarse completamente** antes de actualizar
- El actualizador **NO se actualiza a sí mismo** durante el proceso
- Se recomienda distribuir el actualizador junto con cada versión

## Estructura de Carpetas de Distribución

```
Retorno360/
├── Retorno360Tacna.exe
├── Retorno360Actualizador.exe
├── Microsoft.Data.SqlClient.dll
├── Octokit.dll
├── QuestPDF.dll
├── SkiaSharp.dll
├── LiveChartsCore.dll
└── (otras DLLs necesarias)
```
