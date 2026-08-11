# 🔐 Configuración de Login y Multi-Servidor

## 📋 Resumen de Cambios

Se simplificó el sistema de login para **eliminar el conflicto** entre:
- ✅ La tabla `Conexiones` (login inicial)
- ✅ La clase `GestorConexiones` (enrutamiento multi-servidor)

---

## 🎯 Nueva Arquitectura

### 1️⃣ Login (Servidor Principal)
```
┌─────────────────────────────────────┐
│ Login.cs                            │
├─────────────────────────────────────┤
│ 1. Carga SOLO servidor principal    │
│    (172.20.20.26)                   │
│                                     │
│ 2. Valida usuario/contraseña       │
│    contra: RetornoMaster.Usuarios  │
│                                     │
│ 3. Prueba conexión a servidor      │
│    principal                        │
│                                     │
│ 4. Pasa conexión principal a       │
│    MainMenu/RetornoService         │
└─────────────────────────────────────┘
```

### 2️⃣ RetornoService (Enrutamiento Automático)
```
┌─────────────────────────────────────┐
│ RetornoService.cs                   │
├─────────────────────────────────────┤
│ 1. Recibe conexión principal        │
│                                     │
│ 2. ConfigurarConexionesSecundarias()│
│    - Lee RAZONXTABLA               │
│    - Descubre bases en 172.20.21.33│
│    - Compara con NOM_TABLARAZON    │
│    - Registra en GestorConexiones  │
│                                     │
│ 3. Enruta automáticamente según BD  │
│    - SEERT_VIDRIOS → 172.20.21.33  │
│    - SEERT_SALAD → 172.20.20.26    │
│    - etc.                           │
└─────────────────────────────────────┘
```

---

## 🔧 Configuración de Base de Datos

### Tabla `Conexiones` (RetornoMaster)

La tabla `Conexiones` ahora **solo debe tener el servidor principal**:

```sql
-- Verificar configuración actual
SELECT * FROM RetornoMaster.dbo.Conexiones WHERE Activo = 1;

-- DEBE EXISTIR SOLO UNA CONEXIÓN ACTIVA:
-- IdConexion | NombreConexion        | Servidor      | UsuarioSQL  | PasswordSQL | Activo
-- -----------|----------------------|---------------|-------------|-------------|-------
-- 1          | Servidor Principal   | 172.20.20.26  | MedTiempos  | T3ch4dm1n   | 1
```

### ⚠️ SI TIENES MÚLTIPLES CONEXIONES ACTIVAS:

```sql
-- DESACTIVAR conexiones secundarias (no eliminarlas, por si acaso)
UPDATE RetornoMaster.dbo.Conexiones 
SET Activo = 0 
WHERE Servidor != '172.20.20.26';

-- Verificar que solo quede una activa
SELECT * FROM RetornoMaster.dbo.Conexiones WHERE Activo = 1;
```

---

## ✅ Verificación del Sistema

### 1. Login
```
✓ Al abrir la aplicación:
  - ComboBox muestra SOLO "Servidor Principal"
  - ComboBox está DESHABILITADO (no se puede cambiar)
  - Usuario ingresa credenciales
  - Sistema valida contra RetornoMaster.Usuarios
  - Sistema prueba conexión al servidor principal
```

### 2. Carga de DiagramasOperacion
```
✓ Al cargar el dashboard:
  - RetornoService se conecta al servidor principal
  - Ejecuta ConfigurarConexionesSecundarias()
  - Descubre automáticamente SEERT_VIDRIOS en 172.20.21.33
  - Enruta correctamente según la base de datos
```

### 3. Cálculo de Retorno
```
✓ Al calcular retornos:
  - Sistema detecta servidor de origen de la razón social
  - Si base está en servidor secundario:
    → Usa GestorConexiones para conectarse
  - Si base está en servidor principal:
    → Usa conexión principal
  - Valida que origen y destino estén en el MISMO servidor
```

---

## 🐛 Solución de Problemas

### Error: "Usuario o contraseña incorrectos"
```
CAUSA: Credenciales incorrectas en la tabla Usuarios
SOLUCIÓN:
  1. Verificar usuario en RetornoMaster.dbo.Usuarios
  2. Confirmar que Activo = 1
  3. Verificar PasswordHash
```

### Error: "No se pudo conectar al servidor principal"
```
CAUSA: Servidor 172.20.20.26 no responde
SOLUCIÓN:
  1. Verificar conectividad de red
  2. Ping 172.20.20.26
  3. Verificar que SQL Server esté activo
  4. Verificar firewall
```

### Error: Login attempt failed for user 'MedTiempos' (en servidor secundario)
```
CAUSA: Usuario SQL no configurado en servidor secundario
SOLUCIÓN:
  Ejecutar script: SQL_Configurar_Servidor_Secundario.sql
```

### Error: "El ComboBox muestra múltiples servidores"
```
CAUSA: Múltiples conexiones activas en tabla Conexiones
SOLUCIÓN:
  -- Desactivar conexiones secundarias
  UPDATE RetornoMaster.dbo.Conexiones 
  SET Activo = 0 
  WHERE Servidor != '172.20.20.26';
```

---

## 📊 Flujo Completo de Datos

```
┌──────────────┐
│   USUARIO    │
└──────┬───────┘
       │
       │ Ingresa credenciales
       ▼
┌──────────────────────────────────────┐
│ Login.cs                             │
│ ┌──────────────────────────────────┐ │
│ │ 1. LoginService.ObtenerConexiones││ │
│ │    → Lee tabla Conexiones        ││ │
│ │    → Filtra 172.20.20.26        ││ │
│ └──────────────────────────────────┘ │
│ ┌──────────────────────────────────┐ │
│ │ 2. LoginService.ValidarUsuario   ││ │
│ │    → Conecta a RetornoMaster     ││ │
│ │    → SELECT en Usuarios          ││ │
│ └──────────────────────────────────┘ │
│ ┌──────────────────────────────────┐ │
│ │ 3. Conexion.ProbarConexion()     ││ │
│ │    → Intenta abrir conexión      ││ │
│ │    → Valida credenciales SQL     ││ │
│ └──────────────────────────────────┘ │
└──────────────┬───────────────────────┘
               │
               │ ✓ Login exitoso
               ▼
┌──────────────────────────────────────┐
│ MainMenu.cs                          │
│  ├─ DiagramasOperacion               │
│  │   └─> RetornoService              │
│  │        └─> GestorConexiones       │
│  │             ├─ Principal: .26     │
│  │             └─ Secundario: .33    │
│  └─ FrmRetorno                       │
│      └─> RetornoService              │
│           └─> GestorConexiones       │
└──────────────────────────────────────┘
```

---

## 🎓 Conceptos Clave

### Separación de Responsabilidades

| Componente | Responsabilidad |
|-----------|----------------|
| `Login.cs` | Autenticación de **usuarios de la aplicación** contra `RetornoMaster.Usuarios` |
| `Conexion.cs` | Proporcionar conexiones SQL a un servidor específico |
| `GestorConexiones.cs` | Enrutar bases de datos al servidor correcto (principal/secundario) |
| `RetornoService.cs` | Lógica de negocio + auto-configuración multi-servidor |

### ¿Por qué solo el servidor principal en Login?

1. **Seguridad**: Centralizar autenticación en un solo punto
2. **Simplicidad**: El usuario no necesita elegir servidor
3. **Automatización**: `GestorConexiones` decide el enrutamiento
4. **Escalabilidad**: Fácil agregar nuevos servidores secundarios sin cambiar el login

---

## 📝 Siguiente Paso

Si necesitas agregar un **nuevo servidor secundario**:

1. **NO modificar** la tabla `Conexiones`
2. **NO modificar** `Login.cs`
3. **Solo agregar** la entrada en `RAZONXTABLA` con el nuevo servidor
4. `RetornoService` lo detectará automáticamente

Ejemplo:
```sql
-- Agregar nueva razón social en servidor terciario
INSERT INTO RetornoMaster.dbo.RAZONXTABLA (RazonSocial, Servidor)
VALUES ('NUEVA_RAZON', '172.20.21.50');

-- RetornoService auto-detectará y configurará el enrutamiento
```

---

## 🔍 Logs de Diagnóstico

Para ver cómo se están enrutando las conexiones:

```csharp
// En RetornoService, después de ConfigurarConexionesSecundarias():
var diagnostico = ObtenerDiagnosticoConexiones();
// diagnostico contiene:
// - Conexión principal
// - Bases de datos secundarias
// - Servidor de cada base
```

---

## ✨ Ventajas del Nuevo Sistema

✅ **Sin conflictos**: Login y enrutamiento están completamente separados  
✅ **Auto-configuración**: Nuevas bases se detectan automáticamente  
✅ **Mantenible**: Solo un punto de configuración (RAZONXTABLA)  
✅ **Escalable**: Fácil agregar servidores sin modificar código  
✅ **Seguro**: Autenticación centralizada en RetornoMaster  

---

**Última actualización**: Enero 2026  
**Versión**: 2.0 - Sistema Multi-Servidor Unificado
