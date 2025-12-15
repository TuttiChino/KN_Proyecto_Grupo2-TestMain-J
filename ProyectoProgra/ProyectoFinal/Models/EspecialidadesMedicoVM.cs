using System.Collections.Generic;

namespace ProyectoFinal.Models
{
    public class EspecialidadesMedicoVM
    {
        public List<MedicoDto> Medicos { get; set; }
        public int? ConsecutivoMedicoSeleccionado { get; set; }

        public List<EspecialidadDto> EspecialidadesDisponibles { get; set; }
        public List<EspecialidadesMedicoDto> EspecialidadesAsignadas { get; set; }
    }


}
