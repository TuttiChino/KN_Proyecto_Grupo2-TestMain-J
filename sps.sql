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

    -------------------------------------------------------------------
    -- 📅 Normalizar día de la semana (1=Lunes ... 7=Domingo)
    -------------------------------------------------------------------
    DECLARE @DiaSemana TINYINT;
    DECLARE @prevDF INT = @@DATEFIRST;
    SET DATEFIRST 1;
    SET @DiaSemana = DATEPART(WEEKDAY, @Fecha);
    SET DATEFIRST @prevDF;

    -------------------------------------------------------------------
    -- ⏰ 1) Validar si la cita cae dentro del horario laboral del médico
    -------------------------------------------------------------------
    IF NOT EXISTS (
        SELECT 1
        FROM dbo.tbHorarioMedico h
        WHERE h.ConsecutivoMedico = @MedicoId
          AND h.DiaSemana = @DiaSemana
          AND @HoraInicio >= h.HoraInicio
          AND @HoraFin    <= h.HoraFin
    )
        SET @HayFueraHorario = 1;

    -------------------------------------------------------------------
    -- ⚠️ 2) Validar si hay solape con otra cita activa del mismo médico
    -------------------------------------------------------------------
    IF @HayFueraHorario = 0
    BEGIN
        SELECT TOP 1 @CitaChoque = C.ConsecutivoCita
        FROM dbo.tbCita C
        WHERE
            C.ConsecutivoMedico = @MedicoId
            AND C.Fecha = @Fecha
            AND C.Estado IN (N'Programada', N'Reprogramada', N'Completada')
            AND (@HoraInicio < C.HoraFin) AND (C.HoraInicio < @HoraFin);

        IF @CitaChoque IS NOT NULL
            SET @HaySolape = 1;
    END

    -------------------------------------------------------------------
    -- 🟢 Devolver resultado (todo como INT)
    -------------------------------------------------------------------
    SELECT 
        CAST(@HayFueraHorario AS INT) AS HayFueraHorario,
        CAST(@HaySolape AS INT)       AS HaySolape,
        @CitaChoque                   AS ConsecutivoCitaChoque;
END;
GO


---------- MODULO DE CITAS -----------------------------

--LISTAR

IF OBJECT_ID('dbo.sp_GetCitas', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetCitas;
GO

CREATE PROCEDURE dbo.sp_GetCitas
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        C.ConsecutivoCita         AS IdCita,
        C.ConsecutivoMedico       AS IdDoctor,
        C.ConsecutivoPaciente     AS IdPaciente,
        UMed.Nombre               AS NombreDoctor,
        UPac.Nombre               AS NombrePaciente,
        DATEADD(SECOND, DATEDIFF(SECOND, 0, C.HoraInicio), CAST(C.Fecha AS DATETIME)) AS FechaHoraInicio,
        DATEADD(SECOND, DATEDIFF(SECOND, 0, C.HoraFin),    CAST(C.Fecha AS DATETIME)) AS FechaHoraFin,
        C.Observaciones           AS Motivo,
        C.Estado
    FROM dbo.tbCita C
    INNER JOIN dbo.tbMedico M      ON M.ConsecutivoMedico      = C.ConsecutivoMedico
    INNER JOIN dbo.tbUsuario UMed  ON UMed.ConsecutivoUsuario  = M.ConsecutivoUsuario
    INNER JOIN dbo.tbUsuario UPac  ON UPac.ConsecutivoUsuario  = C.ConsecutivoPaciente
    ORDER BY C.Fecha DESC, C.HoraInicio ASC;
END;
GO





--INSERT

IF OBJECT_ID('dbo.sp_InsertCita', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_InsertCita;
GO

CREATE PROCEDURE dbo.sp_InsertCita
    @DoctorId INT,
    @PacienteId INT,
    @FechaInicio DATETIME,
    @FechaFin DATETIME,
    @Motivo NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- ============================================
        -- 1️⃣ Validaciones de entrada
        -- ============================================
        IF @FechaFin <= @FechaInicio
        BEGIN
            SELECT 0 AS Codigo, N'La hora de fin debe ser mayor que la de inicio.' AS Mensaje;
            RETURN;
        END;

        -- ============================================
        -- 2️⃣ Validar horario del médico
        -- ============================================
        DECLARE @DiaSemana INT = DATEPART(WEEKDAY, @FechaInicio);

        IF NOT EXISTS (
            SELECT 1
            FROM tbHorarioMedico H
            WHERE H.ConsecutivoMedico = @DoctorId
              AND H.DiaSemana = @DiaSemana
              AND CAST(@FechaInicio AS TIME) BETWEEN H.HoraInicio AND H.HoraFin
              AND CAST(@FechaFin AS TIME) BETWEEN H.HoraInicio AND H.HoraFin
        )
        BEGIN
            SELECT 0 AS Codigo, N'La cita está fuera del horario laboral del médico.' AS Mensaje;
            RETURN;
        END;

        -- ============================================
        -- 3️⃣ Insertar cita
        -- ============================================
        DECLARE @Fecha DATE = CAST(@FechaInicio AS DATE);
        DECLARE @HoraInicio TIME = CAST(@FechaInicio AS TIME);
        DECLARE @HoraFin TIME = CAST(@FechaFin AS TIME);

        INSERT INTO tbCita (
            ConsecutivoMedico,
            ConsecutivoPaciente,
            Fecha,
            HoraInicio,
            HoraFin,
            Observaciones,
            Estado
        )
        VALUES (
            @DoctorId,
            @PacienteId,
            @Fecha,
            @HoraInicio,
            @HoraFin,
            @Motivo,
            N'Programada'
        );

        SELECT 1 AS Codigo, N'Cita registrada correctamente.' AS Mensaje;
    END TRY
    BEGIN CATCH
        SELECT 
            0 AS Codigo,
            N'Error al registrar la cita: ' + ERROR_MESSAGE() AS Mensaje;
    END CATCH
END;
GO


--EDITAR

IF OBJECT_ID('dbo.sp_EditarCita', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_EditarCita;
GO

CREATE PROCEDURE dbo.sp_EditarCita
    @IdCita       INT,
    @DoctorId     INT,
    @PacienteId   INT,
    @Fecha        DATE,
    @HoraInicio   TIME(0),
    @HoraFin      TIME(0),
    @Motivo       NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        UPDATE dbo.tbCita
        SET
            ConsecutivoMedico   = @DoctorId,
            ConsecutivoPaciente = @PacienteId,
            Fecha               = @Fecha,
            HoraInicio          = @HoraInicio,
            HoraFin             = @HoraFin,
            Observaciones       = @Motivo,
            Estado              = N'Reprogramada'
        WHERE ConsecutivoCita = @IdCita;

        SELECT CAST(1 AS INT) AS Codigo, N'Cita actualizada correctamente.' AS Mensaje;
    END TRY
    BEGIN CATCH
        SELECT CAST(0 AS INT) AS Codigo,
               N'Error al actualizar la cita: ' + ERROR_MESSAGE() AS Mensaje;
    END CATCH
END;
GO




--CANCELAR

IF OBJECT_ID('dbo.sp_CancelarCita', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_CancelarCita;
GO

CREATE PROCEDURE dbo.sp_CancelarCita
    @IdCita INT,
    @Motivo NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        UPDATE dbo.tbCita
        SET Estado = N'Cancelada',
            Observaciones = ISNULL(@Motivo, N'Cancelada por el usuario.')
        WHERE ConsecutivoCita = @IdCita;

        SELECT CAST(1 AS INT) AS Codigo, N'Cita cancelada correctamente.' AS Mensaje;
    END TRY
    BEGIN CATCH
        SELECT CAST(0 AS INT) AS Codigo,
               N'Error al cancelar la cita: ' + ERROR_MESSAGE() AS Mensaje;
    END CATCH
END;
GO

--DOC MARCAR

IF OBJECT_ID('dbo.sp_MarcarCitaAtendida', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_MarcarCitaAtendida;
GO

CREATE PROCEDURE dbo.sp_MarcarCitaAtendida
    @IdCita INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM tbCita WHERE ConsecutivoCita = @IdCita)
        BEGIN
            SELECT 0 AS Codigo, N'La cita especificada no existe.' AS Mensaje;
            RETURN;
        END;

        IF EXISTS (SELECT 1 FROM tbCita WHERE ConsecutivoCita = @IdCita AND Estado = N'Cancelada')
        BEGIN
            SELECT 0 AS Codigo, N'No se puede marcar como atendida una cita cancelada.' AS Mensaje;
            RETURN;
        END;

        UPDATE tbCita
        SET Estado = N'Atendida'
        WHERE ConsecutivoCita = @IdCita;

        SELECT 1 AS Codigo, N'Cita marcada como atendida correctamente.' AS Mensaje;
    END TRY
    BEGIN CATCH
        SELECT 0 AS Codigo, N'Error al actualizar cita: ' + ERROR_MESSAGE() AS Mensaje;
    END CATCH
END

--CITAS DOCTOR

IF OBJECT_ID('dbo.sp_GetCitasPorDoctor', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetCitasPorDoctor;
GO

CREATE PROCEDURE dbo.sp_GetCitasPorDoctor
    @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        C.ConsecutivoCita AS IdCita,
        UPac.Nombre AS NombrePaciente,
        UMed.Nombre AS NombreDoctor,
        CAST(C.Fecha AS DATETIME) + CAST(C.HoraInicio AS DATETIME) AS FechaHoraInicio,
        CAST(C.Fecha AS DATETIME) + CAST(C.HoraFin AS DATETIME) AS FechaHoraFin,
        C.Observaciones AS Motivo,
        C.Estado,
        C.ConsecutivoMedico AS IdDoctor,
        C.ConsecutivoPaciente AS IdPaciente
    FROM tbCita C
        INNER JOIN tbMedico M ON M.ConsecutivoMedico = C.ConsecutivoMedico
        INNER JOIN tbUsuario UMed ON UMed.ConsecutivoUsuario = M.ConsecutivoUsuario
        INNER JOIN tbUsuario UPac ON UPac.ConsecutivoUsuario = C.ConsecutivoPaciente
    WHERE M.ConsecutivoUsuario = @IdUsuario
      AND C.Estado <> 'Cancelada'
    ORDER BY C.Fecha DESC, C.HoraInicio ASC;
END;
GO


