# Vehicle registration

## Purpose
Permitir registrar vehículos válidos y mantener la unicidad de su matrícula en la flota.

## ADDED Requirements

### Requirement: Alta REST de vehículos
El servicio SHALL admitir `POST /vehicles` con `licensePlate`, `brand`, `model` y `manufacturedOn`, y generar el identificador del vehículo.

#### Scenario: Alta válida
- **WHEN** se envían datos válidos
- **THEN** persiste el vehículo y devuelve 201 con `id` y los datos, incluyendo matrícula normalizada

#### Scenario: Modelo inválido
- **WHEN** faltan campos obligatorios, están vacíos, superan sus longitudes o el JSON o la fecha son inválidos
- **THEN** devuelve 400 sin persistir

### Requirement: Antigüedad al registrar
El servicio SHALL validar la fecha contra una referencia UTC capturada una vez usando la política de cinco años del modelo.

#### Scenario: Fecha en el límite
- **WHEN** la fabricación coincide con la fecha actual menos cinco años de calendario
- **THEN** admite el alta

#### Scenario: Fecha futura o antigua
- **WHEN** la fabricación es futura o anterior al límite
- **THEN** responde 400 sin escribir

### Requirement: Unicidad de matrícula
El servicio SHALL impedir matrículas equivalentes después de normalizarlas, incluso ante concurrencia.

#### Scenario: Matrícula duplicada
- **WHEN** ya existe `AB-123` y se solicita ` ab-123 `
- **THEN** responde 409 y no crea otro vehículo

#### Scenario: Altas simultáneas
- **WHEN** dos solicitudes intentan crear la misma matrícula
- **THEN** como máximo una tiene éxito y no quedan duplicados

### Requirement: Errores seguros
El servicio SHALL responder a los errores con Problem Details y un código estable, sin exponer SQL ni datos internos.

#### Scenario: Conflicto de matrícula
- **WHEN** la restricción única de matrícula rechaza una escritura
- **THEN** la respuesta identifica el conflicto sin exponer la excepción de la base de datos
