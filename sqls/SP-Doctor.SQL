ALTER PROCEDURE dbo.sp_ValidarSolapeCita
    @MedicoId    INT,
    @Fecha       DATE,
    @HoraInicio  TIME(0),
    @HoraFin     TIME(0),
    @CitaActualId INT = NULL  -- nuevo parámetro opcional
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @HaySolape BIT = 0;
    DECLARE @CitaChoque INT = NULL;
    DECLARE @HayFueraHorario BIT = 0;

    DECLARE @DiaSemana TINYINT;
    DECLARE @prevDF INT = @@DATEFIRST;
    SET DATEFIRST 1;
    SET @DiaSemana = DATEPART(WEEKDAY, @Fecha);
    SET DATEFIRST @prevDF;

    -- 1) Validar que la cita esté dentro del horario del médico
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

    -- 2) Validar solape, ignorando la cita actual si se edita
    IF @HayFueraHorario = 0
    BEGIN
        SELECT TOP 1 @CitaChoque = C.ConsecutivoCita
        FROM dbo.tbCita C
        WHERE
            C.ConsecutivoMedico = @MedicoId
            AND C.Fecha = @Fecha
            AND C.Estado <> N'Cancelada'
            AND (@HoraInicio < C.HoraFin) AND (C.HoraInicio < @HoraFin)
            AND (@CitaActualId IS NULL OR C.ConsecutivoCita <> @CitaActualId);

        IF @CitaChoque IS NOT NULL
            SET @HaySolape = 1;
    END

    SELECT 
        @HayFueraHorario AS HayFueraHorario,
        @HaySolape       AS HaySolape,
        @CitaChoque      AS ConsecutivoCitaChoque;
END
GO
