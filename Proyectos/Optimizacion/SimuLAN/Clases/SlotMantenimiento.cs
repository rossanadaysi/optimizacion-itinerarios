using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using SimuLAN.Utils;

namespace SimuLAN.Clases
{
    /// <summary>
    /// Clase que encapsula la información necesaria para simular un slot de mantenimiento.
    /// </summary>
    public class SlotMantenimiento : Slot, IDisposable
    {
        #region ATRIBUTES

        /// <summary>
        /// Diccionario que guarda las causas de atraso que afectaron el tramo, junto con su duración en minutos
        /// </summary>
        private Dictionary<TipoDisrupcion, int> _causas_atraso;

        /// <summary>
        /// Indica si se ha finalizado el slot de mantenimiento
        /// </summary>
        private bool _completado;

        /// <summary>
        /// Duración del trabajo de mantención
        /// </summary>
        private int _duracion_mantenimiento;

        /// <summary>
        /// Tiempo de inicio resultante del slot mantto
        /// </summary>
        private int _t_ini_mantto_rst;

        /// <summary>
        /// Tiempo de término resultante del slot mantto
        /// </summary>
        private int _t_fin_mantto_rst;

        /// <summary>
        /// Tiempo de inicio programado del slot mantto
        /// </summary>
        private int _t_ini_mantto_prg;

        /// <summary>
        /// Tiempo de término programado del slot mantto
        /// </summary>
        private int _t_fin_mantto_prg;
        
        /// <summary>
        /// Tramo base leido desde archivo del itinerario
        /// </summary>
        private TramoBase _tramo_base;

        /// <summary>
        /// Referencia al tramo inmediatamente anterior al slor de mantenimiento.
        /// </summary>
        private Tramo _tramo_previo;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Diccionario que guarda las causas de atraso que afectaron el tramo, junto con su duración en minutos
        /// </summary>
        [XmlIgnore]
        public Dictionary<TipoDisrupcion, int> CausasAtraso
        {
            get { return _causas_atraso; }
        }

        /// <summary>
        /// Indica si se ha finalizado el slot de mantenimiento
        /// </summary>
        [XmlIgnore]
        public bool Completado
        {
            set { _completado = value; }
            get { return _completado; }
        }

        /// <summary>
        /// Duración del trabajo de mantención
        /// </summary>
        public int DuracionMantenimiento
        {
            set { _duracion_mantenimiento = value; }
            get { return _duracion_mantenimiento; }
        }

        /// <summary>
        /// Hour Block Time en formato hh:mm
        /// </summary>
        [XmlIgnore]
        public string HBTProg
        {
            get
            {
                int duracion = _t_fin_mantto_prg - _t_ini_mantto_prg;
                int minutos = duracion % 60;
                int horas = Convert.ToInt16(Math.Floor(duracion / 60.0));
                string minutos_str = (minutos.ToString().Length == 0) ? "00" : (minutos.ToString().Length == 1) ? "0" + minutos.ToString() : minutos.ToString();
                string horas_str = (horas.ToString().Length == 0) ? "00" : (horas.ToString().Length == 1) ? "0" + horas.ToString() : horas.ToString();
                return horas_str + ":" + minutos_str;
            }
        }

        /// <summary>
        /// Tiempo de inicio resultante del slot mantto
        /// </summary>
        public int TiempoInicioManttoRst
        {
            set { _t_ini_mantto_rst = value; }
            get { return _t_ini_mantto_rst; }
        }

        /// <summary>
        /// Tiempo de término resultante del slot mantto
        /// </summary>
        public int TiempoFinManttoRst
        {
            set { _t_fin_mantto_rst = value; }
            get { return _t_fin_mantto_rst; }
        }

        /// <summary>
        /// Tiempo de inicio programado del slot mantto
        /// </summary>
        public int TiempoInicioManttoPrg
        {
            set { _t_ini_mantto_prg = value; }
            get { return _t_ini_mantto_prg; }
        }

        /// <summary>
        /// Tiempo de término programado del slot mantto
        /// </summary>
        public int TiempoFinManttoPrg
        {
            set { _t_fin_mantto_rst = value; }
            get { return _t_fin_mantto_rst; }
        }

        /// <summary>
        /// Tramo base leido desde archivo Excel del itinerario
        /// </summary>
        public TramoBase TramoBase
        {
            get { return _tramo_base; }
            set { _tramo_base = value; }
        }

        /// <summary>
        /// Referencia al tramo inmediatamente anterior al slor de mantenimiento.
        /// </summary>        
        public Tramo TramoPrevio
        {
            set { _tramo_previo = value; }
            get { return _tramo_previo; }
        }
        
        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Constructor para serialización
        /// </summary>
        public SlotMantenimiento()
        { }
      
        /// <summary>
        /// Contructor de un slot de mantenimiento
        /// </summary>
        /// <param name="tramoBase">Información leida de archivo</param>
        /// <param name="flota">Flota a la que pertenece el avión asignado</param>
        /// <param name="tramoPrevio">Tramo inmediatamente anterior al slot</param>
        /// <param name="fechaBase">Fecha inicial del itinerario</param>
        public SlotMantenimiento(TramoBase tramoBase, string flota, Tramo tramoPrevio, DateTime fechaBase)
        {
            this._tramo_base = tramoBase;
            this.Estacion = tramoBase.Origen;
            this._tramo_previo = tramoPrevio;
            this._completado = false;
            this._causas_atraso = new Dictionary<TipoDisrupcion, int>();         
            int diasDesfaseSalida = Convert.ToInt32((tramoBase.Fecha_Salida - fechaBase).TotalDays);
            int diasDesfaseLlegada = Convert.ToInt32((tramoBase.Fecha_Llegada - fechaBase).TotalDays);
            int horaSalidaNum = Utilidades.ConvertirMinutosDesdeHoraString(tramoBase.Hora_Salida);
            int horaLlegadaNum = Utilidades.ConvertirMinutosDesdeHoraString(tramoBase.Hora_Llegada);
            this._t_ini_mantto_prg = diasDesfaseSalida * 24 * 60 + horaSalidaNum;
            this._t_ini_mantto_rst = _t_ini_mantto_prg;
            this._t_fin_mantto_prg = diasDesfaseLlegada * 24 * 60 + horaLlegadaNum;
            this._t_fin_mantto_rst = _t_fin_mantto_prg;
            this._duracion_mantenimiento = _t_fin_mantto_prg - _t_ini_mantto_prg;          
            this.AvionProgramado = this._tramo_base.Numero_Ac.ToString();
        }

        #endregion

        #region INTERNAL METHODS
        
        /// <summary>
        /// Crea una copia del mantenimiento actual
        /// </summary>
        /// <returns>Slot de mantenimiento clonado</returns>
        internal SlotMantenimiento Clonar()
        {
            SlotMantenimiento clonado = new SlotMantenimiento();
            clonado.AvionProgramado = this.AvionProgramado;
            clonado._causas_atraso = new Dictionary<TipoDisrupcion, int>();
            clonado.Completado = false;
            clonado.Duracion = this.Duracion;
            clonado.Estacion = this.Estacion;
            clonado.FlotaProgramada = this.FlotaProgramada;
            clonado.TiempoFin = this.TiempoFin;
            clonado._t_fin_mantto_prg = this.TiempoFinManttoPrg;
            clonado.TiempoFinManttoRst = this.TiempoFinManttoPrg;
            clonado.TiempoInicio = this.TiempoInicio;
            clonado.TiempoInicioManttoPrg = this.TiempoInicioManttoPrg;
            clonado.TiempoInicioManttoRst = this.TiempoInicioManttoPrg;
            clonado.DuracionMantenimiento = clonado.TiempoFinManttoPrg - clonado.TiempoInicioManttoPrg;
            clonado.TramoBase = (TramoBase)this.TramoBase.Clone();
            clonado.TramoPrevio = null;
            clonado.TurnAroundMinimo = this.TurnAroundMinimo;
            return clonado;
        }

        /// <summary>
        /// Traslada un slot de mantenimiento en "minutos de desplazamiento"
        /// </summary>
        /// <param name="minutos_desplazamiento">Minutos a desplazar</param>
        internal void Desplazar(int minutos_desplazamiento)
        {
            int minutosEfectivosDesplazo = Math.Max(minutos_desplazamiento - Math.Max(TiempoInicioManttoRst - _tramo_previo.TFinalProg - TurnAroundMinimo, 0), 0);
            this.TiempoInicio += minutos_desplazamiento;
            this._t_fin_mantto_rst += minutosEfectivosDesplazo;
            this._t_ini_mantto_rst += minutosEfectivosDesplazo;
            this.TiempoFin += this.TramoPrevio.Tramo_Siguiente != null ? Math.Max(minutosEfectivosDesplazo - (this.TramoPrevio.Tramo_Siguiente.TInicialProg - _t_fin_mantto_rst - TurnAroundMinimo), 0) : 0;
        }

        /// <summary>
        /// Método de simulación para el inicio operacional de un slot de mantenimiento
        /// </summary>
        /// <returns></returns>
        internal bool IniciarMantenimientoProgramado()
        {            
            Avion portador = _tramo_previo.GetAvion(AvionProgramado);
            portador.ActualizarTiempoSimulacion(_t_ini_mantto_rst);
            portador.EventosAvion.RemoveAt(0);
            portador.EventosAvion.Add(new Evento(TipoEvento.FinMantenimiento, _t_fin_mantto_rst, new MetodoEventoEventHandler(TerminarMantenimientoProgramado)));
            portador.EventosAvion.Sort();
            return false;
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Método de simulación para el término operacional de un slot de mantenimiento.
        /// </summary>
        /// <returns></returns>
        private bool TerminarMantenimientoProgramado()
        {
            Avion portador = _tramo_previo.GetAvion(AvionProgramado);
            portador.ActualizarTiempoSimulacion(_t_fin_mantto_rst);
            portador.EventosAvion.RemoveAt(0);
            this._completado = true;            
            return false;
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Agrega al slot de mantenimiento la información de su slot contenedor en función de los tramos que delimitan al slot contenedor
        /// </summary>
        /// <param name="tramo_previo">Tramo previo al slot</param>
        /// <param name="tramo_siguiente">Tramo siguiente al slot</param>
        public void AgregarInfoSlot(Tramo tramo_previo, Tramo tramo_siguiente)
        {
            Slot slotContenedor = new Slot(tramo_previo, tramo_siguiente);
            this.TiempoInicio = slotContenedor.TiempoInicio;
            this.TiempoFin = slotContenedor.TiempoFin;
            this.Duracion = slotContenedor.Duracion;
            this.TurnAroundMinimo = slotContenedor.TurnAroundMinimo;
            this.FlotaProgramada = slotContenedor.FlotaProgramada;
        }

        #endregion 

        #region IDisposable Members

        private bool IsDisposed = false;

        /// <summary>
        /// Destructor de instancias de la clase
        /// </summary>        
        ~SlotMantenimiento()
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
                    _causas_atraso.Clear();
                }
                _causas_atraso = null;
                TramoPrevio = null;
                _tramo_base = null;
            }
            IsDisposed = true;
        }

        #endregion
    }
}
