USE BDCitasMedicas;
GO

IF OBJECT_ID('dbo.sp_ValidarSolapeCita', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ValidarSolapeCita;
GO

CREATE PROCEDURE dbo.sp_ValidarSolapeCita
    @MedicoId    INT,
    @Fecha       DATE,
    @HoraInicio  TIME(0),
    @HoraFin     TIME(0)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @HaySolape BIT = 0;
    DECLARE @CitaChoque INT = NULL;
    DECLARE @HayFueraHorario BIT = 0;

    /* Normalizamos el día de la semana a 1..7 con lunes=1 */
    DECLARE @DiaSemana TINYINT;
    DECLARE @prevDF INT = @@DATEFIRST;  -- guardar valor actual
    SET DATEFIRST 1;                    -- 1 = lunes
    SET @DiaSemana = DATEPART(WEEKDAY, @Fecha);
    SET DATEFIRST @prevDF;              -- restaurar

    /* 1) Validar que la cita cae completamente dentro de una ventana de horario del médico */
    IF NOT EXISTS (
        SELECT 1
        FROM dbo.tbHorarioMedico h
        WHERE h.ConsecutivoMedico = @MedicoId
          AND h.DiaSemana = @DiaSemana
          AND @HoraInicio >= h.HoraInicio
          AND @HoraFin    <= h.HoraFin
    )
    BEGIN
        SET @HayFueraHorario = 1;
    END

    /* 2) Validar solape con citas NO canceladas (Programada/Reprogramada/Completada bloquean el rango)
          Nota: aunque 'Completada' suele ser pasado, mantenemos la regla general de no permitir
          que otra cita coincida en el mismo rango temporal registrado para ese médico y fecha.
          Si preferís que 'Completada' NO bloquee (histórico), se puede cambiar a: C.Estado IN ('Programada','Reprogramada') */
    IF @HayFueraHorario = 0
    BEGIN
        SELECT TOP 1 @CitaChoque = C.ConsecutivoCita
        FROM dbo.tbCita C
        WHERE
            C.ConsecutivoMedico = @MedicoId
            AND C.Fecha = @Fecha
            AND C.Estado <> N'Cancelada'
            AND (@HoraInicio < C.HoraFin) AND (C.HoraInicio < @HoraFin);

        IF @CitaChoque IS NOT NULL
            SET @HaySolape = 1;
    END

    SELECT 
        @HayFueraHorario AS HayFueraHorario,
        @HaySolape       AS HaySolape,
        @CitaChoque      AS ConsecutivoCitaChoque;
END
GO

--------------------------------------------------------------------------
--OBTENER USUARIO

USE BDCitasMedicas;
GO

IF OBJECT_ID('dbo.sp_GetUsuarioPorId') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetUsuarioPorId;
GO

CREATE PROCEDURE dbo.sp_GetUsuarioPorId
(
    @ConsecutivoUsuario INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  ConsecutivoUsuario,
            Identificacion,
            Nombre,
            CorreoElectronico
    FROM    dbo.tbUsuario           -- 👈 NOMBRE CORRECTO DE LA TABLA
    WHERE   ConsecutivoUsuario = @ConsecutivoUsuario
      AND   Estado = 1;             -- si usas Estado como activo/inactivo
END;
GO


---------------------------------------------------------------------------
--CAMBIAR PASSWORD

USE BDCitasMedicas;
GO

IF OBJECT_ID('dbo.sp_CambiarContrasena') IS NOT NULL
    DROP PROCEDURE dbo.sp_CambiarContrasena;
GO

CREATE PROCEDURE dbo.sp_CambiarContrasena
(
    @ConsecutivoUsuario INT,
    @ContrasenaActual   NVARCHAR(100),
    @ContrasenaNueva    NVARCHAR(100)
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Codigo  INT;
    DECLARE @Mensaje NVARCHAR(200);

    -- Validar contraseña actual
    IF NOT EXISTS (
        SELECT 1
        FROM   dbo.tbUsuario
        WHERE  ConsecutivoUsuario = @ConsecutivoUsuario
          AND  Contrasenna = @ContrasenaActual
          AND  Estado = 1
    )
    BEGIN
        SET @Codigo  = 0;
        SET @Mensaje = N'La contraseña actual es incorrecta.';
    END
    ELSE
    BEGIN
        UPDATE dbo.tbUsuario
        SET    Contrasenna = @ContrasenaNueva
        WHERE  ConsecutivoUsuario = @ConsecutivoUsuario;

        SET @Codigo  = 1;
        SET @Mensaje = N'Contraseña actualizada correctamente.';
    END

    SELECT @Codigo  AS Codigo,
           @Mensaje AS Mensaje;
END;
GO

--------------------------------------------------------------------

--USUARIOS POR PERFIL

CREATE PROCEDURE dbo.sp_GetUsuariosPorPerfil
(
    @ConsecutivoPerfil INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  ConsecutivoUsuario,
            Identificacion,
            Nombre,
            CorreoElectronico,
            Estado,
            ConsecutivoPerfil
    FROM    dbo.tbUsuario
    WHERE   ConsecutivoPerfil = @ConsecutivoPerfil;
END;
GO

-------------------------------------------------------------

--USUARIO POR ID

CREATE PROCEDURE dbo.sp_GetUsuarioPorId
(
    @ConsecutivoUsuario INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  ConsecutivoUsuario,
            Identificacion,
            Nombre,
            CorreoElectronico,
            Estado,
            ConsecutivoPerfil
    FROM    dbo.tbUsuario
    WHERE   ConsecutivoUsuario = @ConsecutivoUsuario;
END;
GO

---------------------------------------------------------------

--INSERTAR USUARIOS

ALTER PROCEDURE dbo.sp_InsertUsuario
(
    @Identificacion     NVARCHAR(25),
    @Nombre             NVARCHAR(120),
    @Correo             NVARCHAR(150),
    @Contrasenna        NVARCHAR(100),
    @Estado             BIT,
    @ConsecutivoPerfil  INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Codigo           INT;
    DECLARE @Mensaje          NVARCHAR(200);
    DECLARE @NuevoConsecutivo INT;

    -- Validar que no exista la identificación o correo
    IF EXISTS (SELECT 1 FROM dbo.tbUsuario WHERE Identificacion = @Identificacion)
    BEGIN
        SET @Codigo           = 0;
        SET @Mensaje          = N'Ya existe un usuario con esa identificación.';
        SET @NuevoConsecutivo = 0;
    END
    ELSE IF EXISTS (SELECT 1 FROM dbo.tbUsuario WHERE CorreoElectronico = @Correo)
    BEGIN
        SET @Codigo           = 0;
        SET @Mensaje          = N'Ya existe un usuario con ese correo electrónico.';
        SET @NuevoConsecutivo = 0;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.tbUsuario
            (Identificacion, Nombre, CorreoElectronico, Contrasenna, Estado, ConsecutivoPerfil)
        VALUES
            (@Identificacion, @Nombre, @Correo, @Contrasenna, @Estado, @ConsecutivoPerfil);

        SET @NuevoConsecutivo = SCOPE_IDENTITY();
        SET @Codigo           = 1;
        SET @Mensaje          = N'Usuario creado correctamente.';
    END

    SELECT @Codigo           AS Codigo, 
           @Mensaje          AS Mensaje,
           @NuevoConsecutivo AS ConsecutivoUsuario;
END;
GO




----------------------------------------------------------

--ACTUALIZAR USUARIO

CREATE PROCEDURE dbo.sp_UpdateUsuario
(
    @ConsecutivoUsuario INT,
    @Identificacion     NVARCHAR(25),
    @Nombre             NVARCHAR(120),
    @Correo             NVARCHAR(150),
    @Estado             BIT,
    @ConsecutivoPerfil  INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Codigo  INT;
    DECLARE @Mensaje NVARCHAR(200);

    IF NOT EXISTS (SELECT 1 FROM dbo.tbUsuario WHERE ConsecutivoUsuario = @ConsecutivoUsuario)
    BEGIN
        SET @Codigo  = 0;
        SET @Mensaje = N'No se encontró el usuario.';
    END
    ELSE
    BEGIN
        UPDATE dbo.tbUsuario
        SET Identificacion    = @Identificacion,
            Nombre            = @Nombre,
            CorreoElectronico = @Correo,
            Estado            = @Estado,
            ConsecutivoPerfil = @ConsecutivoPerfil
        WHERE ConsecutivoUsuario = @ConsecutivoUsuario;

        SET @Codigo  = 1;
        SET @Mensaje = N'Usuario actualizado correctamente.';
    END

    SELECT @Codigo AS Codigo, @Mensaje AS Mensaje;
END;
GO

----------------------------------------------------------------

--SOFT DELETE USUARIO

CREATE PROCEDURE dbo.sp_DeleteUsuario
(
    @ConsecutivoUsuario INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Codigo  INT;
    DECLARE @Mensaje NVARCHAR(200);

    IF NOT EXISTS (SELECT 1 FROM dbo.tbUsuario WHERE ConsecutivoUsuario = @ConsecutivoUsuario)
    BEGIN
        SET @Codigo  = 0;
        SET @Mensaje = N'No se encontró el usuario.';
    END
    ELSE
    BEGIN
        UPDATE dbo.tbUsuario
        SET Estado = 0
        WHERE ConsecutivoUsuario = @ConsecutivoUsuario;

        SET @Codigo  = 1;
        SET @Mensaje = N'Usuario desactivado correctamente.';
    END

    SELECT @Codigo AS Codigo, @Mensaje AS Mensaje;
END;
GO

-----------------------------------------------------------
--AGREGAR MEDICO

CREATE PROCEDURE dbo.sp_InsertMedico
(
    @ConsecutivoUsuario INT,
    @Nombre             NVARCHAR(120),
    @Identificacion     NVARCHAR(25),
    @Correo             NVARCHAR(150),
    @Estado             BIT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Codigo        INT;
    DECLARE @Mensaje       NVARCHAR(200);
    DECLARE @NuevoMedicoId INT;

    IF NOT EXISTS (SELECT 1 FROM dbo.tbUsuario WHERE ConsecutivoUsuario = @ConsecutivoUsuario)
    BEGIN
        SET @Codigo  = 0;
        SET @Mensaje = N'No existe el usuario asociado.';
    END
    ELSE
    BEGIN
        INSERT INTO dbo.tbMedico
            (Nombre, Identificacion, Correo, Estado, ConsecutivoUsuario)
        VALUES
            (@Nombre, @Identificacion, @Correo, @Estado, @ConsecutivoUsuario);

        SET @NuevoMedicoId = SCOPE_IDENTITY();
        SET @Codigo        = 1;
        SET @Mensaje       = N'Médico creado correctamente.';
    END

    SELECT @Codigo        AS Codigo,
           @Mensaje       AS Mensaje,
           @NuevoMedicoId AS ConsecutivoMedico;
END;
GO

----------------------------------------------

--OBTENER MEDICOS

CREATE PROCEDURE dbo.sp_GetMedicos
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  m.ConsecutivoMedico,
            m.Nombre,
            m.Identificacion,
            m.Correo AS CorreoElectronico,
            m.Estado,
            m.ConsecutivoUsuario
    FROM    dbo.tbMedico m;
END;
GO

-------------------------------------

--INSERTAR MEDICO - ESPECIALIDAD

CREATE PROCEDURE dbo.sp_InsertMedicoEspecialidad
(
    @ConsecutivoMedico     INT,
    @ConsecutivoEspecialidad INT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM   dbo.tbMedicoEspecialidad
        WHERE  ConsecutivoMedico     = @ConsecutivoMedico
          AND  ConsecutivoEspecialidad = @ConsecutivoEspecialidad
    )
    BEGIN
        INSERT INTO dbo.tbMedicoEspecialidad (ConsecutivoMedico, ConsecutivoEspecialidad)
        VALUES (@ConsecutivoMedico, @ConsecutivoEspecialidad);
    END
END;
GO

----------------------------------------------------------

--LISTAR ESPECIALIDADES - MEDICO

CREATE PROCEDURE dbo.sp_GetEspecialidadesPorMedico
(
    @ConsecutivoMedico INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT me.ConsecutivoMedico,
           me.ConsecutivoEspecialidad
           -- aquí puedes hacer JOIN a tbEspecialidad para sacar el nombre
    FROM   dbo.tbMedicoEspecialidad me
    WHERE  me.ConsecutivoMedico = @ConsecutivoMedico;
END;
GO

------------------------------------

--INSERT HORARIO MEDICO

CREATE PROCEDURE dbo.sp_InsertHorarioMedico
(
    @ConsecutivoMedico INT,
    @DiaSemana         TINYINT,   -- 1=lunes ... 7=domingo (por ejemplo)
    @HoraInicio        TIME(0),
    @HoraFin           TIME(0)
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.tbHorarioMedico (ConsecutivoMedico, DiaSemana, HoraInicio, HoraFin)
    VALUES (@ConsecutivoMedico, @DiaSemana, @HoraInicio, @HoraFin);
END;
GO

-------------------------------

--LISTAR HORARIO MEDICO

CREATE PROCEDURE dbo.sp_GetHorariosPorMedico
(
    @ConsecutivoMedico INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ConsecutivoHorario,
           ConsecutivoMedico,
           DiaSemana,
           HoraInicio,
           HoraFin
    FROM   dbo.tbHorarioMedico
    WHERE  ConsecutivoMedico = @ConsecutivoMedico;
END;
GO

-----------------------------------------

--Obtener medico id

CREATE PROCEDURE dbo.sp_GetMedicoPorId
(
    @ConsecutivoMedico INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  m.ConsecutivoMedico,
            m.Nombre,
            m.Identificacion,
            m.Correo AS CorreoElectronico,
            m.Estado,
            m.ConsecutivoUsuario
    FROM    dbo.tbMedico m
    WHERE   m.ConsecutivoMedico = @ConsecutivoMedico;
END;
GO

--------------------------------------------

--Actualizar medico

CREATE PROCEDURE dbo.sp_UpdateMedico
(
    @ConsecutivoMedico INT,
    @Nombre            NVARCHAR(120),
    @Identificacion    NVARCHAR(25),
    @Correo            NVARCHAR(150),
    @Estado            BIT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Codigo  INT;
    DECLARE @Mensaje NVARCHAR(200);

    IF NOT EXISTS (SELECT 1 FROM dbo.tbMedico WHERE ConsecutivoMedico = @ConsecutivoMedico)
    BEGIN
        SET @Codigo  = 0;
        SET @Mensaje = N'No se encontró el médico.';
    END
    ELSE
    BEGIN
        UPDATE dbo.tbMedico
        SET Nombre        = @Nombre,
            Identificacion= @Identificacion,
            Correo        = @Correo,
            Estado        = @Estado
        WHERE ConsecutivoMedico = @ConsecutivoMedico;

        SET @Codigo  = 1;
        SET @Mensaje = N'Médico actualizado correctamente.';
    END

    SELECT @Codigo AS Codigo, @Mensaje AS Mensaje;
END;
GO

-----------------------------------------

-- Eliminar - desactivar medico

CREATE PROCEDURE dbo.sp_DeleteMedico
(
    @ConsecutivoMedico INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Codigo  INT;
    DECLARE @Mensaje NVARCHAR(200);

    IF NOT EXISTS (SELECT 1 FROM dbo.tbMedico WHERE ConsecutivoMedico = @ConsecutivoMedico)
    BEGIN
        SET @Codigo  = 0;
        SET @Mensaje = N'No se encontró el médico.';
    END
    ELSE
    BEGIN
        UPDATE dbo.tbMedico
        SET Estado = 0
        WHERE ConsecutivoMedico = @ConsecutivoMedico;

        SET @Codigo  = 1;
        SET @Mensaje = N'Médico desactivado correctamente.';
    END

    SELECT @Codigo AS Codigo, @Mensaje AS Mensaje;
END;
GO

