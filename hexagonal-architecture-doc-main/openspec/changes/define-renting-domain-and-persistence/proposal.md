# Proposal

## Why

La infraestructura ya conecta con PostgreSQL, pero el contexto está vacío. Los futuros casos de uso necesitan un modelo de dominio y unas garantías de integridad comunes que funcionen también ante concurrencia.

## What Changes

- Definir entidades de vehículo y alquiler y sus invariantes, independientes de EF Core.
- Mapear el modelo a PostgreSQL, con claves, restricciones e índices únicos parciales para alquileres activos.
- Añadir migración inicial, Unit of Work, aplicación explícitamente habilitada de migraciones en Compose y pruebas aisladas.
- Documentar los supuestos y comandos en `README.PRUEBA.md`, preservando los documentos originales.
- Excluir endpoints, casos de uso HTTP y gestión de clientes; se abordarán posteriormente.

## Capabilities

### New Capabilities

- `fleet-rental-model`: invariantes de vehículo y alquiler y persistencia íntegra de sus datos.
- `database-schema-lifecycle`: creación y actualización reproducible del esquema local mediante migraciones.

### Modified Capabilities

Ninguna: todavía no hay especificaciones principales archivadas.

## Impact

Domain, Infrastructure, Host y configuración local de Compose; migraciones de EF Core, pruebas unitarias y de integración PostgreSQL, y documentación adicional. No se modifica el README original.
