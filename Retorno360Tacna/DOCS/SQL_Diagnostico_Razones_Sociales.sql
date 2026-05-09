-- =========================================================
-- SCRIPT DE DIAGNÓSTICO: Validación de Razones Sociales
-- =========================================================
-- Propósito: Identificar inconsistencias en la asociación
--            de bases de datos con razones sociales
-- Base de datos: RetornoMaster
-- =========================================================

USE RetornoMaster;
GO

PRINT '===================================================';
PRINT '🔍 DIAGNÓSTICO DE RAZONES SOCIALES Y BASES DE DATOS';
PRINT '===================================================';
PRINT '';

-- =========================================================
-- 1. VERIFICAR BASES DE DATOS HUÉRFANAS
-- =========================================================
PRINT '1️⃣ Bases en NOM_TABLARAZON sin correspondencia en RAZONXTABLA:';
PRINT '--------------------------------------------------------------';

SELECT 
    NT.IdTabla,
    NT.IdRazon,
    NT.NOMBRE_TABLA AS BaseDatos,
    NT.ConnExterna,
    NT.IdConexion,
    '❌ HUÉRFANA' AS Estado
FROM NOM_TABLARAZON NT
LEFT JOIN RAZONXTABLA R ON R.IdRazon = NT.IdRazon
WHERE R.IdRazon IS NULL;

PRINT '';

-- =========================================================
-- 2. VERIFICAR BASES DE DATOS DUPLICADAS
-- =========================================================
PRINT '2️⃣ Bases de datos asignadas a MÚLTIPLES razones sociales:';
PRINT '-----------------------------------------------------------';

SELECT 
    NT.NOMBRE_TABLA AS BaseDatos,
    COUNT(DISTINCT NT.IdRazon) AS CantidadRazones,
    STRING_AGG(CAST(NT.IdRazon AS VARCHAR), ', ') AS IdsRazon,
    '⚠️ DUPLICADA' AS Estado
FROM NOM_TABLARAZON NT
GROUP BY NT.NOMBRE_TABLA
HAVING COUNT(DISTINCT NT.IdRazon) > 1
ORDER BY CantidadRazones DESC, NT.NOMBRE_TABLA;

PRINT '';

-- =========================================================
-- 3. VERIFICAR CASO ESPECÍFICO: SEERT_Jlo
-- =========================================================
PRINT '3️⃣ Análisis específico de la base "SEERT_Jlo":';
PRINT '------------------------------------------------';

SELECT 
    NT.IdRazon,
    R.NOMBRE_RAZON,
    NT.NOMBRE_TABLA AS BaseDatos,
    R.DB AS BaseDatosGlosa,
    NT.ConnExterna,
    NT.IdConexion,
    CASE 
        WHEN R.NOMBRE_RAZON IS NULL THEN '❌ Sin razón social asociada'
        ELSE '✅ OK'
    END AS Estado
FROM NOM_TABLARAZON NT
LEFT JOIN RAZONXTABLA R ON R.IdRazon = NT.IdRazon
WHERE NT.NOMBRE_TABLA LIKE '%SEERT_Jlo%'
ORDER BY NT.IdRazon;

PRINT '';

-- =========================================================
-- 4. VERIFICAR TODAS LAS RAZONES SOCIALES
-- =========================================================
PRINT '4️⃣ Resumen de razones sociales y sus bases de datos:';
PRINT '-------------------------------------------------------';

SELECT 
    R.IdRazon,
    R.NOMBRE_RAZON AS RazonSocial,
    R.DB AS BaseDatosGlosa,
    COUNT(NT.NOMBRE_TABLA) AS CantidadBases,
    STRING_AGG(NT.NOMBRE_TABLA, ', ') WITHIN GROUP (ORDER BY NT.NOMBRE_TABLA) AS BasesDatos
FROM RAZONXTABLA R
LEFT JOIN NOM_TABLARAZON NT ON NT.IdRazon = R.IdRazon
GROUP BY R.IdRazon, R.NOMBRE_RAZON, R.DB
ORDER BY R.NOMBRE_RAZON;

PRINT '';

-- =========================================================
-- 5. VERIFICAR BASES CON VALORES NULOS
-- =========================================================
PRINT '5️⃣ Bases de datos con valores nulos o vacíos:';
PRINT '-----------------------------------------------';

SELECT 
    NT.IdTabla,
    NT.IdRazon,
    R.NOMBRE_RAZON,
    NT.NOMBRE_TABLA,
    CASE 
        WHEN NT.NOMBRE_TABLA IS NULL THEN '❌ NULL'
        WHEN LTRIM(RTRIM(NT.NOMBRE_TABLA)) = '' THEN '❌ VACÍO'
        ELSE '✅ OK'
    END AS EstadoNombreTabla,
    NT.ConnExterna,
    NT.IdConexion
FROM NOM_TABLARAZON NT
LEFT JOIN RAZONXTABLA R ON R.IdRazon = NT.IdRazon
WHERE NT.NOMBRE_TABLA IS NULL 
   OR LTRIM(RTRIM(NT.NOMBRE_TABLA)) = '';

PRINT '';

-- =========================================================
-- 6. VERIFICAR CONEXIONES EXTERNAS
-- =========================================================
PRINT '6️⃣ Bases con conexión externa sin IdConexion válido:';
PRINT '--------------------------------------------------------';

SELECT 
    NT.IdTabla,
    NT.IdRazon,
    R.NOMBRE_RAZON,
    NT.NOMBRE_TABLA AS BaseDatos,
    NT.ConnExterna,
    NT.IdConexion,
    C.NombreConexion,
    CASE 
        WHEN NT.ConnExterna = 'S' AND NT.IdConexion IS NULL THEN '❌ Sin IdConexion'
        WHEN NT.ConnExterna = 'S' AND C.IdConexion IS NULL THEN '❌ Conexión no existe'
        ELSE '✅ OK'
    END AS Estado
FROM NOM_TABLARAZON NT
LEFT JOIN RAZONXTABLA R ON R.IdRazon = NT.IdRazon
LEFT JOIN Conexiones C ON NT.IdConexion = C.IdConexion
WHERE NT.ConnExterna = 'S' 
  AND (NT.IdConexion IS NULL OR C.IdConexion IS NULL);

PRINT '';

-- =========================================================
-- 7. QUERY CORREGIDO vs ORIGINAL
-- =========================================================
PRINT '7️⃣ Comparación de resultados: Query Original vs Query Corregido';
PRINT '------------------------------------------------------------------';

DECLARE @IdRazonPrueba INT;

-- Buscar el IdRazon que contiene SEERT_Jlo
SELECT TOP 1 @IdRazonPrueba = IdRazon 
FROM NOM_TABLARAZON 
WHERE NOMBRE_TABLA LIKE '%SEERT_Jlo%';

IF @IdRazonPrueba IS NOT NULL
BEGIN
    PRINT 'Probando con IdRazon = ' + CAST(@IdRazonPrueba AS VARCHAR);
    PRINT '';

    PRINT '❌ Query ORIGINAL (con bug):';
    PRINT '------------------------------';
    SELECT NOMBRE_TABLA AS BaseDatos
    FROM NOM_TABLARAZON 
    WHERE IdRazon = @IdRazonPrueba 
    ORDER BY NOMBRE_TABLA;

    PRINT '';
    PRINT '✅ Query CORREGIDO (con validación):';
    PRINT '-------------------------------------';
    SELECT DISTINCT NT.NOMBRE_TABLA AS BaseDatos
    FROM NOM_TABLARAZON NT
    INNER JOIN RAZONXTABLA R ON R.IdRazon = NT.IdRazon
    WHERE NT.IdRazon = @IdRazonPrueba 
      AND NT.NOMBRE_TABLA IS NOT NULL
    ORDER BY NT.NOMBRE_TABLA;
END
ELSE
BEGIN
    PRINT 'No se encontró SEERT_Jlo en la base de datos';
END

PRINT '';
PRINT '===================================================';
PRINT '✅ DIAGNÓSTICO COMPLETADO';
PRINT '===================================================';

GO
