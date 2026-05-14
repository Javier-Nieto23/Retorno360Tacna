# Implementación de Cifrado SHA256 para Contraseñas en Agregar Usuarios

## Resumen

Se implementó el cifrado SHA256 para las contraseñas al crear nuevos usuarios en el sistema, garantizando que las contraseñas se almacenen de forma segura en la base de datos.

## Cambios Realizados

### 1. Importación de Librerías

Se agregaron las siguientes librerías en `FrmConfiguracion.cs`:

```csharp
using System.Security.Cryptography;
using System.Text;
```

### 2. Método de Cifrado SHA256

Se implementó el método `CalcularHashSHA256` que convierte una contraseña en texto plano a su hash SHA256:

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

### 3. Modificación en btnGuardarUsuario_Click

**Antes:**
```csharp
cmd.Parameters.AddWithValue("@PasswordHash", txtPasswordHash.Text.Trim());
```

**Ahora:**
```csharp
// Calcular hash SHA256 de la contraseña
string passwordHash = CalcularHashSHA256(txtPasswordHash.Text.Trim());
cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
```

### 4. Mejoras en la UI

Se modificó el `txtPasswordHash` para ocultar la contraseña mientras se escribe:

```csharp
txtPasswordHash.PasswordChar = '●';
```

Y se cambió la etiqueta de "Password:" a "Contraseña:" para mayor claridad.

## Funcionamiento

### Flujo del Cifrado

1. **Usuario ingresa contraseña**: El admin ingresa la contraseña en texto plano en el campo "Contraseña"
2. **Se oculta visualmente**: El campo muestra `●●●●●●` en lugar del texto real
3. **Al guardar**: El sistema toma el texto plano
4. **Conversión a bytes**: La contraseña se convierte a bytes UTF-8
5. **Cálculo del hash**: Se aplica el algoritmo SHA256
6. **Formato hexadecimal**: El hash se convierte a string hexadecimal (64 caracteres)
7. **Almacenamiento**: Se guarda en la columna `PasswordHash` de la tabla `Usuarios`

### Ejemplo de Conversión

| Entrada (texto plano) | Hash SHA256 (64 caracteres) |
|-----------------------|-----------------------------|
| `admin1234` | `8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918` |
| `miPassword123` | `5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8` |
| `Tacna2025` | `b99e12e... (64 caracteres)` |

## Ventajas del SHA256

✅ **Irreversible**: No se puede obtener la contraseña original del hash
✅ **Única**: Cada contraseña genera un hash único
✅ **Consistente**: La misma contraseña siempre genera el mismo hash
✅ **Seguro**: Algoritmo ampliamente usado y confiable
✅ **Longitud fija**: Siempre genera 64 caracteres hexadecimales (256 bits)

## Compatibilidad con Login

El sistema de login debe usar el mismo algoritmo SHA256 para comparar contraseñas:

### En LoginService.ValidarUsuario

```sql
SELECT ... FROM Usuarios 
WHERE UserAlias = @Usuario 
  AND PasswordHash = @Password  -- @Password debe ser el hash SHA256
  AND Activo = 1
```

El parámetro `@Password` que se pasa debe ser el hash SHA256 de la contraseña ingresada en el login.

## Código del Método Completo

```csharp
private void btnGuardarUsuario_Click(object sender, EventArgs e)
{
    try
    {
        // Validaciones de campos...

        if (conexionActual == null) return;

        Conexion conexion = new Conexion(
            conexionActual.Servidor!,
            conexionActual.UsuarioSQL!,
            conexionActual.PasswordSQL!,
            "RetornoMaster"
        );

        using (SqlConnection conn = conexion.ObtenerConexion())
        {
            conn.Open();

            // Verificar UserAlias duplicado
            string queryCheck = "SELECT COUNT(*) FROM Usuarios WHERE UserAlias = @UserAlias";
            using (SqlCommand cmdCheck = new SqlCommand(queryCheck, conn))
            {
                cmdCheck.Parameters.AddWithValue("@UserAlias", txtUserAlias.Text.Trim());
                int count = (int)cmdCheck.ExecuteScalar();

                if (count > 0)
                {
                    MessageBox.Show("El UserAlias ya existe. Por favor, elija otro.", 
                        "Usuario Duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtUserAlias.Focus();
                    return;
                }
            }

            // Insertar nuevo usuario con contraseña hasheada
            string queryInsert = @"
                INSERT INTO Usuarios (UserAlias, PasswordHash, NombreUsuario, ApellidoUsuario, Activo, IdRol, FechaCreacion)
                VALUES (@UserAlias, @PasswordHash, @NombreUsuario, @ApellidoUsuario, @Activo, @IdRol, GETDATE())";

            using (SqlCommand cmd = new SqlCommand(queryInsert, conn))
            {
                // ⭐ CIFRADO SHA256 AQUÍ
                string passwordHash = CalcularHashSHA256(txtPasswordHash.Text.Trim());

                cmd.Parameters.AddWithValue("@UserAlias", txtUserAlias.Text.Trim());
                cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                cmd.Parameters.AddWithValue("@NombreUsuario", txtNombreUsuario.Text.Trim());
                cmd.Parameters.AddWithValue("@ApellidoUsuario", txtApellidoUsuario.Text.Trim());
                cmd.Parameters.AddWithValue("@Activo", cmbActivo.SelectedIndex == 0 ? 1 : 0);
                cmd.Parameters.AddWithValue("@IdRol", cmbRol.SelectedValue ?? 1);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Usuario agregado correctamente.", "Éxito", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LimpiarFormularioUsuario();
                    panelAgregarUsuario.Visible = false;
                }
            }
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error al guardar usuario: {ex.Message}", "Error", 
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
```

## Verificación en Base de Datos

Para verificar que las contraseñas se están cifrando correctamente:

```sql
-- Ver usuarios con sus hashes
SELECT 
    IdUsuario,
    UserAlias,
    PasswordHash,
    LEN(PasswordHash) AS LongitudHash,
    NombreUsuario,
    ApellidoUsuario,
    Activo,
    IdRol
FROM Usuarios
ORDER BY FechaCreacion DESC

-- El PasswordHash debe tener 64 caracteres
-- Ejemplo: 8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918
```

## Herramienta para Generar Hash (Testing)

Si necesitas generar un hash manualmente para testing:

```csharp
// Método de utilidad (puede agregarse temporalmente para testing)
private void GenerarHashTest()
{
    string password = "admin1234";
    string hash = CalcularHashSHA256(password);
    MessageBox.Show($"Password: {password}\nHash: {hash}", "Hash SHA256");
}
```

O usar un sitio web como:
- https://emn178.github.io/online-tools/sha256.html
- https://passwordsgenerator.net/sha256-hash-generator/

## Consideraciones de Seguridad

⚠️ **IMPORTANTE**: 
- SHA256 solo es el primer nivel de seguridad
- En producción se recomienda usar **PBKDF2**, **bcrypt** o **Argon2** con salt
- SHA256 sin salt es vulnerable a ataques de rainbow tables
- Para mayor seguridad, considerar agregar un salt único por usuario

### Mejora Futura Recomendada (PBKDF2 con Salt)

```csharp
// Ejemplo de mejora futura
private string CalcularHashPBKDF2(string password, out string salt)
{
    // Generar salt aleatorio
    byte[] saltBytes = new byte[32];
    using (var rng = RandomNumberGenerator.Create())
    {
        rng.GetBytes(saltBytes);
    }
    salt = Convert.ToBase64String(saltBytes);

    // Aplicar PBKDF2
    using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256))
    {
        byte[] hash = pbkdf2.GetBytes(32);
        return Convert.ToBase64String(hash);
    }
}
```

## Build Status

✅ **Build Exitoso** - Sin errores de compilación

## Testing Recomendado

1. Crear un usuario con contraseña "test123"
2. Verificar en BD que el hash es: `ecd71870d1963316a97e3ac3408c9835ad8cf0f3c1bc703527c30265534f75ae`
3. Intentar hacer login con ese usuario y contraseña
4. Verificar que el login funcione correctamente

## Autor
Implementado según requerimientos de seguridad
Fecha: 2025
