# Vehicle rental

## Purpose
Permitir alquileres inmediatos garantizando la exclusividad del cliente y del vehículo ante concurrencia.

## ADDED Requirements

### Requirement: Alquiler REST
El servicio SHALL aceptar `POST /rentals` con UUID no vacíos `vehicleId` y `customerId`, generar identidad e instante UTC y devolver 201 con `id`, `vehicleId`, `customerId`, `rentedAt` y `returnedAt: null`.

#### Scenario: Alquiler válido
- **WHEN** vehículo y cliente cumplen las condiciones
- **THEN** persiste un alquiler activo y responde 201

#### Scenario: Datos inválidos
- **WHEN** los identificadores faltan, están vacíos o son inválidos
- **THEN** responde 400 sin crear alquiler

#### Scenario: Vehículo inexistente
- **WHEN** el vehículo indicado no existe
- **THEN** responde 404 sin crear alquiler

### Requirement: Elegibilidad y exclusividad
El servicio SHALL rechazar con 409 vehículos que exceden el límite, vehículos ya alquilados y clientes con otro alquiler activo.

#### Scenario: Disponibilidad desactualizada
- **WHEN** un vehículo envejece o pasa a estar alquilado tras consultarse la disponibilidad
- **THEN** el intento de alquiler se rechaza sin modificar los alquileres existentes

#### Scenario: Cliente ocupado
- **WHEN** un cliente con alquiler activo solicita otro vehículo
- **THEN** responde 409 y conserva el alquiler anterior

### Requirement: Integridad concurrente
El servicio SHALL mantener como máximo un alquiler activo por cliente y por vehículo ante escrituras simultáneas.

#### Scenario: Competencia por vehículo
- **WHEN** dos clientes alquilan simultáneamente el mismo vehículo
- **THEN** solo una operación tiene éxito y la otra recibe 409

#### Scenario: Competencia por cliente
- **WHEN** el mismo cliente alquila simultáneamente dos vehículos
- **THEN** solo una operación tiene éxito y la otra recibe 409

### Requirement: Errores de alquiler seguros
El servicio SHALL usar Problem Details con códigos `vehicle_not_found`, `vehicle_not_eligible`, `vehicle_already_rented` y `customer_has_active_rental`, sin exponer SQL ni detalles internos.

#### Scenario: Restricción de unicidad
- **WHEN** la base de datos rechaza una carrera por uno de los índices de alquiler activo
- **THEN** responde el conflicto correspondiente sin crear un registro parcial
