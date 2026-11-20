# Proposal

## Why
Facilitar la evaluación local con un vehículo disponible sin registrar datos previamente.

## What Changes
- Carga opcional de un vehículo y un alquiler histórico devuelto en una base vacía.
- Activación explícita en Compose después de migraciones.
- Protección de datos existentes y pruebas PostgreSQL aisladas.

## Capabilities
### New Capabilities
- `demo-data-seeding`: inicialización opcional e idempotente de datos de demostración.

### Modified Capabilities
Ninguna.

## Impact
Host, Infrastructure, Compose, pruebas y README.PRUEBA-TECNICA-ALVARO.md; no cambia el esquema ni los endpoints.
