-- 1. CREACIÓN DE TABLAS (DDL)
CREATE TABLE IF NOT EXISTS owners (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(100) NOT NULL
);

CREATE TABLE IF NOT EXISTS vehicles (
    plate VARCHAR(20) PRIMARY KEY,
    type INT NOT NULL,
    owner_id INT NOT NULL REFERENCES owners(id),
    is_active BOOLEAN DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS rule_configs (
    id SERIAL PRIMARY KEY,
    vehicle_plate VARCHAR(20) NOT NULL REFERENCES vehicles(plate),
    rule_type INT NOT NULL,
    threshold_value VARCHAR(50) NOT NULL,
    is_active BOOLEAN DEFAULT TRUE
);

-- 2. DATOS DE PRUEBA (DML) - SEEDING
TRUNCATE TABLE rule_configs, vehicles, owners RESTART IDENTITY;

-- A. Dueño Genérico
INSERT INTO owners (name, email) VALUES ('CCS Auditor', 'auditor@ccs.com');

-- B. Vehículos para cada Caso de Prueba

-- 1. Carga Masiva (Stress Test)
INSERT INTO vehicles (plate, type, owner_id) VALUES ('STRESS-TEST', 1, 1);
INSERT INTO rule_configs (vehicle_plate, rule_type, threshold_value) VALUES ('STRESS-TEST', 1, '10');

-- 2. Velocidad (Camión)
INSERT INTO vehicles (plate, type, owner_id) VALUES ('SPEED-TRUCK', 2, 1);
INSERT INTO rule_configs (vehicle_plate, rule_type, threshold_value) VALUES ('SPEED-TRUCK', 1, '80'); -- Máx 80 km/h

-- 3. Temperatura (Refrigerado)
INSERT INTO vehicles (plate, type, owner_id) VALUES ('FROZEN-01', 2, 1);
INSERT INTO rule_configs (vehicle_plate, rule_type, threshold_value) VALUES ('FROZEN-01', 5, '-5'); -- Máx -5°C

-- 4. Botón de Pánico (Taxi)
INSERT INTO vehicles (plate, type, owner_id) VALUES ('TAXI-SOS', 1, 1);
INSERT INTO rule_configs (vehicle_plate, rule_type, threshold_value) VALUES ('TAXI-SOS', 4, 'true'); -- Activo

-- 5. Detención No Planeada (Camión de Valores)
INSERT INTO vehicles (plate, type, owner_id) VALUES ('MONEY-TRUCK', 2, 1);
INSERT INTO rule_configs (vehicle_plate, rule_type, threshold_value) VALUES ('MONEY-TRUCK', 3, '0'); -- Alerta si velocidad es 0

-- 6. Horario Restringido (Moto)
INSERT INTO vehicles (plate, type, owner_id) VALUES ('MOTO-NIGHT', 3, 1);
INSERT INTO rule_configs (vehicle_plate, rule_type, threshold_value) VALUES ('MOTO-NIGHT', 7, '00:00-23:59'); 

-- 7. Geocerca (Salida de Zona)
INSERT INTO vehicles (plate, type, owner_id) VALUES ('ZONE-CAR', 1, 1);
INSERT INTO rule_configs (vehicle_plate, rule_type, threshold_value) VALUES ('ZONE-CAR', 2, '6.2,-75.5,500');

-- 8. Sensor de Puerta (Camión Seguro)
INSERT INTO vehicles (plate, type, owner_id) VALUES ('DOOR-VAN', 2, 1);
INSERT INTO rule_configs (vehicle_plate, rule_type, threshold_value) VALUES ('DOOR-VAN', 6, 'true'); -- Alerta si DoorOpen = true

-- 9. Choque con Detección de Fuerza G y Video
INSERT INTO vehicles (plate, type, owner_id) VALUES ('CRASH-CAM', 2, 1);
-- Usamos Regla 4 (Pánico) para que entre al evaluador de eventos críticos
INSERT INTO rule_configs (vehicle_plate, rule_type, threshold_value) VALUES ('CRASH-CAM', 4, 'true');