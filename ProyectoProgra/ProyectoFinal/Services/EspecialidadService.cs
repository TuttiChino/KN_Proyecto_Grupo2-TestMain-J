using ProyectoFinal.EF;
using ProyectoFinal.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace ProyectoFinal.Services
{
    public class EspecialidadService
    {
        public List<EspecialidadDto> Listar()
        {
            using (var db = new BDCitasMedicasEntities())
            {
                return db.Database
                    .SqlQuery<EspecialidadDto>("EXEC dbo.sp_ListarEspecialidades")
                    .ToList();
            }
        }

        public void Crear(string nombre)
        {
            using (var db = new BDCitasMedicasEntities())
            {
                db.Database.ExecuteSqlCommand(
                    "EXEC dbo.sp_CrearEspecialidad @Nombre",
                    new SqlParameter("@Nombre", nombre)
                );
            }
        }

        public void Editar(EspecialidadDto dto)
        {
            using (var db = new BDCitasMedicasEntities())
            {
                db.Database.ExecuteSqlCommand(
                    "EXEC dbo.sp_EditarEspecialidad @ConsecutivoEspecialidad, @Nombre, @Estado",
                    new SqlParameter("@ConsecutivoEspecialidad", dto.ConsecutivoEspecialidad),
                    new SqlParameter("@Nombre", dto.Nombre),
                    new SqlParameter("@Estado", dto.Estado)
                );
            }
        }

        public void Eliminar(int id)
        {
            using (var db = new BDCitasMedicasEntities())
            {
                db.Database.ExecuteSqlCommand(
                    "EXEC dbo.sp_EliminarEspecialidad @ConsecutivoEspecialidad",
                    new SqlParameter("@ConsecutivoEspecialidad", id)
                );
            }
        }

        public List<EspecialidadesMedicoDto> ListarPorMedico(int consecutivoMedico)
        {
            using (var db = new BDCitasMedicasEntities())
            {
                return db.Database.SqlQuery<EspecialidadesMedicoDto>(
                    "EXEC dbo.sp_ListarEspecialidadesPorMedico @ConsecutivoMedico",
                    new SqlParameter("@ConsecutivoMedico", consecutivoMedico)
                ).ToList();
            }
        }

        public void AgregarEspecialidadMedico(MedicoEspecialidadDto dto)
        {
            using (var db = new BDCitasMedicasEntities())
            {
                db.Database.ExecuteSqlCommand(
                    "EXEC dbo.sp_AgregarEspecialidadMedico @ConsecutivoMedico, @ConsecutivoEspecialidad",
                    new SqlParameter("@ConsecutivoMedico", dto.ConsecutivoMedico),
                    new SqlParameter("@ConsecutivoEspecialidad", dto.ConsecutivoEspecialidad)
                );
            }
        }
    }
}
