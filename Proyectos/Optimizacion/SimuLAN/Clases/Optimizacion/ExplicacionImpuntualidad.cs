using System;
using System.Collections.Generic;
using System.Text;

namespace SimuLAN.Clases.Optimizacion
{
    public class ExplicacionImpuntualidad
    {
        Dictionary<TipoDisrupcion, double> _impuntualidad_por_disrupcion;

        public double ImpuntualidadTotal
        { 
            get
            {
                double impuntualidad = 0;
                foreach (double valor in _impuntualidad_por_disrupcion.Values)
                {
                    impuntualidad += valor;
                }
                return impuntualidad;
            }
        }

        public double PuntualidadTotal
        {
            get
            {
                return 1 - ImpuntualidadTotal;
            }
        }

        public double ImpuntualidadReaccionarios
        {
            get 
            {
                double impuntualidad = 0;
                List<TipoDisrupcion> reaccionarios = new List<TipoDisrupcion>();
                reaccionarios.Add(TipoDisrupcion.RC);
                reaccionarios.Add(TipoDisrupcion.RC_PAX);
                reaccionarios.Add(TipoDisrupcion.RC_TRIP);
                reaccionarios.Add(TipoDisrupcion.HBT);
                foreach (TipoDisrupcion t in reaccionarios)
                {
                    impuntualidad += ObtenerImpuntualidad(t);
                }
                return impuntualidad;
            }
        }

        public double ImpuntualidadSinReaccionarios
        {
            get
            {
                return ImpuntualidadTotal - ImpuntualidadReaccionarios;
            }
        }

        public double ImpuntualidadProcesosTurnAround
        {
            get
            {
                double impuntualidad = 0;
                List<TipoDisrupcion> dirupciones = new List<TipoDisrupcion>();
                dirupciones.Add(TipoDisrupcion.RECURSOS_DEL_APTO);
                dirupciones.Add(TipoDisrupcion.TA_BAJO_ALA);
                dirupciones.Add(TipoDisrupcion.TA_SOBRE_ALA);
                foreach (TipoDisrupcion t in dirupciones)
                {
                    impuntualidad += ObtenerImpuntualidad(t);
                }
                return impuntualidad; 
            }
        }

        public double RazonReaccionarios
        {
            get
            {
                if (ImpuntualidadTotal > 0)
                {
                    return ImpuntualidadReaccionarios / ImpuntualidadTotal;
                }
                else
                {
                    return 0;
                }
            }
        }

        public bool TieneAtrasoReaccionario
        {
            get
            {
                return _impuntualidad_por_disrupcion.ContainsKey(TipoDisrupcion.RC)
                    || _impuntualidad_por_disrupcion.ContainsKey(TipoDisrupcion.RC_PAX)
                    || _impuntualidad_por_disrupcion.ContainsKey(TipoDisrupcion.RC_TRIP)
                    || _impuntualidad_por_disrupcion.ContainsKey(TipoDisrupcion.HBT);
            }
        }

        public bool TieneAtrasos
        {
            get
            {
                return _impuntualidad_por_disrupcion.Count > 0;
            }
        }

        public ExplicacionImpuntualidad()
        {
            this._impuntualidad_por_disrupcion = new Dictionary<TipoDisrupcion, double>();
        }

        public double ObtenerImpuntualidad(TipoDisrupcion tipo)
        {
            if (_impuntualidad_por_disrupcion != null && _impuntualidad_por_disrupcion.ContainsKey(tipo))
            {
                return _impuntualidad_por_disrupcion[tipo];
            }
            else
            {
                return 0;
            }
        }

        internal void AgregarDisrupcion(TipoDisrupcion tipo, double impuntualidad)
        {
            if (impuntualidad > 0)
            {
                this._impuntualidad_por_disrupcion.Add(tipo, impuntualidad);
            }
        }

        public string InfoParaReporte()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ImpuntualidadTotal);
            sb.Append("\t" + ImpuntualidadReaccionarios);
            sb.Append("\t" + ImpuntualidadSinReaccionarios);
            sb.Append("\t" + RazonReaccionarios);
            sb.Append("\t" + CausasAtrasoSeparadasPor(", "));
            return sb.ToString();
        }

        private string CausasAtrasoSeparadasPor(string separador)
        {
            StringBuilder sb = new StringBuilder();
            foreach (TipoDisrupcion tipo in _impuntualidad_por_disrupcion.Keys)
            {
                sb.Append(tipo.ToString() + separador);
            }
            return sb.ToString();
        }
    }
}
