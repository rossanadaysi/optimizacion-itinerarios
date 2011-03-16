using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SimuLAN.Clases
{
    /// <summary>
    /// Objeto que encapsula un slot.
    /// </summary>
    public class Slot : IComparable
    {
        #region ATRIBUTES

        /// <summary>
        /// Identificador del avión del slot
        /// </summary>
        private string _avion_prg;

        /// <summary>
        /// Duración del slot
        /// </summary>
        private int _duracion;

        /// <summary>
        /// Estación del slot
        /// </summary>
        private string _estacion;

        /// <summary>
        /// Fecha de término del slot
        /// </summary>
        private DateTime _fecha_fin;

        /// <summary>
        /// Fecha de inicio del slot
        /// </summary>
        private DateTime _fecha_ini;

        /// <summary>
        /// Flota del avión del slot
        /// </summary>
        private string _flota_prg;

        /// <summary>
        /// Hora de término del slot
        /// </summary>
        private string _hora_fin;

        /// <summary>
        /// Hora de inicio del slot
        /// </summary>
        private string _hora_ini;

        /// <summary>
        /// Slot previo al slot actual
        /// </summary>
        private Slot _slot_previo;

        /// <summary>
        /// Slot posterior al slot actual
        /// </summary>
        private Slot _slot_siguiente;

        /// <summary>
        /// Tiempo de término del slot en minutos de simulación
        /// </summary>
        private int _t_fin;

        /// <summary>
        /// Tiempo de inicio del slot en minutos de simulación
        /// </summary>
        private int _t_ini;

        /// <summary>
        /// Tramo de vuelo siguiente al slot.
        /// </summary>
        private Tramo _tramo_siguiente;

        /// <summary>
        /// Turn Around mínimo en el slot
        /// </summary>
        private int _turn_around_minimo;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Identificador del avión del slot
        /// </summary>
        public string AvionProgramado
        {
            get { return _avion_prg; }
            set { _avion_prg = value; }
        }

        /// <summary>
        /// Duración del slot
        /// </summary>
        public int Duracion
        {
            set { _duracion = value; }
            get { return _duracion; }
        }
        
        /// <summary>
        /// Duracion en formato hh:mm
        /// </summary>
        public string DuracionHHMM
        {
            get
            {
                int minutos = _duracion % 60;
                int horas = Convert.ToInt16(Math.Floor(_duracion / 60.0));
                string minutos_str = (minutos.ToString().Length == 0) ? "00" : (minutos.ToString().Length == 1) ? "0" + minutos.ToString() : minutos.ToString();
                string horas_str = (horas.ToString().Length == 0) ? "00" : (horas.ToString().Length == 1) ? "0" + horas.ToString() : horas.ToString();
                return horas_str + ":" + minutos_str;
            }
        }

        /// <summary>
        /// Estación del slot
        /// </summary>
        public string Estacion
        {
            get { return _estacion; }
            set { _estacion = value; }
        }

        /// <summary>
        /// Fecha de término del slot
        /// </summary>
        public DateTime FechaFin
        {
            get { return _fecha_fin; }
            set { _fecha_fin = value; }
        }

        /// <summary>
        /// Fecha de inicio del slot
        /// </summary>
        public DateTime FechaIni
        {
            get { return _fecha_ini; }
            set { _fecha_ini = value; }
        }

        /// <summary>
        /// Flota del avión del slot
        /// </summary>
        public string FlotaProgramada
        {
            get { return _flota_prg; }
            set { _flota_prg = value; }
        }

        /// <summary>
        /// Hora de término del slot
        /// </summary>
        public string HoraFin
        {
            get { return _hora_fin; }
            set { _hora_fin = value; }
        }

        /// <summary>
        /// Hora de inicio del slot
        /// </summary>
        public string HoraIni
        {
            get { return _hora_ini; }
            set { _hora_ini = value; }
        }

        /// <summary>
        /// Slot previo al slot actual
        /// </summary>
        public Slot SlotPrevio
        {
            get { return _slot_previo; }
            set { _slot_previo = value; }
        }

        /// <summary>
        /// Slot posterior al slot actual
        /// </summary>
        public Slot SlotSiguiente
        {
            get { return _slot_siguiente; }
            set { _slot_siguiente = value; }
        }

        /// <summary>
        /// Tiempo de término del slot en minutos de simulación
        /// </summary>
        public int TiempoFin
        {
            set { _t_fin = value; }
            get { return _t_fin; }
        }

        /// <summary>
        /// Tiempo de inicio del slot en minutos de simulación
        /// </summary>
        public int TiempoInicio
        {
            set { _t_ini = value; }
            get { return _t_ini; }
        }

        /// <summary>
        /// Tramo de vuelo siguiente al slot.
        /// </summary>
        public Tramo TramoSiguiente
        {
            get { return _tramo_siguiente; }
            set { _tramo_siguiente = value; }
        }

        /// <summary>
        /// Turn Around mínimo en el slot
        /// </summary>
        public int TurnAroundMinimo
        {
            set { _turn_around_minimo = value; }
            get { return _turn_around_minimo; }
        }
        
        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Constructor para serialización
        /// </summary>
        public Slot()
        { 
        }

        /// <summary>
        /// Constructor de slots
        /// </summary>
        /// <param name="estacion">Estación</param>
        /// <param name="avionProgramado">Id de avión programado</param>
        /// <param name="tiempoInicio">Tiempo de inicio</param>
        /// <param name="tiempoFin">Tiempo de término</param>
        /// <param name="turnAroundMinimo">T/A mínimo</param>
        /// <param name="fechaIni">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de término</param>
        /// <param name="horaIni">Hora de inicio</param>
        /// <param name="horaFin">Hora de término</param>
        /// <param name="tramoSiguiente">Tramo siguiente al slot</param>
        public Slot(string estacion, string avionProgramado, int tiempoInicio, int tiempoFin, int turnAroundMinimo, DateTime fechaIni, DateTime fechaFin, string horaIni, string horaFin, Tramo tramoSiguiente)
        {
            this._estacion = estacion;
            this._t_ini = tiempoInicio;
            this._t_fin = tiempoFin;
            this._duracion = tiempoFin - tiempoInicio;
            this._turn_around_minimo = turnAroundMinimo;
            this._avion_prg = avionProgramado;
            this._fecha_ini = fechaIni;
            this._fecha_fin = fechaFin;
            this._hora_ini = horaIni;
            this._hora_fin = horaFin;
            this._tramo_siguiente = tramoSiguiente;
        }

        /// <summary>
        /// Constructor de slots
        /// </summary>
        /// <param name="previo">Tramo previo</param>
        /// <param name="siguiente">Tramo siguiente</param>
        public Slot(Tramo previo, Tramo siguiente)
        {
            if (previo == null && siguiente == null)
            {
                this._estacion = null;
                this.AvionProgramado = null;
                this.FlotaProgramada = null;
                this._t_ini = 0;
                this._t_fin = 20000;
                this._duracion = _t_fin - _t_ini;
                this.TurnAroundMinimo = 60;            
            }

            else if (previo == null && siguiente != null)
            {
                this._estacion = siguiente.TramoBase.Origen;
                this.AvionProgramado = siguiente.IdAvionProgramado;
                this.FlotaProgramada = siguiente.FlotaProgramada;
                this._t_ini = 0;
                this._t_fin = siguiente.TInicialProg;
                this._duracion = _t_fin - _t_ini;
                this.TurnAroundMinimo = siguiente.GetTurnAroundMinimo(siguiente);
            }

            else if (previo!=null && siguiente==null)
            {
                this._estacion = previo.TramoBase.Destino;
                this.AvionProgramado = previo.IdAvionProgramado;
                this.FlotaProgramada = previo.FlotaProgramada;
                this._t_ini = previo.TFinalProg;
                this._t_fin = 20000;
                this._duracion = _t_fin - _t_ini;
                this.TurnAroundMinimo = 60;
            }

            else if (previo.TramoBase.Destino == siguiente.TramoBase.Origen)
            {
                this._estacion = previo.TramoBase.Destino;
                this.AvionProgramado = previo.IdAvionProgramado;
                this.FlotaProgramada = previo.FlotaProgramada;
                this._t_ini = previo.TFinalProg;
                this._t_fin = siguiente.TInicialProg;
                this._duracion = _t_fin - _t_ini;
                this.TurnAroundMinimo = siguiente.GetTurnAroundMinimo(siguiente);
            }

        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Identificador de slot
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Concat(_estacion, " ", _duracion.ToString());
        }

        #endregion

        #region IComparable Members

        /// <summary>
        /// Compara el slot actual con obj.
        /// </summary>
        /// <param name="obj">Slot a comparar</param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            Slot slotComparado = (Slot)obj;
            Slot slotOriginal = this;
            if (slotOriginal.TiempoInicio < slotComparado.TiempoInicio)
            {
                return -1;
            }
            else if (slotOriginal.TiempoInicio == slotComparado.TiempoInicio)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        #endregion
    }
}
