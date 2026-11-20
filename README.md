# Prueba técnica .NET — Renting

Microservicio para registrar vehículos, consultar disponibilidad, alquilar y devolver.

**Tecnologías:** .NET 10, EF Core, PostgreSQL, MediatR y Docker Compose. Arquitectura hexagonal con CQRS.

## Ejecución

Con Docker iniciado, desde la raíz del repositorio:

```sh
cd hexagonal-architecture-doc-main
docker compose up --build -d
```

**Swagger:** http://localhost:8080/swagger

Se aplican las migraciones y se cargan datos de ejemplo si la base está vacía.

- [Guía paso a paso](hexagonal-architecture-doc-main/README.EJECUCION.md)
- [Resumen técnico y pruebas](hexagonal-architecture-doc-main/README.PRUEBA-TECNICA-ALVARO.md)
