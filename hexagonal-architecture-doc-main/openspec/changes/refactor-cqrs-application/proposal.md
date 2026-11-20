# Proposal

## Why
Los handlers de MediatR en API solo delegan en casos de uso y presenters, aumentando el número de intermediarios.

## What Changes
- Situar commands, queries y handlers en ApplicationCore y devolver resultados independientes de HTTP.
- Mantener las operaciones, los contratos REST y las garantías de persistencia existentes.
- Sustituir las decisiones anteriores de handlers y presenters en API por el nuevo diseño.

## Capabilities
### New Capabilities
- `application-cqrs`: organización de los casos de uso como handlers de aplicación.

### Modified Capabilities
Ninguna: no cambian los contratos de negocio.

## Impact
ApplicationCore, API, pruebas y documentación de la prueba; dominio y esquema de persistencia permanecen independientes de HTTP.
