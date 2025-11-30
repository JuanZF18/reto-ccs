-- .docker/postgres-init/01_init_schema.sql

CREATE TABLE IF NOT EXISTS owners (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(100) NOT NULL,
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS vehicles (
    plate VARCHAR(20) PRIMARY KEY,
    owner_id INT REFERENCES owners(id),
    type INT NOT NULL,
    is_active BOOLEAN DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS rule_configs (
    id SERIAL PRIMARY KEY,
    vehicle_plate VARCHAR(20) REFERENCES vehicles(plate),
    rule_type INT NOT NULL, 
    threshold_value VARCHAR(50), 
    is_active BOOLEAN DEFAULT TRUE
);