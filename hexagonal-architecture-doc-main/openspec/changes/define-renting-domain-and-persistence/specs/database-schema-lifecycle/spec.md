# Database schema lifecycle

## Purpose

Permitir crear y actualizar el esquema de renting de forma reproducible, conservando los datos al reiniciar el entorno local.

## ADDED Requirements

### Requirement: Esquema inicial reproducible
El proyecto SHALL incluir una migración versionada para crear las estructuras y restricciones del modelo en PostgreSQL.

#### Scenario: Base vacía
- **WHEN** se aplica la migración a una base vacía
- **THEN** se crean las tablas y restricciones necesarias y se registra la migración aplicada

### Requirement: Migraciones locales opt-in
El entorno Compose local SHALL habilitar explícitamente la aplicación de migraciones antes de aceptar peticiones; fuera de ese entorno SHALL estar desactivada por defecto.

#### Scenario: Arranque local inicial
- **WHEN** se inicia Compose con una base vacía y la opción local habilitada
- **THEN** la API aplica el esquema antes de comenzar a aceptar peticiones

#### Scenario: Arranque sin opción explícita
- **WHEN** la aplicación arranca sin habilitar la opción
- **THEN** no aplica migraciones automáticamente

#### Scenario: Reinicio con datos
- **WHEN** se reinicia el entorno con migraciones ya aplicadas y volumen conservado
- **THEN** no se recrea el esquema ni se pierden datos

#### Scenario: Fallo de migración
- **WHEN** está habilitada la migración automática y esta falla
- **THEN** el servicio no anuncia un arranque exitoso ni acepta peticiones como si el esquema estuviera listo

### Requirement: Aislamiento de pruebas de esquema
Las pruebas de integración SHALL usar una base de pruebas aislada y no alterar los datos de desarrollo.

#### Scenario: Pruebas repetidas
- **WHEN** se ejecutan las pruebas de integración varias veces
- **THEN** cada ejecución puede preparar su esquema sin modificar la base de desarrollo
