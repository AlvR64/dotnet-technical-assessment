# Vehicle availability

## Purpose
Permitir consultar los vehículos elegibles y sin alquiler activo en una fecha de referencia actual.

## ADDED Requirements

### Requirement: Consulta REST de disponibilidad
El servicio SHALL ofrecer `GET /vehicles?available=true` y devolver un array con `id`, `licensePlate`, `brand`, `model` y `manufacturedOn`.

#### Scenario: Parámetro omitido
- **WHEN** se consulta `GET /vehicles` sin parámetro
- **THEN** se interpreta como `available=true` y responde 200

#### Scenario: Parámetro no soportado
- **WHEN** `available` es false o no es booleano válido
- **THEN** responde 400

#### Scenario: Ningún resultado
- **WHEN** no hay vehículos disponibles
- **THEN** responde 200 con un array vacío

### Requirement: Elegibilidad y ausencia de alquiler activo
El servicio SHALL incluir solo vehículos elegibles por antigüedad y sin alquiler activo, usando una única fecha UTC de referencia por consulta.

#### Scenario: Flota mixta
- **WHEN** hay vehículos libres elegibles, alquilados y envejecidos
- **THEN** devuelve únicamente los libres elegibles

#### Scenario: Historial devuelto
- **WHEN** un vehículo elegible solo tiene alquileres devueltos
- **THEN** aparece como disponible

#### Scenario: Cambio de fecha
- **WHEN** un vehículo supera el límite de cinco años
- **THEN** deja de aparecer sin borrar su registro ni su historial

#### Scenario: Límite inclusivo
- **WHEN** la fabricación coincide con el corte de cinco años de calendario
- **THEN** sigue siendo elegible

### Requirement: Consulta sin efectos secundarios
La consulta SHALL ser de solo lectura y ordenar por matrícula normalizada e identificador.

#### Scenario: Consulta repetida
- **WHEN** se consulta la disponibilidad
- **THEN** no se modifican vehículos ni alquileres
