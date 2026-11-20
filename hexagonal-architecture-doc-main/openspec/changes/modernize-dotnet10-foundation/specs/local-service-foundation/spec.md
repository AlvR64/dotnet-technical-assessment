# Local service foundation

## Purpose

Permitir que cualquier evaluador ejecute y compruebe el servicio localmente sin depender de servicios corporativos externos.

## ADDED Requirements

### Requirement: Ejecución autónoma de la API
El servicio SHALL arrancar en local sin credenciales ni acceso a servicios externos de identidad, secretos o telemetría corporativa.

#### Scenario: Arranque en Development
- **WHEN** se inicia el servicio en Development con la configuración local documentada
- **THEN** el proceso arranca sin requerir Azure ni IdentityServer

#### Scenario: Arranque en Production
- **WHEN** se inicia el servicio en Production con la configuración local documentada
- **THEN** el proceso arranca sin requerir Azure ni IdentityServer

### Requirement: Indicador de actividad HTTP
El servicio SHALL exponer `/health/live` como comprobación de que el proceso HTTP responde, sin implicar disponibilidad de base de datos.

#### Scenario: Comprobar liveness
- **WHEN** se consulta `/health/live` mientras el proceso está activo
- **THEN** responde con un código HTTP de éxito

### Requirement: Documentación HTTP accesible
El servicio SHALL ofrecer su documento OpenAPI y Swagger UI sin servidor de identidad externo.

#### Scenario: Consultar Swagger
- **WHEN** se accede a la interfaz Swagger y a su documento OpenAPI con el servicio activo
- **THEN** ambos se sirven correctamente sin autenticación externa

### Requirement: Ejecución con contenedores
La solución SHALL permitir iniciar la API y PostgreSQL con Docker Compose, sin instalar el SDK ni el servidor de base de datos en el host.

#### Scenario: Primera ejecución local
- **WHEN** el evaluador ejecuta el comando documentado de construcción y arranque con Docker operativo y acceso a imágenes y paquetes
- **THEN** la API y PostgreSQL arrancan y la API responde a la comprobación de actividad

#### Scenario: Persistencia del volumen
- **WHEN** se reinician los servicios sin borrar el volumen de PostgreSQL
- **THEN** los datos que se hayan escrito previamente en ese volumen permanecen

### Requirement: Configuración y conexión de base de datos
La aplicación SHALL aceptar la conexión a PostgreSQL mediante configuración externa y permitir verificar conectividad sin publicar contraseñas en logs.

#### Scenario: Conexión disponible
- **WHEN** se suministra una configuración válida y PostgreSQL está disponible
- **THEN** la comprobación de conectividad puede abrir una conexión

#### Scenario: Configuración no válida
- **WHEN** falta o es incorrecta la configuración necesaria para conectarse
- **THEN** la comprobación falla de forma comprensible sin exponer la contraseña

### Requirement: Documentación adicional sin alterar originales
Las instrucciones de ejecución y comprobación SHALL estar en `README.PRUEBA-TECNICA-ALVARO.md` sin modificar los documentos originales de la plantilla.

#### Scenario: Consulta del evaluador
- **WHEN** el evaluador consulta la documentación de la prueba
- **THEN** encuentra requisitos previos, comandos, configuración, puertos y ejecución de pruebas en el nuevo documento
