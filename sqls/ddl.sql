/* Crear base de datos si no existe */
IF DB_ID('BDCitasMedicas') IS NULL
BEGIN
    CREATE DATABASE BDCitasMedicas;
END
GO

USE BDCitasMedicas;
GO

/* Limpieza idempotente (orden respetando dependencias) */
IF OBJECT_ID('dbo.tbMedicoEspecialidad','U') IS NOT NULL DROP TABLE dbo.tbMedicoEspecialidad;
IF OBJECT_ID('dbo.tbHorarioMedico','U')        IS NOT NULL DROP TABLE dbo.tbHorarioMedico;
IF OBJECT_ID('dbo.tbCita','U')                 IS NOT NULL DROP TABLE dbo.tbCita;
IF OBJECT_ID('dbo.tbErrorLog','U')             IS NOT NULL DROP TABLE dbo.tbErrorLog;
IF OBJECT_ID('dbo.tbMedico','U')               IS NOT NULL DROP TABLE dbo.tbMedico;
IF OBJECT_ID('dbo.tbEspecialidad','U')         IS NOT NULL DROP TABLE dbo.tbEspecialidad;
IF OBJECT_ID('dbo.tbUsuario','U')              IS NOT NULL DROP TABLE dbo.tbUsuario;
IF OBJECT_ID('dbo.tbPerfil','U')               IS NOT NULL DROP TABLE dbo.tbPerfil;
GO

/* Perfiles / Roles */
CREATE TABLE dbo.tbPerfil(
    ConsecutivoPerfil INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tbPerfil PRIMARY KEY,
    Nombre            NVARCHAR(30) NOT NULL CONSTRAINT UQ_tbPerfil_Nombre UNIQUE
);
GO

/* Usuarios (Admin/Doctor/Paciente) */
CREATE TABLE dbo.tbUsuario(
    ConsecutivoUsuario INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tbUsuario PRIMARY KEY,
    Identificacion     NVARCHAR(25)  NOT NULL,
    Nombre             NVARCHAR(120) NOT NULL,
    CorreoElectronico  NVARCHAR(150) NOT NULL,
    Contrasenna        NVARCHAR(100) NOT NULL, -- Texto plano por consigna
    Estado             BIT           NOT NULL CONSTRAINT DF_tbUsuario_Estado DEFAULT(1),
    ConsecutivoPerfil  INT           NOT NULL CONSTRAINT FK_tbUsuario_tbPerfil REFERENCES dbo.tbPerfil(ConsecutivoPerfil)
);
GO
ALTER TABLE dbo.tbUsuario ADD CONSTRAINT UQ_tbUsuario_Correo         UNIQUE (CorreoElectronico);
ALTER TABLE dbo.tbUsuario ADD CONSTRAINT UQ_tbUsuario_Identificacion UNIQUE (Identificacion);
GO

/* Médicos (vínculo obligatorio 1:1 con usuario de perfil Doctor) */
CREATE TABLE dbo.tbMedico(
    ConsecutivoMedico  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tbMedico PRIMARY KEY,
    Nombre             NVARCHAR(120) NOT NULL,
    Identificacion     NVARCHAR(25)  NOT NULL,
    Correo             NVARCHAR(150) NOT NULL,
    Estado             BIT           NOT NULL CONSTRAINT DF_tbMedico_Estado DEFAULT(1),
    ConsecutivoUsuario INT           NOT NULL CONSTRAINT FK_tbMedico_Usuario REFERENCES dbo.tbUsuario(ConsecutivoUsuario)
);
GO
ALTER TABLE dbo.tbMedico ADD CONSTRAINT UQ_tbMedico_Identificacion     UNIQUE (Identificacion);
ALTER TABLE dbo.tbMedico ADD CONSTRAINT UQ_tbMedico_Correo             UNIQUE (Correo);
ALTER TABLE dbo.tbMedico ADD CONSTRAINT UQ_tbMedico_ConsecutivoUsuario UNIQUE (ConsecutivoUsuario);
GO

/* Especialidades (catálogo) */
CREATE TABLE dbo.tbEspecialidad(
    ConsecutivoEspecialidad INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tbEspecialidad PRIMARY KEY,
    Nombre                  NVARCHAR(120) NOT NULL CONSTRAINT UQ_tbEspecialidad_Nombre UNIQUE,
    Estado                  BIT NOT NULL CONSTRAINT DF_tbEspecialidad_Estado DEFAULT(1)
);
GO

/* Relación N:N Médico-Especialidad */
CREATE TABLE dbo.tbMedicoEspecialidad(
    ConsecutivoMedico       INT NOT NULL CONSTRAINT FK_tbME_Medico       REFERENCES dbo.tbMedico(ConsecutivoMedico),
    ConsecutivoEspecialidad INT NOT NULL CONSTRAINT FK_tbME_Especialidad REFERENCES dbo.tbEspecialidad(ConsecutivoEspecialidad),
    CONSTRAINT PK_tbMedicoEspecialidad PRIMARY KEY (ConsecutivoMedico, ConsecutivoEspecialidad)
);
GO

/* Horarios del Médico (ventanas de atención) */
CREATE TABLE dbo.tbHorarioMedico(
    ConsecutivoHorario INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tbHorarioMedico PRIMARY KEY,
    ConsecutivoMedico  INT NOT NULL CONSTRAINT FK_tbHorarioMedico_Medico REFERENCES dbo.tbMedico(ConsecutivoMedico),
    DiaSemana          TINYINT NOT NULL,         -- 1=Lunes ... 7=Domingo
    HoraInicio         TIME(0) NOT NULL,
    HoraFin            TIME(0) NOT NULL,
    CONSTRAINT CK_tbHorarioMedico_Dia   CHECK (DiaSemana BETWEEN 1 AND 7),
    CONSTRAINT CK_tbHorarioMedico_Rango CHECK (HoraFin > HoraInicio)
);
GO
CREATE INDEX IX_tbHorarioMedico_Medico_Dia ON dbo.tbHorarioMedico(ConsecutivoMedico, DiaSemana, HoraInicio, HoraFin);
GO

/* Citas */
CREATE TABLE dbo.tbCita(
    ConsecutivoCita     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tbCita PRIMARY KEY,
    ConsecutivoPaciente INT NOT NULL CONSTRAINT FK_tbCita_Paciente REFERENCES dbo.tbUsuario(ConsecutivoUsuario),
    ConsecutivoMedico   INT NOT NULL CONSTRAINT FK_tbCita_Medico   REFERENCES dbo.tbMedico(ConsecutivoMedico),
    Fecha               DATE    NOT NULL,
    HoraInicio          TIME(0) NOT NULL,
    HoraFin             TIME(0) NOT NULL,
    Estado              NVARCHAR(20) NOT NULL,
    Observaciones       NVARCHAR(500) NULL,
    CONSTRAINT CK_tbCita_Rango CHECK (HoraFin > HoraInicio)
);
GO
/* Estados válidos de la cita: Programada, Reprogramada, Completada, Cancelada */
ALTER TABLE dbo.tbCita
  ADD CONSTRAINT CK_tbCita_Estado
      CHECK (Estado IN (N'Programada', N'Reprogramada', N'Completada', N'Cancelada'));
GO
ALTER TABLE dbo.tbCita
  ADD CONSTRAINT DF_tbCita_Estado DEFAULT (N'Programada') FOR Estado;
GO
CREATE INDEX IX_tbCita_Medico_Fecha ON dbo.tbCita(ConsecutivoMedico, Fecha, HoraInicio, HoraFin);
GO

/* Bitácora de errores */
CREATE TABLE dbo.tbErrorLog(
    Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tbErrorLog PRIMARY KEY,
    FechaHora   DATETIME2(0) NOT NULL CONSTRAINT DF_tbErrorLog_FechaHora DEFAULT (SYSUTCDATETIME()),
    UsuarioId   INT NULL CONSTRAINT FK_tbErrorLog_Usuario REFERENCES dbo.tbUsuario(ConsecutivoUsuario),
    Area        NVARCHAR(50)  NULL,
    Controlador NVARCHAR(50)  NULL,
    Accion      NVARCHAR(50)  NULL,
    Mensaje     NVARCHAR(MAX) NOT NULL,
    StackTrace  NVARCHAR(MAX) NULL,
    DataJson    NVARCHAR(MAX) NULL
);
GO
