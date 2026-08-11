# Resumen de Correcciones - Reporte IGI

## Cambios Implementados

### 1. ✅ Corrección de Selección de Base Glosa
**Problema:** El sistema seleccionaba incorrectamente la base de glosa buscando nombres que contuvieran "ABLE" o "GLOSA".

**Solución:** Ahora se usa el campo `DB` de la tabla `RAZONXTABLA` que contiene la base de datos correcta donde está `TR_GLOSA`.

**Archivo:** `Retorno360Tacna\SERVICES\ReporteIGIService.cs` (líneas 93-122)

**Antes:**
```csharp
var candidata = bases.FirstOrDefault(b => 
	b.BaseDatos.IndexOf("ABLE", StringComparison.OrdinalIgnoreCase) >= 0 
	|| b.BaseDatos.IndexOf("GLOSA", StringComparison.OrdinalIgnoreCase) >= 0);
baseGlosa = candidata?.BaseDatos ?? baseDatos;
```

**Después:**
```csharp
int idRazon = ObtenerIdRazonDesdeBaseDatos(baseDatos);
var razonSocial = ObtenerRazonSocial(idRazon);
baseGlosa = razonSocial.BaseDatosOrigen;  // Campo DB de RAZONXTABLA
```

**Documentación:** `DOCS\FIX_Base_Glosa_RAZONXTABLA.md`

---

### 2. ✅ Implementación de Cálculo de Diferencia IGI

**Problema:** La tabla IGI no mostraba la diferencia entre IGI Calculado e IGI Pagado.

**Solución:** Se agregó la columna `Diferencia_IGI` con lógica especial para forma de pago '5'.

**Archivo:** `Retorno360Tacna\SERVICES\ReporteIGIService.cs`

#### Columna agregada:
```csharp
tablaIGI.Columns.Add("Diferencia_IGI", typeof(decimal));
```

#### Lógica de cálculo:
```csharp
if (FormaPago_IGI == "5")
{
	IGI_Pagado = 0;
	Diferencia_IGI = 0 - IGI_Calculado;
}
else
{
	Diferencia_IGI = IGI_Calculado - IGI_Pagado;
}
```

**Fórmula:**
- **Diferencia > 0**: Falta pagar (IGI_Calculado mayor que IGI_Pagado)
- **Diferencia < 0**: Sobrepago (IGI_Pagado mayor que IGI_Calculado)
- **Diferencia = 0**: Pago exacto

**Documentación:** `DOCS\FIX_Calculo_Diferencia_IGI.md`

---

### 3. ✅ Diagnóstico Mejorado

**Problema:** No había visibilidad de por qué algunas bases no mostraban datos.

**Solución:** Se agregaron logs detallados de diagnóstico:

- 📊 Cantidad de pedimentos cliente encontrados
- 📊 Cantidad de pedimentos glosa encontrados
- 🔗 Análisis de coincidencias de pedimentos
- ✅ Resultado del JOIN y conteo final
- 🌐 Servidor y conexión utilizada para cada base

**Ejemplo de log:**
```
🔍 Buscando conexión para 'SEERT_Able'...
   ✅ Encontrado en NOM_TABLARAZON
   📋 IdRazon: 1
   🔗 IdConexion: 1
   🌐 Servidor: 172.20.20.26

📊 Pedimentos cliente encontrados: 7
	  - 400-3621-6008837 | FP: 2026-01-05 | IGI: $88.00 | Forma: 0

📊 Pedimentos glosa IGI encontrados: 0
   ⚠️ NO se encontraron pedimentos en TR_GLOSA para el rango de fechas

🔗 Pedimentos en común: 0
✅ Registros IGI después del JOIN: 0
```

---

## Flujo Corregido

### Escenario: Usuario consulta `SEERT_Able`

#### 1. **Resolución de conexión cliente**
```
SEERT_Able → NOM_TABLARAZON → IdConexion: 1
		   → Conexiones → Servidor: 172.20.20.26
```

#### 2. **Resolución de base glosa**
```
SEERT_Able → IdRazon: 1
		   → RAZONXTABLA(IdRazon=1) → DB: "Retorno2023"  ✅ CORRECTO
```

#### 3. **Consultas SQL**
```sql
-- Cliente
SELECT ... FROM [SEERT_Able].dbo.Di_Pedimento ...

-- Glosa
SELECT ... FROM [Retorno2023].dbo.TR_GLOSA ...
```

#### 4. **JOIN y Cálculo**
```
pedimentosCliente JOIN pedimentosGlosa ON Pedimento + FormaPago
→ Agrupar por Año, Mes, FormaPago
→ Calcular Diferencia_IGI (con lógica especial para forma '5')
```

---

## Estructura de Datos

### Tabla IGI (DataTable)

| Columna          | Tipo    | Descripción                                      |
|------------------|---------|--------------------------------------------------|
| Año              | int     | Año del pedimento                                |
| Mes              | int     | Mes del pedimento                                |
| IGI_Pagado       | decimal | Monto pagado (0 si forma de pago = '5')         |
| IGI_Calculado    | decimal | Monto calculado por el sistema                   |
| **Diferencia_IGI** | **decimal** | **IGI_Calculado - IGI_Pagado** (NUEVA)       |
| FormaPago_IGI    | string  | Código de forma de pago ('0', '5', '21', etc.)  |

---

## Casos de Prueba

### ✅ Caso 1: Base con datos en mismo servidor
- Base cliente: `SEERT_Able`
- Conexión: `IdConexion = 1` → `172.20.20.26`
- Base glosa: `Retorno2023` (desde `RAZONXTABLA.DB`)
- **Resultado esperado:** Datos correctos si existen en TR_GLOSA

### ✅ Caso 2: Base con forma de pago '5'
- Pedimento con `FormaPago_IGI = '5'`
- **IGI_Pagado:** $1,500 (original en glosa)
- **IGI_Calculado:** $1,450
- **Resultado:**
  - `IGI_Pagado` mostrado: $0
  - `Diferencia_IGI`: -$1,450

### ✅ Caso 3: Base vacía o sin datos en rango
- Pedimentos cliente: 7
- Pedimentos glosa: 0
- **Resultado:** Grids vacíos, log indica "NO se encontraron pedimentos en TR_GLOSA"

---

## Archivos Modificados

1. **`Retorno360Tacna\SERVICES\ReporteIGIService.cs`**
   - Líneas 75-81: Agregada columna `Diferencia_IGI`
   - Líneas 93-122: Cambio de lógica de selección de base glosa
   - Líneas 378-396: Cálculo de diferencia con forma de pago '5'
   - Múltiples líneas: Logs de diagnóstico

2. **`Retorno360Tacna\DOCS\FIX_Base_Glosa_RAZONXTABLA.md`** (NUEVO)
   - Documentación de corrección de base glosa

3. **`Retorno360Tacna\DOCS\FIX_Calculo_Diferencia_IGI.md`** (NUEVO)
   - Documentación de cálculo de diferencia IGI

---

## Verificación

✅ **Build:** Compilación correcta sin errores  
✅ **Lógica:** Consistente con `ReporteIGIService_Extension.cs`  
✅ **Diagnóstico:** Logs detallados para troubleshooting  
✅ **Documentación:** Completa y actualizada

---

## Próximos Pasos Sugeridos

1. **Ejecutar consulta** para `SEERT_Able` con la fecha corregida
2. **Verificar** que ahora se use `Retorno2023` como base glosa
3. **Revisar** si `Retorno2023.dbo.TR_GLOSA` tiene datos para enero 2026
4. **Validar** cálculo de `Diferencia_IGI` con diferentes formas de pago

---

## Notas Importantes

- ⚠️ Si `RAZONXTABLA.DB` es NULL, el sistema usa la misma base cliente como fallback
- ⚠️ El formato de la columna `Diferencia_IGI` ya está configurado en `FrmReportes.cs` (línea 510-515)
- ✅ La lógica es consistente entre consulta única y consulta agregada (múltiples bases)
