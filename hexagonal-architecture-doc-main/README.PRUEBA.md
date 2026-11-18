# Preparación de la prueba técnica

Este documento complementa el README original de la plantilla. La base usa .NET 10, EF Core, PostgreSQL y Docker Compose. Todavía no implementa las operaciones de renting.

## Requisitos

- Para ejecutar todo en contenedores: Docker con Compose y acceso a las imágenes y a NuGet en la primera construcción.
- Para compilar o ejecutar pruebas en el equipo: SDK .NET 10 compatible con `global.json`.

Desde esta carpeta:

```sh
docker compose up --build -d
```

- API: http://localhost:8080
- Swagger: http://localhost:8080/swagger
- Liveness: http://localhost:8080/health/live
- PostgreSQL: `localhost:5432`, base y usuario `renting`, contraseña `renting-local`.

Las credenciales de Compose son **solo para desarrollo local**. La API no tiene autenticación: no está preparada para desplegarse en producción. El endpoint liveness solo comprueba que el proceso HTTP responde, no la conexión a la base de datos.

Para parar los contenedores sin borrar los datos:

```sh
docker compose stop
```

Los datos de PostgreSQL se conservan en un volumen de Docker; no se deben eliminar los volúmenes si se quieren conservar.

## Compilación y pruebas rápidas

```sh
dotnet restore src/microservice.sln
dotnet build src/microservice.sln -c Release
dotnet test src/microservice.sln -c Release --filter "Category!=Postgres"
dotnet tool restore
dotnet dotnet-ef --version
```

Las pruebas cubren el modelo y la persistencia, pero aún no existen endpoints de negocio ni los ejemplos de casos de uso exigidos por el PDF.

## Prueba de conexión a PostgreSQL

Con el servicio `db` de Compose en marcha, en PowerShell:

```powershell
$env:RENTING_TEST_CONNECTION = 'Host=localhost;Port=5432;Database=renting;Username=renting;Password=renting-local'
dotnet test test/functional/GtMotive.Estimate.Microservice.FunctionalTests -c Release --filter 'Category=Postgres'
dotnet test test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests -c Release --filter 'Category=Postgres'
```

En bash:

```sh
export RENTING_TEST_CONNECTION='Host=localhost;Port=5432;Database=renting;Username=renting;Password=renting-local'
dotnet test test/functional/GtMotive.Estimate.Microservice.FunctionalTests -c Release --filter 'Category=Postgres'
dotnet test test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests -c Release --filter 'Category=Postgres'
```

Las pruebas funcionales crean y eliminan una base temporal de nombre único en PostgreSQL; el usuario configurado necesita permiso `CREATEDB`. No modifican la base `renting` ni eliminan volúmenes. La aplicación recibe `ConnectionStrings__Renting` desde Compose; al ejecutar el host fuera de Compose puede suministrarse la misma variable con `Host=localhost`.

## Decisiones de la adaptación

Se conserva la separación en capas de la plantilla. Se han retirado del arranque las integraciones con Azure e IdentityServer porque no son necesarias para esta prueba autónoma.

## Modelo y migraciones

Un vehículo tiene identidad, matrícula normalizada, marca, modelo y fecha de fabricación; un alquiler identifica vehículo, cliente externo y fechas UTC. Una devolución nula indica alquiler activo. No existe tabla de clientes: el sistema externo debe proporcionar siempre el mismo UUID para una misma persona.

El modelo acepta hasta cinco años de antigüedad en la fecha de referencia, usando años de calendario. Al superar ese límite, el vehículo deja de ser elegible, pero el historial se conserva y se puede devolver un alquiler ya activo. Los futuros casos de uso aplicarán la elegibilidad en sus consultas y operaciones.

PostgreSQL garantiza una matrícula única, una referencia válida al vehículo y, mediante índices únicos parciales, como máximo un alquiler activo por vehículo y por cliente. El índice protege frente a solicitudes concurrentes; la fecha de antigüedad se comprueba en el dominio, no con un check SQL que dependa del reloj.

Compose activa `Database__ApplyMigrations=true`: la API aplica las migraciones antes de atender peticiones. Fuera de Compose está desactivado por defecto. Para aplicar explícitamente desde el equipo:

```powershell
$env:ConnectionStrings__Renting = 'Host=localhost;Port=5432;Database=renting;Username=renting;Password=renting-local'
dotnet dotnet-ef database update --project src/GtMotive.Estimate.Microservice.Infrastructure --startup-project src/GtMotive.Estimate.Microservice.Infrastructure
```

En producción, ejecutar las migraciones de manera controlada y coordinada, no simultáneamente desde varias réplicas. Los documentos originales de la plantilla no se han modificado.
