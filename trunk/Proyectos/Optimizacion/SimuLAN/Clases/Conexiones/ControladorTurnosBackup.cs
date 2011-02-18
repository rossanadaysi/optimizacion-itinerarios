using System;
using System.Collections.Generic;
using System.Text;

namespace SimuLAN.Clases
{
    /// <summary>
    ///  Clase que encapsula la lógica de uso de turnos de backup
    /// </summary>
    public class ControladorTurnosBackup : IDisposable
    {
        #region CONSTANTS

        /// <summary>
        /// Hora de inicio de la mañana
        /// </summary>
        const int HORA_INI_MANANA = 5;

        /// <summary>
        /// Hora de término de la mañana
        /// </summary>
        const int HORA_FIN_MANANA = 13;

        /// <summary>
        /// Hora de inicio de la tarde
        /// </summary>
        const int HORA_INI_TARDE = 14;

        /// <summary>
        /// Hora de término de la tarde
        /// </summary>
        const int HORA_FIN_TARDE = 22;

        #endregion

        #region ATRIBUTES

        /// <summary>
        /// Turnos de mañana usados por día
        /// </summary>
        private Dictionary<DateTime, int> _turnos_manana;

        /// <summary>
        /// Cantidad de turnos disponibles de mañana
        /// </summary>
        private int _turnos_manana_max;

        /// <summary>
        /// Turnos de tarde usados por día
        /// </summary>
        private Dictionary<DateTime, int> _turnos_tarde;

        /// <summary>
        /// Cantidad de turnos disponibles de tarde
        /// </summary>
        private int _turnos_tarde_max;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Turnos de mañana usados por día
        /// </summary>
        public Dictionary<DateTime, int> TurnosManana
        {
            get { return _turnos_manana; }
        }

        /// <summary>
        /// Cantidad de turnos disponibles de mañana
        /// </summary>
        public int TurnosMananaMax
        {
            get { return _turnos_manana_max; }
            set { _turnos_manana_max = value; }
        }

        /// <summary>
        /// Turnos de tarde usados por día
        /// </summary>
        public Dictionary<DateTime, int> TurnosTarde
        {
            get { return _turnos_tarde; }
        }
        /// <summary>
        /// Cantidad de turnos disponibles de tarde
        /// </summary>
        public int TurnosTardeMax
        {
            get { return _turnos_tarde_max; }
            set { _turnos_tarde_max = value; }
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_turnos_manana_max">Cantidad de turnos disponibles de mañana</param>
        /// <param name="_turnos_tarde_max">Cantidad de turnos disponibles de tarde</param>
        /// <param name="fecha_ini">Fecha de inicio</param>
        /// <param name="fecha_fin">Fecha de término</param>
        public ControladorTurnosBackup(int _turnos_manana_max, int _turnos_tarde_max, DateTime fecha_ini, DateTime fecha_fin)
        {
            this._turnos_manana_max = _turnos_manana_max;
            this._turnos_tarde_max = _turnos_tarde_max;
            this._turnos_manana = new Dictionary<DateTime, int>();
            this._turnos_tarde = new Dictionary<DateTime, int>();
            DateTime dt = fecha_ini;
            while (dt <= fecha_fin)
            {
                _turnos_manana.Add(dt, 0);
                _turnos_tarde.Add(dt, 0);
                dt = dt.AddDays(1);
            }
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Indica si hay un turno disponible para cierta fecha y hora
        /// </summary>
        /// <param name="hora_local">Hora local de consulta</param>
        /// <param name="fecha">Fecha de consulta</param>
        /// <returns>True si hay turno</returns>
        public bool TurnoDisponible(int hora_local, DateTime fecha)
        {
            if (hora_local >= HORA_INI_MANANA && hora_local <= HORA_FIN_MANANA)
            {
                int turnos_usados = _turnos_manana[fecha];
                if (turnos_usados >= _turnos_manana_max)
                {
                    return false;
                }
                return true;
            }
            else if (hora_local >= HORA_INI_TARDE && hora_local <= HORA_FIN_TARDE)
            {
                int turnos_usados = _turnos_tarde[fecha];
                if (turnos_usados >= _turnos_tarde_max)
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Usa un turno de backup en una cierta hora y fecha
        /// </summary>
        /// <param name="hora_local">Hora local</param>
        /// <param name="fecha">Fecha</param>
        public void UsarTurnoBackup(int hora_local, DateTime fecha)
        {
            if (hora_local >= HORA_INI_MANANA && hora_local <= HORA_FIN_MANANA)
            {
                _turnos_manana[fecha]++;
            }
            else if (hora_local >= HORA_INI_TARDE && hora_local <= HORA_FIN_TARDE)
            {
                _turnos_tarde[fecha]++;
            }
        }

        #region IDisposable Members

        private bool IsDisposed = false;

        /// <summary>
        /// Destructor de instancias de la clase
        /// </summary>
        ~ControladorTurnosBackup()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool Disposing)
        {
            if (!IsDisposed)
            {
                if (Disposing)
                {
                    _turnos_manana.Clear();
                    _turnos_tarde.Clear();
                }
                _turnos_tarde = null;
                _turnos_manana = null;
            }
            IsDisposed = true;
        }
        #endregion

        #endregion
    }
}
