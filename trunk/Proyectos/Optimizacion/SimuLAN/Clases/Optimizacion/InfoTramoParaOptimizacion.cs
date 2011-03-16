using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SimuLAN.Clases.Optimizacion
{
    public class InfoTramoParaOptimizacion
    {
        private int _id_tramo;
        private ExplicacionImpuntualidad _explicacion_impuntualidad_base;
        private ExplicacionImpuntualidad _explicacion_impuntualidad_actual;
        private ExplicacionImpuntualidad _explicacion_impuntualidad_previa;
        private int _variacion_menos_maxima;
        private int _variacion_mas_maxima;
        private int _variacion_menos_aplicada;
        private int _variacion_mas_aplicada;

        public int IdTramo
        {
            get { return _id_tramo; }
        }

        public int VariacionMenosMaxima
        {
            get { return _variacion_menos_maxima; }
            set { _variacion_menos_maxima = value; }
        }

        public int VariacionMasMaxima
        {
            get { return _variacion_mas_maxima; }
            set { _variacion_mas_maxima = value; }
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
            get { return _variacion_menos_maxima + _variacion_mas_maxima > 0 && _explicacion_impuntualidad_base.ImpuntualidadTotal > 0; }
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

        public InfoTramoParaOptimizacion(int id, int variacion_menos, int variacion_mas)
        {
            this._id_tramo = id;
            this._explicacion_impuntualidad_base = null;
            this._variacion_menos_maxima = variacion_menos;
            this._variacion_mas_maxima = variacion_mas;
            this._variacion_menos_aplicada = 0;
            this._variacion_mas_aplicada = 0;
        }

        internal void IncrementarVariacionMasAplicada(int _salto_variaciones)
        {
            int aux_variacion = _variacion_mas_aplicada;
            aux_variacion += _salto_variaciones;
            if (aux_variacion < _variacion_mas_maxima)
            {
                _variacion_mas_aplicada = aux_variacion;
            }
            else
            {
                _variacion_mas_aplicada = _variacion_mas_maxima;
            }
        }

        internal void IncrementarVariacionMenosAplicada(int _salto_variaciones)
        {
            int aux_variacion = _variacion_menos_aplicada;
            aux_variacion += _salto_variaciones;
            if (aux_variacion < _variacion_menos_maxima)
            {
                _variacion_menos_aplicada = aux_variacion;
            }
            else
            {
                _variacion_menos_aplicada = _variacion_menos_maxima;
            }
        }

        internal bool ConvieneOptimizar()
        {
            if (_explicacion_impuntualidad_base != null && (_variacion_mas_maxima > 0 || _variacion_menos_maxima > 0))
            {
                if (_explicacion_impuntualidad_actual != null)
                {
                    if (_explicacion_impuntualidad_previa != null)
                    {
                        bool puntualidadTotalActualMejorPrevia = _explicacion_impuntualidad_previa.ImpuntualidadTotal > _explicacion_impuntualidad_actual.ImpuntualidadTotal;
                        bool impuntualidadReaccionariaActualMejorPrevia = _explicacion_impuntualidad_previa.ImpuntualidadReaccionarios > _explicacion_impuntualidad_actual.ImpuntualidadReaccionarios;
                        bool reaccionariosPredominantes = _explicacion_impuntualidad_actual.RazonReaccionarios > 0.75;
                        return puntualidadTotalActualMejorPrevia && impuntualidadReaccionariaActualMejorPrevia && reaccionariosPredominantes;
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
