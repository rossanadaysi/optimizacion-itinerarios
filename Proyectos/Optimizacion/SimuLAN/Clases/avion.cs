using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Data.OleDb;
using SimuLAN.Clases;
using SimuLAN.Utils;
using System.Xml.Serialization;
using SimuLAN.Clases.Disrupciones;

namespace SimuLAN.Clases
{
    /// <summary>
    /// Objeto que encapsula un avión junto con su itinerario programado
    /// </summary>
    [XmlRoot("avion")]
    public class Avion:ICloneable, IDisposable
    {
        #region CONSTANT

        /// <summary>
        /// Se asume que no puede haber un adelanto en el tiempo programado de despegue mayor a 60 minutos.
        /// </summary>
        private const int ADELANTO_MAXIMO = 60;

        #endregion

        #region ATRIBUTES

        /// <summary>
        /// Delegado para acceder y modificar el tiempo de simulación
        /// </summary>
        private ActualizarTiempoSimulacionEventHandler _actualizar_tiempo_simulacion;

        /// <summary>
        /// Aircraft Type
        /// </summary>
        private string _ac_type;
        
        /// <summary>
        /// Aeropuerto de salida del tramo actual en operación
        /// </summary>
        private Aeropuerto _aeropuerto_actual;

        /// <summary>
        /// Atraso sobre el tiempo programado de aterrizaje
        /// </summary>
        private int _atraso_vuelo;

        /// <summary>
        /// Atraso sobre el tiempo programado de despegue
        /// </summary>
        private int _atraso_despegue;

        /// <summary>
        /// Lista de eventos pendientes del avión
        /// </summary>
        private List<Evento> _eventos_avion;

        /// <summary>
        /// Delegado para obtener un aeropuerto
        /// </summary>
        private GetAeropuertoEventHandler _get_aeropuerto;

        /// <summary>
        /// Delegado para obtener slots de backup
        /// </summary>
        private GetBackupsAvionEventHandler _get_backups;

        /// <summary>
        /// Delegado para obtener la flota
        /// </summary>
        private GetFlotaEventHandler _get_flota;

        /// <summary>
        /// Delegado para obtener información de atrasos.
        /// </summary>
        private GetInformacionAtrasosEventHandler _get_info_atrasos;

        /// <summary>
        /// Grupo de flota al que pertenece el avión.
        /// </summary>
        private GrupoFlota _grupo;

        /// <summary>
        /// Indica si se ha realizó sobre el último tramo operado el algoritmo de recovery
        /// </summary>
        private bool _hizo_recovery;

        /// <summary>
        /// Número global del avión
        /// </summary>
        private string _id_avion;

        /// <summary>
        /// Lista de tramos del avión
        /// </summary>
        private SerializableList<Tramo> _legs;

        /// <summary>
        /// Inicial de la matrícula del avión
        /// </summary>
        private string _matricula;

        /// <summary>
        /// Lista con todos los pares origen destino programados en el itinerario del avión
        /// </summary>
        [XmlIgnore]
        private SerializableList<string> _pares_od_programados;

        /// <summary>
        /// Indica si el primer slot del itinerario es de mantenimiento
        /// </summary>
        private bool _primer_slot_es_mantenimiento;

        /// <summary>
        /// Indica si el avión acaba de aterrizar
        /// </summary>
        [XmlIgnore]
        private bool _recien_aterrizado;

        /// <summary>
        /// Delegado para algoritmo de recovery
        /// </summary>
        private RecoveryEventHandler _recovery;

        /// <summary>
        /// Indica si para el avión actual se efectuó una opción de recovery en el tramo anteriormente operado.
        /// </summary>
        [XmlIgnore]
        private bool _recovery_reciente;        

        /// <summary>
        /// Slot actual en operación
        /// </summary>
        [XmlIgnore]
        public Slot _slot_actual;

        /// <summary>
        /// Lista de slots de mantenimiento del avión
        /// </summary>
        private SerializableList<SlotMantenimiento> _slots_mantenimiento;

        /// <summary>
        /// Primer slot del avión
        /// </summary>
        [XmlIgnore]
        public Slot _slot_raiz;

        /// <summary>
        /// SubFlota a la que pertenece el avión
        /// </summary>
        private string _subFlota;

        /// <summary>
        /// Tipo de avión (normal, backup full, backup parcial)
        /// </summary>
        private TipoAvion _tipo_avion;

        /// <summary>
        /// Minutos a partir de los cuales se ejecuta el algoritmo de recovery
        /// </summary>
        private int _tolerancia_recovery;

        /// <summary>
        /// Minutos de atraso a partir de los cuales se usa un turno de tripulación de backup
        /// </summary>
        private int _tolerancia_turno;

        /// <summary>
        ///Tramo actual en operación 
        /// </summary>
        [XmlIgnore]
        public Tramo Tramo_Actual;

        /// <summary>
        /// Tramo inicial asignado al avión
        /// </summary>
        [XmlIgnore]
        public Tramo Tramo_Raiz;

        /// <summary>
        /// Último tramo asignado al avión
        /// </summary>
        [XmlIgnore]
        public Tramo Ultimo_Tramo_Agregado;

        /// <summary>
        /// Delegado que encapsula método para usar avión de backup
        /// </summary>
        [XmlIgnore]
        private UsarTurnoBackupEventHandler _usar_turno_backup;
        
        #endregion

        #region PROPERTIES
        
        /// <summary>
        /// Delegado para acceder y modificar el tiempo de simulación
        /// </summary>
        [XmlIgnore]
        public ActualizarTiempoSimulacionEventHandler ActualizarTiempoSimulacion
        {
            get { return _actualizar_tiempo_simulacion; }
            set { this._actualizar_tiempo_simulacion = value; }
        }

        /// <summary>
        /// Aircraft Type
        /// </summary>
        public string AcType
        {
            get { return _ac_type; }
            set { _ac_type = value; }
        }

        /// <summary>
        /// Aeropuerto de salida del tramo actual en operación
        /// </summary>
        [XmlIgnore]
        public Aeropuerto AeropuertoActual
        {
            get { return _aeropuerto_actual; }
        }

        /// <summary>
        /// Atraso sobre el tiempo programado de aterrizaje
        /// </summary>
        [XmlIgnore]
        public int AtrasoVuelo
        {
            get { return _atraso_vuelo; }
        }

        /// <summary>
        /// Atraso sobre el tiempo programado de despegue
        /// </summary>
        [XmlIgnore]
        public int AtrasoDespegue
        {
            get { return _atraso_despegue; }
        }        

        /// <summary>
        /// Lista de eventos pendientes del avión
        /// </summary>
        [XmlIgnore]
        public List<Evento> EventosAvion
        {
            get { return _eventos_avion; }
        }

        /// <summary>
        ///  Delegado para obtener un aeropuerto
        /// </summary>
        [XmlIgnore]
        public GetAeropuertoEventHandler GetAeropuerto
        {
            set { _get_aeropuerto = value; }
            get { return _get_aeropuerto; }
        }

        /// <summary>
        /// Delegado para obtener slots de backup
        /// </summary>
        [XmlIgnore]
        public GetBackupsAvionEventHandler GetBackups
        {
            get { return _get_backups; }
            set { _get_backups = value; }
        }

        /// <summary>
        /// Delegado para obtener la flota
        /// </summary>
        [XmlIgnore]
        public GetFlotaEventHandler GetFlota
        {
            get { return _get_flota; }
            set { _get_flota = value; }
        }

        /// <summary>
        /// Delegado para cargar las curvas a un tramo
        /// </summary>
        [XmlIgnore]
        public GetInformacionAtrasosEventHandler GetInfoAtrasos
        {
            get { return _get_info_atrasos; }
            set { _get_info_atrasos = value; }
        }

        /// <summary>
        /// Grupo de flota al que pertenece el avión.
        /// </summary>
        public GrupoFlota GrupoAvion
        {
            get { return _grupo; }
            set { _grupo = value; }
        }

        /// <summary>
        /// Indica si se ha realizó sobre el último tramo operado el algoritmo de recovery
        /// </summary>
        [XmlIgnore]
        public bool HizoRecovery
        {
            get { return _hizo_recovery; }
            set { _hizo_recovery = value; }
        }

        /// <summary>
        /// Número global del avión
        /// </summary>
        public string IdAvion
        {
            get { return _id_avion; }
            set { _id_avion = value; }
        }

        /// <summary>
        /// Lista de tramos del avión
        /// </summary>
        [XmlIgnore]
        public SerializableList<Tramo> Legs
        {
            get { return _legs; }
            set { _legs = value; }
        }

        /// <summary>
        /// Inicial de la matrícula del avión
        /// </summary>
        public string Matricula
        {
            get { return _matricula; }
            set { _matricula = value; }
        }

        /// <summary>
        /// Indica si el primer slot del itinerario es de mantenimiento
        /// </summary>
        public bool PrimerSlotEsMantenimiento
        {
            set { _primer_slot_es_mantenimiento = value; }
            get { return (_slots_mantenimiento.Count > 0 && (Tramo_Raiz == null || (Tramo_Raiz != null && _slots_mantenimiento[0].TiempoInicioManttoRst < Tramo_Raiz.TInicialProg))); }
        }

        /// <summary>
        /// Indica si el avión acaba de aterrizar
        /// </summary>
        [XmlIgnore]
        public bool RecienAterrizado
        {
            get { return _recien_aterrizado; }
            set { _recien_aterrizado = value; }
        }

        /// <summary>
        /// Delegado para algoritmo de recovery
        /// </summary>
        [XmlIgnore]
        public RecoveryEventHandler Recovery
        {
            set { _recovery = value; }
            get { return _recovery; }
        }

        /// <summary>
        /// Indica si para el avión actual se efectuó una opción de recovery en el tramo anteriormente operado.
        /// </summary>
        [XmlIgnore]
        public bool RecoveryReciente
        {
            get { return _recovery_reciente; }
            set { _recovery_reciente = value; }
        }

        /// <summary>
        /// Slot actual en operación
        /// </summary>
        [XmlIgnore]
        public Slot SlotActual
        {
            get { return _slot_actual; }
            set { _slot_actual = value; }
        }

        /// <summary>
        ///  Lista de slots de mantenimiento del avión
        /// </summary>
        public SerializableList<SlotMantenimiento> SlotsMantenimiento
        {
            get { return _slots_mantenimiento; }
            set { _slots_mantenimiento = value; }
        }

        /// <summary>
        /// Primer slot del avión
        /// </summary>
        [XmlIgnore]
        public Slot SlotRaiz
        {
            get { return _slot_raiz; }
            set { _slot_raiz = value; }
        }

        /// <summary>
        /// SubFlota a la que pertenece el avión
        /// </summary>
        public string SubFlota
        {
            get { return _subFlota; }
            set { _subFlota = value; }
        }

        /// <summary>
        /// Tipo de avión (normal, backup full, backup parcial) 
        /// </summary>
        public TipoAvion TipoAvion
        {
            get { return _tipo_avion; }
            set { _tipo_avion = value; }
        }

        /// <summary>
        /// Minutos a partir de los cuales se ejecuta el algoritmo de recovery
        /// </summary>
        public int ToleranciaRecovery
        {
            get { return _tolerancia_recovery; }
            set { _tolerancia_recovery = value; }
        }

        /// <summary>
        /// Minutos de atraso a partir de los cuales se usa un turno de tripulación de backup
        /// </summary>
        public int ToleranciaTurno
        {
            set { _tolerancia_turno = value; }
            get { return _tolerancia_turno; }
        }

        /// <summary>
        /// Último tramo asignado al avión
        /// </summary>
        [XmlIgnore]
        public Tramo UltimoTramoAgregado
        {
            get { return Ultimo_Tramo_Agregado; }
            set { Ultimo_Tramo_Agregado = value; }
        }

        /// <summary>
        /// Delegado que encapsula método para intentar usar un turno de backup
        /// </summary>
        [XmlIgnore]
        public UsarTurnoBackupEventHandler UsarTurnoBackupProp
        {
            get { return _usar_turno_backup; }
            set { _usar_turno_backup = value; }
        }
        
        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Constructor para serialización
        /// </summary>
        public Avion()
        {         
        }

        /// <summary>
        /// Constructor de un avión
        /// </summary>
        /// <param name="acType">Aircraft Type</param>
        /// <param name="matricula">Matrícula del avión</param>
        /// <param name="semilla">Semilla para la generación de números aleatorios</param>
        public Avion(string acType, string subFlota, string matricula)
        {
            this._subFlota = subFlota;
            this._matricula = null;
            this._ac_type = acType;
            this._id_avion = matricula;
            this._tipo_avion = TipoAvion.Normal;
            this._slots_mantenimiento = new SerializableList<SlotMantenimiento>();
            this._legs = new SerializableList<Tramo>();
            this._eventos_avion = new List<Evento>();
            this._recien_aterrizado = true;
            this.Ultimo_Tramo_Agregado = null;
            this._recovery_reciente = false;
            this._pares_od_programados = new SerializableList<string>();
            this._hizo_recovery = false;
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Permite agregar un nuevo tramo
        /// </summary>
        /// <param name="tramoNuevo">Tramo nuevo a agregar</param>
        public void AgregarTramoEnOrden(Tramo tramoNuevo)
        {
            this._legs.Add(tramoNuevo);
            this._pares_od_programados.Add(tramoNuevo.ParOD);
            if (this.Ultimo_Tramo_Agregado == null) //Si se agrega un tramo por primera vez
            {
                Tramo_Raiz = tramoNuevo;
                Tramo_Actual = Tramo_Raiz;
                this.Ultimo_Tramo_Agregado = Tramo_Actual;
                //Para el primer tramo se crea el primer evento de iniciar tramo
                _eventos_avion.Add(new Evento(TipoEvento.InicioTramo, Tramo_Raiz.TInicialProg, new MetodoEventoEventHandler(IniciarTramo)));

            }
            else
            {
                Tramo_Actual.Tramo_Siguiente = tramoNuevo;
                Tramo_Actual = Tramo_Actual.Tramo_Siguiente;
                this.Ultimo_Tramo_Agregado = Tramo_Actual;
            }
        }

        /// <summary>
        /// Crea la secuencia de slots programados para el avión desde el "tramoInicial"
        /// </summary>
        /// <param name="tramoInicial">Tramo desde donde se comenzarán a crear los slots</param>
        /// <param name="minutosFinalUltimoSlot">Minutos donde se termina el itinerario</param>
        public void CrearSlots(Tramo tramoInicial, int minutosFinalUltimoSlot)
        {
            if (tramoInicial != null)
            {
                Tramo auxTramo = tramoInicial;
                //Se crea slot inicial
                //_slot_raiz = new Slot(tramoInicial.TramoBase.Origen, auxTramo.IdAvionProgramado, 0, tramoInicial.TInicialProg, tramoInicial.GetTurnAroundMin(tramoInicial),tramoInicial.TramoBase.Fecha_Salida.AddDays(-1),tramoInicial.TramoBase.Fecha_Salida,"0000",tramoInicial.TramoBase.Hora_Salida);
                _slot_raiz = new Slot(tramoInicial.TramoBase.Origen, auxTramo.IdAvionProgramado, tramoInicial.TInicialProg, tramoInicial.TInicialProg, tramoInicial.TurnAroundMinimoOrigen, tramoInicial.TramoBase.Fecha_Salida, tramoInicial.TramoBase.Fecha_Salida, tramoInicial.TramoBase.Hora_Salida, tramoInicial.TramoBase.Hora_Salida, tramoInicial);
                _slot_raiz.SlotPrevio = null;
                _slot_actual = _slot_raiz;
                while (auxTramo.Tramo_Siguiente != null)
                {
                    auxTramo = auxTramo.Tramo_Siguiente;
                    _slot_actual.SlotSiguiente = new Slot(auxTramo.TramoBase.Origen, auxTramo.IdAvionProgramado, auxTramo.Tramo_Previo.TFinalProg, auxTramo.TInicialProg, auxTramo.TurnAroundMinimoOrigen, auxTramo.Tramo_Previo.TramoBase.Fecha_Llegada, auxTramo.TramoBase.Fecha_Salida, auxTramo.Tramo_Previo.TramoBase.Hora_Llegada, auxTramo.TramoBase.Hora_Salida, auxTramo);
                    _slot_actual.SlotSiguiente.SlotPrevio = _slot_actual;
                    _slot_actual = _slot_actual.SlotSiguiente;
                }
                //Par el último slot se asume cierta cantidad de minutos donde termina el itinerario
                //corregir.. puede que no importe.
                //_slot_actual.SlotSiguiente = new Slot(auxTramo.TramoBase.Destino, auxTramo.IdAvionProgramado, auxTramo.TFinalProg, minutosFinalUltimoSlot, auxTramo.GetTurnAroundMin(auxTramo), auxTramo.TramoBase.Fecha_Llegada, auxTramo.TramoBase.Fecha_Llegada.AddDays(1), auxTramo.TramoBase.Hora_Llegada, "0000");
                _slot_actual.SlotSiguiente = new Slot(auxTramo.TramoBase.Destino, auxTramo.IdAvionProgramado, auxTramo.TFinalProg, auxTramo.TFinalProg, auxTramo.TurnAroundMinimoOrigen, auxTramo.TramoBase.Fecha_Llegada, auxTramo.TramoBase.Fecha_Llegada, auxTramo.TramoBase.Hora_Llegada, auxTramo.TramoBase.Hora_Llegada, null);
                _slot_actual.SlotSiguiente.SlotPrevio = _slot_actual;
                _slot_actual.SlotSiguiente.SlotSiguiente = null;
                _slot_actual = _slot_raiz;
            }
            else
            {
                _slot_actual = null;
                _slot_raiz = null;
            }
        }

        /// <summary>
        /// Retorna una lista con los slots programados para el avión
        /// </summary>
        /// <returns>Lista con los slots del avión</returns>
        public List<Slot> ObtenerListaSlots()
        {
            _slot_raiz = null;
            CrearSlots(Tramo_Raiz, 150000);
            List<Slot> listaSlots = new List<Slot>();
            Slot slotAux = _slot_raiz;
            if (slotAux != null)
            {
                while (slotAux != null)
                {
                    listaSlots.Add(slotAux);
                    slotAux = slotAux.SlotSiguiente;
                }
            }
            return listaSlots;
        }

        /// <summary>
        /// Retorna una lista con los tramos programados para el avión
        /// </summary>
        /// <param name="tramoRaiz">Tramo inicial</param>
        /// <returns>Lista con los tramos programados del avión</returns>
        public List<Tramo> ObtenerListaTramos(Tramo tramoRaiz)
        {
            List<Tramo> listaTramos = new List<Tramo>();
            Tramo tramoAux = tramoRaiz;
            if (tramoAux != null)
            {
                while (tramoAux != null)
                {
                    listaTramos.Add(tramoAux);
                    tramoAux = tramoAux.Tramo_Siguiente;
                }
            }
            return listaTramos;
        }
        
        /// <summary>
        /// ToString
        /// </summary>
        /// <returns>String identificador del avión</returns>
        public override string ToString()
        {
            return _id_avion.ToString();
        }

        #region ICloneable Members

        /// <summary>
        /// Crea una copia del avión
        /// </summary>
        /// <returns>Objeto avión</returns>
        public object Clone()
        {
            Avion clonado = new Avion(this.AcType,this._subFlota, this._id_avion);

            return clonado;
        }

        #endregion

        #endregion

        #region INTERNAL METHODS
        
        /// <summary>
        /// Actualiza los eventos de un avión luego del algoritmo de recovery
        /// </summary>    
        internal void ActualizarListaEventos()
        {
            if (Tramo_Actual != null)
            {
                if (Tramo_Actual.Estado == EstadoTramo.NoIniciado)
                {
                    Evento nuevoEvento = new Evento(TipoEvento.InicioTramo, Tramo_Actual.TInicialRst, new MetodoEventoEventHandler(IniciarTramo));
                    ReemplazarEventoDelMismoTipo(nuevoEvento);
                }
                else if (Tramo_Actual.Estado == EstadoTramo.Finalizado)
                {
                    if (Tramo_Actual.MantenimientoPosterior != null)
                    {
                        Evento nuevoEvento = new Evento(TipoEvento.InicioMantenimiento, Tramo_Actual.MantenimientoPosterior.TiempoInicioManttoRst, new MetodoEventoEventHandler(Tramo_Actual.MantenimientoPosterior.IniciarMantenimientoProgramado));
                        ReemplazarEventoDelMismoTipo(nuevoEvento);
                    }
                    else if (Tramo_Actual.Tramo_Siguiente != null)
                    {
                        Evento nuevoEvento = new Evento(TipoEvento.InicioTramo, Tramo_Actual.Tramo_Siguiente.TInicialRst, new MetodoEventoEventHandler(IniciarTramo));
                        ReemplazarEventoDelMismoTipo(nuevoEvento);
                    }
                }
            }
            else
            {
                _eventos_avion.Clear();
            }
        }

        /// <summary>
        /// Método para estimar la utilización de un intervalo de tiempo
        /// </summary>
        /// <param name="ini">Inicio intervalo</param>
        /// <param name="fin">Fin intervalo</param>
        /// <param name="minutos_vuelo">Minutos de vuelo</param>
        /// <param name="minutos_turn_around_internos">Minutos de turn around de los tramos internos del intervalo</param>
        /// <param name="minutos_turn_around_totales">Minutos de turn around totales, incluyendo los extremos del intervalo</param>
        internal void CalcularUtilizacionEnIntervalo(int ini, int fin, out int minutos_vuelo, out int minutos_turn_around_internos, out int minutos_turn_around_totales)
        {
            minutos_vuelo = 0;
            minutos_turn_around_internos = 0;
            minutos_turn_around_totales = 0;
            Tramo tramoAux = Tramo_Raiz;
            bool primerTramo = true;
            bool ultimoTramo = false;
            while (!(tramoAux == null || tramoAux.TInicialRst > fin))
            {
                if (tramoAux.TFinalRst > ini)
                {
                    int inicio_conteo = Math.Max(ini, tramoAux.TInicialRst);
                    int fin_conteo = Math.Min(fin, tramoAux.TFinalRst);
                    minutos_vuelo += fin_conteo - inicio_conteo;
                    int turnAround = tramoAux.TurnAroundMinimoOrigen;
                    if (tramoAux.Tramo_Siguiente != null && tramoAux.Tramo_Siguiente.TInicialRst > fin)
                    {
                        ultimoTramo = true;
                    }
                    if (primerTramo || ultimoTramo)
                    {
                        minutos_turn_around_totales += turnAround;
                        primerTramo = false;
                    }
                    else
                    {
                        minutos_turn_around_totales += turnAround;
                        minutos_turn_around_internos += turnAround;
                    }
                }
                tramoAux = tramoAux.Tramo_Siguiente;
            }
        }

        /// <summary>
        /// Crea una copia del avión
        /// </summary>
        /// <param name="semilla">Semilla para la generación de números aleatorios</param>
        /// <returns>Avión clonado</returns>
        internal virtual Avion Clonar(int semilla)
        {
            Avion clonado = (Avion)this.Clone();
            clonado._matricula = this._matricula;
            clonado._pares_od_programados = this._pares_od_programados;
            clonado._grupo = this._grupo;
            clonado.ClonarTramos(this, semilla);            
            return clonado;
        }

        /// <summary>
        /// Retorna el tramo programado más próximo a continuación de tiempo_fin_slot
        /// </summary>
        /// <param name="tiempo_fin_slot">Tiempo en minutos de búsqueda</param>
        internal Tramo GetTramoMasCercanoA(int tiempo_fin_slot)
        {
            Tramo tramoAux = Tramo_Actual;
            if (tramoAux != null && tramoAux.TInicialRst > tiempo_fin_slot)
            {
                return tramoAux;
            }
            while (tramoAux != null && tramoAux.Tramo_Siguiente != null)
            {
                if (tramoAux.Tramo_Siguiente.TInicialRst > tiempo_fin_slot)
                {
                    return tramoAux.Tramo_Siguiente;
                }
                tramoAux = tramoAux.Tramo_Siguiente;
            }
            return null;
        }

        /// <summary>
        /// Método que entrega una lista con los tramos programados del avión que vienen después de tiempo_programacion minutos.
        /// </summary>
        /// <param name="tiempo_programacion">Minutos a partir de los cuales se obtiene la lista</param>
        /// <returns>Lista de tramos</returns>
        internal List<Tramo> ObtenerListaTramosDespuesDe(int tiempo_programacion)
        {
            List<Tramo> listaTramos = new List<Tramo>();
            Tramo tramoAux = this.Tramo_Raiz;
            if (tramoAux != null)
            {
                while (tramoAux != null && tramoAux.TFinalProg <= tiempo_programacion)
                {
                    tramoAux = tramoAux.Tramo_Siguiente;
                }
                while (tramoAux != null)
                {
                    listaTramos.Add(tramoAux);
                    tramoAux = tramoAux.Tramo_Siguiente;
                }
            }
            return listaTramos;
        }

        /// <summary>
        /// Indica si el avión tiene algún tramo con cierto par origen-destino
        /// </summary>
        /// <param name="tramo_od">Par origen-destino</param>
        /// <returns>True si el avión contiene tramo_od</returns>
        internal bool OperaEntre(string tramo_od)
        {
            return _pares_od_programados.Contains(tramo_od);
        }

        #endregion
        
        #region PRIVATE METHODS

        /// <summary>
        /// Actualiza las causas de atraso para un tramo
        /// </summary>
        /// <param name="tramo"></param>
        private void ActualizarCausasDeAtraso(Tramo tramo)
        {
            if (tramo.TInicialProg == tramo.TInicialRst)
            {
                tramo.CausasAtraso.Clear();
            }
            else
            {
                int sumaAtrasos = 0;
                foreach (TipoDisrupcion c in tramo.CausasAtraso.Keys)
                {
                    sumaAtrasos += tramo.CausasAtraso[c];
                }
                int atrasoReal = tramo.TInicialRst - tramo.TInicialProg;
                if (atrasoReal < sumaAtrasos)
                {
                    int diferencia = sumaAtrasos - atrasoReal;
                    if (tramo.CausasAtraso.ContainsKey(TipoDisrupcion.RC) && tramo.CausasAtraso.ContainsKey(TipoDisrupcion.HBT) && tramo.CausasAtraso.Count == 2)
                    {
                        if (tramo.CausasAtraso[TipoDisrupcion.RC] > diferencia)
                        {
                            tramo.CausasAtraso[TipoDisrupcion.RC] -= diferencia;
                        }
                        else if (tramo.CausasAtraso[TipoDisrupcion.RC] == diferencia)
                        {
                            tramo.CausasAtraso.Remove(TipoDisrupcion.RC);
                        }
                        else if (tramo.CausasAtraso[TipoDisrupcion.RC] < diferencia)
                        {
                            tramo.CausasAtraso.Remove(TipoDisrupcion.RC);
                            tramo.CausasAtraso[TipoDisrupcion.HBT] = atrasoReal;
                        }
                    }
                    else if (tramo.CausasAtraso.ContainsKey(TipoDisrupcion.RC) && tramo.CausasAtraso.Count == 1)
                    {
                        tramo.CausasAtraso[TipoDisrupcion.RC] -= diferencia;
                    }
                    else if (tramo.CausasAtraso.ContainsKey(TipoDisrupcion.HBT) && tramo.CausasAtraso.Count == 1)
                    {
                        tramo.CausasAtraso[TipoDisrupcion.HBT] -= diferencia;
                    }
                }
                else if (atrasoReal > 0 && atrasoReal > sumaAtrasos)
                {
                    if (tramo.CausasAtraso.ContainsKey(TipoDisrupcion.RC))
                    {
                        tramo.CausasAtraso[TipoDisrupcion.RC] += atrasoReal - sumaAtrasos;
                    }
                    else
                    {
                        tramo.AgregarCausaDeAtraso(TipoDisrupcion.RC, atrasoReal - sumaAtrasos);
                    }
                }
            }
        }

        /// <summary>
        /// Ejecuta el evento de aterrizaje para un tramo de vuelo
        /// </summary>
        /// <returns>Controlador para la actualización del tramoActual en clase "Simulación"</returns>
        private bool Aterrizar()
        {
            //Actualiza estado del tramo
            Tramo_Actual.Estado = EstadoTramo.Finalizado;
            //Actualiza tiempo de simulación
            _actualizar_tiempo_simulacion(Tramo_Actual.TFinalRst);
            //Actualiza aeropuerto del avión
            _aeropuerto_actual = GetAeropuerto(Tramo_Actual.TramoBase.Destino);
            //Limpia la lista de eventos
            _eventos_avion.Clear();

            //Si hay algún mantenimiento posterior agrega eventos para la ejecución del mantenimiento
            if (Tramo_Actual.MantenimientoPosterior != null)
            {
                if (Tramo_Actual.MantenimientoPosterior.TramoPrevio == null)
                {

                }
                Tramo_Actual.MantenimientoPosterior.Desplazar(Tramo_Actual.TFinalRst - Tramo_Actual.TFinalProg);
                _eventos_avion.Add(new Evento(TipoEvento.InicioMantenimiento, Tramo_Actual.MantenimientoPosterior.TiempoInicioManttoRst, new MetodoEventoEventHandler(Tramo_Actual.MantenimientoPosterior.IniciarMantenimientoProgramado)));
            }

            _hizo_recovery = false;
            int atrasoReaccionario = _atraso_despegue + _atraso_vuelo;
            if (atrasoReaccionario > 0)
            {   //Si hay atraso reaccionario se mueven los tramos posteriores y se hacen intentos de recovery
                MoverLegs(_atraso_vuelo + _atraso_despegue, _recovery);
            }
            else if (Tramo_Actual.Tramo_Siguiente != null)
            {
                //Si no hay atraso reaccionario se reordenan los tramos
                OrdenarTramos(Tramo_Actual.Tramo_Siguiente);
            }

            if (_hizo_recovery)
            {
                //Si se hizo recovery se crea un evento de inicio sobre el tramo actual (actualizado por el recovery)
                if (Tramo_Actual != null)
                {
                    ReemplazarEventoDelMismoTipo(new Evento(TipoEvento.InicioTramo, Tramo_Actual.TInicialRst, new MetodoEventoEventHandler(IniciarTramo)));
                }
            }
            else
            {
                //Si no se hizo recovery se crea un evento de inicio para el tramo siguiente (si existe)
                if (Tramo_Actual.Tramo_Siguiente != null)
                {
                    ReemplazarEventoDelMismoTipo(new Evento(TipoEvento.InicioTramo, Tramo_Actual.Tramo_Siguiente.TInicialRst, new MetodoEventoEventHandler(IniciarTramo)));
                }
            }
            _recien_aterrizado = true;
            return !_hizo_recovery;
        }

        /// <summary>
        /// Clona la lista de tramos del avión "referencia" y los asigna al avión actual
        /// </summary>
        /// <param name="referencia">Avión de donde se toman los tramos a clonar</param>
        /// <param name="semilla">Semilla para la generación de números aleatorios</param>
        private void ClonarTramos(Avion referencia, int semilla)
        {
            Tramo prev = null;
            bool primeraVez = true;
            List<Tramo> listaAux = referencia._legs.ToList();
            listaAux.Sort();
            foreach (Tramo t in listaAux)
            {
                Tramo aux = (Tramo)t.Clone();
                aux.PonerSemilla(semilla);
                aux.Tramo_Previo = prev;
                aux.Tramo_Siguiente = null;
                if (prev != null)
                {
                    prev.Tramo_Siguiente = aux;
                }
                prev = aux;
                this._legs.Add(aux);
                if (primeraVez)
                {
                    primeraVez = false;
                    Tramo_Actual = aux;
                    Tramo_Raiz = Tramo_Actual;
                    _eventos_avion.Add(new Evento(TipoEvento.InicioTramo, Tramo_Raiz.TInicialProg, new MetodoEventoEventHandler(IniciarTramo)));
                }
            }
        }

        /// <summary>
        /// Ejecuta el despegue de un tramo de vuelo
        /// </summary>
        /// <returns>Controlador para la actualización del tramoActual en clase "Simulación"</returns>
        private bool Despegar()
        {
            //Actualiza tiempo de despegue del tramo
            Tramo_Actual.TInicialRst += _atraso_despegue;
            Tramo_Actual.TFinalRst += _atraso_despegue;
            //Actualiza tiempo de aterrizaje del tramo
            Tramo_Actual.TFinalRst += _atraso_vuelo;
            //Actualiza tiempo de simulación
            _actualizar_tiempo_simulacion(Tramo_Actual.TInicialRst);
            //Actualiza estado de tramo
            Tramo_Actual.Estado = EstadoTramo.EnProceso;
            _recien_aterrizado = false;

            //Actualiza tiempos de slots de mantenimiento
            if (Tramo_Actual.Tramo_Previo != null)
            {
                SlotMantenimiento slotAux = Tramo_Actual.Tramo_Previo.MantenimientoPosterior;
                if (slotAux != null)
                {
                    slotAux.TiempoFin = Tramo_Actual.TInicialRst;
                    slotAux.Duracion = slotAux.TiempoFin - slotAux.TiempoInicio;
                }
            }

            //Elimina evento de despegue de la lista
            _eventos_avion.RemoveAt(0);
            return false;
        }

        /// <summary>
        /// Entrega el posible atraso reaccionario por vuelos conectados por pairings y pasajeros
        /// </summary>
        /// <param name="puedeDespegar">Indica si el tramo actual podrá ser iniciado</param>
        /// <param name="causaAtrasoConexion">Indica la causa de atraso predominante que será registrada</param>
        /// <returns>Minutos de atraso reaccionario por conexiones</returns>
        private int EstimarAtrasoReaccionarioConexiones(out bool puedeDespegar, out TipoDisrupcion causaAtrasoConexion)
        {
            //Caso de doble conexión: pasajeros y pairing: se estima primero el atraso por pairing, luego en función 
            //de este, se actualiza el atraso y se estiman los vuelos en conexión que se pierden.
            if (Tramo_Actual.EsperaTramoPorConexionPairing && Tramo_Actual.EsperaTramoPorConexionPasajeros)
            {
                int atraso_total = 0;
                causaAtrasoConexion = TipoDisrupcion.RC_TRIP;
                SerializableList<ConexionLegs> conexionesPairings = Tramo_Actual.ConexionesPairingAnteriores;// Tramo_Actual.GetConexion(Tramo_Actual.TramoBase.Numero_Global, TipoConexion.Pairing, true);
                ConexionLegs conexionPairing = ConexionLegs.BuscaConexionCriticaPairings(conexionesPairings);
                Tramo tramo_previo_conex = conexionPairing.GetTramo(conexionPairing.NumTramoIni);
                puedeDespegar = tramo_previo_conex.PuedeDespegar;
                int tiempoResultanteActualDespegue = Tramo_Actual.TInicialRst;
                int turnAroundTramoActual = Tramo_Actual.TurnAroundMinimoOrigen;
                int turnAroundConexion = ConexionPairing.TIEMPO_CAMBIO_AVION;
                int tiempoFinalTramoPrevio = tramo_previo_conex.TiempoFinalResultanteEstimado();
                int atrasoPairing = Math.Max(0, tiempoFinalTramoPrevio + turnAroundConexion - tiempoResultanteActualDespegue);
                atraso_total = atrasoPairing;
                //Si no puede despegar, los vuelos en conexión por pasajeros quedan pendientes.
                if (!puedeDespegar)
                {
                    return atrasoPairing;
                }
                //Si puede despegar, se revisan los vuelos en conexión por pasajeros.
                else
                {
                    SerializableList<ConexionLegs> conexionesPasajeros = Tramo_Actual.ConexionesPaxAnteriores;// Tramo_Actual.GetConexion(Tramo_Actual.TramoBase.Numero_Global, TipoConexion.Pasajeros, true);
                    int minutos_proximo_vuelo = Tramo_Actual.GetMinutosProximoVuelo(Tramo_Actual);
                    int turn_around_conexion_pax = GetAeropuerto(Tramo_Actual.TramoBase.Origen).Minutos_Conexion_Pax;
                    tiempoResultanteActualDespegue = Tramo_Actual.TInicialRst;
                    List<ConexionesConTiempoMaximoEspera> conexiones_con_minutos_espera = new List<ConexionesConTiempoMaximoEspera>();
                    foreach (ConexionLegs con in conexionesPasajeros)
                    {
                        int max_minutos_espera_aceptados = con.GetEspera(minutos_proximo_vuelo);
                        conexiones_con_minutos_espera.Add(new ConexionesConTiempoMaximoEspera(con, max_minutos_espera_aceptados));
                    }
                    conexiones_con_minutos_espera.Sort();
                    foreach (ConexionesConTiempoMaximoEspera con in conexiones_con_minutos_espera)
                    {
                        tramo_previo_conex = con._conexion_base.GetTramo(con._conexion_base.NumTramoIni);
                        tiempoFinalTramoPrevio = tramo_previo_conex.TiempoFinalResultanteEstimado();
                        int atrasoEsperado = Math.Max(0, tiempoFinalTramoPrevio + turn_around_conexion_pax - tiempoResultanteActualDespegue);
                        //El tiempo de espera máxima se eleva al nivel del atraso de pairing.
                        if (atrasoEsperado > Math.Max(con._tiempo_maximo_espera, atrasoPairing))
                        {
                            //Se pierde la conexion
                        }
                        else
                        {
                            //Se actualiza el atraso total y se cambia la causa de atraso a reaccionario de pasajeros
                            if (atrasoEsperado > atraso_total)
                            {
                                atraso_total = atrasoEsperado;
                                causaAtrasoConexion = TipoDisrupcion.RC_PAX;
                            }
                        }
                    }

                }
                return atraso_total;
            }
            else if (Tramo_Actual.EsperaTramoPorConexionPairing)
            {
                //Obtiene conexión y tramo previo de conexión
                SerializableList<ConexionLegs> conexiones = Tramo_Actual.ConexionesPairingAnteriores;// Tramo_Actual.GetConexion(Tramo_Actual.TramoBase.Numero_Global, TipoConexion.Pairing, true);
                ConexionLegs conexion = ConexionLegs.BuscaConexionCriticaPairings(conexiones);
                Tramo tramo_previo_conex = conexion.GetTramo(conexion.NumTramoIni);
                //Se determina si se puede despegar en función del estado del tramo anterior.
                puedeDespegar = tramo_previo_conex.PuedeDespegar;
                //Estima atraso reaccionario de tripulacion
                int tiempoResultanteActualDespegue = Tramo_Actual.TInicialRst;
                int turnAroundTramoActual = Tramo_Actual.TurnAroundMinimoOrigen;
                int turnAroundConexion = ConexionPairing.TIEMPO_CAMBIO_AVION;
                int tiempoFinalTramoPrevio = tramo_previo_conex.TiempoFinalResultanteEstimado();
                int atraso = Math.Max(0, tiempoFinalTramoPrevio + turnAroundConexion - tiempoResultanteActualDespegue);
                causaAtrasoConexion = TipoDisrupcion.RC_TRIP;
                return atraso;
            }
            else if (Tramo_Actual.EsperaTramoPorConexionPasajeros)
            {
                //Se asume que siempre se podrá despegar. Lo que variará es el atraso.
                puedeDespegar = true;
                SerializableList<ConexionLegs> conexiones = Tramo_Actual.ConexionesPaxAnteriores;// Tramo_Actual.GetConexion(Tramo_Actual.TramoBase.Numero_Global, TipoConexion.Pasajeros, true);
                int minutos_proximo_vuelo = Tramo_Actual.GetMinutosProximoVuelo(Tramo_Actual);
                int turn_around_conexion_pax = GetAeropuerto(Tramo_Actual.TramoBase.Origen).Minutos_Conexion_Pax;
                int atraso_total = 0;
                int tiempoResultanteActualDespegue = Tramo_Actual.TInicialRst;
                List<ConexionesConTiempoMaximoEspera> conexiones_con_minutos_espera = new List<ConexionesConTiempoMaximoEspera>();
                foreach (ConexionLegs con in conexiones)
                {
                    int max_minutos_espera_aceptados = con.GetEspera(minutos_proximo_vuelo);
                    conexiones_con_minutos_espera.Add(new ConexionesConTiempoMaximoEspera(con, max_minutos_espera_aceptados));
                }
                conexiones_con_minutos_espera.Sort();
                foreach (ConexionesConTiempoMaximoEspera con in conexiones_con_minutos_espera)
                {
                    Tramo tramo_previo_conex = con._conexion_base.GetTramo(con._conexion_base.NumTramoIni);
                    int tiempoFinalTramoPrevio = tramo_previo_conex.TiempoFinalResultanteEstimado();
                    int atrasoEsperado = Math.Max(0, tiempoFinalTramoPrevio + turn_around_conexion_pax - tiempoResultanteActualDespegue);
                    if (atrasoEsperado > con._tiempo_maximo_espera)
                    {
                        //Se pierde la conexion
                    }
                    else
                    {
                        if (atrasoEsperado > atraso_total)
                        {
                            atraso_total = atrasoEsperado;
                        }
                    }
                }
                causaAtrasoConexion = TipoDisrupcion.RC_PAX;
                puedeDespegar = true;
                return atraso_total;
            }
            else
            {
                puedeDespegar = true;
                causaAtrasoConexion = TipoDisrupcion.RC;
                return 0;
            }
        }

        /// <summary>
        /// Inicia la operación de un tramo de vuelo. Genera atrasos para las distintas disrupciones.
        /// </summary>
        /// <returns>Controlador para la actualización del tramoActual en clase "Simulación"</returns>
        private bool IniciarTramo()
        {
            //Inicializa
            int adelanto = 0;
            int holguraMinutos = 0;
            int turnAround = 0;
            _atraso_vuelo = 0;
            _atraso_despegue = 0;

            //Actualiza tiempo de simulación
            _actualizar_tiempo_simulacion(Tramo_Actual.TInicialRst);

            //Actualiza variables de estado y de resultado de tramoActual y avión
            _aeropuerto_actual = GetAeropuerto(Tramo_Actual.TramoBase.Origen);//Ejecutar delegado desde clase itinerario
            Tramo_Actual.IdAvionOperado = this._id_avion;
            Tramo_Actual.FlotaOperada = GetFlota(this._ac_type);
            _recien_aterrizado = false;
            if (Tramo_Actual != null && Tramo_Actual.Tramo_Siguiente != null)
            {
                turnAround = Tramo_Actual.TurnAroundMinimoDestino;
            }

            //Registra atraso reaccionario (si existiera)
            if (Tramo_Actual.IdAvionProgramado != this._id_avion || _recovery_reciente || Tramo_Actual.TramoPostCadenaSwap || Tramo_Actual.HayMantenimientoAnterior)
            {
                if (Tramo_Actual.TInicialProg != Tramo_Actual.TInicialRst)
                {
                    //Se supone que el atrasos reaccionario es la diferencia entre el atraso efectivo 
                    //y la suma de los atrasos por otras causas.
                    int otrosAtrasos = 0;
                    foreach (TipoDisrupcion c in Tramo_Actual.CausasAtraso.Keys)
                        otrosAtrasos += Tramo_Actual.CausasAtraso[c];
                    Tramo_Actual.AgregarCausaDeAtraso(TipoDisrupcion.RC, -Tramo_Actual.TInicialProg + Tramo_Actual.TInicialRst - otrosAtrasos);
                }
                else if(Tramo_Actual.CausasAtraso.Count>0)
                {
                    Tramo_Actual.CausasAtraso.Clear();
                }
            }
            _recovery_reciente = false;
            //Estima la holgura posterior
            if (Tramo_Actual.Tramo_Siguiente != null)
            {               
                holguraMinutos = Tramo_Actual.Tramo_Siguiente.TInicialRst - Tramo_Actual.TFinalRst - turnAround;
            }

            //**Si no puede despegar es porque el tramo previo en la conexion ni siquiera se ha iniciado. 
            //**Con esto, se debería trasladar el inicio del tramoActual al tiempo de aterrizaje a turn around del 
            //**tramo_previo de la conexion.
            TipoDisrupcion causaConexion;
            bool puedeDespegar;
            int atrasoReaccionarioConexiones = EstimarAtrasoReaccionarioConexiones(out puedeDespegar, out causaConexion);
            Tramo_Actual.AgregarCausaDeAtraso(causaConexion, atrasoReaccionarioConexiones);
            if (puedeDespegar)
            {
                //Se generan atrasos sobre tramo
                _atraso_despegue = GenerarDisrupcionDespeque() + atrasoReaccionarioConexiones;
                _atraso_vuelo = GenerarAtrasoHBT();
                //Se registran las causas reaccionarias sobre tramo siguiente al actual. 
                //Se considera que el HBT es una causa última reaccionaria.
                AgregarCausasReaccionarias(_atraso_vuelo, _atraso_despegue, holguraMinutos, Tramo_Actual);

                //Hay adelanto siempre que no haya atraso de ningun tipo.
                if (_atraso_despegue == 0 && Tramo_Actual.TInicialProg == Tramo_Actual.TInicialRst)
                {
                    //Se asume que no puede ocurrir un adelanto mayor a 60 minutos
                    adelanto = Math.Min(GenerarAdelanto(Tramo_Actual.TramoBase.Origen), ADELANTO_MAXIMO);

                    //regla excepcional para SCL: adelanto máximo es 5 minutos.
                    if (Tramo_Actual.TramoBase.Origen == "SCL")
                    {
                        adelanto = Math.Min(5, adelanto);
                    }

                    if (adelanto > 0)
                    {
                        //Hay un atraso de vuelo, pero el adelanto más la holgura amortiguan el atraso de vuel 
                        if (_atraso_vuelo > 0 && adelanto >= _atraso_vuelo - holguraMinutos)
                        {
                            //Si tenía un atraso HBT se elimina. El adelanto más la holgura lo hacen inválido 
                            //sobre el tramo posterior.
                            if (Tramo_Actual.Tramo_Siguiente != null && Tramo_Actual.Tramo_Siguiente.CausasAtraso.ContainsKey(TipoDisrupcion.HBT))
                                Tramo_Actual.Tramo_Siguiente.CausasAtraso.Remove(TipoDisrupcion.HBT);

                            //Si el tramo posterior tenía registrado un atraso reaccionario
                            if (Tramo_Actual.Tramo_Siguiente != null && Tramo_Actual.Tramo_Siguiente.CausasAtraso.ContainsKey(TipoDisrupcion.RC))
                            {
                                if (Tramo_Actual.Tramo_Siguiente.CausasAtraso[TipoDisrupcion.RC] - adelanto + _atraso_vuelo <= 0)
                                {
                                    //Si tenía atraso reaccionario se elimina. El adelanto amortigua la suma del 
                                    //atraso reaccionario (ya existente) más el atraso de vuelo.                                   
                                    Tramo_Actual.Tramo_Siguiente.CausasAtraso.Remove(TipoDisrupcion.RC);
                                }
                                else
                                {
                                    //Se actualiza el atraso reaccionario por el efecto del adelanto
                                    Tramo_Actual.Tramo_Siguiente.CausasAtraso[TipoDisrupcion.RC] -= adelanto - _atraso_vuelo;
                                }
                            }
                        }
                        //Si hay atraso de vuelo y adelanto, pero el atraso es mayor al adelanto
                        else if (_atraso_vuelo > 0 && adelanto > 0 && _atraso_vuelo - adelanto > 0)
                        {
                            //Se actualiza valor del atraso por HBT.
                            if (Tramo_Actual.Tramo_Siguiente != null && Tramo_Actual.Tramo_Siguiente.CausasAtraso.ContainsKey(TipoDisrupcion.HBT))
                                Tramo_Actual.Tramo_Siguiente.CausasAtraso[TipoDisrupcion.HBT] = _atraso_vuelo - adelanto - holguraMinutos;
                        }
                        _atraso_despegue = -adelanto;
                    }
                }

                //Se inicia recovery de turnos siempre que el tramo actual sea el primero en un pairing
                if (Tramo_Actual.IniciaConexionPairing)
                {
                    RecoveryTurnos(Tramo_Actual,_atraso_despegue, _atraso_vuelo);
                }

                //Actualización de eventos.
                //Se crean los eventos de despegue y aterrizaje.
                Tramo_Actual.Estado = EstadoTramo.Iniciado;
                _eventos_avion.RemoveAt(0);
                ReemplazarEventoDelMismoTipo(new Evento(TipoEvento.Despegue, Tramo_Actual.TInicialRst + _atraso_despegue, new MetodoEventoEventHandler(Despegar)));
                ReemplazarEventoDelMismoTipo(new Evento(TipoEvento.Aterrizaje, Tramo_Actual.TFinalRst + _atraso_despegue + _atraso_vuelo, new MetodoEventoEventHandler(Aterrizar)));
                _eventos_avion.Sort();
            }
            else
            {   //Cuando la conexión impide el despegue, se reprograma el inicio del tramo al resultante actual.
                Tramo_Actual.TInicialRst += atrasoReaccionarioConexiones;
                Tramo_Actual.TFinalRst += atrasoReaccionarioConexiones;
                Tramo_Actual.Estado = EstadoTramo.NoIniciado;
                _eventos_avion.RemoveAt(0);
                ReemplazarEventoDelMismoTipo(new Evento(TipoEvento.InicioTramo, Tramo_Actual.TInicialRst, new MetodoEventoEventHandler(IniciarTramo)));
                _eventos_avion.Sort();
            }
            return false;
        }

        /// <summary>
        /// Se mueven los tramos posteriores al actual, un atraso inicial de "atraso"
        /// </summary>
        /// <param name="atraso">Atraso inicial propagado a tramos posteriores</param>
        /// <param name="recovery">Delegado para ejecutar algoritmo de recovery</param>
        private void MoverLegs(int atraso, RecoveryEventHandler recovery)
        {
            int atrasoReaccionario = atraso;
            //Se posiciona sobre tramo siguiente al actual en operación
            bool esPrimerTramo = true;
            Tramo tramoObjetivo = Tramo_Actual.Tramo_Siguiente;
            _hizo_recovery = false;

            //Se avanza hacia tramos posteriores hasta el último si es que no se cumple alguna de las condiciones de término
            while (tramoObjetivo != null)
            {
                int turnAroundTime = tramoObjetivo.TurnAroundMinimoOrigen;
                //En este caso se puede reducir el atraso reaccionario ya que hay una holgura
                if (tramoObjetivo.TInicialRst - tramoObjetivo.TFinRstTramoPrevio > turnAroundTime)
                {
                    //Primera condición de término: el tramo objetivo no tiene atraso. Esto implica que las holguras han 
                    //consumido todo el atraso reaccionario.
                    if (tramoObjetivo.TInicialRst == tramoObjetivo.TInicialProg)
                        return;
                    //Se reduce el atraso reaccionario al máximo posible: al máximo entre el inicio programdo o al mínimo 
                    //tiempo de despegue después del tramo previo al tramo objetivo
                    else
                    {
                        int HBT_Resultante = tramoObjetivo.TFinalRst - tramoObjetivo.TInicialRst;
                        tramoObjetivo.TInicialRst = Math.Max(tramoObjetivo.TInicialProg, tramoObjetivo.TFinRstTramoPrevio + turnAroundTime);
                        tramoObjetivo.TFinalRst = tramoObjetivo.TInicialRst + HBT_Resultante;
                        //Se inicia recovery de turnos siempre que el tramo actual sea el primero en un pairing
                        if (tramoObjetivo.IniciaConexionPairing)
                        {
                            RecoveryTurnos(tramoObjetivo, 0, 0);
                        }
                        tramoObjetivo = tramoObjetivo.Tramo_Siguiente;
                    }
                }

                //En este caso hay un atraso que supera la tolerancia de recovery
                else if ((tramoObjetivo.TInicialRst - tramoObjetivo.TFinRstTramoPrevio) < (turnAroundTime - _tolerancia_recovery))
                {
                    //Se estima el atraso reaccionario
                    atrasoReaccionario = turnAroundTime + tramoObjetivo.TFinRstTramoPrevio - tramoObjetivo.TInicialRst;
                    Tramo previo = tramoObjetivo.Tramo_Previo;
                    //Se hacen esfuerzos de recovery
                    if (recovery(this,tramoObjetivo, atrasoReaccionario))
                    {
                        //Si el recovery es exitoso chequea el atraso reaccionario
                        int atrasoReaccionarioPostRecovery = tramoObjetivo.TInicialRst - tramoObjetivo.TInicialProg;
                        if (atrasoReaccionarioPostRecovery > 0)
                        {
                            //Si el es primer tramo posterior al tramo actual
                            if (esPrimerTramo)
                            {
                                //Si ya tenía HBT este se actualiza al mínimo entre éste y el reaccionario post-recovery.
                                if (tramoObjetivo.CausasAtraso.ContainsKey(TipoDisrupcion.HBT))
                                {
                                    //Se actualiza HBT
                                    tramoObjetivo.CausasAtraso[TipoDisrupcion.HBT] = Math.Min(tramoObjetivo.CausasAtraso[TipoDisrupcion.HBT], atrasoReaccionarioPostRecovery);

                                    //Si contenía atraso reaccionario se actualiza
                                    if (tramoObjetivo.CausasAtraso.ContainsKey(TipoDisrupcion.RC))
                                    {
                                        tramoObjetivo.CausasAtraso[TipoDisrupcion.RC] = Math.Max(atrasoReaccionarioPostRecovery - tramoObjetivo.CausasAtraso[TipoDisrupcion.HBT],0);
                                        //Si el atraso reaccionario da cero se elimina
                                        if (tramoObjetivo.CausasAtraso[TipoDisrupcion.RC] == 0)
                                        {
                                            tramoObjetivo.CausasAtraso.Remove(TipoDisrupcion.RC);
                                        }
                                    }
                                }
                                //Si contenía atraso reaccionario, se actualiza; si no, se agrega
                                else if (tramoObjetivo.CausasAtraso.ContainsKey(TipoDisrupcion.RC))
                                {
                                    //Se actualiza el atraso reaccionario
                                    tramoObjetivo.CausasAtraso[TipoDisrupcion.RC] = atrasoReaccionarioPostRecovery;
                                }
                                else
                                {
                                    //Se agrega el atraso reaccionario
                                    tramoObjetivo.AgregarCausaDeAtraso(TipoDisrupcion.RC, atrasoReaccionarioPostRecovery);
                                }
                            }
                            else
                            {
                                //Si el atraso reaccionario es positivo se registra
                                if (tramoObjetivo.CausasAtraso.ContainsKey(TipoDisrupcion.RC))
                                {
                                    //Se actualiza el atraso reaccionario
                                    tramoObjetivo.CausasAtraso[TipoDisrupcion.RC] = atrasoReaccionarioPostRecovery;
                                }
                                else
                                {
                                    //Se agrega el atraso reaccionario
                                    tramoObjetivo.AgregarCausaDeAtraso(TipoDisrupcion.RC, atrasoReaccionarioPostRecovery);
                                }
                                //if (tramoObjetivo.CausasAtraso.ContainsKey(CausasAtraso.HBT))
                                //{
                                //    tramoObjetivo.CausasAtraso.Remove(CausasAtraso.HBT);
                                //}
                            }

                            //Se inicia recovery de turnos siempre que el tramo actual sea el primero en un pairing
                            if (tramoObjetivo.IniciaConexionPairing)
                            {
                                RecoveryTurnos(tramoObjetivo, 0, 0);
                            }
                        }
                        else
                        {
                            //Si no hay atraso reaccionario, se eliminan las causas de atraso.
                            if (tramoObjetivo.CausasAtraso.ContainsKey(TipoDisrupcion.RC) || tramoObjetivo.CausasAtraso.ContainsKey(TipoDisrupcion.HBT))
                            {
                                tramoObjetivo.CausasAtraso.Clear();
                            }
                        }
                        _hizo_recovery = true;
                        tramoObjetivo = previo.Tramo_Siguiente;
                    }
                    //El recovery no es exitoso
                    else
                    {
                        //Se actualiza el atraso reaccionario
                        atrasoReaccionario = turnAroundTime + tramoObjetivo.TFinRstTramoPrevio - tramoObjetivo.TInicialRst;
                        //Se propaga el atraso
                        tramoObjetivo.TInicialRst += atrasoReaccionario;
                        tramoObjetivo.TFinalRst += atrasoReaccionario;
                        //Se inicia recovery de turnos siempre que el tramo actual sea el primero en un pairing
                        if (tramoObjetivo.IniciaConexionPairing)
                        {
                            RecoveryTurnos(tramoObjetivo, 0, 0);
                        }
                        tramoObjetivo = tramoObjetivo.Tramo_Siguiente;
                    }
                }
                //En este atraso hay un atraso bajo el mínimo de tolerancia. No se intenta recuperar turnos.
                else
                {
                    //Se actualiza atraso reaccionario
                    if (tramoObjetivo.Tramo_Previo != null)
                    {
                        atrasoReaccionario = turnAroundTime + tramoObjetivo.Tramo_Previo.TFinalRst - tramoObjetivo.TInicialRst;
                    }
                    else
                    {
                        atrasoReaccionario = tramoObjetivo.TInicialRst - tramoObjetivo.TInicialProg + atraso;
                    }
                    //Se propaga atraso
                    tramoObjetivo.TInicialRst += atrasoReaccionario;
                    tramoObjetivo.TFinalRst += atrasoReaccionario;
                    tramoObjetivo = tramoObjetivo.Tramo_Siguiente;
                }

                esPrimerTramo = false;
            }
        }

        /// <summary>
        /// Ordena los tramos, ajustando por tiempos de inicio y fin resultantes y T/A.
        /// </summary>
        /// <param name="tramoInicial">Tramo inicial desde donde se comienza a ordenar</param>
        private void OrdenarTramos(Tramo tramoInicial)
        {
            //Si el avión tiene tramos programados
            if (tramoInicial != null && !(tramoInicial.Estado != EstadoTramo.NoIniciado && tramoInicial.Tramo_Siguiente == null))
            {
                if (tramoInicial.Estado != EstadoTramo.NoIniciado)
                {
                    tramoInicial = tramoInicial.Tramo_Siguiente;
                }
                Tramo tramoAux;
                if (tramoInicial.Tramo_Previo != null)
                    tramoAux = tramoInicial.Tramo_Previo;
                else
                    tramoAux = tramoInicial;

                //Se recorren todos los tramos hasta el último
                while (tramoAux != null && tramoAux.Tramo_Siguiente != null)
                {
                    //No hay mantenimiento programado
                    if (tramoAux.MantenimientoPosterior == null)
                    {
                        //Se calculan los minutos de holgura ajustando el turn around al mínimo
                        int minutosLibres = tramoAux.Tramo_Siguiente.TInicialRst - tramoAux.Tramo_Siguiente.TFinRstTramoPrevio - tramoAux.Tramo_Siguiente.TurnAroundMinimoOrigen;
                        //Si hay minutos libres se ajusta el tiempo de despegue y aterrizaje
                        if (minutosLibres >= 0)
                        {
                            int minutosRecuperados = Math.Min(minutosLibres, tramoAux.Tramo_Siguiente.TInicialRst - tramoAux.Tramo_Siguiente.TInicialProg);
                            tramoAux.Tramo_Siguiente.TInicialRst = tramoAux.Tramo_Siguiente.TInicialRst - minutosRecuperados;
                            tramoAux.Tramo_Siguiente.TFinalRst = tramoAux.Tramo_Siguiente.TFinalRst - minutosRecuperados;
                        }
                        //Si no, se propaga un atraso reaccionario
                        else
                        {
                            tramoAux.Tramo_Siguiente.TInicialRst = tramoAux.Tramo_Siguiente.TInicialRst - minutosLibres;
                            tramoAux.Tramo_Siguiente.TFinalRst = tramoAux.Tramo_Siguiente.TFinalRst - minutosLibres;
                        }

                        //Se actualizan las causas de atraso
                        ActualizarCausasDeAtraso(tramoAux.Tramo_Siguiente);                        
                    }

                    //Si hay un mantenimiento programado antes del siguiente tramo
                    else
                    {
                        SlotMantenimiento slotMantto = tramoAux.MantenimientoPosterior;
                        int turnAround = tramoAux.MantenimientoPosterior.TurnAroundMinimo;
                        slotMantto.TiempoInicio = tramoAux.TFinalRst;
                        //Primero: se actualiza programación del mantenimiento
                        slotMantto.TiempoInicioManttoRst = Math.Max(slotMantto.TiempoInicio + turnAround, slotMantto.TiempoInicioManttoPrg);
                        slotMantto.TiempoFinManttoRst = slotMantto.TiempoInicioManttoRst + slotMantto.DuracionMantenimiento;
                        //Luego, se actualiza el tramo posterior al mantenimiento                        
                        int minutosLibres = tramoAux.Tramo_Siguiente.TInicialRst - Math.Max(slotMantto.TiempoFinManttoRst + turnAround, tramoAux.Tramo_Siguiente.TInicialProg);
                        tramoAux.Tramo_Siguiente.TInicialRst = tramoAux.Tramo_Siguiente.TInicialRst - minutosLibres;
                        tramoAux.Tramo_Siguiente.TFinalRst = tramoAux.Tramo_Siguiente.TFinalRst - minutosLibres;
                        //Se actualizan los tiempos del slot de mantenimiento
                        slotMantto.TiempoFin = tramoAux.Tramo_Siguiente.TInicialRst;
                        slotMantto.Duracion = slotMantto.TiempoFin - slotMantto.TiempoInicio;
                        ActualizarCausasDeAtraso(tramoAux.Tramo_Siguiente);
                    }
                    tramoAux = tramoAux.Tramo_Siguiente;
                }
            }
        }

        /// <summary>
        /// Método para recovery de turnos.
        /// </summary>
        /// <param name="_atraso_despegue">Atraso de despegue del tramo actual</param>
        /// <param name="_atraso_vuelo">Atraso de vuelo del tramo actual</param>
        private void RecoveryTurnos(Tramo tramoObjetivo, int _atraso_despegue, int _atraso_vuelo)
        {
            //Primero se obtiene el atraso del tramo conectado
            int tiempo_fin_resultante = tramoObjetivo.TFinalRst + _atraso_despegue + _atraso_vuelo;
            SerializableList<ConexionLegs> conexionesPairings = tramoObjetivo.ConexionesPairingPosteriores;
            ConexionLegs conexion = conexionesPairings[0];
            Tramo tramoSiguiente = conexion.GetTramo(conexion.NumTramoFin);
            int tiempo_ini_sin_conexion = tramoSiguiente.TiempoInicialResultanteEstimado();
            int turn_around_conexion = ConexionPairing.TIEMPO_CAMBIO_AVION;
            int atraso_potencial = tiempo_fin_resultante + turn_around_conexion - tiempo_ini_sin_conexion;

            if (atraso_potencial > ToleranciaTurno)
            {
                //Si el atraso es mayor a la tolerancia se estima el atraso que resultaría al activar un turno.
                int tiempo_desplazo_turno = GetAeropuerto(tramoObjetivo.TramoBase.Destino).Minutos_Llega_Turno;
                int tiempo_decision =  tramoObjetivo.TInicialProg+ _atraso_despegue;
                int tiempo_llegada_turno = tiempo_desplazo_turno + tiempo_decision;
                int atraso_por_cambio_turno = Math.Max(0, tiempo_llegada_turno - tiempo_ini_sin_conexion);

                if (atraso_potencial - atraso_por_cambio_turno > ToleranciaTurno)
                {
                    //Si el atraso potencialmente reducido es mayor a la tolerancia se intenta usar el turno vión de backup
                    UsarTurnoBackup(tramoSiguiente, atraso_por_cambio_turno);         
                }
            }
        }

        /// <summary>
        /// Agrega un evento. Si existe un evento del mismo tipo, lo saca.
        /// </summary>
        /// <param name="nuevoEvento">Evento a agregar</param>
        private void ReemplazarEventoDelMismoTipo(Evento nuevoEvento)
        {
            int indexReemplazo = -1;
            int contador = 0;
            foreach (Evento e in _eventos_avion)
            {
                if (e.esDelMismoTipo(nuevoEvento))
                {
                    indexReemplazo = contador;
                }
                contador++;
            }
            if (indexReemplazo != -1)
            {
                _eventos_avion.RemoveAt(indexReemplazo);
                _eventos_avion.Add(nuevoEvento);
            }
            else
            {
                _eventos_avion.Add(nuevoEvento);
            }
        }

        /// <summary>
        /// Método para reprogramar el tiempo programado de un evento de inicio cuando este cambia producto de una 
        /// activación de turno
        /// </summary>
        /// <param name="tramo">Tramo objetivo</param>
        private void ReprogramarEventoPorRecoveryTurnos(Tramo tramo)
        {
            Avion avion = tramo.GetAvion(tramo.IdAvionProgramadoActual);
            if (avion.EventosAvion.Exists(
                delegate(Evento e)
                {
                    return e.EsDelTipo(TipoEvento.InicioTramo);
                }))
            {
                Evento inicio = avion.EventosAvion.Find(
                delegate(Evento e)
                {
                    return e.EsDelTipo(TipoEvento.InicioTramo);
                });
                if (inicio.TiempoInicioEvento > tramo.TInicialProg && inicio.TiempoInicioEvento < tramo.TInicialRst)
                {
                    inicio.TiempoInicioEvento = tramo.TInicialRst;
                }
            }
        }

        /// <summary>
        /// Usa un turno de backup.
        /// </summary>
        /// <param name="tramoTurno">Tramo que operará el turno</param>
        /// <param name="atraso_por_cambio_turno">Atraso provocado por el cambio de turno</param>
        internal void UsarTurnoBackup(Tramo tramoTurno, int atraso_por_cambio_turno)
        {
            bool pudo_usar_turno_backup;
            string grupo_flota = tramoTurno.GetAvion(tramoTurno.IdAvionProgramadoActual).GrupoAvion.Nombre;
            DateTime fecha = new DateTime(tramoTurno.DtIniProg.Year, tramoTurno.DtIniProg.Month, tramoTurno.DtIniProg.Day);
            int hora_utc = tramoTurno.DtIniProg.AddMinutes(tramoTurno.TInicialRst - tramoTurno.TInicialProg).Hour;
            int desfase_utc = GetAeropuerto(tramoTurno.TramoBase.Origen).Horas_Desfase_UTC;
            int hora_local = (hora_utc - desfase_utc) % 24 >= 0 ? ((hora_utc - desfase_utc) % 24) : (24 + (hora_utc - desfase_utc) % 24);
            //Acá se gestiona el turno de backup
            _usar_turno_backup(grupo_flota, fecha, hora_local, out pudo_usar_turno_backup);
            if (pudo_usar_turno_backup)
            {
                //Si se pudo activar un turno se desconecta el tramo, se retrasa parcialmente el tramo y se 
                //agrega un atraso a la lista de atrasos del tramo.
                //tramoTurno.EsperaTramoPorConexionPairing = false;
                tramoTurno.TInicialRst += atraso_por_cambio_turno;
                tramoTurno.AgregarCausaDeAtraso(TipoDisrupcion.RC_TRIP, atraso_por_cambio_turno);
                ReprogramarEventoPorRecoveryTurnos(tramoTurno);
            }
            else
            {
                //REGISTRAR VECES EN QUE SE INTENTO USAR TURNO, PERO NO QUEDABAN
            }
        }

        #region Generacion de Disrupciones

        /// <summary>
        /// Agrega causas de atraso reaccionarios (RC y HBT) a un tramo
        /// </summary>
        /// <param name="atrasoDeVuelo">Minutos de atraso de vuelo</param>
        /// <param name="atrasoDespegue">Minutos de atraso de despegue</param>
        /// <param name="espacioDisponible">Minutos de holgura disponible</param>
        /// <param name="tramoObjetivo">Tramo objetivo para agregar las causas reaccionarias</param>
        private void AgregarCausasReaccionarias(int atrasoDeVuelo, int atrasoDespegue, int espacioDisponible, Tramo tramoObjetivo)
        {
            if (tramoObjetivo.Tramo_Siguiente != null)
            {
                int atrasoInicial = tramoObjetivo.Tramo_Siguiente.TInicialRst - tramoObjetivo.Tramo_Siguiente.TInicialProg;
                //Si ya viene con una traso
                if (atrasoInicial > 0) //En este caso siempre el especio disponible es cero
                {
                    //Si hay un adelanto en la llegada
                    if (atrasoDeVuelo < 0 && atrasoInicial + atrasoDeVuelo > 0)
                    {
                        //Se agrega reaccionario
                        if (!tramoObjetivo.Tramo_Siguiente.CausasAtraso.ContainsKey(TipoDisrupcion.RC))
                        {
                            tramoObjetivo.Tramo_Siguiente.AgregarCausaDeAtraso(TipoDisrupcion.RC, atrasoInicial + atrasoDeVuelo);
                        }
                        else
                        {
                            tramoObjetivo.Tramo_Siguiente.CausasAtraso[TipoDisrupcion.RC] = atrasoInicial + atrasoDeVuelo;
                        }
                        if (atrasoDespegue > 0)
                        {
                            tramoObjetivo.Tramo_Siguiente.CausasAtraso[TipoDisrupcion.RC] += atrasoDespegue;
                        }
                    }
                    //Si hay un adelanto en la llegada que anula el atraso inicial, pero que con el atraso de despegue genera un reaccionario
                    else if (atrasoDeVuelo < 0 && atrasoInicial + atrasoDeVuelo <= 0 && atrasoInicial + atrasoDeVuelo + atrasoDespegue > 0)
                    {
                        //Se agrega reaccionario
                        if (!tramoObjetivo.Tramo_Siguiente.CausasAtraso.ContainsKey(TipoDisrupcion.RC))
                        {
                            tramoObjetivo.Tramo_Siguiente.AgregarCausaDeAtraso(TipoDisrupcion.RC, atrasoInicial + atrasoDeVuelo + atrasoDespegue);
                        }
                        else
                        {
                            tramoObjetivo.Tramo_Siguiente.CausasAtraso[TipoDisrupcion.RC] = atrasoInicial + atrasoDeVuelo + atrasoDespegue;
                        }
                    }
                    //Si hay un atraso de vuelo no negativo
                    else if (atrasoDeVuelo >= 0)
                    {
                        //Si ya trae un atraso reaccionario y no está registrado, se registra. Esto sucede cuando hay efecto dominó
                        if (!tramoObjetivo.Tramo_Siguiente.CausasAtraso.ContainsKey(TipoDisrupcion.RC))
                        {
                            tramoObjetivo.Tramo_Siguiente.AgregarCausaDeAtraso(TipoDisrupcion.RC, atrasoInicial);
                        }
                        else
                        {
                            tramoObjetivo.Tramo_Siguiente.CausasAtraso[TipoDisrupcion.RC] = atrasoInicial;
                        }
                        if (atrasoDespegue> 0)
                        {
                            tramoObjetivo.Tramo_Siguiente.CausasAtraso[TipoDisrupcion.RC] += atrasoDespegue;
                        }
                        //Se agrega atraso HBT
                        if (atrasoDeVuelo - Math.Max(0,espacioDisponible - atrasoDespegue) > 0)
                        {
                            tramoObjetivo.Tramo_Siguiente.AgregarCausaDeAtraso(TipoDisrupcion.HBT, atrasoDeVuelo - Math.Max(0, espacioDisponible - atrasoDespegue));
                        }
                    }
                }

                //Si no viene con atraso
                else
                {
                    //Hay atraso de vuelo (HBT)
                    if (atrasoDeVuelo > 0)
                    {
                        //Hay atraso de despegue
                        if (atrasoDespegue > 0)
                        {
                            if (atrasoDespegue - espacioDisponible > 0)
                            {
                                tramoObjetivo.Tramo_Siguiente.AgregarCausaDeAtraso(TipoDisrupcion.RC, atrasoDespegue - espacioDisponible);
                                tramoObjetivo.Tramo_Siguiente.AgregarCausaDeAtraso(TipoDisrupcion.HBT, atrasoDeVuelo);
                            }
                            else if (atrasoDeVuelo + atrasoDespegue - espacioDisponible > 0)
                            {
                                tramoObjetivo.Tramo_Siguiente.AgregarCausaDeAtraso(TipoDisrupcion.HBT, atrasoDeVuelo + atrasoDespegue - espacioDisponible);
                            }                            
                        }
                        else if (atrasoDeVuelo - espacioDisponible > 0)
                        {
                            tramoObjetivo.Tramo_Siguiente.AgregarCausaDeAtraso(TipoDisrupcion.HBT, atrasoDeVuelo - espacioDisponible);
                        }                        
                    }
                    //Hay adelanto en la llegada
                    else
                    {
                        //Hay atraso reaccionario
                        if (atrasoDespegue - espacioDisponible + atrasoDeVuelo > 0)
                        {
                            //Se agrega o actualiza el atraso reaccionario
                            if (tramoObjetivo.Tramo_Siguiente.CausasAtraso.ContainsKey(TipoDisrupcion.RC))
                            {
                                tramoObjetivo.Tramo_Siguiente.CausasAtraso[TipoDisrupcion.RC] = atrasoDespegue - espacioDisponible + atrasoDeVuelo;
                            }
                            else tramoObjetivo.Tramo_Siguiente.AgregarCausaDeAtraso(TipoDisrupcion.RC, atrasoDespegue - espacioDisponible + atrasoDeVuelo);
                        }    
                        //Se elimina atraso reaccionario
                        else if (tramoObjetivo.Tramo_Siguiente.CausasAtraso.ContainsKey(TipoDisrupcion.RC) && atrasoDespegue <= 0)
                        {
                            tramoObjetivo.Tramo_Siguiente.CausasAtraso.Remove(TipoDisrupcion.RC);
                        }
                        //Se elimina atraso HBT
                        if (atrasoDeVuelo - espacioDisponible <=0 && tramoObjetivo.Tramo_Siguiente.CausasAtraso.ContainsKey(TipoDisrupcion.HBT))
                        {
                            tramoObjetivo.Tramo_Siguiente.CausasAtraso.Remove(TipoDisrupcion.HBT); 
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Genera minutos de atraso al despegue por las distintas causas de atraso
        /// </summary>
        /// <returns>Minutos de atraso de despegue</returns>
        private int GenerarDisrupcionDespeque()
        {
            int cantidadaMaximaAtrasos = 3;
            int atrasoTotal = 0;
            List<KeyValuePair<TipoDisrupcion, int>> atrasos_generados = new List<KeyValuePair<TipoDisrupcion, int>>();
            //Se genera retraso climatico            
            atrasos_generados.Add(new KeyValuePair<TipoDisrupcion, int>(TipoDisrupcion.METEREOLOGIA, Math.Max(GenerarAtrasoClima(Tramo_Actual.TramoBase.Origen), GenerarAtrasoClima(Tramo_Actual.TramoBase.Destino))));
            atrasos_generados.Add(new KeyValuePair<TipoDisrupcion, int>(TipoDisrupcion.MANTENIMIENTO, GenerarDisrupcionMantencion()));
            atrasos_generados.Add(new KeyValuePair<TipoDisrupcion, int>(TipoDisrupcion.ATC, GenerarAtrasoATC()));
            atrasos_generados.Add(new KeyValuePair<TipoDisrupcion, int>(TipoDisrupcion.OTROS, GenerarAtrasoOtros(TipoDisrupcion.OTROS)));
            atrasos_generados.Add(new KeyValuePair<TipoDisrupcion, int>(TipoDisrupcion.RECURSOS_DEL_APTO, GenerarAtrasoOtros(TipoDisrupcion.RECURSOS_DEL_APTO)));
            atrasos_generados.Add(new KeyValuePair<TipoDisrupcion, int>(TipoDisrupcion.TA_BAJO_ALA, GenerarAtrasoOtros(TipoDisrupcion.TA_BAJO_ALA)));
            atrasos_generados.Add(new KeyValuePair<TipoDisrupcion, int>(TipoDisrupcion.TA_SOBRE_ALA, GenerarAtrasoOtros(TipoDisrupcion.TA_SOBRE_ALA)));
            atrasos_generados.Add(new KeyValuePair<TipoDisrupcion, int>(TipoDisrupcion.TRIPULACIONES, GenerarAtrasoOtros(TipoDisrupcion.TRIPULACIONES)));
            atrasos_generados.Sort(delegate(KeyValuePair<TipoDisrupcion, int> kvp1, KeyValuePair<TipoDisrupcion, int> kvp2)
            {
                if (kvp1.Value > kvp2.Value) return -1;
                else if (kvp1.Value < kvp2.Value) return 1;
                else return 0;
            });
            for (int i = 0; i < cantidadaMaximaAtrasos; i++)
            {
                Tramo_Actual.AgregarCausaDeAtraso(atrasos_generados[i].Key, atrasos_generados[i].Value);
                atrasoTotal += atrasos_generados[i].Value;
            }

            int cuentaAtrasos = 0;
            for (int i = 0; i < atrasos_generados.Count; i++)
            {
                cuentaAtrasos += atrasos_generados[i].Value > 0 ? 1 : 0;
            }
            return atrasoTotal;
        }

        /// <summary>
        /// Genera minutos de adelanto
        /// </summary>
        /// <returns>Minutos de adelanto</returns>
        private int GenerarAdelanto(string origen)
        {
            List<string> factores = new List<string>();
            factores.Add(Tramo_Actual.TramoBase.Origen);
            DistribucionesEnum distribucion;
            double factorEscenario;
            DataDisrupcion data;
            GetInfoAtrasos(TipoDisrupcion.ADELANTO, factores, out factorEscenario, out data, out distribucion);
            int adelantoGenerado = Convert.ToInt32(Distribuciones.GenerarAleatorio(Tramo_Actual.RdmTramo, distribucion, data.Prob, data.Media, data.Desvest, 0, int.MaxValue));                    
            
            if (Tramo_Actual.Tramo_Previo != null)
            {
                if (Tramo_Actual.Tramo_Previo.MantenimientoPosterior == null)
                {
                     return adelantoGenerado;    
                }
                else
                {
                    int finManttoMasTAT = Tramo_Actual.Tramo_Previo.MantenimientoPosterior.TiempoFinManttoRst + Tramo_Actual.TurnAroundMinimoOrigen;
                    int minutosMaximoAdelanto = Math.Max(Tramo_Actual.TInicialRst - finManttoMasTAT,0);
                    int adelanto = Math.Min(minutosMaximoAdelanto, adelantoGenerado);
                    return adelanto;
                }
            }
            else if (Tramo_Actual.Tramo_Previo == null)
            {
                if(!_primer_slot_es_mantenimiento)
                {                
                    return adelantoGenerado;
                }
                else
                {
                    SlotMantenimiento primerSlot = this.SlotsMantenimiento[0];
                    int finManttoMasTAT = primerSlot.TiempoFinManttoRst + Tramo_Actual.TurnAroundMinimoOrigen;
                    int minutosMaximoAdelanto = Math.Max(Tramo_Actual.TInicialRst - finManttoMasTAT, 0);
                    //int adelantoGenerado = Convert.ToInt32(Tramo_Actual.InfoAtrasos[TipoDisrupcion.Adelanto].GenerarAtraso(Tramo_Actual.RdmTramo));
                    int adelanto = Math.Min(minutosMaximoAdelanto, adelantoGenerado);
                    return adelanto;
                }
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Genera minutos de atraso por la disrupción "ATC"
        /// </summary>
        /// <returns>Minutos de atraso</returns>
        private int GenerarAtrasoATC()
        {
            string mes = Tramo_Actual.Mes.ToString();
            string aeropuerto = Tramo_Actual.TramoBase.Origen;
            string hora = Tramo_Actual.HoraActual.ToString();
            List<string> factores = new List<string>();
            factores.Add(mes);
            factores.Add(aeropuerto);
            factores.Add(hora);
            DistribucionesEnum distribucion;
            double factorEscenario;
            DataDisrupcion data;
            GetInfoAtrasos(TipoDisrupcion.ATC, factores, out factorEscenario, out data, out distribucion);
            int atrasoGenerado = Convert.ToInt32(Distribuciones.GenerarAleatorio(Tramo_Actual.RdmTramo, distribucion, data.Prob, data.Media, data.Desvest, 0, int.MaxValue));
            return atrasoGenerado;
        }

        /// <summary>
        /// Genera minutos de atraso por la disrupción "WXS" en un aeropuerto
        /// <param name="aeropuerto">Aeropuerto objetivo</param>
        /// </summary>
        /// <returns>Minutos de atraso</returns>
        private int GenerarAtrasoClima(string aeropuerto)
        {
            Aeropuerto a = _get_aeropuerto(aeropuerto);
            if (a.Tiene_Info_Historica_WXS)
            {
                int atraso = a.EstimarMinutosAtrasoWXSHistorico(this.Tramo_Actual);
                return atraso;
            }
            else
            {
                if (!a.BuenClima)
                {
                    string mes = Tramo_Actual.Mes.ToString();
                    string hora = Tramo_Actual.HoraActual.ToString();     
                    List<string> factores = new List<string>();
                    factores.Add(mes);
                    factores.Add(aeropuerto);
                    factores.Add(hora);
                    DistribucionesEnum distribucion;
                    double factorEscenario;
                    DataDisrupcion data;
                    GetInfoAtrasos(TipoDisrupcion.METEREOLOGIA, factores, out factorEscenario, out data, out distribucion);
                    int atrasoGenerado = Convert.ToInt32(Distribuciones.GenerarAleatorio(Tramo_Actual.RdmTramo, distribucion, 1, data.Media, data.Desvest, 0, int.MaxValue));                    
                    return atrasoGenerado;
                }
                else return 0;
            }            
        }

        /// <summary>
        /// Genera minutos de atraso por la disrupción "OTROS"
        /// </summary>
        /// <returns>Minutos de atraso</returns>
        private int GenerarAtrasoOtros(TipoDisrupcion disrupcion)
        {
            string aeropuerto = Tramo_Actual.TramoBase.Origen;
            string hora = Tramo_Actual.HoraActual.ToString();
            List<string> factores = new List<string>();
            factores.Add(aeropuerto);
            factores.Add(hora);
            DistribucionesEnum distribucion;
            double factorEscenario;
            DataDisrupcion data;
            GetInfoAtrasos(disrupcion, factores, out factorEscenario, out data, out distribucion);
            int atrasoGenerado = Convert.ToInt32(Distribuciones.GenerarAleatorio(Tramo_Actual.RdmTramo, distribucion, data.Prob, data.Media, data.Desvest, 0, int.MaxValue));
            return atrasoGenerado;
        }

        /// <summary>
        /// Genera minutos de variación por diferencias entre HBT programado y resultante
        /// </summary>
        /// <returns>Minutos de variación de HBT</returns>
        private int GenerarAtrasoHBT()
        {
            string par_od = Tramo_Actual.ParOD;
            string grupo = Tramo_Actual.GetAvion(Tramo_Actual.IdAvionProgramado).GrupoAvion.Nombre;// this.GrupoAvion.Nombre;
            List<string> factores = new List<string>();
            factores.Add(par_od);
            factores.Add(grupo);
            DistribucionesEnum distribucion;
            double factorEscenario;
            DataDisrupcion data;
            GetInfoAtrasos(TipoDisrupcion.HBT, factores, out factorEscenario, out data, out distribucion);
            if (data.Prob == 0)
            {               
                return 0;
            }
            else
            {
                int hbtAleatorio = Convert.ToInt32(Distribuciones.GenerarAleatorio(Tramo_Actual.RdmTramo, distribucion, data.Prob, data.Media, data.Desvest, data.Min, data.Max));
                int hbtOriginal = Tramo_Actual.TFinalProg - Tramo_Actual.TInicialProg;
                int variacionHBT = hbtAleatorio - hbtOriginal;
                return variacionHBT;
            }
        }

        /// <summary>
        /// Genera minutos de atraso por la disrupción "Mantto"
        /// </summary>
        /// <returns>Minutos de atraso</returns>
        private int GenerarDisrupcionMantencion()
        {
            string key_mantto = Tramo_Actual.KeyMantto;
            List<string> factores = new List<string>();
            factores.Add(key_mantto);
            DistribucionesEnum distribucion;
            double factorEscenario;
            DataDisrupcion data;
            GetInfoAtrasos(TipoDisrupcion.MANTENIMIENTO, factores, out factorEscenario, out data, out distribucion);
            int atrasoGenerado = Convert.ToInt32(Distribuciones.GenerarAleatorio(Tramo_Actual.RdmTramo, distribucion, data.Prob * factorEscenario, data.Media, data.Desvest, 0, int.MaxValue));
            return atrasoGenerado;      
            //return Convert.ToInt32(Tramo_Actual.InfoAtrasos[TipoDisrupcion.Mantto].GenerarAtraso(Tramo_Actual.RdmTramo));
        }

        #endregion

        #endregion

        #region IDisposable Members

        private bool IsDisposed=false;

        /// <summary>
        /// Destructor de instancias de la clase
        /// </summary>
        ~Avion()
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
            if(!IsDisposed)
            {
                if(Disposing)
                {
                    _eventos_avion.Clear();
                    _legs.Clear();
                    _pares_od_programados.Clear();
                    foreach (SlotMantenimiento s in _slots_mantenimiento)
                    {
                        s.Dispose();
                        s.SlotPrevio = null;
                        s.SlotSiguiente = null;                        
                    }
                    _slots_mantenimiento.Clear();
                    Tramo tramoAux = Tramo_Raiz;                    
                    while (tramoAux != null)
                    {
                        if(tramoAux.Tramo_Previo !=null)
                        {
                            tramoAux.Tramo_Previo.Tramo_Siguiente = null;
                            tramoAux.Tramo_Previo.Dispose();
                        }
                        
                        tramoAux.Tramo_Previo = null;
                        tramoAux = tramoAux.Tramo_Siguiente;
                    }
                }
                _eventos_avion = null;
                _aeropuerto_actual = null;
                _actualizar_tiempo_simulacion = null;
                _get_aeropuerto = null;
                _usar_turno_backup = null;
                _recovery = null;
                Tramo_Raiz = null;
                Tramo_Actual = null;
                Ultimo_Tramo_Agregado = null;
                _slots_mantenimiento = null;
                _slot_actual = null;
                _legs = null;
                _ac_type = null;
                _id_avion = null;
                _get_backups = null;
                _get_flota = null;
                _get_info_atrasos = null;
                _get_aeropuerto = null;
                _grupo = null;
                _subFlota = null;
                _matricula = null;
                _pares_od_programados = null;

            }
            IsDisposed=true;
        }
        #endregion

    }
}
