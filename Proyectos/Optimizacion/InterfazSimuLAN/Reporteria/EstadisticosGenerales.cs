using System;
using System.Collections.Generic;
using System.Text;

namespace InterfazSimuLAN.Reportes
{
    /// <summary>
    /// Estructura que almacena estadísticas de puntualidad
    /// </summary>
    public class EstadisticosGenerales
    {
        #region ATRIBUTES

        /// <summary>
        /// Desviación estándar de los datos
        /// </summary>
        private double _desvest;

        /// <summary>
        /// Máximo de los datos
        /// </summary>
        private double _max;

        /// <summary>
        /// Media de los datos
        /// </summary>
        private double _media;

        /// <summary>
        /// Mínimo de los datos
        /// </summary>
        private double _min;

        /// <summary>
        /// Número de datos
        /// </summary>
        private double _n;

        /// <summary>
        /// Lista con todos los elementos considerados para los cálculos de los estadísticos
        /// </summary>
        private List<double> _valores;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Desviación estándar de los datos
        /// </summary>
        public double Desvest
        {
            get { return _desvest; }
        }

        /// <summary>
        /// Máximo de los datos
        /// </summary>
        public double Max
        {
            get { return _max; }
        }

        /// <summary>
        /// Número de datos
        /// </summary>
        public double N
        {
            get { return _n; }
        }

        /// <summary>
        /// Media de los datos
        /// </summary>
        public double Media
        {
            get { return _media; }
        }

        /// <summary>
        /// Mínimo de los datos
        /// </summary>
        public double Min
        {
            get { return _min; }
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Inicializa y calcula los estadísticos de la estructura 
        /// </summary>
        /// <param name="valores">Lista con los valores a procesar</param>
        public EstadisticosGenerales(List<double> valores)
        {
            //Inicialización
            this._valores = valores;
            this._media = 0;
            this._desvest = 0;
            this._n = 0;
            this._min = 1000;
            this._max = -1000;

            //Cálculo
            EstimarEstadisticos();
        }

        /// <summary>
        /// Inicializa y calcula los estadísticos de la estructura 
        /// </summary>
        /// <param name="valores">Arreglo con los valores a procesar</param>
        public EstadisticosGenerales(double[] valores)
        {
            //Inicialización
            this._valores = new List<double>();
            this._media = 0;
            this._desvest = 0;
            this._n = 0;
            this._min = 1000;
            this._max = -1000;
            for (int i = 0; i < valores.Length; i++)
			{
			    _valores.Add(valores[i]);
			}
            //Cálculo
            EstimarEstadisticos();
        }

        /// <summary>
        /// Inicializa y calcula los estadísticos de la estructura 
        /// </summary>
        /// <param name="valores">Matriz con los valores a procesar</param>
        public EstadisticosGenerales(double[,] valores)
        {
            //Inicialización
            this._valores = new List<double>();
            this._media = 0;
            this._desvest = 0;
            this._n = 0;
            this._min = 1000;
            this._max = -1000;
            for (int i = 0; i < valores.GetLength(0); i++)
            {
                for (int j = 0; j < valores.GetLength(1); j++)
                {
                    _valores.Add(valores[i, j]);
                }
            }
            //Cálculo
             EstimarEstadisticos();
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Calcula los valores de los estadísticos
        /// </summary>
        public void EstimarEstadisticos()
        {
            _n = _valores.Count;
            _media = EstimarMedia(_valores);
            _desvest = EstimarDesvest(_valores, _media);
            _min = EstimarMin(_valores);
            _max = EstimarMax(_valores);
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Estima la desviación estándar del total de los valores computados
        /// </summary>
        /// <param name="valores">Lista de valores</param>
        /// <returns>Desviación estándar de los valores</returns>
        private double EstimarDesvest(List<double> valores, double media)
        {
            double suma = 0;
            if (_n < 2)
            {
                return 0;
            }
            foreach (double d in valores)
            {
                suma += (d - media) * (d - media);
            }
            return Math.Sqrt(suma / (_n - 1));
        }

        /// <summary>
        /// Estima el máximo entre el total de los valores computados
        /// </summary>
        /// <param name="valores">Lista de valores</param>
        /// <returns>Máximo entre los valores</returns>
        private double EstimarMax(List<double> valores)
        {
            double maximo = -1000;
            foreach (double valor in valores)
            {
                if (maximo < valor)
                {
                    maximo = valor;
                }
            }
            return maximo;
        }

        /// <summary>
        /// Estima la media del total de los valores computados
        /// </summary>
        /// <param name="valores">Lista de valores</param>
        /// <returns>Media de los valores</returns>
        private double EstimarMedia(List<double> valores)
        {
            double suma = 0;
            foreach (double d in valores)
            {
                suma += d;
            }
            return suma / _n;
        }

        /// <summary>
        /// Estima el mínimo entre el total de los valores computados
        /// </summary>
        /// <param name="valores">Lista de valores</param>
        /// <returns>Mínimo entre los valores</returns>
        private double EstimarMin(List<double> valores)
        {
            double minimo = 1000;
            foreach (double valor in valores)
            {
                if (minimo > valor)
                {
                    minimo = valor;
                }
            }
            return minimo;
        }

        #endregion
    }
}
