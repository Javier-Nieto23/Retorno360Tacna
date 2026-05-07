# Script para publicar Retorno360Actualizador
# Este script compila y publica el actualizador en modo Release

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Publicando Retorno360Actualizador" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Rutas
$projectPath = "C:\Users\jnieto\source\repos\Retorno360Tacna\Retorno360Actualizador\Retorno360Actualizador.csproj"
$outputPath = "C:\Users\jnieto\source\repos\Retorno360Tacna\Retorno360Actualizador\publish"

# Limpiar carpeta de publicación anterior
if (Test-Path $outputPath) {
    Write-Host "Limpiando carpeta de publicación anterior..." -ForegroundColor Yellow
    Remove-Item -Path $outputPath -Recurse -Force
}

# Publicar el proyecto
Write-Host "Compilando y publicando el actualizador..." -ForegroundColor Green
dotnet publish $projectPath -c Release -o $outputPath --self-contained false

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  Publicación exitosa!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Ubicación: $outputPath" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Archivos generados:" -ForegroundColor Yellow
    Get-ChildItem -Path $outputPath -File | Select-Object Name, @{Name="Tamaño (KB)"; Expression={[math]::Round($_.Length/1KB, 2)}} | Format-Table -AutoSize

    Write-Host ""
    Write-Host "Para distribuir el actualizador:" -ForegroundColor Yellow
    Write-Host "1. Copia Retorno360Actualizador.exe al directorio de instalación" -ForegroundColor White
    Write-Host "2. Asegúrate de incluir todos los archivos .dll necesarios" -ForegroundColor White
} else {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "  Error en la publicación" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
}

Write-Host ""
Write-Host "Presiona cualquier tecla para continuar..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
