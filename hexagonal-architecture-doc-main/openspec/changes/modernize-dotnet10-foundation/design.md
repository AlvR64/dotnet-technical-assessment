# Design

## Context

La solución separa Host, Api, ApplicationCore, Domain e Infrastructure, y tres proyectos de pruebas. `global.json` fija 9.0.200 sin roll-forward; los proyectos y `Directory.Build.targets` fijan `net9.0`. El host contiene configuración corporativa y el Dockerfile omite Domain en el restore. No hay entidades ni repositorios de negocio. Véanse los requisitos en las especificaciones del cambio.

## Goals / Non-Goals

**Goals:** conservar los límites de la plantilla, asegurar compilación y pruebas con .NET 10, y establecer un entorno de ejecución local reproducible.

**Non-Goals:** diseñar el modelo de alquiler, crear tablas o migraciones vacías, introducir autenticación sustituta o reorganizar la arquitectura completa.

## Decisions

### SDK y dependencias

Usar un SDK .NET 10 estable con roll-forward limitado a versiones compatibles de la misma major; centralizar el target framework en un lugar que MSBuild evalúe correctamente. Alinear los paquetes Microsoft usados con .NET 10 y seleccionar versiones compatibles del resto tras restore y build. Mantener MediatR, xUnit y analizadores salvo incompatibilidad justificada; no actualizar cada paquete solo por tener versión más reciente. Alternativa: conservar .NET 9, descartada por soporte y por requerir un SDK no instalado.

### Integraciones y seguridad

Simplificar el host retirando IdentityServer4, Key Vault y Application Insights y eliminar referencias solo tras comprobar sus consumidores. La API local no implementa autenticación: es una decisión explícita para la prueba, no una recomendación para producción. Conservar contratos de dominio y un adaptador no operativo de telemetría si es útil, evitando eliminar archivos de la plantilla indiscriminadamente. Usar logs en consola y no exponer detalles sensibles de excepciones. Alternativa: emular servicios externos, descartada por complejidad innecesaria.

### Persistencia

Incorporar EF Core 10 y Npgsql en Infrastructure y registrar un DbContext mínimo mediante conexión externa. Fijar `dotnet-ef` localmente con versión compatible. Sin modelo de negocio ni migración inicial. PostgreSQL en Compose ofrece integridad relacional real para la siguiente fase. Alternativas: MongoDB, cuyo código actual no implementa repositorios, y SQLite, más simple pero menos representativa del stack acordado.

### Host, Swagger y salud

Mantener controladores y Swagger UI, adaptando los cambios de API de Swashbuckle/OpenAPI si se actualiza la major. Eliminar la configuración OAuth ligada al proveedor corporativo. `/health/live` comprueba solo la vida del proceso; una prueba separada verifica la conexión real a PostgreSQL. Evitar confiar en cualquier forwarded header salvo configuración explícita para un proxy conocido.

### Contenedores y documentación

Usar compilación multietapa con imágenes .NET 10 y PostgreSQL versionadas, puerto explícito, usuario sin privilegios, healthcheck de PostgreSQL y volumen persistente. No pasar tokens de feed al build. Credenciales de ejemplo solo para desarrollo local y documentadas como no aptas para producción. Conservar íntegros `README.md` y demás documentos originales; usar `README.PRUEBA-TECNICA-ALVARO.md` para instrucciones nuevas.

### Pruebas

Usar TestHost para HTTP y una prueba independiente de conexión a PostgreSQL real. Las pruebas rápidas no deben requerir Docker; documentar cómo arrancar la dependencia y ejecutar la integración por separado. Validar que `dotnet test` descubre pruebas reales, no aceptar éxito con cero pruebas. Estas comprobaciones no reemplazan las tres clases de pruebas de la funcionalidad final.

## Risks / Trade-offs

- [Compatibilidad de bibliotecas y analizadores] → Actualizar selectivamente, compilar y corregir advertencias concretas; no desactivar globalmente los controles.
- [Cambio de API OpenAPI] → Simplificar las extensiones y verificar tanto el documento como la UI.
- [Docker daemon no disponible] → Solicitar que se inicie Docker Desktop antes de las verificaciones con contenedores; declarar verificaciones no ejecutadas si continúa indisponible.
- [Arranque antes de que PostgreSQL esté listo] → Healthcheck y dependencia de Compose; la liveness no significa readiness.
- [Seguridad] → Documentar que la API carece de autenticación y que las credenciales de ejemplo son solo para entorno local.

## Migration Plan

Actualizar SDK, proyectos y paquetes; simplificar el host; preparar persistencia y pruebas; después construir Compose y documentar. Verificar primero restore/build/tests rápidos y después integración con PostgreSQL y contenedores. Al no existir un esquema de negocio ni datos que migrar, no se requiere migración de datos; si la actualización no resulta viable, conservar los cambios identificables para revertirlos de forma selectiva, sin tocar trabajo ajeno.
