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
        Dictionary<int, Dictionary<TipoDisrupcion,double>> _impuntualidad_por_disrupcion_std;

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
        public Dictionary<int,double> ImpuntualidadReaccionarios
        {
            get 
            {                
                Dictionary<int, double> impuntualidades_por_std = new Dictionary<int,double>();
                List<TipoDisrupcion> reaccionarios = new List<TipoDisrupcion>();
                reaccionarios.Add(TipoDisrupcion.RC);
                reaccionarios.Add(TipoDisrupcion.RC_PAX);
                reaccionarios.Add(TipoDisrupcion.RC_TRIP);
                reaccionarios.Add(TipoDisrupcion.HBT);
                
                foreach (TipoDisrupcion t in reaccionarios)
                {
                    foreach (int std in _impuntualidad_por_disrupcion_std.Keys)
                    {
                        if (!impuntualidades_por_std.ContainsKey(std))
                        {
                            impuntualidades_por_std.Add(std, 0);
                        }
                        double impuntualidad_tipo = ObtenerImpuntualidad(t, std);
                        impuntualidades_por_std[std] += impuntualidad_tipo;
                    }
                }
                return impuntualidades_por_std;
            }
        }
        public Dictionary<int,double> ImpuntualidadSinReaccionarios
        {
            get
            {
                Dictionary<int, double> impuntualidad_sin_reaccionarios = new Dictionary<int, double>();
                Dictionary<int, double> impuntualidad_total = ImpuntualidadTotal;
                Dictionary<int, double> impuntualidad_reaccionarios = ImpuntualidadReaccionarios;
                foreach (int std in impuntualidad_total.Keys)
                {
                    impuntualidad_sin_reaccionarios.Add(std, impuntualidad_total[std] - impuntualidad_reaccionarios[std]);
                }
                return impuntualidad_sin_reaccionarios;
            }
        }
        public Dictionary<int, double> ImpuntualidadTotal
        {
            get
            {
                Dictionary<int, double> impuntualidad_std = new Dictionary<int, double>();
                foreach (int std in _impuntualidad_por_disrupcion_std.Keys)
                {
                    impuntualidad_std.Add(std, 0);
                    foreach (TipoDisrupcion tipo in _impuntualidad_por_disrupcion_std[std].Keys)
                    {
                        impuntualidad_std[std] += _impuntualidad_por_disrupcion_std[std][tipo];
                    }
                }
                return impuntualidad_std;
            }
        }
        public Dictionary<int, double> PuntualidadTotal
        {
            get
            {
                Dictionary<int, double> puntualidad = new Dictionary<int, double>();
                Dictionary<int, double> impuntualidad = ImpuntualidadTotal;
                foreach (int std in impuntualidad.Keys)
                {
                    impuntualidad.Add(std, 1 - impuntualidad[std]);
                }
                return puntualidad;
            }
        }

        #endregion

        #region Constructor

        public ExplicacionImpuntualidad()
        {
            this._impuntualidad_por_disrupcion_std = new Dictionary<int, Dictionary<TipoDisrupcion, double>>();
            this._atraso_por_disrupcion = new Dictionary<TipoDisrupcion, double>();
        }

        #endregion

        #region Métodos

        internal void AgregarAtraso(TipoDisrupcion tipo, double atraso)
        {
            if (atraso > 0)
            {
                this._atraso_por_disrupcion.Add(tipo, atraso);
            }
        }
        internal void AgregarDisrupcion(int std, Dictionary<TipoDisrupcion,double> impuntualidad)
        {
            foreach (TipoDisrupcion tipo in impuntualidad.Keys)
            {
                if (impuntualidad[tipo] > 0)
                {
                    if (!_impuntualidad_por_disrupcion_std.ContainsKey(std))
                    {
                        this._impuntualidad_por_disrupcion_std.Add(std, new Dictionary<TipoDisrupcion, double>());
                    }
                    this._impuntualidad_por_disrupcion_std[std].Add(tipo, impuntualidad[tipo]);
                }
            }
        }
        private string CausasAtrasoSeparadasPor(string separador)
        {
            StringBuilder sb = new StringBuilder();
            foreach (TipoDisrupcion tipo in _impuntualidad_por_disrupcion_std.Keys)
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
            sb.Append("\t" + AtrasoTotal);
            sb.Append("\t" + AtrasoReaccionarios);
            sb.Append("\t" + AtrasoSinReaccionarios);
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
        public double ObtenerImpuntualidad(TipoDisrupcion tipo,int std)
        {
            if (_impuntualidad_por_disrupcion_std != null 
                && _impuntualidad_por_disrupcion_std.ContainsKey(std)
                && _impuntualidad_por_disrupcion_std[std].ContainsKey(tipo))
            {
                return _impuntualidad_por_disrupcion_std[std][tipo];
            }
            else
            {
                return 0;
            }
        } 

        #endregion
    }
}
