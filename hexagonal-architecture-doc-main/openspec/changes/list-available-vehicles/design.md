# Design

## Context
El modelo incluye fecha de fabricación y alquileres históricos con devolución nullable. El listado no reserva vehículos.

## Goals / Non-Goals
**Goals:** consulta eficiente y determinista. **Non-Goals:** listado general, filtros adicionales o paginación.

## Decisions
- Caso de uso y puerto específico de lectura con presenter; EF usa AsNoTracking, proyección y NOT EXISTS ejecutado en PostgreSQL, sin N+1 ni Unit of Work para lectura.
- Capturar fecha UTC con TimeProvider y filtrar `manufactured_on >= today.AddYears(-5)` y `<= today`, consistente con el dominio; ordenar por matrícula e identificador.
- Omitir `available` equivale a true; false y valores inválidos producen 400. Documentar ausencia de paginación.
- La lista es una fotografía, no garantiza disponibilidad posterior. Alternativa de crear una reserva descartada por alcance.

## Risks / Trade-offs
- [Flotas grandes] → Documentar ausencia de paginación como limitación.
- [Carrera con alquiler] → Revalidar y aplicar índices en el caso de alquiler.

## Migration Plan
Implementar tras el plan 1 y el alta; sin cambios de esquema previstos.
