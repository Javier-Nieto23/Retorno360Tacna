-- ========================================================================
-- Script: Agregar Columnas ConnExterna e IdConexion a RAZONXTABLA
-- Propósito: Preparar la tabla para el nuevo sistema de conexiones externas
-- Fecha: Enero 2026
-- ========================================================================

USE RetornoMaster;
GO

PRINT '🔧 Iniciando configuración de RAZONXTABLA...';
PRINT '';

-- ========================================================================
-- PASO 1: Verificar si las columnas ya existen
-- ========================================================================

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'RAZONXTABLA' 
    AND COLUMN_NAME = 'ConnExterna'
)
BEGIN
    PRINT '✅ Agregando columna ConnExterna...';

    ALTER TABLE RAZONXTABLA
    ADD ConnExterna VARCHAR(1) NULL;

    PRINT '   Columna ConnExterna agregada correctamente.';
END
ELSE
BEGIN
    PRINT '⚠️  La columna ConnExterna ya existe.';
END

PRINT '';

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'RAZONXTABLA' 
    AND COLUMN_NAME = 'IdConexion'
)
BEGIN
    PRINT '✅ Agregando columna IdConexion...';

    ALTER TABLE RAZONXTABLA
    ADD IdConexion INT NULL;

    PRINT '   Columna IdConexion agregada correctamente.';
END
ELSE
BEGIN
    PRINT '⚠️  La columna IdConexion ya existe.';
END

PRINT '';

-- ========================================================================
-- PASO 2: Crear Foreign Key (si no existe)
-- ========================================================================

IF NOT EXISTS (
    SELECT 1 
    FROM sys.foreign_keys 
    WHERE name = 'FK_RAZONXTABLA_Conexiones'
)
BEGIN
    PRINT '✅ Creando Foreign Key FK_RAZONXTABLA_Conexiones...';

    ALTER TABLE RAZONXTABLA
    ADD CONSTRAINT FK_RAZONXTABLA_Conexiones
    FOREIGN KEY (IdConexion) REFERENCES Conexiones(IdConexion);

    PRINT '   Foreign Key creada correctamente.';
END
ELSE
BEGIN
    PRINT '⚠️  La Foreign Key FK_RAZONXTABLA_Conexiones ya existe.';
END

PRINT '';

-- ========================================================================
-- PASO 3: Verificar estructura de tabla Conexiones
-- ========================================================================

PRINT '📊 Verificando tabla Conexiones...';

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Conexiones')
BEGIN
    PRINT '❌ ERROR: La tabla Conexiones no existe.';
    PRINT '   Por favor, crea la tabla Conexiones primero.';
    PRINT '';
    PRINT 'Script sugerido:';
    PRINT 'CREATE TABLE Conexiones (';
    PRINT '    IdConexion INT PRIMARY KEY,';
    PRINT '    NombreConexion VARCHAR(100),';
    PRINT '    Servidor VARCHAR(50),';
    PRINT '    UsuarioSQL VARCHAR(50),';
    PRINT '    PasswordSQL VARCHAR(100),';
    PRINT '    TipoMotor VARCHAR(50),';
    PRINT '    Activo BIT DEFAULT 1';
    PRINT ');';
END
ELSE
BEGIN
    PRINT '✅ Tabla Conexiones existe.';

    -- Mostrar conexiones activas
    PRINT '';
    PRINT 'Conexiones activas en el sistema:';
    SELECT 
        IdConexion,
        NombreConexion,
        Servidor,
        UsuarioSQL,
        Activo
    FROM Conexiones
    WHERE Activo = 1
    ORDER BY IdConexion;
END

PRINT '';

-- ========================================================================
-- PASO 4: Inicializar valores por defecto
-- ========================================================================

PRINT '📝 Inicializando valores por defecto...';

-- Marcar todas las razones existentes como conexión principal (NULL o 'N')
UPDATE RAZONXTABLA
SET 
    ConnExterna = NULL,
    IdConexion = NULL
WHERE ConnExterna IS NULL AND IdConexion IS NULL;

PRINT '   Todas las razones marcadas como conexión principal por defecto.';
PRINT '';

-- ========================================================================
-- PASO 5: Configurar conexiones externas conocidas (EJEMPLO)
-- ========================================================================

PRINT '🔧 Configurando conexiones externas conocidas...';
PRINT '';

-- EJEMPLO: Marcar SEERT_VIDRIOS como conexión externa
-- Ajusta este ejemplo según tus necesidades

IF EXISTS (SELECT 1 FROM RAZONXTABLA WHERE DB = 'SEERT_VIDRIOS')
AND EXISTS (SELECT 1 FROM Conexiones WHERE IdConexion = 1002)
BEGIN
    PRINT '✅ Configurando SEERT_VIDRIOS como conexión externa...';

    UPDATE RAZONXTABLA
    SET 
        ConnExterna = 'S',
        IdConexion = 1002
    WHERE DB = 'SEERT_VIDRIOS';

    PRINT '   SEERT_VIDRIOS configurado correctamente.';
END
ELSE
BEGIN
    PRINT '⚠️  SEERT_VIDRIOS o IdConexion 1002 no encontrados.';
    PRINT '   Configura manualmente si es necesario.';
END

PRINT '';

-- ========================================================================
-- PASO 6: Validación final
-- ========================================================================

PRINT '🔍 Validación final...';
PRINT '';

-- Verificar razones con ConnExterna = 'S' pero sin IdConexion
IF EXISTS (SELECT 1 FROM RAZONXTABLA WHERE ConnExterna = 'S' AND IdConexion IS NULL)
BEGIN
    PRINT '❌ ERROR: Existen razones con ConnExterna = ''S'' pero sin IdConexion:';
    SELECT IdRazon, NOMBRE_RAZON, DB, ConnExterna, IdConexion
    FROM RAZONXTABLA
    WHERE ConnExterna = 'S' AND IdConexion IS NULL;
    PRINT '';
END
ELSE
BEGIN
    PRINT '✅ No hay razones con ConnExterna = ''S'' sin IdConexion.';
END

-- Verificar IdConexion inválidos
IF EXISTS (
    SELECT 1 
    FROM RAZONXTABLA R
    LEFT JOIN Conexiones C ON R.IdConexion = C.IdConexion
    WHERE R.ConnExterna = 'S' AND C.IdConexion IS NULL
)
BEGIN
    PRINT '❌ ERROR: Existen razones con IdConexion inválido:';
    SELECT R.IdRazon, R.NOMBRE_RAZON, R.DB, R.ConnExterna, R.IdConexion
    FROM RAZONXTABLA R
    LEFT JOIN Conexiones C ON R.IdConexion = C.IdConexion
    WHERE R.ConnExterna = 'S' AND C.IdConexion IS NULL;
    PRINT '';
END
ELSE
BEGIN
    PRINT '✅ Todos los IdConexion son válidos.';
END

PRINT '';

-- ========================================================================
-- PASO 7: Resumen de configuración
-- ========================================================================

PRINT '📊 RESUMEN DE CONFIGURACIÓN';
PRINT '═══════════════════════════════════════════════════════════════';
PRINT '';

-- Contar razones por tipo de conexión
DECLARE @TotalRazones INT;
DECLARE @ConexionPrincipal INT;
DECLARE @ConexionExterna INT;

SELECT @TotalRazones = COUNT(*) FROM RAZONXTABLA;
SELECT @ConexionPrincipal = COUNT(*) FROM RAZONXTABLA WHERE ConnExterna IS NULL OR ConnExterna != 'S';
SELECT @ConexionExterna = COUNT(*) FROM RAZONXTABLA WHERE ConnExterna = 'S';

PRINT 'Total de razones sociales: ' + CAST(@TotalRazones AS VARCHAR);
PRINT 'Conexión principal: ' + CAST(@ConexionPrincipal AS VARCHAR);
PRINT 'Conexión externa: ' + CAST(@ConexionExterna AS VARCHAR);
PRINT '';

-- Mostrar detalle de conexiones externas
IF @ConexionExterna > 0
BEGIN
    PRINT 'Razones con conexión externa:';
    PRINT '';
    SELECT 
        R.IdRazon,
        R.NOMBRE_RAZON,
        R.DB,
        C.Servidor,
        C.UsuarioSQL,
        C.NombreConexion
    FROM RAZONXTABLA R
    INNER JOIN Conexiones C ON R.IdConexion = C.IdConexion
    WHERE R.ConnExterna = 'S'
    ORDER BY R.NOMBRE_RAZON;
END
ELSE
BEGIN
    PRINT 'No hay razones configuradas con conexión externa.';
END

PRINT '';
PRINT '═══════════════════════════════════════════════════════════════';
PRINT '✅ Configuración completada exitosamente.';
PRINT '';
PRINT 'SIGUIENTE PASO:';
PRINT '  Para agregar más conexiones externas, ejecuta:';
PRINT '';
PRINT '  UPDATE RAZONXTABLA';
PRINT '  SET ConnExterna = ''S'', IdConexion = [ID]';
PRINT '  WHERE DB = ''[NOMBRE_BASE_DATOS]'';';
PRINT '';

GO
