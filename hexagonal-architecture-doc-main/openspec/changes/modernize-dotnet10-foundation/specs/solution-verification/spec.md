# Solution verification

## Purpose

Permitir verificar que la base de la solución compila y que su infraestructura de pruebas funciona de forma reproducible.

## ADDED Requirements

### Requirement: Compilación reproducible
La solución SHALL restaurar sus dependencias públicas y compilar en Release con el SDK .NET 10 configurado.

#### Scenario: Construcción completa
- **WHEN** se ejecutan los comandos documentados de restore y build Release con acceso a NuGet público
- **THEN** todos los proyectos de la solución se restauran y compilan sin requerir credenciales de un feed privado

### Requirement: Ejecución verificable de pruebas
La solución SHALL permitir descubrir y ejecutar pruebas automatizadas reales para comprobar su configuración técnica.

#### Scenario: Ejecución del conjunto de pruebas
- **WHEN** se ejecuta el comando de pruebas documentado
- **THEN** se descubren pruebas y finalizan correctamente, incluyendo una comprobación HTTP del endpoint de actividad

### Requirement: Pruebas de conectividad
La solución SHALL ofrecer una comprobación automatizada de conexión con un PostgreSQL real, sin requerir una instalación local del servidor.

#### Scenario: Base de datos de prueba disponible
- **WHEN** la instancia PostgreSQL de prueba documentada está disponible y se ejecuta la comprobación de integración
- **THEN** se establece una conexión real con esa instancia

#### Scenario: Ejecución sin Docker
- **WHEN** se ejecutan las pruebas rápidas documentadas sin PostgreSQL disponible
- **THEN** las pruebas que no requieren base de datos pueden ejecutarse por separado y el README explica cómo ejecutar las de integración
