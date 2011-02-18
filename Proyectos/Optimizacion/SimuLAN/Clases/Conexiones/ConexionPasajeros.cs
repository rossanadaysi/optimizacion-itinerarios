using System;
using System.Collections.Generic;
using System.Text;

namespace SimuLAN.Clases
{   
    /// <summary>
    /// Conexión de pasajeros
    /// </summary>
    public class ConexionPasajeros:Conexion
    {
        #region STATIC ATRIBUTES

        /// <summary>
        /// Variable que cuenta la cantidad de pairings que se han agregado
        /// </summary>
        public static int Serial = 0;
        
        #endregion

        #region ATRIBUTES

        /// <summary>
        /// Desviación estándar de pasajeros en conexión
        /// </summary>
        private double _pax_desvest;

        /// <summary>
        /// Promedio de pajeros en conexión
        /// </summary>
        private double _paxs_promedio;

        /// <summary>
        /// Objeto para la generación de números aleatorios
        /// </summary>
        private Random _rdm;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Promedio de pajeros en conexión
        /// </summary>
        public double Paxs_Promedio
        {
            get { return _paxs_promedio; }
            set { _paxs_promedio = value; }
        }

        /// <summary>
        /// Desviación estándar de pasajeros en conexión
        /// </summary>
        public double Pax_Desvest
        {
            get { return _pax_desvest; }
            set { _pax_desvest = value; }
        }

        /// <summary>
        /// Objeto para la generación de números aleatorios
        /// </summary>
        public Random Rdm
        {
            get { return _rdm; }
            set { _rdm = value; }
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id_vuelo_1">Número de vuelo inicial</param>
        /// <param name="id_vuelo_2">Número de vuelo final</param>
        /// <param name="tipo">Tipo de conexión</param>
        /// <param name="paxs_prom">Promedio de pajeros en conexión</param>
        /// <param name="pax_desvest">Desviación estándar de pasajeros en conexión</param>
        public ConexionPasajeros(string id_vuelo_1, string id_vuelo_2, TipoConexion tipo, double paxs_prom, double pax_desvest)
            : base(id_vuelo_1, id_vuelo_2, tipo)
        {
            this._pax_desvest = pax_desvest;
            this._paxs_promedio = paxs_prom;
            Serial++;
            _rdm = new Random();
        }

        #endregion
    }
}
