/*
 
CREATE DATABASE "edutrack";
CREATE ROLE edudrole WITH PASSWORD 'Qwerty#2025';
GRANT ALL PRIVILEGES ON DATABASE "edutrack" TO edudrole;
GRANT ALL PRIVILEGES ON SCHEMA public TO edudrole;
ALTER ROLE edudrole SET search_path TO public;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO edudrole;
ALTER ROLE edudrole LOGIN;
  
*/

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE tipo_configuracion (
 id text NOT NULL,
 nombre text NOT NULL, -- Descripción de la configuración
 CONSTRAINT tipo_configuracion_pk PRIMARY KEY (id)
);

CREATE TABLE configuracion (
 id text NOT NULL,
 valor text NOT NULL,
 CONSTRAINT configuracion_pk PRIMARY KEY (id),
 CONSTRAINT configuracion_fk_tipo_configuracion FOREIGN KEY (id)
 REFERENCES tipo_configuracion (id) MATCH SIMPLE ON UPDATE RESTRICT ON DELETE RESTRICT
);

CREATE TABLE rol (
 id text NOT NULL,
 nombre text NOT NULL,
 CONSTRAINT rol_pk PRIMARY KEY (id),
 CONSTRAINT rol_codigo_unq UNIQUE (nombre)
);

INSERT INTO rol (id, nombre) VALUES
 ('00', 'ADMIN'),
 ('01', 'MARCADOR');
 
CREATE TABLE usuario (
 id uuid NOT NULL DEFAULT uuid_generate_v4(),
 nombre_usuario text NOT NULL,
 contrasena_hash text NOT NULL,
 sello_seguridad uuid NOT NULL DEFAULT uuid_generate_v4(), -- Un valor aleatorio que debería cambiar cuando se modifiquen credenciales
 activo boolean NOT NULL DEFAULT true,
 CONSTRAINT usuario_pk PRIMARY KEY (id),
 CONSTRAINT usuario_usuario_unq UNIQUE (nombre_usuario)
);

--Usuario 'admin' con clave "AaBbCcDs12345"
INSERT INTO usuario(nombre_usuario,contrasena_hash) VALUES('admin','ALiah/YdxclgLLhWoIw11aa8F4RcCP1b6f0l12wyENzsRmxPWYPn7I+v4pF93Bc8qg==');

--Usuario 'marcador1' con clave "AaBbCcDs12345"
INSERT INTO usuario(nombre_usuario,contrasena_hash) VALUES('marcador1','ALiah/YdxclgLLhWoIw11aa8F4RcCP1b6f0l12wyENzsRmxPWYPn7I+v4pF93Bc8qg==');

CREATE TABLE usuario_rol (
 usuario_id uuid NOT NULL,
 rol_id text NOT NULL,
 CONSTRAINT usuario_rol_pk PRIMARY KEY (usuario_id, rol_id),
 CONSTRAINT usuario_rol_fk_usuario FOREIGN KEY(usuario_id) REFERENCES usuario (id) MATCH SIMPLE ON UPDATE RESTRICT ON DELETE CASCADE,
 CONSTRAINT usuario_rol_fk_rol FOREIGN KEY (rol_id) REFERENCES rol (id) MATCH SIMPLE ON UPDATE RESTRICT ON DELETE RESTRICT
);

INSERT INTO usuario_rol SELECT id,'00' FROM usuario WHERE nombre_usuario='admin';
INSERT INTO usuario_rol SELECT id,'01' FROM usuario WHERE nombre_usuario='marcador1';

CREATE TABLE grado (
    id uuid DEFAULT uuid_generate_v4(),
    nombre_grado text NOT NULL,
    CONSTRAINT grado_pk PRIMARY KEY(id)
);

INSERT INTO grado(nombre_grado) VALUES('1° A - SECUNDARIA');

CREATE TABLE alumno (
    id uuid DEFAULT uuid_generate_v4(),
    nombre_completo text NOT NULL,
    grado_id uuid NOT NULL,
    biometrico_id int NOT NULL,
    nombre_apoderado text NOT NULL,
    numero_apoderado text NOT NULL,
    CONSTRAINT alumno_PK PRIMARY KEY(id),
    CONSTRAINT alumno_grado_fk FOREIGN KEY(grado_id) REFERENCES grado(id) MATCH SIMPLE ON UPDATE RESTRICT ON DELETE CASCADE,
    CONSTRAINT unique_biometrico_grado UNIQUE (grado_id, biometrico_id)
);

-- Crear tabla de asistencia (para registrar los horarios de entrada y salida de los alumnos)
CREATE TABLE asistencia (
    id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    alumno_id uuid NOT NULL,  -- Relacionado con el alumno
    fecha_hora timestamp with time zone DEFAULT now(),
    CONSTRAINT fk_alumno FOREIGN KEY (alumno_id) REFERENCES alumno(id) MATCH SIMPLE ON UPDATE RESTRICT ON DELETE CASCADE
);