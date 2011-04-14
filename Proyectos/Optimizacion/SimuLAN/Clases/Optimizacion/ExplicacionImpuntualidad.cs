using System;
using System.Collections.Generic;
using System.Text;

namespace SimuLAN.Clases.Optimizacion
{
    public class ExplicacionImpuntualidad
    {
        #region Atributos

        /// <summary>
        /// Atraso promedio por disrupción
        /// </summary>
        Dictionary<TipoDisrupcion, double> _atraso_por_disrupcion;
        /// <summary>
        /// Impuntualidad promedio por disrupción
        /// </summary>
        Dictionary<TipoDisrupcion, double> _impuntualidad_por_disrupcion;

        #endregion

        #region Propiedades

        public double AtrasoProcesosTurnAround
        {
            get
            {
                double atraso = 0;
                List<TipoDisrupcion> dirupciones = new List<TipoDisrupcion>();
                dirupciones.Add(TipoDisrupcion.RECURSOS_DEL_APTO);
                dirupciones.Add(TipoDisrupcion.TA_BAJO_ALA);
                dirupciones.Add(TipoDisrupcion.TA_SOBRE_ALA);
                foreach (TipoDisrupcion t in dirupciones)
                {
                    atraso += ObtenerAtraso(t);
                }
                return atraso;
            }
        }
        public double AtrasoReaccionarios
        {
            get
            {
                double atraso = 0;
                List<TipoDisrupcion> reaccionarios = new List<TipoDisrupcion>();
                reaccionarios.Add(TipoDisrupcion.RC);
                reaccionarios.Add(TipoDisrupcion.RC_PAX);
                reaccionarios.Add(TipoDisrupcion.RC_TRIP);
                reaccionarios.Add(TipoDisrupcion.HBT);
                foreach (TipoDisrupcion t in reaccionarios)
                {
                    atraso += ObtenerAtraso(t);
                }
                return atraso;
            }
        }
        public double AtrasoSinReaccionarios
        {
            get
            {
                return AtrasoTotal - AtrasoReaccionarios;
            }
        }
        public double AtrasoTotal
        {

            get
            {
                double atraso = 0;
                foreach (double valor in _atraso_por_disrupcion.Values)
                {
                    atraso += valor;
                }
                return atraso;               
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
        public double RazonAtrasoReaccionarios
        {
            get
            {
                if (AtrasoTotal > 0)
                {
                    return AtrasoReaccionarios / AtrasoTotal;
                }
                else
                {
                    return 0;
                }
            }
        }
        public double RazonImpuntualidadReaccionarios
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

        #endregion

        #region Constructor

        public ExplicacionImpuntualidad()
        {
            this._impuntualidad_por_disrupcion = new Dictionary<TipoDisrupcion, double>();
            this._atraso_por_disrupcion = new Dictionary<TipoDisrupcion, double>();
        }

        #endregion

        #region Métodos

        internal void AgregarDisrupcion(TipoDisrupcion tipo, double impuntualidad, double atraso)
        {
            if (impuntualidad > 0)
            {
                this._impuntualidad_por_disrupcion.Add(tipo, impuntualidad);
                this._atraso_por_disrupcion.Add(tipo, atraso);
            }
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
        public string InfoParaReporte()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ImpuntualidadTotal);
            sb.Append("\t" + ImpuntualidadReaccionarios);
            sb.Append("\t" + ImpuntualidadSinReaccionarios);
            sb.Append("\t" + RazonImpuntualidadReaccionarios);
            sb.Append("\t" + CausasAtrasoSeparadasPor(", "));
            return sb.ToString();
        }
        private double ObtenerAtraso(TipoDisrupcion tipo)
        {
            if (_atraso_por_disrupcion != null && _atraso_por_disrupcion.ContainsKey(tipo))
            {
                return _atraso_por_disrupcion[tipo];
            }
            else
            {
                return 0;
            }
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

        #endregion
    }
}
