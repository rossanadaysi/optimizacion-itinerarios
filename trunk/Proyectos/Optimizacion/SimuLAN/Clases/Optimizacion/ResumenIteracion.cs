using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimuLAN.Clases.Optimizacion
{
    public class ResumenIteracion
    {
        private Dictionary<int, ExplicacionImpuntualidad> _impuntualidades;
        private Dictionary<int, int> _variaciones;
        private FaseOptimizacion _fase;
        private int _numero_iteracion;
        private Dictionary<int,double> _impuntualidad_total;
        private Dictionary<int,double> _impuntualidad_reaccionarios;
        private Dictionary<int,double> _impuntualidad_no_reaccionarios;
        private double _atraso_total;
        private double _atraso_reaccionario_total;
        private double _atraso_no_reaccionario_total;
        private Dictionary<int, int> _frecuencias_de_variaciones;
        private double _promedio_variaciones_positivos;
        private double _promedio_variaciones_negativos;      
        private double _promedio_total_variaciones_absolutas;
        private double _promedio_total_variaciones_absolutas_con_ceros;
        private double _cantidad_variaciones_positivas;
        private double _cantidad_variaciones_negativas;


        public double CantidadTramosNoVariados
        {
            get
            {
                if (_frecuencias_de_variaciones.ContainsKey(0))
                {
                    return _frecuencias_de_variaciones[0];
                }
                else
                {
                    return 0;
                }
            }
        }
        public double CantidadVariacionesTotales
        {
            get
            {
                return _cantidad_variaciones_positivas + _cantidad_variaciones_negativas;
            }
        }
        public Dictionary<int, double> ImpuntualidadTotal
        {
            get
            {
                return _impuntualidad_total;
            }
        }
        public Dictionary<int, double> ImpuntualidadReaccionarios
        {
            get
            {
                return _impuntualidad_reaccionarios;
            }
        }
        public Dictionary<int, double> ImpuntualidadNoReaccionarios
        {
            get
            {
                return _impuntualidad_no_reaccionarios;
            }
        }
        public double AtrasoTotal
        {
            get
            {
                return _atraso_total;
            }
        }
        public double AtrasoReaccionarioTotal
        {
            get
            {
                return _atraso_reaccionario_total;
            }
        }
        public double AtrasoNoReaccionarioTotal
        {
            get
            {
                return _atraso_no_reaccionario_total;
            }
        }
        public Dictionary<int, int> FrecuenciasDeVariaciones
        {
            get
            {
                return _frecuencias_de_variaciones;
            }
        }
        public double PromedioVariacionesPositivos
        {
            get
            {
                return _promedio_variaciones_positivos;
            }
        }
        public double PromedioVariacionesNegativos
        {
            get
            {
                return _promedio_variaciones_negativos;
            }
        }
        public double PromedioTotalVariacionesAbsolutas
        {
            get
            {
                return _promedio_total_variaciones_absolutas;
            }
        }
        public double PromedioTotalVariacionesAbsolutasConCeros
        {
            get
            {
                return _promedio_total_variaciones_absolutas_con_ceros;
            }
        }
        public double CantidadVariacionesPositivas
        {
            get
            {
                return _cantidad_variaciones_positivas;
            }
        }
        public double CantidadVariacionesNegativas
        {
            get
            {
                return _cantidad_variaciones_negativas;
            }
        }

        public ResumenIteracion(FaseOptimizacion fase, int numero_iteracion, Dictionary<int, ExplicacionImpuntualidad> impuntualidades, Dictionary<int, int> variaciones)
        {
            this._impuntualidades = impuntualidades;
            this._variaciones = variaciones;
            this._fase = fase;
            this._numero_iteracion = numero_iteracion;
            EstimarIndicadores();
        }

        public ResumenIteracion(FaseOptimizacion fase, int numero_iteracion, Dictionary<int, ExplicacionImpuntualidad> impuntualidades)
        {
            this._impuntualidades = impuntualidades;
            this._fase = fase;
            this._numero_iteracion = numero_iteracion;  
            this._variaciones = new Dictionary<int, int>();
            foreach (int id_tramo in impuntualidades.Keys)
            {
                _variaciones.Add(id_tramo, 0);
            }
            EstimarIndicadores();
        }

        public void EstimarIndicadores()
        {
            EstimarImpuntualidades();
            EstimarAtrasosTotales();
            EstimarVariaciones();
        }

        private void EstimarVariaciones()
        {
            _frecuencias_de_variaciones = new Dictionary<int, int>();
            _cantidad_variaciones_positivas = 0;
            _cantidad_variaciones_negativas = 0;
            foreach (int variacion in _variaciones.Values)
            {
                if (!_frecuencias_de_variaciones.ContainsKey(variacion))
                {
                    _frecuencias_de_variaciones.Add(variacion, 0);
                }
                _frecuencias_de_variaciones[variacion]++;
                if (variacion > 0)
                {
                    _cantidad_variaciones_positivas++;
                    _promedio_variaciones_positivos += variacion;
                }
                else if (variacion < 0)
                {
                    _cantidad_variaciones_negativas++;
                    _promedio_variaciones_negativos += variacion;
                }
                int variacion_absoluta = Math.Abs(variacion);
                if (variacion_absoluta > 0)
                {
                    _promedio_total_variaciones_absolutas_con_ceros += variacion;
                    _promedio_total_variaciones_absolutas += variacion;
                }
            }
            if (_cantidad_variaciones_positivas > 0)
            {
                _promedio_variaciones_positivos /= _cantidad_variaciones_positivas;
            }
            if (_cantidad_variaciones_negativas > 0)
            {
                _promedio_variaciones_negativos /= _cantidad_variaciones_negativas;
            }
            if (CantidadVariacionesTotales > 0)
            {
                _promedio_total_variaciones_absolutas /= CantidadVariacionesTotales;
            }
            if (_variaciones.Count > 0)
            {
                _promedio_total_variaciones_absolutas_con_ceros /= _variaciones.Count;
            }
        }

        private void EstimarAtrasosTotales()
        {
            _atraso_total = 0;
            _atraso_reaccionario_total = 0;
            _atraso_no_reaccionario_total = 0;
            foreach (ExplicacionImpuntualidad explicacion_impuntualidad in _impuntualidades.Values)
            {
                _atraso_total += explicacion_impuntualidad.AtrasoTotal;
                _atraso_reaccionario_total += explicacion_impuntualidad.AtrasoReaccionarios;
                _atraso_no_reaccionario_total += explicacion_impuntualidad.AtrasoSinReaccionarios;
            }
        }

        private void EstimarImpuntualidades()
        {
            foreach (int id_tramo in _impuntualidades.Keys)
            {
                foreach (int std in _impuntualidades[id_tramo].ImpuntualidadTotal.Keys)
                {
                    if (!_impuntualidad_total.ContainsKey(std))
                    {
                        _impuntualidad_total.Add(std, 0);
                        _impuntualidad_reaccionarios.Add(std, 0);
                        _impuntualidad_no_reaccionarios.Add(std, 0);
                    }
                    _impuntualidad_total[std] += _impuntualidades[id_tramo].ImpuntualidadTotal[std];
                    _impuntualidad_reaccionarios[std] += _impuntualidades[id_tramo].ImpuntualidadReaccionarios[std];
                    _impuntualidad_no_reaccionarios[std] += _impuntualidades[id_tramo].ImpuntualidadSinReaccionarios[std];
                }
            }
            int total_tramos = _impuntualidades.Count;
            foreach (int std in _impuntualidad_total.Keys)
            {
                _impuntualidad_total[std] /= total_tramos;
                _impuntualidad_reaccionarios[std] /= total_tramos;
                _impuntualidad_no_reaccionarios[std] /= total_tramos;
            }
        }

        private void ImprimirResumen(List<int> dominio, List<int> stds)
        {
            StringBuilder sb = new StringBuilder();
            string tab = "\t";
            sb.Append(_numero_iteracion.ToString());
            sb.Append(tab + _fase.ToString());
            sb.Append(tab + _atraso_total.ToString());
            sb.Append(tab + _atraso_reaccionario_total.ToString());
            sb.Append(tab + _atraso_no_reaccionario_total.ToString());
            foreach (int std in stds)
            {
                sb.Append(tab + _impuntualidad_total[std].ToString());
                sb.Append(tab + _impuntualidad_reaccionarios[std].ToString());
                sb.Append(tab + _impuntualidad_no_reaccionarios[std].ToString());
            }
            sb.Append(tab + CantidadVariacionesTotales.ToString());
            sb.Append(tab + _cantidad_variaciones_positivas.ToString());
            sb.Append(tab + _cantidad_variaciones_negativas.ToString());
            sb.Append(tab +  CantidadTramosNoVariados.ToString());
            sb.Append(tab + _promedio_total_variaciones_absolutas.ToString());
            sb.Append(tab + _promedio_total_variaciones_absolutas_con_ceros.ToString());
            sb.Append(tab + _promedio_variaciones_positivos.ToString());
            sb.Append(tab + _promedio_variaciones_negativos.ToString());
            foreach (int variacion in dominio)
            {
                sb.Append(tab + _frecuencias_de_variaciones[variacion].ToString());
            }
        }

    }
}
