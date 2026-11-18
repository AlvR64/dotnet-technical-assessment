# Tasks

## 1. Aplicación y persistencia
- [ ] 1.1 Añadir contratos, puertos y resultados tipados; comprobar independencia de frameworks.
- [ ] 1.2 Implementar caso de uso y probar éxito, inexistencia, edad y conflictos, sin Save en rechazos.
- [ ] 1.3 Implementar adaptadores EF y clasificación de restricciones conocidas; probar errores esperados y propagación segura de desconocidos.

## 2. REST y verificación
- [ ] 2.1 Añadir endpoint, request, handler y presenter; probar 201/400/404/409 y UTC.
- [ ] 2.2 Añadir pruebas funcionales sin host con PostgreSQL aislado para invariantes y ausencia de efectos laterales.
- [ ] 2.3 Ejecutar carreras para unicidad de cliente y vehículo con contextos separados; comprobar un éxito y un conflicto sin sleeps.
- [ ] 2.4 Documentar contrato e identidad externa solo en README.PRUEBA.md; ejecutar build Release y suites.
