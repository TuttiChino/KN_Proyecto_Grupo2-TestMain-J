using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProyectoFinal.Models
{
    public class ValidarSolapeCitaResult
    {
        public bool HayFueraHorario { get; set; }
        public bool HaySolape { get; set; }
        public int? ConsecutivoCitaChoque { get; set; }
    }
}
