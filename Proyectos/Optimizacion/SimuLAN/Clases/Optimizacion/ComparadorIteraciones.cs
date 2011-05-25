using System;
using System.Collections.Generic;
using System.Text;

namespace SimuLAN.Clases.Optimizacion
{
    public class ComparadorIteraciones
    {
        private ResumenIteracion _iteracion_1;

        private ResumenIteracion _iteracion_2;

        public double DiferenciaAtrasoTotal
        {
            get
            {
                return _iteracion_1.AtrasoTotal - _iteracion_2.AtrasoTotal;
            }
        }

        public double DiferenciaNoReaccioarioTotal
        {
            get
            {
                return _iteracion_1.AtrasoNoReaccionarioTotal - _iteracion_2.AtrasoNoReaccionarioTotal;
            }
        }

        public double DiferenciaReaccionarioTotal
        {
            get
            {
                return _iteracion_1.AtrasoReaccionarioTotal - _iteracion_2.AtrasoReaccionarioTotal;
            }
        }

        public Dictionary<int, double> DiferenciaImpuntualidadTotal
        {
            get
            {
                Dictionary<int, double> diferencias = new Dictionary<int, double>();
                foreach (int std in _iteracion_1.ImpuntualidadTotal.Keys)
                {
                    diferencias.Add(std, _iteracion_1.ImpuntualidadTotal[std] - _iteracion_2.ImpuntualidadTotal[std]);
                }
                return diferencias;
            }
        }

        public Dictionary<int, double> DiferenciaImpuntualidadNoReaccionarios
        {
            get
            {
                Dictionary<int, double> diferencias = new Dictionary<int, double>();
                foreach (int std in _iteracion_1.ImpuntualidadNoReaccionarios.Keys)
                {
                    diferencias.Add(std, _iteracion_1.ImpuntualidadNoReaccionarios[std] - _iteracion_2.ImpuntualidadNoReaccionarios[std]);
                }
                return diferencias;
            }
        }

        public Dictionary<int, double> DiferenciaImpuntualidadReaccionarios
        {
            get
            {
                Dictionary<int, double> diferencias = new Dictionary<int, double>();
                foreach (int std in _iteracion_1.ImpuntualidadReaccionarios.Keys)
                {
                    diferencias.Add(std, _iteracion_1.ImpuntualidadReaccionarios[std] - _iteracion_2.ImpuntualidadReaccionarios[std]);
                }
                return diferencias;
            }
        }

        public ComparadorIteraciones(ResumenIteracion iteracion1, ResumenIteracion iteracion2)
        {
            this._iteracion_1 = iteracion1;
            this._iteracion_2 = iteracion2;

        }

        internal string EscribirResumenVertical(List<int> stds)
        {
            StringBuilder sb = new StringBuilder();
            string tab = "\t";
            
            sb.AppendLine("Ganancia minutos atraso total" + tab + DiferenciaAtrasoTotal.ToString());
            sb.AppendLine("Ganancia minutos atraso reaccionario" + tab + DiferenciaReaccionarioTotal.ToString());
            sb.AppendLine("Diferencia minutos atraso no reaccionario" + tab + DiferenciaNoReaccioarioTotal.ToString());
            foreach (int std in stds)
            {
                sb.AppendLine("Diferencia Impuntualidad total STD" + std + tab + DiferenciaImpuntualidadTotal[std].ToString());
                sb.AppendLine("Diferencia Impuntualidad reaccionarios STD" + std + tab + DiferenciaImpuntualidadReaccionarios[std].ToString());
                sb.AppendLine("Diferencia Impuntualidad no reaccionarios STD" + std + tab + DiferenciaImpuntualidadNoReaccionarios[std].ToString());
            }
            return sb.ToString();
        }
    }
}
