using System;
using System.Collections.Generic;
using System.Text;

namespace SimuLAN.Clases.Optimizacion
{
    public class Curva
    {
        private Dictionary<double, double> _puntos_curva;

        private double _punto_optimo;

        private double _valor_optimo;

        private double _rango_menos;

        private double _rango_mas;

        private double _salto_variacion;

        public double PuntoOptimo
        {
            get
            {
                if (_punto_optimo == double.MinValue)
                {
                    BuscarMinimoCercanoCero();
                }
                return _punto_optimo;
            }
        }


        public double PuntoMasCercanoCero
        {
            get
            {
                double diff_min = double.MaxValue;                 
                bool encuentra = false;
                foreach (double key in _puntos_curva.Keys)
                {
                    if (Math.Abs(diff_min) > Math.Abs(key))
                    {
                        diff_min = key;
                        encuentra = true;
                    }
                }
                if (encuentra)
                {
                    return diff_min;
                }
                else
                {
                    return 0;
                }
            }
        }
        
        public double ValorOptimo
        {
            get
            {
                if (_punto_optimo == double.MinValue)
                {
                    BuscarMinimoCercanoCero();
                }
                return _valor_optimo;
            }
        }

        public double DiferenciaValorOptimoConValorEnCero
        {
            get
            {
                if (_puntos_curva.ContainsKey(0))
                {
                    return _puntos_curva[0] - ValorOptimo;
                }
                else
                {
                    return 0;
                }
            }
        }

        public Curva(Dictionary<double, double> puntos_curva,double rangoMenos, double rangoMas,double salto_variacion)
        {
            this._puntos_curva = puntos_curva;
            this._punto_optimo = double.MinValue;
            this._valor_optimo = double.MaxValue;
            this._rango_menos = rangoMenos;
            this._rango_mas = rangoMas;
            this._salto_variacion = salto_variacion;
        }

        private void BuscarMinimoCercanoCero()
        {
            for (double i = 0; i <= _rango_mas; i = i + _salto_variacion)
            {
                if (_puntos_curva.ContainsKey(i))
                {
                    if (_puntos_curva[i] < _valor_optimo)
                    {
                        _punto_optimo = i;
                        _valor_optimo = _puntos_curva[i];
                    }
                }
            }
            for (double i = 0; i >= _rango_menos; i = i - _salto_variacion)
            {
                if (_puntos_curva.ContainsKey(i))
                {
                    if (_puntos_curva[i] < _valor_optimo)
                    {
                        _punto_optimo = i;
                        _valor_optimo = _puntos_curva[i];
                    }
                }
            }
        }


    }
}
