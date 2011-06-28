using System;
using System.Collections.Generic;
using System.Text;

namespace SimuLAN.Clases.Optimizacion
{
    public class InfoConexionParaOptimizacion
    {
        public InfoTramoParaOptimizacion _info_tramo;
        public ConexionLegs _conexion;

        public InfoConexionParaOptimizacion(InfoTramoParaOptimizacion info_tramo, ConexionLegs conexion)
        {
            this._info_tramo = info_tramo;
            this._conexion = conexion;
        }
    }
}
