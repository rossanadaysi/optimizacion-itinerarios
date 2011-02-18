using System;
using System.Collections.Generic;
using System.Text;

namespace SimuLAN.Clases.Disrupciones
{
    /// <summary>
    /// Método que encapsula la información particular de una disrupción: probabilidad, 
    /// media y desviación estándar.
    /// </summary>
    public class DataDisrupcion
    {
        #region ATRIBUTES

        /// <summary>
        /// Desviación estándar
        /// </summary>
        private double _desvest;

        /// <summary>
        /// Media
        /// </summary>
        private double _media;

        /// <summary>
        /// Probabilidad
        /// </summary>
        private double _prob;

        /// <summary>
        /// Mínimo
        /// </summary>
        private double _min;

        /// <summary>
        /// Máximo
        /// </summary>
        private double _max;

        #endregion

        #region PROPERTIES
        
        /// <summary>
        /// Desviación estándar
        /// </summary>
        public double Desvest
        {
            get { return _desvest; }
            set { _desvest = value; }
        }

        /// <summary>
        /// Media
        /// </summary>
        public double Media
        {
            get { return _media; }
            set { _media = value; }
        }

        /// <summary>
        /// Probabilidad
        /// </summary>
        public double Prob
        {
            get { return _prob; }
            set { _prob = value; }
        }

        /// <summary>
        /// Minimo
        /// </summary>
        public double Min
        {
            get { return _min; }
            set { _min = value; }
        }

        /// <summary>
        /// Maximo
        /// </summary>
        public double Max
        {
            get { return _max; }
            set { _max = value; }
        }
        
        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Crea una instancia para guardar los datos de una disrupción particular
        /// </summary>
        /// <param name="prob">Probabilidad</param>
        /// <param name="media">Media</param>
        /// <param name="desvest">Desviación estándar</param>
        public DataDisrupcion(double prob, double media, double desvest)
        {
            this._prob = prob;
            this._media = media;
            this._desvest = desvest;
            this._min = 0;
            this._max = 0;
        }

        /// <summary>
        /// Crea una instancia para guardar los datos de una disrupción particular
        /// </summary>
        /// <param name="prob">Probabilidad</param>
        /// <param name="media">Media</param>
        /// <param name="desvest">Desviación estándar</param>
        /// <param name="min">Mínimo</param>
        /// <param name="max">Máximo</param>
        public DataDisrupcion(double prob, double media, double desvest, double min, double max)
        {
            this._prob = prob;
            this._media = media;
            this._desvest = desvest;
            this._min = min;
            this._max = max;
        }        
        
        /// <summary>
        /// Constructor para serialización
        /// </summary>
        public DataDisrupcion()
        {

        }

        #endregion
    }
}
