# Design

## Context
Los alquileres activos se identifican por `returned_at IS NULL`; los índices parciales se liberan cuando se confirma la devolución.

## Goals / Non-Goals
**Goals:** devolución idempotente y segura bajo concurrencia. **Non-Goals:** autenticación, borrar historial o modificar disponibilidad almacenada.

## Decisions
- Caso de uso y puerto distinguen alquiler inexistente, ya devuelto y devolución confirmada; TimeProvider proporciona UTC. No condicionar la devolución a la edad del vehículo.
- Usar actualización condicional donde `returned_at IS NULL`; si no se modifica ninguna fila, consultar existencia y distinguir ya devuelto de inexistente. Alternativa de leer y actualizar sin protección descartada porque puede sobrescribir la primera fecha.
- Si EF ejecuta la actualización inmediatamente, documentar y probar esa frontera de escritura; no atribuir la confirmación a un Save posterior. No mantener un booleano adicional de disponibilidad.
- Evitar restricciones de ruta que conviertan un UUID malformado en 404; la validación debe responder 400.

## Risks / Trade-offs
- [Devoluciones simultáneas] → Probar dos contextos y asegurar que no se sobrescribe la primera fecha.
- [Sin identidad] → Documentar que no se autoriza propiedad del alquiler; no es una API lista para producción.

## Migration Plan
Implementar tras modelo y alquiler; comprobar interacción con el listado sin cambios de esquema previstos.
