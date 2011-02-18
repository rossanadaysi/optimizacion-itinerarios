

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SimuLAN.Clases;
using SimuLAN.Clases.Disrupciones;
using SimuLAN.Clases.Recovery;
namespace SimuLAN.Clases
{
    /// <summary>
    /// Objeto que representa un réplica de simulación: un itinerario y su correcta operación
    /// </summary>
    public class Simulacion : IDisposable
    {
        #region ATRIBUTES

        /// <summary>
        /// Delegado que encapsula la actualización del tiempo de simulación
        /// </summary>
        private ActualizarTiempoSimulacionEventHandler _ats;

        /// <summary>
        /// Heap Binario: estructura de datos usada para representar los eventos más próximos de cada avión
        /// </summary>
        private BinaryHeap _eventos;

        /// <summary>
        /// Fecha de inicio del procesamiento de los reportes.
        /// </summary>
        private DateTime _fecha_ini;

        /// <summary>
        /// Fecha de término del procesamiento de los reportes.
        /// </summary>
        private DateTime _fecha_termino;

        /// <summary>
        /// Minutos entre los que se actualiza el clima para modelo basado en información de atrasos.
        /// </summary>
        private int _gap_clima;

        /// <summary>
        /// Delegado para obtener información de atrasos.
        /// </summary>
        private GetInformacionAtrasosEventHandler _get_info_atrasos;

        /// <summary>
        /// Estructura que encapsula la información de curvas de disrupciones.
        /// </summary>
        private ModeloDisrupciones _info_disrupciones;

        /// <summary>
        /// Itinerario representado en la simulación
        /// </summary>
        private Itinerario _itinerario;

        /// <summary>
        /// Set de parámetros propios de la simulación
        /// </summary>
        private ParametrosSimuLAN _parametros;

        /// <summary>
        /// Delegado que encapsula al algoritmo de recovery
        /// </summary>
        private RecoveryEventHandler _recovery;

        /// <summary>
        /// Diccionario que almacena el la puntualidad final para cada estándar. 
        /// </summary>
        private Dictionary<int, double> _std_calculado;

        /// <summary>
        /// Lista de los estándares de puntualidad a estimar.
        /// </summary>
        private List<int> _stds;

        /// <summary>
        /// Lista con los swaps realizados durante la simulación
        /// </summary>
        private List<Swap> _swaps;

        /// <summary>
        /// Reloj de actualización del clima
        /// </summary>
        private int _t_clima;

        /// <summary>
        /// Reloj de la simulación. Cuenta la cantidad de minutos transcurridos en la simulación
        /// </summary>
        private int _tiempo_simulacion;

        #endregion

        #region PROPIEDADES

        /// <summary>
        /// Itinerario representado en la simulación
        /// </summary>
        public Itinerario Itinerario
        {
            get { return _itinerario; }
            set { _itinerario = value; }
        }

        /// <summary>
        /// Diccionario que almacena el la puntualidad final para cada estándar. 
        /// </summary>
        public Dictionary<int, double> StdCalculado
        {
            get { return _std_calculado; }
            set { _std_calculado = value; }
        }

        /// <summary>
        /// Lista con los swaps realizados durante la simulación
        /// </summary>
        public List<Swap> Swaps
        {
            get { return _swaps; }
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Genera un objeto que representa un experimiento de simulación
        /// </summary>
        /// <param name="itinerario">Itinerario</param>
        /// <param name="parametros">Parámetros de la simulación</param>
        /// <param name="stds">Estándares para calcular la puntualidad</param>
        /// <param name="fechaIni">Fecha de inicio del procesamiento de los reportes.</param>
        /// <param name="fechaFin">Fecha de término del procesamiento de los reportes.</param>
        public Simulacion(Itinerario itinerario, ParametrosSimuLAN parametros, ModeloDisrupciones info_disrupciones, List<int> stds, DateTime fechaIni, DateTime fechaFin)
        {
            this._ats = new ActualizarTiempoSimulacionEventHandler(ActualizarTiempoSimulacion);
            this._eventos = new BinaryHeap();
            this._itinerario = itinerario;
            this._parametros = parametros;
            this._recovery = new RecoveryEventHandler(RecoveryAviones);
            int periodos_wxs = info_disrupciones.ColeccionDisrupciones[TipoDisrupcion.METEREOLOGIA.ToString()].CantidadDeValoresPorColumna(2);
            this._gap_clima = Convert.ToInt16(60 * 24 / periodos_wxs);
            this._get_info_atrasos = new GetInformacionAtrasosEventHandler(GetInformacionAtrasos);
            this._stds = stds;
            this._std_calculado = new Dictionary<int, double>();
            this._tiempo_simulacion = 0;
            this._swaps = new List<Swap>();
            this._info_disrupciones = info_disrupciones;
            this._fecha_ini = fechaIni;
            this._fecha_termino = fechaFin;
            //Inicializa cada avión
            foreach (Avion a in itinerario.AvionesDictionary.Values)
            {
                a.ToleranciaTurno = parametros.Escalares.ToleranciaTurno;
                a.ToleranciaRecovery = parametros.Escalares.Tolerancia;
                a.Recovery = _recovery;
                a.ActualizarTiempoSimulacion = _ats;
                a.GetInfoAtrasos = _get_info_atrasos;
                //Inserta en el heap cada avión con key igual al tiempo de su próximo despegue
                if (a.Tramo_Actual != null && a.EventosAvion.Count > 0)
                {
                    _eventos.Insert(-a.EventosAvion[0].TiempoInicioEvento, a);
                }
            }
        }

        #endregion

        #region PRIVATE METHODS

        #region CALCULOS

        /// <summary>
        /// Calcula la puntualidad del itinerario,
        /// </summary>
        /// <param name="s">Estándar de puntualidad</param>
        /// <returns>Porcentaje de puntualidad</returns>
        private double CalcularStd(double estandar)
        {
            float atrasosAceptables = 0;
            int nTramos = 0;
            foreach (Avion a in _itinerario.AvionesDictionary.Values)
            {
                Tramo aux = a.Tramo_Raiz;
                while (aux != null)
                {
                    if (aux.TramoBase.Fecha_Salida >= _fecha_ini && aux.TramoBase.Fecha_Salida <= _fecha_termino)
                    {
                        if (aux.TInicialRst - aux.TInicialProg <= estandar)
                        {
                            atrasosAceptables++;
                        }
                        nTramos++;
                        
                    }
                    aux = aux.Tramo_Siguiente;
                }
            }
            return nTramos > 0 ? Convert.ToDouble(atrasosAceptables / nTramos) : 0;
        }

        /// <summary>
        /// Estima la puntualidad en estándar 'std' para un negocio particular
        /// </summary>
        /// <param name="std">Estándar de puntualidad objetivo</param>
        /// <param name="negocio">Negocio objetivo</param>
        /// <returns></returns>
        private double CalcularStdNegocio(int std, string negocio)
        {
            float atrasosAceptables = 0;
            int nTramos = 0;
            foreach (Avion a in _itinerario.AvionesDictionary.Values)
            {
                Tramo aux = a.Tramo_Raiz;
                while (aux != null)
                {
                    if (aux.TramoBase.Fecha_Salida >= _fecha_ini && aux.TramoBase.Fecha_Salida <= _fecha_termino)
                    {
                        if (aux.Negocio == negocio)
                        {
                            if (aux.TInicialRst - aux.TInicialProg <= std)
                            {
                                atrasosAceptables++;
                            }
                            nTramos++;
                        }
                    }
                    aux = aux.Tramo_Siguiente;
                }
            }
            double punt = (nTramos > 0) ? Convert.ToDouble(atrasosAceptables / nTramos) : 0;
            return punt;
        }

        /// <summary>
        /// Estima el porcentaje de la impuntualidad para un tipo de disrupción en un estandar de puntualidad.
        /// </summary>
        /// <param name="tipo">Tipo de disrupción</param>
        /// <param name="estandar">Estándar de puntualidad</param>
        /// <returns>Porcentaje de impuntualidad</returns>
        private double EstimarImpuntualidadCausaAtraso(TipoDisrupcion tipo, int estandar)
        {
            double atrasosSobreEstandar = 0;
            int nTramos = 0;
            foreach (Avion a in _itinerario.AvionesDictionary.Values)
            {
                Tramo aux = a.Tramo_Raiz;
                while (aux != null)
                {
                    if (aux.TramoBase.Fecha_Salida >= _fecha_ini && aux.TramoBase.Fecha_Salida <= _fecha_termino)
                    {
                        if (aux.TInicialRst - aux.TInicialProg > estandar && aux.CausasAtraso.ContainsKey(tipo))
                        {
                            atrasosSobreEstandar += aux.CausasAtraso[tipo] / ((double)(aux.TInicialRst - aux.TInicialProg));
                        }
                        nTramos++;
                    }
                    aux = aux.Tramo_Siguiente;
                }
            }
            return atrasosSobreEstandar / nTramos;
        }

        /// <summary>
        /// Estima la puntualidad para cierto negocio en todos los estándares de puntualidad soportados.
        /// </summary>
        /// <param name="negocio">Negocio objetivo</param>
        /// <returns></returns>
        public Dictionary<int, double> EstimarPuntualidadNegocio(string negocio)
        {
            Dictionary<int, double> punt = new Dictionary<int, double>();
            foreach (int std in _stds)
            {
                punt.Add(std, CalcularStdNegocio(std, negocio));
            }
            return punt;
        }

        #endregion

        #region CONTROL SIMULACION

        /// <summary>
        /// Actualiza el estado del clima de todos los aeropuertos
        /// </summary>
        private void ActualizarClima()
        {
            _tiempo_simulacion = _t_clima;
            List<string> keys = new List<string>();
            foreach (string key in _itinerario.AeropuertosDictionary.Keys)
            {
                keys.Add(key);
            }
            for (int i = 0; i < _itinerario.AeropuertosDictionary.Count; i++)
            {
                int mes = _itinerario.FechaInicio.AddMinutes(_tiempo_simulacion).Month;
                _itinerario.AeropuertosDictionary[keys[i]].RevisarClima(_tiempo_simulacion, mes);
            }
            _t_clima += _gap_clima;
        }

        /// <summary>
        /// Actualiza el heap de eventos
        /// </summary>
        private void ActualizarEventos()
        {
            //Limpia el heap de eventos y para cada avión agrega el evento más próximo, siempre y cuando queden tramos y eventos.
            _eventos.Clear();
            foreach (Avion av in _itinerario.AvionesDictionary.Values)
            {
                if (av.Tramo_Actual != null && av.EventosAvion.Count > 0)
                {
                    _eventos.Insert(-av.EventosAvion[0].TiempoInicioEvento, av);
                }
            }
        }

        /// <summary>
        /// Actualiza el tiempo actual de la simulación a "t"
        /// </summary>
        /// <param name="t">minutos de simulación</param>
        private void ActualizarTiempoSimulacion(int t)
        {
            _tiempo_simulacion = t;
        }

        /// <summary>
        /// Carga Heap Binanario con los aviones.
        /// </summary>
        private void CargarHeap()
        {
            foreach (Avion av in _itinerario.AvionesDictionary.Values)
            {
                if (av.Tramo_Actual != null && av.EventosAvion.Count > 0)
                {
                    _eventos.Insert(-av.EventosAvion[0].TiempoInicioEvento, av);
                }
            }
        }

        /// <summary>
        /// Método para obtener las curvas de un tramo en operación
        /// </summary>
        /// <param name="tipoDisrupcion">Disrupción que se requiere consultar</param>
        /// <param name="keys">Argumentos de búsqueda</param>
        /// <param name="factorEscenario">Factor de variación según escenario simulado</param>
        /// <param name="infoAtrasoTramo">Estructura con los parámetros de la curva</param>
        /// <param name="distribucion">Distribucion de probabilidades de la curva</param>
        private void GetInformacionAtrasos(TipoDisrupcion tipoDisrupcion, List<string> keys, out double factorEscenario, out DataDisrupcion atrasoTramo, out DistribucionesEnum distribucion)
        {
            factorEscenario = 1;
            int dimension = _info_disrupciones.ColeccionDisrupciones[tipoDisrupcion.ToString()].Dimension;
            atrasoTramo = new DataDisrupcion(0, 0, 0);
            if (dimension == 1)
            {
                InfoDisrupcion1D info = (InfoDisrupcion1D)_info_disrupciones.ColeccionDisrupciones[tipoDisrupcion.ToString()];
                if (info.AplicaDesviacionEnEscenarios)
                {
                    TipoEscenarioDisrupcion escenario = _info_disrupciones.MapDisrupcionesEscenario[tipoDisrupcion];
                    factorEscenario = info.GetFactorDesviacionProb(keys[0], escenario);
                }
                if (info.Parametros.ContainsKey(keys[0]))
                {
                    atrasoTramo = info.Parametros[keys[0]];
                }
                else
                {
                    throw new Exception("Falta información de " + tipoDisrupcion.ToString() + " en: " + keys[0]);
                }
                distribucion = info.Distribucion;
            }
            else if (dimension == 2)
            {
                InfoDisrupcion2D info = (InfoDisrupcion2D)_info_disrupciones.ColeccionDisrupciones[tipoDisrupcion.ToString()];
                if (tipoDisrupcion == TipoDisrupcion.OTROS || tipoDisrupcion == TipoDisrupcion.RECURSOS_DEL_APTO || tipoDisrupcion == TipoDisrupcion.TA_BAJO_ALA || tipoDisrupcion == TipoDisrupcion.TA_SOBRE_ALA || tipoDisrupcion == TipoDisrupcion.TRIPULACIONES)
                {
                    keys[1] = Utils.Utilidades.GetPeriodo(Convert.ToInt16(keys[1]), info.CantidadDeValoresPorColumna(1));
                }
                if (info.AplicaDesviacionEnEscenarios)
                {
                    TipoEscenarioDisrupcion escenario = _info_disrupciones.MapDisrupcionesEscenario[tipoDisrupcion];
                    factorEscenario = info.GetFactorDesviacionProb(keys[0], escenario);
                }
                if (info.Parametros.ContainsKey(keys[0]) && info.Parametros[keys[0]].ContainsKey(keys[1]))
                {
                    atrasoTramo = info.Parametros[keys[0]][keys[1]];
                }
                else
                {
                    throw new Exception("Falta información de " + tipoDisrupcion.ToString() + " en: " + keys[0] + " ó " + keys[1]);
                }
                distribucion = info.Distribucion;
            }
            else
            {
                InfoDisrupcion3D info = (InfoDisrupcion3D)_info_disrupciones.ColeccionDisrupciones[tipoDisrupcion.ToString()];
                if (tipoDisrupcion == TipoDisrupcion.ATC || tipoDisrupcion == TipoDisrupcion.METEREOLOGIA)
                {
                    keys[2] = Utils.Utilidades.GetPeriodo(Convert.ToInt16(keys[2]), info.CantidadDeValoresPorColumna(2));
                }
                if (info.AplicaDesviacionEnEscenarios)
                {
                    TipoEscenarioDisrupcion escenario = _info_disrupciones.MapDisrupcionesEscenario[tipoDisrupcion];
                    factorEscenario = info.GetFactorDesviacionProb(keys[0], escenario);
                }
                if (info.Parametros.ContainsKey(keys[0]) && info.Parametros[keys[0]].ContainsKey(keys[1]) && info.Parametros[keys[0]][keys[1]].ContainsKey(keys[2]))
                {
                    atrasoTramo = info.Parametros[keys[0]][keys[1]][keys[2]];
                }
                else
                {
                    throw new Exception("Falta información de " + tipoDisrupcion.ToString() + " en: " + keys[0] + " ó " + keys[1] + " ó " + keys[2]);
                }
                distribucion = info.Distribucion;
            }
        }

        /// <summary>
        /// Verifica si la condición de término se ha cumplido
        /// </summary>
        /// <returns>True si ha terminado la simulación. False si queda algún tramo por operar.</returns>
        private bool VerificarFinDeSimulación()
        {
            foreach (Avion a in _itinerario.AvionesDictionary.Values)
            {
                if (a.Tramo_Actual != null)
                    return false;
            }
            return true;
        }

        #endregion

        #region ALGORITMO DE RECOVERY

        /// <summary>
        /// Método para buscar un swap que usa slots de backup
        /// </summary>
        /// <param name="emisor_avion">Avión afectado</param>
        /// <param name="emisor_tramo_inicio">Tramo afectado por un atraso reaccionario</param>
        /// <param name="atraso_reaccionario">Minutos de atraso reaccionario</param>
        /// <param name="backups">Lista con las unidades de backup disponibles</param>
        /// <param name="encuentraBackup">True si encuentra un swap útil</param>
        /// <param name="unidad_usada">Unidad de backup correspondiente al swap encontrado</param>
        /// <param name="necesitaActivarTurno">True si necesita activar un turno de backup por romper pairings</param>
        /// <returns></returns>
        private Swap BuscarBackup(Avion emisor_avion, Tramo emisor_tramo_inicio, int atraso_reaccionario, List<UnidadBackup> backups, out bool encuentraBackup, out UnidadBackup unidad_usada, out bool necesitaActivarTurno)
        {
            Swap swap_global = new Swap();
            int swap_valor_discriminante = int.MinValue;
            Tramo receptor_tramo_factible = null;
            int minutos_atraso_turno = 0;
            encuentraBackup = false;
            unidad_usada = null;
            //Siempre necesita activar turno de backup.
            necesitaActivarTurno = true;// !emisor_tramo_inicio.PasaRestriccionConexionParaRecovery(true);
            bool quedanTurnos = true;
            if (necesitaActivarTurno)
            {
                string grupo_flota = emisor_avion.GrupoAvion.Nombre;
                Aeropuerto origen_emisor = emisor_avion.GetAeropuerto(emisor_tramo_inicio.TramoBase.Origen);
                int tiempo_desplazo_turno = origen_emisor.Minutos_Llega_Turno;
                int tiempo_decision = emisor_avion.Tramo_Actual.TiempoInicialResultanteEstimado();
                int tiempo_llegada_turno = tiempo_desplazo_turno + tiempo_decision;
                minutos_atraso_turno = Math.Max(0, tiempo_llegada_turno - emisor_tramo_inicio.TiempoInicialResultanteEstimado());
                DateTime fecha_peticion = new DateTime(emisor_tramo_inicio.DtIniProg.Year, emisor_tramo_inicio.DtIniProg.Month, emisor_tramo_inicio.DtIniProg.Day);
                int hora_utc = emisor_tramo_inicio.DtIniProg.AddMinutes(emisor_tramo_inicio.TInicialRst - emisor_tramo_inicio.TInicialProg).Hour;
                int desfase_utc = origen_emisor.Horas_Desfase_UTC;
                int hora_local_peticion = (hora_utc - desfase_utc) % 24;
                quedanTurnos = Itinerario.TurnosBackup[grupo_flota].TurnoDisponible(hora_local_peticion, fecha_peticion);
            }
            if (!necesitaActivarTurno || (necesitaActivarTurno && quedanTurnos))
            {
                List<SlotBackup> backups_utiles = Itinerario.SlotsUtiles(this._tiempo_simulacion, emisor_tramo_inicio.TFinalRst, emisor_tramo_inicio.TramoBase.Origen);

                //Recorre todos los aviones
                foreach (SlotBackup bu in backups_utiles)
                {
                    //Setea avión receptor
                    Avion receptor_avion = _itinerario.AvionesDictionary[bu.Matricula];
                    receptor_tramo_factible = receptor_avion.GetTramoMasCercanoA(bu.TiempoFinRst);
                    //Se verifica que haya especio factible, que la búsqueda se haga en un avión distinto al emisor y que se comience y termine en el mismo aeropuerto
                    if (receptor_tramo_factible != null
                        && emisor_avion.IdAvion != receptor_avion.IdAvion
                        && receptor_tramo_factible.TramoBase.Origen == emisor_tramo_inicio.TramoBase.Origen)
                    {
                        //Se verifica la factibilidad de hacer un swap entre el avión emisor y el avión receptor
                        double compatibilidad_flotas = CompatibilidadFlotaFlota(emisor_avion.GetFlota(emisor_avion.AcType), receptor_avion.GetFlota(receptor_avion.AcType));
                        bool compatibles = (compatibilidad_flotas == 1);
                        if(!compatibles)
                        {
                            if(compatibilidad_flotas == 0)
                            {
                                compatibles = false;
                            }
                            else
                            {
                                Random rdm = new Random(emisor_tramo_inicio.TInicialProg);
                                compatibles = (rdm.NextDouble() <= compatibilidad_flotas);
                            }
                        }
                        if (compatibles)
                        {
                            //Se busca el mejor swap factible entre el avión emisor y receptor
                            Swap insercion_local = BuscarInsercion(emisor_tramo_inicio, receptor_tramo_factible, receptor_avion, emisor_avion, atraso_reaccionario, minutos_atraso_turno);
                            Swap swap_local = BuscarSwapEnProfundidad(emisor_tramo_inicio, receptor_tramo_factible, receptor_avion, emisor_avion, atraso_reaccionario, minutos_atraso_turno);
                            //Si el swap encontrado supera al anterior, se actualiza el swap elegido
                            if (swap_local.EsMejorQue(swap_global) && swap_local.ValorDiscriminante > swap_valor_discriminante)
                            {
                                encuentraBackup = true;
                                unidad_usada = bu.Contenedor;
                                swap_global = swap_local;
                                swap_global.TipoUsoBackup = UsoBackup.IniReceptor;
                                swap_valor_discriminante = swap_global.ValorDiscriminante;
                            }
                            if (insercion_local.EsMejorQue(swap_global) && insercion_local.ValorDiscriminante > swap_valor_discriminante)
                            {
                                encuentraBackup = true;
                                unidad_usada = bu.Contenedor;
                                swap_global = insercion_local;
                                swap_global.TipoUsoBackup = UsoBackup.IniReceptor;
                                swap_valor_discriminante = insercion_local.ValorDiscriminante;
                            }
                        }
                    }
                }
            }
            return swap_global;
        }

        /// <summary>
        /// Busca el mejor swap de inserción posible entre dos aviones
        /// </summary>
        /// <param name="tramo_ini_emisor">Primer tramo de la cadena del avión emisor</param>
        /// <param name="tramo_ini_receptor">Tramo posterior al slot donde se insertará la cadena del avión emisor</param>
        /// <param name="avion_receptor">Avión receptor de la cadena del avión afectado por atraso reaccionario</param>
        /// <param name="avion_emisor">Avión afectado por atraso reaccionario</param>
        /// <param name="atraso_reaccionario">Minutos de atraso reaccionario</param>
        /// <param name="minutos_atraso_turno">Minutos de atraso producto de la activación de un turno de backup</param>
        /// <returns>El mejor swap de inserción posible</returns>
        private Swap BuscarInsercion(Tramo emisor_tramo_inicio, Tramo receptor_tramo_factible, Avion receptor_avion, Avion emisor_avion, int atraso_reaccionario, int minutos_atraso_turno)
        {
            int swap_num_tramos_emisor = 1;
            Tramo tramo_emisor_aux = emisor_tramo_inicio;

            //Busca una cadena en el emisor que inicie y termine en la estación de origen del tramo receptor
            while (tramo_emisor_aux.Tramo_Siguiente != null
                && tramo_emisor_aux.TInicialProg < receptor_tramo_factible.TFinalRst
                && receptor_tramo_factible.TramoBase.Origen != tramo_emisor_aux.TramoBase.Destino
                && tramo_emisor_aux.MantenimientoPosterior == null)
            {
                tramo_emisor_aux = tramo_emisor_aux.Tramo_Siguiente;
                swap_num_tramos_emisor++;
            }

            //Caso inserción
            if (tramo_emisor_aux != null && tramo_emisor_aux.MantenimientoPosterior == null && receptor_tramo_factible.TramoBase.Origen == tramo_emisor_aux.TramoBase.Destino)//emisor_tramo_inicio.TramoBase.Origen == tramo_emisor_aux.TramoBase.Destino && swap_num_tramos_emisor > 1)
            {
                Swap swap_prueba = new Swap(receptor_tramo_factible, receptor_tramo_factible, emisor_tramo_inicio, tramo_emisor_aux, atraso_reaccionario, minutos_atraso_turno, TipoSwap.Insercion, UsoBackup.NoUsa);
                if (swap_prueba.NoRompeCadenaVuelo)
                    return swap_prueba;
                else return new Swap();
            }
            else
            {
                return new Swap();
            }
        }

        /// <summary>
        /// Busca una ventana temporal en el avión emisor que permita alguna recuperación
        /// </summary>
        /// <param name="receptor_tramo_inicial">Tramo actual del avión receptor</param>
        /// <param name="tiempo_limite">Tiempo resultante de salida del tramo afectado por atraso reaccionario en avión emisor</param>
        /// <param name="receptor_acType">Flota del avión receptor</param>
        /// <returns>Tramo inmediatamente posterior a la ventana factible</returns>
        private Tramo BuscarSlotInicialParaSwap(Tramo receptor_tramo_inicial, int tiempo_limite, string receptor_acType)
        {
            //Caso en que aún no se comienza a operar el primer tramo del avión receptor
            if (receptor_tramo_inicial != null
                && receptor_tramo_inicial.Tramo_Previo == null)
            {
                //Si primera leg no tiene mantto programado previo
                if (!receptor_tramo_inicial.GetAvion(receptor_tramo_inicial.TramoBase.Numero_Ac.ToString()).PrimerSlotEsMantenimiento)
                {
                    //Hay holgura antes de iniciar el próximo tramo (que no ha iniciado)
                    if (receptor_tramo_inicial.TInicialRst > tiempo_limite
                        && receptor_tramo_inicial.Estado == EstadoTramo.NoIniciado
                        && receptor_tramo_inicial.PasaRestriccionConexionParaRecovery(true)
                        && Itinerario.TramoSinBackup(receptor_tramo_inicial))
                    {
                        return receptor_tramo_inicial;
                    }
                    //Se revisa el tramo siguiente (no iniciado)
                    else
                    {
                        return BuscarSlotInicialParaSwap(receptor_tramo_inicial.Tramo_Siguiente, tiempo_limite, receptor_acType);
                    }
                }
                //Hay un mantenimiento antes del primer tramo. Se asume que no se puede hacer recovery. Se busca en el tramo siguiente.
                else
                {
                    return BuscarSlotInicialParaSwap(receptor_tramo_inicial.Tramo_Siguiente, tiempo_limite, receptor_acType);
                }
            }

            //Caso en que el avión no tiene tramos, o ya los voló todos, o posee un mantto justo antes.
            if (receptor_tramo_inicial == null
                || (receptor_tramo_inicial.Tramo_Previo != null && receptor_tramo_inicial.Tramo_Previo.MantenimientoPosterior != null)
                || !receptor_tramo_inicial.PasaRestriccionConexionParaRecovery(true))
                return null;

            //Se recorren los tramos
            //Condición de término: 
            //1). El tiempo límite está estrictamente entre el final del tramo previo y el inicio del tramo actual.
            //2). El tramo no debe estar iniciado
            //3). El tramo no debe estar conectado con pairing
            //4). El slot previo al tramo no debe contener un backup
            //Esto significa que se podría recuperar al menos un minuto posteriormente.
            int turnAround = receptor_tramo_inicial.GetTurnAroundMin(receptor_tramo_inicial);
            while (!(receptor_tramo_inicial.Tramo_Previo.TFinalRst + turnAround < tiempo_limite
                && receptor_tramo_inicial.TInicialRst > tiempo_limite
                && receptor_tramo_inicial.Estado == EstadoTramo.NoIniciado
                && receptor_tramo_inicial.PasaRestriccionConexionParaRecovery(true)
                && Itinerario.TramoSinBackup(receptor_tramo_inicial)))
            {
                //Se detiene la búsqueda cuando se ha superado el tiempo límite. 
                if (receptor_tramo_inicial.Tramo_Previo.TFinalRst + turnAround >= tiempo_limite)
                {
                    return null;
                }
                else
                {
                    //Se avanza al siguiente tramo
                    receptor_tramo_inicial = receptor_tramo_inicial.Tramo_Siguiente;
                    //Cuando se acaban los tramo o se encuentra con un mantto programado, se retorna null
                    if (receptor_tramo_inicial == null || receptor_tramo_inicial.Tramo_Previo.MantenimientoPosterior != null)
                        return null;
                    turnAround = receptor_tramo_inicial.GetTurnAroundMin(receptor_tramo_inicial);
                }
            }
            return receptor_tramo_inicial;
        }

        /// <summary>
        /// Busca el mejor swap para el tramo emisor_tramo_inicio operado por el avión emisor_avion que 
        /// trae un atraso reaccionario.
        /// </summary>
        /// <param name="emisor_avion">Avión afectado</param>
        /// <param name="emisor_tramo_inicio">Tramo afectado por un atraso reaccionario</param>
        /// <param name="atraso_reaccionario">Minutos de atraso reaccionario</param>
        /// <param name="encuentraSwap">True si el proceso es exitoso</param>
        /// <returns>Swap factible</returns>
        private Swap BuscarSwap(Avion emisor_avion, Tramo emisor_tramo_inicio, int atraso_reaccionario, out bool encuentraSwap, out List<Swap> swaps_usan_backup)
        {
            Swap swap_global = new Swap();
            swaps_usan_backup = new List<Swap>();
            int swap_valor_discriminante = int.MinValue;
            Tramo receptor_tramo_factible = null;
            encuentraSwap = false;

            //Se verifica que el tramo afectado en el emisor no rompa una conexión en el mismo avión.
            if (!emisor_tramo_inicio.PasaRestriccionConexionParaRecovery(true))
            {
                return new Swap();
            }

            List<string> ids_avion = new List<string>();
            foreach (string key in _itinerario.AvionesDictionary.Keys)
                ids_avion.Add(key);

            //Recorre todos los aviones
            foreach (string matricula in ids_avion)
            {
                //Setea avión receptor
                Avion receptor_avion = _itinerario.AvionesDictionary[matricula];
                //Se verifica que la búsqueda se haga en un avión distinto al emisor
                if (emisor_avion.IdAvion != receptor_avion.IdAvion)
                {
                    //Busca un espacio factible desde el cual iniciar búsqueda de swaps.Se filtran slots intermedios de conexion.
                    receptor_tramo_factible = BuscarSlotInicialParaSwap(receptor_avion.Tramo_Actual, emisor_tramo_inicio.TInicialRst + atraso_reaccionario, receptor_avion.AcType);

                    //Si hay un espacio factible
                    if (receptor_tramo_factible != null)
                    {
                        //Se chequea que haya coincidencia en el origen entre los tramos 
                        //de inicio del swap del avión emisor y receptor.
                        if (emisor_tramo_inicio.TramoBase.Origen == receptor_tramo_factible.TramoBase.Origen)
                        {
                            //Se verifica la factibilidad de hacer un swap entre el avión emisor y el avión receptor
                            double compatibilidad_flotas = CompatibilidadFlotaFlota(emisor_avion.GetFlota(emisor_avion.AcType), receptor_avion.GetFlota(receptor_avion.AcType));
                            bool compatibles = (compatibilidad_flotas == 1);
                            if(!compatibles)
                            {
                                if(compatibilidad_flotas == 0)
                                {
                                    compatibles = false;
                                }
                                else
                                {
                                    Random rdm = new Random(emisor_tramo_inicio.TInicialProg);
                                    compatibles = (rdm.NextDouble() <= compatibilidad_flotas);
                                }
                            }
                            if (compatibles)
                            {
                                //Se busca el mejor swap factible entre el avión emisor y receptor
                                Swap swap_local = BuscarSwapEnProfundidad(emisor_tramo_inicio, receptor_tramo_factible, receptor_avion, emisor_avion, atraso_reaccionario, 0);
                                Swap insercion_local = BuscarInsercion(emisor_tramo_inicio, receptor_tramo_factible, receptor_avion, emisor_avion, atraso_reaccionario, 0);
                                //Si el swap encontrado supera al anterior, se actualiza el swap elegido
                                if (swap_local.EsMejorQue(swap_global) && swap_local.ValorDiscriminante > swap_valor_discriminante)
                                {
                                    UsoBackup uso;
                                    if (swap_local.UsaSlotDeBackupAlFinal(Itinerario, out uso))
                                    {
                                        swap_local.TipoUsoBackup = uso;
                                        swaps_usan_backup.Add(swap_local);
                                        swaps_usan_backup.Sort();
                                    }
                                    else
                                    {
                                        encuentraSwap = true;
                                        swap_global = swap_local;
                                        swap_global.TipoUsoBackup = UsoBackup.NoUsa;
                                        swap_valor_discriminante = swap_local.ValorDiscriminante;
                                    }
                                }
                                if (insercion_local.EsMejorQue(swap_global) && insercion_local.ValorDiscriminante > swap_valor_discriminante)
                                {
                                    encuentraSwap = true;
                                    swap_global = insercion_local;
                                    swap_global.TipoUsoBackup = UsoBackup.NoUsa;
                                    swap_valor_discriminante = insercion_local.ValorDiscriminante;
                                }
                            }
                        }
                    }
                }
            }
            return swap_global;
        }

        /// <summary>
        /// Busca el mejor swap posible entre dos aviones
        /// </summary>
        /// <param name="tramo_ini_emisor">Primer tramo de la cadena del avión emisor</param>
        /// <param name="tramo_ini_receptor">Primer tramo de la cadena del avión receptor</param>
        /// <param name="avion_receptor">Avión receptor de la cadena del avión afectado por atraso reaccionario</param>
        /// <param name="avion_emisor">Avión afectado por atraso reaccionario</param>
        /// <param name="atraso_reaccionario">Minutos de atraso reaccionario</param>
        /// <param name="minutos_atraso_turno">Minutos de atraso producto de la activación de un turno de backup</param>
        /// <returns>El mejor swap posible</returns>
        private Swap BuscarSwapEnProfundidad(Tramo tramo_ini_emisor, Tramo tramo_ini_receptor, Avion avion_receptor, Avion avion_emisor, int atraso_reaccionario, int minutos_atraso_turno)
        {
            Swap swap_elegido = new Swap();
            bool primerSwap = true;
            int swap_num_tramos_emisor = 1;
            int swap_num_tramos_receptor = 0;
            Tramo tramo_emisor_aux = tramo_ini_emisor;
            Tramo tramo_receptor_aux = tramo_ini_receptor;
            bool cadenas_compatibles = true;

            /*Se eligen cadenas cada vez más largas en el avión receptor. 
            El límite de búsqueda viene dado por el parámetro Gap
            Las condiciones para avanzar son:
            1) No superar el gap
            2) Que la cadena del avión emisor tenga más de un tramo
            3) Que no hayan mantenimientos programados en la cadena
            4) Que todos los tramos de la cadena del receptor puedan ser operados por el avión emisor.
            5) Que la cadena del avión emisor sea factible (dado el tamaño de la cadena del receptor)
            */
            while (swap_num_tramos_receptor <= _parametros.Escalares.Gap
                && !(tramo_receptor_aux != null && tramo_receptor_aux.Tramo_Previo == null && tramo_receptor_aux.Tramo_Siguiente == null)
                && (tramo_receptor_aux.Tramo_Siguiente != null && !HayMantenimientosEntreTramos(tramo_ini_receptor, tramo_receptor_aux.Tramo_Siguiente))
                && (tramo_receptor_aux.Tramo_Siguiente != null && CompatibilidadMultioperador(avion_emisor.SubFlota, tramo_receptor_aux.TramoBase.Ac_Owner))
                && cadenas_compatibles)
            {
                //Inicialización
                tramo_emisor_aux = tramo_ini_emisor;
                swap_num_tramos_emisor = 1;

                //Se aumenta la cadena del avión receptor
                swap_num_tramos_receptor++;
                if (swap_num_tramos_receptor > 1)
                    tramo_receptor_aux = tramo_receptor_aux.Tramo_Siguiente;

                bool cadenaEmisorFactible = true;

                //Evalúa factibilidad de tramo emisor inicial
                if (tramo_emisor_aux != null
                    && (!CompatibilidadMultioperador(avion_receptor.SubFlota, tramo_emisor_aux.TramoBase.Ac_Owner)
                    || HayMantenimientosEntreTramos(tramo_ini_emisor, tramo_emisor_aux)))
                    cadenaEmisorFactible = false;

                //Se busca una cadena del avión emisor que sea factible en términos de que deben coincidir 
                //los destinos de las cadenas.
                while (cadenaEmisorFactible && tramo_emisor_aux != null && tramo_emisor_aux.Tramo_Siguiente !=null)                    
                {
                    tramo_emisor_aux = tramo_emisor_aux.Tramo_Siguiente;
                    swap_num_tramos_emisor++;
                    //Si no hay compatibilidad multioperador entre las cadenas, se detendrá la búsqueda.
                    bool compatibilidad_multioperador = CompatibilidadMultioperador(avion_receptor.SubFlota, tramo_emisor_aux.TramoBase.Ac_Owner);
                    //Si hay algún mantto entre la cadena del avión emisor, se detendrá la búsqueda.
                    bool hay_mantto_entre_tramos = HayMantenimientosEntreTramos(tramo_ini_emisor, tramo_emisor_aux);
                    //La cadena se hace infactible cuando no hay compatibilidad operacional o se presenta 
                    //un mantenimiento programado
                    if (tramo_emisor_aux != null
                        && (!compatibilidad_multioperador
                            || hay_mantto_entre_tramos))
                    {

                        cadenaEmisorFactible = false;
                    }


                    //Si la cadena del emisor es infactible y su largo es mucho menor a la del receptor, 
                    //se finaliza la búsqueda global.
                    if (!cadenaEmisorFactible)
                        
                    {
                        int diferenciaEmisor = tramo_emisor_aux.TFinalRst - tramo_ini_emisor.TInicialProg;
                        int diferenciaReceptor = tramo_receptor_aux.TFinalRst - tramo_ini_receptor.TFinRstTramoPrevio;
                        if (2 * diferenciaEmisor < diferenciaReceptor)
                        {
                            cadenas_compatibles = false;
                        }
                    }

                    //Se evalúa el swap (encuentra una cadena factible)
                    else if (tramo_emisor_aux != null && tramo_receptor_aux.TramoBase.Destino == tramo_emisor_aux.TramoBase.Destino)
                    {
                        Swap swap_prueba = new Swap(tramo_ini_receptor, tramo_receptor_aux, tramo_ini_emisor, tramo_emisor_aux, atraso_reaccionario, minutos_atraso_turno, TipoSwap.Normal, UsoBackup.NoUsa);
                        if (primerSwap && swap_prueba.NoRompeCadenaVuelo)
                        {
                            swap_elegido = swap_prueba;
                            primerSwap = false;
                        }
                        else if (swap_prueba.EsMejorQue(swap_elegido) && swap_prueba.NoRompeCadenaVuelo)
                        {
                            swap_elegido = swap_prueba;
                        }
                    }
                }
            }
            return swap_elegido;
        }

        /// <summary>
        /// Indica si el tramoOD se puede operar por una flota
        /// </summary>
        /// <param name="tramoOD">Tramo (par origen-destino)</param>
        /// <param name="flota">Flota que quiere operar el tramo</param>
        /// <returns>True si hay compatibilidad tramo-flota</returns>
        private double CompatibilidadFlotaFlota(string flota1, string flota2)
        {
            if (_parametros.MatrizFlotaFlota.ContainsKey(flota1))
            {
                if (_parametros.MatrizFlotaFlota[flota1].ContainsKey(flota2))
                {
                    return _parametros.MatrizFlotaFlota[flota1][flota2];
                }
                else
                {
                    throw new Exception("No hay matriz flota-flota para :" + flota2);
                }
            }
            else
            {
                throw new Exception("No hay matriz flota-flota para :" + flota1);
            }
        }

        /// <summary>
        /// Indica si una sublfota es compratible con un operador.
        /// </summary>
        /// <param name="subFlota">Subflota</param>
        /// <param name="owner">Operador</param>
        /// <returns>True su hay compatibilidad</returns>
        private bool CompatibilidadMultioperador(string subFlota, string owner)
        {
            if (_parametros.MatrizMultioperador.ContainsKey(subFlota))
            {
                if (_parametros.MatrizMultioperador[subFlota].ContainsKey(owner))
                {
                    if (_parametros.MatrizMultioperador[subFlota][owner] == 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Indica si hay algún slot de mantenimiento programado entre dos tramos inclusive
        /// </summary>
        /// <param name="tramo_ini">Tramo inicial</param>
        /// <param name="tramo_fin">Tramo final</param>
        /// <returns>True si hay mantenimiento programado</returns>
        private bool HayMantenimientosEntreTramos(Tramo tramo_ini, Tramo tramo_fin)
        {
            Tramo tramoAux = tramo_ini;
            while (tramoAux != tramo_fin)
            {
                if (tramoAux.MantenimientoPosterior != null)
                {
                    return true;
                }
                tramoAux = tramoAux.Tramo_Siguiente;
            }
            return false;
        }

        /// <summary>
        /// Ordena los tramos, ajustando por tiempos de inicio y fin resultantes y T/A.
        /// </summary>
        /// <param name="tramoInicial">Tramo inicial desde donde se comienza a ordenar</param>
        private void OrdenarTramos(Tramo tramoInicial)
        {
            bool corrigeCausasAtraso = true;
            if (tramoInicial != null)
            {
                if (tramoInicial.Estado != EstadoTramo.NoIniciado)
                {
                    tramoInicial = tramoInicial.Tramo_Siguiente;
                    corrigeCausasAtraso = false;
                }
                Tramo tramoAux;
                if (tramoInicial.Tramo_Previo != null)
                    tramoAux = tramoInicial.Tramo_Previo;
                else
                    tramoAux = tramoInicial;

                //MANTENIMIENTOS NO SE MUEVEN MAS DE LO PERMITIDO, PERO PUEDEN RECUPERARSE MINUTOS EN SLOT.
                //SE ASUME UN CUMPLIMIENTO DE T/A AL INICIO

                while (tramoAux != null && tramoAux.Tramo_Siguiente != null)
                {
                    if (tramoAux.MantenimientoPosterior == null)
                    {
                        int minutosLibres = tramoAux.Tramo_Siguiente.TInicialRst - tramoAux.Tramo_Siguiente.TFinRstTramoPrevio - tramoAux.Tramo_Siguiente.GetTurnAroundMin(tramoAux.Tramo_Siguiente);
                        //Adelanto o recupero minutos
                        if (minutosLibres >= 0)
                        {
                            int minutosRecuperados = Math.Min(minutosLibres, tramoAux.Tramo_Siguiente.TInicialRst - tramoAux.Tramo_Siguiente.TInicialProg);
                            tramoAux.Tramo_Siguiente.TInicialRst = tramoAux.Tramo_Siguiente.TInicialRst - minutosRecuperados;
                            tramoAux.Tramo_Siguiente.TFinalRst = tramoAux.Tramo_Siguiente.TFinalRst - minutosRecuperados;
                        }
                        //Atraso reaccionario
                        else
                        {
                            tramoAux.Tramo_Siguiente.TInicialRst = tramoAux.Tramo_Siguiente.TInicialRst - minutosLibres;
                            tramoAux.Tramo_Siguiente.TFinalRst = tramoAux.Tramo_Siguiente.TFinalRst - minutosLibres;
                        }
                        //Se actualizan las causas de atraso
                        if (corrigeCausasAtraso)
                        {
                            if (tramoAux.Tramo_Siguiente.TInicialProg == tramoAux.Tramo_Siguiente.TInicialRst)
                            {
                                tramoAux.Tramo_Siguiente.CausasAtraso.Clear();
                            }
                            else
                            {
                                int sumaAtrasos = 0;
                                foreach (TipoDisrupcion c in tramoAux.Tramo_Siguiente.CausasAtraso.Keys)
                                {
                                    sumaAtrasos += tramoAux.Tramo_Siguiente.CausasAtraso[c];
                                }
                                int atrasoReal = tramoAux.Tramo_Siguiente.TInicialRst - tramoAux.Tramo_Siguiente.TInicialProg;
                                if (atrasoReal < sumaAtrasos)
                                {
                                    int diferencia = sumaAtrasos - atrasoReal;
                                    if (tramoAux.Tramo_Siguiente.CausasAtraso.ContainsKey(TipoDisrupcion.RC) && tramoAux.Tramo_Siguiente.CausasAtraso.ContainsKey(TipoDisrupcion.HBT) && tramoAux.Tramo_Siguiente.CausasAtraso.Count == 2)
                                    {
                                        if (tramoAux.Tramo_Siguiente.CausasAtraso[TipoDisrupcion.RC] > diferencia)
                                        {
                                            tramoAux.Tramo_Siguiente.CausasAtraso[TipoDisrupcion.RC] -= diferencia;
                                        }
                                        else if (tramoAux.Tramo_Siguiente.CausasAtraso[TipoDisrupcion.RC] == diferencia)
                                        {
                                            tramoAux.Tramo_Siguiente.CausasAtraso.Remove(TipoDisrupcion.RC);
                                        }
                                        else if (tramoAux.Tramo_Siguiente.CausasAtraso[TipoDisrupcion.RC] < diferencia)
                                        {
                                            tramoAux.Tramo_Siguiente.CausasAtraso.Remove(TipoDisrupcion.RC);
                                            tramoAux.Tramo_Siguiente.CausasAtraso[TipoDisrupcion.HBT] = atrasoReal;
                                        }
                                    }
                                    else if (tramoAux.Tramo_Siguiente.CausasAtraso.ContainsKey(TipoDisrupcion.RC) && tramoAux.Tramo_Siguiente.CausasAtraso.Count == 1)
                                    {
                                        tramoAux.Tramo_Siguiente.CausasAtraso[TipoDisrupcion.RC] -= diferencia;
                                    }
                                    else if (tramoAux.Tramo_Siguiente.CausasAtraso.ContainsKey(TipoDisrupcion.HBT) && tramoAux.Tramo_Siguiente.CausasAtraso.Count == 1)
                                    {
                                        tramoAux.Tramo_Siguiente.CausasAtraso[TipoDisrupcion.HBT] -= diferencia;
                                    }
                                }
                            }
                        }
                    }

                    //SE TRABAJA CON MANTTOS
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
                    }
                    tramoAux = tramoAux.Tramo_Siguiente;
                }
            }
        }

        /// <summary>
        /// Busca el mejor swap para el tramo emisor_tramo_inicio operado por el avión emisor_avion que 
        /// trae un atraso reaccionario. Si no, intenta usar algún avión de backup.
        /// </summary>
        /// <param name="emisor_avion">Avión afectado</param>
        /// <param name="emisor_tramo_inicio">Tramo afectado por un atraso reaccionario</param>
        /// <param name="atraso_reaccionario">Minutos de atraso reaccionario</param>
        /// <returns>True si el recovery resultó exitoso</returns>
        private bool RecoveryAviones(Avion avion_emisor, Tramo tramo_ini_emisor, int atraso_reaccionario)
        {
            //Primero intenta hacer swaps.
            bool encuentraSwap;
            List<Swap> swaps_usan_backup;
            Swap mejorSwap = BuscarSwap(avion_emisor, tramo_ini_emisor, atraso_reaccionario, out encuentraSwap, out swaps_usan_backup);


            if (encuentraSwap)
            {
                mejorSwap.GetSlotsBackup = Itinerario.DelegateGetSlotsBackup;
                Avion avion_receptor = _itinerario.AvionesDictionary[mejorSwap.TramoIniReceptor.IdAvionProgramadoActual];
                mejorSwap.AplicarSwap(avion_emisor, avion_receptor);
                OrdenarTramos(avion_receptor.Tramo_Actual);
                OrdenarTramos(avion_emisor.Tramo_Actual);
                avion_emisor.ActualizarListaEventos();
                avion_receptor.ActualizarListaEventos();
                avion_emisor.RecoveryReciente = true;
                avion_receptor.RecoveryReciente = true;
                _swaps.Add(mejorSwap);
                ActualizarEventos();
                return true;
            }
            //Si no pudo hacer swap, intenta usar avión de backup
            else if (atraso_reaccionario >= _parametros.Escalares.MinBackup)
            {
                bool encuentraBackup;
                UnidadBackup bu;
                bool necesitaActivarTurno;
                Swap mejorBackup = BuscarBackup(avion_emisor, tramo_ini_emisor, atraso_reaccionario, Itinerario.ControladorBackups.BackupsLista.ToList(), out encuentraBackup, out bu, out necesitaActivarTurno);
                if (encuentraBackup || swaps_usan_backup.Count > 0)
                {
                    SeleccionarMejorSwap(ref mejorBackup, ref bu, ref necesitaActivarTurno, swaps_usan_backup);
                    mejorBackup.GetSlotsBackup = Itinerario.DelegateGetSlotsBackup;
                    Avion avion_receptor = _itinerario.AvionesDictionary[mejorBackup.TramoIniReceptor.IdAvionProgramadoActual];
                    if (mejorBackup.TramoPreBackup.TInicialProg <= bu.TiempoFinPrg)
                    {
                        bu.AddSwap(_swaps.Count.ToString(), mejorBackup);
                    }
                    else
                    {
                        //Swap es válido, pero no se registra en los slots de backups
                    }
                    mejorBackup.AplicarSwap(avion_emisor, avion_receptor);
                    if (necesitaActivarTurno)
                    {
                        avion_emisor.UsarTurnoBackup(tramo_ini_emisor, Math.Max(mejorBackup.MinutosAtrasoTurno, mejorBackup.MinutosAtrasoReaccionarioInicial));
                    }
                    OrdenarTramos(avion_receptor.Tramo_Actual);
                    OrdenarTramos(avion_emisor.Tramo_Actual);
                    avion_emisor.ActualizarListaEventos();
                    avion_receptor.ActualizarListaEventos();
                    avion_emisor.RecoveryReciente = true;
                    avion_receptor.RecoveryReciente = true;
                    _swaps.Add(mejorBackup);
                    ActualizarEventos();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Selecciona el mejor swap de una lista basado en el criterio del discriminante
        /// </summary>
        /// <param name="mejorBackup">Swap independiente</param>
        /// <param name="bu">Unidad de backup perteneciente al swap elegido</param>
        /// <param name="swaps_usan_backup">Lista de swaps</param>
        private void SeleccionarMejorSwap(ref Swap mejorBackup, ref UnidadBackup bu, ref bool necesitaActivarTurno, List<Swap> swaps_usan_backup)
        {
            foreach (Swap s in swaps_usan_backup)
            {
                if (s.ValorDiscriminante > mejorBackup.ValorDiscriminante)
                {
                    mejorBackup = s;
                    bu = Itinerario.TramoSinBackup_UnidadBackup(s.TramoPostBackup);
                    //Cuando hay rotación y se usa el backup, no es necesario activar un turno.
                    necesitaActivarTurno = false;
                }
            }
        }

        #endregion

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Se estima la puntualidad de la réplica simulada en los estándares definidos en "Stds"
        /// </summary>
        public void EstimarPuntualidadReplica()
        {
            if (_stds != null)
            {
                foreach (int std in _stds)
                {
                    _std_calculado.Add(std, CalcularStd(std));
                }
            }
        }

        /// <summary>
        /// Genera la explicación de impuntualidad por causa de atraso dentro.
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, Dictionary<TipoDisrupcion, double>> ImpuntualidadPorCausasDeAtraso()
        {
            Dictionary<int, Dictionary<TipoDisrupcion, double>> retorno = new Dictionary<int, Dictionary<TipoDisrupcion, double>>();
            foreach (int estandar in _stds)
            {
                retorno.Add(estandar, new Dictionary<TipoDisrupcion, double>());
                foreach (TipoDisrupcion tipo in TipoDisrupcion.GetValues(typeof(TipoDisrupcion)))
                {
                    if (tipo != TipoDisrupcion.ADELANTO)
                    {
                        retorno[estandar].Add(tipo, EstimarImpuntualidadCausaAtraso(tipo, estandar));
                    }
                }
            }
            return retorno;
        }

        /// <summary>
        /// Se comienza el ciclo de simulación
        /// </summary>
        public void Simular()
        {
            int contadorEntradas = 0;
            try
            {
                //Se actualiza el estado del clima
                ActualizarClima();
                //Utiliza backup por efecto de AOG
                _itinerario.ControladorBackups.GenerarAOGs(_itinerario.FechaInicio, _itinerario.FechaTermino, _parametros.InfoAOG);
                //Se itinera mientras se cumpla la condición de término
                bool recargarHeap = false;
                int t_evento = 0;
                int cuentaRepeticiones = 0;
                while (!VerificarFinDeSimulación())
                {
                    contadorEntradas++;
                    //Se inicializa el heap de eventos
                    if (_eventos.Count == 0 || recargarHeap)
                    {
                        _eventos.Clear();
                        CargarHeap();
                        cuentaRepeticiones = 0;
                        recargarHeap = false;
                    }

                    //Se extrae el avión con el evento más cercano
                    Avion a = (Avion)_eventos.Remove();
                    if (a.Tramo_Actual != null && a.EventosAvion.Count > 0)
                    {
                        //Se comparan los tiempos de eventos de avión (tramos) y aeropuertos (actualización clima)

                        if (_t_clima < a.EventosAvion[0].TiempoInicioEvento)
                        {
                            //Se actualiza el clima
                            ActualizarClima();
                            //Se reinserta el nodo del avión
                            _eventos.Insert(-a.EventosAvion[0].TiempoInicioEvento, a);
                        }
                        else
                        {
                            //Se ejecuta el método que encapsula el evento.
                            int t_aux = a.EventosAvion[0].TiempoInicioEvento;
                            bool actualizaCurrent = a.EventosAvion[0].AccionEvento();
                            //Secuencia que reinicia heap cuando falla.
                            if (t_evento == t_aux)
                            {
                                cuentaRepeticiones++;
                                if (cuentaRepeticiones > 100)
                                {
                                    recargarHeap = true;
                                }
                            }
                            else
                            {
                                t_evento = t_aux;
                                cuentaRepeticiones = 0;
                            }
                            //El método anterior retorno un bool que indica si hay que mantener o cambiar el tramo actual
                            if (actualizaCurrent)
                            {
                                //Se avanza al tramo siguiente
                                if (a.Tramo_Actual != null && a.RecienAterrizado)
                                    a.Tramo_Actual = a.Tramo_Actual.Tramo_Siguiente;

                            }

                            //Si le quedan aventos al avión, se reinserta el nodo al heap.
                            if (a.EventosAvion.Count > 0)
                            {
                                _eventos.Insert(-a.EventosAvion[0].TiempoInicioEvento, a);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Excepción no controlada en proceso de simulación. Entrada número: " + contadorEntradas + ". Tiempo simulación: " + _tiempo_simulacion + " Excepción: " + e);
            }
        }

        #endregion

        #region IDisposable Members

        private bool IsDisposed = false;

        /// <summary>
        /// Destructor de instancias de la clase
        /// </summary>
        ~Simulacion()
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
                    _itinerario.Dispose();
                    foreach (Swap s in _swaps)
                    {
                        s.Dispose();
                    }
                    _swaps.Clear();
                }
                _ats = null;
                _eventos = null;
                _recovery = null;
                _swaps = null;
                _itinerario = null;
                _parametros = null;
                _info_disrupciones = null;
                _get_info_atrasos = null;
            }
            IsDisposed = true;
        }

        #endregion

    }
}

