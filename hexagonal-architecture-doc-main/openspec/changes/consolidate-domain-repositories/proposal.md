# Proposal

## Why
Los puertos de persistencia por operación fragmentan el acceso a vehículos y alquileres. Se consolidan en repositorios de dominio.

## What Changes
- Añadir interfaces de repositorio de vehículo y alquiler en dominio e implementaciones en infraestructura.
- Integrar el listado de disponibilidad en el repositorio de vehículos y eliminar stores y reader específicos.
- Conservar contratos REST y reglas de negocio.

## Capabilities
### New Capabilities
- `domain-repositories`: acceso a persistencia mediante repositorios de dominio.

### Modified Capabilities
Ninguna.

## Impact
Dominio, aplicación, infraestructura, pruebas y documentación propia; sin cambios de esquema.
