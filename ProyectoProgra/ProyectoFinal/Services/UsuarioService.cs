using ProyectoFinal.EF;
using ProyectoFinal.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace ProyectoFinal.Services
{
    public class UsuarioService
    {
        private readonly BDCitasMedicasEntities _context = new BDCitasMedicasEntities();

        // Ajusta este valor al ConsecutivoPerfil que uses para MÉDICO en tbUsuario
        private const int PERFIL_MEDICO = 2; // <-- cámbialo si tu valor es otro

        // ==========================================
        // Listar usuarios por perfil (genérico)
        // ==========================================
        public List<UsuarioDto> ListarUsuariosPorPerfil(int consecutivoPerfil)
        {
            using (var db = new BDCitasMedicasEntities())
            {
                var result = db.Database.SqlQuery<UsuarioDto>(
                    "EXEC dbo.sp_GetUsuariosPorPerfil @ConsecutivoPerfil",
                    new SqlParameter("@ConsecutivoPerfil", consecutivoPerfil)
                ).ToList();

                return result;
            }
        }

        // ==========================================
        // Listar MÉDICOS usando el perfil médico
        // (esto ya es opcional si ahora usas tbMedico)
        // ==========================================
        public List<UsuarioDto> ListarMedicos()
        {
            return ListarUsuariosPorPerfil(PERFIL_MEDICO);
        }

        // ==========================================
        // Obtener usuario por ID
        // ==========================================
        public UsuarioDto ObtenerUsuario(int consecutivoUsuario)
        {
            using (var db = new BDCitasMedicasEntities())
            {
                var result = db.Database.SqlQuery<UsuarioDto>(
                    "EXEC dbo.sp_GetUsuarioPorId @ConsecutivoUsuario",
                    new SqlParameter("@ConsecutivoUsuario", consecutivoUsuario)
                ).FirstOrDefault();

                return result;
            }
        }

        // ==========================================
        // Crear usuario (médico, paciente o admin)
        // ==========================================
        public ResultadoSP CrearUsuario(UsuarioDto usuario)
        {
            using (var db = new BDCitasMedicasEntities())
            {
                var result = db.Database.SqlQuery<ResultadoSP>(
                    "EXEC dbo.sp_InsertUsuario @Identificacion, @Nombre, @Correo, @Contrasenna, @Estado, @ConsecutivoPerfil",
                    new SqlParameter("@Identificacion", usuario.Identificacion),
                    new SqlParameter("@Nombre", usuario.Nombre),
                    new SqlParameter("@Correo", usuario.CorreoElectronico),
                    new SqlParameter("@Contrasenna", usuario.Contrasenna),
                    new SqlParameter("@Estado", usuario.Estado),
                    new SqlParameter("@ConsecutivoPerfil", usuario.ConsecutivoPerfil)
                ).FirstOrDefault();

                return result ?? new ResultadoSP
                {
                    Codigo = 0,
                    Mensaje = "No se obtuvo respuesta del procedimiento.",
                    ConsecutivoUsuario = 0
                };
            }
        }

        // ==========================================
        // Actualizar usuario
        // ==========================================
        public ResultadoSP ActualizarUsuario(UsuarioDto usuario)
        {
            using (var db = new BDCitasMedicasEntities())
            {
                var result = db.Database.SqlQuery<ResultadoSP>(
                    "EXEC dbo.sp_UpdateUsuario @ConsecutivoUsuario, @Identificacion, @Nombre, @Correo, @Estado, @ConsecutivoPerfil",
                    new SqlParameter("@ConsecutivoUsuario", usuario.ConsecutivoUsuario),
                    new SqlParameter("@Identificacion", usuario.Identificacion),
                    new SqlParameter("@Nombre", usuario.Nombre),
                    new SqlParameter("@Correo", usuario.CorreoElectronico),
                    new SqlParameter("@Estado", usuario.Estado),
                    new SqlParameter("@ConsecutivoPerfil", usuario.ConsecutivoPerfil)
                ).FirstOrDefault();

                return result ?? new ResultadoSP
                {
                    Codigo = 0,
                    Mensaje = "No se obtuvo respuesta del procedimiento.",
                    ConsecutivoUsuario = 0
                };
            }
        }

        // ==========================================
        // Eliminar / Desactivar usuario
        // ==========================================
        public ResultadoSP EliminarUsuario(int consecutivoUsuario)
        {
            using (var db = new BDCitasMedicasEntities())
            {
                var result = db.Database.SqlQuery<ResultadoSP>(
                    "EXEC dbo.sp_DeleteUsuario @ConsecutivoUsuario",
                    new SqlParameter("@ConsecutivoUsuario", consecutivoUsuario)
                ).FirstOrDefault();

                return result ?? new ResultadoSP
                {
                    Codigo = 0,
                    Mensaje = "No se obtuvo respuesta del procedimiento.",
                    ConsecutivoUsuario = 0
                };
            }
        }
    }
}
