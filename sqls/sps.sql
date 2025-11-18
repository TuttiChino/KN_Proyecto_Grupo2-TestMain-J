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
