# Script para crear ZIP del Release v1.3.0
# Ejecutar desde PowerShell en la raíz del proyecto

param(
    [string]$VersionTag = "v1.3.0"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Retorno360 - Creador de Release ZIP" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Rutas
$ProjectRoot = "C:\Users\jnieto\source\repos\Retorno360Tacna"
$PublishFolder = Join-Path $ProjectRoot "Release"
$ZipFileName = "Retorno360Tacna-$VersionTag.zip"
$ZipPath = Join-Path $ProjectRoot $ZipFileName

Write-Host "📁 Carpeta de publicación: $PublishFolder" -ForegroundColor Yellow
Write-Host "📦 Archivo ZIP: $ZipFileName" -ForegroundColor Yellow
Write-Host ""

# Verificar que existe la carpeta de publicación
if (-Not (Test-Path $PublishFolder)) {
    Write-Host "❌ Error: La carpeta de publicación no existe." -ForegroundColor Red
    Write-Host "   Ejecuta primero: dotnet publish -c Release" -ForegroundColor Yellow
    exit 1
}

# Eliminar ZIP anterior si existe
if (Test-Path $ZipPath) {
    Write-Host "🗑️  Eliminando ZIP anterior..." -ForegroundColor Gray
    Remove-Item $ZipPath -Force
}

# Crear el ZIP
Write-Host "📦 Creando ZIP..." -ForegroundColor Green

try {
    # Comprimir la carpeta Release
    Compress-Archive -Path "$PublishFolder\*" -DestinationPath $ZipPath -CompressionLevel Optimal

    $ZipSize = (Get-Item $ZipPath).Length / 1MB

    Write-Host ""
    Write-Host "✅ ZIP creado exitosamente!" -ForegroundColor Green
    Write-Host "   Archivo: $ZipFileName" -ForegroundColor Cyan
    Write-Host "   Tamaño: $([math]::Round($ZipSize, 2)) MB" -ForegroundColor Cyan
    Write-Host "   Ubicación: $ZipPath" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "📋 Próximos pasos:" -ForegroundColor Yellow
    Write-Host "   1. Ir a: https://github.com/Javier-Nieto23/Retorno360Tacna/releases" -ForegroundColor White
    Write-Host "   2. Clic en 'Draft a new release'" -ForegroundColor White
    Write-Host "   3. Tag version: $VersionTag" -ForegroundColor White
    Write-Host "   4. Adjuntar el archivo: $ZipFileName" -ForegroundColor White
    Write-Host "   5. Publicar el release" -ForegroundColor White
    Write-Host ""

    # Abrir la carpeta donde está el ZIP
    Write-Host "🔍 Abriendo ubicación del ZIP..." -ForegroundColor Gray
    Start-Process "explorer.exe" -ArgumentList "/select,`"$ZipPath`""

} catch {
    Write-Host ""
    Write-Host "❌ Error al crear el ZIP:" -ForegroundColor Red
    Write-Host "   $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Proceso completado" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
