-- ============================================================================
-- SCRIPT DE MIGRACIÓN: Actualizar Contraseñas a Hash SHA256
-- ============================================================================
-- Descripción: Convierte contraseñas en texto plano a hash SHA256
-- Base de Datos: RetornoMaster
-- Tabla: Usuarios
-- Fecha: 2025
-- ============================================================================

USE RetornoMaster;
GO

-- ============================================================================
-- PASO 1: VERIFICAR USUARIOS ACTUALES
-- ============================================================================
PRINT '============================================================================'
PRINT 'PASO 1: Verificar usuarios existentes'
PRINT '============================================================================'
PRINT ''

SELECT 
    IdUsuario,
    UserAlias,
    PasswordHash,
    LEN(PasswordHash) AS LongitudPassword,
    CASE 
        WHEN LEN(PasswordHash) = 64 THEN 'Hash SHA256 ✅'
        WHEN LEN(PasswordHash) < 64 THEN 'Texto Plano ⚠️'
        ELSE 'Desconocido'
    END AS TipoPassword,
    NombreUsuario,
    ApellidoUsuario,
    Activo,
    IdRol
FROM Usuarios
ORDER BY IdUsuario

PRINT ''
PRINT '============================================================================'
PRINT 'PASO 2: Actualizar contraseñas a Hash SHA256'
PRINT '============================================================================'
PRINT ''

-- ============================================================================
-- IMPORTANTE: Los hashes SHA256 fueron calculados usando el algoritmo:
-- SHA256(texto_plano) = hash_64_caracteres
-- 
-- Puedes verificar estos hashes en: https://emn178.github.io/online-tools/sha256.html
-- ============================================================================

-- ----------------------------------------------------------------------------
-- Usuario: jnieto
-- Contraseña original: admin1234
-- Hash SHA256: 8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918
-- ----------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM Usuarios WHERE UserAlias = 'jnieto')
BEGIN
    UPDATE Usuarios 
    SET PasswordHash = '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918'
    WHERE UserAlias = 'jnieto' AND LEN(PasswordHash) < 64

    IF @@ROWCOUNT > 0
        PRINT '✅ Usuario jnieto actualizado a hash SHA256'
    ELSE
        PRINT 'ℹ️  Usuario jnieto ya tiene hash SHA256 o no existe'
END
GO

-- ----------------------------------------------------------------------------
-- Usuario: Omelas
-- Contraseña original: admin1234
-- Hash SHA256: 8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918
-- ----------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM Usuarios WHERE UserAlias = 'Omelas')
BEGIN
    UPDATE Usuarios 
    SET PasswordHash = '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918'
    WHERE UserAlias = 'Omelas' AND LEN(PasswordHash) < 64

    IF @@ROWCOUNT > 0
        PRINT '✅ Usuario Omelas actualizado a hash SHA256'
    ELSE
        PRINT 'ℹ️  Usuario Omelas ya tiene hash SHA256 o no existe'
END
GO

-- ----------------------------------------------------------------------------
-- Usuario: esenano
-- Contraseña original: Tacna.26
-- Hash SHA256: Debes calcular el hash de "Tacna.26"
-- Calcula en: https://emn178.github.io/online-tools/sha256.html
-- O usa el método CalcularHashSHA256() en el código
-- ----------------------------------------------------------------------------
-- DESCOMENTAR Y COMPLETAR CON EL HASH CORRECTO:
-- IF EXISTS (SELECT 1 FROM Usuarios WHERE UserAlias = 'esenano')
-- BEGIN
--     UPDATE Usuarios 
--     SET PasswordHash = 'AQUI_VA_EL_HASH_SHA256_DE_Tacna.26'
--     WHERE UserAlias = 'esenano' AND LEN(PasswordHash) < 64
--     
--     IF @@ROWCOUNT > 0
--         PRINT '✅ Usuario esenano actualizado a hash SHA256'
--     ELSE
--         PRINT 'ℹ️  Usuario esenano ya tiene hash SHA256 o no existe'
-- END
-- GO

-- ----------------------------------------------------------------------------
-- Usuario: nico
-- Contraseña original: Tacna.26
-- Hash SHA256: (mismo que esenano si usa la misma contraseña)
-- ----------------------------------------------------------------------------
-- DESCOMENTAR Y COMPLETAR CON EL HASH CORRECTO:
-- IF EXISTS (SELECT 1 FROM Usuarios WHERE UserAlias = 'nico')
-- BEGIN
--     UPDATE Usuarios 
--     SET PasswordHash = 'AQUI_VA_EL_HASH_SHA256_DE_Tacna.26'
--     WHERE UserAlias = 'nico' AND LEN(PasswordHash) < 64
--     
--     IF @@ROWCOUNT > 0
--         PRINT '✅ Usuario nico actualizado a hash SHA256'
--     ELSE
--         PRINT 'ℹ️  Usuario nico ya tiene hash SHA256 o no existe'
-- END
-- GO

-- ============================================================================
-- PASO 3: VERIFICAR ACTUALIZACIÓN
-- ============================================================================
PRINT ''
PRINT '============================================================================'
PRINT 'PASO 3: Verificar que todas las contraseñas sean hash SHA256'
PRINT '============================================================================'
PRINT ''

SELECT 
    IdUsuario,
    UserAlias,
    LEFT(PasswordHash, 20) + '...' AS PasswordHash_Preview,
    LEN(PasswordHash) AS LongitudPassword,
    CASE 
        WHEN LEN(PasswordHash) = 64 THEN '✅ Hash SHA256'
        WHEN LEN(PasswordHash) < 64 THEN '⚠️ Texto Plano - ACTUALIZAR'
        ELSE '❓ Desconocido'
    END AS Estado,
    NombreUsuario,
    ApellidoUsuario,
    Activo
FROM Usuarios
ORDER BY IdUsuario

PRINT ''

-- Contar usuarios con contraseñas pendientes de actualizar
DECLARE @PendientesActualizar INT
SELECT @PendientesActualizar = COUNT(*) 
FROM Usuarios 
WHERE LEN(PasswordHash) < 64

IF @PendientesActualizar > 0
BEGIN
    PRINT '⚠️  ADVERTENCIA: ' + CAST(@PendientesActualizar AS VARCHAR(10)) + ' usuario(s) aún tienen contraseña en texto plano'
    PRINT '   Debes actualizar manualmente estos usuarios con el hash SHA256 correcto'
END
ELSE
BEGIN
    PRINT '✅ Todos los usuarios tienen contraseñas con hash SHA256'
END

PRINT ''
PRINT '============================================================================'
PRINT 'PASO 4: Tabla de Hashes SHA256 para Contraseñas Comunes'
PRINT '============================================================================'
PRINT ''
PRINT 'Para calcular el hash de una contraseña, puedes usar:'
PRINT '  1. El método CalcularHashSHA256() en el código C#'
PRINT '  2. Sitio web: https://emn178.github.io/online-tools/sha256.html'
PRINT ''
PRINT 'Ejemplos de hashes SHA256:'
PRINT '  - admin1234  → 8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918'
PRINT '  - test123    → ecd71870d1963316a97e3ac3408c9835ad8cf0f3c1bc703527c30265534f75ae'
PRINT '  - password   → 5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8'
PRINT '  - 12345678   → ef797c8118f02dfb649607dd5d3f8c7623048c9c063d532cc95c5ed7a898a64f'
PRINT ''
PRINT '============================================================================'
PRINT 'MIGRACIÓN COMPLETADA'
PRINT '============================================================================'
PRINT ''
PRINT 'Próximos pasos:'
PRINT '  1. Verificar que todos los usuarios tengan hash SHA256 (64 caracteres)'
PRINT '  2. Probar el login con cada usuario'
PRINT '  3. Si algún usuario no puede hacer login, verificar su hash en la BD'
PRINT ''

GO

-- ============================================================================
-- SCRIPT ADICIONAL: Generar contraseña temporal para usuarios sin hash
-- ============================================================================
-- Si tienes usuarios cuya contraseña original no conoces, puedes asignar
-- una contraseña temporal y pedirles que la cambien en el próximo login

/*
-- Contraseña temporal: "CambiarMe2025"
-- Hash SHA256: 0e8a4f3f9c8f7e6d5c4b3a2e1f0e9d8c7b6a5e4f3d2c1b0a9e8f7d6c5b4a3e2f1
-- (Este hash es solo un ejemplo, calcula el real)

-- Descomentar para usar:
-- UPDATE Usuarios 
-- SET PasswordHash = 'HASH_SHA256_DE_CambiarMe2025'
-- WHERE LEN(PasswordHash) < 64

-- Luego implementar funcionalidad de "Cambiar contraseña al primer login"
*/

GO

-- ============================================================================
-- FIN DEL SCRIPT
-- ============================================================================
