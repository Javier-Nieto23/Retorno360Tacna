# Retorno360 Tacna - Sistema de Gestión de Retorno Aduanero

## 📋 Descripción General del Sistema

**Retorno360 Tacna** es un sistema integral desarrollado en .NET 10 WinForms para la gestión y análisis del porcentaje de retorno aduanero en operaciones de comercio exterior. El sistema permite calcular, visualizar y generar reportes detallados de IGI (Impuesto General a las Importaciones) e IVA para empresas que operan bajo el régimen de retorno en Tacna, México.

### 🎯 Objetivo Principal

Facilitar el cálculo automático del porcentaje de retorno comparando los valores de importación versus exportación, validando pedimentos aduaneros y generando reportes ejecutivos con visualizaciones gráficas interactivas.

---

## 🏗️ Arquitectura del Sistema

### Componentes Principales

```
Retorno360Tacna/
├── FORMS/                      # Interfaz de Usuario (WinForms)
│   ├── Login.cs               # Pantalla de autenticación
│   ├── MainMenu.cs            # Menú principal
│   ├── FrmRetorno.cs          # Cálculo de porcentaje de retorno
│   ├── FrmReportes.cs         # Reportes de IGI/IVA
│   ├── FrmSeleccionRazon.cs   # Selección de razón social
│   ├── FrmConfiguracion.cs    # Configuración del sistema
│   ├── CargaDePantalla.cs     # Pantalla de carga
│   └── DiagramasOperacion.cs  # Diagramas operacionales
│
├── SERVICES/                   # Lógica de Negocio
│   ├── LoginService.cs        # Autenticación de usuarios
│   ├── RetornoService.cs      # Cálculo de retorno
│   ├── ReporteIGIService.cs   # Generación de reportes IGI
│   ├── PdfGeneradorService.cs # Exportación a PDF
│   └── ConfiguracionService.cs # Gestión de configuración
│
├── MODELS/                     # Modelos de Datos
│   ├── Usuario.cs             # Modelo de usuario
│   ├── RazonSocial.cs         # Razón social
│   ├── ConexionInfo.cs        # Información de conexión
│   ├── ReporteIGI.cs          # Datos de reporte IGI
│   └── ConfiguracionUsuario.cs # Configuración de usuario
│
├── CNX/                        # Capa de Conexión
│   └── Conexion.cs            # Gestión de conexiones SQL
│
└── Properties/                 # Recursos y configuración
    └── Resources.resx         # Recursos embebidos
```

---

## ⚙️ Tecnologías Utilizadas

### Framework y Lenguaje
- **.NET 10.0** (net10.0-windows)
- **C# 12** con WinForms
- **Target Platform**: Windows (x64/x86)

### Dependencias NuGet

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| **Microsoft.Data.SqlClient** | 7.0.0 | Conexión a SQL Server |
| **LiveChartsCore.SkiaSharpView.WinForms** | 2.0.0-rc4 | Gráficas interactivas con zoom |
| **SkiaSharp** | 3.0.0 | Renderizado de gráficas |
| **QuestPDF** | 2024.12.3 | Generación de reportes PDF |

---

## 🔑 Funcionalidades Principales

### 1. **Módulo de Autenticación**
- Login con validación de usuarios
- Gestión de permisos y roles
- Sesión persistente

### 2. **Cálculo de Retorno (FrmRetorno)**
- Cálculo automático del porcentaje de retorno
- Validación de pedimentos de importación/exportación
- Comparación de valores comerciales
- Gráficas interactivas:
  - **Gráfica de columnas**: Importaciones vs Exportaciones con zoom
  - **Gráfica circular**: Distribución porcentual
- Navegación entre gráficas con botones de flecha
- Exportación a PDF

**Características de Zoom:**
- Scroll del mouse para acercar/alejar
- Arrastrar para navegar cuando está con zoom
- Doble clic para resetear vista
- Labels de datos escalables

### 3. **Reportes de IGI/IVA (FrmReportes)**
- Reportes detallados por forma de pago
- Separación IGI (Formas de Pago 5 y 0)
- Separación IVA (Formas de Pago 21 y 0)
- **Gráficas horizontales apiladas** por mes y forma de pago con zoom
- Navegación entre gráfica IGI y gráfica IVA
- Tablas con totales y resúmenes
- Exportación a PDF con:
  - Resumen ejecutivo
  - Gráficas IGI e IVA generadas
  - Tablas detalladas por mes
  - Totales por forma de pago

**Estructura de Gráficas:**
- IGI: Barras apiladas (Pagado, Calculado, Diferencia)
- IVA: Barras por mes y forma de pago
- Etiquetas con formato compacto (K/M)
- Leyendas descriptivas

### 4. **Generación de PDF**
- Reportes ejecutivos profesionales
- Gráficas embebidas en alta resolución
- Tablas con formato condicional
- Totales y resúmenes destacados
- Pie de página con numeración

### 5. **Configuración del Sistema**
- Gestión de conexiones a base de datos
- Configuración de parámetros
- Respaldo y restauración de configuración

---

## 🚀 Instalación y Despliegue

### Requisitos Previos

#### Sistema Operativo
- Windows 10 (20H2) o superior
- Windows 11
- Windows Server 2019 o superior

#### Software Requerido
- **.NET 10 Runtime** (Desktop Runtime x64/x86)
  - Se instalará automáticamente si no está presente
- **SQL Server** (versión compatible con el cliente)
  - SQL Server 2016 o superior
  - SQL Server Express también es compatible

#### Hardware Mínimo
- **Procesador**: Intel Core i3 o equivalente
- **RAM**: 4 GB (recomendado 8 GB)
- **Disco**: 500 MB de espacio libre
- **Pantalla**: 1280x720 o superior

---

## 📦 Instalación con Advanced Installer

### Paso 1: Descargar el Instalador

🔗 **[Descargar Instalador de Retorno360 Tacna](https://digizen.tacna.net/index.php/s/NqeekQR2MrtkH3x)**

> **Nota**: El instalador está firmado digitalmente y puede requerir permisos de administrador.

### Paso 2: Ejecutar el Instalador

1. **Ejecutar** el archivo descargado: `Retorno360Tacna-Setup.exe` o `Retorno360Tacna-Setup.msi`

2. **Aceptar** el control de cuentas de usuario (UAC) si aparece

3. **Bienvenida**: Clic en "Siguiente"

   ![Welcome Screen](docs/images/install-welcome.png)

4. **Acuerdo de Licencia**: 
   - Leer el acuerdo de licencia
   - Marcar "Acepto los términos"
   - Clic en "Siguiente"

5. **Ubicación de Instalación**:
   - Ubicación predeterminada: `C:\Program Files\Retorno360Tacna\`
   - Para cambiar, clic en "Examinar"
   - Clic en "Siguiente"

   ```
   Espacio requerido: ~200 MB
   Espacio disponible: [Verificar automáticamente]
   ```

6. **Componentes a Instalar**:
   - ✅ **Aplicación Principal** (obligatorio)
   - ✅ **Acceso directo en Escritorio** (opcional)
   - ✅ **Acceso directo en Menú Inicio** (recomendado)
   - ✅ **.NET 10 Runtime** (se instalará si no está presente)
   - Clic en "Siguiente"

7. **Listo para Instalar**:
   - Revisar resumen de instalación
   - Clic en "Instalar"

8. **Instalación en Progreso**:
   ```
   ⏳ Copiando archivos...
   ⏳ Registrando componentes...
   ⏳ Creando accesos directos...
   ⏳ Instalando .NET 10 Runtime (si es necesario)...
   ```

9. **Instalación Completada**:
   - ✅ Marcar "Ejecutar Retorno360 Tacna" (opcional)
   - Clic en "Finalizar"

---

## 📖 Guía de Uso

### Calcular Porcentaje de Retorno

1. **Abrir FrmRetorno** desde el menú principal
2. **Seleccionar Razón Social** del combo
3. **Seleccionar Base de Datos** (o marcar "Calcular por Razón Social" para todas)
4. **Configurar fechas**: Inicio y Fin del período
5. **Opciones**:
   - ☑ Incluir Materia Prima
   - ☑ Forzar cálculo (omitir validaciones)
6. Clic en **"Calcular Retorno"**
7. **Resultados**:
   - Valor importado
   - Valor exportado
   - Porcentaje de retorno
   - Cantidad de pedimentos
8. **Visualizar Gráficas**:
   - Usar botones ◀ ▶ para cambiar entre gráficas
   - **Scroll** para hacer zoom
   - **Arrastrar** para navegar
   - **Doble clic** para resetear
9. Clic en **"Generar PDF"** para exportar

### Generar Reportes de IGI/IVA

1. **Abrir FrmReportes**
2. **Seleccionar Razón Social** y **Cliente**
3. **Configurar fechas** del reporte
4. Clic en **"Generar Reporte"**
5. **Visualizar datos**:
   - Panel resumen con totales por forma de pago
   - Tabla IGI separada por forma de pago
   - Tabla IVA separada por forma de pago
6. **Gráficas interactivas**:
   - Gráfica IGI (Forma de pago 5 y 0)
   - Gráfica IVA (Forma de pago 21 y 0)
   - Usar ◀ ▶ para cambiar entre gráficas
   - **Zoom habilitado** con scroll
7. Clic en **"Exportar a PDF"**

---

## 🐛 Solución de Problemas Comunes

### Error de Conexión a Base de Datos

**Problema**: "No se puede conectar al servidor SQL"

**Solución**:
1. Verificar que el servicio SQL Server esté activo
2. Comprobar firewall (puerto 1433 abierto)
3. Validar credenciales de usuario SQL
4. Revisar nombre del servidor (puede requerir instancia: `servidor\instancia`)

### Error: ".NET Runtime no encontrado"

**Problema**: "La aplicación requiere .NET 10"

**Solución**:
1. El instalador debería instalar .NET 10 automáticamente
2. Si falla, descargar manualmente desde:
   - [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)
3. Instalar y reiniciar la aplicación

### Gráficas no se Visualizan

**Problema**: Las gráficas aparecen en blanco o con errores

**Solución**:
1. Verificar que haya datos en el período seleccionado
2. Actualizar drivers gráficos
3. Reiniciar la aplicación
4. Verificar que SkiaSharp esté correctamente instalado (el instalador lo incluye)

### PDF no se Genera

**Problema**: Error al exportar a PDF

**Solución**:
1. Verificar permisos de escritura en la carpeta de destino
2. Asegurar que la ruta no tenga caracteres especiales
3. Cerrar cualquier PDF abierto del mismo nombre
4. Verificar espacio en disco

### Botones de Navegación no Visibles

**Problema**: Los botones ◀ ▶ no aparecen en las gráficas

**Solución**:
1. Verificar que los paneles de gráficas estén inicializados
2. Ajustar resolución de pantalla (mínimo 1280x720)
3. Reiniciar la aplicación
4. Actualizar a la última versión

---

## 🔄 Actualizaciones y Versiones

### Versión Actual: 1.5.0

**Fecha de Lanzamiento**: Enero 2025

#### ✨ Nuevas Características

1. **Zoom Interactivo en Gráficas**
   - Implementado zoom con scroll del mouse
   - Pan/arrastrar cuando está con zoom
   - Doble clic para resetear vista
   - Labels escalables con zoom

2. **Navegación Mejorada entre Gráficas**
   - Botones de flecha en ambos paneles de gráficas
   - Transiciones suaves entre IGI e IVA
   - Títulos descriptivos con numeración (1/2, 2/2)

3. **Mejoras en Reportes PDF**
   - Gráficas de alta resolución embebidas
   - Formato compacto para valores grandes (K/M)
   - Mejor organización de tablas
   - Sección de resumen ejecutivo

4. **Optimización de UI**
   - Mejor espaciado entre controles
   - Panel de resultados reorganizado
   - Etiquetas de pedimentos dentro del groupBox
   - Gráficas más grandes y visibles

#### 🐛 Correcciones de Bugs

- ✅ Botones de navegación ahora visibles en todas las gráficas
- ✅ Zoom funciona correctamente en gráficas de columnas
- ✅ PDF genera correctamente ambas gráficas (IGI e IVA)
- ✅ Espaciado mejorado en FrmRetorno
- ✅ Ejes de gráficas con límites dinámicos

#### 🔧 Mejoras de Rendimiento

- Optimización en generación de gráficas para PDF
- Reducción de tiempo de carga de reportes grandes
- Mejor manejo de memoria en zoom de gráficas

---

## 🆕 Fix de Última Versión (v1.5.0)

### Aplicar Actualización Manual

Si ya tienes una versión anterior instalada y necesitas actualizar manualmente:

#### Opción 1: Instalador Automático (Recomendado)

1. **Descargar** la última versión:
   - 🔗 [Retorno360 Tacna v1.5.0](https://digizen.tacna.net/index.php/s/NqeekQR2MrtkH3x)

2. **Ejecutar** el instalador
   - El instalador detectará la versión anterior
   - Seleccionar "Actualizar instalación existente"

3. **Completar** la actualización
   - Los datos y configuraciones se preservarán
   - Se recomienda hacer backup de la configuración antes

#### Opción 2: Actualización Manual

Si el instalador automático falla:

1. **Backup de Configuración**:
   ```
   C:\Users\[Usuario]\AppData\Local\Retorno360Tacna\
   └── config.json (guardar copia)
   ```

2. **Desinstalar** versión anterior:
   - Panel de Control → Programas → Desinstalar Retorno360 Tacna
   - **NO** eliminar la carpeta de configuración

3. **Instalar** nueva versión:
   - Ejecutar instalador v2.5.0
   - Seguir pasos de instalación estándar

4. **Restaurar Configuración**:
   - Copiar `config.json` de vuelta a la carpeta de configuración
   - O reconfigurar manualmente

### Verificar Versión Instalada

Para verificar la versión actual:

1. Abrir Retorno360 Tacna
2. Menú **Ayuda** → **Acerca de**
3. Verificar número de versión: `v2.5.0`

### Cambios Importantes en v2.5.0

#### ⚠️ Cambios que Requieren Acción

1. **Nueva Dependencia: .NET 10**
   - Si actualiza desde versiones anteriores con .NET 8/9
   - El instalador instalará .NET 10 automáticamente
   - No es necesaria intervención manual

2. **Configuración de Gráficas**
   - Las configuraciones de visualización de gráficas se resetearán
   - Zoom está habilitado por defecto
   - Se puede deshabilitar en configuración si se desea

3. **Estructura de PDF**
   - Los PDFs generados tendrán nuevo formato
   - Compatible con reportes anteriores
   - Mejor calidad de gráficas embebidas

#### 🔄 Migración de Datos

**No se requiere migración de base de datos**. La estructura de BD es compatible con versiones anteriores.

### Solución de Problemas Post-Actualización

#### Problema: "La aplicación no inicia después de actualizar"

**Solución**:
```powershell
# 1. Verificar instalación de .NET 10
dotnet --list-runtimes

# 2. Reinstalar .NET 10 Desktop Runtime si no aparece
# Descargar de: https://dotnet.microsoft.com/download/dotnet/10.0

# 3. Reparar instalación de Retorno360
# Panel de Control → Programas → Retorno360 Tacna → Modificar → Reparar
```

#### Problema: "Configuración perdida después de actualizar"

**Solución**:
1. Revisar carpeta de backup automático:
   ```
   C:\Users\[Usuario]\AppData\Local\Retorno360Tacna\backup\
   ```
2. Restaurar `config.json` de la fecha más reciente
3. Reiniciar aplicación

#### Problema: "Gráficas no funcionan correctamente"

**Solución**:
1. Limpiar caché de la aplicación:
   ```
   C:\Users\[Usuario]\AppData\Local\Retorno360Tacna\cache\
   ```
2. Reiniciar aplicación
3. Si persiste, reinstalar limpiamente

#### Problema: "PDFs generados son diferentes"

**Explicación**: El nuevo formato de PDF es mejorado. Para compatibilidad con formato anterior:
1. Ir a **Configuración** → **Reportes**
2. Activar "Usar formato PDF clásico"
3. Regenerar reportes

---


### GitHub Repository

🔗 **[GitHub - Retorno360Tacna](https://github.com/Javier-Nieto23/Retorno360Tacna)**

---

## 👥 Créditos

**Desarrollado por**: Javier Nieto 
**Versión**: 1.5.0  
**Última Actualización**: Mayo 2026  
**Licencia**: Propietaria

---

## 📄 Licencia

© 2026 Retorno360 Tacna. Todos los derechos reservados.

Este software es propiedad de Retorno360 y está protegido por las leyes de derechos de autor. El uso no autorizado, distribución o modificación está estrictamente prohibido.

---

## 🔐 Seguridad y Privacidad

- Todas las conexiones a base de datos están cifradas
- Las credenciales se almacenan de forma segura
- Los datos del usuario están protegidos según normativas vigentes
- Auditoría de acciones críticas del sistema

---

---

**¡Gracias por usar Retorno360 Tacna!** 🚀
