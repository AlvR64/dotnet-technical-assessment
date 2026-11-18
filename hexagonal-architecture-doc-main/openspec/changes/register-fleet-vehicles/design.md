# Design

## Context
El plan 1 aporta entidades, migración y restricciones. Se sigue la plantilla hexagonal y los documentos originales no se modifican.

## Goals / Non-Goals
**Goals:** alta verificable y segura ante duplicados. **Non-Goals:** gestión de clientes, modificación de vehículos u otros endpoints.

## Decisions
- Controller → request/handler MediatR → caso de uso → output port → presenter, con puerto de persistencia específico y `IUnitOfWork` scoped compartido. Evitar repositorio genérico y dependencias HTTP/EF en ApplicationCore.
- TimeProvider.System inyectable; capturar fecha UTC una sola vez y usar la política del dominio. DTO valida formato y obligatoriedad; el dominio conserva invariantes.
- Traducir únicamente la restricción única conocida de matrícula a conflicto tipado; las preconsultas no garantizan exclusividad. No exponer SQL ni transformar errores desconocidos en 409.
- Devolver 201 con datos, sin Location que apunte a un endpoint GET inexistente. Alternativa de crear un GET adicional descartada por alcance.

## Risks / Trade-offs
- [Carreras] → Índice único PostgreSQL y prueba concurrente.
- [Reloj] → Pruebas con fecha fija y fecha de referencia capturada una vez.

## Migration Plan
Implementar tras el plan 1; no se requiere migración adicional salvo incompatibilidad identificada y acordada.
