-- ============================================================
-- BASE DE DATOS: API GENERAL (ACADÉMICA) - multi-institución
-- Aula Virtual - Producto comercializable a varias instituciones
-- Motor: PostgreSQL
-- ============================================================
-- id_usuario en docente/estudiante es referencia LÓGICA a
-- login_db.usuario (bases de datos separadas, sin FK real).
-- ============================================================

-- ============== TENANT / ESTRUCTURA INSTITUCIONAL ==============

CREATE SCHEMA IF NOT EXISTS aula_virtual_academica;
SET search_path TO aula_virtual_academica;


CREATE TABLE institucion (
    id_institucion  SERIAL PRIMARY KEY,
    nombre          VARCHAR(150) NOT NULL,
    dominio         VARCHAR(100) NOT NULL UNIQUE  -- ej. umes, cunoc (subdominio de acceso)
);

CREATE TABLE facultad (
    id_facultad     SERIAL PRIMARY KEY,
    id_institucion  INTEGER NOT NULL REFERENCES institucion(id_institucion),
    nombre          VARCHAR(100) NOT NULL,
    UNIQUE (id_institucion, nombre)
);

CREATE TABLE carrera (
    id_carrera      SERIAL PRIMARY KEY,
    id_facultad     INTEGER NOT NULL REFERENCES facultad(id_facultad),
    nombre          VARCHAR(100) NOT NULL
);

-- Semestre es catálogo global (1..10), no depende de institución
CREATE TABLE semestre (
    id_semestre SERIAL PRIMARY KEY,
    numero      INTEGER NOT NULL UNIQUE CHECK (numero > 0)
);

CREATE TABLE carrera_semestre (
    id_carrera_semestre SERIAL PRIMARY KEY,
    id_carrera          INTEGER NOT NULL REFERENCES carrera(id_carrera),
    id_semestre         INTEGER NOT NULL REFERENCES semestre(id_semestre),
    UNIQUE (id_carrera, id_semestre)
);

CREATE TABLE curso (
    id_curso        SERIAL PRIMARY KEY,
    id_institucion  INTEGER NOT NULL REFERENCES institucion(id_institucion),
    nombre          VARCHAR(100) NOT NULL,
    codigo          VARCHAR(20)  NOT NULL,
    UNIQUE (id_institucion, codigo)
);

CREATE TABLE pensum (
    id_pensum            SERIAL PRIMARY KEY,
    id_carrera_semestre  INTEGER NOT NULL REFERENCES carrera_semestre(id_carrera_semestre),
    id_curso             INTEGER NOT NULL REFERENCES curso(id_curso),
    UNIQUE (id_carrera_semestre, id_curso)
);

-- ============== PERSONAS ==============

CREATE TABLE docente (
    id_docente      SERIAL PRIMARY KEY,
    id_institucion  INTEGER NOT NULL REFERENCES institucion(id_institucion),
    nombre          VARCHAR(100) NOT NULL,
    apellido        VARCHAR(100) NOT NULL,
    id_usuario      INTEGER NOT NULL UNIQUE  -- ref. lógica a login_db.usuario
);

CREATE TABLE estudiante (
    id_estudiante   SERIAL PRIMARY KEY,
    id_institucion  INTEGER NOT NULL REFERENCES institucion(id_institucion),
    nombre          VARCHAR(100) NOT NULL,
    apellido        VARCHAR(100) NOT NULL,
    carne           VARCHAR(20)  NOT NULL,
    id_carrera      INTEGER NOT NULL REFERENCES carrera(id_carrera),
    id_usuario      INTEGER NOT NULL UNIQUE, -- ref. lógica a login_db.usuario
    UNIQUE (id_institucion, carne)
);

-- ============== ESPACIO FÍSICO / HORARIO ==============

CREATE TABLE aula (
    id_aula         SERIAL PRIMARY KEY,
    id_institucion  INTEGER NOT NULL REFERENCES institucion(id_institucion),
    nombre          VARCHAR(50) NOT NULL,
    capacidad       INTEGER,
    UNIQUE (id_institucion, nombre)
);

CREATE TABLE modulo (
    id_modulo       SERIAL PRIMARY KEY,
    id_institucion  INTEGER NOT NULL REFERENCES institucion(id_institucion),
    nombre          VARCHAR(30) NOT NULL,  -- ej. "Módulo 1"
    hora_inicio     TIME NOT NULL,
    hora_fin        TIME NOT NULL
);

CREATE TABLE horario (
    id_horario  SERIAL PRIMARY KEY,
    id_modulo   INTEGER NOT NULL REFERENCES modulo(id_modulo),
    id_aula     INTEGER NOT NULL REFERENCES aula(id_aula),
    dia         VARCHAR(10) NOT NULL
                CHECK (dia IN ('LUNES','MARTES','MIERCOLES','JUEVES','VIERNES','SABADO')),
    UNIQUE (id_modulo, id_aula, dia)
);

-- ============== OFERTA ACADÉMICA POR CICLO ==============

CREATE TABLE ciclo_academico (
    id_ciclo        SERIAL PRIMARY KEY,
    id_institucion  INTEGER NOT NULL REFERENCES institucion(id_institucion),
    nombre          VARCHAR(30) NOT NULL,  -- ej. "2026 - Ciclo 2"
    fecha_inicio    DATE NOT NULL,
    fecha_fin       DATE NOT NULL,
    UNIQUE (id_institucion, nombre)
);

-- Seccion = nodo central: la oferta real de un curso en un ciclo
CREATE TABLE seccion (
    id_seccion  SERIAL PRIMARY KEY,
    id_pensum   INTEGER NOT NULL REFERENCES pensum(id_pensum),
    id_ciclo    INTEGER NOT NULL REFERENCES ciclo_academico(id_ciclo),
    id_docente  INTEGER NOT NULL REFERENCES docente(id_docente),
    id_horario  INTEGER NOT NULL REFERENCES horario(id_horario)
);

CREATE TABLE estudiante_seccion (
    id_estudiante_seccion SERIAL PRIMARY KEY,
    id_estudiante         INTEGER NOT NULL REFERENCES estudiante(id_estudiante),
    id_seccion            INTEGER NOT NULL REFERENCES seccion(id_seccion),
    fecha_inscripcion     TIMESTAMP NOT NULL DEFAULT NOW(),
    UNIQUE (id_estudiante, id_seccion)
);

-- ============== CLASE VIRTUAL Y ASISTENCIA ==============

CREATE TABLE sesion_virtual (
    id_sesion_virtual SERIAL PRIMARY KEY,
    id_seccion        INTEGER NOT NULL REFERENCES seccion(id_seccion),
    fecha             DATE NOT NULL,
    hora              TIME NOT NULL,
    enlace            VARCHAR(255)  -- link de la videollamada
);

CREATE TABLE asistencia (
    id_asistencia         SERIAL PRIMARY KEY,
    id_estudiante_seccion INTEGER NOT NULL REFERENCES estudiante_seccion(id_estudiante_seccion),
    id_sesion_virtual     INTEGER NOT NULL REFERENCES sesion_virtual(id_sesion_virtual),
    estado                VARCHAR(15) NOT NULL
                          CHECK (estado IN ('PRESENTE','AUSENTE','JUSTIFICADO')),
    UNIQUE (id_estudiante_seccion, id_sesion_virtual)
);

-- ============== CONTENIDO Y EVALUACIÓN ==============

CREATE TABLE recurso (
    id_recurso          SERIAL PRIMARY KEY,
    id_seccion          INTEGER NOT NULL REFERENCES seccion(id_seccion),
    titulo              VARCHAR(150) NOT NULL,
    tipo                VARCHAR(20) NOT NULL CHECK (tipo IN ('DOCUMENTO','ENLACE','VIDEO')),
    url_o_archivo       VARCHAR(255) NOT NULL,
    fecha_publicacion   TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE anuncio (
    id_anuncio          SERIAL PRIMARY KEY,
    id_seccion          INTEGER NOT NULL REFERENCES seccion(id_seccion),
    titulo              VARCHAR(150) NOT NULL,
    contenido           TEXT NOT NULL,
    fecha_publicacion   TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE tarea (
    id_tarea            SERIAL PRIMARY KEY,
    id_seccion          INTEGER NOT NULL REFERENCES seccion(id_seccion),
    titulo              VARCHAR(150) NOT NULL,
    descripcion         TEXT,
    fecha_publicacion   TIMESTAMP NOT NULL DEFAULT NOW(),
    fecha_limite        TIMESTAMP NOT NULL
);

CREATE TABLE entrega (
    id_entrega      SERIAL PRIMARY KEY,
    id_tarea        INTEGER NOT NULL REFERENCES tarea(id_tarea),
    id_estudiante   INTEGER NOT NULL REFERENCES estudiante(id_estudiante),
    archivo         VARCHAR(255) NOT NULL,
    fecha_entrega   TIMESTAMP NOT NULL DEFAULT NOW(),
    nota            NUMERIC(5,2),
    UNIQUE (id_tarea, id_estudiante)
);

CREATE TABLE nota (
    id_nota                SERIAL PRIMARY KEY,
    id_estudiante_seccion  INTEGER NOT NULL REFERENCES estudiante_seccion(id_estudiante_seccion),
    tipo                   VARCHAR(30) NOT NULL,  -- ej. "ZONA", "EXAMEN FINAL"
    valor                  NUMERIC(5,2) NOT NULL
);

-- ============== ÍNDICES DE APOYO ==============

CREATE INDEX idx_estudiante_carrera ON estudiante (id_carrera);
CREATE INDEX idx_pensum_carrera_semestre ON pensum (id_carrera_semestre);
CREATE INDEX idx_seccion_ciclo ON seccion (id_ciclo);
CREATE INDEX idx_tarea_seccion ON tarea (id_seccion);
CREATE INDEX idx_asistencia_estudiante_seccion ON asistencia (id_estudiante_seccion);


INSERT INTO institucion (nombre, dominio) VALUES ('UMES', 'umes');

INSERT INTO semestre (numero) VALUES (1),(2),(3),(4),(5),(6),(7),(8),(9),(10);