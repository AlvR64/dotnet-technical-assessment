# Application CQRS

## ADDED Requirements

### Requirement: Commands y queries de aplicación
Las cuatro operaciones SHALL usar commands o queries y handlers de MediatR en ApplicationCore, sin dependencia de HTTP ni EF.

#### Scenario: Operación desde API
- **WHEN** una petición HTTP válida llega al controller
- **THEN** el controller envía un mensaje de aplicación y traduce su resultado a HTTP sin utilizar presenters.

### Requirement: Compatibilidad funcional
La refactorización SHALL conservar los contratos REST y las reglas temporales, de concurrencia e idempotencia existentes.

#### Scenario: Peticiones concurrentes
- **WHEN** compiten alquileres o devoluciones
- **THEN** se mantienen las mismas garantías de exclusividad y primera fecha de devolución.
