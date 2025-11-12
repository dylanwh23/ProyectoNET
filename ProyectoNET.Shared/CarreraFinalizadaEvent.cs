using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoNET.Shared
{
    public class CarreraFinalizadaEvent
    {
        public int IdCarrera { get; set; }
        public DateTime? FechaFin { get; set; }
        public int TotalCorredores { get; set; }
        public int CorredoresFinalizados { get; set; }
    }
}
