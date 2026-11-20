# Demo data seeding

## ADDED Requirements

### Requirement: Carga opcional de datos de demostración
El servicio SHALL cargar un vehículo elegible y un alquiler devuelto únicamente con la opción activada y ambas tablas vacías.

#### Scenario: Primera inicialización
- **WHEN** se habilita la opción sobre una base migrada y vacía
- **THEN** se insertan ambos registros en una transacción y el vehículo aparece disponible.

#### Scenario: Base existente
- **WHEN** alguna de las tablas contiene datos
- **THEN** no se insertan ni modifican datos de demostración.

### Requirement: Idempotencia y seguridad
El servicio SHALL conservar datos y fechas ante reinicios e inicializadores simultáneos.

#### Scenario: Inicializaciones concurrentes
- **WHEN** dos inicializadores cooperantes se ejecutan simultáneamente
- **THEN** se conserva una sola pareja de registros sin inserciones parciales.

### Requirement: Activación local
Compose SHALL activar la carga de ejemplo; la opción SHALL estar desactivada por defecto fuera de Compose.

#### Scenario: Opción desactivada
- **WHEN** no se configura la opción
- **THEN** no se cargan datos de demostración.
