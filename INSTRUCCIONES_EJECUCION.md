🚀 Guía de Ejecución Rápida - CCS Tracking System

Este documento detalla los pasos para desplegar, validar y probar la carga de la solución de rastreo vehicular.

📋 1. Requisitos Previos

Solo se requiere tener instalado:

Docker Desktop (Debe estar corriendo).

Git (Para clonar el repositorio).

(Opcional) PowerShell o Terminal Bash.

Nota: No es necesario tener instalado .NET 8 ni bases de datos. Todo corre dentro de contenedores.

🛠️ 2. Despliegue de la Solución

Abra una terminal en la carpeta raíz del proyecto.

Ejecute el siguiente comando para construir y levantar toda la arquitectura:

docker-compose up --build -d


Espere aproximadamente 30 segundos para que todos los servicios (especialmente RabbitMQ y la Base de Datos) inicien correctamente.

Verifique que los 5 contenedores estén activos:

docker ps


Debe ver:
ccs_ingestion, ccs_rules, ccs_notifications, ccs_rabbitmq, ccs_postgres.

✅ 3. Verificación Funcional

Abra su navegador y acceda a las siguientes interfaces para confirmar que el sistema está operativo:

Documentación API (Swagger):
👉 http://localhost:8080/swagger

(Aquí puede probar los endpoints manualmente si lo desea).

Panel de Mensajería (RabbitMQ):
👉 http://localhost:15672

Usuario: guest
Contraseña: guest

⚡ 4. Prueba de Carga (Stress Test)

Para demostrar que el sistema soporta +500 peticiones por segundo, se incluye un script de prueba automatizado con k6.

🅰️ Paso A: Preparar Datos (Crear Vehículo)

Para generar alertas reales durante la prueba:

Vaya a Swagger → http://localhost:8080/swagger

Ejecute el endpoint:

1. Crear Dueño

POST /api/owners → Try it out → Execute
(Crea el Dueño con ID = 1)

2. Crear Vehículo

POST /api/vehicles
Body:

{
  "plate": "STRESS-TEST",
  "type": 1,
  "ownerId": 1
}

3. Crear Regla de Velocidad

POST /api/vehicles/{plate}/rules

Plate: STRESS-TEST
Body:

{
  "ruleType": 1,
  "threshold": "10"
}

4. Recargar Motor de Reglas

Ejecute:

docker restart ccs_rules

🅱️ Paso B: Ejecutar el Ataque

Asegúrese de estar en la carpeta donde está el archivo stress-test.js.

En Windows (PowerShell):
Get-Content stress-test.js | docker run --rm -i --add-host=host.docker.internal:host-gateway grafana/k6 run -

En Mac/Linux:
docker run --rm -i --add-host=host.docker.internal:host-gateway grafana/k6 run - < stress-test.js

🎯 Resultados Esperados

Al finalizar la prueba, verá algo similar a:

http_reqs: ~600/s

checks: 100% (todas las peticiones exitosas)

🧹 5. Detener y Limpiar

Para detener la solución y liberar recursos:

docker-compose down

🆘 Solución de Problemas Comunes
⚠️ Error: "Ports are not available"

Asegúrese de no tener otro servicio usando los puertos 8080 o 5432.

⚠️ Error: "Connection refused" en los logs

RabbitMQ puede tardar en iniciar.
Espere un minuto o reinicie el servicio afectado:

docker restart <nombre_contenedor>
