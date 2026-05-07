-- ========================================================================================================
-- SCRIPT DE VERIFICACIÓN Y CORRECCIÓN DE CONFIGURACIÓN DE CONEXIONES
-- ========================================================================================================
-- 
-- Este script verifica y corrige la configuración de conexiones para el sistema de Retorno360Tacna
-- 
-- TABLAS INVOLUCRADAS:
--   1. NOM_TABLARAZON: Contiene las bases de datos seleccionables (Di_Pedimento/De_Pedimento)
--   2. RAZONXTABLA: Contiene las bases de datos origen/glosa (TR_Glosa)
--   3. Conexiones: Contiene la información de servidores externos
-- 
-- ========================================================================================================

USE RetornoMaster
GO

PRINT '========================================================================================================';
PRINT 'PASO 1: VERIFICAR ESTRUCTURA DE TABLAS';
PRINT '========================================================================================================';
PRINT '';

-- Verificar columnas de NOM_TABLARAZON
PRINT '1.1 Verificando estructura de NOM_TABLARAZON...';
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('NOM_TABLARAZON') AND name = 'IdConexion')
BEGIN
    PRINT '   ❌ ERROR: Falta la columna IdConexion en NOM_TABLARAZON';
    PRINT '   SOLUCIÓN: ALTER TABLE NOM_TABLARAZON ADD IdConexion INT NULL';
END
ELSE
BEGIN
    PRINT '   ✅ OK: La columna IdConexion existe en NOM_TABLARAZON';
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('NOM_TABLARAZON') AND name = 'ConnExterna')
BEGIN
    PRINT '   ⚠️  ADVERTENCIA: La columna ConnExterna existe en NOM_TABLARAZON (NO SE USA)';
    PRINT '   NOTA: El sistema solo utiliza IdConexion para determinar el servidor';
END
ELSE
BEGIN
    PRINT '   ✅ OK: La columna ConnExterna NO existe (correcto, no se necesita)';
END
PRINT '';

-- Verificar columnas de RAZONXTABLA
PRINT '1.2 Verificando estructura de RAZONXTABLA...';
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RAZONXTABLA') AND name = 'ConnExterna')
BEGIN
    PRINT '   ❌ ERROR: Falta la columna ConnExterna en RAZONXTABLA';
END
ELSE
BEGIN
    PRINT '   ✅ OK: La columna ConnExterna existe en RAZONXTABLA';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RAZONXTABLA') AND name = 'IdConexion')
BEGIN
    PRINT '   ❌ ERROR: Falta la columna IdConexion en RAZONXTABLA';
END
ELSE
BEGIN
    PRINT '   ✅ OK: La columna IdConexion existe en RAZONXTABLA';
END
PRINT '';

PRINT '========================================================================================================';
PRINT 'PASO 2: VERIFICAR DATOS DE CONEXIONES';
PRINT '========================================================================================================';
PRINT '';

-- Verificar tabla Conexiones
PRINT '2.1 Conexiones disponibles en tabla Conexiones:';
SELECT 
    IdConexion,
    NombreConexion,
    Servidor,
    UsuarioSQL,
    CASE WHEN PasswordSQL IS NOT NULL THEN '[CONFIGURADA]' ELSE '[SIN CONFIGURAR]' END AS Password,
    CASE WHEN Activo = 1 THEN 'Activa' ELSE 'Inactiva' END AS Estado
FROM Conexiones
ORDER BY IdConexion;
PRINT '';

-- Verificar bases de datos en NOM_TABLARAZON sin IdConexion configurado
PRINT '2.2 Bases de datos en NOM_TABLARAZON sin IdConexion (usan conexión principal):';
SELECT 
    N.IdTabla,
    N.NOMBRE_TABLA,
    N.IdRazon,
    N.IdConexion
FROM NOM_TABLARAZON N
WHERE N.IdConexion IS NULL
ORDER BY N.NOMBRE_TABLA;
PRINT '';

-- Verificar bases de datos en NOM_TABLARAZON con IdConexion configurado
PRINT '2.3 Bases de datos en NOM_TABLARAZON con conexión externa:';
SELECT 
    N.IdTabla,
    N.NOMBRE_TABLA,
    N.IdRazon,
    N.IdConexion,
    C.NombreConexion,
    C.Servidor,
    C.UsuarioSQL
FROM NOM_TABLARAZON N
INNER JOIN Conexiones C ON N.IdConexion = C.IdConexion
ORDER BY N.NOMBRE_TABLA;
PRINT '';

-- Verificar bases de datos en RAZONXTABLA
PRINT '2.4 Bases de datos en RAZONXTABLA (TR_Glosa):';
SELECT 
    R.IdRazon,
    R.NOMBRE_RAZON,
    R.DB,
    R.ConnExterna,
    R.IdConexion,
    C.NombreConexion,
    C.Servidor,
    C.UsuarioSQL
FROM RAZONXTABLA R
LEFT JOIN Conexiones C ON R.IdConexion = C.IdConexion
ORDER BY R.NOMBRE_RAZON;
PRINT '';

PRINT '========================================================================================================';
PRINT 'PASO 3: DETECTAR PROBLEMAS';
PRINT '========================================================================================================';
PRINT '';

-- Detectar IdConexion inválidos en NOM_TABLARAZON
PRINT '3.1 Bases de datos en NOM_TABLARAZON con IdConexion inválido:';
SELECT 
    N.IdTabla,
    N.NOMBRE_TABLA,
    N.IdConexion AS IdConexion_Configurado
FROM NOM_TABLARAZON N
WHERE N.IdConexion IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM Conexiones C WHERE C.IdConexion = N.IdConexion)
ORDER BY N.NOMBRE_TABLA;
PRINT '';

-- Detectar IdConexion inválidos en RAZONXTABLA
PRINT '3.2 Bases de datos en RAZONXTABLA con IdConexion inválido:';
SELECT 
    R.IdRazon,
    R.NOMBRE_RAZON,
    R.DB,
    R.IdConexion AS IdConexion_Configurado
FROM RAZONXTABLA R
WHERE R.IdConexion IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM Conexiones C WHERE C.IdConexion = R.IdConexion)
ORDER BY R.NOMBRE_RAZON;
PRINT '';

-- Detectar problema específico: SEERT_Jlo
PRINT '3.3 Configuración específica de SEERT_Jlo:';
IF EXISTS (SELECT 1 FROM NOM_TABLARAZON WHERE NOMBRE_TABLA = 'SEERT_Jlo')
BEGIN
    SELECT 
        'NOM_TABLARAZON' AS Tabla,
        N.NOMBRE_TABLA AS BaseDatos,
        N.IdConexion,
        C.NombreConexion,
        C.Servidor,
        C.UsuarioSQL,
        CASE 
            WHEN N.IdConexion IS NULL THEN 'Usará conexión principal'
            WHEN C.IdConexion IS NULL THEN '❌ ERROR: IdConexion no existe en tabla Conexiones'
            ELSE 'OK: Conexión externa configurada'
        END AS Estado
    FROM NOM_TABLARAZON N
    LEFT JOIN Conexiones C ON N.IdConexion = C.IdConexion
    WHERE N.NOMBRE_TABLA = 'SEERT_Jlo';
END
ELSE
BEGIN
    PRINT '   ⚠️  SEERT_Jlo no encontrada en NOM_TABLARAZON';
END
PRINT '';

PRINT '========================================================================================================';
PRINT 'PASO 4: SOLUCIONES RECOMENDADAS';
PRINT '========================================================================================================';
PRINT '';
PRINT '4.1 Si SEERT_Jlo está en el servidor principal (172.20.20.26):';
PRINT '    UPDATE NOM_TABLARAZON SET IdConexion = NULL WHERE NOMBRE_TABLA = ''SEERT_Jlo'';';
PRINT '';
PRINT '4.2 Si SEERT_Jlo está en un servidor externo:';
PRINT '    -- Primero, agregar el servidor a la tabla Conexiones si no existe:';
PRINT '    INSERT INTO Conexiones (NombreConexion, Servidor, UsuarioSQL, PasswordSQL, TipoMotor, Activo)';
PRINT '    VALUES (''Nombre del Servidor'', ''IP o Nombre'', ''Usuario'', ''Password'', ''SQLServer'', 1);';
PRINT '';
PRINT '    -- Luego, actualizar NOM_TABLARAZON con el IdConexion correcto:';
PRINT '    UPDATE NOM_TABLARAZON SET IdConexion = [IdConexion] WHERE NOMBRE_TABLA = ''SEERT_Jlo'';';
PRINT '';
PRINT '========================================================================================================';
PRINT 'FIN DEL DIAGNÓSTICO';
PRINT '========================================================================================================';
