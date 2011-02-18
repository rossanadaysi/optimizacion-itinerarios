using System;
using System.Collections.Generic;
using System.Text;

namespace SimuLAN.Clases
{
    /// <summary>
    /// Representa una agrupación o familia de flotas.
    /// </summary>
    public class GrupoFlota
    {
        #region ATRIBUTES

        /// <summary>
        /// Nombre del grupo
        /// </summary>
        private string _nombre;

        /// <summary>
        /// Cantidad de turnos de mañana disponibles para el grupo cada día
        /// </summary>
        private int _turnos_manana;

        /// <summary>
        /// Cantidad de turnos de tarde disponibles para el grupo cada día
        /// </summary>
        private int _turnos_tarde;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Nombre del grupo
        /// </summary>
        public string Nombre
        {
            get { return _nombre; }
            set { _nombre = value; }
        }

        /// <summary>
        /// Cantidad de turnos de mañana disponibles para el grupo cada día
        /// </summary>
        public int Turnos_Manana
        {
            get { return _turnos_manana; }
            set { _turnos_manana = value; }
        }

        /// <summary>
        /// Cantidad de turnos de tarde disponibles para el grupo cada día
        /// </summary>
        public int Turnos_Tarde
        {
            get { return _turnos_tarde; }
            set { _turnos_tarde = value; }
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor de un grupo o familia de flotas
        /// </summary>
        /// <param name="nombre">Nombre de la flota</param>
        /// <param name="turnos_manana">Cantidad de turnos de mañana</param>
        /// <param name="turnos_tarde">Cantidad de turnos de tarde</param>
        public GrupoFlota(string nombre, int turnos_manana, int turnos_tarde)
        {
            this._nombre = nombre;
            this._turnos_manana = turnos_manana;
            this._turnos_tarde = turnos_tarde;
        }

        #endregion
    }
}
