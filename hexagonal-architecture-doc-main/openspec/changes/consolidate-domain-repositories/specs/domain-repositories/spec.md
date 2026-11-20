# Domain repositories

## ADDED Requirements

### Requirement: Repositorios de dominio
El sistema SHALL usar IVehicleRepository e IRentalRepository en dominio, implementados en infraestructura, sin dependencias del dominio hacia aplicación o frameworks.

#### Scenario: Acceso desde aplicación
- **WHEN** un handler necesita persistencia
- **THEN** utiliza los repositorios de dominio sin stores específicos de caso de uso.

### Requirement: Disponibilidad integrada
IVehicleRepository SHALL ofrecer el listado de disponibilidad en una sola consulta sin seguimiento y sin escrituras.

#### Scenario: Flota mixta
- **WHEN** existen vehículos elegibles, antiguos, futuros y alquilados
- **THEN** se devuelven solo los elegibles sin alquiler activo, en orden estable.

### Requirement: Compatibilidad y concurrencia
La refactorización SHALL conservar contratos REST, integridad y comportamiento de devolución.

#### Scenario: Escrituras concurrentes
- **WHEN** compiten alquileres o devoluciones
- **THEN** se conservan la exclusividad y la primera fecha de devolución.
