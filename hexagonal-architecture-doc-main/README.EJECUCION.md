# Guía de ejecución local

Pasos para levantar y probar el microservicio con Docker Compose. Para una descripción de la solución, consulta [README.PRUEBA-TECNICA-ALVARO.md](README.PRUEBA-TECNICA-ALVARO.md).

## 1. Requisitos

- Git y Docker con el plugin Compose. En Windows o macOS, inicia Docker Desktop y espera a que esté operativo.
- Los puertos locales **8080** (API) y **5432** (PostgreSQL) deben estar libres.
- Acceso a Internet para descargar las imágenes y restaurar paquetes la primera vez.

No necesitas instalar .NET ni PostgreSQL en tu equipo para ejecutar la aplicación.

## 2. Clonar y arrancar

En una terminal:

```sh
git clone https://github.com/AlvR64/dotnet-technical-assessment.git
cd dotnet-technical-assessment/hexagonal-architecture-doc-main
docker compose up --build -d
```

La primera construcción puede tardar varios minutos. Comprueba los servicios con:

```sh
docker compose ps
docker compose logs api
```

PostgreSQL debe estar `healthy` y la API en ejecución. La API aplica las migraciones al arrancar. Abre **http://localhost:8080/swagger**; también puedes comprobar http://localhost:8080/health/live.

## 3. Probar un alquiler en Swagger

En una base nueva, Compose crea un vehículo disponible de ejemplo y un alquiler histórico ya devuelto. Si ya hay datos, **no** se insertan ejemplos: usa un vehículo existente o registra uno con `POST /vehicles`. La fecha del vehículo de ejemplo no se renueva al reiniciar y puede dejar de ser elegible.

1. En Swagger, abre **GET /vehicles**, pulsa **Try it out** y **Execute**. Copia el `id` de un vehículo disponible. En una base nueva será `11111111-1111-4111-8111-111111111111`.
2. Abre **POST /rentals**, pulsa **Try it out** e introduce el JSON siguiente. Sustituye `vehicleId` por el identificador copiado si es diferente:

   ```json
   {
     "vehicleId": "11111111-1111-4111-8111-111111111111",
     "customerId": "33333333-3333-4333-8333-333333333333"
   }
   ```

3. Pulsa **Execute**: recibirás `201` con el alquiler. Copia su `id`. Mientras esté activo, el vehículo no aparecerá en el listado y otro intento de alquiler producirá `409`.
4. Abre **PUT /rentals/{rentalId}/return**, pulsa **Try it out**, pega el identificador del alquiler y ejecuta. La respuesta será `204`. Repite **GET /vehicles**: el vehículo aparecerá de nuevo si sigue siendo elegible.

Si utilizas un cliente ya existente con un alquiler activo, emplea otro UUID de cliente o devuelve primero su alquiler. El ejemplo solo funciona con sus identificadores en una base donde se hayan insertado esos datos.

## 4. Detener y resolver problemas

```sh
docker compose logs api
docker compose logs db
docker compose stop
```

Si Swagger no responde, revisa que Docker esté iniciado, los puertos libres y ambos servicios arrancados. `/health/live` indica que el proceso HTTP responde, no que la base de datos esté lista.

`stop` conserva el volumen de PostgreSQL. **No elimines el volumen si necesitas conservar los datos.** Las credenciales de Compose son solo para desarrollo y la API no tiene autenticación.
