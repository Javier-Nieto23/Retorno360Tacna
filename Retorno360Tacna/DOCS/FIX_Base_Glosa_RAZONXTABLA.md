# Fix: Corrección de Lógica de Selección de Base Glosa

## Problema Identificado

El método `ObtenerResumenTablasPorBase()` en `ReporteIGIService.cs` estaba usando una lógica incorrecta para determinar la base de datos de glosa (TR_GLOSA).

### Lógica Anterior (INCORRECTA)
```csharp
// Buscaba cualquier base que contuviera "ABLE" o "GLOSA" en el nombre
var candidata = bases.FirstOrDefault(b => 
	!string.Equals(b.BaseDatos, baseDatos, StringComparison.OrdinalIgnoreCase)
	&& (b.BaseDatos.IndexOf("ABLE", StringComparison.OrdinalIgnoreCase) >= 0 
		|| b.BaseDatos.IndexOf("GLOSA", StringComparison.OrdinalIgnoreCase) >= 0));
```

**Resultado:** Seleccionaba `SEERT_Acme` cuando se consultaba `SEERT_Able`, causando que no se encontraran registros en TR_GLOSA.

### Lógica Nueva (CORRECTA)
```csharp
// Obtiene la base glosa directamente del campo DB de RAZONXTABLA
int idRazon = ObtenerIdRazonDesdeBaseDatos(baseDatos);
var razonSocial = ObtenerRazonSocial(idRazon);

if (!string.IsNullOrEmpty(razonSocial.BaseDatosOrigen))
{
	baseGlosa = razonSocial.BaseDatosOrigen;  // Este campo contiene la base real de TR_GLOSA
}
```

## Origen de Datos

La base de glosa correcta se obtiene de:

```sql
SELECT DB 
FROM RAZONXTABLA 
WHERE IdRazon = @IdRazon
```

El campo `DB` en `RAZONXTABLA` contiene el nombre de la base de datos donde se encuentra la tabla `TR_GLOSA` para esa razón social.

## Ejemplo de Datos

**Tabla RAZONXTABLA:**
```
IdRazon | NOMBRE_RAZON              | DB           | IdConexion
--------|---------------------------|--------------|------------
1       | MAM DE LA FRONTERA SA...  | Retorno2023  | NULL
```

**Tabla NOM_TABLARAZON (bases seleccionables):**
```
IdRazon | NOMBRE_TABLA  | IdConexion
--------|---------------|------------
1       | SEERT_Able    | 1
1       | SEERT_Acme    | 1
1       | SEERT_Bi      | 1
...
```

## Flujo Correcto

1. Usuario selecciona `SEERT_Able` (base cliente)
2. Sistema busca `IdRazon = 1` desde `NOM_TABLARAZON` 
3. Sistema consulta `RAZONXTABLA` con `IdRazon = 1`
4. Obtiene `DB = "Retorno2023"` (base glosa)
5. Consulta cliente: `SEERT_Able.dbo.Di_Pedimento`
6. Consulta glosa: `Retorno2023.dbo.TR_GLOSA` ✅

## Flujo Anterior (Incorrecto)

1. Usuario selecciona `SEERT_Able`
2. Sistema busca todas las bases de `IdRazon = 1`
3. Sistema filtra por nombre que contenga "ABLE"
4. Encuentra `SEERT_Acme` (¡incorrecta!)
5. Consulta cliente: `SEERT_Able.dbo.Di_Pedimento`
6. Consulta glosa: `SEERT_Acme.dbo.TR_GLOSA` ❌ (base incorrecta)

## Resultado

- ✅ **Ahora se usa la base glosa correcta** definida en `RAZONXTABLA.DB`
- ✅ **No depende de convenciones de nombres** ("ABLE", "GLOSA", etc.)
- ✅ **Sigue la estructura real** de las tablas del sistema
- ✅ **Permite TR_GLOSA en cualquier base** configurada en `RAZONXTABLA`

## Archivos Modificados

- `Retorno360Tacna\SERVICES\ReporteIGIService.cs` (líneas 93-122)

## Métodos Utilizados

- `ObtenerIdRazonDesdeBaseDatos(string baseDatos)` - Obtiene IdRazon desde NOM_TABLARAZON
- `ObtenerRazonSocial(int idRazon)` - Obtiene el registro de RAZONXTABLA incluyendo el campo DB
- `razonSocial.BaseDatosOrigen` - Propiedad que mapea el campo DB (base de glosa)

## Prueba

Ejecutar consulta de IGI para `SEERT_Able` con fecha `01/01/2026` - `31/01/2026`:

**Log esperado:**
```
>> Base de glosa desde RAZONXTABLA.DB: Retorno2023
```

En lugar de:
```
?? Base de glosa encontrada: SEERT_Acme  (incorrecto)
```
