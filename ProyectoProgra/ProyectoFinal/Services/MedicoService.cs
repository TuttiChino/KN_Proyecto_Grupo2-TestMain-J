using ProyectoFinal.EF;
using ProyectoFinal.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace ProyectoFinal.Services
{
    public class MedicoService
    {
        private readonly BDCitasMedicasEntities _context = new BDCitasMedicasEntities();

        // ================================
        // Listar todos los médicos
        // SP: sp_GetMedicos
        // ================================
        public List<MedicoDto> ListarMedicos()
        {
            using (var db = new BDCitasMedicasEntities())
            {
                var result = db.Database.SqlQuery<MedicoDto>(
                    "EXEC dbo.sp_GetMedicos"
                ).ToList();

                return result;
            }
        }

        // ================================
        // Obtener un médico por ID
        // SP: sp_GetMedicoPorId
        // ================================
        public MedicoDto ObtenerMedico(int consecutivoMedico)
        {
            using (var db = new BDCitasMedicasEntities())
            {
                var result = db.Database.SqlQuery<MedicoDto>(
                    "EXEC dbo.sp_GetMedicoPorId @ConsecutivoMedico",
                    new SqlParameter("@ConsecutivoMedico", consecutivoMedico)
                ).FirstOrDefault();

                return result;
            }
        }

        // ================================
        // Crear médico (DESPUÉS de crear usuario)
        // SP: sp_InsertMedico
        // ================================
        public ResultadoSP CrearMedico(MedicoDto medico)
        {
            using (var db = new BDCitasMedicasEntities())
            {
                var res = db.Database.SqlQuery<ResultadoSP>(
                    "EXEC dbo.sp_InsertMedico @ConsecutivoUsuario, @Nombre, @Identificacion, @Correo, @Estado",
                    new SqlParameter("@ConsecutivoUsuario", medico.ConsecutivoUsuario),
                    new SqlParameter("@Nombre", medico.Nombre),
                    new SqlParameter("@Identificacion", medico.Identificacion),
                    new SqlParameter("@Correo", medico.CorreoElectronico),
                    new SqlParameter("@Estado", medico.Estado)
                ).FirstOrDefault();

                return res ?? new ResultadoSP
                {
                    Codigo = 0,
                    Mensaje = "No se obtuvo respuesta del procedimiento.",
                    ConsecutivoUsuario = 0
                };
            }
        }

        // ================================
        // Actualizar médico
        // SP: sp_UpdateMedico
        // ================================
        public ResultadoSP ActualizarMedico(MedicoDto medico)
        {
            using (var db = new BDCitasMedicasEntities())
            {
                var res = db.Database.SqlQuery<ResultadoSP>(
                    "EXEC dbo.sp_UpdateMedico @ConsecutivoMedico, @Nombre, @Identificacion, @Correo, @Estado",
                    new SqlParameter("@ConsecutivoMedico", medico.ConsecutivoMedico),
                    new SqlParameter("@Nombre", medico.Nombre),
                    new SqlParameter("@Identificacion", medico.Identificacion),
                    new SqlParameter("@Correo", medico.CorreoElectronico),
                    new SqlParameter("@Estado", medico.Estado)
                ).FirstOrDefault();

                return res ?? new ResultadoSP
                {
                    Codigo = 0,
                    Mensaje = "No se obtuvo respuesta del procedimiento.",
                    ConsecutivoUsuario = 0
                };
            }
        }

        // ================================
        // Eliminar / desactivar médico
        // SP: sp_DeleteMedico
        // ================================
        public ResultadoSP EliminarMedico(int consecutivoMedico)
        {
            using (var db = new BDCitasMedicasEntities())
            {
                var res = db.Database.SqlQuery<ResultadoSP>(
                    "EXEC dbo.sp_DeleteMedico @ConsecutivoMedico",
                    new SqlParameter("@ConsecutivoMedico", consecutivoMedico)
                ).FirstOrDefault();

                return res ?? new ResultadoSP
                {
                    Codigo = 0,
                    Mensaje = "No se obtuvo respuesta del procedimiento.",
                    ConsecutivoUsuario = 0
                };
            }
        }

        public List<HorarioMedicoDto> ListarHorariosMedico(int consecutivoMedico)
        {
            using (var db = new BDCitasMedicasEntities())
            {
                var result = db.Database.SqlQuery<HorarioMedicoDto>(
                    "EXEC dbo.sp_GetHorariosPorMedico @ConsecutivoMedico",
                    new SqlParameter("@ConsecutivoMedico", consecutivoMedico)
                ).ToList();

                return result;
            }
        }

        // Insertar horario (usando sp_InsertHorarioMedico)
        public void InsertarHorarioMedico(HorarioMedicoDto horario)
        {
            using (var db = new BDCitasMedicasEntities())
            {
                db.Database.ExecuteSqlCommand(
                    "EXEC dbo.sp_InsertHorarioMedico @ConsecutivoMedico, @DiaSemana, @HoraInicio, @HoraFin",
                    new SqlParameter("@ConsecutivoMedico", horario.ConsecutivoMedico),
                    new SqlParameter("@DiaSemana", horario.DiaSemana),
                    new SqlParameter("@HoraInicio", horario.HoraInicio),
                    new SqlParameter("@HoraFin", horario.HoraFin)
                );
            }
        }
    }
}
