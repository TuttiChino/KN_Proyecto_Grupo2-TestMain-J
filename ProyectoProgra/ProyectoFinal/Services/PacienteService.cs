using ProyectoFinal.EF;
using ProyectoFinal.Models;
using System;
using System.Data.SqlClient;
using System.Linq;

namespace ProyectoFinal.Services
{
    public class PacienteService
    {
        private readonly BDCitasMedicasEntities _context = new BDCitasMedicasEntities();

        // ==========================================
        // Obtener perfil de paciente por ID
        // ==========================================
        public PerfilPacienteDto ObtenerPerfil(int consecutivoUsuario)
        {
            using (var db = new BDCitasMedicasEntities())
            {
                var result = db.Database.SqlQuery<PerfilPacienteDto>(
                    "EXEC dbo.sp_GetUsuarioPorId @ConsecutivoUsuario",
                    new SqlParameter("@ConsecutivoUsuario", consecutivoUsuario)
                ).FirstOrDefault();

                return result;
            }
        }

        // ==========================================
        // Cambiar contraseña de paciente
        // (usa el mismo ResultadoSP que CitaService)
        // ==========================================
        public ResultadoSP CambiarContrasena(int consecutivoUsuario,
                                             string contrasenaActual,
                                             string contrasenaNueva)
        {
            using (var db = new BDCitasMedicasEntities())
            {
                var result = db.Database.SqlQuery<ResultadoSP>(
                    "EXEC dbo.sp_CambiarContrasena @ConsecutivoUsuario, @ContrasenaActual, @ContrasenaNueva",
                    new SqlParameter("@ConsecutivoUsuario", consecutivoUsuario),
                    new SqlParameter("@ContrasenaActual", contrasenaActual),
                    new SqlParameter("@ContrasenaNueva", contrasenaNueva)
                ).FirstOrDefault();

                return result ?? new ResultadoSP
                {
                    Codigo = 0,
                    Mensaje = "No se obtuvo respuesta del procedimiento."
                };
            }
        }
    }
}
