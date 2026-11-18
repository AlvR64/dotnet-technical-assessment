# Proposal

## Why
Un alquiler debe poder finalizar conservando su historial y liberando las restricciones activas.

## What Changes
- Añadir devolución REST idempotente, incluso ante peticiones concurrentes.
- Verificar el flujo completo y los requisitos de pruebas del PDF.

## Capabilities
### New Capabilities
- `rental-return`: devolución idempotente de alquileres.
### Modified Capabilities
Ninguna.

## Impact
Api, ApplicationCore, Infrastructure, pruebas y README.PRUEBA.md. Depende del modelo y del caso de alquiler; la disponibilidad se evalúa sin estado duplicado. Se conservan los documentos originales.
