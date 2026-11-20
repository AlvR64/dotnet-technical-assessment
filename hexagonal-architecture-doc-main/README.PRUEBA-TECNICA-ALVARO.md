# Prueba técnica .NET — Renting

Microservicio de gestión de alquileres con .NET 10, EF Core y PostgreSQL, construido sobre la plantilla proporcionada.

## Ejecución

Desde esta carpeta, con Docker y Compose instalados:

```sh
docker compose up --build -d
```

Para clonar, arrancar y probar el flujo paso a paso, consulta la [guía de ejecución](README.EJECUCION.md).

- **Swagger:** http://localhost:8080/swagger
- **API:** http://localhost:8080 · **Liveness:** http://localhost:8080/health/live
- Para detener los servicios sin borrar datos: `docker compose stop`.

Compose aplica las migraciones y, si ambas tablas están vacías, crea un vehículo disponible y un alquiler histórico devuelto. Para probar el alquiler desde Swagger:

```json
{"vehicleId":"11111111-1111-4111-8111-111111111111","customerId":"33333333-3333-4333-8333-333333333333"}
```

Los datos de ejemplo no se insertan si ya hay registros ni se renuevan al reiniciar. El vehículo puede dejar de ser elegible con el tiempo. PostgreSQL conserva los datos en un volumen: **no lo elimines si quieres conservarlos**.

## Endpoints

| Método y ruta | Función |
| --- | --- |
| `POST /vehicles` | Registra un vehículo con matrícula, marca, modelo y fecha de fabricación. |
| `GET /vehicles` | Lista los vehículos elegibles sin alquiler activo; también admite `?available=true`. |
| `POST /rentals` | Alquila un vehículo a un cliente identificado por UUID. |
| `PUT /rentals/{rentalId}/return` | Devuelve un alquiler; repetir la petición conserva la primera fecha de devolución. |

Swagger contiene los modelos y respuestas. Se permite una antigüedad de hasta **cinco años de calendario**, evaluada con fecha UTC. Un cliente y un vehículo pueden tener como máximo un alquiler activo: índices únicos parciales de PostgreSQL protegen esta regla incluso ante peticiones concurrentes. El historial se conserva y un vehículo envejecido puede devolverse, pero no volver a alquilarse.

## Arquitectura

Arquitectura hexagonal basada en la plantilla, con separación de responsabilidades inspirada en Clean Architecture, CQRS con MediatR y principios de DDD:

- **API:** controllers, validación HTTP y conversión de resultados a respuestas.
- **ApplicationCore:** commands, queries y handlers que coordinan los casos de uso.
- **Domain:** entidades, invariantes e interfaces de repositorio, sin dependencias de HTTP o EF.
- **Infrastructure:** repositorios EF Core, PostgreSQL y migraciones.

## Pruebas

Con el SDK .NET 10 instalado, desde esta carpeta:

```sh
dotnet build src/microservice.sln -c Release
dotnet test src/microservice.sln -c Release --filter "Category!=Postgres"
```

Las pruebas incluyen ejemplos **unitarios** con dependencias sustituidas, de **infraestructura** que comprueban recepción y validación HTTP sin ejecutar negocio, y **funcionales** con handlers y PostgreSQL reales sin host. También cubren conflictos concurrentes, devoluciones, migraciones y datos de demostración.

Para ejecutar las pruebas PostgreSQL, inicia `docker compose up -d db` y configura `RENTING_TEST_CONNECTION`. Ejemplo en PowerShell:

```powershell
$env:RENTING_TEST_CONNECTION = 'Host=localhost;Port=5432;Database=renting;Username=renting;Password=renting-local'
dotnet test src/microservice.sln -c Release --filter 'Category=Postgres'
```

Estas pruebas crean y eliminan bases temporales de nombre único; el usuario necesita `CREATEDB`. No las ejecutes en un servidor donde eso no sea seguro.

## Consideraciones

- El UUID del cliente es externo y debe ser estable; no existe gestión de clientes.
- El listado es una fotografía sin paginación, no una reserva. La API no tiene autenticación y las credenciales de Compose son solo para desarrollo.
- Compose activa las migraciones y datos de demostración; fuera de Compose ambas opciones están desactivadas por defecto. Si se activa solo `Database__SeedDemoData`, el esquema debe existir previamente.
- Para aplicar migraciones manualmente, configura `ConnectionStrings__Renting`, ejecuta `dotnet tool restore` y después `dotnet dotnet-ef database update --project src/GtMotive.Estimate.Microservice.Infrastructure --startup-project src/GtMotive.Estimate.Microservice.Infrastructure`. En producción deben coordinarse, no ejecutarse simultáneamente desde varias réplicas.
