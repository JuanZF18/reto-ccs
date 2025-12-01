🚀 Guía de Ejecución Rápida – CCS Tracking System

Esta guía describe cómo desplegar, verificar y realizar pruebas de carga y validación funcional sobre la solución de rastreo vehicular CCS Tracking System.

------------------------------------------------------------
# 📚 Tabla de Contenido
------------------------------------------------------------

1. Requisitos Previos  
2. Despliegue de la Solución  
3. Verificación Funcional  
4. Prueba de Carga (Stress Test)  
5. Detener y Limpiar  
6. Solución de Problemas  
7. Validación de Reglas de Negocio  
8. Guía de Pruebas y Cobertura (Code Coverage)

------------------------------------------------------------
# 📋 1. Requisitos Previos
------------------------------------------------------------

Solo necesitas tener instalado:

- Docker Desktop (debe estar corriendo)
- Git (para clonar el repositorio)
- PowerShell o Terminal Bash (opcional)

Nota: No se requiere instalar .NET 8 ni bases de datos.  
Toda la arquitectura corre dentro de contenedores Docker.

------------------------------------------------------------
# 🛠️ 2. Despliegue de la Solución
------------------------------------------------------------

### Despliegue rápido (Docker)

1. Clonar el repositorio y moverse a la raíz del proyecto:
```bash
git clone https://github.com/JuanZF18/reto-ccs
cd reto-ccs
```

2. Levantar los contenedores en segundo plano (desde la raíz donde está docker-compose.yml):
```bash
docker-compose up --build -d
```

3. Verificar que los contenedores estén corriendo:
```bash
docker ps
```

4. Contenedores esperados (deben aparecer en la lista):
```
ccs_ingestion
ccs_rules
ccs_notifications
ccs_rabbitmq
ccs_postgres
```

Notas:
- Si algún contenedor no arranca, ver logs con `docker-compose logs <nombre_contenedor>` o `docker logs <container_id>`.
- Para detener y limpiar: `docker-compose down`.
- Asegúrate de tener Docker Desktop corriendo antes de ejecutar los comandos.

------------------------------------------------------------
# ✅ 3. Verificación Funcional
------------------------------------------------------------

Swagger:  
http://localhost:8080/index.html

RabbitMQ:  
http://localhost:15672  
Usuario: guest  
Contraseña: guest

------------------------------------------------------------
# ⚡ 4. Prueba de Carga (Stress Test)
------------------------------------------------------------

La base de datos ya viene inicializada automáticamente mediante `.docker/postgres-init`.

Ubícate en la carpeta con el archivo stress-test.js.

Ejecute el siguiente comando dependiendo del sistema operativo.

Windows:

Ejecutar desde la carpeta que contiene stress-test.js.

PowerShell (recomendado en Windows):
```powershell
Get-Content -Raw .\stress-test.js |
  docker run --rm -i --add-host=host.docker.internal:host-gateway grafana/k6 run -
```

CMD (Windows):
```cmd
type stress-test.js | docker run --rm -i --add-host=host.docker.internal:host-gateway grafana/k6 run -
```

macOS / Linux / WSL:
```bash
docker run --rm -i --add-host=host.docker.internal:host-gateway grafana/k6 run - < stress-test.js
```

Resultados esperados:  
http_reqs: ~600/s  
checks: 100%

------------------------------------------------------------
# 🧹 5. Detener y Limpiar
------------------------------------------------------------

docker-compose down

------------------------------------------------------------
# 🆘 6. Solución de Problemas
------------------------------------------------------------

“Ports are not available”  
→ Otro proceso usa 8080 o 5432.

“Connection refused”  
→ RabbitMQ tarda en iniciar. Esperar o reiniciar:

docker restart <nombre_contenedor>

------------------------------------------------------------
# 🧪 7. Validación de Reglas de Negocio
------------------------------------------------------------

Abrir Swagger → POST /api/telemetry → Try it out → Enviar JSON → Revisar logs del contenedor ccs_notifications en docker.

Swagger:  
http://localhost:8080/index.html

Para probar todos estos casos simplemente nos ubicamos en el endpoint api/telemetry y pegamos en el body cada uno de los siguientes JSON.

### Caso 1: Prueba de Estrés (Stress Test Check)
- Descripción: 500+ por segundo durante 2min.

---

### Caso 2: Exceso de Velocidad (MaxSpeed)
- Vehículo: SPEED-TRUCK  
- Regla: máximo 80 km/h  
- Escenario: 100 km/h

```json
{
  "plate": "SPEED-TRUCK",
  "speed": 100,
  "lat": 6.2,
  "lon": -75.5,
  "heading": 0,
  "metadata": {}
}
```

Log esperado: “Velocidad 100 supera límite de 80”

---

### Caso 3: Temperatura de Carga (CargoTemperature)
- Vehículo: FROZEN-01  
- Regla: máximo -5°C  
- Escenario: 2°C

```json
{
  "plate": "FROZEN-01",
  "speed": 40,
  "lat": 6.2,
  "lon": -75.5,
  "heading": 0,
  "metadata": {
    "cargo_temp": 2.0
  }
}
```

Log esperado: “Temperatura 2°C excede límite de -5°C”

---

### Caso 4: Botón de Pánico (PanicButton)
- Vehículo: TAXI-SOS  
- Regla: alarma si panic=true

```json
{
  "plate": "TAXI-SOS",
  "speed": 0,
  "lat": 6.2,
  "lon": -75.5,
  "heading": 0,
  "metadata": {
    "panic_btn": true
  }
}
```

Log esperado: “PANIC BUTTON ACTIVATED”

---

### Caso 5: Detención No Planeada (UnplannedStop)
- Vehículo: MONEY-TRUCK  
- Escenario: velocidad = 0

```json
{
  "plate": "MONEY-TRUCK",
  "speed": 0,
  "lat": 6.2,
  "lon": -75.5,
  "heading": 0,
  "metadata": {}
}
```

Log esperado: “Detención no planeada detectada”

---

### Caso 6: Horario Restringido (RestrictedSchedule)
- Vehículo: MOTO-NIGHT  
- Regla: prohibido 00:00–23:59  
- Escenario: movimiento

```json
{
  "plate": "MOTO-NIGHT",
  "speed": 30,
  "lat": 6.2,
  "lon": -75.5,
  "heading": 0,
  "metadata": {}
}
```

Log esperado: “Movimiento en horario no permitido”

---

### Caso 7: Geocerca (Geofence)
- Vehículo: ZONE-CAR  
- Regla: radio 500m desde (6.2, -75.5)  
- Escenario: lat 7.0

```json
{
  "plate": "ZONE-CAR",
  "speed": 40,
  "lat": 7.0,
  "lon": -75.5,
  "heading": 0,
  "metadata": {}
}
```

Log esperado: “Vehículo fuera de geocerca…”

---

### Caso 8: Sensor de Puerta (DoorSensor)
- Vehículo: DOOR-VAN  
- Regla: puerta no puede abrirse si hay movimiento  
- Escenario: doorOpen=true y speed=50

```json
{
  "plate": "DOOR-VAN",
  "speed": 50,
  "lat": 6.2,
  "lon": -75.5,
  "heading": 0,
  "metadata": {
    "doorOpen": true
  }
}
```

Log esperado: “Puerta trasera abierta detectada con vehículo en movimiento”

---

### Caso 9: Choque Detectado (Vibración > 10G)
- Vehículo: CRASH-CAM  
- Regla: Detección automática de accidente por fuerza G (Vibración).  
- Escenario: El sensor detecta un impacto de **45.5 G** (muy superior al límite de 10.0) y adjunta evidencia de video.

```json
{
  "plate": "CRASH-CAM",
  "speed": 0,
  "lat": 6.2, "lon": -75.5, "heading": 0,
  "metadata": {
      "cargo_vib": 45.5,
      "videoUrl": "[https://cdn.ccs.com/evidence/crash_2023.mp4](https://cdn.ccs.com/evidence/crash_2023.mp4)"
  }
}
```

Log esperado: (adjuntar evidencia y revisar alerta generada)

------------------------------------------------------------
# 🧪 Guía de Pruebas y Cobertura (Code Coverage)
------------------------------------------------------------

Requisitos:  
- .NET 8 SDK  
- reportgenerator (se instala más abajo)

Ejecutar pruebas:

dotnet test
```bash
dotnet test
```

Generar datos de cobertura:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Instalar ReportGenerator:

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

Generar reporte HTML:

```bash
reportgenerator -reports:"./TestResults/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

Abrir archivo:

coveragereport/index.html