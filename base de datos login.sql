-- Database: login_db

-- DROP DATABASE IF EXISTS login_db;

CREATE DATABASE login_db
    WITH
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'Spanish_Latin America.1252'
    LC_CTYPE = 'Spanish_Latin America.1252'
    LOCALE_PROVIDER = 'libc'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;




	CREATE SCHEMA IF NOT EXISTS login_db;
SET search_path TO login_db;


CREATE TABLE rol (
    id_rol      SERIAL PRIMARY KEY,
    tipo_rol    VARCHAR(20) NOT NULL UNIQUE
                CHECK (tipo_rol IN ('ADMINISTRADOR', 'DOCENTE', 'ESTUDIANTE'))
);

CREATE TABLE usuario (
    id_usuario      SERIAL PRIMARY KEY,
    id_institucion  INTEGER      NOT NULL, -- ref. lógica a academic_db.institucion
    usuario         VARCHAR(50)  NOT NULL,
    contrasena_hash VARCHAR(255) NOT NULL,
    id_rol          INTEGER      NOT NULL REFERENCES rol(id_rol),
    activo          BOOLEAN      NOT NULL DEFAULT TRUE,
    fecha_creacion  TIMESTAMP    NOT NULL DEFAULT NOW(),
    -- el mismo nombre de usuario puede repetirse entre instituciones distintas
    UNIQUE (id_institucion, usuario)
);

INSERT INTO rol (tipo_rol) VALUES
    ('ADMINISTRADOR'),
    ('DOCENTE'),
    ('ESTUDIANTE');

CREATE INDEX idx_usuario_login ON usuario (id_institucion, usuario);