# Proposal

## Why

La plantilla fija un SDK .NET 9 no instalado y su host y Dockerfile dependen de configuración corporativa ajena a la prueba. Necesitamos una base compatible con .NET 10 que cualquier evaluador pueda ejecutar y comprobar localmente sin servicios externos.

## What Changes

- Actualizar los proyectos, SDK y dependencias necesarias a versiones compatibles con .NET 10, manteniendo la arquitectura y patrones de la plantilla.
- **BREAKING** Retirar del host las integraciones con IdentityServer, Azure Key Vault y Application Insights; la aplicación de esta prueba no implementará autenticación.
- Preparar EF Core y PostgreSQL sin introducir aún entidades, esquema ni migraciones de negocio.
- Habilitar el arranque y la documentación HTTP locales, un endpoint de liveness, Docker Compose y comprobaciones técnicas mínimas.
- Documentar las instrucciones nuevas en `README.PRUEBA-TECNICA-ALVARO.md`, sin modificar los documentos originales de la plantilla.
- Quedan fuera vehículos, alquileres, reglas de negocio y las pruebas funcionales específicas exigidas para la solución final.

## Capabilities

### New Capabilities

- `local-service-foundation`: ejecución local de la API, documentación HTTP, salud y conectividad de PostgreSQL.
- `solution-verification`: compilación y ejecución verificable de las pruebas técnicas de la solución.

### Modified Capabilities

Ninguna: aún no existen especificaciones de capacidades.

## Impact

La solución, proyectos y configuración de compilación; el host y Swagger; referencias NuGet y adaptadores de infraestructura; configuración de pruebas; Dockerfile y Compose. Se introduce una dependencia local PostgreSQL en contenedor y documentación separada, conservando los documentos originales intactos.
