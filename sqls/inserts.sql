USE BDCitasMedicas;
GO

/* 1) Perfiles fijos por nombre */
INSERT INTO dbo.tbPerfil (Nombre)
VALUES (N'Admin'), (N'Doctor'), (N'Paciente');
GO

/* 2) Usuarios de prueba (texto plano por consigna) */
INSERT INTO dbo.tbUsuario (Identificacion, Nombre, CorreoElectronico, Contrasenna, Estado, ConsecutivoPerfil)
SELECT N'ADM-001', N'Administrador',  N'admin@cm.test',    N'Admin123', 1, p.ConsecutivoPerfil
FROM dbo.tbPerfil p WHERE p.Nombre = N'Admin';

INSERT INTO dbo.tbUsuario (Identificacion, Nombre, CorreoElectronico, Contrasenna, Estado, ConsecutivoPerfil)
SELECT N'DOC-001', N'Dr. House',      N'drhouse@cm.test',  N'Doc12345', 1, p.ConsecutivoPerfil
FROM dbo.tbPerfil p WHERE p.Nombre = N'Doctor';

INSERT INTO dbo.tbUsuario (Identificacion, Nombre, CorreoElectronico, Contrasenna, Estado, ConsecutivoPerfil)
SELECT N'PAC-001', N'Juan Paciente',  N'paciente@cm.test', N'Pac12345', 1, p.ConsecutivoPerfil
FROM dbo.tbPerfil p WHERE p.Nombre = N'Paciente';
GO

/* 3) Médico enlazado al usuario (perfil Doctor) — 1:1 */
INSERT INTO dbo.tbMedico (Nombre, Identificacion, Correo, Estado, ConsecutivoUsuario)
SELECT N'Gregory House', N'MED-001', N'drhouse@cm.test', 1, u.ConsecutivoUsuario
FROM dbo.tbUsuario u
JOIN dbo.tbPerfil  p ON p.ConsecutivoPerfil = u.ConsecutivoPerfil
WHERE u.CorreoElectronico = N'drhouse@cm.test' AND p.Nombre = N'Doctor';
GO

/* 4) Especialidades (catálogo amplio, reales) */
INSERT INTO dbo.tbEspecialidad (Nombre, Estado) VALUES
(N'Alergología',1),(N'Anestesiología',1),(N'Angiología',1),(N'Cardiología',1),(N'Cardiología Pediátrica',1),
(N'Cirugía Cardiotorácica',1),(N'Cirugía General',1),(N'Cirugía Maxilofacial',1),(N'Cirugía Pediátrica',1),
(N'Cirugía Plástica',1),(N'Cirugía Vascular',1),(N'Coloproctología',1),(N'Dermatología',1),
(N'Endocrinología',1),(N'Gastroenterología',1),(N'Geriatría',1),(N'Ginecología y Obstetricia',1),
(N'Hematología',1),(N'Hepatología',1),(N'Infectología',1),(N'Medicina del Deporte',1),
(N'Medicina de Emergencias',1),(N'Medicina Familiar',1),(N'Medicina Física y Rehabilitación',1),
(N'Medicina Interna',1),(N'Nefrología',1),(N'Neonatología',1),(N'Neumología',1),(N'Neurocirugía',1),
(N'Neurología',1),(N'Nutriología Clínica',1),(N'Odontología',1),(N'Oncología Médica',1),
(N'Oncología Quirúrgica',1),(N'Oncología Radioterápica',1),(N'Oftalmología',1),(N'Oncohematología',1),
(N'Ortopedia y Traumatología',1),(N'Otorrinolaringología',1),(N'Pediatría',1),(N'Psiquiatría',1),
(N'Radiología e Imagen',1),(N'Reumatología',1),(N'Urología',1),(N'Patología',1),
(N'Medicina del Trabajo',1),(N'Medicina Paliativa',1),(N'Medicina Preventiva',1),
(N'Toxicología',1),(N'Medicina Intensiva (UCI)',1),(N'Microbiología',1),
(N'Genética Médica',1),(N'Inmunología Clínica',1),(N'Medicina del Sueño',1),
(N'Dolor y Cuidados Paliativos',1),(N'Foniatría',1),(N'Proctología',1),(N'Andrología',1);
GO

/* 5) Relación Médico-Especialidad (Dr. House con 2 especialidades reales) */
INSERT INTO dbo.tbMedicoEspecialidad (ConsecutivoMedico, ConsecutivoEspecialidad)
SELECT m.ConsecutivoMedico, e.ConsecutivoEspecialidad
FROM dbo.tbMedico m
JOIN dbo.tbEspecialidad e ON e.Nombre = N'Medicina Interna'
WHERE m.Identificacion = N'MED-001';

INSERT INTO dbo.tbMedicoEspecialidad (ConsecutivoMedico, ConsecutivoEspecialidad)
SELECT m.ConsecutivoMedico, e.ConsecutivoEspecialidad
FROM dbo.tbMedico m
JOIN dbo.tbEspecialidad e ON e.Nombre = N'Cardiología'
WHERE m.Identificacion = N'MED-001';
GO

/* 6) Horarios del médico (L-V, bloques simples) */
DECLARE @MedicoId INT = (SELECT ConsecutivoMedico FROM dbo.tbMedico WHERE Identificacion = N'MED-001');

INSERT INTO dbo.tbHorarioMedico (ConsecutivoMedico, DiaSemana, HoraInicio, HoraFin) VALUES
(@MedicoId, 1, '08:00', '12:00'),  -- Lunes
(@MedicoId, 2, '08:00', '12:00'),  -- Martes
(@MedicoId, 3, '08:00', '12:00'),  -- Miércoles
(@MedicoId, 4, '13:00', '17:00'),  -- Jueves
(@MedicoId, 5, '13:00', '17:00');  -- Viernes
GO
