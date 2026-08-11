# Sistema de Autenticación con Hash SHA256 - Guía Completa

## 📋 Índice

1. [Resumen del Sistema](#resumen-del-sistema)
2. [Cambios Implementados](#cambios-implementados)
3. [Configuración Inicial](#configuración-inicial)
4. [Migración de Usuarios](#migración-de-usuarios)
5. [Pruebas](#pruebas)
6. [Troubleshooting](#troubleshooting)
7. [Seguridad](#seguridad)

## 🎯 Resumen del Sistema

El sistema de autenticación ahora utiliza **hash SHA256** para almacenar y validar contraseñas, garantizando que nunca se guarden en texto plano.

### Componentes Afectados

| Componente | Archivo | Función |
|-----------|---------|---------|
| **Login** | `Login.cs` | Calcula hash SHA256 antes de validar |
| **Agregar Usuario** | `FrmConfiguracion.cs` | Calcula hash SHA256 antes de insertar |
| **Validación** | `LoginService.cs` | Compara hash en consulta SQL |
| **Base de Datos** | `Usuarios.PasswordHash` | Almacena hash de 64 caracteres |

## 🔧 Cambios Implementados

### 1. Login.cs

**Librerías agregadas:**
```csharp
using System.Security.Cryptography;
using System.Text;
```

**Método agregado:**
```csharp
private string CalcularHashSHA256(string texto)
{
    using (SHA256 sha256 = SHA256.Create())
    {
        byte[] bytes = Encoding.UTF8.GetBytes(texto);
        byte[] hash = sha256.ComputeHash(bytes);

        StringBuilder builder = new StringBuilder();
        foreach (byte b in hash)
        {
            builder.Append(b.ToString("x2"));
        }

        return builder.ToString();
    }
}
```

**Modificación en button1_Click:**
```csharp
// ANTES
Usuario? usuario = loginService.ValidarUsuario(textBox1.Text, textBox2.Text);

// AHORA
string passwordHash = CalcularHashSHA256(textBox2.Text);
Usuario? usuario = loginService.ValidarUsuario(textBox1.Text, passwordHash);
```

### 2. FrmConfiguracion.cs

Ya implementado en versión anterior. El mismo método `CalcularHashSHA256()` se usa al crear usuarios.

### 3. LoginService.cs

Ya configurado. La consulta compara directamente el hash:

```sql
SELECT ... FROM Usuarios u
INNER JOIN Roles r ON u.IdRol = r.IdRol
WHERE u.UserAlias = @Usuario 
  AND u.PasswordHash = @Password  -- @Password ya es hash SHA256
  AND u.Activo = 1
```

## ⚙️ Configuración Inicial

### Paso 1: Backup de la Base de Datos

**IMPORTANTE**: Antes de hacer cualquier cambio, haz un backup:

```sql
-- En SQL Server Management Studio
USE master;
BACKUP DATABASE RetornoMaster
TO DISK = 'C:\Backups\RetornoMaster_PreSHA256.bak'
WITH FORMAT, INIT, NAME = 'Backup antes de migración SHA256';
```

### Paso 2: Ejecutar Script de Migración

1. Abre SQL Server Management Studio
2. Conecta al servidor de base de datos
3. Abre el archivo: `SQL_Scripts\Migracion_Passwords_SHA256.sql`
4. Revisa el script (especialmente los usuarios a actualizar)
5. Ejecuta el script

### Paso 3: Verificar Migración

```sql
USE RetornoMaster;

-- Verificar que todos los usuarios tengan hash de 64 caracteres
SELECT 
    UserAlias,
    LEN(PasswordHash) AS Longitud,
    CASE 
        WHEN LEN(PasswordHash) = 64 THEN '✅ OK'
        ELSE '❌ ERROR'
    END AS Estado
FROM Usuarios;
```

## 🔄 Migración de Usuarios

### Usuarios con Contraseñas Conocidas

Si conoces las contraseñas actuales de tus usuarios:

#### Opción 1: Usar el Script SQL

Edita `SQL_Scripts\Migracion_Passwords_SHA256.sql` y actualiza los hashes:

```sql
-- Ejemplo: Usuario con contraseña conocida
UPDATE Usuarios 
SET PasswordHash = '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918'
WHERE UserAlias = 'jnieto' AND LEN(PasswordHash) < 64;
```

#### Opción 2: Usar Herramienta Online

1. Ve a: https://emn178.github.io/online-tools/sha256.html
2. Ingresa la contraseña en texto plano
3. Copia el hash resultante (64 caracteres)
4. Actualiza la BD:

```sql
UPDATE Usuarios 
SET PasswordHash = 'HASH_COPIADO_AQUI'
WHERE UserAlias = 'nombreusuario';
```

#### Opción 3: Usar el Sistema

1. Login como admin (después de migrar al menos un admin)
2. Ve a Configuración
3. Haz clic en "Agregar Usuario"
4. Crea el usuario con la contraseña deseada
5. El sistema automáticamente guarda el hash

### Usuarios con Contraseñas Desconocidas

Si no conoces las contraseñas:

#### Opción 1: Asignar Contraseña Temporal

```sql
-- Contraseña temporal: "CambiarMe2025"
-- Hash SHA256: (calcular usando herramienta online o método en código)

UPDATE Usuarios 
SET PasswordHash = 'HASH_DE_CambiarMe2025'
WHERE UserAlias = 'usuario_sin_password';
```

Luego notifica al usuario para que cambie su contraseña (requiere implementar función de cambio de password).

#### Opción 2: Recrear el Usuario

1. Elimina el usuario antiguo (guarda su información primero)
2. Créalo nuevamente usando el sistema
3. Asigna una contraseña nueva

### Tabla de Hashes Comunes

Para testing y desarrollo:

| Contraseña | Hash SHA256 |
|-----------|-------------|
| `admin1234` | `8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918` |
| `test123` | `ecd71870d1963316a97e3ac3408c9835ad8cf0f3c1bc703527c30265534f75ae` |
| `password` | `5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8` |
| `12345678` | `ef797c8118f02dfb649607dd5d3f8c7623048c9c063d532cc95c5ed7a898a64f` |

## 🧪 Pruebas

### Test 1: Crear Nuevo Usuario

1. **Login como Admin**
   - Usuario: `jnieto`
   - Password: `admin1234`

2. **Ir a Configuración**
   - Menú → Configuración

3. **Agregar Usuario**
   - Clic en "➕ Agregar Usuario"
   - UserAlias: `testuser`
   - Password: `test123`
   - Nombre: `Usuario`
   - Apellido: `Prueba`
   - Activo: `Sí`
   - Rol: `Usuario`
   - Clic en "Guardar"

4. **Verificar en BD**
   ```sql
   SELECT UserAlias, PasswordHash, LEN(PasswordHash) 
   FROM Usuarios 
   WHERE UserAlias = 'testuser';

   -- Esperado:
   -- UserAlias: testuser
   -- PasswordHash: ecd71870d1963316a97e3ac3408c9835ad8cf0f3c1bc703527c30265534f75ae
   -- LEN: 64
   ```

### Test 2: Login con Usuario Nuevo

1. **Cerrar Sesión**

2. **Login**
   - Usuario: `testuser`
   - Password: `test123`
   - Clic en "Ingresar"

3. **Resultado Esperado**
   - ✅ Login exitoso
   - Se carga MainMenu
   - Se muestra nombre del usuario

### Test 3: Login con Password Incorrecta

1. **Cerrar Sesión**

2. **Login**
   - Usuario: `testuser`
   - Password: `incorrecta`
   - Clic en "Ingresar"

3. **Resultado Esperado**
   - ❌ Mensaje: "Usuario o contraseña incorrectos"
   - No se permite acceso

### Test 4: Verificar Todos los Usuarios

```sql
-- Verificar que todos tengan hash SHA256
SELECT 
    IdUsuario,
    UserAlias,
    LEFT(PasswordHash, 20) + '...' AS PasswordHash_Preview,
    LEN(PasswordHash) AS Longitud,
    CASE 
        WHEN LEN(PasswordHash) = 64 THEN '✅ OK'
        ELSE '❌ PENDIENTE'
    END AS Estado,
    Activo,
    IdRol
FROM Usuarios
ORDER BY IdUsuario;
```

## 🔍 Troubleshooting

### Problema 1: "Usuario o contraseña incorrectos" con credenciales correctas

**Causa**: El usuario tiene contraseña en texto plano en BD

**Solución**:
```sql
-- Verificar longitud del hash
SELECT UserAlias, LEN(PasswordHash) AS Longitud 
FROM Usuarios 
WHERE UserAlias = 'usuario_problema';

-- Si Longitud < 64, actualizar con hash correcto
UPDATE Usuarios 
SET PasswordHash = 'HASH_SHA256_64_CARACTERES'
WHERE UserAlias = 'usuario_problema';
```

### Problema 2: Todos los logins fallan después de migración

**Causa**: El hash calculado en el código no coincide con BD

**Solución**:
1. Verifica que `CalcularHashSHA256()` esté implementado correctamente
2. Prueba con un hash conocido:
   ```csharp
   string hash = CalcularHashSHA256("test123");
   MessageBox.Show(hash);
   // Debe mostrar: ecd71870d1963316a97e3ac3408c9835ad8cf0f3c1bc703527c30265534f75ae
   ```

### Problema 3: Usuario específico no puede hacer login

**Diagnóstico**:
```sql
-- Ver información del usuario
SELECT 
    UserAlias,
    PasswordHash,
    LEN(PasswordHash) AS Longitud,
    Activo,
    IdRol
FROM Usuarios 
WHERE UserAlias = 'usuario_problema';
```

**Verificar**:
- ¿Activo = 1? (debe ser 1)
- ¿LEN(PasswordHash) = 64? (debe ser 64)
- ¿IdRol existe en tabla Roles? (debe existir)

### Problema 4: Error al crear usuario nuevo

**Error típico**: "Violación de restricción FOREIGN KEY"

**Causa**: IdRol no existe en tabla Roles

**Solución**:
```sql
-- Verificar roles disponibles
SELECT IdRol, NombreRol FROM Roles;

-- Si falta algún rol, agregarlo
INSERT INTO Roles (IdRol, NombreRol) VALUES (1, 'Admin');
INSERT INTO Roles (IdRol, NombreRol) VALUES (2, 'Usuario');
```

## 🔒 Seguridad

### Nivel Actual de Seguridad

✅ **Implementado**:
- Hash SHA256 (256 bits)
- Contraseñas nunca en texto plano
- Hash irreversible
- Mismo algoritmo en todo el sistema

⚠️ **Limitaciones**:
- No usa salt (vulnerable a rainbow tables)
- SHA256 simple (no es el más seguro para passwords)
- No hay límite de intentos de login
- No hay expiración de contraseñas

### Mejoras Futuras Recomendadas

#### 1. Implementar PBKDF2 con Salt

```csharp
// Ejemplo de implementación
private (string hash, string salt) HashPasswordPBKDF2(string password)
{
    byte[] saltBytes = new byte[32];
    using (var rng = RandomNumberGenerator.Create())
    {
        rng.GetBytes(saltBytes);
    }

    string salt = Convert.ToBase64String(saltBytes);

    using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 
        10000, HashAlgorithmName.SHA256))
    {
        byte[] hash = pbkdf2.GetBytes(32);
        return (Convert.ToBase64String(hash), salt);
    }
}
```

#### 2. Agregar Campo Salt en BD

```sql
ALTER TABLE Usuarios
ADD PasswordSalt NVARCHAR(100) NULL;
```

#### 3. Límite de Intentos de Login

```csharp
// Agregar campo en Usuarios
ALTER TABLE Usuarios
ADD IntentosLogin INT DEFAULT 0,
    UltimoIntentoLogin DATETIME NULL,
    BloqueadoHasta DATETIME NULL;
```

#### 4. Auditoría de Accesos

```sql
CREATE TABLE LogAccesos (
    IdLog INT PRIMARY KEY IDENTITY(1,1),
    UserAlias NVARCHAR(50),
    FechaHora DATETIME DEFAULT GETDATE(),
    Exitoso BIT,
    DireccionIP NVARCHAR(50),
    Detalles NVARCHAR(500)
);
```

### Mejores Prácticas

✅ **Hacer**:
- Usar contraseñas complejas (mínimo 8 caracteres, mezcla de tipos)
- Cambiar contraseñas periódicamente
- Mantener backup de la BD
- Monitorear intentos fallidos de login
- Usar HTTPS en producción

❌ **No Hacer**:
- Compartir contraseñas entre usuarios
- Almacenar contraseñas en archivos de texto
- Reutilizar contraseñas entre sistemas
- Ignorar actualizaciones de seguridad

## 📝 Checklist de Implementación

- [ ] Backup de base de datos realizado
- [ ] Script de migración ejecutado
- [ ] Todos los usuarios tienen hash SHA256 (64 caracteres)
- [ ] Test de login con usuario admin exitoso
- [ ] Test de creación de nuevo usuario exitoso
- [ ] Test de login con nuevo usuario exitoso
- [ ] Test de password incorrecta (debe fallar)
- [ ] Documentación actualizada
- [ ] Usuarios notificados sobre cambios
- [ ] Plan de recuperación en caso de problemas

## 📚 Referencias

- [SHA256 Online Tool](https://emn178.github.io/online-tools/sha256.html)
- [Microsoft - Data Protection](https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/)
- [OWASP - Password Storage](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)

## 📞 Soporte

Si encuentras problemas durante la implementación:

1. Revisa la sección [Troubleshooting](#troubleshooting)
2. Verifica los logs en SQL Server
3. Consulta la documentación en `DOCS/`
4. Restaura backup si es necesario

## ✅ Build Status

- **Build**: ✅ Exitoso
- **Tests**: ⏳ Pendiente de ejecutar
- **Migración**: ⏳ Pendiente de ejecutar

---

**Fecha de Implementación**: 2025  
**Versión**: 1.0  
**Sistema**: Retorno360Tacna
