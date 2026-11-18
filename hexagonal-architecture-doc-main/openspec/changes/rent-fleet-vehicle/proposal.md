# Proposal

## Why
La empresa necesita alquilar vehículos sin permitir dos alquileres activos por persona ni por vehículo.

## What Changes
- Añadir `POST /rentals` con control de elegibilidad y conflictos.
- Garantizar coherencia ante solicitudes concurrentes y probarla con PostgreSQL.

## Capabilities
### New Capabilities
- `vehicle-rental`: alquiler inmediato y unicidad de alquileres activos.
### Modified Capabilities
Ninguna.

## Impact
Api, ApplicationCore, Infrastructure, pruebas y README.PRUEBA.md. Depende del modelo del plan 1. No introduce reservas futuras, pagos, autenticación ni gestión de clientes.
