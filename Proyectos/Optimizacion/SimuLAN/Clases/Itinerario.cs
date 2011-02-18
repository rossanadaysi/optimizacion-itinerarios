using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using SimuLAN.Utils;
using SimuLAN.Clases.Recovery;


namespace SimuLAN.Clases
{   
    /// <summary>
    /// Clase que encapsula el itinerario: Aviones, aeropuertos y tramos.
    /// </summary>
    [XmlRoot("itinerario")]
    public class Itinerario:IDisposable
    {
        #region CONSTANTS

        /// <summary>
        /// Número de serie de Excel de la fecha correspondiente al 01/01/2008.
        /// </summary>
        private const int NUM_SERIE_FECHA_BASE = 39448;

        /// <summary>
        /// Fecha base correspondiente a NUM_SERIE_FECHA_BASE
        /// </summary>
        private static readonly DateTime FECHA_BASE = new DateTime(2008, 1, 1);

        #endregion

        #region ENUMS

        /// <summary>
        /// Tipo de búsqueda en pareo de conexiones por número de vuelo
        /// </summary>
        private enum TipoBusqueda { Num_Vuelo, Id_Hub };

        #endregion

        #region ATRIBUTES

        /// <summary>
        /// Diccionary con AcType
        /// </summary>
        private SerializableDictionary<string, AcType> _ac_type_dictionary;

        /// <summary>
        /// Diccionario con los aeropuertos del itinerario
        /// </summary>
        private SerializableDictionary<string, Aeropuerto> _aeropuertos_dictionary;

        /// <summary>
        /// Diccionario de aviones usado en interfaz
        /// </summary>
        private SerializableDictionary<string, Avion> _aviones_dictionary;

        /// <summary>
        /// Lista de vuelos en conexión
        /// </summary>
        private SerializableList<ConexionLegs> _conexiones_lista;

        /// <summary>
        /// Contador de los tramos del itinerario
        /// </summary>
        private int _contador_tramos;

        /// <summary>
        /// Controlador de slots de backups
        /// </summary>
        private ControladorBackups _controlador_backups;

        /// <summary>
        /// Delegado entregado para el cálculo de la utilización de slots de backup
        /// </summary>
        private EstimarUtilizacionSlotEventHandler _estimar_utilizacion;

        /// <summary>
        /// Fecha de inicio del itinerario
        /// </summary>
        private DateTime _fecha_inicio;

        /// <summary>
        /// String con fecha de inicio del itinerario
        /// </summary>
        private string _fecha_inicio_string;

        /// <summary>
        /// Fecha de término del itinerario
        /// </summary>
        private DateTime _fecha_termino;

        /// <summary>
        /// String con fecha de término del itinerario
        /// </summary>
        private string _fecha_termino_string;

        /// <summary>
        /// Delegado para obtener el aeropuerto de un tramo
        /// </summary>
        private GetAeropuertoEventHandler _get_aeropuerto;

        /// <summary>
        /// Delegado para obtener el avión de un tramo
        /// </summary>
        private GetAvionEventHandler _get_avion;

        /// <summary>
        /// Delegado que encapsula método para obtener la lista de slots de backup asignada inicialmente a una matrícula
        /// </summary>
        [XmlIgnore]
        private GetBackupsAvionEventHandler _get_backups;

        /// <summary>
        /// Delegado que entrega una conexión de pairing para un tramo específico
        /// </summary>
        [XmlIgnore]
        private GetConexionEventHandler _get_conexion;

        /// <summary>
        /// Delegado para obtener la flota de un tramo
        /// </summary>
        private GetFlotaEventHandler _get_flota;

        /// <summary>
        /// Delegado que entrega los minutos que faltan para que se repita en alguna parte del itinerario el mismo par OD del tramo actual.
        /// </summary>
        [XmlIgnore]
        private GetMinutosHastaProximoVueloEventHandler _get_minutos_proximo_vuelo;

        /// <summary>
        /// String usado para identificar el itinerario
        /// </summary>
        private string _identificador;

        /// <summary>
        /// Lista con los negocios presentes en el itinerario.
        /// </summary>
        [XmlIgnore]
        private List<string> _negocios;

        /// <summary>
        /// Diccionario con todos los tramos del itinerario
        /// </summary>
        private SerializableDictionary<int, Tramo> _tramos;

        /// <summary>
        /// Diccionario para gestionar los turnos de backup (por grupo-flota)
        /// </summary>
        private Dictionary<string, ControladorTurnosBackup> _turnos_backup;

        /// <summary>
        /// Delegado que encapsula método para usar avión de backup
        /// </summary>
        private UsarTurnoBackupEventHandler _usar_turno_backup;

        #endregion

        #region PROPERTIES
        
        /// <summary>
        /// Diccionary con AcType
        /// </summary>
        [XmlIgnore]
        public SerializableDictionary<string, AcType> AcTypeDictionary
        {
            get { return _ac_type_dictionary; }
            set { _ac_type_dictionary = value; }
        }

        /// <summary>
        /// Diccionario con los aeropuertos del itinerario
        /// </summary>
        [XmlIgnore]
        public SerializableDictionary<string, Aeropuerto> AeropuertosDictionary
        {
            get { return _aeropuertos_dictionary; }
            set { _aeropuertos_dictionary = value; }
        }

        /// <summary>
        /// Diccionario de aviones usado en interfaz
        /// </summary>
        [XmlIgnore]
        public SerializableDictionary<string, Avion> AvionesDictionary
        {
            get { return _aviones_dictionary; }
            set { _aviones_dictionary = value; }
        }

        /// <summary>
        /// Lista de vuelos en conexión
        /// </summary>
        public SerializableList<ConexionLegs> Conexiones_Lista
        {
            get { return _conexiones_lista; }
        }

        /// <summary>
        /// Contador de los tramos del itinerario
        /// </summary>
        public int ContadorTramos
        {
            get { return _contador_tramos; }
            set { _contador_tramos = value; }
        }

        /// <summary>
        /// Controlador de slots de backups
        /// </summary>
        public ControladorBackups ControladorBackups
        {
            get { return _controlador_backups; }
            set { _controlador_backups = value; }
        }

        /// <summary>
        /// Retorna un delegado GetSlotsBackupEventHander (para buscar slots de backup)
        /// </summary>
        public GetSlotsBackupEventHander DelegateGetSlotsBackup
        {
            get
            {
                return new GetSlotsBackupEventHander(GetSlotsBackup);
            }
        }

        /// <summary>
        /// Fecha de inicio del itinerario
        /// </summary>
        public DateTime FechaInicio
        {
            get { return _fecha_inicio; }
            set { _fecha_inicio = value; }
        }

        /// <summary>
        /// String con fecha de inicio del itinerario
        /// </summary>
        public string FechaInicioString
        {
            get { return _fecha_inicio_string; }
            set { _fecha_inicio_string = value; }
        }

        /// <summary>
        /// Fecha de termino del itinerario
        /// </summary>
        public DateTime FechaTermino
        {
            get { return _fecha_termino; }
            set { _fecha_termino = value; }
        }

        /// <summary>
        /// String con fecha de termino del itinerario
        /// </summary>
        public string FechaTerminoString
        {
            get { return _fecha_termino_string; }
            set { _fecha_termino_string = value; }
        }

        /// <summary>
        /// String usado para identificar el itinerario
        /// </summary>
        public string Identificador
        {
            get { return _identificador; }
            set { _identificador = value; }
        }

        /// <summary>
        /// Lista con los negocios presentes en el itinerario.
        /// </summary>
        [XmlIgnore]
        public List<string> Negocios
        {
            get { return _negocios; }
        }

        /// <summary>
        /// Diccionario con todos los tramos del itinerario
        /// </summary>
        public SerializableDictionary<int, Tramo> Tramos
        {
            get { return _tramos; }
            set { _tramos = value; }
        }

        /// <summary>
        /// Diccionario para gestionar los turnos de backup (por grupo-flota)
        /// </summary>
        public Dictionary<string, ControladorTurnosBackup> TurnosBackup
        {
            get { return _turnos_backup; }
        }

        #endregion
        
        #region CONSTRUCTOR

        /// <summary>
        /// Constructor para serializacion
        /// </summary>
        public Itinerario()
        { 
        
        }

        /// <summary>
        /// Constructor de itinerario
        /// </summary>
        /// <param name="o">Objeto para diferenciar de primer constructor</param>
        public Itinerario(object o)
        {           
            this._fecha_inicio = new DateTime();
            this._fecha_termino = new DateTime();
            this._identificador = o.ToString();
            this._ac_type_dictionary = new SerializableDictionary<string, AcType>();
            this._aviones_dictionary = new SerializableDictionary<string, Avion>();
            this._aeropuertos_dictionary = new SerializableDictionary<string, Aeropuerto>();
            this._tramos = new SerializableDictionary<int, Tramo>();
            this._get_aeropuerto = new GetAeropuertoEventHandler(GetAeropuerto);
            this._usar_turno_backup = new UsarTurnoBackupEventHandler(IntentaUsarTurnoBackup);
            this._estimar_utilizacion = new EstimarUtilizacionSlotEventHandler(EstimarUtilizacionSlot);
            this._get_avion = new GetAvionEventHandler(GetAvion);
            this._get_flota = new GetFlotaEventHandler(GetFlotaMetodo);
            this._conexiones_lista = new SerializableList<ConexionLegs>();
            this._get_conexion = new GetConexionEventHandler(GetConexionLeg);
            this._get_backups = new GetBackupsAvionEventHandler(GetSlotsBackupAvion);
            this._get_minutos_proximo_vuelo = new GetMinutosHastaProximoVueloEventHandler(EstimarMinutosHastaProximoVuelo);
            this._get_conexion = new GetConexionEventHandler(GetConexionLeg);
            this._turnos_backup = new Dictionary<string, ControladorTurnosBackup>();
            this._negocios = new List<string>();
            this._controlador_backups = new ControladorBackups(this._get_flota);
            _contador_tramos = 0;
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Agrega un nuevo aeropuerto al itinerario
        /// </summary>
        /// <param name="id_avion">Identificador del aeropuerto</param>
        /// <param name="avion">Objeto Aeropuerto a agregar</param>
        public void AgregarAeropuerto(Aeropuerto aeropuerto, string id_aeropuerto)
        {
            _aeropuertos_dictionary.Add(id_aeropuerto, aeropuerto);
        }

        /// <summary>
        /// Agrega un nuevo avión al itinerario
        /// </summary>
        /// <param name="id_avion">Identificador del avión</param>
        /// <param name="avion">Objeto Avion a agregar</param>
        public void AgregarAvion(string id_avion, Avion avion)
        {
            _aviones_dictionary.Add(id_avion, avion);
            _aviones_dictionary[id_avion].GetAeropuerto = _get_aeropuerto;
            _aviones_dictionary[id_avion].GetFlota = _get_flota;
            _aviones_dictionary[id_avion].GetBackups = _get_backups;
        }

        /// <summary>
        /// Carga delegado para obtener minutos de espera por conexión a las conexiones
        /// </summary>
        /// <param name="getMinutosEsperaConexionEventHandler">Delegado para obtener minutos de espera</param>
        public void CargarDelegatesEnConexiones(GetMinutosEsperaConexionEventHandler getMinutosEsperaConexionEventHandler)
        {
            foreach (ConexionLegs conex in _conexiones_lista)
            {
                if (conex.ConexionBase.Tipo == TipoConexion.Pasajeros)
                {
                    conex.SetDelegate(getMinutosEsperaConexionEventHandler);
                }
            }
        }

        /// <summary>
        /// Carga el diccionario de AcTypes desde un diccionario externo con los AcTypes y Flotas.
        /// </summary>
        /// <param name="serializableDictionary">Diccionario externo con |AcType , Flota|></param>
        public void CargarFlotasEnAcTypes(SerializableDictionary<string, string> dictionaryAcTypeFlota)
        {
            foreach (string acType in dictionaryAcTypeFlota.Keys)
            {
                string key = acType;
                if (!_ac_type_dictionary.ContainsKey(key))
                {
                    _ac_type_dictionary.Add(key, new AcType(acType.ToString(), dictionaryAcTypeFlota[acType].ToString()));
                }
                else
                {
                    _ac_type_dictionary[key].Flota = dictionaryAcTypeFlota[acType].ToString();
                }
            }
        }

        /// <summary>
        /// Carga la información externa de flotas en los tramos en función de su AcType asignado
        /// </summary>
        public void CargarFlotasEnTramos()
        {
            if (_tramos != null && _tramos.Count > 0)
            {
                foreach (int key in _tramos.Keys)
                {
                    string flota = _ac_type_dictionary.ContainsKey(_tramos[key].TramoBase.AcType) ? _ac_type_dictionary[_tramos[key].TramoBase.AcType].Flota : "";
                    _tramos[key].FlotaOperada = flota;
                    _tramos[key].FlotaProgramada = flota;
                }
            }
        }

        /// <summary>
        /// Carga las definiciones de grupos de avión
        /// </summary>
        /// <param name="gruposAvion">Diccionario con las definiciones de grupos de avión - flota</param>
        /// <param name="infoGrupos">Información de los grupos de avión (turnos de backup)</param>
        public void CargarGruposAvion(SerializableDictionaryWithHeaders gruposAvion, SerializableDictionary<string, GrupoFlota> infoGrupos)
        {
            foreach (Avion a in AvionesDictionary.Values)
            {
                string flota = a.GetFlota(a.AcType);
                if (flota !=null && gruposAvion.Dict.ContainsKey(flota))
                {
                    string grupo = gruposAvion.Dict[a.GetFlota(a.AcType)];
                    a.GrupoAvion = infoGrupos[grupo];
                }
                else
                {
                    a.GrupoAvion = null;
                }
            }
        }

        /// <summary>
        /// Carga la información de los hubs a los elementos del diccioario de aeropuertos
        /// </summary>
        /// <param name="hubs_dict">Diccionario con la información de hubs</param>
        public void CargarInfoHubsToAeropuertos(SerializableDictionary<string, int[]> hubs_dict)
        {
            foreach (string hub in hubs_dict.Keys)
            {
                if (this.AeropuertosDictionary.ContainsKey(hub))
                {
                    this.AeropuertosDictionary[hub].Es_Hub = true;
                    this.AeropuertosDictionary[hub].Minutos_Conexion_Pax = hubs_dict[hub][0];
                    this.AeropuertosDictionary[hub].Minutos_Llega_Turno = hubs_dict[hub][1];
                    this.AeropuertosDictionary[hub].Horas_Desfase_UTC = hubs_dict[hub][2];
                }
            }
        }

        /// <summary>
        /// Método para cargar a cada tramo el negocio al que pertenece
        /// </summary>
        /// <param name="infoRutas">Información que liga tramos con rutas</param>
        public void CargarInfoRutasEnTramos(SerializableDictionaryWithHeaders2D infoRutas)
        {
            foreach (Avion a in AvionesDictionary.Values)
            {
                Tramo tramoAux = a.Tramo_Raiz;
                while (tramoAux != null)
                {
                    string key_tramo = tramoAux.KeyHUB;
                    if (infoRutas.Dict.ContainsKey(key_tramo))
                    {
                        foreach (string negocio in infoRutas.Dict[key_tramo].Keys)
                        {
                            tramoAux.Negocio = negocio;
                            tramoAux.VueloHUB = Utilidades.IntToBool(Convert.ToInt16(infoRutas.Dict[key_tramo][negocio]));
                            if (!_negocios.Contains(negocio))
                            {
                                _negocios.Add(negocio);
                            }
                        }
                    }
                    tramoAux = tramoAux.Tramo_Siguiente;
                }
            }
        }

        /// <summary>
        /// Carga la información histórica de WXS en los aeropuertos correspondientes en el diccionario de aeropuertos
        /// </summary>
        /// <param name="info_wxs_historica">Diccionario con información la  histórica de WXS por aeropuerto</param>
        public void CargarInfoWXSHistoricaEnAeropuertos(SerializableDictionary<string, double[,]> info_wxs_historica)
        {
            foreach (string apto in info_wxs_historica.Keys)
            {
                if (_aeropuertos_dictionary.ContainsKey(apto))
                {
                    _aeropuertos_dictionary[apto].CargarInfoHistoricaWXS(info_wxs_historica[apto], _fecha_inicio, _fecha_termino);
                }
            }
        }

        /// <summary>
        /// Método para cargar prefijos de matrículas a los aviones según la sublota que poseen
        /// </summary>
        /// <param name="dataSubFlotaMatricula">Data que liga cada subflota con matrículas</param>
        public void CargarMatriculasEnAviones(SerializableDictionaryWithHeaders dataSubFlotaMatricula)
        {
            SerializableList<string> faltante = new SerializableList<string>();
            foreach (Avion a in AvionesDictionary.Values)
            {
                if (dataSubFlotaMatricula.Dict.ContainsKey(a.SubFlota))
                {
                    a.Matricula = dataSubFlotaMatricula.Dict[a.SubFlota];
                }
            }
        }

        /// <summary>
        /// Carga slots en cada avión a partir de su itinerario programado
        /// </summary>
        public void CargarSlots()
        {
            foreach (Avion a in _aviones_dictionary.Values)
            {
                a.CrearSlots(a.Tramo_Raiz, Convert.ToInt32((_fecha_termino.AddDays(1) - _fecha_inicio).TotalMinutes));
            }
        }

        /// <summary>
        /// Carga turnos de backup en la estructura de gestión de los turnos usada en la simulación
        /// </summary>
        /// <param name="grupos_flotas">Diccionario con la información de los turnos de backup por grupos de flota</param>
        public void CargarTurnosBackup(SerializableDictionary<string, GrupoFlota> grupos_flotas)
        {
            _turnos_backup.Clear();
            foreach (GrupoFlota gf in grupos_flotas.Values)
            {
                _turnos_backup.Add(gf.Nombre, new ControladorTurnosBackup(gf.Turnos_Manana, gf.Turnos_Tarde, this.FechaInicio, this.FechaTermino));
            }
        }

        /// <summary>
        /// Clona el itinerario actual
        /// </summary>
        /// <param name="semilla">Semilla para la generación de números aleatorios</param>
        /// <returns>Itinerario clonado</returns>
        public Itinerario Clonar(int semilla)
        {
            Itinerario itinerarioClonado = new Itinerario("clonado");

            //Clona generados de AOGs usando la semilla de nivel "Réplica".
            itinerarioClonado._controlador_backups.SetSemilla(semilla);
            itinerarioClonado._conexiones_lista = this._conexiones_lista;          
            foreach(ConexionLegs c in itinerarioClonado._conexiones_lista)
            {
                c.GetTramo = new GetTramoEventHandler(itinerarioClonado.GetTramo);                
            }
            itinerarioClonado._fecha_inicio = this._fecha_inicio;
            itinerarioClonado._fecha_termino = this._fecha_termino;
            itinerarioClonado._negocios = this._negocios;
            itinerarioClonado.ContadorTramos = 0;
            foreach(AcType at in _ac_type_dictionary.Values)
            {
                AcType clonado = new AcType(at.Nombre, at.Flota);                
                clonado.Activo = at.Activo;
                itinerarioClonado.AcTypeDictionary.Add(clonado.Nombre,clonado);
            }
            foreach (Aeropuerto aep in _aeropuertos_dictionary.Values)
            {
                //Clona aeropuertos tomando semilla de nivel "Replica".
                Aeropuerto clonado = aep.Clonar(semilla);                    
                itinerarioClonado.AeropuertosDictionary.Add(clonado.Nombre, clonado);                               
            }
            foreach (Avion av in _aviones_dictionary.Values)
            {
                //Clona tramos tomando semilla de nivel "Replica"
                Avion clonado = av.Clonar(semilla);
                if (av.SlotsMantenimiento.Count > 0 && av.SlotsMantenimiento[0].TramoPrevio == null)
                {
                    clonado.PrimerSlotEsMantenimiento = true;
                    clonado.SlotsMantenimiento.Add(av.SlotsMantenimiento[0].Clonar());
                }
                clonado.GetAeropuerto = itinerarioClonado._get_aeropuerto;
                clonado.GetFlota = itinerarioClonado._get_flota;
                clonado.GetBackups = itinerarioClonado._get_backups;
                clonado.UsarTurnoBackupProp = itinerarioClonado._usar_turno_backup;
                Tramo aux = clonado.Tramo_Raiz;
                while (aux != null)
                {
                    aux.GetAvion = itinerarioClonado._get_avion;
                    aux.GetConexion = itinerarioClonado._get_conexion;
                    aux.GetMinutosProximoVuelo = itinerarioClonado._get_minutos_proximo_vuelo;
                    if (aux.MantenimientoPosterior != null)
                        clonado.SlotsMantenimiento.Add(aux.MantenimientoPosterior);
                    aux = aux.Tramo_Siguiente;
                }                
                itinerarioClonado.AvionesDictionary.Add(clonado.IdAvion.ToString(), clonado);                
            }
            foreach (UnidadBackup bu in _controlador_backups.BackupsLista)
            {
                UnidadBackup bu_clonada = (UnidadBackup)bu.Clone();
                bu_clonada.EstimarUtilizacionSlot = itinerarioClonado._estimar_utilizacion;
                itinerarioClonado._controlador_backups.BackupsLista.Add(bu_clonada);
                
            }

            LlenarTramosDictionary(itinerarioClonado);
            itinerarioClonado.CargarSlots();
            return itinerarioClonado;
        }

        /// <summary>
        /// Llena la lista de conexiones de pasajeros
        /// </summary>
        /// <param name="conexiones_pasajeros">Conexiones de pasajeros definidas para el itienerario</param>
        /// <param name="hubs">Lista de hubs de conexión</param>
        /// <param name="semilla">Semilla para la generación de números aleatorios</param>
        public void CrearConexionesPasajeros(SerializableDictionary<int, ConexionPasajeros> conexiones_pasajeros, SerializableDictionary<string, int[]> hubs, int semilla)
        {
            foreach (int index in conexiones_pasajeros.Keys)
            {
                _conexiones_lista.AddAll(GetConexionsPaxs(conexiones_pasajeros[index], hubs, semilla));
            }
        }

        /// <summary>
        /// Llena la lista de conexiones de pairings
        /// </summary>
        /// <param name="pairings">Pairings definidos para el itienerario</param>
        public void CrearConexionesPairing(SerializableDictionary<int, ConexionPairing> pairings)
        {
            foreach (int index in pairings.Keys)
            {
                _conexiones_lista.AddAll(GetConexionsPairing(pairings[index]));
            }
        }

        /// <summary>
        /// Retorno el aeropuerto correspondiente a id_aeropuerto si existe
        /// </summary>
        /// <param name="id_avion">Identificador del aeropuerto</param>
        /// <returns>Avion correspondiente a id_aeropuerto</returns>
        public Aeropuerto GetAeropuerto(string id_aeropuerto)
        {
            if (_aeropuertos_dictionary.ContainsKey(id_aeropuerto))
            {
                return _aeropuertos_dictionary[id_aeropuerto];
            }
            else return null;
        }

        /// <summary>
        /// Retorno el avión correspondiente a id_avion si existe
        /// </summary>
        /// <param name="id_avion">Identificador del avión</param>
        /// <returns>Avion correspondiente a id_avion</returns>
        public Avion GetAvion(string id_avion)
        {
            if (_aviones_dictionary.ContainsKey(id_avion))
            {
                return _aviones_dictionary[id_avion];
            }
            else return null;
        }

        /// <summary>
        /// Obtiene una lista con las conexiones asociadas a un pairing específico
        /// </summary>
        /// <param name="conex">Pairing de conexión</param>
        /// <returns></returns>
        public SerializableList<ConexionLegs> GetConexionsPairing(ConexionPairing conex)
        {
            SerializableList<ConexionLegs> retorno = new SerializableList<ConexionLegs>();
            List<Tramo> tramos_ini = GetTramos(conex.IdVuelo1, TipoBusqueda.Num_Vuelo);
            List<Tramo> tramos_fin = GetTramos(conex.IdVuelo2, TipoBusqueda.Num_Vuelo);
            List<ConexionLegs> conexiones = HacerConexionesPairings(tramos_ini, tramos_fin, conex);
            foreach (ConexionLegs conexion in conexiones)
            {
                conexion.GetTramo = new GetTramoEventHandler(this.GetTramo);
                retorno.Add(conexion);
            }
            return retorno;
        }

        /// <summary>
        /// Obtiene una lista con las conexiones asociadas a una conexión de pasajeros específica
        /// </summary>
        /// <param name="conex">Conexión de pasajeros</param>
        /// <param name="semilla">Semilla para la generación de números aleatorios</param>
        /// <returns></returns>
        public SerializableList<ConexionLegs> GetConexionsPaxs(ConexionPasajeros conex,SerializableDictionary<string, int[]> hubs, int semilla)
        {
            conex.Rdm = new Random(semilla);
            SerializableList<ConexionLegs> retorno = new SerializableList<ConexionLegs>();
            List<Tramo> tramos_ini = GetTramos(conex.IdVuelo1, TipoBusqueda.Num_Vuelo);
            List<Tramo> tramos_fin = GetTramos(conex.IdVuelo2, TipoBusqueda.Num_Vuelo);
            List<ConexionLegs> conexiones = HacerConexionesPasajeros(tramos_ini, tramos_fin, conex, hubs);
            foreach (ConexionLegs conexion in conexiones)
            {
                conexion.GetTramo = new GetTramoEventHandler(this.GetTramo);
                retorno.Add(conexion);
            }
            return retorno;
        }

        /// <summary>
        /// Metodo que obtiene la flota de un acType
        /// </summary>
        /// <param name="acType">Identificador del AcType</param>
        /// <returns>Flota correspondiente a "acType"</returns>
        public string GetFlotaMetodo(string acType)
        {
            if (_ac_type_dictionary != null && _ac_type_dictionary.ContainsKey(acType))
                return _ac_type_dictionary[acType].Flota;
            else return null;
        }

        /// <summary>
        /// Retorna lista con todos los tramos que itinerario
        /// </summary>
        public List<Tramo> GetTramos()
        {
            List<Tramo> retorno = new List<Tramo>();
            foreach (Avion a in _aviones_dictionary.Values)
            {                
                retorno.AddRange(a.ObtenerListaTramos(a.Tramo_Raiz));           
            }                     
            return retorno;
        }

        /// <summary>
        /// Retorna una lista con los slots de backup programados para cada avión
        /// </summary>
        /// <param name="id_matricula">Matrícula de avión objetivo</param>
        /// <returns></returns>
        public List<UnidadBackup> GetSlotsBackupAvion(string id_matricula)
        {
            List<UnidadBackup> lista = new List<UnidadBackup>();
            lista = _controlador_backups.BackupsLista.ToList().FindAll(delegate(UnidadBackup u)
            {
                return (u.TramoBase.Numero_Ac == id_matricula);
            });
            return lista;
        }

        /// <summary>
        /// Terminada la simulación, verifica que se hayan agregado todos los atrasos reaccionarios en los slots de mantenimiento.
        /// </summary>
        public void PostProcesarSlotsMantto()
        {
            //Se añaden los atrasos reaccionarios de los manttos
            int cuentaSlots = 0;
            foreach (Avion a in AvionesDictionary.Values)
            {
                foreach (SlotMantenimiento s in a.SlotsMantenimiento)
                {
                    cuentaSlots++;
                    int atraso = s.TiempoInicioManttoRst - s.TiempoInicioManttoPrg;
                    if (atraso > 0)
                    {
                        s.CausasAtraso.Add(TipoDisrupcion.RC, atraso);
                    }
                }
            }
        }

        /// <summary>
        /// Serializa el itinerario a el archivo .xml de "pathXmlFile"
        /// </summary>
        /// <param name="pathXmlFile">Ruta del archivo de serialización</param>
        public void Serialize(string pathXmlFile)
        {
            XmlSerializer s = new XmlSerializer(typeof(Itinerario));
            TextWriter w = new StreamWriter(pathXmlFile);
            s.Serialize(w, this);
            w.Close();
        }

        #endregion

        #region INTERNAL METHODS

        /// <summary>
        /// Método para usar turnos de backup. Se usará un turno de backup siempre que esté disponible
        /// </summary>
        /// <param name="grupo_flota">Grupo flota del turno reqeridoque requiere turno</param>
        /// <param name="fecha">Fecha de petición del turno</param>
        /// <param name="hora_local_peticion">Hora local de petición del turno</param>
        /// <param name="exitoso">True si se usó un turno de backup</param>
        internal void IntentaUsarTurnoBackup(string grupo_flota, DateTime fecha, int hora_local_peticion, out bool exitoso)
        {
            exitoso = _turnos_backup[grupo_flota].TurnoDisponible(hora_local_peticion, fecha);
            if (exitoso)
            {
                _turnos_backup[grupo_flota].UsarTurnoBackup(hora_local_peticion, fecha);
            }
        }

        /// <summary>
        /// Retorna una lista de todos los slots útiles en un intervalo de tiempo para una estación particular
        /// </summary>
        /// <param name="tiempo_actual">Tiempo inicio</param>
        /// <param name="tiempo_fin_tramo">Tiempo término</param>
        /// <param name="estacion">Estación requerida</param>
        /// <returns></returns>
        internal List<SlotBackup> SlotsUtiles(int tiempo_actual, int tiempo_fin_tramo, string estacion)
        {
            List<SlotBackup> lista = new List<SlotBackup>();
            foreach (UnidadBackup bu in _controlador_backups.BackupsLista)
            {
                foreach (SlotBackup sb in bu.Slots)
                {
                    if (sb.Estacion == estacion && sb.TiempoFinRst > tiempo_actual && sb.TiempoIniRst < tiempo_fin_tramo)
                    {
                        lista.Add(sb);
                    }
                }
            }
            return lista;
        }

        /// <summary>
        /// Indica si un tramo tiene un slot de backup previo
        /// </summary>
        /// <param name="tramo_post_slot">Tramo consultado</param>
        internal bool TramoSinBackup(Tramo tramo_post_slot)
        {
            foreach (UnidadBackup bu in _controlador_backups.BackupsLista)
            {
                SlotBackup s = bu.BuscarSlot(tramo_post_slot.TFinRstTramoPrevio, tramo_post_slot.TInicialRst, tramo_post_slot.IdAvionProgramadoActual, tramo_post_slot.TramoBase.Origen);
                if (s != null)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Retorna el slot de backup previo de un tramo
        /// </summary>
        /// <param name="tramo_post_slot">Tramo consultado</param>
        internal SlotBackup TramoSinBackup_SlotBackup(Tramo tramo_post_slot)
        {
            foreach (UnidadBackup bu in _controlador_backups.BackupsLista)
            {
                SlotBackup s = bu.BuscarSlot(tramo_post_slot.TFinRstTramoPrevio, tramo_post_slot.TInicialRst, tramo_post_slot.IdAvionProgramadoActual, tramo_post_slot.TramoBase.Origen);
                if (s != null)
                {
                    return s;
                }
            }
            return null;
        }

        /// <summary>
        /// Retorna la unidad de backup previa de un tramo
        /// </summary>
        /// <param name="tramo_post_slot">Tramo consultado</param>        
        internal UnidadBackup TramoSinBackup_UnidadBackup(Tramo tramo_post_slot)
        {
            foreach (UnidadBackup bu in _controlador_backups.BackupsLista)
            {
                SlotBackup s = bu.BuscarSlot(tramo_post_slot.TFinRstTramoPrevio, tramo_post_slot.TInicialRst, tramo_post_slot.IdAvionProgramadoActual, tramo_post_slot.TramoBase.Origen);
                if (s != null)
                {
                    return bu;
                }
            }
            return null;
        }

        #endregion

        #region STATIC METHODS

        /// <summary>
        /// Crea un objeto Itinerario a partir de la deserialización de un objeto .xml 
        /// </summary>
        /// <param name="pathXmlFile">Ruta de archivo .xml a deserializar</param>
        /// <returns>Itinerario deserializado</returns>
        public static Itinerario Deserialize(string pathXmlFile)
        {
            XmlSerializer s = new XmlSerializer(typeof(Itinerario));
            Itinerario newItinerario;
            TextReader r = new StreamReader(pathXmlFile);
            newItinerario = (Itinerario)s.Deserialize(r);
            r.Close();
            return newItinerario;
        }

        /// <summary>
        /// Retorna una fecha correspondiente a un número de serie
        /// </summary>
        /// <param name="numeroSerie">string con el número de serie de fecha Excel</param>
        /// <returns>Fecha</returns>
        public static DateTime FechaDesdeNumeroSerieExcel(string numeroSerie)
        {
            int num = Convert.ToInt32(numeroSerie);
            return FECHA_BASE.AddDays(num - NUM_SERIE_FECHA_BASE);
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Método para estimar los minutos que quedan hasta que ocurra se repita el par origen-destino de uno de los vuelos.
        /// </summary>
        /// <param name="tramo_objetivo">Tramo analizado</param>
        /// <returns>Minutos hasta el próximo vuelo similar</returns>
        private int EstimarMinutosHastaProximoVuelo(Tramo tramo_objetivo)
        {
            int minutos_prox_global = int.MaxValue;
            foreach (Avion a in AvionesDictionary.Values)
            {
                int minutos_prox_avion = int.MaxValue;
                if (a.OperaEntre(tramo_objetivo.ParOD))
                {
                    List<Tramo> tramos_siguientes = a.ObtenerListaTramosDespuesDe(tramo_objetivo.TFinalProg);
                    foreach (Tramo t in tramos_siguientes)
                    {
                        if (t.ParOD == tramo_objetivo.ParOD)
                        {
                            minutos_prox_avion = t.TInicialProg - tramo_objetivo.TFinalProg;
                            if (minutos_prox_avion < minutos_prox_global)
                            {
                                minutos_prox_global = minutos_prox_avion;
                            }
                        }
                    }
                }
            }
            return minutos_prox_global;
        }

        /// <summary>
        /// Método para estiamar la utilización de un avión en cierto intervalo de tiempo
        /// </summary>
        /// <param name="matricula">Matrícula consultada</param>
        /// <param name="ini">Tiempo de inicio</param>
        /// <param name="fin">Tiempo de fin</param>
        /// <param name="conTA">True si se consideran los T/A en el cálculo</param>
        /// <returns>Porcentaje de utilización de slot</returns>
        private double EstimarUtilizacionSlot(string matricula, int ini, int fin, bool conTA)
        {
            double utilizacion = 0;
            int minutos_vuelo;
            int minutos_ta_internos;
            int minutos_ta_totales;
            _aviones_dictionary[matricula].CalcularUtilizacionEnIntervalo(ini, fin, out minutos_vuelo, out minutos_ta_internos, out minutos_ta_totales);
            if (conTA)
            {
                utilizacion = (minutos_vuelo + minutos_ta_internos) / ((double)(fin - ini));
            }
            else
            {
                utilizacion = (minutos_vuelo) / ((double)(fin - ini));
            }
            return utilizacion;
        }

        /// <summary>
        /// Método de búsqueda que retorna una lista con las conexiones correspondientes a un número de 
        /// tramo global de uno de los tramos extremos de la conexión.
        /// </summary>
        /// <param name="num_tramo_global">Número de tramo buscado en la conexión</param>
        /// <param name="tipoConexion">Tipo de conexión buscado</param>
        /// <param name="segundoTramo">True si se busca el segundo tramo de la conexión</param>
        /// <returns>Conexión encontrada. Retorna null si existe la conexión buscada</returns>
        private SerializableList<ConexionLegs> GetConexionLeg(int num_tramo_global,TipoConexion tipoConexion, bool segundoTramo)
        {
            SerializableList<ConexionLegs> lista_conexiones = new SerializableList<ConexionLegs>();
            lista_conexiones = _conexiones_lista.FindAll(
            delegate(ConexionLegs c)
            {
                return c.ConexionCumpleCondicion(num_tramo_global, tipoConexion, segundoTramo);
            });
            return lista_conexiones;
        }

        /// <summary>
        /// Retorna todos los slots de backup entre dos tramos de una cadena
        /// </summary>
        /// <param name="tramoIni">Tramo inicial</param>
        /// <param name="tramoFin">Tramo final</param>
        /// <returns></returns>
        private List<SlotBackup> GetSlotsBackup(Tramo tramoIni, Tramo tramoFin)
        {
            List<SlotBackup> retorno = new List<SlotBackup>();
            while (tramoIni != tramoFin.Tramo_Siguiente)
            {
                foreach (UnidadBackup bu in _controlador_backups.BackupsLista)
                {
                    SlotBackup s = bu.BuscarSlot(tramoIni.TInicialRst, tramoIni.TFinalRst, tramoIni.IdAvionProgramadoActual, tramoIni.TramoBase.Origen);
                    if (s != null)
                    {
                        retorno.Add(s);
                    }
                }
                tramoIni = tramoIni.Tramo_Siguiente;
            }
            return retorno;
        }

        /// <summary>
        /// Método de búsqueda para obtener el tramo específico correspondiente a "num_global_tramo"
        /// </summary>
        /// <param name="num_global_tramo">Número global del tramo en el itinerario</param>
        /// <returns>Tramo de vuelo buscado</returns>
        private Tramo GetTramo(int num_global_tramo)
        {
            return this.Tramos[num_global_tramo];
        }

        /// <summary>
        /// Obtiene la lista de tramos de todo el itinerario que tienen el mismo número de vuelo
        /// </summary>
        /// <param name="num_vuelo">Número de vuelo</param>
        /// <returns>Lista de tramos con número de vuelo "num_vuelo"</returns>
        private List<Tramo> GetTramos(string filtro,TipoBusqueda tipo)
        {
            List<Tramo> retorno = new List<Tramo>();
            foreach (Avion a in _aviones_dictionary.Values)
            {
                Tramo tramoAux = a.Tramo_Raiz;
                while (tramoAux != null)
                {
                    if (tipo == TipoBusqueda.Num_Vuelo && filtro == tramoAux.TramoBase.Numero_Vuelo)
                    {
                        retorno.Add(tramoAux);
                    }
                    else if (tipo == TipoBusqueda.Id_Hub && filtro == tramoAux.IdHub)
                    {
                        retorno.Add(tramoAux);
                    }
                    tramoAux = tramoAux.Tramo_Siguiente;
                }
            }
            return retorno;
        }

        /// <summary>
        /// Método que retorna todas las conexiones correspondientes a un pairing. 
        /// Se generan todos los pareos entre tramos_ini y tramos_fin, seleccionándose sólo los factibles.
        /// </summary>
        /// <param name="tramos_ini">Lista de tramos inciales</param>
        /// <param name="tramos_fin">Lista de tramos finales</param>
        /// <param name="conexion">Pairing</param>
        /// <returns>Lista con todas las conexiones correspondientes a un pairing específico</returns>
        private List<ConexionLegs> HacerConexionesPairings(List<Tramo> tramos_ini, List<Tramo> tramos_fin, ConexionPairing conexion)
        {
            List<ConexionLegs> retorno = new List<ConexionLegs>();
            foreach (Tramo tramo_ini in tramos_ini)
            {
                if (conexion.GetAplicaDiaSemana(tramo_ini.DtFinProg.DayOfWeek))
                {
                    foreach (Tramo tramo_fin in tramos_fin)
                    {
                        if (conexion.GetAplicaDiaSemana(tramo_fin.DtIniProg.DayOfWeek))
                        {
                            //Caso uno de conexión: Tramos seguidos de un mismo avión.
                            if (tramo_fin == tramo_ini.Tramo_Siguiente)
                            {
                                ConexionLegs c = new ConexionLegs(tramo_ini, tramo_fin, conexion);
                                retorno.Add(c);
                                break;
                            }
                            else if (tramo_ini.IdAvionProgramado != tramo_fin.IdAvionProgramado && VerificaContinuidadEntreTramos(tramo_ini, tramo_fin, TipoConexion.Pairing, null))
                            {
                                ConexionLegs conexion_encontrada = new ConexionLegs(tramo_ini, tramo_fin, conexion);
                                if ((tramo_ini.TFinalProg + ConexionPairing.TIEMPO_CAMBIO_AVION <= tramo_fin.TInicialProg)
                                  && (tramo_ini.TFinalProg + ConexionPairing.TIEMPO_MAXIMO_PAIRING > tramo_fin.TInicialProg))
                                {
                                    tramo_ini.IniciaConexionPairing = true;
                                    tramo_fin.EsperaTramoPorConexionPairing = true;
                                    retorno.Add(conexion_encontrada);
                                }
                                else
                                { 
                                    //CONEXIONES QUE NO CUMPLEN REGLAS
                                }
                                
                                break;
                            }
                        }
                    }
                }
            }
            return retorno;
        }

        /// <summary>
        /// Método que retorna todas las conexiones correspondientes a una conexión de pasajeros. 
        /// Se generan todos los pareos entre tramos_ini y tramos_fin, seleccionándose sólo los factibles.
        /// </summary>
        /// <param name="tramos_ini">Lista de tramos inciales</param>
        /// <param name="tramos_fin">Lista de tramos finales</param>
        /// <param name="conexion">Conexión de pasajeros</param>
        /// <returns>Lista con todas las conexiones correspondientes a un pairing específico</returns>
        private List<ConexionLegs> HacerConexionesPasajeros(List<Tramo> tramos_ini, List<Tramo> tramos_fin, ConexionPasajeros conexion, SerializableDictionary<string, int[]> hubs)
        {
            List<ConexionLegs> retorno = new List<ConexionLegs>();
            foreach (Tramo tramo_ini in tramos_ini)
            {                
                foreach (Tramo tramo_fin in tramos_fin)
                {                   
                    //Caso de conexión: Tramos seguidos un mismo avión distinto.
                    if (tramo_ini.IdAvionProgramado != tramo_fin.IdAvionProgramado
                        && VerificaContinuidadEntreTramos(tramo_ini, tramo_fin, conexion.Tipo, hubs))
                    {
                        ConexionLegs con_encontrada = new ConexionLegs(tramo_ini, tramo_fin, conexion);
                        tramo_fin.EsperaTramoPorConexionPasajeros = true;
                        con_encontrada.PasajerosConectados = Distribuciones.GenerarAleatorio(conexion.Rdm, DistribucionesEnum.Normal, 1, conexion.Paxs_Promedio, conexion.Pax_Desvest, 0, int.MaxValue);
                        retorno.Add(con_encontrada);
                        break;
                    }                 
                }                
            }
            return retorno;
        }

        /// <summary>
        /// Llena la el diccionario de tramos del itinerario entregado como argumento
        /// </summary>
        /// <param name="itinerarioClonado">Itinerario objetivo</param>
        private void LlenarTramosDictionary(Itinerario itin)
        {
            itin.Tramos.Clear();
            foreach (Avion a in itin.AvionesDictionary.Values)
            {
                Tramo auxTramo = a.Tramo_Raiz;
                while (auxTramo != null)
                {
                    itin.Tramos.Add(auxTramo.TramoBase.Numero_Global, auxTramo);
                    auxTramo = auxTramo.Tramo_Siguiente;
                }
            }
        }

        /// <summary>
        /// Verifica si dos tramos están conectados en el itinerario, para el caso en que los tramos son de aviones distintos.
        /// </summary>
        /// <param name="tramo_ini">Tramo inicial de una conexión</param>
        /// <param name="tramo_fin">Tramo final de una conexión</param>
        /// <param name="tipo">Tipo de conexión: pairing o pasajeros</param>
        /// <returns>True si los tramos están conectados</returns>
        private bool VerificaContinuidadEntreTramos(Tramo tramo_ini, Tramo tramo_fin, TipoConexion tipo, SerializableDictionary<string,int[]> hubs)
        {
            if (tipo == TipoConexion.Pairing)
            {
                if (tramo_ini.TramoBase.Destino == tramo_fin.TramoBase.Origen)
                {
                    if (tramo_ini.TFinalProg < tramo_fin.TInicialProg)
                    {
                        //No puede haber un tramo entre los dos.
                        if (tramo_ini.Tramo_Siguiente != null && tramo_ini.Tramo_Siguiente.TFinalProg <= tramo_fin.TInicialProg)
                        {
                            return false;
                        }
                        //No puede haber un tramo entre los dos.
                        if (tramo_fin.Tramo_Previo != null && tramo_fin.Tramo_Previo.TInicialProg >= tramo_ini.TFinalProg)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
            else if (tipo == TipoConexion.Pasajeros)
            {
                if (tramo_ini.TramoBase.Destino == tramo_fin.TramoBase.Origen && hubs.ContainsKey(tramo_ini.TramoBase.Destino))
                {
                    string hub_conexion = tramo_ini.TramoBase.Destino;
                    int minutos_min_conexion = hubs[hub_conexion][0];
                    int minuros_max_conexion = 300; //PARAMETRIZAR
                    if ((tramo_ini.TFinalProg + minutos_min_conexion <= tramo_fin.TInicialProg) && (tramo_ini.TFinalProg + minuros_max_conexion >= tramo_fin.TInicialProg))
                    {
                        return true;
                    }
                    else if ((tramo_ini.TFinalProg <= tramo_fin.TInicialProg) && (tramo_ini.TFinalProg + minuros_max_conexion >= tramo_fin.TInicialProg))
                    {
                        //conexiones_no_cumplen_minimo.Add(new ConexionLegs(tramo_ini, tramo_fin, null));
                    }
                }
            }
            return false;
        }

        #endregion

        #region IDisposable Members

        private bool IsDisposed = false;

        /// <summary>
        /// Destructor de instancias de la clase
        /// </summary>
        ~Itinerario()
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
                    foreach (Aeropuerto a in _aeropuertos_dictionary.Values)
                    {
                        a.Dispose();
                    }
                    foreach (AcType a in _ac_type_dictionary.Values)
                    {
                        a.Dispose();
                    }
                    foreach (Avion a in _aviones_dictionary.Values)
                    {
                        a.Dispose();
                    }
                    foreach (Tramo a in _tramos.Values)
                    {
                        a.Dispose();
                    }
                    foreach (UnidadBackup u in _controlador_backups.BackupsLista)
                    {
                        u.Dispose();
                    }
                    foreach (ControladorTurnosBackup c in _turnos_backup.Values)
                    {
                        c.Dispose();
                    }

                    _aviones_dictionary.Clear();                    
                    _aeropuertos_dictionary.Clear();                                                         
                    _ac_type_dictionary.Clear();                                         
                    _tramos.Clear();                    
                    _turnos_backup.Clear();
                }
                _aeropuertos_dictionary = null;
                _ac_type_dictionary = null;
                _aviones_dictionary = null;
                _tramos = null;
                _get_aeropuerto = null;
                _get_avion = null;
                _get_flota = null;
                _get_backups = null;
                _estimar_utilizacion = null;
                _get_conexion = null;
                _get_minutos_proximo_vuelo = null;
                _fecha_inicio_string = null;
                _fecha_termino_string = null;
                _identificador = null;
                _usar_turno_backup = null;
            }
            IsDisposed = true;
        }
        
        #endregion
    }
}
