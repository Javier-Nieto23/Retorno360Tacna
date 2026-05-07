# ⚡ Guía Rápida - Retorno360 Actualizador

## 🎯 Inicio Rápido

### 1. Copiar el Actualizador
```
Copia estos archivos a la carpeta de tu aplicación:
- Retorno360Actualizador.exe
- Octokit.dll
```

### 2. Crear un Release en GitHub

1. **Compilar tu aplicación**:
   ```bash
   dotnet publish Retorno360Tacna.csproj -c Release -o ./release
   ```

2. **Crear ZIP**: Comprime la carpeta `release` → `Retorno360-v1.0.0.zip`

3. **Subir a GitHub**:
   - Ve a: https://github.com/Javier-Nieto23/Retorno360Tacna/releases
   - Click "Create a new release"
   - Tag: `v1.0.0`
   - Título: `Versión 1.0.0`
   - Arrastra el archivo ZIP
   - Click "Publish release"

### 3. Integrar en tu Aplicación

Agrega un botón en `MainMenu`:

```csharp
private void btnActualizar_Click(object sender, EventArgs e)
{
    var result = MessageBox.Show(
        "¿Buscar actualizaciones? Se cerrará la aplicación.",
        "Actualizar",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question
    );

    if (result == DialogResult.Yes)
    {
        var actualizador = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Retorno360Actualizador.exe"
        );

        if (File.Exists(actualizador))
        {
            Process.Start(actualizador);
            Application.Exit();
        }
    }
}
```

## ✅ Checklist de Distribución

- [ ] Copiar `Retorno360Actualizador.exe` a la carpeta de instalación
- [ ] Copiar `Octokit.dll` a la carpeta de instalación
- [ ] Crear release en GitHub con archivo ZIP
- [ ] Agregar botón de actualización en el menú
- [ ] Probar el flujo completo de actualización

## 🔄 Flujo de Uso

1. Usuario hace click en "Buscar Actualización"
2. Aplicación principal se cierra
3. Actualizador se abre automáticamente
4. Se conecta a GitHub
5. Descarga última versión
6. Instala archivos
7. Usuario hace click en "Finalizar"
8. Usuario vuelve a abrir la aplicación

## 📁 Estructura Final

```
C:\Retorno360\
├── Retorno360Tacna.exe           ← Tu aplicación
├── Retorno360Actualizador.exe    ← Actualizador ✨
├── Octokit.dll                   ← Requerido ✨
├── (todas tus otras DLLs)
```

## 🛠️ Comandos Útiles

**Compilar actualizador**:
```bash
dotnet build Retorno360Actualizador.csproj -c Release
```

**Publicar actualizador**:
```bash
dotnet publish Retorno360Actualizador.csproj -c Release -o ./publish
```

**O usar el script PowerShell**:
```powershell
.\publish.ps1
```

## 💡 Tips

- El actualizador **debe estar en la misma carpeta** que tu aplicación
- **Cerrar completamente** la aplicación antes de actualizar
- El ZIP debe contener **todos** los archivos necesarios
- Marcar el release como **"Latest"** en GitHub

## 🚨 Solución de Problemas Comunes

| Problema | Solución |
|----------|----------|
| "No se encontró el actualizador" | Verificar que `Retorno360Actualizador.exe` esté en la misma carpeta |
| "No hay versiones disponibles" | Crear al menos un release en GitHub |
| "No se encontró archivo ZIP" | Subir un archivo .zip al release |
| "Error de conexión" | Verificar conexión a Internet |

## 📞 ¿Necesitas Ayuda?

Consulta los archivos de documentación completa:
- `RESUMEN.md` - Resumen completo del proyecto
- `README.md` - Documentación técnica detallada
- `INTEGRACION.md` - Guía de integración paso a paso

---

**¡Listo para usar!** 🚀
