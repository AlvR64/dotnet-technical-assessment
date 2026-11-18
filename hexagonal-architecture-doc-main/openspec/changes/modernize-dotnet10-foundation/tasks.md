# Tasks

## 1. SDK y dependencias

- [x] 1.1 Configurar un SDK 10 con roll-forward compatible y centralizar el target `net10.0`; verificar `dotnet --version` y los frameworks efectivos de los ocho proyectos.
- [x] 1.2 Actualizar selectivamente dependencias Microsoft y paquetes usados, unificar Moq y revisar analizadores; verificar restore público y diagnosticar vulnerabilidades o incompatibilidades sin desactivar reglas globalmente.
- [x] 1.3 Retirar referencias y declaraciones sin uso tras comprobar sus consumidores; verificar que restore y build de la solución siguen funcionando.

## 2. Host, Swagger y comprobación HTTP

- [x] 2.1 Retirar integraciones corporativas de identidad, secretos y telemetría del host y sus dependencias no utilizadas, conservando los contratos necesarios; verificar arranque en Development y Production sin esos servicios.
- [x] 2.2 Simplificar configuración, logs y forwarded headers sin exponer datos sensibles; verificar el arranque con configuración local y revisar salida de logs.
- [x] 2.3 Adaptar Swagger a versiones compatibles y eliminar OAuth corporativo; verificar documento OpenAPI y UI por HTTP.
- [x] 2.4 Incorporar `/health/live` independiente de PostgreSQL y una prueba HTTP automatizada; ejecutarla con el host de pruebas.
- [x] 2.5 Crear `README.PRUEBA.md` con requisitos y comandos de API, Swagger y pruebas rápidas, indicando ausencia de autenticación; verificar que los documentos originales no cambian.

## 3. Persistencia PostgreSQL

- [x] 3.1 Añadir EF Core 10, Npgsql y herramientas de diseño compatibles en Infrastructure y fijar `dotnet-ef` localmente; verificar restore y resolución de la herramienta.
- [x] 3.2 Registrar un DbContext mínimo y configuración externa de conexión sin crear entidades ni migración vacía; comprobar resolución del contexto y errores sin contraseñas en logs.
- [x] 3.3 Añadir prueba de conectividad contra PostgreSQL real y permitir separarla de las pruebas rápidas; verificar tanto la conexión exitosa como la ejecución aislada sin base de datos.
- [x] 3.4 Documentar en `README.PRUEBA.md` configuración y ejecución de la prueba de conectividad, sin modificar documentos originales.

## 4. Docker y pruebas

- [x] 4.1 Actualizar Dockerfile a .NET 10, incluir Domain en restore, eliminar feed privado y configurar usuario/puerto; verificar construcción de la imagen con Docker operativo.
- [x] 4.2 Añadir `.dockerignore` y Compose con PostgreSQL versionado, volumen y healthcheck; verificar `docker compose up --build`, liveness y conectividad desde API.
- [x] 4.3 Actualizar TestHost y fixtures, quitar exclusión de Domain y dependencias de autenticación de prueba innecesarias; verificar descubrimiento y ejecución de pruebas reales.
- [x] 4.4 Documentar Compose, puertos, credenciales locales, pruebas y conservación del volumen en `README.PRUEBA.md`; ejecutar los comandos documentados y confirmar documentos originales intactos.

## 5. Verificación integrada

- [x] 5.1 Ejecutar restore, build Release y pruebas rápidas de la solución; registrar resultados y cualquier bloqueo explícitamente.
- [x] 5.2 Ejecutar prueba de PostgreSQL real, verificar Swagger y liveness, y comprobar persistencia del volumen tras reiniciar servicios sin eliminarlo; registrar resultados.
- [x] 5.3 Revisar dependencias, diferencias y alcance para confirmar que no hay lógica de renting ni cambios a documentos originales.
