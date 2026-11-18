# Design

## Context
PostgreSQL ya dispone de índices parciales únicos para cliente y vehículo activos. El identificador del cliente es externo y debe mantenerse estable.

## Goals / Non-Goals
**Goals:** alquiler inmediato seguro ante concurrencia. **Non-Goals:** reservas futuras, clientes, pagos o autenticación.

## Decisions
- Orquestar mediante caso de uso, puertos específicos, output port y presenter; capturar una vez el instante UTC con TimeProvider y usar la política de elegibilidad del dominio.
- Comprobaciones previas permiten mensajes claros, pero la inserción confirmada y los índices parciales garantizan exclusividad. Identificar SQLSTATE 23505 y nombre de índice en Infrastructure; traducir solo restricciones conocidas a errores tipados.
- Una sola inserción y Unit of Work scoped al mismo contexto. Desechar el contexto tras un fallo; no almacenar IsAvailable ni usar locks en memoria. Alternativa de comprobar antes de insertar sin índice descartada por carreras.
- No inventar Location ni endpoints GET adicionales. Errores desconocidos no se traducen a conflictos.

## Risks / Trade-offs
- [Identidad externa inestable] → Documentar que la regla depende de un customerId consistente.
- [Peticiones simultáneas] → Probar ambas restricciones con contextos separados, sin sleeps.

## Migration Plan
Implementar tras el plan 1, reutilizando sus índices y migración.
