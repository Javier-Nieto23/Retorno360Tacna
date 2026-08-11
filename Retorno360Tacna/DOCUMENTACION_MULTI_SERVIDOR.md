# Soporte Multi-Servidor para Cálculo de Retorno

## Problema Resuelto

Anteriormente, cuando se intentaba calcular el retorno entre dos bases de datos ubicadas en **servidores SQL diferentes**, el sistema mostraba un error:

```
❌ ERROR DE CONFIGURACIÓN MULTI-SERVIDOR

Las bases de datos están en servidores diferentes:
  • Base seleccionada: SEERT_TODCO → 172.20.20.26
  • Base origen: SEERT_RASMUSSEN → 172.20.21.36
```

### Ejemplo del Problema

**Caso:** Razón Social "RASMUSSEN DE TECATE SA DE CV"

| Tabla | Campo | Valor |
|-------|-------|-------|
| NOM_TABLARAZON | NOMBRE_TABLA | SEERT_TODCO |
| NOM_TABLARAZON | IdRazon | 17 |
| RAZONXTABLA | NOMBRE_RAZON | RASMUSSEN DE TECATE SA DE CV |
| RAZONXTABLA | DB | SEERT_RASMUSSEN |
| RAZONXTABLA | ConnExterna | S |
| RAZONXTABLA | IdConexion | 2 |
| Conexiones | Servidor | 172.20.21.36 |

**Resultado:**
- Base seleccionada (SEERT_TODCO) → Servidor principal: 172.20.20.26
- Base origen (SEERT_RASMUSSEN) → Servidor externo: 172.20.21.36
- ❌ Error: No se puede hacer JOIN cross-server

## Solución Implementada

Se ha implementado una **estrategia alternativa multi-servidor** que:

1. ✅ **Detecta** automáticamente cuando las bases están en servidores diferentes
2. ✅ **Obtiene pedimentos** de cada servidor por separado
3. ✅ **Valida en memoria** la correspondencia entre pedimentos
4. ✅ **Continúa** con el cálculo normal de retorno

### Flujo de Trabajo

#### Escenario 1: Mismo Servidor (Comportamiento Original)

```
┌─────────────────────────────────────────────┐
│ Servidor: 172.20.20.26                      │
├─────────────────────────────────────────────┤
│                                             │
│  Base A: SEERT_TODCO                        │
│  ├── Di_Pedimento                           │
│  └── De_Pedimento                           │
│                                             │
│  Base B: SEERT_RASMUSSEN                    │
│  └── TR_Glosa                               │
│                                             │
│  ✅ JOIN directo en SQL                     │
│                                             │
└─────────────────────────────────────────────┘
```

**Método usado:** `ValidarPedimentosCruzados` (original)
- Ejecuta un solo query con JOIN cross-database
- Más rápido y eficiente

#### Escenario 2: Servidores Diferentes (Nueva Estrategia)

```
┌─────────────────────────┐    ┌─────────────────────────┐
│ Servidor A:             │    │ Servidor B:             │
│ 172.20.20.26            │    │ 172.20.21.36            │
├─────────────────────────┤    ├─────────────────────────┤
│                         │    │                         │
│ SEERT_TODCO             │    │ SEERT_RASMUSSEN         │
│ ├── Di_Pedimento        │    │ └── TR_Glosa            │
│ └── De_Pedimento        │    │                         │
│                         │    │                         │
│ Query 1 ────────────────┼────┼─► Pedimentos Base A     │
│                         │    │                         │
│                         │    │ Query 2 ◄───────────────┤
│                         │    │   Pedimentos TR_Glosa   │
└─────────────────────────┘    └─────────────────────────┘
                 │                         │
                 └──────────┬──────────────┘
                            ▼
                  ┌────────────────────┐
                  │ Aplicación         │
                  │ Validación en      │
                  │ Memoria            │
                  └────────────────────┘
```

**Método usado:** `ValidarPedimentosCruzadosMultiServidor` (nuevo)

### Pasos de la Estrategia Multi-Servidor

**PASO 1: Obtener pedimentos de la base seleccionada**
```csharp
ObtenerPedimentosDeBaseDatos(baseDatosSeleccionada, fechaInicio, fechaFin)
```
- Conexión: Servidor A (172.20.20.26)
- Consulta: `Di_Pedimento` y `De_Pedimento`
- Resultado: Lista de pedimentos de importación/exportación

**PASO 2: Obtener pedimentos de la glosa**
```csharp
ObtenerPedimentosDeGlosa(baseDatosOrigen, fechaInicio, fechaFin)
```
- Conexión: Servidor B (172.20.21.36)
- Consulta: `TR_Glosa` (con filtro `Gl_OrigenZipGlosa = 'S'`)
- Resultado: Lista de pedimentos en la glosa

**PASO 3: Validación cruzada en memoria**
```csharp
// Para cada pedimento de la base seleccionada
foreach (var pedBase in pedimentosBaseSeleccionada)
{
    // Buscar coincidencia en TR_Glosa
    var existeEnGlosa = pedimentosGlosa.Any(pg =>
        pg.Aduana == pedBase.Aduana &&
        pg.Patente == pedBase.Patente &&
        pg.Pedimento == pedBase.Pedimento &&
        pg.Tipo == pedBase.Tipo);

    if (existeEnGlosa)
    {
        pedimentosValidos.Add(pedBase);
    }
}
```

**PASO 4: Continuar con cálculo normal**
- Usa los pedimentos validados
- Calcula importaciones y exportaciones
- Genera el resultado de retorno

## Implementación Técnica

### Archivos Modificados

**`SERVICES/RetornoService.cs`**

#### Métodos Nuevos

1. **`ValidarPedimentosCruzadosMultiServidor`**
   - Coordina la estrategia multi-servidor
   - Llama a los otros métodos auxiliares
   - Retorna lista de pedimentos validados

2. **`ObtenerPedimentosDeBaseDatos`**
   - Obtiene pedimentos de `Di_Pedimento` y `De_Pedimento`
   - Se conecta al servidor de la base seleccionada
   - Retorna lista de pedimentos sin validar

3. **`ObtenerPedimentosDeGlosa`**
   - Obtiene pedimentos de `TR_Glosa`
   - Se conecta al servidor de la base origen
   - Filtra por `Gl_OrigenZipGlosa = 'S'`

#### Método Modificado

**`ValidarPedimentosCruzados`**

Antes:
```csharp
if (!servidorSeleccionada.Equals(servidorOrigen))
{
    throw new Exception("ERROR DE CONFIGURACIÓN MULTI-SERVIDOR...");
}
```

Después:
```csharp
if (!servidorSeleccionada.Equals(servidorOrigen))
{
    // Usar estrategia alternativa
    return ValidarPedimentosCruzadosMultiServidor(
        baseDatosSeleccionada,
        baseDatosOrigen,
        fechaInicio,
        fechaFin
    );
}
```

## Ventajas

✅ **Transparente para el usuario**: No requiere configuración adicional  
✅ **Automático**: Detecta y usa la estrategia correcta  
✅ **Compatible**: Funciona con la configuración existente de `Conexiones`  
✅ **Sin Linked Servers**: No requiere configurar linked servers en SQL  
✅ **Mismo resultado**: Produce resultados idénticos al método original  

## Desventajas

⚠️ **Ligeramente más lento**: Hace 2 queries en lugar de 1  
⚠️ **Más uso de memoria**: Carga pedimentos en memoria para comparar  
⚠️ **Más tráfico de red**: Dos conexiones separadas en lugar de una  

## Consideraciones de Rendimiento

| Aspecto | Mismo Servidor | Multi-Servidor |
|---------|----------------|----------------|
| Queries ejecutados | 1 | 2 |
| Conexiones abiertas | 1 | 2 |
| Validación | En SQL | En memoria |
| Tiempo estimado | Rápido | Ligeramente más lento |
| Red | Una conexión | Dos conexiones |

**Recomendación:** Si es posible, mantener bases de datos relacionadas en el mismo servidor para mejor rendimiento.

## Debugging

El sistema genera logs de diagnóstico:

```
🔀 VALIDACIÓN MULTI-SERVIDOR
   Estrategia: Consultas separadas + validación en memoria

   📋 PASO 1: Obtener pedimentos de SEERT_TODCO
   ✅ Encontrados 150 pedimentos en SEERT_TODCO

   📋 PASO 2: Obtener pedimentos de TR_Glosa en SEERT_RASMUSSEN
   ✅ Encontrados 142 pedimentos en TR_Glosa

   🔍 PASO 3: Validación cruzada en memoria
   ✅ Pedimentos validados: 138
      • Importaciones: 75
      • Exportaciones: 63
```

## Casos de Uso

### Caso 1: Base de datos única en servidor principal
- **ConnExterna:** NULL
- **IdConexion:** NULL
- **Estrategia:** Método original (mismo servidor)

### Caso 2: Base de datos en servidor externo
- **ConnExterna:** S
- **IdConexion:** 2
- **Estrategia:** Multi-servidor (nueva implementación)

### Caso 3: Ambas bases en servidor externo (mismo)
- **ConnExterna:** S
- **IdConexion:** 2 (ambas)
- **Estrategia:** Método original (mismo servidor)

## Versión

- **Implementado:** Mayo 2026
- **Versión del sistema:** .NET 10
- **Archivo modificado:** `SERVICES/RetornoService.cs`
- **Métodos nuevos:** 3
- **Métodos modificados:** 1
