using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SimuLAN.Clases.Optimizacion
{
    public class InfoTramoParaOptimizacion
    {
        private int _numero_conexiones_pre;
        private int _numero_conexiones_post;
        private ExplicacionImpuntualidad _explicacion_impuntualidad_base;
        private ExplicacionImpuntualidad _explicacion_impuntualidad_actual;
        private ExplicacionImpuntualidad _explicacion_impuntualidad_previa;
        private int _variacion_menos_maxima_comercial;
        private int _variacion_mas_maxima_comercial;
        private int _variacion_aplicada;
        private bool _tramo_abierto;
        private InfoTramoParaOptimizacion _tramo_previo;
        private InfoTramoParaOptimizacion _tramos_siguiente;
        private Tramo _tramo_original;

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

        public int NumeroConexionesPre
        {
            get { return _numero_conexiones_pre; }
            set { _numero_conexiones_pre = value; }
        }

        public int NumeroConexionesPost
        {
            get { return _numero_conexiones_post; }
            set { _numero_conexiones_post = value; }
        }

        public double ComparadorPrioridadOptimizacion
        {
            get
            {
                int aux1 = Math.Max(1, this.NumeroConexionesPost);
                int aux2 = Math.Max(1, this.NumeroConexionesPre);
                return this.ExplicacionImpuntualidadActual.AtrasoReaccionarios * aux1 / aux2; 
            }
        }

        public double ComparadorPrioridadOptimizacionReaccionarioNeto
        {
            get 
            {
                int aux = Math.Max(1, this.NumeroConexionesPre);
                return this.ExplicacionImpuntualidadActual.ImpuntualidadReaccionarios * this.NumeroConexionesPost / aux; 
            }
        }

        public bool Optimizable
        {
            get
            {
                return _tramo_abierto && TramoOptimizable;
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
                return _tramos_siguiente;
            }
            set
            {
                _tramos_siguiente = value;
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

        public bool TramoOptimizable
        {
            get { return _variacion_menos_maxima_comercial + _variacion_mas_maxima_comercial > 0 && _explicacion_impuntualidad_base.ImpuntualidadTotal > 0; }
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
                if (_tramos_siguiente != null)
                {
                    return _tramos_siguiente.VariacionAplicada;
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

        public InfoTramoParaOptimizacion(Tramo tramo, InfoTramoParaOptimizacion tramo_previo)
        {
            this._explicacion_impuntualidad_base = null;
            this._variacion_menos_maxima_comercial = 15;
            this._variacion_mas_maxima_comercial = 15;
            this._variacion_aplicada = 0;
            this._numero_conexiones_post = tramo.NumConexionesPost;
            this._numero_conexiones_pre = tramo.NumConexionesPre;
            this._tramo_abierto = true;      
            this._tramo_previo = tramo_previo;
            this._tramo_original = tramo;
            if(tramo_previo!=null)
            {
                tramo_previo.TramoSiguiente = this;
            }
        }

        internal bool ConvieneOptimizar()
        {
            if (_explicacion_impuntualidad_base != null)
            {
                if (_explicacion_impuntualidad_actual != null)
                {
                    if (_explicacion_impuntualidad_previa != null)
                    {
                        bool mejora_cr_previa = _explicacion_impuntualidad_previa.ImpuntualidadReaccionarios >= _explicacion_impuntualidad_actual.ImpuntualidadReaccionarios;
                        bool mejora_cr_base = _explicacion_impuntualidad_previa.ImpuntualidadReaccionarios >= _explicacion_impuntualidad_actual.ImpuntualidadReaccionarios;
                        //bool reaccionariosPredominantes = _explicacion_impuntualidad_actual.RazonReaccionarios > 0.75;
                        return mejora_cr_previa && mejora_cr_base;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
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
    }
}
