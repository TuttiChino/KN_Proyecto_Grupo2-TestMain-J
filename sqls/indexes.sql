USE BDCitasMedicas;
GO

/* Mis Citas (Paciente) por fecha */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = N'IX_tbCita_Paciente_Fecha' AND object_id = OBJECT_ID(N'dbo.tbCita')
)
BEGIN
    CREATE INDEX IX_tbCita_Paciente_Fecha
      ON dbo.tbCita (ConsecutivoPaciente, Fecha);
END
GO

/* Listar médicos por especialidad */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = N'IX_tbMedicoEspecialidad_Especialidad' AND object_id = OBJECT_ID(N'dbo.tbMedicoEspecialidad')
)
BEGIN
    CREATE INDEX IX_tbMedicoEspecialidad_Especialidad
      ON dbo.tbMedicoEspecialidad (ConsecutivoEspecialidad);
END
GO

/* Filtros por rol/perfil en usuarios */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = N'IX_tbUsuario_Perfil' AND object_id = OBJECT_ID(N'dbo.tbUsuario')
)
BEGIN
    CREATE INDEX IX_tbUsuario_Perfil
      ON dbo.tbUsuario (ConsecutivoPerfil);
END
GO
