using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitacomm
{

    public class EPC
    {
        public string epc;
        public int IdEPC;
       // public int IdRegistro;
        public int Estado = 0;
        public int Ubicacion =0;
        public bool ActualizaEstado = false;
        public bool InsertaEvento = false;
    }
    public class Sala
    {
        public int IdSala;
        public int asistencia;
    }

}
