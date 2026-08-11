# 🔧 Solución de Problemas: Servidor Secundario

## ❌ Error: "Cannot open database 'SEERT_VIDRIOS' requested by the login"

### 📋 Descripción del Error

Cuando intentas calcular el porcentaje de retorno y aparece el siguiente error:

```
Error al calcular el porcentaje de retorno:
Error al calcular retorno:
Error al validar pedimentos cruzados:
Cannot open database 'SEERT_VIDRIOS' requested by the login.
The login failed for user 'MedTiempos'.
```

### ⚠️ Causa del Problema

Este error **NO es un problema de validación de usuario de la aplicación**. Es un problema de permisos SQL Server:

1. **El usuario de la aplicación (textbox login) se valida correctamente** contra `RetornoMaster.Usuarios`
2. **El problema es con la autenticación SQL hacia el servidor secundario** (`172.20.21.33` o `172.20.21.36`)
3. El usuario SQL `MedTiempos` no puede:
   - Conectarse al servidor secundario, O
   - No tiene permisos para abrir la base de datos `SEERT_VIDRIOS` en ese servidor

### ✅ Soluciones

#### Opción 1: Verificar que el usuario SQL existe en el servidor secundario

1. Conéctate al servidor secundario (`172.20.21.33`) con SQL Server Management Studio
2. Navega a: **Security > Logins**
3. Verifica que el usuario `MedTiempos` existe
4. Si NO existe, créalo con la misma contraseña que el servidor principal

```sql
-- En el servidor secundario 172.20.21.33
USE master;
GO

-- Crear el login si no existe
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'MedTiempos')
BEGIN
    CREATE LOGIN [MedTiempos] WITH PASSWORD = 'T3ch4dm1n';
END
GO
```

#### Opción 2: Dar permisos al usuario sobre las bases de datos

Una vez que el usuario existe, debe tener permisos en CADA base de datos:

```sql
-- En el servidor secundario 172.20.21.33
USE SEERT_VIDRIOS;
GO

-- Crear usuario en la base de datos
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'MedTiempos')
BEGIN
    CREATE USER [MedTiempos] FOR LOGIN [MedTiempos];
END
GO

-- Dar permisos de lectura
ALTER ROLE db_datareader ADD MEMBER [MedTiempos];
GO
```

**IMPORTANTE**: Repite esto para TODAS las bases de datos que están en el servidor secundario.

#### Opción 3: Dar permiso VIEW ANY DATABASE

Si hay muchas bases de datos, puedes dar un permiso global:

```sql
-- En el servidor secundario 172.20.21.33
USE master;
GO

GRANT VIEW ANY DATABASE TO [MedTiempos];
GO

-- Dar acceso a TODAS las bases de datos automáticamente
EXEC sp_MSforeachdb '
USE [?];
IF ''?'' NOT IN (''master'', ''tempdb'', ''model'', ''msdb'')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = ''MedTiempos'')
    BEGIN
        CREATE USER [MedTiempos] FOR LOGIN [MedTiempos];
    END
    ALTER ROLE db_datareader ADD MEMBER [MedTiempos];
END
';
GO
```

#### Opción 4: Usar credenciales diferentes por servidor

Si el servidor secundario requiere un usuario DIFERENTE, actualiza el código:

**Archivo**: `Retorno360Tacna\SERVICES\RetornoService.cs`

**Busca el método** `ConfigurarConexionesSecundarias()` (línea ~25)

**Cambia**:
```csharp
ConfigurarServidorSecundario(
    servidor: "172.20.21.33",
    usuario: "MedTiempos",      // ← Usuario del servidor PRINCIPAL
    password: "T3ch4dm1n"       // ← Contraseña del servidor PRINCIPAL
);
```

**Por**:
```csharp
ConfigurarServidorSecundario(
    servidor: "172.20.21.33",
    usuario: "UsuarioServidor2",     // ← Usuario específico del servidor secundario
    password: "PasswordServidor2"    // ← Contraseña del servidor secundario
);
```

### 🔍 Cómo Verificar la Configuración

#### 1. Verificar si el usuario puede conectarse

```sql
-- Desde SQL Management Studio, conectarse al servidor 172.20.21.33
-- usando el usuario MedTiempos y contraseña T3ch4dm1n
```

Si la conexión falla → **El login no existe o la contraseña es incorrecta**

#### 2. Verificar permisos en la base de datos

```sql
-- Una vez conectado al servidor secundario
USE SEERT_VIDRIOS;
GO

SELECT * FROM sys.database_principals WHERE name = 'MedTiempos';
GO

-- Ver roles del usuario
SELECT 
    dp.name AS UserName,
    rp.name AS RoleName
FROM sys.database_principals dp
LEFT JOIN sys.database_role_members drm ON dp.principal_id = drm.member_principal_id
LEFT JOIN sys.database_principals rp ON drm.role_principal_id = rp.principal_id
WHERE dp.name = 'MedTiempos';
GO
```

#### 3. Probar acceso desde código

El código ahora incluye mensajes de error detallados. Cuando falla, mostrará:

```
❌ No se puede abrir la base de datos 'SEERT_VIDRIOS'.

Servidor: 172.20.21.33

Posibles causas:
1. El usuario SQL no tiene permiso para acceder a esta base de datos
2. La base de datos no existe en el servidor 172.20.21.33
3. Las credenciales del servidor secundario son incorrectas

Por favor, verifica la configuración en RetornoService.ConfigurarConexionesSecundarias()
```

### 📊 Resumen Visual

```
┌─────────────────────────────────────────────────────────────────┐
│ FLUJO DE AUTENTICACIÓN                                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│ 1. Usuario inicia sesión en la app                             │
│    ├─> Textbox usuario/contraseña                              │
│    └─> ✅ Valida contra RetornoMaster.Usuarios (172.20.20.26)  │
│                                                                 │
│ 2. Usuario calcula retorno                                     │
│    ├─> Selecciona razón social                                 │
│    └─> Selecciona base de datos (ej: SEERT_VIDRIOS)            │
│                                                                 │
│ 3. Sistema determina servidor                                  │
│    ├─> ¿SEERT_VIDRIOS está en servidor principal?              │
│    │   NO → Está en servidor secundario (172.20.21.33)         │
│    │                                                            │
│    └─> ✅ Conectar con SQL User del servidor secundario        │
│         (MedTiempos / T3ch4dm1n configurado en código)          │
│                                                                 │
│ 4. ❌ Error si:                                                 │
│    ├─> El usuario SQL no existe en servidor secundario         │
│    ├─> La contraseña es incorrecta                             │
│    └─> El usuario no tiene permisos en la base de datos        │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### 🎯 Conclusión

**Respuesta a tu pregunta**:
> "este error es provocado por que no se puede validar el usuario SQL?"

**Sí, PERO** no es el usuario que ingresa en el login de la aplicación.

- ✅ El usuario de la **aplicación** (login screen) se valida correctamente
- ❌ El usuario **SQL del servidor secundario** no puede autenticarse o no tiene permisos

El sistema ahora usa:
- **Usuario de App**: Para login y validación contra RetornoMaster
- **Usuario SQL Primario**: Para bases en servidor 172.20.20.26
- **Usuario SQL Secundario**: Para bases en servidor 172.20.21.33 (aquí está fallando)

**Acción requerida**: Crear o verificar credenciales SQL en el servidor secundario 172.20.21.33.
