-- =========================================================
-- SCRIPT DE LIMPIEZA: Corrección de Datos Inconsistentes
-- =========================================================
-- Propósito: Corregir inconsistencias en NOM_TABLARAZON
--            que causan que bases aparezcan en razones
--            sociales incorrectas
-- Base de datos: RetornoMaster
-- =========================================================
-- ⚠️ ADVERTENCIA: Este script modifica datos.
--    Ejecutar primero en un ambiente de prueba.
--    Hacer un backup antes de ejecutar en producción.
-- =========================================================

USE RetornoMaster;
GO

PRINT '===================================================';
PRINT '🔧 LIMPIEZA DE DATOS: Razones Sociales';
PRINT '===================================================';
PRINT '';

-- =========================================================
-- PASO 0: CREAR BACKUP DE SEGURIDAD
-- =========================================================
PRINT '📦 Paso 0: Creando tabla de respaldo...';
PRINT '--------------------------------------------';

-- Crear tabla de backup si no existe
IF OBJECT_ID('NOM_TABLARAZON_BACKUP', 'U') IS NOT NULL
    DROP TABLE NOM_TABLARAZON_BACKUP;

SELECT * 
INTO NOM_TABLARAZON_BACKUP
FROM NOM_TABLARAZON;

DECLARE @CantidadBackup INT = (SELECT COUNT(*) FROM NOM_TABLARAZON_BACKUP);
PRINT '✅ Backup creado: ' + CAST(@CantidadBackup AS VARCHAR) + ' registros respaldados en NOM_TABLARAZON_BACKUP';
PRINT '';

-- =========================================================
-- PASO 1: ELIMINAR REGISTROS HUÉRFANOS
-- =========================================================
PRINT '🗑️ Paso 1: Eliminando registros huérfanos (sin razón social válida)...';
PRINT '--------------------------------------------------------------------------';

-- Primero mostrar cuántos registros se eliminarán
DECLARE @HuerfanosCount INT;

SELECT @HuerfanosCount = COUNT(*)
FROM NOM_TABLARAZON NT
LEFT JOIN RAZONXTABLA R ON R.IdRazon = NT.IdRazon
WHERE R.IdRazon IS NULL;

IF @HuerfanosCount > 0
BEGIN
    PRINT 'Se encontraron ' + CAST(@HuerfanosCount AS VARCHAR) + ' registros huérfanos:';

    SELECT 
        NT.IdTabla,
        NT.IdRazon,
        NT.NOMBRE_TABLA AS BaseDatos
    FROM NOM_TABLARAZON NT
    LEFT JOIN RAZONXTABLA R ON R.IdRazon = NT.IdRazon
    WHERE R.IdRazon IS NULL;

    -- Eliminar registros huérfanos
    DELETE NT
    FROM NOM_TABLARAZON NT
    LEFT JOIN RAZONXTABLA R ON R.IdRazon = NT.IdRazon
    WHERE R.IdRazon IS NULL;

    PRINT '✅ ' + CAST(@HuerfanosCount AS VARCHAR) + ' registros huérfanos eliminados';
END
ELSE
BEGIN
    PRINT '✅ No se encontraron registros huérfanos';
END

PRINT '';

-- =========================================================
-- PASO 2: ELIMINAR REGISTROS CON NOMBRE_TABLA NULO O VACÍO
-- =========================================================
PRINT '🗑️ Paso 2: Eliminando registros con NOMBRE_TABLA nulo o vacío...';
PRINT '---------------------------------------------------------------------';

DECLARE @NulosCount INT;

SELECT @NulosCount = COUNT(*)
FROM NOM_TABLARAZON
WHERE NOMBRE_TABLA IS NULL 
   OR LTRIM(RTRIM(NOMBRE_TABLA)) = '';

IF @NulosCount > 0
BEGIN
    PRINT 'Se encontraron ' + CAST(@NulosCount AS VARCHAR) + ' registros con nombre nulo/vacío:';

    SELECT IdTabla, IdRazon, NOMBRE_TABLA
    FROM NOM_TABLARAZON
    WHERE NOMBRE_TABLA IS NULL 
       OR LTRIM(RTRIM(NOMBRE_TABLA)) = '';

    -- Eliminar registros con nombre nulo o vacío
    DELETE FROM NOM_TABLARAZON
    WHERE NOMBRE_TABLA IS NULL 
       OR LTRIM(RTRIM(NOMBRE_TABLA)) = '';

    PRINT '✅ ' + CAST(@NulosCount AS VARCHAR) + ' registros con nombre nulo/vacío eliminados';
END
ELSE
BEGIN
    PRINT '✅ No se encontraron registros con nombre nulo/vacío';
END

PRINT '';

-- =========================================================
-- PASO 3: IDENTIFICAR DUPLICADOS (NO ELIMINAR AUTOMÁTICAMENTE)
-- =========================================================
PRINT '⚠️ Paso 3: Identificando bases de datos DUPLICADAS en múltiples razones...';
PRINT '-----------------------------------------------------------------------------';

IF OBJECT_ID('tempdb..#Duplicados') IS NOT NULL
    DROP TABLE #Duplicados;

-- Crear tabla temporal con duplicados
SELECT 
    NT.NOMBRE_TABLA AS BaseDatos,
    COUNT(DISTINCT NT.IdRazon) AS CantidadRazones
INTO #Duplicados
FROM NOM_TABLARAZON NT
GROUP BY NT.NOMBRE_TABLA
HAVING COUNT(DISTINCT NT.IdRazon) > 1;

DECLARE @DuplicadosCount INT = (SELECT COUNT(*) FROM #Duplicados);

IF @DuplicadosCount > 0
BEGIN
    PRINT '⚠️ ATENCIÓN: Se encontraron ' + CAST(@DuplicadosCount AS VARCHAR) + ' bases duplicadas:';
    PRINT '';

    SELECT 
        D.BaseDatos,
        D.CantidadRazones,
        STRING_AGG(CAST(NT.IdRazon AS VARCHAR) + ' - ' + ISNULL(R.NOMBRE_RAZON, 'N/A'), ' | ') AS RazonesSociales
    FROM #Duplicados D
    INNER JOIN NOM_TABLARAZON NT ON NT.NOMBRE_TABLA = D.BaseDatos
    LEFT JOIN RAZONXTABLA R ON R.IdRazon = NT.IdRazon
    GROUP BY D.BaseDatos, D.CantidadRazones
    ORDER BY D.BaseDatos;

    PRINT '';
    PRINT '⚠️ ACCIÓN REQUERIDA: Revisar manualmente estos duplicados';
    PRINT '   y eliminar los registros incorrectos con el siguiente comando:';
    PRINT '';
    PRINT '   DELETE FROM NOM_TABLARAZON';
    PRINT '   WHERE NOMBRE_TABLA = ''[NOMBRE_BASE]'' AND IdRazon = [ID_RAZON_INCORRECTO];';
    PRINT '';
    PRINT '   Ejemplo para SEERT_Jlo:';
    PRINT '   DELETE FROM NOM_TABLARAZON';
    PRINT '   WHERE NOMBRE_TABLA = ''SEERT_Jlo'' AND IdRazon <> [ID_RAZON_CORRECTO];';
END
ELSE
BEGIN
    PRINT '✅ No se encontraron bases de datos duplicadas';
END

PRINT '';

-- =========================================================
-- PASO 4: VERIFICAR CONEXIONES EXTERNAS
-- =========================================================
PRINT '🔍 Paso 4: Verificando bases con conexión externa sin IdConexion válido...';
PRINT '-----------------------------------------------------------------------------';

DECLARE @ConexionesInvalidasCount INT;

SELECT @ConexionesInvalidasCount = COUNT(*)
FROM NOM_TABLARAZON NT
LEFT JOIN Conexiones C ON NT.IdConexion = C.IdConexion
WHERE NT.ConnExterna = 'S' 
  AND (NT.IdConexion IS NULL OR C.IdConexion IS NULL);

IF @ConexionesInvalidasCount > 0
BEGIN
    PRINT '⚠️ Se encontraron ' + CAST(@ConexionesInvalidasCount AS VARCHAR) + ' bases con conexión externa inválida:';

    SELECT 
        NT.IdTabla,
        NT.IdRazon,
        R.NOMBRE_RAZON,
        NT.NOMBRE_TABLA AS BaseDatos,
        NT.ConnExterna,
        NT.IdConexion
    FROM NOM_TABLARAZON NT
    LEFT JOIN RAZONXTABLA R ON R.IdRazon = NT.IdRazon
    LEFT JOIN Conexiones C ON NT.IdConexion = C.IdConexion
    WHERE NT.ConnExterna = 'S' 
      AND (NT.IdConexion IS NULL OR C.IdConexion IS NULL);

    PRINT '';
    PRINT '⚠️ ACCIÓN REQUERIDA: Verificar y corregir manualmente estos registros';
END
ELSE
BEGIN
    PRINT '✅ Todas las conexiones externas tienen IdConexion válido';
END

PRINT '';

-- =========================================================
-- PASO 5: RESUMEN FINAL
-- =========================================================
PRINT '===================================================';
PRINT '📊 RESUMEN DE LIMPIEZA';
PRINT '===================================================';

DECLARE @TotalRegistros INT = (SELECT COUNT(*) FROM NOM_TABLARAZON);
DECLARE @TotalBackup INT = (SELECT COUNT(*) FROM NOM_TABLARAZON_BACKUP);
DECLARE @RegistrosEliminados INT = @TotalBackup - @TotalRegistros;

PRINT 'Registros en backup: ' + CAST(@TotalBackup AS VARCHAR);
PRINT 'Registros actuales:  ' + CAST(@TotalRegistros AS VARCHAR);
PRINT 'Registros eliminados: ' + CAST(@RegistrosEliminados AS VARCHAR);
PRINT '';

IF @DuplicadosCount > 0
BEGIN
    PRINT '⚠️ PENDIENTE: Revisar y eliminar manualmente ' + CAST(@DuplicadosCount AS VARCHAR) + ' bases duplicadas';
END

PRINT '';
PRINT '✅ LIMPIEZA COMPLETADA';
PRINT '===================================================';
PRINT '';
PRINT '💾 Para restaurar el backup en caso de error:';
PRINT '   TRUNCATE TABLE NOM_TABLARAZON;';
PRINT '   INSERT INTO NOM_TABLARAZON SELECT * FROM NOM_TABLARAZON_BACKUP;';
PRINT '';

GO
