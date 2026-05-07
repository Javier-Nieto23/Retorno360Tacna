-- ============================================================================
-- SCRIPT DE DIAGNÓSTICO: Verificar Configuración Multi-Servidor
-- ============================================================================
-- Ejecutar este script para verificar que todo está configurado correctamente
-- ============================================================================

PRINT '============================================================================'
PRINT 'DIAGNÓSTICO: Configuración de Retorno360Tacna Multi-Servidor'
PRINT '============================================================================'
PRINT ''

-- ============================================================================
-- 1. VERIFICAR CONFIGURACIÓN EN RAZONXTABLA
-- ============================================================================
PRINT '1️⃣ CONFIGURACIÓN DE RAZONES SOCIALES'
PRINT '------------------------------------------------------------'

SELECT 
    IdRazon,
    NOMBRE_RAZON,
    DB as BaseDatosOrigen,
    '❓ Verificar en qué servidor está esta base' as Nota
FROM RAZONXTABLA
ORDER BY IdRazon

PRINT ''
PRINT ''

-- ============================================================================
-- 2. VERIFICAR BASES DE DATOS DISPONIBLES POR RAZÓN
-- ============================================================================
PRINT '2️⃣ BASES DE DATOS DISPONIBLES POR RAZÓN SOCIAL'
PRINT '------------------------------------------------------------'

SELECT 
    R.IdRazon,
    R.NOMBRE_RAZON,
    R.DB as BaseDatosOrigen,
    N.NOMBRE_TABLA as BasesDisponibles,
    '❓ Verificar en qué servidor está esta base' as Nota
FROM RAZONXTABLA R
LEFT JOIN NOM_TABLARAZON N ON R.IdRazon = N.IdRazon
ORDER BY R.IdRazon, N.NOMBRE_TABLA

PRINT ''
PRINT ''

-- ============================================================================
-- 3. BUSCAR BASES DE DATOS QUE PUEDEN ESTAR EN SERVIDORES DIFERENTES
-- ============================================================================
PRINT '3️⃣ ANÁLISIS DE POSIBLES PROBLEMAS'
PRINT '------------------------------------------------------------'
PRINT '⚠️  Si una razón social tiene múltiples bases, TODAS deben estar'
PRINT '    en el MISMO servidor para que el cálculo de retorno funcione.'
PRINT ''

SELECT 
    R.IdRazon,
    R.NOMBRE_RAZON,
    COUNT(DISTINCT N.NOMBRE_TABLA) as TotalBases,
    STRING_AGG(N.NOMBRE_TABLA, ', ') as ListaBases,
    CASE 
        WHEN COUNT(DISTINCT N.NOMBRE_TABLA) > 1 
        THEN '⚠️  Verificar que todas estén en el mismo servidor'
        ELSE '✅ Solo una base'
    END as Estado
FROM RAZONXTABLA R
LEFT JOIN NOM_TABLARAZON N ON R.IdRazon = N.IdRazon
GROUP BY R.IdRazon, R.NOMBRE_RAZON
ORDER BY R.IdRazon

PRINT ''
PRINT ''

-- ============================================================================
-- 4. VERIFICAR CASO ESPECÍFICO: SEERT_VIDRIOS
-- ============================================================================
PRINT '4️⃣ CASO ESPECÍFICO: SEERT_VIDRIOS'
PRINT '------------------------------------------------------------'

-- Verificar si SEERT_VIDRIOS está en RAZONXTABLA
IF EXISTS (SELECT 1 FROM RAZONXTABLA WHERE DB = 'SEERT_VIDRIOS')
BEGIN
    PRINT '✅ SEERT_VIDRIOS encontrado en RAZONXTABLA'

    SELECT 
        'Razón Social' = NOMBRE_RAZON,
        'Base Origen (DB)' = DB,
        'IdRazon' = IdRazon
    FROM RAZONXTABLA 
    WHERE DB = 'SEERT_VIDRIOS'

    PRINT ''

    -- Verificar bases relacionadas
    PRINT 'Bases de datos configuradas para esta razón social:'
    SELECT 
        N.NOMBRE_TABLA as BaseDatos,
        '❓ Verificar que esté en servidor 172.20.21.33' as Nota
    FROM RAZONXTABLA R
    INNER JOIN NOM_TABLARAZON N ON R.IdRazon = N.IdRazon
    WHERE R.DB = 'SEERT_VIDRIOS'
END
ELSE
BEGIN
    PRINT '❌ SEERT_VIDRIOS NO encontrado en RAZONXTABLA'
    PRINT '   Debe agregarse con el IdRazon correcto'
END

PRINT ''
PRINT ''

-- ============================================================================
-- 5. INSTRUCCIONES PARA VERIFICAR EN SERVIDOR SECUNDARIO
-- ============================================================================
PRINT '============================================================================'
PRINT '5️⃣ PASOS PARA VERIFICAR EN SERVIDOR SECUNDARIO (172.20.21.33)'
PRINT '============================================================================'
PRINT ''
PRINT '1. Conectarse al servidor 172.20.21.33 con SQL Server Management Studio'
PRINT '   usando el usuario MedTiempos'
PRINT ''
PRINT '2. Ejecutar este query para ver las bases de datos disponibles:'
PRINT '   SELECT name FROM sys.databases WHERE database_id > 4'
PRINT ''
PRINT '3. Verificar que SEERT_VIDRIOS existe en la lista'
PRINT ''
PRINT '4. Verificar que el usuario MedTiempos tiene permisos:'
PRINT '   USE SEERT_VIDRIOS;'
PRINT '   SELECT USER_NAME();'
PRINT '   -- Debe devolver "MedTiempos"'
PRINT ''
PRINT '5. Verificar permisos de lectura:'
PRINT '   USE SEERT_VIDRIOS;'
PRINT '   SELECT TOP 1 * FROM TR_Glosa;'
PRINT '   -- Debe devolver datos sin error'
PRINT ''

-- ============================================================================
-- 6. SCRIPT PARA CREAR USUARIO EN SERVIDOR SECUNDARIO (SI ES NECESARIO)
-- ============================================================================
PRINT ''
PRINT '============================================================================'
PRINT '6️⃣ SCRIPT PARA CONFIGURAR USUARIO EN SERVIDOR SECUNDARIO'
PRINT '============================================================================'
PRINT ''
PRINT '-- Ejecutar en servidor 172.20.21.33 si el usuario no existe'
PRINT ''
PRINT '-- 1. Crear login (si no existe)'
PRINT 'USE master;'
PRINT 'GO'
PRINT 'IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = ''MedTiempos'')'
PRINT 'BEGIN'
PRINT '    CREATE LOGIN [MedTiempos] WITH PASSWORD = ''T3ch4dm1n'';'
PRINT '    PRINT ''✅ Login MedTiempos creado'';'
PRINT 'END'
PRINT 'ELSE'
PRINT 'BEGIN'
PRINT '    PRINT ''ℹ️  Login MedTiempos ya existe'';'
PRINT 'END'
PRINT 'GO'
PRINT ''
PRINT '-- 2. Dar permiso para ver todas las bases de datos'
PRINT 'GRANT VIEW ANY DATABASE TO [MedTiempos];'
PRINT 'PRINT ''✅ Permiso VIEW ANY DATABASE otorgado'';'
PRINT 'GO'
PRINT ''
PRINT '-- 3. Crear usuario y dar permisos en SEERT_VIDRIOS'
PRINT 'USE SEERT_VIDRIOS;'
PRINT 'GO'
PRINT 'IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = ''MedTiempos'')'
PRINT 'BEGIN'
PRINT '    CREATE USER [MedTiempos] FOR LOGIN [MedTiempos];'
PRINT '    PRINT ''✅ Usuario MedTiempos creado en SEERT_VIDRIOS'';'
PRINT 'END'
PRINT 'ELSE'
PRINT 'BEGIN'
PRINT '    PRINT ''ℹ️  Usuario MedTiempos ya existe en SEERT_VIDRIOS'';'
PRINT 'END'
PRINT 'GO'
PRINT ''
PRINT 'ALTER ROLE db_datareader ADD MEMBER [MedTiempos];'
PRINT 'PRINT ''✅ Permisos de lectura otorgados en SEERT_VIDRIOS'';'
PRINT 'GO'
PRINT ''
PRINT '-- 4. Verificar configuración'
PRINT 'SELECT '
PRINT '    dp.name AS UserName,'
PRINT '    rp.name AS RoleName'
PRINT 'FROM sys.database_principals dp'
PRINT 'LEFT JOIN sys.database_role_members drm ON dp.principal_id = drm.member_principal_id'
PRINT 'LEFT JOIN sys.database_principals rp ON drm.role_principal_id = rp.principal_id'
PRINT 'WHERE dp.name = ''MedTiempos'';'
PRINT 'GO'
PRINT ''

-- ============================================================================
-- 7. RESUMEN Y PRÓXIMOS PASOS
-- ============================================================================
PRINT ''
PRINT '============================================================================'
PRINT '7️⃣ RESUMEN Y PRÓXIMOS PASOS'
PRINT '============================================================================'
PRINT ''
PRINT '✅ LO QUE ESTÁ BIEN:'
PRINT '   • El código ya está configurado para servidor secundario 172.20.21.33'
PRINT '   • El gestor de conexiones enruta automáticamente las consultas'
PRINT '   • Los queries usan la TR_Glosa correcta (baseDatosOrigen)'
PRINT ''
PRINT '⚠️  LO QUE PUEDE FALTAR:'
PRINT '   • El usuario SQL ''MedTiempos'' puede no existir en 172.20.21.33'
PRINT '   • El usuario puede no tener permisos en SEERT_VIDRIOS'
PRINT '   • La base de datos puede no existir en ese servidor'
PRINT ''
PRINT '📋 PASOS A SEGUIR:'
PRINT '   1. Ejecutar la sección 1-4 de este script para ver la configuración'
PRINT '   2. Conectarse al servidor 172.20.21.33'
PRINT '   3. Verificar que SEERT_VIDRIOS existe'
PRINT '   4. Ejecutar el script de la sección 6 para crear/configurar usuario'
PRINT '   5. Probar el cálculo de retorno nuevamente'
PRINT ''
PRINT '🔍 PARA DEBUGGING:'
PRINT '   • En Visual Studio, abrir View > Output > Debug'
PRINT '   • Ejecutar el cálculo de retorno'
PRINT '   • Revisar los mensajes de diagnóstico que muestran:'
PRINT '     - Qué servidor se usa'
PRINT '     - Qué base de datos se consulta'
PRINT '     - Qué TR_Glosa se accede'
PRINT ''
PRINT '============================================================================'
PRINT 'FIN DEL DIAGNÓSTICO'
PRINT '============================================================================'
