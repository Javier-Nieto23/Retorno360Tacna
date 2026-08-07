

🔗 **[Descargar Instalador de Retorno360 Tacna](https://digizen.tacna.net/index.php/s/NqeekQR2MrtkH3x)**


# Retorno360 Tacna

Aplicación de escritorio para la gestión de retorno aduanero, conciliación de IGI/IVA, inventarios y generación de reportes operativos para empresas de comercio exterior.

## Características principales

- Autenticación de usuarios y control de roles.
- Cálculo de porcentaje de retorno a partir de importaciones y exportaciones.
- Conciliación de IGI e IVA por periodo y forma de pago.
- Consulta y análisis de pedimentos, materia prima y catálogo de partes.
- Indicadores, gráficas y tablas interactivas.
- Exportación de reportes a PDF y Excel.
- Gestión de inventarios y archivos anexos.
- Publicación de resultados al portal web.

## Arquitectura

La solución está organizada por responsabilidades:

```text
Retorno360Tacna/
├── FORMS/       # Interfaz de usuario WinForms y diseñadores
├── SERVICES/    # Reglas de negocio, reportes e integraciones
├── MODELS/      # Modelos de datos y resultados de cálculo
├── CNX/         # Acceso a SQL Server
├── HELPERS/     # Utilidades de notificación, errores y UI
├── Properties/  # Recursos y configuración de aplicación
└── DOCS/        # Documentación técnica
```

### Capas

| Capa | Responsabilidad |
| --- | --- |
| Presentación | Formularios WinForms, validaciones, navegación, tablas y gráficas. |
| Servicios | Consultas, cálculos de retorno, conciliaciones, generación de archivos e integraciones. |
| Modelos | Usuarios, conexiones, pedimentos, reportes, catálogo de partes e inventario. |
| Infraestructura | SQL Server, PostgreSQL, Cloudflare R2, secretos locales y notificaciones. |

## Tecnologías

| Tecnología | Uso |
| --- | --- |
| .NET 10 / WinForms | Aplicación de escritorio para Windows. |
| C# | Lógica de presentación, negocio e integración. |
| Microsoft.Data.SqlClient | Acceso a SQL Server. |
| Npgsql | Comunicación con PostgreSQL para el portal web. |
| LiveChartsCore y SkiaSharp | Gráficas e indicadores visuales. |
| QuestPDF | Generación de reportes PDF. |
| ClosedXML y OpenXML | Lectura y exportación de archivos Excel. |
| AWS SDK S3 | Integración compatible con Cloudflare R2. |

## Módulos funcionales

### Autenticación y configuración

Administra el acceso de usuarios, roles, conexiones de base de datos y parámetros de la aplicación.

Componentes principales: `Login`, `MainMenu`, `LoginService`, `ConfiguracionService` y `SecretStoreService`.

### Retorno aduanero

Calcula el porcentaje de retorno con información de importaciones, exportaciones y pedimentos validados. Permite filtrar por razón social, base de datos y periodo.

Componentes principales: `FrmRetorno`, `RetornoService` y `CalculadoraRetorno`.

### Reportes IGI e IVA

Concilia importes pagados y calculados, agrupa datos por mes y forma de pago, y genera tablas, gráficas y reportes PDF.

Componentes principales: `FrmReportes`, `ReporteIGIService`, `ReporteServiceBase`, `ReporteIGIService_Extension` y `PdfGeneradorService`.

### Inventarios y catálogo de partes

Permite calcular y consultar inventarios, revisar materia prima/BOM y analizar indicadores por periodo.

Componentes principales: `FrmCalculoInventarios`, `FrmReportesInventario`, `FrmCatalogoPartes` y `CatalogoPartesService`.

### Archivos y portal web

Gestiona archivos anexos en Cloudflare R2 y publica resultados seleccionados en PostgreSQL para su consulta desde el portal web.

Componentes principales: `CloudflareR2Service`, `PortalWebService`, `FrmAnexos` y `FrmSolicitud`.

## Flujo de operación

1. El usuario inicia sesión.
2. Selecciona razón social, base de datos y periodo.
3. La aplicación consulta las fuentes operativas en SQL Server.
4. Los servicios consolidan, calculan y concilian los datos.
5. Los resultados se muestran en tablas y gráficas.
6. El usuario puede exportar PDF/Excel o publicar resultados al portal web.

## Requisitos

- Windows 10 o Windows 11.
- .NET 10 Desktop Runtime.
- Acceso a las bases de datos configuradas.
- Credenciales autorizadas para los servicios externos requeridos.

## Compilación

```powershell
dotnet restore Retorno360Tacna.slnx
dotnet build Retorno360Tacna.slnx --configuration Release
```

## Configuración y seguridad

No almacenes contraseñas, cadenas de conexión, claves de almacenamiento ni tokens en el repositorio. La aplicación incluye `SecretStoreService`, que usa DPAPI de Windows para proteger secretos locales del usuario.

Para ejecutar o distribuir una versión de producción:

- Configura secretos mediante variables de entorno o el almacén seguro local.
- Usa cuentas con permisos mínimos necesarios.
- Mantén actualizadas las dependencias NuGet.
- Firma los ejecutables y valida la integridad de las actualizaciones.

## Calidad y mantenimiento

Las mejoras recomendadas para la evolución del proyecto son:

- Agregar pruebas unitarias para cálculo de retorno, conciliaciones y transformaciones de datos.
- Separar gradualmente la lógica de negocio de los formularios más extensos.
- Resolver advertencias de nulabilidad y APIs obsoletas.
- Automatizar compilación, pruebas, análisis de dependencias y publicación mediante CI.

## Documentación adicional

- `CHANGELOG_v2.6.0.md`
- `CHANGELOG_v2.7.0.md`

## Licencia

Software Javier Nieto. Todos los derechos reservados.
