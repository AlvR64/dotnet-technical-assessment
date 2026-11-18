# Fleet rental model

## Purpose

Garantizar que los vehículos y alquileres conservan invariantes coherentes y datos íntegros antes de exponer los casos de uso.

## ADDED Requirements

### Requirement: Datos de vehículo válidos
El modelo SHALL exigir identificador no vacío, matrícula, marca, modelo y fecha de fabricación válidos. La matrícula se normaliza eliminando espacios exteriores y pasando a mayúsculas; su longitud máxima es 20, y la de marca y modelo es 100.

#### Scenario: Datos incompletos o demasiado largos
- **WHEN** se intenta crear un vehículo con identificador vacío, campos en blanco o longitudes superiores a las permitidas
- **THEN** se rechaza su creación

#### Scenario: Matrícula normalizada
- **WHEN** se crea un vehículo con matrícula ` ab-123 `
- **THEN** se conserva como `AB-123`

### Requirement: Límite de antigüedad
El modelo SHALL rechazar altas con fecha futura o antigüedad superior a cinco años respecto de una fecha de referencia explícita, usando años de calendario.

#### Scenario: Límite exacto
- **WHEN** la fabricación es exactamente cinco años anterior a la fecha de referencia
- **THEN** se admite el vehículo

#### Scenario: Fuera del límite o fecha futura
- **WHEN** la fabricación es más de cinco años anterior o posterior a la fecha de referencia
- **THEN** se rechaza el vehículo

#### Scenario: Año bisiesto
- **WHEN** se evalúa la elegibilidad de un vehículo fabricado el 29 de febrero frente a una fecha de referencia en un año no bisiesto
- **THEN** se aplica aritmética de calendario coherente y comprobable, no una duración fija de días

### Requirement: Elegibilidad reevaluada
El modelo SHALL permitir evaluar la elegibilidad por antigüedad con una fecha de referencia sin eliminar datos históricos ni impedir devolver alquileres existentes.

#### Scenario: Vehículo envejecido durante el alquiler
- **WHEN** un vehículo supera la antigüedad máxima después del inicio de un alquiler
- **THEN** deja de ser elegible para nuevos alquileres pero su alquiler existente puede devolverse

### Requirement: Validez e idempotencia de la devolución
El modelo SHALL exigir identificadores no vacíos y fechas UTC, no aceptar devoluciones anteriores al inicio y conservar la primera fecha en devoluciones repetidas.

#### Scenario: Devolución repetida
- **WHEN** se devuelve un alquiler ya devuelto
- **THEN** se conserva la fecha de devolución original

#### Scenario: Fechas incoherentes
- **WHEN** se intenta devolver antes del inicio o utilizar un instante no UTC
- **THEN** se rechaza la operación

### Requirement: Matrículas únicas
La persistencia SHALL impedir dos vehículos con la misma matrícula normalizada.

#### Scenario: Alta duplicada
- **WHEN** se intenta persistir una matrícula equivalente a otra existente tras su normalización
- **THEN** la base de datos rechaza la segunda escritura

### Requirement: Un alquiler activo por cliente y vehículo
La persistencia SHALL impedir más de un alquiler activo por cliente y más de uno por vehículo, incluso ante escrituras concurrentes.

#### Scenario: Escrituras concurrentes incompatibles
- **WHEN** dos contextos intentan confirmar alquileres activos incompatibles para el mismo cliente o vehículo
- **THEN** como máximo uno se confirma y los datos permanecen coherentes

#### Scenario: Historial devuelto
- **WHEN** un alquiler ya tiene fecha de devolución
- **THEN** no bloquea un alquiler activo posterior del mismo cliente o vehículo

### Requirement: Integridad histórica
La persistencia SHALL requerir un vehículo existente para cada alquiler, impedir devolución anterior al inicio y restringir el borrado del vehículo con historial.

#### Scenario: Alquiler sin vehículo existente
- **WHEN** se intenta persistir un alquiler que referencia un vehículo inexistente
- **THEN** la base de datos rechaza la escritura

#### Scenario: Vehículo con historial
- **WHEN** se intenta borrar un vehículo que tiene alquileres registrados
- **THEN** la base de datos rechaza el borrado y conserva el historial
