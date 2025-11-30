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
