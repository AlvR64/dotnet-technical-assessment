# Design

Los controllers convierten DTO HTTP en commands o queries de MediatR; sus handlers residen en ApplicationCore y devuelven resultados tipados que API traduce a HTTP. La lógica de negocio reside en esos handlers, sin una segunda clase delegada ni presenters. La persistencia sigue detrás de puertos, con índices parciales y devolución condicional. Esta decisión sustituye las referencias a handler y presenter en API de los diseños de registro, consulta, alquiler y devolución, sin modificar sus requisitos funcionales.

ApplicationCore referencia MediatR, pero no HTTP ni EF; Domain no referencia MediatR. Se conservan los documentos originales de la plantilla y se explica la divergencia en README.PRUEBA-TECNICA-ALVARO.md.
