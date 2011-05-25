using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SimuLAN.Utils;

namespace SimuLAN.Clases.Optimizacion
{
    public class InfoTramoParaOptimizacion
    {
        #region Atributos

        private ExplicacionImpuntualidad _explicacion_impuntualidad_base;
        private ExplicacionImpuntualidad _explicacion_impuntualidad_actual;
        private ExplicacionImpuntualidad _explicacion_impuntualidad_previa;
        private int _variacion_menos_maxima_comercial;
        private int _variacion_mas_maxima_comercial;
        private int _variacion_aplicada;
        private bool _tramo_abierto;
        private InfoTramoParaOptimizacion _tramo_previo;
        private InfoTramoParaOptimizacion _tramo_siguiente;
        private Tramo _tramo_original;
        private List<InfoTramoParaOptimizacion> _info_tramos_posteriores_conectados;
        private List<InfoTramoParaOptimizacion> _info_tramos_anteriores_conectados;
        private BuscarTramosConectadosEventHandler _buscar_tramos;

        #endregion

        #region Propiedades
        public List<InfoTramoParaOptimizacion> InfoTramosPosterioresConectados
        {
            get
            {
                if (_info_tramos_posteriores_conectados == null)
                {
                    _info_tramos_posteriores_conectados = _buscar_tramos(_tramo_original.TodasLasConexionesPosteriores, true);
                    if (this._tramo_siguiente != null)
                    {
                        _info_tramos_posteriores_conectados.Add(this._tramo_siguiente);
                    }
                }
                return _info_tramos_posteriores_conectados;
            }

        }
        public List<InfoTramoParaOptimizacion> InfoTramosAnterioresConectados
        {
            get
            {
                if (_info_tramos_anteriores_conectados == null)
                {
                    _info_tramos_anteriores_conectados = _buscar_tramos(_tramo_original.TodasLasConexionesAnteriores, false);
                    if (this._tramo_previo != null)
                    {
                        _info_tramos_anteriores_conectados.Add(this._tramo_previo);
                    }
                }
                return _info_tramos_anteriores_conectados;
            }

        }
        public Tramo TramoOriginal
        {
            get { return _tramo_original; }
        }
        public double AtrasoTramoPrevio
        {
            get 
            {
                if (_tramo_previo != null)
                {
                    return _tramo_previo._explicacion_impuntualidad_actual.AtrasoTotal + _tramo_previo.VariacionAplicada;          
                }
                return 0;
            }
        }
        public int IdTramo
        {
            get { return _tramo_original.TramoBase.Numero_Global; }
        }
        public double ComparadorPrioridadOptimizacion
        {
            get
            {
                return this.ExplicacionImpuntualidadActual.AtrasoReaccionarios; 
            }
        }       
        public InfoTramoParaOptimizacion TramoPrevio
        {
           get
           {
                return _tramo_previo;
           }
           set
           {
               _tramo_previo = value;
           }
        }
        public InfoTramoParaOptimizacion TramoSiguiente
        {
            get
            {
                return _tramo_siguiente;
            }
            set
            {
                _tramo_siguiente = value;
            }
        }
        public int VariacionAplicada
        {
            get { return _variacion_aplicada; }
            set { _variacion_aplicada = value; }
        }
        public int VariacionMasMaximaComercial
        {
            get { return _variacion_mas_maxima_comercial; }
            set { _variacion_mas_maxima_comercial = value; }
        }
        public int VariacionMenosMaximaComercial
        {
            get { return _variacion_menos_maxima_comercial; }
            set { _variacion_menos_maxima_comercial = value; }
        }
        public int VariacionAplicadaTramoPrevio
        {
            get
            {
                if (_tramo_previo != null)
                {
                    return _tramo_previo.VariacionAplicada;
                }
                return 0;
            }
        }
        public int VariacionAplicadaTramoSiguiente
        {
            get
            {
                if (_tramo_siguiente != null)
                {
                    return _tramo_siguiente.VariacionAplicada;
                }
                return 0;
            }
        }
        public ExplicacionImpuntualidad ExplicacionImpuntualidadBase
        {
            get { return _explicacion_impuntualidad_base; }
            set { _explicacion_impuntualidad_base = value; }
        }
        public ExplicacionImpuntualidad ExplicacionImpuntualidadActual
        {
            get { return _explicacion_impuntualidad_actual; }
            set { _explicacion_impuntualidad_actual = value; }
        }
        public ExplicacionImpuntualidad ExplicacionImpuntualidadPrevia
        {
            get { return _explicacion_impuntualidad_previa; }
            set { _explicacion_impuntualidad_previa = value; }
        }
        public bool TramoAbierto
        {
            get { return _tramo_abierto; }
            set { _tramo_abierto = value; }
        }
        public Dictionary<int, int> VariacionesPropuestasConexionesAtras
        {
            get
            {
                Dictionary<int, int> variaciones = new Dictionary<int, int>();
                foreach (InfoTramoParaOptimizacion info_tramo in InfoTramosAnterioresConectados)
                {
                    if (!variaciones.ContainsKey(info_tramo.IdTramo))
                    {
                        variaciones.Add(info_tramo.IdTramo, info_tramo.VariacionAplicada);
                    }
                }
                return variaciones;
            }
        }
        public Dictionary<int, int> VariacionesPropuestasConexionesDelante
        {
            get
            {
                Dictionary<int, int> variaciones = new Dictionary<int, int>();
                foreach (InfoTramoParaOptimizacion info_tramo in InfoTramosPosterioresConectados)
                {
                    variaciones.Add(info_tramo.IdTramo, info_tramo.VariacionAplicada);
                }
                return variaciones;
            }
        }
        /// <summary>
        /// Considerando las variaciones propuestas entrega la holgura resultante de todos los tramos que conectan por delante al tramo instanciado
        /// </summary>
        public Dictionary<int, int> HolgurasResultatesDelante
        {
            get
            {
                Dictionary<int, int> holguras_finales = new Dictionary<int, int>();
                int ta_destino = this.TramoOriginal.TurnAroundMinimoDestino;
                Dictionary<int, int> holguras = this.TramoOriginal.HolgurasDelanteParaCadaConexion;
                Dictionary<int, int> variaciones = this.VariacionesPropuestasConexionesDelante;
                foreach (int key_tramo in holguras.Keys)
                {
                    int holgura_tramo = holguras[key_tramo] - ta_destino + variaciones[key_tramo] - this.VariacionAplicada;
                    holguras_finales.Add(key_tramo, holgura_tramo);
                }
                return holguras_finales;
            }
        }
        /// <summary>
        /// Considerando las variaciones propuestas entrega la holgura resultante de todos los tramos que conectan por delante al tramo instanciado
        /// </summary>
        public Dictionary<int, int> HolgurasResultatesAtras
        {
            get
            {
                Dictionary<int, int> holguras_finales = new Dictionary<int, int>();
                int ta_origen = this.TramoOriginal.TurnAroundMinimoOrigen;
                Dictionary<int, int> holguras = this.TramoOriginal.HolgurasAtrasParaCadaConexion;
                Dictionary<int, int> variaciones = this.VariacionesPropuestasConexionesAtras;
                foreach (int key_tramo in holguras.Keys)
                {
                    int holgura_tramo = holguras[key_tramo] - ta_origen - variaciones[key_tramo] + this.VariacionAplicada;
                    holguras_finales.Add(key_tramo, holgura_tramo);
                }
                return holguras_finales;
            }
        }
        #endregion

        #region Constructor

        public InfoTramoParaOptimizacion(Tramo tramo, InfoTramoParaOptimizacion tramo_previo, int variacion_permitida, BuscarTramosConectadosEventHandler buscar_tramos)
        {
            this._explicacion_impuntualidad_base = null;
            this._variacion_menos_maxima_comercial = variacion_permitida;
            this._variacion_mas_maxima_comercial = variacion_permitida;
            this._variacion_aplicada = 0;
            this._tramo_abierto = true;      
            this._tramo_previo = tramo_previo;
            this._tramo_original = tramo;
            this._buscar_tramos = buscar_tramos;
            
            if(tramo_previo!=null)
            {
                tramo_previo.TramoSiguiente = this;
            }
        }

        #endregion

        #region Metodos
        /// <summary>
        /// Considerando las variaciones propuestas entrega la holgura resultante de todos los tramos que conectan por delante al tramo instanciado
        /// </summary>
        private Dictionary<int, int> HolgurasSimuladasAtras(int variacion)
        {
            Dictionary<int, int> holguras_finales = new Dictionary<int, int>();
            int ta_origen = this.TramoOriginal.TurnAroundMinimoOrigen;
            Dictionary<int, int> holguras = this.TramoOriginal.HolgurasAtrasParaCadaConexion;
            Dictionary<int, int> variaciones = this.VariacionesPropuestasConexionesAtras;
            foreach (int key_tramo in holguras.Keys)
            {
                int holgura_tramo = holguras[key_tramo] - ta_origen - variaciones[key_tramo] + variacion;
                holguras_finales.Add(key_tramo, holgura_tramo);
            }
            return holguras_finales;
        }
        /// <summary>
        /// Considerando las variaciones propuestas entrega la holgura resultante de todos los tramos que conectan por delante al tramo instanciado
        /// </summary>
        private Dictionary<int, int> HolgurasSimuladasDelante(int variacion)
        {
            Dictionary<int, int> holguras_finales = new Dictionary<int, int>();
            int ta_destino = this.TramoOriginal.TurnAroundMinimoDestino;
            Dictionary<int, int> holguras = this.TramoOriginal.HolgurasDelanteParaCadaConexion;
            Dictionary<int, int> variaciones = this.VariacionesPropuestasConexionesDelante;
            foreach (int key_tramo in holguras.Keys)
            {
                int holgura_tramo = holguras[key_tramo] - ta_destino + variaciones[key_tramo] - variacion;
                holguras_finales.Add(key_tramo, holgura_tramo);
            }
            return holguras_finales;
        }        
        public bool Optimizable(int std_objetivo)
        {
            return _tramo_abierto && TramoOptimizable(std_objetivo);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="atraso_inicial">Atraso reaccionario inducido sobre el tramo</param>
        /// <param name="variacion_tramo_inicial">Variación evaluada sobre el tramo</param>
        /// <returns></returns>
        internal double EstimarAtrasoArbolPropagacion(double atraso_inicial, int variacion_tramo_inicial, int profundidad_evaluacion)
        {
            double atraso_propagado_arbol = 0;            
            InfoTramoParaOptimizacion actual = this;
            int variacion_original = actual.VariacionAplicada;
            //Reaccionario corredido: considera atraso inicial, máxima holgura posible(considera itinerario programado con sus conexiones)
            double holgura_minima_atras = ObtenerMinimaHolguraAtrasSimulada(variacion_tramo_inicial);
            double reaccionario_efectivo = Math.Max(0, atraso_inicial - holgura_minima_atras);
            double atrasoGenerado = actual.ExplicacionImpuntualidadActual.AtrasoSinReaccionarios;
            double atrasoTotal = reaccionario_efectivo + atrasoGenerado;
            atraso_propagado_arbol += atrasoTotal;
            atraso_inicial = atrasoTotal;
            actual.VariacionAplicada = variacion_tramo_inicial;
            if (profundidad_evaluacion > 0)
            {
                List<InfoTramoParaOptimizacion> tramos_con_conexiones = this.InfoTramosPosterioresConectados;
                foreach (InfoTramoParaOptimizacion tramo in tramos_con_conexiones)
                {
                    double atraso_propagado_rama = tramo.EstimarAtrasoArbolPropagacion(atraso_inicial, tramo.VariacionAplicada, profundidad_evaluacion - 1);
                    atraso_propagado_arbol += atraso_propagado_rama;
                }
            }
            actual.VariacionAplicada = variacion_original;
            return atraso_propagado_arbol;
        }

        private double ObtenerMinimaHolguraAtrasSimulada(int variacion_tramo)
        {
            int minimo = int.MaxValue;
            Dictionary<int, int> holguras_minimas_simuladas = HolgurasSimuladasAtras(variacion_tramo);
            if (holguras_minimas_simuladas != null && holguras_minimas_simuladas.Count > 0)
            {
                foreach (int holgura in holguras_minimas_simuladas.Values)
                {
                    if (holgura < minimo)
                    {
                        minimo = holgura;
                    }
                }
                return minimo;
            }
            else
            {
                return 0;
            }
        }
        //Por ahora se evalúa solo el avión. Después será todo el sistema
        internal double EstimarAtrasoPropagadoAvion(double atraso_previo, int variacion_propuesta)
        {
            double atraso_propagado_global = 0;
            InfoTramoParaOptimizacion actual = this;
            while (actual != null)
            {
                if (actual == this)
                {
                    double reaccionario = Math.Max(0, atraso_previo - actual.TramoOriginal.MinutosMaximaVariacionAtras - (variacion_propuesta - actual.VariacionAplicadaTramoPrevio));
                    double atrasoGenerado = actual.ExplicacionImpuntualidadActual.AtrasoSinReaccionarios;
                    double atrasoTotal = reaccionario + atrasoGenerado;
                    atraso_propagado_global += atrasoTotal;
                    atraso_previo = atrasoTotal;
                }
                else if (actual == this.TramoSiguiente)
                {
                    double reaccionario = Math.Max(0, atraso_previo - actual.TramoOriginal.MinutosMaximaVariacionAtras - (actual.VariacionAplicada - variacion_propuesta));
                    double atrasoGenerado = actual.ExplicacionImpuntualidadActual.AtrasoSinReaccionarios;
                    double atrasoTotal = reaccionario + atrasoGenerado;
                    atraso_propagado_global += atrasoTotal;
                    atraso_previo = atrasoTotal;
                }
                else
                {
                    double reaccionario = Math.Max(0, atraso_previo - actual.TramoOriginal.MinutosMaximaVariacionAtras - (actual.VariacionAplicada - actual.VariacionAplicadaTramoPrevio));
                    double atrasoGenerado = actual.ExplicacionImpuntualidadActual.AtrasoSinReaccionarios;
                    double atrasoTotal = reaccionario + atrasoGenerado;
                    atraso_propagado_global += atrasoTotal;
                    atraso_previo = atrasoTotal;
                }
                actual = actual.TramoSiguiente;
            }

            return atraso_propagado_global;
        }
        public bool TramoOptimizable(int std_objetivo)
        {
            return _variacion_menos_maxima_comercial + _variacion_mas_maxima_comercial > 0
                && _explicacion_impuntualidad_base.ImpuntualidadTotal != null
                && _explicacion_impuntualidad_base.ImpuntualidadTotal.ContainsKey(std_objetivo)
                && _explicacion_impuntualidad_base.ImpuntualidadTotal[std_objetivo] > 0;
        }
        #endregion
    }
}
