using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SimuLAN.Clases.Optimizacion
{
    public class InfoTramoParaOptimizacion
    {
        private int _id_tramo;
        private int _numero_conexiones_pre;
        private int _numero_conexiones_post;
        private ExplicacionImpuntualidad _explicacion_impuntualidad_base;
        private ExplicacionImpuntualidad _explicacion_impuntualidad_actual;
        private ExplicacionImpuntualidad _explicacion_impuntualidad_previa;
        private int _variacion_menos_maxima_inicial;
        private int _variacion_mas_maxima_inicial;
        private int _variacion_menos_aplicada;
        private int _variacion_mas_aplicada;
        private bool _tramo_abierto;
        private InfoTramoParaOptimizacion _tramo_previo;
        private InfoTramoParaOptimizacion _tramos_siguiente;
        public int IdTramo
        {
            get { return _id_tramo; }
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

        public double ComparadorPrioridadOptimizacionRazonReaccionarios
        {
            get
            {
                int aux = Math.Max(1, this.NumeroConexionesPre);
                return this.ExplicacionImpuntualidadActual.RazonReaccionarios * this.NumeroConexionesPost / aux; 
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
                return _tramo_abierto && (VariacionMenosMaxima + VariacionMasMaxima) > 0;
            }
        }

        private InfoTramoParaOptimizacion TramoPrevio
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

        private InfoTramoParaOptimizacion TramoSiguiente
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

        public int VariacionMenosMaxima
        {
            get 
            {
                int variacion_extra = 0;
                if (_tramo_previo != null)
                {
                    if (_tramo_previo.VariacionMasAplicada > 0)
                    {
                        variacion_extra = -_tramo_previo.VariacionMasAplicada;
                    }
                    else if (_tramo_previo.VariacionMenosAplicada > 0)
                    {
                        variacion_extra = _tramo_previo.VariacionMenosAplicada;
                    }
                }
                return _variacion_menos_maxima_inicial + variacion_extra; 
            
            }
        }

        public int VariacionMasMaxima
        {
            get 
            {
                int variacion_extra = 0;
                if (_tramos_siguiente != null)
                {
                    if (_tramos_siguiente.VariacionMasAplicada > 0)
                    {
                        variacion_extra = _tramos_siguiente.VariacionMasAplicada;
                    }
                    else if (_tramos_siguiente.VariacionMenosAplicada > 0)
                    {
                        variacion_extra = -_tramos_siguiente.VariacionMenosAplicada;
                    }
                }

                return _variacion_mas_maxima_inicial + variacion_extra;
            }
        }

        public int VariacionMenosAplicada
        {
            get { return _variacion_menos_aplicada; }
            set { _variacion_menos_aplicada = value; }
        }

        public int VariacionMasAplicada
        {
            get { return _variacion_mas_aplicada; }
            set { _variacion_mas_aplicada = value; }
        }

        public int VariacionAplicadaResultante
        {
            get
            {
                if (_variacion_menos_aplicada > 0)
                {
                    return -_variacion_menos_aplicada;
                }
                else if (_variacion_mas_aplicada > 0)
                {
                    return _variacion_mas_aplicada;
                }
                else
                {
                    return 0;
                }
            }
           
        }

        public bool TramoOptimizable
        {
            get { return _variacion_menos_maxima_inicial + _variacion_mas_maxima_inicial > 0 && _explicacion_impuntualidad_base.ImpuntualidadTotal > 0; }
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
            this._id_tramo = tramo.TramoBase.Numero_Global;
            this._explicacion_impuntualidad_base = null;
            this._variacion_menos_maxima_inicial = tramo.MinutosMaximaVariacionDelante;
            this._variacion_mas_maxima_inicial = tramo.MinutosMaximaVariacionAtras;
            this._variacion_menos_aplicada = 0;
            this._variacion_mas_aplicada = 0;
            this._numero_conexiones_post = tramo.NumConexionesPost;
            this._numero_conexiones_pre = tramo.NumConexionesPre;
            this._tramo_abierto = true;      
            this._tramo_previo = tramo_previo;
            if(tramo_previo!=null)
            {
                tramo_previo.TramoSiguiente = this;
            }
        }

        internal void IncrementarVariacionMasAplicada(int _salto_variaciones)
        {
            int aux_variacion = _variacion_mas_aplicada;
            if (_variacion_menos_aplicada > 0)
            {
                IncrementarVariacionMenosAplicada(_salto_variaciones);
                return;
            }
            aux_variacion += _salto_variaciones;
            if (aux_variacion > VariacionMasMaxima)
            {
                _variacion_mas_aplicada = VariacionMasMaxima;
            }
            else if (aux_variacion > 0)
            {
                _variacion_mas_aplicada =aux_variacion;
            }
            else
            {
                _variacion_mas_aplicada = 0;
            }
        }

        internal void IncrementarVariacionMenosAplicada(int _salto_variaciones)
        {
            int aux_variacion = _variacion_menos_aplicada;
            if (_variacion_mas_aplicada > 0)
            {
                IncrementarVariacionMasAplicada(_salto_variaciones);
                return;
            }
            aux_variacion += _salto_variaciones;
            if (aux_variacion > VariacionMenosMaxima)
            {
                _variacion_menos_aplicada = VariacionMenosMaxima;
            }
            else if (aux_variacion > 0)
            {
                _variacion_menos_aplicada = aux_variacion;
            }
            else
            {
                _variacion_menos_aplicada = 0;

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
                        bool mejora_cr_previa = _explicacion_impuntualidad_previa.ImpuntualidadReaccionarios > _explicacion_impuntualidad_actual.ImpuntualidadReaccionarios;
                        bool mejora_cr_base = _explicacion_impuntualidad_previa.ImpuntualidadReaccionarios > _explicacion_impuntualidad_actual.ImpuntualidadReaccionarios;
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
    }
}
