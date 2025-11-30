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