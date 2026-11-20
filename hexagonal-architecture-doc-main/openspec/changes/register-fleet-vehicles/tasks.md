# Tasks

## 1. Aplicación y persistencia
- [x] 1.1 Añadir contratos de entrada/salida y puerto específico; comprobar que ApplicationCore no referencia HTTP/EF.
- [x] 1.2 Implementar caso de uso, reloj y resultados; verificar con pruebas unitarias éxito, límite y rechazos sin escritura.
- [x] 1.3 Implementar adaptador y traducción de conflicto por matrícula; probar persistencia, normalización y carrera en PostgreSQL aislado.

## 2. REST y verificación
- [x] 2.1 Añadir request, handler, presenter y controlador; verificar 201/400/409 y Swagger.
- [x] 2.2 Añadir prueba de host con caso de uso sustituido: campo obligatorio ausente produce 400 sin invocar negocio ni base de datos.
- [x] 2.3 Añadir prueba funcional sin host con caso de uso y PostgreSQL reales; comprobar salida y persistencia.
- [x] 2.4 Documentar petición, respuestas y comandos solo en README.PRUEBA-TECNICA-ALVARO.md; verificar build Release y suites.
