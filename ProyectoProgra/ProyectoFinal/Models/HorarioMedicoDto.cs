using System;

namespace ProyectoFinal.Models
{
    public class HorarioMedicoDto
    {
        public int ConsecutivoHorario { get; set; }
        public int ConsecutivoMedico { get; set; }
        public byte DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
    }
}
