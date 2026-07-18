# Copilot Instructions

## General Guidelines
- El sistema no debe subir archivos propios a R2; solo debe monitorear el bucket configurado y mostrar notificaciones locales de Windows o dentro de Retorno cuando detecte nuevas cargas en R2.

## Database Rules
- En la base PostgreSQL del portal web debe usarse la tabla `usuarios` en lugar de `users` para datos relacionados con usuarios.
- Para el combo de vinculación de usuario web en PostgreSQL debe usarse la tabla `usuarios` y mostrar `id` y `alias`.

## File Handling Rules
- Para localizar archivos en `archivos_historial` al crear observaciones, no debe usarse `nombre_archivo`; debe compararse usando `nombre_almacenado`, `storage_url` y `storage_key` según la ruta del bucket R2.
- En el módulo de contabilidad R2, el año debe extraerse del inicio del nombre del archivo Excel; el formato esperado empieza por 'yyyy-MM', por ejemplo '2026-01_1784312305330.xlsx'. No se debe depender de cargar carpetas por año porque tarda demasiado; el año debe obtenerse a partir del nombre del archivo para mejorar el rendimiento.

## Project-Specific Rules
- En FrmReportes, todas las gráficas deben mostrarse en el mismo panel `panelGrafica`; no debe usarse un panel separado para la gráfica IVA.
- En FrmReportes, el botón para cambiar entre gráficas debe mantenerse visible y la gráfica IGI debe mostrarse visualmente como una sola barra compuesta por pagado + diferencia, aproximando el calculado, sin cambiar la lógica de datos.

## Architectural Guidelines
- Prefiere una arquitectura más limpia, evitando mezclar diseño visual con lógica y clases en los formularios. Se busca separar responsabilidades para evitar código espagueti.