# Rental return

## Purpose
Permitir cerrar alquileres conservando su historial y liberando sus restricciones activas de forma idempotente.

## ADDED Requirements

### Requirement: Devolución REST
El servicio SHALL admitir `PUT /rentals/{rentalId}/return` sin requerir cuerpo ni aceptar una fecha del cliente.

#### Scenario: Alquiler activo
- **WHEN** el identificador corresponde a un alquiler activo
- **THEN** registra la fecha UTC del servidor y devuelve 204 sin cuerpo

#### Scenario: Identificador inválido
- **WHEN** el identificador es vacío o malformado
- **THEN** responde 400

#### Scenario: Alquiler inexistente
- **WHEN** el UUID es válido pero no corresponde a un alquiler
- **THEN** responde 404 sin modificar otros datos

### Requirement: Devolución idempotente y concurrente
El servicio SHALL conservar la primera fecha de devolución incluso cuando las peticiones sean concurrentes.

#### Scenario: Devolución repetida
- **WHEN** se devuelve un alquiler ya devuelto
- **THEN** responde 204 y conserva la fecha original

#### Scenario: Devoluciones simultáneas
- **WHEN** dos solicitudes intentan devolver el mismo alquiler con instantes diferentes
- **THEN** ambas responden 204 y ninguna sobrescribe la primera fecha confirmada

### Requirement: Disponibilidad después de devolver
El servicio SHALL liberar al cliente y conservar el historial; el vehículo solo volverá a estar disponible si continúa siendo elegible.

#### Scenario: Vehículo elegible
- **WHEN** se devuelve un vehículo aún elegible
- **THEN** el cliente puede alquilar de nuevo y el vehículo puede aparecer disponible

#### Scenario: Vehículo envejecido
- **WHEN** el vehículo supera el límite durante el alquiler
- **THEN** se permite la devolución pero no vuelve a estar disponible para alquiler
