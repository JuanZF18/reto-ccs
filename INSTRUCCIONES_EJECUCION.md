🚀 Guía de Ejecución Rápida – CCS Tracking System

Esta guía describe cómo desplegar, verificar y realizar pruebas de carga sobre la solución de rastreo vehicular CCS Tracking System.

## 📋 1. Requisitos Previos

Solo necesitas tener instalado:

- Docker Desktop (debe estar corriendo).
- Git (para clonar el repositorio).
- PowerShell o Terminal Bash (opcional).

Nota: No se requiere instalar .NET 8 ni bases de datos.
Toda la arquitectura corre dentro de contenedores Docker.

## 🛠️ 2. Despliegue de la Solución

Abre una terminal en la carpeta raíz del proyecto.

Ejecuta:

docker-compose up --build -d

Espera ~30 segundos para que todos los servicios inicien correctamente.

Verifica que los 5 contenedores estén activos:

docker ps

Debes ver estos contenedores:

ccs_ingestion
ccs_rules
ccs_notifications
ccs_rabbitmq
ccs_postgres

## ✅ 3. Verificación Funcional

Abre tu navegador y accede a:

📚 Documentación API (Swagger)
👉 http://localhost:8080/swagger

Aquí puedes probar los endpoints manualmente.

📨 Panel de Mensajería (RabbitMQ)
👉 http://localhost:15672

Credenciales:
Usuario: guest
Contraseña: guest

## ⚡ 4. Prueba de Carga (Stress Test)

El proyecto incluye un script automatizado con k6 para demostrar soporte de +500 peticiones por segundo.

## Solo debemos ejecutar un solo paso. En la carpeta .docker\postgres-init se encuentra el archivo sql encargado de inicializar la BD con datos de prueba, por lo que solo será necesario ejecutar el test en javascript.

## Paso 1: Ejecutar el Ataque

Ubícate en la carpeta donde está el archivo stress-test.js.

Windows (PowerShell):

Get-Content stress-test.js | docker run --rm -i --add-host=host.docker.internal:host-gateway grafana/k6 run -

Mac / Linux:

docker run --rm -i --add-host=host.docker.internal:host-gateway grafana/k6 run - < stress-test.js

## 🎯 Resultados Esperados

http_reqs: ~600/s o muchas más
checks: 100% (todas las peticiones exitosas)

## 🧹 5. Detener y Limpiar

docker-compose down

## 🆘 Solución de Problemas Comunes

⚠️ "Ports are not available"  
Otro servicio está usando los puertos 8080 o 5432.  
Ciérralo o cambia los puertos en docker-compose.yml.

⚠️ "Connection refused" en los logs  
RabbitMQ puede tardar en iniciar.  
Soluciones:  
- Espera 1 minuto.  
- O reinicia el contenedor afectado:

docker restart <nombre_contenedor>


## 🧪 5. Validación de Reglas de Negocio (Paso a Paso)

El sistema ya ha sido inicializado automáticamente con una flota de 8 vehículos de prueba gracias al script de inicio (`init.sql`). Cada vehículo está configurado para validar una de las reglas de negocio requeridas.

**Instrucciones:**
1. Abra Swagger: [http://localhost:8080/swagger](http://localhost:8080/swagger)
2. Busque el endpoint **`POST /api/telemetry`**.
3. Haga clic en **Try it out**.
4. Copie el JSON de cada caso, péguelo en el campo "Request body" y haga clic en **Execute**.
5. Verifique el código **202 Accepted** y revise los logs del contenedor `ccs_notifications` para ver la alerta.

---

### 🏎️ Caso 1: Prueba de Estrés (Stress Test Check)

- **Vehículo:** `STRESS-TEST`

- **Regla:** Límite de velocidad muy bajo (**10 km/h**) para facilitar pruebas de carga.

- **Escenario:** El vehículo va a 50 km/h.

```json
{
  "plate": "STRESS-TEST",
  "speed": 50,
  "lat": 6.2, "lon": -75.5, "heading": 0,
  "metadata": {}
}

### 🛑 Caso 2: Exceso de Velocidad (MaxSpeed)

-   **Vehículo:** `SPEED-TRUCK`

-   **Regla:** Límite máximo de **80 km/h**.

-   **Escenario:** El camión reporta una velocidad de **100 km/h**.

`{
  "plate": "SPEED-TRUCK",
  "speed": 100,
  "lat": 6.2,
  "lon": -75.5,
  "heading": 0,
  "metadata": {}
}`

**Resultado en Logs:** `"Velocidad 100 supera límite de 80"`.

* * * * *

### ❄️ Caso 3: Temperatura de Carga (CargoTemperature)

-   **Vehículo:** `FROZEN-01`

-   **Regla:** Temperatura máxima permitida de **-5°C**.

-   **Escenario:** La temperatura sube a **2.0°C** (riesgo de descongelamiento).

`{
  "plate": "FROZEN-01",
  "speed": 40,
  "lat": 6.2,
  "lon": -75.5,
  "heading": 0,
  "metadata": {
    "cargoTemp": 2.0
  }
}`

**Resultado en Logs:** `"Temperatura 2°C excede límite de -5°C"`.

* * * * *

### 🚨 Caso 4: Botón de Pánico (PanicButton)

-   **Vehículo:** `TAXI-SOS`

-   **Regla:** Generar alerta crítica si el botón es activado.

-   **Escenario:** El conductor presiona el botón de pánico.

`{
  "plate": "TAXI-SOS",
  "speed": 0,
  "lat": 6.2,
  "lon": -75.5,
  "heading": 0,
  "metadata": {
    "panic": true
  }
}`

**Resultado en Logs:** `"PANIC BUTTON ACTIVATED"`.

* * * * *

### 🅿️ Caso 5: Detención No Planeada (UnplannedStop)

-   **Vehículo:** `MONEY-TRUCK`

-   **Regla:** Alerta si la velocidad baja a **0 km/h** en ruta.

-   **Escenario:** El camión de valores se detiene inesperadamente.

`{
  "plate": "MONEY-TRUCK",
  "speed": 0,
  "lat": 6.2,
  "lon": -75.5,
  "heading": 0,
  "metadata": {}
}`

**Resultado en Logs:** `"Detención no planeada detectada"`.

* * * * *

### 🕒 Caso 6: Horario Restringido (RestrictedSchedule)

-   **Vehículo:** `MOTO-NIGHT`

-   **Regla:** Prohibido circular entre **00:00 y 23:59** (restricción total para pruebas).

-   **Escenario:** La motocicleta reporta movimiento.

`{
  "plate": "MOTO-NIGHT",
  "speed": 30,
  "lat": 6.2,
  "lon": -75.5,
  "heading": 0,
  "metadata": {}
}`

**Resultado en Logs:** `"Movimiento en horario no permitido"`.

* * * * *

### 🗺️ Caso 7: Geocerca (Geofence)

-   **Vehículo:** `ZONE-CAR`

-   **Regla:** Debe permanecer dentro de un radio de **500m** del punto (6.2, -75.5).

-   **Escenario:** El vehículo se aleja (latitud 7.0 → fuera de zona).

`{
  "plate": "ZONE-CAR",
  "speed": 40,
  "lat": 7.0,
  "lon": -75.5,
  "heading": 0,
  "metadata": {}
}`

**Resultado en Logs:** `"Vehículo fuera de geocerca..."`.

* * * * *

### 🚪 Caso 8: Sensor de Puerta (DoorSensor)

-   **Vehículo:** `DOOR-VAN`

-   **Regla:** La puerta trasera **no puede abrirse mientras hay movimiento**.

-   **Escenario:** El vehículo reporta `doorOpen: true` a **50 km/h**.

`{
  "plate": "DOOR-VAN",
  "speed": 50,
  "lat": 6.2,
  "lon": -75.5,
  "heading": 0,
  "metadata": {
    "doorOpen": true
  }
}`

**Resultado en Logs:** `"Puerta trasera abierta detectada con vehículo en movimiento"`

# 🧪 Guía de Pruebas y Cobertura (Code Coverage)

Este documento detalla cómo ejecutar las pruebas unitarias y de integración, así como generar el reporte visual de cobertura de código para validar la calidad del software.

---

## 📋 1. Requisitos Previos

A diferencia de la ejecución con Docker, para correr las pruebas de código fuente se requiere:
* .NET 8 SDK instalado.
* Herramienta de Reportes (se instala en el paso 3).

---

## 🚀 2. Ejecución Rápida de Pruebas

Para verificar que toda la lógica de negocio y las integraciones funcionan correctamente, abra una terminal en la raíz del proyecto y ejecute:

dotnet test

---

## 📊 3. Generar Reporte de Cobertura (Code Coverage)

Para visualizar qué porcentaje del código está cubierto por pruebas, siga estos pasos:

Resultado esperado:

Passed!


Paso A: Ejecutar Tests recolectando datos

Este comando ejecuta las pruebas y crea un archivo XML con las métricas de cobertura.

PowerShell:

dotnet test --collect:"XPlat Code Coverage"

Paso B: Instalar Generador de Reportes

Para convertir ese archivo XML en un reporte HTML navegable, instale ReportGenerator:

dotnet tool install -g dotnet-reportgenerator-globaltool

Paso C: Generar Reporte HTML

Ejecute este comando para generar el informe visual:

reportgenerator -reports:"./TestResults/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

Paso D: Abrir el Reporte

El reporte quedará en:

coveragereport/index.html

Ábralo con doble clic desde su explorador de archivos.
