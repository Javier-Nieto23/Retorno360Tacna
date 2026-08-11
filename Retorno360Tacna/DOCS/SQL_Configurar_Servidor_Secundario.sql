-- ============================================================================
-- CONFIGURACIÓN SERVIDOR SECUNDARIO: 172.20.21.33
-- ============================================================================
-- Ejecutar este script EN EL SERVIDOR 172.20.21.33
-- con una cuenta que tenga permisos de sysadmin
-- ============================================================================

USE master;
GO

PRINT '============================================================================'
PRINT 'CONFIGURACIÓN DE USUARIO SQL PARA RETORNO360TACNA'
PRINT 'Servidor: 172.20.21.33 (Servidor Secundario)'
PRINT '============================================================================'
PRINT ''

-- ============================================================================
-- PASO 1: Crear Login si no existe
-- ============================================================================
PRINT '📝 PASO 1: Verificar/Crear Login MedTiempos'
PRINT '------------------------------------------------------------'

IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'MedTiempos')
BEGIN
    CREATE LOGIN [MedTiempos] WITH PASSWORD = N'T3ch4dm1n';
    PRINT '✅ Login [MedTiempos] creado exitosamente'
END
ELSE
BEGIN
    PRINT 'ℹ️  Login [MedTiempos] ya existe'

    -- Opcional: Cambiar contraseña si es necesaria
    -- ALTER LOGIN [MedTiempos] WITH PASSWORD = N'T3ch4dm1n';
    -- PRINT '✅ Contraseña actualizada'
END
GO

-- ============================================================================
-- PASO 2: Dar permiso para ver todas las bases de datos
-- ============================================================================
PRINT ''
PRINT '📝 PASO 2: Otorgar permiso VIEW ANY DATABASE'
PRINT '------------------------------------------------------------'

GRANT VIEW ANY DATABASE TO [MedTiempos];
PRINT '✅ Permiso VIEW ANY DATABASE otorgado'
PRINT '   (Permite al usuario ver la lista de bases de datos)'
GO

-- ============================================================================
-- PASO 3: Configurar permisos en SEERT_VIDRIOS
-- ============================================================================
PRINT ''
PRINT '📝 PASO 3: Configurar permisos en SEERT_VIDRIOS'
PRINT '------------------------------------------------------------'

-- Verificar si la base de datos existe
IF DB_ID('SEERT_VIDRIOS') IS NOT NULL
BEGIN
    USE SEERT_VIDRIOS;

    -- Crear usuario en la base de datos
    IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'MedTiempos')
    BEGIN
        CREATE USER [MedTiempos] FOR LOGIN [MedTiempos];
        PRINT '✅ Usuario [MedTiempos] creado en SEERT_VIDRIOS'
    END
    ELSE
    BEGIN
        PRINT 'ℹ️  Usuario [MedTiempos] ya existe en SEERT_VIDRIOS'
    END

    -- Agregar al rol db_datareader (lectura)
    ALTER ROLE db_datareader ADD MEMBER [MedTiempos];
    PRINT '✅ Usuario agregado al rol db_datareader'
    PRINT '   (Puede leer todas las tablas de la base de datos)'

    -- Opcional: Agregar permisos de ejecución si hay stored procedures
    -- GRANT EXECUTE TO [MedTiempos];
    -- PRINT '✅ Permisos de ejecución otorgados'
END
ELSE
BEGIN
    PRINT '❌ ERROR: La base de datos SEERT_VIDRIOS NO EXISTE en este servidor'
    PRINT '   Por favor, verifica:'
    PRINT '   1. El nombre de la base de datos es correcto'
    PRINT '   2. La base de datos está en el servidor correcto'
    PRINT '   3. Restaura el backup de SEERT_VIDRIOS si es necesario'
END
GO

-- ============================================================================
-- PASO 4: Aplicar permisos a TODAS las bases de datos (OPCIONAL)
-- ============================================================================
PRINT ''
PRINT '📝 PASO 4 (OPCIONAL): Aplicar permisos a todas las bases'
PRINT '------------------------------------------------------------'
PRINT 'Si quieres dar acceso a TODAS las bases de datos del servidor'
PRINT 'secundario de una vez, descomenta y ejecuta el siguiente bloque:'
PRINT ''

/*
-- DESCOMENTA ESTE BLOQUE PARA APLICAR A TODAS LAS BASES
EXEC sp_MSforeachdb '
USE [?];
IF ''?'' NOT IN (''master'', ''tempdb'', ''model'', ''msdb'', ''RetornoMaster'')
BEGIN
    PRINT ''Configurando permisos en [?]''

    IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = ''MedTiempos'')
    BEGIN
        CREATE USER [MedTiempos] FOR LOGIN [MedTiempos];
    END

    ALTER ROLE db_datareader ADD MEMBER [MedTiempos];

    PRINT ''✅ Permisos configurados en [?]''
END
'
*/

PRINT 'ℹ️  Bloque comentado. Ejecuta manualmente si es necesario.'
GO

-- ============================================================================
-- PASO 5: Verificar configuración
-- ============================================================================
PRINT ''
PRINT '📝 PASO 5: Verificar configuración'
PRINT '------------------------------------------------------------'

-- Verificar login
PRINT ''
PRINT '1️⃣ Login en el servidor:'
SELECT 
    name AS LoginName,
    type_desc AS LoginType,
    create_date AS FechaCreacion,
    is_disabled AS EstaDeshabilitado
FROM sys.server_principals 
WHERE name = 'MedTiempos'

-- Verificar permisos a nivel servidor
PRINT ''
PRINT '2️⃣ Permisos a nivel servidor:'
SELECT 
    pr.name AS LoginName,
    pe.permission_name AS Permiso,
    pe.state_desc AS Estado
FROM sys.server_principals pr
LEFT JOIN sys.server_permissions pe ON pr.principal_id = pe.grantee_principal_id
WHERE pr.name = 'MedTiempos'

-- Verificar usuario en base de datos
IF DB_ID('SEERT_VIDRIOS') IS NOT NULL
BEGIN
    PRINT ''
    PRINT '3️⃣ Usuario en SEERT_VIDRIOS:'

    USE SEERT_VIDRIOS;

    SELECT 
        dp.name AS NombreUsuario,
        dp.type_desc AS TipoUsuario,
        dp.create_date AS FechaCreacion
    FROM sys.database_principals dp
    WHERE dp.name = 'MedTiempos'

    PRINT ''
    PRINT '4️⃣ Roles del usuario en SEERT_VIDRIOS:'

    SELECT 
        USER_NAME(drm.member_principal_id) AS NombreUsuario,
        USER_NAME(drm.role_principal_id) AS Rol
    FROM sys.database_role_members drm
    WHERE USER_NAME(drm.member_principal_id) = 'MedTiempos'
END
GO

-- ============================================================================
-- PASO 6: Prueba de conexión
-- ============================================================================
PRINT ''
PRINT '📝 PASO 6: Prueba de conexión y lectura'
PRINT '------------------------------------------------------------'

IF DB_ID('SEERT_VIDRIOS') IS NOT NULL
BEGIN
    USE SEERT_VIDRIOS;

    -- Probar que el usuario puede leer TR_Glosa
    DECLARE @TotalRegistros INT

    BEGIN TRY
        SELECT @TotalRegistros = COUNT(*) FROM TR_Glosa
        PRINT '✅ Prueba de lectura exitosa'
        PRINT '   Total de registros en TR_Glosa: ' + CAST(@TotalRegistros AS VARCHAR(20))
    END TRY
    BEGIN CATCH
        PRINT '❌ Error al leer TR_Glosa:'
        PRINT '   ' + ERROR_MESSAGE()
    END CATCH

    -- Probar lectura de otras tablas importantes
    BEGIN TRY
        SELECT @TotalRegistros = COUNT(*) FROM Di_Pedimento
        PRINT '✅ Lectura de Di_Pedimento exitosa'
        PRINT '   Total de registros: ' + CAST(@TotalRegistros AS VARCHAR(20))
    END TRY
    BEGIN CATCH
        PRINT '❌ Error al leer Di_Pedimento:'
        PRINT '   ' + ERROR_MESSAGE()
    END CATCH

    BEGIN TRY
        SELECT @TotalRegistros = COUNT(*) FROM De_Pedimento
        PRINT '✅ Lectura de De_Pedimento exitosa'
        PRINT '   Total de registros: ' + CAST(@TotalRegistros AS VARCHAR(20))
    END TRY
    BEGIN CATCH
        PRINT '❌ Error al leer De_Pedimento:'
        PRINT '   ' + ERROR_MESSAGE()
    END CATCH
END
GO

-- ============================================================================
-- PASO 7: String de conexión de prueba
-- ============================================================================
PRINT ''
PRINT '============================================================================'
PRINT '📝 PASO 7: String de conexión para prueba'
PRINT '============================================================================'
PRINT ''
PRINT 'Usa este connection string para probar desde SQL Server Management Studio:'
PRINT ''
PRINT 'Server=172.20.21.33;Database=SEERT_VIDRIOS;User Id=MedTiempos;Password=T3ch4dm1n;TrustServerCertificate=True;'
PRINT ''
PRINT 'O desde la aplicación, el código ya está configurado para usar:'
PRINT '  servidor: "172.20.21.33"'
PRINT '  usuario: "MedTiempos"'
PRINT '  password: "T3ch4dm1n"'
PRINT ''

-- ============================================================================
-- RESUMEN FINAL
-- ============================================================================
PRINT '============================================================================'
PRINT '📋 RESUMEN DE CONFIGURACIÓN'
PRINT '============================================================================'
PRINT ''
PRINT '✅ PASOS COMPLETADOS:'
PRINT '   1. Login [MedTiempos] creado/verificado'
PRINT '   2. Permiso VIEW ANY DATABASE otorgado'
PRINT '   3. Usuario creado en SEERT_VIDRIOS'
PRINT '   4. Rol db_datareader asignado'
PRINT '   5. Configuración verificada'
PRINT '   6. Pruebas de lectura realizadas'
PRINT ''
PRINT '🎯 PRÓXIMOS PASOS:'
PRINT '   1. Ejecutar el diagnóstico en la aplicación'
PRINT '   2. En Visual Studio: View > Output > Debug'
PRINT '   3. Calcular retorno para SEERT_VIDRIOS'
PRINT '   4. Verificar que los logs muestren:'
PRINT '      - Conexión a servidor 172.20.21.33'
PRINT '      - Acceso a SEERT_VIDRIOS.dbo.TR_Glosa'
PRINT '      - Cálculo exitoso'
PRINT ''
PRINT '⚠️  SI AÚN HAY ERRORES:'
PRINT '   • Verifica que SEERT_VIDRIOS esté en NOM_TABLARAZON'
PRINT '   • Verifica que RAZONXTABLA tenga DB = ''SEERT_VIDRIOS'''
PRINT '   • Revisa el firewall del servidor 172.20.21.33'
PRINT '   • Verifica que el servicio SQL Server esté corriendo'
PRINT ''
PRINT '============================================================================'
PRINT 'CONFIGURACIÓN COMPLETADA'
PRINT '============================================================================'

