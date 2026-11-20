# Tasks

## 1. Modelo de dominio

- [x] 1.1 Crear `Vehicle` encapsulado con identidad, matrícula normalizada y datos obligatorios; verificar con pruebas unitarias valores vacíos, longitudes y normalización.
- [x] 1.2 Implementar elegibilidad con fecha de referencia explícita y cinco años de calendario; verificar fechas futuras, límite exacto, un día antes/después y 29 de febrero.
- [x] 1.3 Crear `Rental` con identificadores válidos, instantes UTC y devolución idempotente; verificar por pruebas unitarias fechas inválidas, repetición y vehículo envejecido.

## 2. Modelo relacional y pruebas

- [x] 2.1 Añadir `DbSet` y mapeos EF para ambas entidades con columnas, claves, FK restrictiva y check de fechas; verificar guardado y recuperación contra PostgreSQL real.
- [x] 2.2 Configurar unicidad de matrícula normalizada y los dos índices únicos parciales de alquiler activo; verificar duplicados, historial devuelto y concurrencia con contextos separados.
- [x] 2.3 Implementar y registrar `IUnitOfWork` scoped compartiendo el contexto; comprobar mediante una prueba de integración que `Save()` persiste las entidades.
- [x] 2.4 Preparar aislamiento de pruebas usando una base temporal de nombre único y limpieza limitada a esa base; verificar ejecuciones repetidas sin alterar la base de desarrollo.

## 3. Migraciones y ejecución local

- [x] 3.1 Crear el mecanismo de contexto de diseño y migración inicial EF; revisar el SQL generado y verificar creación del esquema en una base temporal vacía sin `EnsureCreated`.
- [x] 3.2 Habilitar migraciones en el arranque solo mediante opción explícita local de Compose; probar opción desactivada, activada y error de migración que impide arrancar.
- [x] 3.3 Verificar arranque con base vacía y reinicio conservando datos en una base y volumen aislados de pruebas, sin eliminar volúmenes existentes.
- [x] 3.4 Actualizar solo `README.PRUEBA-TECNICA-ALVARO.md` con esquema, supuestos, comandos de migración y pruebas; ejecutar los comandos y confirmar que los documentos originales permanecen intactos.

## 4. Validación integrada

- [x] 4.1 Ejecutar build Release y pruebas unitarias; registrar resultados y comprobar que Domain no depende de EF ni de HTTP.
- [x] 4.2 Ejecutar pruebas PostgreSQL reales y concurrencia, comprobar migraciones repetidas y documentar resultados.
- [x] 4.3 Revisar alcance y confirmar que no se añadieron endpoints, gestión de clientes ni lógica de los casos de uso posteriores.
