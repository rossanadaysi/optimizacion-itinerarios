using System;
using System.Collections.Generic;
using System.Text;

namespace SimuLAN.Clases
{
    /// <summary>
    /// Clase abstracta de la que derivan la conexión por pairing y pasajeros.
    /// </summary>
    public abstract class Conexion
    {
        #region ATRIBUTES

        /// <summary>
        /// Id de vuelo inicial
        /// </summary>
        private string _id_vuelo_1;

        /// <summary>
        /// Id de vuelo final
        /// </summary>
        private string _id_vuelo_2;

        /// <summary>
        /// Tipo de conexión
        /// </summary>
        private TipoConexion _tipo;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Id de vuelo inicial
        /// </summary>
        public string IdVuelo1
        {
            get { return _id_vuelo_1; }
            set { _id_vuelo_1 = value; }
        }

        /// <summary>
        /// Id de vuelo final
        /// </summary>
        public string IdVuelo2
        {
            get { return _id_vuelo_2; }
            set { _id_vuelo_2 = value; }
        }

        /// <summary>
        /// Tipo de conexión
        /// </summary>
        public TipoConexion Tipo
        {
            get { return _tipo; }
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id_vuelo_1">Id de vuelo inicial</param>
        /// <param name="id_vuelo_2">Id de vuelo final</param>
        /// <param name="tipo">Tipo de conexión</param>
        public Conexion(string id_vuelo_1, string id_vuelo_2, TipoConexion tipo)
        {
            this._id_vuelo_1 = id_vuelo_1;
            this._id_vuelo_2 = id_vuelo_2;
            this._tipo = tipo;
        }

        #endregion
    }
}
