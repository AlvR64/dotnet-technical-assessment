# Proposal

## Why
La flota necesita incorporar vehículos válidos mediante REST.

## What Changes
- Añadir `POST /vehicles` con validación, persistencia y errores coherentes.
- Incorporar pruebas unitarias, de host y funcionales sin host.

## Capabilities
### New Capabilities
- `vehicle-registration`: alta de vehículos de la flota.
### Modified Capabilities
Ninguna.

## Impact
Api, ApplicationCore, Infrastructure, pruebas y README.PRUEBA-TECNICA-ALVARO.md. Depende del modelo y migración del plan 1. Sin modificar documentos originales ni añadir clientes o endpoints adicionales.
