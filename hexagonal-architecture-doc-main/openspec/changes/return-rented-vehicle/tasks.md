# Tasks

## 1. Aplicación y persistencia
- [ ] 1.1 Definir entrada, puertos y resultados; verificar que la orquestación no depende de HTTP/EF.
- [ ] 1.2 Implementar caso de uso y pruebas unitarias con reloj fijo: devolución, repetición, inexistencia y vehículo envejecido.
- [ ] 1.3 Implementar actualización condicional y probar dos devoluciones concurrentes con contextos separados, sin sobrescribir la primera fecha.

## 2. REST y verificación
- [ ] 2.1 Añadir endpoint, handler y presenter; probar 204 sin cuerpo, 400 para identificador inválido y 404 para alquiler inexistente.
- [ ] 2.2 Probar sin host el flujo de alta, alquiler, rechazo, devolución y nuevo alquiler, conservando el historial.
- [ ] 2.3 Probar devolución de vehículo envejecido y su exclusión del listado usando la misma política temporal.
- [ ] 2.4 Documentar idempotencia y flujo en README.PRUEBA.md; ejecutar build, pruebas y demostración REST con Compose.
- [ ] 2.5 Contrastar las cuatro operaciones y restricciones con el PDF, incluidos ejemplos unitario, host de validación y funcional sin host; preparar explicación de decisiones y limitaciones.
