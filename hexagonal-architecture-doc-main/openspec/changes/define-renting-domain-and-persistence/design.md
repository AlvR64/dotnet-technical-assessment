# Design

## Context

La solución tiene un `RentingDbContext` vacío, Npgsql y PostgreSQL 17 en Compose. El contrato `IUnitOfWork.Save()` está en Domain. El host configura el contexto con una cadena externa; aún no hay migraciones ni entidades de negocio. El documento original de la plantilla debe permanecer intacto.

## Goals / Non-Goals

**Goals:** conservar entidades independientes de EF, garantizar integridad concurrente en PostgreSQL y permitir instalar el esquema local sin intervención manual.

**Non-Goals:** endpoints, casos de uso, repositorios anticipados, registro de clientes, eliminación automática de vehículos, mensajería y datos semilla.

## Decisions

### Modelo y columnas

Dos entidades con identidad UUID generada por la aplicación: `Vehicle` con `id`, `license_plate` varchar(20), `brand` y `model` varchar(100), y `manufactured_on` date; `Rental` con `id`, `vehicle_id`, `customer_id`, `rented_at` y `returned_at` opcional, ambos instantes timestamptz UTC. Sin entidad Customer: se asume un identificador externo consistente. La matrícula se recorta y normaliza a mayúsculas invariantes antes de persistirla; no se inventa un formato nacional.

EF configura las entidades de dominio con clases de configuración externas, constructores/acceso adecuados y propiedades encapsuladas. Alternativa: entidades de persistencia duplicadas; descartada inicialmente para evitar mapeos adicionales sin necesidad.

### Reglas de tiempo

La fecha de referencia se pasa explícitamente a la política de antigüedad: fecha de fabricación no futura y >= fecha de referencia menos cinco años de calendario, con `DateOnly.AddYears(-5)` y pruebas en torno al 29 de febrero. Al envejecer, el vehículo sigue almacenado pero no es elegible para nuevos alquileres; se permite devolver uno ya activo. El filtrado en listados y alquileres pertenece a cambios posteriores. Alternativa: estado persistido actualizado con tareas programadas, descartada por complejidad.

Los alquileres aceptan instantes UTC y rechazan devolución anterior al inicio; la devolución repetida conserva el primer instante.

### Restricciones y concurrencia

Clave única en la matrícula normalizada, FK vehículo con borrado restringido, check `returned_at IS NULL OR returned_at >= rented_at`, e índices únicos parciales en `vehicle_id` y `customer_id` donde `returned_at IS NULL`. La restricción de antigüedad no se expresa como check basado en el reloj, pues el paso del tiempo no reevaluaría filas. La aplicación podrá hacer comprobaciones previas para mensajes claros, pero los índices serán la última garantía ante carreras. Alternativa: leer antes de insertar sin restricción; descartada por condiciones de carrera.

### Unidad de trabajo y migración

Registrar una implementación scoped del contrato existente `IUnitOfWork` que comparta el mismo `RentingDbContext` y delegue en `SaveChangesAsync`. Añadir migración inicial y mecanismo de diseño para herramientas EF; no usar `EnsureCreated`. En Compose habilitar explícitamente la migración en el arranque, desactivada por defecto en otros entornos; el fallo impide que se inicie el host. Para producción se recomiendan migraciones separadas y coordinadas, no aplicar simultáneamente desde múltiples réplicas.

### Pruebas aisladas

Cubrir dominio con reloj/referencia determinista y usar PostgreSQL real para restricciones, carrera entre contextos y migraciones. Crear una base de pruebas temporal con nombre único, o alternativa equivalente aislada, y limpiar únicamente esa base; nunca borrar el volumen ni las bases de desarrollo. Proporcionar comandos claros para lanzar las pruebas que requieran Docker.

## Risks / Trade-offs

- [Identidad externa] → Documentar que el proveedor debe mantener el mismo UUID para la persona; sin ello no se garantiza unicidad de negocio.
- [Años bisiestos y fecha de corte] → Especificar y probar explícitamente `AddYears(-5)` en los límites.
- [Índices con normalización] → Normalizar antes de persistir y probar duplicados de mayúsculas/espacios; escrituras directas que no respeten esa normalización quedan fuera del contrato de la aplicación.
- [Migraciones en arranque] → Solo opt-in local; fallar de forma visible y recomendar despliegue coordinado para producción.
- [Pruebas destructivas] → Trabajar únicamente sobre bases temporales propias y no eliminar volúmenes ni datos ajenos.

## Migration Plan

Crear entidades y pruebas aisladas; añadir mapeos y migración; probar el SQL y sus restricciones en una base temporal; habilitar la migración local y validar un arranque limpio y reinicio con datos. La migración inicial crea tablas nuevas; cualquier reversión posterior debe preservar datos existentes y documentarse, sin borrar volúmenes automáticamente.
