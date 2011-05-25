using System;
using System.Collections.Generic;
using System.Text;
using SimuLAN.Clases;
using System.Xml.Serialization;
using SimuLAN.Utils;

namespace SimuLAN.Clases
{
    /// <summary>
    /// Objeto que encapsula un tramo de vuelo
    /// </summary>
    [XmlRoot("leg")]
    public class Tramo : IComparable,ICloneable,IDisposable
    {
        #region CONSTANTS

        /// <summary>
        /// Indica el nombre del negocio considerado por defecto cuando no hay información
        /// </summary>
        private const string NEGOCIO_DEFAULT = "SIN";

        #endregion

        #region ATRIBUTES

        /// <summary>
        /// Diccionario que guarda las causas de atraso que afectaron el tramo, junto con su duración en minutos
        /// </summary>
        private Dictionary<TipoDisrupcion, int> _causas_atraso;

        private SerializableList<ConexionLegs> _conexiones_pairing_posteriores;   
        private SerializableList<ConexionLegs> _conexiones_pax_posteriores;
        private SerializableList<ConexionLegs> _conexiones_pairing_anteriores;
        private SerializableList<ConexionLegs> _conexiones_pax_anteriores;
        /// <summary>
        /// Contador para todos los tramos de vuelo representados en la simulación
        /// </summary>
        [XmlIgnore]
        public static int _cuenta_tramos = 0;

        /// <summary>
        /// Fecha inicial de aterrizaje (Fecha y Hora)
        /// </summary>
        private DateTime _date_time_fin_prog;

        /// <summary>
        /// Fecha inicial de despegue (Fecha y Hora)
        /// </summary>
        private DateTime _date_time_ini_prog;

        /// <summary>
        /// Indica si el tramo actual debe esperar a un tramo de otro avión por conexiones de tripulantes
        /// </summary>
        private bool _espera_tramo_por_conexion_pairing;

        /// <summary>
        /// Indica si el tramo actual debe esperar a un tramo de otro avión por conexiones de pasajeros
        /// </summary>
        private bool _espera_tramo_por_conexion_pasajeros;

        /// <summary>
        /// Estado del tramo: { NoIniciado, Iniciado, EnProceso, Finalizado}
        /// </summary>
        private EstadoTramo _estado;

        /// <summary>
        /// Flota del avión que operó el tramo
        /// </summary>
        private string _flota_operada;

        /// <summary>
        /// Flota del avión al que fue asignado el tramo
        /// </summary>
        private string _flota_programada;

        /// <summary>
        /// Delegado que encapsula método para obtener el objeto "Avion" correspondiente al atributo "matrícula" del tramo.
        /// </summary>
        private GetAvionEventHandler _get_avion;

        /// <summary>
        /// Delegado que entrega una conexión de pairing para un tramo específico
        /// </summary>
        private GetConexionEventHandler _get_conexion;

        /// <summary>
        /// Delegado que entrega los minutos que faltan para que se repita en alguna parte del itinerario el mismo par OD del tramo actual.
        /// </summary>
        private GetMinutosHastaProximoVueloEventHandler _get_minutos_proximo_vuelo;

        /// <summary>
        /// Delegado que encapsula el método para obtener el turn around mínimo correspondiente al origen del tramo
        /// </summary>
        private GetTurnAroundMinEventHandler _get_turn_around_min;
        private Dictionary<int,int> _holguras_delante_para_cada_conexion;
        private Dictionary<int, int> _holguras_atras_para_cada_conexion;
        /// <summary>
        /// Matrícula del avión que operó el tramo
        /// </summary>
        private string _id_avion_operado;

        /// <summary>
        /// Matrícula del avión al que fue asignado el tramo
        /// </summary>
        private string _id_avion_programado;

        /// <summary>
        /// Matrícula del avión al que fue reasignado durante la simulación
        /// </summary>
        private string _id_avion_programado_actual;

        /// <summary>
        /// Identificar del tramo creado para la implementación de vuelos en conexión
        /// </summary>
        private string _id_hub;

        /// <summary>
        /// Identificador del tramo para reporte de puntualidad
        /// </summary>
        private string _id_vuelo_reporte;

        /// <summary>
        /// Indica si el tramo actual inicia una conexion de tripulantes
        /// </summary>
        private bool _inicia_conexion_pairing;

        /// <summary>
        /// Referencia al slot de mantenimiento posterior, si es que tiene.
        /// </summary>
        private SlotMantenimiento _mantenimiento_posterior;

        /// <summary>
        /// Indica el negocio o ruta comercial al que pertenece el tramo
        /// </summary>
        private string _negocio;

        /// <summary>
        /// Origen - Destino
        /// </summary>
        private string _par_OD;

        /// <summary>
        /// Objeto para la generación de números aleatorios para el tramo
        /// </summary>
        private Random _rdm;

        /// <summary>
        /// Tiempo de aterrizaje programado
        /// </summary>
        private int _t_final_prog;

        /// <summary>
        /// Tiempo de aterrizaje resultante
        /// </summary>
        private int _t_final_rst;

        /// <summary>
        /// Tiempo de despegue programado
        /// </summary>
        private int _t_inicial_prog;

        /// <summary>
        /// Tiempo de despegue resultante
        /// </summary>
        private int _t_inicial_rst;

        /// <summary>
        /// Tramo base leido desde archivo Excel del itinerario
        /// </summary>
        private TramoBase _tramo_base;

        /// <summary>
        /// Indica si el tramo es posterior a una cadena de swap
        /// </summary>
        [XmlIgnore]
        private bool _tramo_post_cadena_swap;

        /// <summary>
        /// True si el tramo es un vuelo de tipo HUB
        /// </summary>
        private bool _vuelo_hub;

        /// <summary>
        /// Referencia al tramo previo en el itinerario del avión portador
        /// </summary>
        [XmlIgnore]
        public Tramo Tramo_Previo;

        ///  /// <summary>
        /// Referencia al tramo posterior en el itinerario del avión portador
        /// </summary>
        [XmlIgnore]
        public Tramo Tramo_Siguiente;

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

        public SerializableList<ConexionLegs> ConexionesPairingPosteriores
        {
            get
            {
                if (_conexiones_pairing_posteriores == null)
                {
                    this._conexiones_pairing_posteriores = this.GetConexion(Convert.ToInt32(this.TramoBase.Numero_Global), TipoConexion.Pairing, false);
                }
                return _conexiones_pairing_posteriores;
            }
        }

        public SerializableList<ConexionLegs> ConexionesPaxPosteriores
        {
            get
            {
                if (_conexiones_pax_posteriores == null)
                {
                    this._conexiones_pax_posteriores = this.GetConexion(Convert.ToInt32(this.TramoBase.Numero_Global), TipoConexion.Pasajeros, false);
                }
                return _conexiones_pax_posteriores;
            }
        }

        public SerializableList<ConexionLegs> ConexionesPairingAnteriores
        {
            get
            {
                if (_conexiones_pairing_anteriores == null)
                {
                    this._conexiones_pairing_anteriores = this.GetConexion(Convert.ToInt32(this.TramoBase.Numero_Global), TipoConexion.Pairing, true);
                }
                return _conexiones_pairing_anteriores;
            }
        }

        public SerializableList<ConexionLegs> TodasLasConexionesPosteriores
        {
            get
            {
                SerializableList<ConexionLegs> conexiones = new SerializableList<ConexionLegs>();
                conexiones.AddAll(ConexionesPairingPosteriores);
                conexiones.AddAll(ConexionesPaxPosteriores);
                return conexiones;
            }
        }


        public SerializableList<ConexionLegs> TodasLasConexionesAnteriores
        {
            get
            {
                SerializableList<ConexionLegs> conexiones = new SerializableList<ConexionLegs>();
                conexiones.AddAll(ConexionesPairingAnteriores);
                conexiones.AddAll(ConexionesPaxAnteriores);
                return conexiones;
            }
        }
        public SerializableList<ConexionLegs> ConexionesPaxAnteriores
        {
            get
            {
                if (_conexiones_pax_anteriores == null)
                {
                    this._conexiones_pax_anteriores = this.GetConexion(Convert.ToInt32(this.TramoBase.Numero_Global), TipoConexion.Pasajeros, true);
                }
                return _conexiones_pax_anteriores;
            }
        }
        /// <summary>
        /// Fecha inicial de aterrizaje (Fecha y Hora)
        /// </summary>
        public DateTime DtFinProg
        {
            get { return _date_time_fin_prog; }
            set { _date_time_fin_prog = value; }
        }
        
        /// <summary>
        /// Fecha inicial de despegue (Fecha y Hora)
        /// </summary>
        public DateTime DtIniProg 
        {
            get { return _date_time_ini_prog; }
            set { _date_time_ini_prog = value; }
        }

        /// <summary>
        /// Indica si el tramo actual debe esperar a un tramo de otro avión por conexiones de tripulantes
        /// </summary>
        public bool EsperaTramoPorConexionPairing
        {
            get { return _espera_tramo_por_conexion_pairing; }
            set { _espera_tramo_por_conexion_pairing = value; }
        }

        /// <summary>
        /// Indica si el tramo actual debe esperar a un tramo de otro avión por conexiones de pasajeros
        /// </summary>
        public bool EsperaTramoPorConexionPasajeros
        {
            get { return _espera_tramo_por_conexion_pasajeros; }
            set { _espera_tramo_por_conexion_pasajeros = value; }
        }

        /// <summary>
        /// Estado del tramo: { NoIniciado, Iniciado, EnProceso, Finalizado}
        /// </summary>
        public EstadoTramo Estado
        {
            get { return _estado; }
            set { _estado = value; }
        }

        /// <summary>
        /// Flota del avión que operó el tramo
        /// </summary>
        public string FlotaOperada
        {
            get { return _flota_operada; }
            set { _flota_operada = value; }
        }

        /// <summary>
        /// Flota del avión al que fue asignado el tramo
        /// </summary>
        public string FlotaProgramada
        {
            get { return _flota_programada; }
            set { _flota_programada = value; }
        }

        /// <summary>
        /// Delegado que encapsula método para obtener el objeto "Avion" correspondiente al atributo "matrícula" del tramo.
        /// </summary>
        [XmlIgnore]
        public GetAvionEventHandler GetAvion
        {
            get { return _get_avion; }
            set { _get_avion = value; }
        }

        /// <summary>
        /// Delegado que entrega una conexión de pairing para un tramo específico
        /// </summary>
        [XmlIgnore]
        public GetConexionEventHandler GetConexion
        {
            get { return _get_conexion; }
            set { _get_conexion = value; }
        }

        /// <summary>
        /// Delegado que entrega los minutos que faltan para que se repita en alguna parte del itinerario el mismo par OD del tramo actual.
        /// </summary>
        [XmlIgnore]
        public GetMinutosHastaProximoVueloEventHandler GetMinutosProximoVuelo
        {
            get { return _get_minutos_proximo_vuelo; }
            set { _get_minutos_proximo_vuelo = value; }
        }

        /// <summary>
        /// Delegado que encapsula el método para obtener el turn around mínimo correspondiente al origen del tramo
        /// </summary>
        [XmlIgnore]
        public GetTurnAroundMinEventHandler GetTurnAroundMinimo
        {
            get { return _get_turn_around_min; }
            set { _get_turn_around_min = value; }
        }

        /// <summary>
        /// True si antes del tramo hay un mantenimiento programado
        /// </summary>
        [XmlIgnore]
        public bool HayMantenimientoAnterior
        {
            get 
            {
                if (Tramo_Previo != null)
                {
                    return Tramo_Previo.MantenimientoPosterior != null;
                }
                else
                {
                    Avion a = this.GetAvion(this.IdAvionProgramadoActual);
                    return a.PrimerSlotEsMantenimiento;
                }            
            }
        }

        /// <summary>
        /// Hour Block Time en formato hh:mm
        /// </summary>
        [XmlIgnore]
        public string HBTProg
        {
            get
            {
                int duracion = _t_final_prog - _t_inicial_prog;
                int minutos = duracion % 60;
                int horas = Convert.ToInt16(Math.Floor(duracion / 60.0));
                string minutos_str = (minutos.ToString().Length == 0) ? "00" : (minutos.ToString().Length == 1) ? "0" + minutos.ToString() : minutos.ToString();
                string horas_str = (horas.ToString().Length == 0) ? "00" : (horas.ToString().Length == 1) ? "0" + horas.ToString() : horas.ToString();
                return horas_str + ":" + minutos_str;
            }
        }

        /// <summary>
        ///Hora resultante de operación del tramo
        /// </summary>
        [XmlIgnore]
        public int HoraActual
        {
            get { return this.DtIniProg.AddMinutes(this.TInicialRst - this.TInicialProg).Hour; }
        }

        /// <summary>
        /// Matrícula del avión que operó el tramo
        /// </summary>
        public string IdAvionOperado
        {
            get { return _id_avion_operado; }
            set { _id_avion_operado = value; }
        }

        /// <summary>
        /// Matrícula del avión al que fue asignado el tramo
        /// </summary>
        public string IdAvionProgramado
        {
            get { return _id_avion_programado; }
            set { _id_avion_programado = value; }
        }

        /// <summary>
        /// Matrícula del avión al que fue reasignado durante la simulación
        /// </summary>
        [XmlIgnore]
        public string IdAvionProgramadoActual
        {
            get { return _id_avion_programado_actual; }
            set { _id_avion_programado_actual = value; }
        }

        /// <summary>
        /// Identificar del tramo creado para la implementación de vuelos en conexión
        /// </summary>
        public string IdHub
        {
            get { return _id_hub; }
            set { _id_hub = value; }
        }

        /// <summary>
        /// Identificador del tramo para reporte de puntualidad
        /// </summary>
        public string IdVueloReporte
        {
            get { return _id_vuelo_reporte; }
            set { _id_vuelo_reporte = value; }
        }
        
        /// <summary>
        /// Indica si el tramo actual inicia una conexion de tripulantes
        /// </summary>
        public bool IniciaConexionPairing
        {
            get{return _inicia_conexion_pairing;}
            set{_inicia_conexion_pairing = value;}
        }
        
        /// <summary>
        /// Identificador usado para los tramos que son HUB en la salida
        /// </summary>
        [XmlIgnore]
        public string KeyHUB
        {
            get 
            {
                return _tramo_base.Ac_Owner + _tramo_base.Numero_Vuelo + _tramo_base.Origen + _tramo_base.Destino;
            }
        }
        
        /// <summary>
        /// Identificador usado para la tabla de disrupción de mantto.
        /// </summary>
        [XmlIgnore]
        public string KeyMantto
        {
            get 
            {
                Avion a = this._get_avion(this._id_avion_programado_actual);
                return a.Matricula + " " + a.GetFlota(a.AcType); 
            }
        }

        /// <summary>
        /// Referencia al slot de mantenimiento posterior, si es que tiene.
        /// </summary>
        [XmlIgnore]
        public SlotMantenimiento MantenimientoPosterior
        {
            get { return _mantenimiento_posterior; }
            set { _mantenimiento_posterior = value; }
        }
        
        /// <summary>
        ///Mes correspondiente al tramo 
        /// </summary>
        [XmlIgnore]
        public int Mes
        {
            get { return this.DtIniProg.AddMinutes(this.TInicialRst - this.TInicialProg).Month; }
        }
        
        public List<Tramo> TramosConectadosDelante
        {
            get
            {
                List<Tramo> tramos = new List<Tramo>();
                SerializableList<ConexionLegs> conexiones_posteriores = this.TodasLasConexionesPosteriores;                
                if (conexiones_posteriores != null && conexiones_posteriores.Count > 0)
                {
                    foreach (ConexionLegs c in conexiones_posteriores)
                    {
                        Tramo tramo = c.GetTramo(c.NumTramoFin);
                        tramos.Add(tramo);
                    }
                }
                return tramos;                
            }
        }
        
        public List<Tramo> TramosConectadosAtras
        {
            get
            {
                List<Tramo> tramos = new List<Tramo>();
                SerializableList<ConexionLegs> conexiones_anteriores = this.TodasLasConexionesAnteriores;
                
                if (conexiones_anteriores != null && conexiones_anteriores.Count > 0)
                {
                    foreach (ConexionLegs c in conexiones_anteriores)
                    {
                        Tramo tramo = c.GetTramo(c.NumTramoIni);
                        tramos.Add(tramo);
                    }
                }
                return tramos;
            }
        }
        
        public Dictionary<int, int> HolgurasDelanteParaCadaConexion
        {
            get 
            {
                if (_holguras_delante_para_cada_conexion == null)
                {
                    _holguras_delante_para_cada_conexion = new Dictionary<int, int>();
                    if (this.Tramo_Siguiente != null)
                    {
                        int tiempo_fin_corregido = 0;
                        if (_mantenimiento_posterior != null)
                        {
                            tiempo_fin_corregido = this.TFinalProg + _mantenimiento_posterior.Duracion;
                        }
                        else
                        {
                            tiempo_fin_corregido = this.TFinalProg;
                        }
                        _holguras_delante_para_cada_conexion.Add(this.Tramo_Siguiente.TramoBase.Numero_Global, this.Tramo_Siguiente.TInicialProg - tiempo_fin_corregido);
                        foreach (Tramo t in this.TramosConectadosDelante)
                        {
                            if (!_holguras_delante_para_cada_conexion.ContainsKey(t.TramoBase.Numero_Global))
                            {
                                _holguras_delante_para_cada_conexion.Add(t.TramoBase.Numero_Global, t.TInicialProg - tiempo_fin_corregido);
                            }
                        }
                    }
                }
                return _holguras_delante_para_cada_conexion;
            }
        }

        public Dictionary<int, int> HolgurasAtrasParaCadaConexion
        {
            get
            {
                if (_holguras_atras_para_cada_conexion == null)
                {
                    _holguras_atras_para_cada_conexion = new Dictionary<int, int>();
                    if (this.Tramo_Previo != null)
                    {
                        int tiempo_fin_corregido = 0;
                        if (this.Tramo_Previo._mantenimiento_posterior != null)
                        {

                            tiempo_fin_corregido = this.TInicialProg - this.Tramo_Previo._mantenimiento_posterior.Duracion;
                        }
                        else
                        {
                            tiempo_fin_corregido = this.TInicialProg;
                        }
                        _holguras_atras_para_cada_conexion.Add(this.Tramo_Previo.TramoBase.Numero_Global, tiempo_fin_corregido - this.Tramo_Previo.TFinalProg);
                        foreach (Tramo t in this.TramosConectadosAtras)
                        {
                            if (!_holguras_atras_para_cada_conexion.ContainsKey(t.TramoBase.Numero_Global))
                            {
                                _holguras_atras_para_cada_conexion.Add(t.TramoBase.Numero_Global, tiempo_fin_corregido - t.TFinalProg);
                            }
                        }
                    }
                }
                return _holguras_atras_para_cada_conexion;
            }
        }

        public int HolguraAtrasMaxima
        {
            get
            {
                Dictionary<int, int> holguras = this.HolgurasAtrasParaCadaConexion;
                if (holguras.Count > 0)
                {
                    int minimo = int.MaxValue;
                    foreach (int id_tramo in holguras.Keys)
                    {
                        if (holguras[id_tramo] < minimo)
                        {
                            minimo = holguras[id_tramo];
                        }
                    }
                    return minimo;
                }
                else
                {
                    return 0;
                }
            }
        }

        public int HolguraDelanteMaxima
        {
            get 
            {
                Dictionary<int, int> holguras = this.HolgurasDelanteParaCadaConexion;
                if (holguras.Count > 0)
                {                    
                    int minimo = int.MaxValue;
                    foreach (int id_tramo in holguras.Keys)
                    {
                        if (holguras[id_tramo] < minimo)
                        {
                            minimo = holguras[id_tramo];
                        }
                    }
                    return minimo;
                }
                else
                {
                    return 0;
                }
            }
        }

        public int MinutosMaximaVariacionDelante
        {
            get 
            {
                if (this.Tramo_Siguiente != null)
                {
                    int retorno = HolguraDelanteMaxima - TurnAroundMinimoDestino;
                    return retorno;
                }
                else
                {
                    return 0;
                }
            }
        }

        public int MinutosMaximaVariacionAtras
        {
            get
            {
                if (this.Tramo_Previo != null)
                {
                    int retorno = HolguraAtrasMaxima - TurnAroundMinimoOrigen;
                    return retorno;
                }
                else
                {
                    return 0;
                }
            }
        }

        public int TurnAroundMinimoOrigen
        {
            get
            {
                return this.GetTurnAroundMinimo(this);                
            }
        }

        public int TurnAroundMinimoDestino
        {
            get
            {
                if (this.Tramo_Siguiente != null)
                {
                    int retorno = this.GetTurnAroundMinimo(this.Tramo_Siguiente);
                    return retorno;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Indica el negocio o ruta comercial al que pertenece el tramo
        /// </summary>
        public string Negocio
        {
            get { return _negocio; }
            set { _negocio = value; }
        }

        /// <summary>
        /// Origen - Destino
        /// </summary>
        public string ParOD
        {
            get { return _par_OD; }
            set { _par_OD = value; }
        }

        /// <summary>
        /// Indica si él avión puede despegar o no dependiendo del estado del tramo revisado.
        /// </summary>
        [XmlIgnore]
        public bool PuedeDespegar
        {
            get
            {
                if (this.Estado == EstadoTramo.NoIniciado)
                {
                    return false;
                }
                else return true;
            }
        }

        /// <summary>
        /// Objeto para la generación de números aleatorios para el tramo
        /// </summary>
        [XmlIgnore]
        public Random RdmTramo
        {
            get { return _rdm; }
        }

        /// <summary>
        /// Retorna tiempo de inicio del tramo, si este fuera recuperado a su tiempo de inicio programado
        /// </summary>
        [XmlIgnore]
        public int TiempoInicialRecuperado
        {
            get
            {
                return Math.Max(_t_inicial_prog, TFinRstTramoPrevio + GetTurnAroundMinimo(this));
            }
        }

        /// <summary>
        /// Tiempo de aterrizaje programado
        /// </summary>
        public int TFinalProg
        {
            get { return _t_final_prog; }
            set { _t_final_prog = value; }
        }

        /// <summary>
        /// Tiempo de aterrizaje resultante
        /// </summary>
        public int TFinalRst
        {
            get { return _t_final_rst; }
            set { _t_final_rst = value; }
        }

        /// <summary>
        /// Entrega el tiempo final programado más el turn around del tramo previo al actual
        /// </summary>
        [XmlIgnore]
        public int TFinProgMasTATramoPrevio
        {
            get
            {
                if (this.Tramo_Previo == null)
                {
                    return 0;
                }
                else
                {
                    return Tramo_Previo.TInicialProg + this.GetTurnAroundMinimo(this);
                }
            }    
        }

        /// <summary>
        /// Retorna tiempo fin resultante de tramo previo de la cadena.
        /// </summary>
        [XmlIgnore]
        public int TFinRstTramoPrevio
        {
            get 
            {
                if (this.Tramo_Previo == null)
                {                    
                    return 0;
                }
                else if (this.Tramo_Previo.MantenimientoPosterior != null)
                {
                    return Tramo_Previo.MantenimientoPosterior.TiempoFinManttoRst;
                }
                else
                {
                    return Tramo_Previo.TFinalRst;
                }
            }
        }

        /// <summary>
        /// Retorna tiempo inicial programado de tramo previo de la cadena.
        /// </summary>
        [XmlIgnore]
        public int TIniProgTramoPrevio
        {
            get
            {
                if (this.Tramo_Previo == null)
                {
                    return 0;
                }
                else
                {
                    return Tramo_Previo.TInicialProg;
                }
            }
        }

        /// <summary>
        /// Retorna tiempo inicial resultante de tramo previo de la cadena.
        /// </summary>
        [XmlIgnore]
        public int TIniRstTramoPrevio
        {
            get
            {
                if (this.Tramo_Previo == null)
                {
                    return 0;
                }
                else
                {
                    return Tramo_Previo.TInicialRst;
                }
            }
        } 

        /// <summary>
        /// Tiempo de despegue programado
        /// </summary>
        public int TInicialProg
        {
            get { return _t_inicial_prog; }
            set { _t_final_prog = value; }
        }

        /// <summary>
        /// Tiempo de despegue resultante
        /// </summary>
        public int TInicialRst
        {
            get { return _t_inicial_rst; }
            set { _t_inicial_rst = value; }
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
        /// Indica si el tramo es posterior a una cadena de swap
        /// </summary>
        [XmlIgnore]
        public bool TramoPostCadenaSwap
        {
            get { return _tramo_post_cadena_swap; }
            set { _tramo_post_cadena_swap = value; }
        }

        /// <summary>
        /// True si el tramo es un vuelo de tipo HUB
        /// </summary>
        public bool VueloHUB 
        {
            get { return _vuelo_hub; }
            set { _vuelo_hub = value; }
        }

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Constructor para serializacion
        /// </summary>
        public Tramo()
        { }

        /// <summary>
        /// Constructor de un tramo de vuelo
        /// </summary>
        /// <param name="tramoBase">Tramo base leído desde itinerario</param>
        /// <param name="tramoPrevio">Referencia a tramo previo</param>
        /// <param name="fechaBase">Fecha base del itinerario</param>
        public Tramo(TramoBase tramoBase, Tramo tramoPrevio,DateTime fechaBase)
        {
            this.TramoBase = tramoBase;
            this.Tramo_Previo = tramoPrevio;
            this._causas_atraso = new Dictionary<TipoDisrupcion, int>();
            this._date_time_ini_prog = tramoBase.Fecha_Salida;
            this._date_time_fin_prog = tramoBase.Fecha_Llegada;
            CargarInfoTemporal(TramoBase.Hora_Salida, TramoBase.Hora_Llegada, fechaBase);
            this._id_avion_operado = this._tramo_base.Numero_Ac.ToString();
            this._id_avion_programado = _id_avion_operado;
            this._id_avion_programado_actual = _id_avion_operado;
            this._par_OD = this.TramoBase.Origen.ToString() + "-" + this.TramoBase.Destino.ToString();
            this._id_hub = this.TramoBase.Ac_Owner.ToString() + this.TramoBase.Numero_Vuelo.ToString() + this.TramoBase.Origen + this.TramoBase.Destino;
            this._id_vuelo_reporte = this._par_OD + this._tramo_base.Numero_Vuelo.ToString();
            this._espera_tramo_por_conexion_pairing = false;
            this._espera_tramo_por_conexion_pasajeros = false;
            this._inicia_conexion_pairing = false;
            this._negocio = NEGOCIO_DEFAULT;
            this._vuelo_hub = false;
            this._tramo_post_cadena_swap = false;
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Carga información de tiempos de despegue y aterrizaje del tramo
        /// </summary>
        /// <param name="horaSalida">Hora de salida del vuelo</param>
        /// <param name="horaLlegada">Hora de aterrizaje del vuelo</param>
        /// <param name="fechaBase">Fecha de inicio del itinerario</param>
        private void CargarInfoTemporal(string horaSalida, string horaLlegada,DateTime fechaBase)
        {
            int diasDesfaseSalida = Convert.ToInt32((_date_time_ini_prog - fechaBase).TotalDays);
            int diasDesfaseLlegada = Convert.ToInt32((_date_time_fin_prog - fechaBase).TotalDays);
            int horaSalidaNum = Utilidades.ConvertirMinutosDesdeHoraString(horaSalida);
            int horaLlegadaNum = Utilidades.ConvertirMinutosDesdeHoraString(horaLlegada);
            this._t_inicial_prog = diasDesfaseSalida * 24 * 60 + horaSalidaNum;
            this._t_inicial_rst = TInicialProg;
            this._t_final_prog = diasDesfaseLlegada * 24 * 60 + horaLlegadaNum;
            this._t_final_rst = _t_final_prog;
            this._date_time_ini_prog = _date_time_ini_prog.AddHours(Convert.ToInt32(Math.Truncate(horaSalidaNum / 60.0)));
            this._date_time_ini_prog = _date_time_ini_prog.AddMinutes(horaSalidaNum % 60);
            this._date_time_fin_prog = _date_time_fin_prog.AddHours(Convert.ToInt32(Math.Truncate(horaLlegadaNum / 60.0)));
            this._date_time_fin_prog = _date_time_fin_prog.AddMinutes(horaLlegadaNum % 60);
        }

        #endregion

        #region INTERNAL METHODS

        /// <summary>
        /// Agrega o actualiza una causa de atraso sobre el tramo
        /// </summary>
        /// <param name="causa">Causa de atraso</param>
        /// <param name="tiempo">Minutos de atraso</param>
        internal void AgregarCausaDeAtraso(TipoDisrupcion causa, int tiempo)
        {
            if (tiempo > 0)
            {
                if (_causas_atraso.ContainsKey(causa))
                    _causas_atraso[causa] += tiempo;
                else
                    _causas_atraso.Add(causa, tiempo);
            }
        }

        /// <summary>
        /// Indica si el tramo actual puede ser parte de una cadena de un swap, dependiendo de si tiene conexión.
        /// </summary>
        /// <returns>True si se puede hacer recovery</returns>
        internal bool PasaRestriccionConexionParaRecovery(bool esSegundoTramo)
        {
            SerializableList<ConexionLegs> conexiones;// this.GetConexion(this.TramoBase.Numero_Global, TipoConexion.Pairing, esSegundoTramo);
            if (esSegundoTramo)
            {
                conexiones = this.ConexionesPairingPosteriores;
            }
            else
            {
                conexiones = this.ConexionesPairingAnteriores;
            }
            if (conexiones.Count == 0)
            {
                return true;
            }
            else
            {
                foreach (ConexionLegs c in conexiones)
                {
                    //Si a pesar de que hay conexión, esta es entre aviones distintos, también hay factibilidad.
                    if (c.MismoAvion)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Asigna una semilla al Random del tramo en función de la semilla de nivel "Réplica" y un número único de cada tramo programado (en función de la fecha y hora programada).
        /// </summary>
        /// <param name="semilla">Número que determina la secuencia de números aleatorios asignada al tramo</param>
        internal void PonerSemilla(int semilla)
        {
            int num_semilla = this.TramoBase.Numero_Global;//this.TInicialProg + this.TFinalProg;
            this._rdm = new Random(semilla + num_semilla);
        }

        /// <summary>
        /// Retorno el tiempo inicial resultante del tramo en función del estado del mismo y de los tramo previos.
        /// </summary>
        /// <returns>Tiempo estimado de arrivo</returns>
        internal int TiempoInicialResultanteEstimado()
        {
            Avion avion = this.GetAvion(this.IdAvionOperado);
            int tiempoInicialTramo = 0;
            int atrasoDespegue = 0;
            if (this.Estado == EstadoTramo.NoIniciado)
            {
                tiempoInicialTramo = this.TInicialRst;
                foreach (TipoDisrupcion c in this.CausasAtraso.Keys)
                {
                    tiempoInicialTramo += this.CausasAtraso[c];
                }
                if (tiempoInicialTramo == this.TInicialRst)
                { //Se recalcula atraso en función de lo ocurrido con el tramo anterior 
                    Tramo tramoAuxPrevio = this.Tramo_Previo;
                    if (tramoAuxPrevio != null)
                    {
                        int sumaAtrasos = 0;
                        foreach (TipoDisrupcion c in tramoAuxPrevio.CausasAtraso.Keys)
                        {
                            sumaAtrasos += tramoAuxPrevio.CausasAtraso[c];
                        }
                        int atrasoRealEstimado = Math.Max(sumaAtrasos, tramoAuxPrevio.TInicialRst - tramoAuxPrevio.TInicialProg);
                        int atrasoPropagado = Math.Max(0, atrasoRealEstimado + tramoAuxPrevio.TFinalProg + this.GetTurnAroundMinimo(this) - this.TInicialRst);
                        tiempoInicialTramo += atrasoPropagado;
                    }
                }
                else
                {

                }
            }
            else if (this.Estado == EstadoTramo.Iniciado)
            {
                atrasoDespegue = avion.AtrasoDespegue;
                tiempoInicialTramo = this.TInicialRst + atrasoDespegue;
            }
            //En proceso o Finalizado
            else
            {
                tiempoInicialTramo = this.TInicialRst;
            }
            return tiempoInicialTramo;
        }

        /// <summary>
        /// Retorno el tiempo final resultante del tramo en función del estado del mismo y de los tramo previos.
        /// </summary>
        /// <returns>Tiempo estimado de arrivo</returns>
        internal int TiempoFinalResultanteEstimado()
        {
            Avion avion = this.GetAvion(this.IdAvionOperado);
            int tiempoFinalTramo = 0;
            int atrasoDespegue = 0;
            int atrasoVuelo = 0;
            if (this.Estado == EstadoTramo.NoIniciado)
            {
                tiempoFinalTramo = this.TFinalRst;
                foreach (TipoDisrupcion c in this.CausasAtraso.Keys)
                {
                    tiempoFinalTramo += this.CausasAtraso[c];
                }
                if (tiempoFinalTramo == this.TFinalRst)
                { //Se recalcula atraso en función de lo ocurrido con el tramo anterior 
                    Tramo tramoAuxPrevio = this.Tramo_Previo;
                    if (tramoAuxPrevio != null)
                    {
                        int sumaAtrasos = 0;
                        foreach (TipoDisrupcion c in tramoAuxPrevio.CausasAtraso.Keys)
                        {
                            sumaAtrasos += tramoAuxPrevio.CausasAtraso[c];
                        }
                        int atrasoRealEstimado = Math.Max(sumaAtrasos, tramoAuxPrevio.TInicialRst - tramoAuxPrevio.TInicialProg);
                        int atrasoPropagado = Math.Max(0, atrasoRealEstimado + tramoAuxPrevio.TFinalProg + this.GetTurnAroundMinimo(this) - this.TInicialRst);
                        tiempoFinalTramo += atrasoPropagado;
                    }
                }
                else
                {

                }
            }
            else if (this.Estado == EstadoTramo.Iniciado)
            {
                atrasoDespegue = avion.AtrasoDespegue;
                atrasoVuelo = avion.AtrasoVuelo;
                tiempoFinalTramo = this.TFinalRst + atrasoDespegue + atrasoVuelo;
            }
            else if (this.Estado == EstadoTramo.EnProceso)
            {
                atrasoVuelo = avion.AtrasoVuelo;
                tiempoFinalTramo = this.TFinalRst + atrasoVuelo;
            }
            //Finalizado
            else
            {
                tiempoFinalTramo = this.TFinalRst;
            }
            return tiempoFinalTramo;
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Genera un string con los detalles un tramo para el reporte de detalles de SimuLAN
        /// </summary>
        /// <param name="tramo">Tramo objetivo</param>
        /// <param name="contReplicas">Réplica correspondiente a la simulación</param>
        /// <param name="stdPuntualidad">Lista con los estándares de puntualidad calculados en los reportes</param>
        /// <returns>Lista de strings con los detalles del tramo para cada causa de atraso registrada</returns>
        public List<string> DatosItinerarioToString(Tramo tramo, int contReplicas, List<int> stdPuntualidad)
        {
            List<String> retorno = new List<string>();
            int[] cumpleSTD = new int[stdPuntualidad.Count];
            string id_vuelo = tramo.TramoBase.Numero_Global.ToString();
            string num_vuelo = tramo._tramo_base.Numero_Vuelo;
            string operador = tramo.TramoBase.Ac_Owner;
            string flotaOperada = tramo.FlotaOperada;
            string flotaProgramada = tramo.FlotaProgramada;
            string avionOperado = tramo.IdAvionOperado;
            string avionProgramado = tramo.IdAvionProgramado;
            string acTypeOperado = tramo.GetAvion(tramo.IdAvionOperado).AcType;
            string acTypeProgramado = tramo.TramoBase.AcType;
            string origen = tramo.TramoBase.Origen;
            string destino = tramo.TramoBase.Destino;
            string dia = Convert.ToString(Math.Ceiling((decimal)tramo.TInicialProg / 1440));
            DateTime fecha_std = tramo.DtIniProg;
            DateTime fecha_atd = tramo.DtIniProg.AddMinutes(tramo.TInicialRst - tramo.TInicialProg);
            string std = Utilidades.ConvertirHorario(tramo.TInicialProg);
            string atd = Utilidades.ConvertirHorario(tramo.TInicialRst);
            string dtd = Convert.ToString(tramo.TInicialRst - tramo.TInicialProg);
            DateTime fecha_sta = tramo.DtFinProg;
            DateTime fecha_ata = tramo.DtFinProg.AddMinutes(tramo.TFinalRst - tramo.TFinalProg);
            string sta = Utilidades.ConvertirHorario(tramo.TFinalProg);
            string ata = Utilidades.ConvertirHorario(tramo.TFinalRst);
            string dta = Convert.ToString(tramo.TFinalRst - tramo.TFinalProg);
            string estado = tramo.Estado.ToString();
            string negocio = tramo.Negocio;
            for (int i = 0; i < stdPuntualidad.Count; i++)
            {
                cumpleSTD[i] = 0;
                if (tramo.TInicialRst - tramo.TInicialProg <= stdPuntualidad[i])
                {
                    cumpleSTD[i]++;
                }
            }
            string filaBase = contReplicas + "\t" + id_vuelo + "\t" + operador + "\t" + negocio + "\t" + num_vuelo + "\t" + flotaOperada + "\t" + acTypeOperado + "\t" + avionOperado + "\t" + flotaProgramada
                     + "\t" + acTypeProgramado + "\t" + avionProgramado + "\t" + origen + "\t" + destino + "\t" + origen + "-" + destino + "\t" + dia + "\t";
            filaBase += fecha_std.ToShortDateString() + "\t" + fecha_atd.ToShortDateString() + "\t" + fecha_std.ToShortTimeString() + "\t" + fecha_atd.ToShortTimeString() + "\t" + dtd + "\t";
            filaBase += fecha_sta.ToShortDateString() + "\t" + fecha_ata.ToShortDateString() + "\t" + fecha_sta.ToShortTimeString() + "\t" + fecha_ata.ToShortTimeString() + "\t" + dta + "\t";

            if (tramo.CausasAtraso.Count == 0)
            {
                filaBase += "0\t ";
                for (int i = 0; i < stdPuntualidad.Count; i++)
                {
                    filaBase += "\t" + cumpleSTD[i];
                }
                retorno.Add(filaBase);
            }
            else
            {
                int contador = 0;
                foreach (TipoDisrupcion causa in tramo.CausasAtraso.Keys)
                {
                    string causaAtraso = causa.ToString();
                    if (causaAtraso.StartsWith("BG"))
                    {
                        causaAtraso = "BG";
                    }
                    string filaAtrasoEspecifico = filaBase + tramo.CausasAtraso[causa] + "\t" + causaAtraso;
                    for (int i = 0; i < stdPuntualidad.Count; i++)
                    {
                        filaAtrasoEspecifico += "\t" + Math.Max(Math.Min(2 * contador, 2), cumpleSTD[i]);
                    }
                    retorno.Add(filaAtrasoEspecifico);
                    contador++;
                }
            }
            return retorno;
        }

        /// <summary>
        /// Compara dos tramos en base a su par O-D y número de vuelo
        /// </summary>
        /// <param name="tramo">Tramo a comparar</param>
        /// <returns>True si el tramo objetivo es igual al actual</returns>
        public bool EsSimilarA(Tramo tramo)
        {
            //Agregar parte del dia para Clima y ATC
            if (this._par_OD == tramo._par_OD && this.TramoBase.Numero_Vuelo == tramo.TramoBase.Numero_Vuelo)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.IdHub.ToString() + "\t" + _t_inicial_prog.ToString() + "\t-" + _t_final_prog.ToString();
        }

        #endregion
        
        #region IComparable Members

        /// <summary>
        /// Comparación en base al tiempo de despegue resultante
        /// </summary>
        /// <param name="obj">Tramo a comparar</param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            Tramo di = obj as Tramo;
            if (TInicialRst < di.TInicialRst)
            {
                return -1;
            }
            else if (TInicialRst > di.TInicialRst)
            {
                return 1;
            }
            else
                return 0;
        }

        #endregion

        #region ICloneable Members

        /// <summary>
        /// Clona el tramo actual
        /// </summary>
        /// <returns>Tramo clonado</returns>
        public object Clone()
        {
            Tramo t = new Tramo();
            t._espera_tramo_por_conexion_pairing = this._espera_tramo_por_conexion_pairing;
            t._espera_tramo_por_conexion_pasajeros = this._espera_tramo_por_conexion_pasajeros;
            t._inicia_conexion_pairing = this._inicia_conexion_pairing;
            t.TramoBase = (TramoBase)this.TramoBase.Clone();
            t._causas_atraso = new Dictionary<TipoDisrupcion, int>();
            t._date_time_fin_prog = this._date_time_fin_prog;
            t._date_time_ini_prog = this._date_time_ini_prog;
            t._estado = this._estado;
            t._flota_operada = this._flota_operada;
            t._flota_programada = this._flota_programada;
            t._get_turn_around_min = this._get_turn_around_min;
            t._id_hub = this._id_hub;
            t._mantenimiento_posterior = this._mantenimiento_posterior;
            t._id_avion_operado = this._id_avion_operado;
            t._id_avion_programado = this._id_avion_programado;
            t._id_avion_programado_actual = this._id_avion_programado_actual;
            t._par_OD = this._par_OD;
            t._t_final_prog = this._t_final_prog;
            t._t_final_rst = this.TFinalProg;
            t._t_inicial_prog = this._t_inicial_prog;
            t._t_inicial_rst = this._t_inicial_prog;
            t._id_vuelo_reporte = this._id_vuelo_reporte;
            t._vuelo_hub = this._vuelo_hub;
            t._negocio = this._negocio;
            if (this._mantenimiento_posterior != null)
            {
                t.MantenimientoPosterior = _mantenimiento_posterior.Clonar();
                t.MantenimientoPosterior.TramoPrevio = t;
            }
            else
            {
                t._mantenimiento_posterior = null;
            }
            return t;
        }

        #endregion

        #region IDisposable Members

        private bool IsDisposed=false;

        /// <summary>
        /// Destructor de instancias de la clase
        /// </summary>
        ~Tramo()
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
                    _causas_atraso.Clear();
                }
                _causas_atraso = null;
                _tramo_base = null;
                _get_turn_around_min = null;
                _get_avion = null;
                _get_minutos_proximo_vuelo = null;
                _get_conexion = null;
                _flota_operada = null;
                _flota_programada = null;
                _id_hub = null;
                _id_vuelo_reporte = null;
                _mantenimiento_posterior = null;
                _id_avion_operado = null;
                _id_avion_programado = null; 
                _id_avion_programado_actual = null;
                _par_OD = null;
                _rdm = null;
                _id_vuelo_reporte = null;
                _negocio = null;
            }
            IsDisposed=true;
        }
        
        #endregion

        internal void ReprogramarTramo(int movimiento)
        {
            if (movimiento > 0)
            {
                if (movimiento > MinutosMaximaVariacionDelante)
                {
                    movimiento = MinutosMaximaVariacionDelante;
                }
            }
            else
            {
                if (-movimiento > MinutosMaximaVariacionAtras)
                {
                    movimiento = -MinutosMaximaVariacionAtras;
                }
            }
            if (movimiento != 0)
            {
                _t_final_prog += movimiento;
                _t_final_rst += movimiento;
                _t_inicial_prog += movimiento;
                _t_inicial_rst += movimiento;
            }
        }
    }
}
