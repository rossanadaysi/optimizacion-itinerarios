using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using SimuLAN;
using SimuLAN.Clases;
using SimuLAN.Clases.Disrupciones;
using InterfazSimuLAN.Utils;
using InterfazSimuLAN.Reportes;
using System.Diagnostics;
using System.Data.SqlClient;
using InterfazSimuLAN.AccesoData;
using SimuLAN.Clases.ControlInformacion;
using BrightIdeasSoftware;
using SimuLAN.Utils;
using SimuLAN.Clases.Recovery;
using SimuLAN.Clases.Optimizacion;

namespace InterfazSimuLAN
{


    /// <summary>
    /// Delegado para inicilizar listas desplegadas en tab de itinerario.
    /// </summary>
    public delegate void CargarListasEventHandler();

    /// <summary>
    /// Enumeración con las posibles extensiones que puede tener el itinerario.
    /// </summary>
    public enum OrigenItinerario { XLS, CSV };

    /// <summary>
    /// Clase con los métodos de accion que controlan la interfaz principal de SimuLAN.
    /// </summary>
    public partial class InterfazSimuLAN : Form
    {
        #region PRIVATE ATRIBUTES

        /// <summary>
        /// Instancia de delegado para desplegar mensajes en el label de la interfaz.
        /// </summary>    
        private EnviarMensajeEventHandler _enviarMensajeInterfaz;

        /// <summary>
        /// Instancia de delegado para desplegar mensajes en el label de los menúes de simulación.
        /// </summary> 
        private EnviarMensajeEventHandler _enviarMensajeSimulacion;

        /// <summary>
        /// True si se han realizado cambios en los parámetros
        /// </summary>
        private bool _hayCambiosEnParametros;

        /// <summary>
        /// Instancia de menú de simulación multiescenario.
        /// </summary>
        private MenuSimulacionMultiescenario _menuSimulacionMultiescenario;

        /// <summary>
        /// Instancia de menú de simulación normal.
        /// </summary>
        private MenuSimulacionNormal _menuSimulacionNormal;

        /// <summary>
        /// True si hay curvas cargadas exitosamente.
        /// </summary>
        private bool _OK_Distribuciones;

        /// <summary>
        /// True si hay itinerario cargado exitosamente.
        /// </summary>
        private bool _OK_Itinerario;

        /// <summary>
        /// True si hay parámetros cargado exitosamente.
        /// </summary>
        private bool _OK_Parametros;

        #endregion

        #region INTERNAL ATRIBUTES

        /// <summary>
        /// True si es hay que abrir la carpeta de output que contiene las carpetas con los reportes.
        /// </summary>
        internal bool _abrirCarpetaOutput;

        /// <summary>
        /// True si la ventana del actualizador de curvas está abierto.
        /// </summary>
        internal bool _actualizadorAbierto;

        /// <summary>
        /// Objeto que almacena la información de configuración de la interfaz.
        /// </summary>
        internal Configuracion _config;

        /// <summary>
        /// Diccionario que indica para cada grupo de reporte si deben crearse las hojas de cálculo con la información agrupada.
        /// </summary>
        internal Dictionary<GruposReporte, bool> _escribeGrupo;

        /// <summary>
        /// Diccionario que indica para cada reporte si debe escribirse.
        /// </summary>
        internal Dictionary<NombreReporte, bool> _escribeReporte;

        /// <summary>
        ///  Objeto que encapsula la información de factores de probabilidad por escenarios. 
        ///  Es sobreescrito cuando se cargan sucesivamente archivos de parámetros, los que contienen dos hojas de cálculo con la información por escenarios.
        /// </summary>
        internal Dictionary<TipoDisrupcion, Dictionary<string, Dictionary<TipoEscenarioDisrupcion, double>>> _factoresEscenarios;

        /// <summary>
        /// Itinerario usado en la simulación. Es sobreescrito cuando se cargan otros sucesivamente.
        /// </summary>
        internal Itinerario _itinerarioBase;

        /// <summary>
        /// Objeto que encapsula la información de disrupciones. Es sobreescrito cuando se cargan sucesivamente archivos de curvas.
        /// </summary>
        internal ModeloDisrupciones _modeloDisrupcionesBase;

        /// <summary>
        /// String con la ruta de la carpeta que contiene las carpetas con los reportes.
        /// </summary>
        internal string _outputPath;

        /// <summary>
        /// Objeto que encapsula la información de parámetros. Es sobreescrito cuando se cargan sucesivamente archivos de parámetros.
        /// </summary>
        internal ParametrosSimuLAN _parametrosBase;

        /// <summary>
        /// True si hay la ventana de simulación simple está abierta.
        /// </summary>
        internal bool _simulacionSimpleAbierta;

        /// <summary>
        /// True si la ventana de simulación multiescenario está abierta.
        /// </summary>
        internal bool _simulacionMultiAbierta;

        /// <summary>
        /// True si se interrumpe proceso de simulación
        /// </summary>
        internal bool _simulacion_cancelada;

        /// <summary>
        /// True si hay un proceso de simulación en ejecución.
        /// </summary>
        internal bool _simulando;

        /// <summary>
        /// Lista de estándares con los que se estimará la puntualidad en los diferentes reportes.
        /// </summary>
        internal List<int> _stds; 

        /// <summary>
        /// True si la ventana del validador está abierto.
        /// </summary>
        internal bool _validadorAbierto;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// True si está todo listo para iniciar el proceso de simulación
        /// </summary>
        public bool TodoListoParaSimular
        {
            get { return (_OK_Itinerario && _OK_Distribuciones && _OK_Parametros); }
        }

        /// <summary>
        /// Referencia a delagado para enviar mensaje a label de la interfaz principal de simulación
        /// </summary>
        public EnviarMensajeEventHandler SetMensajeSimulacion
        {
            get { return _enviarMensajeSimulacion; }
            set { _enviarMensajeSimulacion = value; }
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Crea una instancia e inicializa la interfaz
        /// </summary>
        public InterfazSimuLAN()
        {
            InitializeComponent();
            IniciarDelegates();
            IniciarEscenario();
        }

        #endregion

        #region METHODS

        #region INICIALIZACION

        /// <summary>
        /// Carga estado de los CheckBoxes de los reportes
        /// </summary>
        private void CargarChecksReportes()
        {
            int contador = 1;
            _escribeGrupo = new Dictionary<GruposReporte, bool>();
            _escribeReporte = new Dictionary<NombreReporte, bool>();
            foreach (NombreReporte reporte in Enum.GetValues(typeof(NombreReporte)))
            {
                _escribeReporte.Add(reporte, Convert.ToBoolean(_config.GetParametro("check_r" + contador)));
                contador++;
            }
            contador = 1;
            foreach (GruposReporte grupo in Enum.GetValues(typeof(GruposReporte)))
            {
                _escribeGrupo.Add(grupo, Convert.ToBoolean(_config.GetParametro("check_g" + contador)));
                contador++;
            }
        }

        /// <summary>
        /// Inicializa la instancia de Parametros
        /// </summary>
        /// <param name="ok">True si el proceso finaliza correctamente</param>
        private void CargarParametros(bool ok)
        {
            try
            {
                _parametrosBase = CargarParametrosDesdeAppConfig(ok);
            }
            catch
            {
                ok = false;
                mensajes.Invoke(_enviarMensajeInterfaz, "Error: no se pudieron cargar los parámetros");
            }
        }

        /// <summary>
        /// Carga información de parámetros desde appConfig
        /// </summary>
        /// <param name="ok">True si el proceso finaliza correctamente</param>
        /// <returns>Instancia de objeto de ParametrosSimuLAN</returns>
        private ParametrosSimuLAN CargarParametrosDesdeAppConfig(bool ok)
        {
            try
            {
                ParametrosEscalares escalares = new ParametrosEscalares(Convert.ToInt16(_config.GetParametro("replicas")), Convert.ToInt16(_config.GetParametro("semilla"))
                , Convert.ToInt16(_config.GetParametro("gap")), Convert.ToInt16(_config.GetParametro("toleranciaRecovery")), Convert.ToInt16(_config.GetParametro("minutosBackup"))
                , Convert.ToInt16(_config.GetParametro("minConex")), Convert.ToInt16(_config.GetParametro("maxConex")), Convert.ToInt16(_config.GetParametro("toleranciaTurnos")));
                string[] stds_string = _config.GetParametro("stds").ToString().Split(';');
                for (int i = 0; i < stds_string.Length; i++)
                {
                    _stds.Add(Convert.ToInt32(stds_string[i]));
                }
                ParametrosSimuLAN retorno = new ParametrosSimuLAN(escalares);
                ConexionPairing.TIEMPO_CAMBIO_AVION = escalares.MinPairing;
                ConexionPairing.TIEMPO_MAXIMO_PAIRING = escalares.MaxPairing;
                _abrirCarpetaOutput = Convert.ToBoolean(_config.GetParametro("check_output_dir"));
                _outputPath = _config.GetParametro("output_dir").ToString();
                return retorno;
            }
            catch (Exception)
            {
                ok = false;
                return null;
            }
        }       

        /// <summary>
        /// Carga delegados de control de carga de archivos y de mensajes.
        /// </summary>
        private void IniciarDelegates()
        {
            _enviarMensajeInterfaz = new EnviarMensajeEventHandler(EnviarMensajeLabelMensajes);
            openFileDialog_abrirItinerarioXLS.FileOk += new CancelEventHandler(openFileDialog_abrirItinerarioXLS_FileOk);
            openFileDialog_curvas.FileOk += new CancelEventHandler(openFileDialog_curvas_FileOk);
            saveFileDialog_curvas.FileOk += new CancelEventHandler(saveFileDialog_curvas_FileOk);
            saveFileDialog_parametros.FileOk += new CancelEventHandler(saveFileDialog_parametros_FileOk);
            openFileDialog_wxs.FileOk += new CancelEventHandler(openFileDialog_wxs_FileOk);
            saveFileDialog_wxs.FileOk += new CancelEventHandler(saveFileDialog_wxs_FileOk);

        }

        /// <summary>
        /// Inicializa escenario. Da valor a las variables booleanas de control de la interfaz e inicializa el itinerario, el modelo de disrupciones y los parámetros.
        /// </summary>
        private void IniciarEscenario()
        {
            _OK_Parametros = false;
            _OK_Distribuciones = false;
            _OK_Itinerario = false;
            _hayCambiosEnParametros = false;
            _simulando = false;
            _simulacionMultiAbierta = false;
            _simulacionSimpleAbierta = false;
            _validadorAbierto = false;
            _actualizadorAbierto = false;
            _config = new Configuracion();
            _stds = new List<int>();
            _itinerarioBase = new Itinerario("original");
            _factoresEscenarios = new Dictionary<TipoDisrupcion, Dictionary<string, Dictionary<TipoEscenarioDisrupcion, double>>>();
            _modeloDisrupcionesBase = new ModeloDisrupciones(null);
            CargarParametros(_OK_Parametros);
            CargarChecksReportes();
            _menuSimulacionNormal = new MenuSimulacionNormal(this);
            _menuSimulacionMultiescenario = new MenuSimulacionMultiescenario(this);
        }

        #endregion

        #region METODOS DE DELEGADOS

        /// <summary>
        /// Metodo para inicilizar listas desplegadas en tab de itinerario.
        /// </summary>
        private void CargarListas()
        {
            CargarComboBoxCurvas();
            CargarComboBoxTablasParametros();
            CargarAvionesEnListView();
            SetAspectGettersVisorPairings();
            SetAspectGettersVisorPaxs();
            SetGettersVisorMatrices();            
        }

        /// <summary>
        /// Método para desplegar mensaje en el label inferior izquierdo de la interfaz principal de SimuLAN.
        /// </summary>
        /// <param name="mensaje">Texto con el mensaje</param>
        private void EnviarMensajeLabelMensajes(string mensaje)
        {
            mensajes.Text = mensaje;
        }

        #endregion

        #region METODOS DE CONSOLIDACIÓN DE INFORMACION

        /// <summary>
        /// Post procesa información de slot de mantenimiento
        /// </summary>
        private void AgregarInfoMantos()
        {
            foreach (Avion a in _itinerarioBase.AvionesDictionary.Values)
            {
                foreach (SlotMantenimiento s in a.SlotsMantenimiento)
                {
                    if (s.TramoPrevio != null)
                    {
                        s.AgregarInfoSlot(s.TramoPrevio, s.TramoPrevio.Tramo_Siguiente);
                    }
                    else
                    {
                        s.AgregarInfoSlot(null, a.Tramo_Raiz);
                    }
                }
            }
        }

        /// <summary>
        /// Carga información de tablas de datos en itinerario
        /// </summary>
        internal void CargarTablasEnItinerario()
        {
            _itinerarioBase.CargarFlotasEnAcTypes(_parametrosBase.MapFlotas.Dict);
            _itinerarioBase.CargarFlotasEnTramos();
            _itinerarioBase.CargarMatriculasEnAviones(_parametrosBase.MapSubFlotasMatriculas);
            _itinerarioBase.CargarGruposAvion(_parametrosBase.MapGruposFlotas, _parametrosBase.InfoGruposFlotas);
            _itinerarioBase.CargarInfoRutasEnTramos(_parametrosBase.MapVuelosRutas);
            _itinerarioBase.CargarSlots();
        }

        /// <summary>
        /// Consolida información de itinerario, parámetros y curvas luego de que estos han sido cargados.
        /// </summary>
        private void PostProcesarInformacion()
        {
            SetStatusBotonesSimulacion(TodoListoParaSimular);
            if (TodoListoParaSimular)
            {
                _parametrosBase.LimpiarTurnAroundCustom();
                CargarTablasEnItinerario();
                AgregarInfoMantos();
                _itinerarioBase.CargarInfoHubsToAeropuertos(_parametrosBase.Conexiones.Hubs);
                _itinerarioBase.CrearConexiones(_parametrosBase);
                _itinerarioBase.CargarTurnosBackup(_parametrosBase.InfoGruposFlotas);
                _modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.METEREOLOGIA.ToString()].FactorDesviacionEscenario = _factoresEscenarios[TipoDisrupcion.METEREOLOGIA];
                _modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.MANTENIMIENTO.ToString()].FactorDesviacionEscenario = _factoresEscenarios[TipoDisrupcion.MANTENIMIENTO];
                this.Invoke(new CargarListasEventHandler(CargarListas));
                mensajes.Invoke(_enviarMensajeInterfaz, "Listo para simular");
                Button_Simular_Normal.Visible = true;
                Button_multi_sim.Visible = true;
            }
        }

        /// <summary>
        /// Asigna estado de visibilidad a los botones de simulación normal y multiescenario.
        /// </summary>
        /// <param name="status"></param>
        private void SetStatusBotonesSimulacion(bool status)
        {
            Button_multi_sim.Enabled = status;
            Button_Simular_Normal.Enabled = status;
        }

        #endregion

        #region METODOS ACCIONADOS CON PROCESO DE SIMULACION

        /// <summary>
        /// Cambia cursor sobre la interfaz
        /// </summary>
        /// <param name="cursor">Cursor seteado</param>
        internal void CambiarCursor(Cursor cursor)
        {
            this.Cursor = cursor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string ConstruirStringMensajeError()
        {
            string s = "No se puede iniciar la simulación.";

            if (!_OK_Itinerario)
            {
                s += "\n- No se ha cargado el itinerario";
            }
            if (!_OK_Distribuciones)
            {
                s += "\n- No se han cargado las curvas";
            }
            if (!_OK_Parametros)
            {
                s += "\n- Error al cargar app.config";
            }
            return s;
        }

        ///// <summary>
        ///// Inicia proceso de optmización de un escenario simple.
        ///// </summary>
        public void Optimizar()
        {
            ActualizarPorcentajeEventHandler actualizarPorcentaje;
            CambiarVistaSimularEventHandler cambiarVista;
            DateTime tiempoInicio = DateTime.Now;
            _simulacion_cancelada = false;
            _simulando = true;
            actualizarPorcentaje = this._menuSimulacionNormal.GetActualizarPorcentaje;
            cambiarVista = this._menuSimulacionNormal.GetCambiarVistaSimulacion;
            cambiarVista();
            foreach (Avion a in _itinerarioBase.AvionesDictionary.Values)
            {
                _itinerarioBase.CargarDelegadosAvionesTramos(a);
            }
            Optimizador optimizador = new Optimizador(_itinerarioBase, _parametrosBase, _modeloDisrupcionesBase, _stds,0, _menuSimulacionNormal.FechaInicioReportes, _menuSimulacionNormal.FechaTerminoReportes, _menuSimulacionNormal.VariacionPermitida);
            optimizador.OptimizarReaccionarios2(cambiarVista, _enviarMensajeSimulacion, actualizarPorcentaje, ref _simulacion_cancelada,_menuSimulacionNormal.Iteraciones_Optimizacion);
            if (!_simulacion_cancelada)
            {               
                cambiarVista();
            }
            //foreach (Simulacion s in informacionReplicas)
            //{
            //    s.Dispose();
            //}
            DateTime tiempo_fin = DateTime.Now;
            Invoke(_enviarMensajeSimulacion, "Optimización terminada en " + Math.Round((tiempo_fin - tiempoInicio).TotalSeconds, 1).ToString() + " segundos.");
            _simulando = false;
            optimizador = null;
            GC.Collect();
            if (_simulacion_cancelada)
            {
                _menuSimulacionNormal.Invoke(_menuSimulacionNormal.OcultarVentana);
                _simulacion_cancelada = false;
            }
        }

        /// <summary>
        ///  Inicia proceso de simulación multiescenario por clima y mantenimiento.
        /// </summary>
        internal void SimularMultiescenario()
        {                
            ActualizarPorcentajeEventHandler actualizarPorcentaje = this._menuSimulacionMultiescenario.GetActualizarPorcentaje;
            CambiarVistaSimularEventHandler  cambiarVista = this._menuSimulacionMultiescenario.GetCambiarVistaSimulacion;  
            DateTime tiempoInicio = DateTime.Now;
            cambiarVista();
            _simulacion_cancelada = false;
            _simulando = true;
            actualizarPorcentaje("0%");
            cambiarVista();
            //Carga diccionarios que almacenan output del proceso para la generación de reportes.
            Dictionary<string, Dictionary<int, Dictionary<int, double>>> outputSimulacionMultiescenario;
            Dictionary<string, Dictionary<string, Dictionary<int, Dictionary<int, double>>>> outputSimulacionMultiescenarioNegocios;            
            //Itera sobre escenario de metereología
            ManagerSimulacion manager = new ManagerSimulacion(_itinerarioBase, _parametrosBase, _modeloDisrupcionesBase, _stds, _menuSimulacionMultiescenario.FechaInicioReportes, _menuSimulacionMultiescenario.FechaTerminoReportes, this._enviarMensajeSimulacion, actualizarPorcentaje, ref _simulacion_cancelada);
            //Hace proceso de simulación de un escenario
            manager.SimularMultiescenario(out outputSimulacionMultiescenario,out outputSimulacionMultiescenarioNegocios);
             //Genera reportes.
            if (!_simulacion_cancelada)
            {
                _enviarMensajeSimulacion("Generando reporte...");
                CrearReportesMultiescenario(outputSimulacionMultiescenario, outputSimulacionMultiescenarioNegocios, _itinerarioBase.Negocios, _enviarMensajeSimulacion);
                _enviarMensajeSimulacion("Simulación multiescenario terminada en " + Math.Round((DateTime.Now - tiempoInicio).TotalSeconds, 1).ToString() + " segundos.");
            }
            manager = null;
            cambiarVista();
            _simulando = false;
            GC.Collect();
            if (_simulacion_cancelada)
            {
                _menuSimulacionMultiescenario.Invoke(_menuSimulacionMultiescenario.OcultarVentana);
                _simulacion_cancelada = false;
            }
        }
       
        ///// <summary>
        ///// Inicia proceso de simulación de un escenario simple.
        ///// </summary>
        public void SimularNormal()
        {
            ActualizarPorcentajeEventHandler actualizarPorcentaje;
            CambiarVistaSimularEventHandler cambiarVista;
            DateTime tiempoInicio = DateTime.Now;
            _simulacion_cancelada = false;
            _simulando = true; 
            actualizarPorcentaje = this._menuSimulacionNormal.GetActualizarPorcentaje;
            cambiarVista = this._menuSimulacionNormal.GetCambiarVistaSimulacion;
            cambiarVista();     
            ManagerSimulacion managerSimulacion = new ManagerSimulacion(_itinerarioBase, _parametrosBase, _modeloDisrupcionesBase, _stds, _menuSimulacionNormal.FechaInicioReportes, _menuSimulacionNormal.FechaTerminoReportes, this._enviarMensajeSimulacion, actualizarPorcentaje, ref _simulacion_cancelada);
            List<Simulacion> informacionReplicas = managerSimulacion.SimularNormal();
            if (!_simulacion_cancelada)
            {
                Invoke(_enviarMensajeSimulacion, "Generando reportes...");
                CrearReportesNormal(informacionReplicas, _enviarMensajeSimulacion);
            }
            foreach (Simulacion s in informacionReplicas)
            {
                s.Dispose();
            }
            informacionReplicas = null;
            DateTime tiempo_fin = DateTime.Now;
            Invoke(_enviarMensajeSimulacion, "Simulación terminada en " + Math.Round((tiempo_fin - tiempoInicio).TotalSeconds, 1).ToString() + " segundos.");
            cambiarVista();
            _simulando = false;
            managerSimulacion = null;
            GC.Collect();            
            if (_simulacion_cancelada)
            {
                _menuSimulacionNormal.Invoke(_menuSimulacionNormal.OcultarVentana);
                _simulacion_cancelada = false;
            }
        }

        #endregion

        #region METODOS ASOCIADOS A LOS VISORES DE ITINERARIO - CURVAS - PARAMETROS.

        #region TAB Visor Itinerario

        /// <summary>
        /// Actualiza itinerario
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActualizarItinerario(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0 && _hayCambiosEnParametros)
            {
                RefreshTables(null, new ItemsChangedEventArgs());
                _hayCambiosEnParametros = false;
            }
        }

        /// <summary>
        /// Carga aviones en listView de la izquierda
        /// </summary>
        private void CargarAvionesEnListView()
        {
            objectListView_aviones.ClearObjects();
            SetGettersVisorItinerario();
            objectListView_itin.Items.Clear();
            objectListView_aviones.AddObjects(_itinerarioBase.AvionesDictionary.Values);
        }

        /// <summary>
        /// Carga itinerario seleccionado en listView de la derecha
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void CargarItinerarioEnListView(object sender, EventArgs e)
        {
            try
            {
                objectListView_itin.Items.Clear();
                foreach (object o in objectListView_aviones.SelectedObjects)
                {
                    Avion a = (Avion)o;
                    objectListView_itin.AddObjects(a.ObtenerListaTramos(a.Tramo_Raiz));
                    objectListView_itin.AddObjects(a.SlotsMantenimiento.ToList());
                    objectListView_itin.AddObjects(a.GetBackups(a.IdAvion));
                }
                objectListView_itin.Sort(9);
                objectListView_itin.Refresh();
            }
            catch (Exception) { }
            objectListView_slots.Items.Clear();
            foreach (object o in objectListView_aviones.SelectedObjects)
            {
                Avion a = (Avion)o;
                objectListView_slots.AddObjects(a.ObtenerListaSlots());
            }
            objectListView_slots.Sort(7);
            objectListView_slots.Refresh();
        }

        /// <summary>
        /// Actualiza el visor en función de si selecciona legs o slots.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckedChangedRadioButtons(object sender, EventArgs e)
        {
            RadioButton selected = (RadioButton)sender;
            if (selected.Tag.ToString() == "Itinerario")
            {
                objectListView_itin.Visible = selected.Checked;
                objectListView_slots.Visible = !selected.Checked;
            }
            else
            {
                objectListView_itin.Visible = !selected.Checked;
                objectListView_slots.Visible = selected.Checked;
            }
        }

        /// <summary>
        /// Consulta si una selección de legs se puede borrar del itinerario
        /// </summary>
        /// <returns></returns>
        private bool EsEliminableListaItinerarioSeleccionada()
        {
            if (objectListView_itin.SelectedItems.Count > 0)
            {
                Tramo ini;
                Tramo fin;
                GetTramoIniFin(out ini, out fin, objectListView_itin.GetSelectedObjects());

                if (ini.TramoBase.Origen == fin.TramoBase.Destino)
                {
                    return true;
                }
                else if (fin.Tramo_Siguiente == null)
                {
                    return true;
                }
                else return false;
            }
            else return false;
        }

        /// <summary>
        /// Genera un slot de backup a partir de un slot generado artificialmente mediante la interfaz.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void generarSlotDeBackupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (EsEliminableListaItinerarioSeleccionada())
            {
                Tramo ini;
                Tramo fin;
                GetTramoIniFin(out ini, out fin, objectListView_itin.GetSelectedObjects());
                AgregarBU bu_form = new AgregarBU(ini, fin, this);
                CargarTablasEnItinerario();
                bu_form.Show();
            }
            objectListView_itin.Refresh();
        }

        /// <summary>
        /// Obtiene el tramo inicial y final desde una lista de tramos (en orden de tiempo de despegue)
        /// </summary>
        /// <param name="ini">Tramo inicial</param>
        /// <param name="fin">Tramo final</param>
        /// <param name="lista_tramos">Lista de tramos</param>
        private void GetTramoIniFin(out Tramo ini, out Tramo fin, ArrayList lista_tramos)
        {
            List<Tramo> tramos = new List<Tramo>();
            foreach (object o in lista_tramos)
            {
                tramos.Add((Tramo)o);
            }
            tramos.Sort(delegate(Tramo t1, Tramo t2)
            {
                if (t1.TInicialProg > t2.TInicialRst) return 1;
                else if (t1.TInicialProg < t2.TInicialRst) return -1;
                else return 0;
            });
            ini = tramos[0];
            fin = tramos[tramos.Count - 1];
        }

        /// <summary>
        /// Asigna formato a las filas del visor de legs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void objectListView_itin_FormatRow(object sender, FormatRowEventArgs e)
        {

            if (e.Item.RowObject is SlotMantenimiento)
            {
                e.Item.ForeColor = Color.Black;
                e.Item.BackColor = Color.Aqua;
            }
            else if (e.Item.RowObject is UnidadBackup)
            {
                e.Item.ForeColor = Color.Black;
                e.Item.BackColor = Color.LightGreen;
            }

            else if (e.Item.RowObject is Tramo)
            {
                try
                {
                    Tramo t = (Tramo)e.Item.RowObject;
                    if (t.Negocio == "SIN")
                    {
                        e.Item.GetSubItem(3).ForeColor = Color.Red;
                        e.Item.GetSubItem(3).BackColor = Color.Pink;
                    }
                }
                catch (Exception) { }
            }
        }

        /// <summary>
        /// Asigna formato a las filas del visor de slots
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void objectListView_slots_FormatRow(object sender, FormatRowEventArgs e)
        {
            Slot t = (Slot)e.Item.RowObject;
            if (t.TurnAroundMinimo == 0 || (t.Duracion < t.TurnAroundMinimo && t.Duracion > 0))
            {
                for (int i = 0; i < e.Item.SubItems.Count; i++)
                {
                    e.Item.GetSubItem(i).ForeColor = Color.Red;
                }
                e.Item.BackColor = Color.Pink;
            }
            int pos = Convert.ToInt32(Math.Min(510, t.Duracion / 1));
            int red = Math.Min(255, 510 - pos);
            int green = Math.Min(pos, 255);
            e.UseCellFormatEvents = true;
            e.Item.GetSubItem(1).ForeColor = Color.FromArgb(red, green, 0);

        }

        /// <summary>
        /// Selecciona una cadena a partir de un tramo seleccionado. La cadena coincide en un mismo aeropuerto de origen y destino
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SeleccionarCadena(object sender, EventArgs e)
        {
            if (objectListView_itin.SelectedItems.Count != 0)
            {
                Tramo seleccionado = (Tramo)objectListView_itin.GetSelectedObjects()[0];
                string origen = seleccionado.TramoBase.Origen;
                objectListView_itin.SelectedIndices.Clear();
                objectListView_itin.SelectedIndices.Add(objectListView_itin.IndexOf(seleccionado));
                Tramo aux = seleccionado.Tramo_Siguiente;
                while (aux != null && origen != aux.TramoBase.Destino)
                {
                    objectListView_itin.SelectedIndices.Add(objectListView_itin.IndexOf(aux));
                    aux = aux.Tramo_Siguiente;
                }
                if (aux != null && origen == aux.TramoBase.Destino)
                {
                    objectListView_itin.SelectedIndices.Add(objectListView_itin.IndexOf(aux));
                }
            }
            objectListView_itin.Refresh();
        }

        /// <summary>
        /// Permite seleccionar remotamente un Tab de la interfaz principal
        /// </summary>
        /// <param name="tabPageIndex"></param>
        internal void SelectTab(int tabPageIndex)
        {
            this.tabControl1.SelectedIndex = (tabPageIndex <= this.tabControl1.TabCount) ? tabPageIndex : 0;
        }

        /// <summary>
        /// Setea delegados sobre los campos de las tablas de aviones, legs y slots. 
        /// Los delegados permiten interactuar de manera sencilla entre los objetos de las listas y los campos que se despliegan.
        /// </summary>
        private void SetGettersVisorItinerario()
        {
            #region Tramos

            #region AspectGetters

            this.objectListView_itin.AllColumns[0].AspectGetter = delegate(object o)
            {
                if (o is Tramo)
                {
                    Tramo t = (Tramo)o;
                    return t.TramoBase.GetNumGlobalString(3);
                }
                else if (o is UnidadBackup)
                {
                    UnidadBackup t = (UnidadBackup)o;
                    return t.TramoBase.GetNumGlobalString(3);
                }
                else
                {
                    SlotMantenimiento t = (SlotMantenimiento)o;
                    return t.TramoBase.GetNumGlobalString(3);
                }
            };
            this.objectListView_itin.AllColumns[1].AspectGetter = delegate(object o)
            {
                if (o is Tramo)
                {
                    Tramo t = (Tramo)o;
                    return t.TramoBase.Numero_Vuelo.ToString();
                }
                else if (o is UnidadBackup)
                {
                    UnidadBackup t = (UnidadBackup)o;
                    return t.TramoBase.Numero_Vuelo.ToString();
                }
                else
                {
                    SlotMantenimiento t = (SlotMantenimiento)o;
                    return t.TramoBase.Numero_Vuelo.ToString();
                }
            };
            this.objectListView_itin.AllColumns[2].AspectGetter = delegate(object o)
            {
                if (o is Tramo)
                {
                    Tramo t = (Tramo)o;
                    return t.ParOD;
                }
                else if (o is UnidadBackup)
                {
                    UnidadBackup t = (UnidadBackup)o;
                    return t.TramoBase.Origen;
                }
                else
                {
                    SlotMantenimiento t = (SlotMantenimiento)o;
                    return t.TramoBase.Origen;
                }
            };
            this.objectListView_itin.AllColumns[3].AspectGetter = delegate(object o)
            {
                if (o is Tramo)
                {
                    Tramo t = (Tramo)o;
                    return t.Negocio;
                }
                else if (o is UnidadBackup)
                {
                    UnidadBackup t = (UnidadBackup)o;
                    return "-";
                }
                else
                {
                    SlotMantenimiento t = (SlotMantenimiento)o;
                    return "-";
                }
            };
            this.objectListView_itin.AllColumns[4].AspectGetter = delegate(object o)
            {
                if (o is Tramo)
                {
                    Tramo t = (Tramo)o;
                    return t.HBTProg;
                }
                else if (o is UnidadBackup)
                {
                    UnidadBackup t = (UnidadBackup)o;
                    return t.HBTProg;
                }
                else
                {
                    SlotMantenimiento t = (SlotMantenimiento)o;
                    return t.HBTProg;
                }
            };
            this.objectListView_itin.AllColumns[5].AspectGetter = delegate(object o)
            {
                if (o is Tramo)
                {
                    Tramo t = (Tramo)o;
                    return t.TramoBase.Fecha_Salida.ToShortDateString();
                }
                else if (o is UnidadBackup)
                {
                    UnidadBackup t = (UnidadBackup)o;
                    return t.TramoBase.Fecha_Salida.ToShortDateString();
                }
                else
                {
                    SlotMantenimiento t = (SlotMantenimiento)o;
                    return t.TramoBase.Fecha_Salida.ToShortDateString();
                }
            };
            this.objectListView_itin.AllColumns[6].AspectGetter = delegate(object o)
            {
                if (o is Tramo)
                {
                    Tramo t = (Tramo)o;
                    return Utilidades.GetHora(t.TramoBase.Hora_Salida);
                }
                else if (o is UnidadBackup)
                {
                    UnidadBackup t = (UnidadBackup)o;
                    return Utilidades.GetHora(t.TramoBase.Hora_Salida);
                }
                else
                {
                    SlotMantenimiento t = (SlotMantenimiento)o;
                    return Utilidades.GetHora(t.TramoBase.Hora_Salida);
                }
            };
            this.objectListView_itin.AllColumns[7].AspectGetter = delegate(object o)
            {
                if (o is Tramo)
                {
                    Tramo t = (Tramo)o;
                    return t.TramoBase.Fecha_Llegada.ToShortDateString();
                }
                else if (o is UnidadBackup)
                {
                    UnidadBackup t = (UnidadBackup)o;
                    return t.TramoBase.Fecha_Llegada.ToShortDateString();
                }
                else
                {
                    SlotMantenimiento t = (SlotMantenimiento)o;
                    return t.TramoBase.Fecha_Llegada.ToShortDateString();
                }
            };
            this.objectListView_itin.AllColumns[8].AspectGetter = delegate(object o)
            {
                if (o is Tramo)
                {
                    Tramo t = (Tramo)o;
                    return Utilidades.GetHora(t.TramoBase.Hora_Llegada);
                }
                else if (o is UnidadBackup)
                {
                    UnidadBackup t = (UnidadBackup)o;
                    return Utilidades.GetHora(t.TramoBase.Hora_Llegada);
                }
                else
                {
                    SlotMantenimiento t = (SlotMantenimiento)o;
                    return Utilidades.GetHora(t.TramoBase.Hora_Llegada);
                }
            };

            this.objectListView_itin.AllColumns[9].AspectGetter = delegate(object o)
            {
                if (o is Tramo)
                {
                    Tramo t = (Tramo)o;
                    return t.TInicialProg;
                }
                else if (o is UnidadBackup)
                {
                    UnidadBackup t = (UnidadBackup)o;
                    return t.TiempoIniPrg;
                }
                else
                {
                    SlotMantenimiento t = (SlotMantenimiento)o;
                    return t.TiempoInicioManttoPrg;
                }
            };

            this.objectListView_itin.AllColumns[10].AspectGetter = delegate(object o)
            {
                if (o is Tramo)
                {
                    Tramo t = (Tramo)o;
                    return t.KeyHUB;
                }
                else if (o is UnidadBackup)
                {
                    UnidadBackup t = (UnidadBackup)o;
                    return "-";
                }
                else
                {
                    SlotMantenimiento t = (SlotMantenimiento)o;
                    return "-";
                }
            };

            #endregion

            #region GroupKeyGetters

            for (int i = 0; i < this.objectListView_itin.AllColumns.Count; i++)
            {
                this.objectListView_itin.AllColumns[i].GroupKeyGetter = delegate(object o)
                {
                    if (o is Tramo)
                    {
                        Tramo t = (Tramo)o;
                        return (t.TramoBase.Numero_Ac.Length == 1) ? "0" + t.TramoBase.Numero_Ac : t.TramoBase.Numero_Ac;
                    }
                    else if (o is UnidadBackup)
                    {
                        UnidadBackup t = (UnidadBackup)o;
                        return (t.TramoBase.Numero_Ac.Length == 1) ? "0" + t.TramoBase.Numero_Ac : t.TramoBase.Numero_Ac;
                    }
                    else
                    {
                        SlotMantenimiento t = (SlotMantenimiento)o;
                        return (t.TramoBase.Numero_Ac.Length == 1) ? "0" + t.TramoBase.Numero_Ac : t.TramoBase.Numero_Ac;
                    }
                };
            }

            #endregion

            #region Formatters

            this.objectListView_itin.FormatRow += new EventHandler<FormatRowEventArgs>(objectListView_itin_FormatRow);
            #endregion

            #endregion

            #region Slots

            #region AspectGetters

            this.objectListView_slots.AllColumns[0].AspectGetter = delegate(object o)
            {
                Slot t = (Slot)o;
                return t.Estacion;
            };
            this.objectListView_slots.AllColumns[1].AspectGetter = delegate(object o)
            {
                Slot t = (Slot)o;
                return t.DuracionHHMM;
            };
            this.objectListView_slots.AllColumns[2].AspectGetter = delegate(object o)
            {
                Slot t = (Slot)o;
                return t.TurnAroundMinimo.ToString();
            };
            this.objectListView_slots.AllColumns[3].AspectGetter = delegate(object o)
            {
                Slot t = (Slot)o;
                return t.FechaIni.ToShortDateString();
            };
            this.objectListView_slots.AllColumns[4].AspectGetter = delegate(object o)
            {
                Slot t = (Slot)o;
                return Utilidades.GetHora(t.HoraIni);
            };
            this.objectListView_slots.AllColumns[5].AspectGetter = delegate(object o)
            {
                Slot t = (Slot)o;
                return t.FechaFin.ToShortDateString();
            };
            this.objectListView_slots.AllColumns[6].AspectGetter = delegate(object o)
            {
                Slot t = (Slot)o;
                return Utilidades.GetHora(t.HoraFin);
            };
            this.objectListView_slots.AllColumns[7].AspectGetter = delegate(object o)
            {
                Slot t = (Slot)o;
                return t.TiempoInicio;
            };

            #endregion

            #region GroupKeyGetters

            for (int i = 0; i < this.objectListView_slots.AllColumns.Count; i++)
            {
                this.objectListView_slots.AllColumns[i].GroupKeyGetter = delegate(object o)
                {
                    Slot t = (Slot)o;
                    return (t.AvionProgramado.Length == 1) ? "0" + t.AvionProgramado : t.AvionProgramado;
                };
            }

            #endregion

            #region Formatters

            this.objectListView_slots.FormatRow += new EventHandler<FormatRowEventArgs>(objectListView_slots_FormatRow);

            #endregion

            #region AspectPutters

            objectListView_slots.AllColumns[2].AspectPutter += delegate(object selected, object value)
            {
                Slot slot = (Slot)selected;
                if(Utilidades.EsEnteroPositivo(value.ToString()))
                {
                    int valor = Convert.ToInt32(value);
                    slot.TurnAroundMinimo = valor;
                    _parametrosBase.AgregarTurnAroundTramo(slot.TramoSiguiente.TramoBase.Numero_Global, valor);
                }
            };

            #endregion

            #endregion

            #region AVIONES

            this.objectListView_aviones.AllColumns[0].GroupKeyGetter = delegate(object o)
            {
                Avion a = (Avion)o;
                return a.SubFlota.ToString();
            };

            #region AspectGetters

            this.objectListView_aviones.AllColumns[0].AspectGetter = delegate(object o)
            {
                Avion a = (Avion)o;
                return (a.IdAvion.Length == 1) ? "0" + a.IdAvion : a.IdAvion;
            };

            this.objectListView_aviones.AllColumns[1].AspectGetter = delegate(object o)
            {
                Avion a = (Avion)o;
                string flota = a.GetFlota(a.AcType);
                return flota!=null ? flota: "";
            };

            this.objectListView_aviones.AllColumns[2].AspectGetter = delegate(object o)
            {
                Avion a = (Avion)o;
                return a.SubFlota.ToString();
            };

            #endregion

            #endregion
        }

        #endregion

        #region TAB Visor Disrupciones

        /// <summary>
        /// Agrega un nueva curva a la tabla seleccionada
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AgregarNuevaCurva(object sender, EventArgs e)
        {
            if (ValidarValoresCurva())
            {
                string selected = comboBox_curvas.SelectedItem.ToString();
                int dim = _modeloDisrupcionesBase.ColeccionDisrupciones[selected].Dimension;
                List<string> headers = _modeloDisrupcionesBase.ColeccionDisrupciones[selected].Headers;
                int cols = headers.Count;
                int valores = cols - dim;
                DataTable dt = _modeloDisrupcionesBase.ColeccionDisrupciones[selected].Data;
                object[] data = new object[cols];
                if (dim > 2)
                {
                    data[2] = maskedTextBox3.Text;
                }
                if (dim > 1)
                {
                    data[1] = maskedTextBox2.Text;
                }
                if (dim > 0)
                {
                    data[0] = maskedTextBox1.Text;
                }
                if (valores > 4)
                {
                    data[dim + 4] = Convert.ToDouble(textBox_val5.Text);
                }
                if (valores > 3)
                {
                    data[dim + 3] = Convert.ToDouble(textBox_val4.Text);
                }
                if (valores > 2)
                {
                    data[dim + 2] = Convert.ToDouble(textBox_val3.Text);
                }
                if (valores > 1)
                {
                    data[dim + 1] = Convert.ToDouble(textBox_val2.Text);
                }
                if (valores > 0)
                {
                    data[dim] = Convert.ToDouble(textBox_val1.Text);
                }
                dt.Rows.Add(data);
                LimpiarValoresCurva();
            }

        }

        /// <summary>
        /// Borra curva de disrupción seleccionada al presionar DEL o BACK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorrarDisrupcion(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                List<DataRowView> objectsForDelete = new List<DataRowView>();
                for (int i = dataListView_curvas.SelectedObjects.Count; i > 0; i--)
                {
                    objectsForDelete.Add((DataRowView)dataListView_curvas.SelectedObjects[i - 1]);
                }
                dataListView_curvas.DeselectAll();
                if (objectsForDelete.Count > 0)
                {
                    string selected = comboBox_curvas.SelectedItem.ToString();
                    foreach (DataRowView dt in objectsForDelete)
                    {
                        _modeloDisrupcionesBase.ColeccionDisrupciones[selected].Data.Rows.Remove(dt.Row);
                    }
                    dataListView_curvas.DataSource = _modeloDisrupcionesBase.ColeccionDisrupciones[selected].Data;
                    dataListView_curvas.Refresh();
                }
            }
        }

        /// <summary>
        /// Cambia filtro en ObjectListView cada vez que este es modificado por el usuario.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CambiarFiltro(object sender, EventArgs e)
        {
            int dim = (comboBox_f1.Visible ? 1 : 0) + (comboBox_f2.Visible ? 1 : 0) + (comboBox_f3.Visible ? 1 : 0);
            this.dataListView_curvas.ModelFilter = new ModelFilter(delegate(object x)
            {
                DataRowView data = (DataRowView)x;
                bool filtro1 = !comboBox_f1.Visible || comboBox_f1.SelectedItem == null;
                bool filtro2 = !comboBox_f2.Visible || comboBox_f2.SelectedItem == null;
                bool filtro3 = !comboBox_f3.Visible || comboBox_f3.SelectedItem == null;
                bool filtro4 = !checkBox1.Checked;
                if (!filtro1)
                {
                    filtro1 = (comboBox_f1.SelectedItem != null && data[0].ToString() == comboBox_f1.SelectedItem.ToString());
                }
                if (!filtro2)
                {
                    filtro2 = (comboBox_f2.SelectedItem != null && data[1].ToString() == comboBox_f2.SelectedItem.ToString());
                }
                if (!filtro3)
                {
                    filtro3 = (comboBox_f3.SelectedItem != null && data[2].ToString() == comboBox_f3.SelectedItem.ToString());
                }
                if (!filtro4)
                {
                    filtro4 = data[dim].ToString() != "0" || data[dim + 1].ToString() != "0";
                }
                return filtro1 && filtro2 && filtro3 & filtro4;
            });
            dataListView_curvas.Refresh();
            CargarComboBoxesFiltrosCurvas(dim);
        }

        /// <summary>
        /// Carga ComboBoxes para filtrar curvas
        /// </summary>
        /// <param name="dimension">Dimensión de filtrado de la curva seleccionada</param>
        private void CargarComboBoxesFiltrosCurvas(int dimension)
        {

            foreach (object o in dataListView_curvas.Objects)
            {
                DataRowView data = (DataRowView)o;
                if (dimension > 2)
                {
                    if (!comboBox_f3.Items.Contains(data[2]))
                        comboBox_f3.Items.Add(data[2]);
                }
                if (dimension > 1)
                {
                    if (!comboBox_f2.Items.Contains(data[1]))
                        comboBox_f2.Items.Add(data[1]);
                }
                if (!comboBox_f1.Items.Contains(data[0]))
                    comboBox_f1.Items.Add(data[0]);
            }
        }

        /// <summary>
        /// Carga curvas en ComboBox de curvas
        /// </summary>
        public void CargarComboBoxCurvas()
        {
            comboBox_curvas.Items.Clear();
            foreach (string s in _modeloDisrupcionesBase.ColeccionDisrupciones.Keys)
            {
                comboBox_curvas.Items.Add(s);
            }
        }

        /// <summary>
        /// Carga área para agregar una nueva curva según la que esté seleccionada.
        /// </summary>
        private void CargarMenuAgregarCurva()
        {
            string selected = comboBox_curvas.SelectedItem.ToString();
            int dim = _modeloDisrupcionesBase.ColeccionDisrupciones[selected].Dimension;
            List<string> headers = _modeloDisrupcionesBase.ColeccionDisrupciones[selected].Headers;
            int cols = headers.Count;
            int valores = cols - dim;
            label_grupo3.Visible = dim > 2;
            label_grupo2.Visible = dim > 1;
            label_grupo1.Visible = dim > 0;
            label_val5.Visible = valores > 4;
            label_val4.Visible = valores > 3;
            label_val3.Visible = valores > 2;
            label_val2.Visible = valores > 1;
            label_val1.Visible = valores > 0;
            maskedTextBox1.Visible = dim > 0;
            maskedTextBox2.Visible = dim > 1;
            maskedTextBox3.Visible = dim > 2;
            textBox_val1.Visible = valores > 0;
            textBox_val2.Visible = valores > 1;
            textBox_val3.Visible = valores > 2;
            textBox_val4.Visible = valores > 3;
            textBox_val5.Visible = valores > 4;
            label_val5.Text = headers[Math.Min(dim + 4, cols - 1)];
            label_val4.Text = headers[Math.Min(dim + 3, cols - 1)];
            label_val3.Text = headers[Math.Min(dim + 2, cols - 1)];
            label_val2.Text = headers[Math.Min(dim + 1, cols - 1)];
            label_val1.Text = headers[Math.Min(dim, cols - 1)];
            label_grupo1.Text = headers[0];
            label_grupo2.Text = headers[1];
            label_grupo3.Text = headers[2];
        }

        /// <summary>
        /// Configura la tabla de visor/editor de curvas en función de la seleccionada
        /// </summary>
        /// <param name="selected">Nombre de la curva seleccionada</param>
        private void ConfigurarDataObjectListCurvas(string selected)
        {
            int dimension = _modeloDisrupcionesBase.ColeccionDisrupciones[selected].Dimension;
            comboBox_f3.Visible = dimension > 2;
            comboBox_f2.Visible = dimension > 1;
            dataListView_curvas.ClearObjects();
            dataListView_curvas.DataSource = null;
            dataListView_curvas.DataMember = null;
            dataListView_curvas.Groups.Clear();

            List<string> headers = _modeloDisrupcionesBase.ColeccionDisrupciones[selected].Headers;

            for (int i = 0; i < dataListView_curvas.AllColumns.Count; i++)
            {
                if (i < headers.Count)
                {
                    dataListView_curvas.AllColumns[i].Text = headers[i];
                    dataListView_curvas.AllColumns[i].AspectName = headers[i];

                }
                else
                {
                    dataListView_curvas.AllColumns[i].Text = "(Vacía)";
                    dataListView_curvas.AllColumns[i].AspectName = "Column" + (i + 1);
                }
                dataListView_curvas.AllColumns[i].IsEditable = !(i < dimension);
                dataListView_curvas.AllColumns[i].IsVisible = (i < headers.Count);
            }
            dataListView_curvas.RebuildColumns();
            dataListView_curvas.Refresh();
        }

        /// <summary>
        /// Limpia área de formulario para agregar nueva curva
        /// </summary>
        private void LimpiarValoresCurva()
        {
            maskedTextBox1.Text = "";
            maskedTextBox2.Text = "";
            maskedTextBox3.Text = "";
            textBox_val1.Text = "0";
            textBox_val2.Text = "0";
            textBox_val3.Text = "0";
            textBox_val4.Text = "0";
            textBox_val5.Text = "0";
            maskedTextBox1.Mask = "";
            maskedTextBox2.Mask = "";
            maskedTextBox3.Mask = "";
        }
        
        /// <summary>
        /// Reinicia ComboBox de selección de curvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReiniciarComboBox(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                ComboBox cb = (ComboBox)sender;
                cb.SelectedItem = null;
            }
        }

        /// <summary>
        /// Carga curva seleccionada
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SeleccionarCurva(object sender, EventArgs e)
        {
            comboBox_f1.SelectedItem = null;
            comboBox_f2.SelectedItem = null;
            comboBox_f3.SelectedItem = null;
            string selected = comboBox_curvas.SelectedItem.ToString();
            ConfigurarDataObjectListCurvas(selected);
            this.dataListView_curvas.DataSource = _modeloDisrupcionesBase.ColeccionDisrupciones[selected].Data;
            comboBox_f1.Items.Clear();
            comboBox_f2.Items.Clear();
            comboBox_f3.Items.Clear();
            CargarMenuAgregarCurva();
            CargarComboBoxesFiltrosCurvas(_modeloDisrupcionesBase.ColeccionDisrupciones[selected].Dimension);
            groupBox3.Visible = true;
            groupBox5.Visible = true;
        }

        /// <summary>
        /// Valida que los valores cambiados en el editor de parámetros pertenezcan al dominio válido.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValidarEdicionCurvas(object sender, CellEditEventArgs e)
        {
            if (e.Column.Text == "Prob")
            {
                e.Cancel = !Utilidades.EsProbabilidad(e.NewValue.ToString());
            }
            if (e.Column.Text == "Media")
            {
                e.Cancel = !Utilidades.EsNumeroPositivo(e.NewValue.ToString(), true);
            }
            if (e.Column.Text == "Desvest")
            {
                e.Cancel = !Utilidades.EsNumeroPositivo(e.NewValue.ToString(), true);
            }
            if (e.Column.Text == "Min")
            {
                e.Cancel = !Utilidades.EsNumeroPositivo(e.NewValue.ToString(), true);
            }
            if (e.Column.Text == "Max")
            {
                e.Cancel = !Utilidades.EsNumeroPositivo(e.NewValue.ToString(), true);
            }
        }

        /// <summary>
        /// Valida que los valores agregados pertencezcan al dominio factible de parámetros
        /// </summary>
        /// <returns></returns>
        private bool ValidarValoresCurva()
        {
            bool valido1 = (!maskedTextBox1.Visible || maskedTextBox1.Text.Length > 0) && (!maskedTextBox2.Visible || maskedTextBox2.Text.Length > 0) && (!maskedTextBox3.Visible || maskedTextBox3.Text.Length > 0);
            bool valido2 = (!textBox_val1.Visible || Utilidades.EsNumeroPositivo(textBox_val1.Text, true)) && (!textBox_val2.Visible || Utilidades.EsNumeroPositivo(textBox_val2.Text, true)) && (!textBox_val3.Visible || Utilidades.EsNumeroPositivo(textBox_val3.Text, true)) && (!textBox_val4.Visible || Utilidades.EsNumeroPositivo(textBox_val4.Text, true)) && (!textBox_val5.Visible || Utilidades.EsNumeroPositivo(textBox_val5.Text, true));
            return valido1 && valido2;
        }

        #endregion

        #region TAB Editor Parámetros

        /// <summary>
        /// Carga SubTabs del Tab de parámetros
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CargarTabsParametros(object sender, EventArgs e)
        {
            CargarTabPairings();
            CargarTabPaxs();
            CargarTabRecovery();
        }

        #region TAB Tablas

        /// <summary>
        /// Agrega nueva fila a tabla seleccionada
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AgregarNuevaFila(object sender, EventArgs e)
        {
            string selected = ((KeyValuePair<string, string>)comboBox_tablas.SelectedItem).Key;
            int dim;
            DataTable origenData = _parametrosBase.BuscarTabla(selected, out dim);

            if (ValidarValoresFila(origenData, dim))
            {
                object[] data = new object[dim];
                if (dim > 2)
                {
                    data[2] = maskedTextBox_col3.Text;
                }
                if (dim > 1)
                {
                    data[1] = maskedTextBox_col2.Text;
                }
                if (dim > 0)
                {
                    data[0] = maskedTextBox_col1.Text;
                }
                origenData.Rows.Add(data);
                LimpiarValoresCurva();
                maskedTextBox_col1.Text = "";
                maskedTextBox_col2.Text = "";
                maskedTextBox_col3.Text = "";
                _hayCambiosEnParametros = true;
            }

        }

        /// <summary>
        /// Borra fila seleccionada al presionar DEL o BACK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorrarValorTabla(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                List<DataRowView> objectsForDelete = new List<DataRowView>();
                for (int i = dataListView_tablas.SelectedObjects.Count; i > 0; i--)
                {
                    objectsForDelete.Add((DataRowView)dataListView_tablas.SelectedObjects[i - 1]);
                }
                dataListView_tablas.DeselectAll();
                if (objectsForDelete.Count > 0)
                {
                    string selected = ((KeyValuePair<string, string>)comboBox_tablas.SelectedItem).Key;
                    int dim;
                    DataTable data = _parametrosBase.BuscarTabla(selected, out dim);
                    foreach (DataRowView dt in objectsForDelete)
                    {
                        data.Rows.Remove(dt.Row);
                    }
                    _hayCambiosEnParametros = true;
                    dataListView_tablas.DataSource = data;
                    dataListView_tablas.Refresh();
                }
            }
        }

        /// <summary>
        /// Carga ComboBox de selección de tablas 
        /// </summary>
        private void CargarComboBoxTablasParametros()
        {
            comboBox_tablas.Items.Clear();
            comboBox_tablas.Items.Add(new KeyValuePair<string, string>(_parametrosBase.MapFlotas.Nombre, "AcType-Flota"));
            comboBox_tablas.Items.Add(new KeyValuePair<string, string>(_parametrosBase.MapGruposFlotas.Nombre, "Flota-Grupo"));
            comboBox_tablas.Items.Add(new KeyValuePair<string, string>(_parametrosBase.MapSubFlotasMatriculas.Nombre, "Subflota-Matrícula"));
            comboBox_tablas.Items.Add(new KeyValuePair<string, string>(_parametrosBase.MapVuelosRutas.Nombre, "Vuelo-Negocio"));
            comboBox_tablas.Items.Add(new KeyValuePair<string, string>(_parametrosBase.TurnAroundMin.Nombre, "T/A Mínimo"));
            comboBox_tablas.DisplayMember = "Value";
        }

        /// <summary>
        /// Carga zona para agregar una nueva fila en función de la tabla seleccionada
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dim"></param>
        private void CargarMenuAgregarFila(DataTable data, int dim)
        {
            label_col3.Visible = dim > 2;
            label_col2.Visible = dim > 1;
            label_col1.Visible = dim > 0;
            maskedTextBox_col1.Visible = dim > 0;
            maskedTextBox_col2.Visible = dim > 1;
            maskedTextBox_col3.Visible = dim > 2;
            label_col3.Text = dim > 2 ? data.Columns[2].ColumnName : "";
            label_col2.Text = dim > 1 ? data.Columns[1].ColumnName : "";
            label_col1.Text = dim > 0 ? data.Columns[0].ColumnName : "";
        }

        /// <summary>
        /// Configura el ObjectListView Visor/Editor de tabla en función de la tabla seleccionada
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dim"></param>
        private void ConfigurarDataObjectListTablas(DataTable data, int dim)
        {
            dataListView_tablas.ClearObjects();
            dataListView_tablas.DataSource = null;
            dataListView_tablas.DataMember = null;
            dataListView_tablas.Groups.Clear();

            for (int i = 0; i < dataListView_tablas.AllColumns.Count; i++)
            {
                if (i < dim)
                {
                    dataListView_tablas.AllColumns[i].Text = data.Columns[i].ColumnName;
                    dataListView_tablas.AllColumns[i].AspectName = data.Columns[i].ColumnName;
                }
                else
                {
                    dataListView_tablas.AllColumns[i].Text = "(Vacía)";
                    dataListView_tablas.AllColumns[i].AspectName = data.Columns[i].ColumnName;
                }
                dataListView_tablas.AllColumns[i].IsEditable = (i < dim);
                dataListView_tablas.AllColumns[i].IsVisible = (i < dim);
            }
            dataListView_tablas.RebuildColumns();
            dataListView_tablas.Refresh();
        }
        
        /// <summary>
        /// Refresca tabla seleccionado y el visor de itinerario
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void RefreshTables(object sender, ItemsChangedEventArgs e)
        {
            _parametrosBase.Refresh();
            CargarTablasEnItinerario();
            CargarItinerarioEnListView(objectListView_aviones, new EventArgs());
        }

        /// <summary>
        /// Carga tabla seleccionada
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SeleccionarTabla(object sender, EventArgs e)
        {
            string selected = ((KeyValuePair<string, string>)comboBox_tablas.SelectedItem).Key;
            int dim;
            DataTable data = _parametrosBase.BuscarTabla(selected, out dim);
            CargarMenuAgregarFila(data, dim);
            ConfigurarDataObjectListTablas(data, dim);
            this.dataListView_tablas.DataSource = data;
            groupBox6.Visible = true;
            groupBox8.Visible = true;
        }

        /// <summary>
        /// Valida que los valores cambiados en la tabla seleccionada pertenezcan a su dominio.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValidarCambiosTablas(object sender, CellEditEventArgs e)
        {
            if (e.Column.Text == "Minutos")
            {
                e.Cancel = !Utilidades.EsNumeroPositivo(e.NewValue.ToString(), false);
            }
            if (e.Column.Text == "Hub Salida")
            {
                e.Cancel = !Utilidades.EsUnoCero(e.NewValue.ToString());
            }
            if (e.Column.Text == "Grupo")
            {
                e.Cancel = !_parametrosBase.InfoGruposFlotas.ContainsKey(e.NewValue.ToString());
            }
            _hayCambiosEnParametros = _hayCambiosEnParametros || !e.Cancel && e.NewValue != e.Value;
        }

        /// <summary>
        /// Valida que los valores agregados estén en el dominio de la tabla seleccionada
        /// </summary>
        /// <param name="origenData">DataTable de la tabla seleccionada</param>
        /// <param name="dim">Dinensión de la tabla</param>
        /// <returns></returns>
        private bool ValidarValoresFila(DataTable origenData, int dim)
        {
            bool valido1 = !maskedTextBox_col1.Visible || maskedTextBox_col1.Text.Length > 0;
            bool valido2 = !maskedTextBox_col2.Visible || maskedTextBox_col2.Text.Length > 0;
            bool valido3 = !maskedTextBox_col3.Visible || maskedTextBox_col3.Text.Length > 0;
            if (dim == 3)
            {
                if (origenData.Columns[2].ColumnName == "Minutos")
                {
                    if (valido3)
                    {
                        valido3 = Utilidades.EsEnteroPositivo(maskedTextBox_col3.Text);
                    }
                }
                else if (origenData.Columns[2].ColumnName == "HUB Salida")
                {
                    if (valido3)
                    {
                        valido3 = Utilidades.EsUnoCero(maskedTextBox_col3.Text);
                    }
                }
                else if (origenData.Columns[2].ColumnName == "Grupo")
                {
                    if (valido3)
                    {
                        valido3 = _parametrosBase.InfoGruposFlotas.ContainsKey(maskedTextBox_col3.Text);
                    }
                }
            }
            return valido1 && valido2 && valido3;
        }

        #endregion

        #region TAB Recovery

        /// <summary>
        /// Carga tablas de matrices de recovery
        /// </summary>
        private void CargarTabRecovery()
        {
            objectListView_multioperador.ClearObjects();
            objectListView_flota_flota.ClearObjects();
            SetVisibilidadColumnasMatrices();
            if (_parametrosBase != null && _parametrosBase.MatrizMultioperador != null && _parametrosBase.MatrizMultioperador.Count > 0)
            {
                foreach (string key in _parametrosBase.MatrizMultioperador.Keys)
                {
                    objectListView_multioperador.AddObject(new KeyValuePair<string,SerializableDictionary<string,int>>(key,_parametrosBase.MatrizMultioperador[key]));
                }
            }

            if (_parametrosBase != null && _parametrosBase.MatrizFlotaFlota != null && _parametrosBase.MatrizFlotaFlota.Count > 0)
            {
                foreach (string key in _parametrosBase.MatrizFlotaFlota.Keys)
                {
                    objectListView_flota_flota.AddObject(new KeyValuePair<string,SerializableDictionary<string,double>>(key,_parametrosBase.MatrizFlotaFlota[key]));
                }
            }
        }

        /// <summary>
        /// Borra fila seleccionada al presionar DEL o BACK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorrarFilaMatrizFlotaFlota(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                object o = objectListView_flota_flota.GetSelectedObject();
                if (o != null)
                {
                    KeyValuePair<string, SerializableDictionary<string, double>> selected = (KeyValuePair<string, SerializableDictionary<string, double>>)o;
                    string flotaRemovida = selected.Key;
                    foreach (string flota in _parametrosBase.MatrizFlotaFlota.Keys)
                    {
                        _parametrosBase.MatrizFlotaFlota[flota].Remove(flotaRemovida);
                    }
                    _parametrosBase.MatrizFlotaFlota.Remove(flotaRemovida);
                    CargarTabRecovery();
                }
            }
        }

        /// <summary>
        /// Borra fila seleccionada al presionar DEL o BACK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorrarFilaMatrizMultioperador(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                object o = objectListView_multioperador.GetSelectedObject();
                if (o != null)
                {
                    KeyValuePair<string, SerializableDictionary<string, int>> selected = (KeyValuePair<string, SerializableDictionary<string, int>>)o;
                    _parametrosBase.MatrizMultioperador.Remove(selected.Key);
                    CargarTabRecovery();
                }
            }
        }

        /// <summary>
        /// Setea delegados sobre los campos de las tablas de matrices. 
        /// Los delegados permiten interactuar de manera sencilla entre los objetos de las listas y los campos que se despliegan.
        /// </summary>
        private void SetGettersVisorMatrices()
        {
            #region MATRIZ MULTIOPERADOR

            #region aspectGetters

            objectListView_multioperador.AllColumns[0].AspectGetter = delegate(object o)
            {
                KeyValuePair<string, SerializableDictionary<string, int>> value = (KeyValuePair<string, SerializableDictionary<string, int>>)o;
                if (objectListView_multioperador.AllColumns[0].IsVisible)
                {

                    return value.Key;
                }
                else
                {
                    return "";
                }
            };
            objectListView_multioperador.AllColumns[1].AspectGetter = delegate(object o)
            {
                KeyValuePair<string, SerializableDictionary<string, int>> value = (KeyValuePair<string, SerializableDictionary<string, int>>)o;
                if (objectListView_multioperador.AllColumns[1].IsVisible)
                {

                    return value.Value[_parametrosBase.ColumnaMultioperador(0)];
                }
                else
                {
                    return "";
                }
            };
            objectListView_multioperador.AllColumns[2].AspectGetter = delegate(object o)
            {
                KeyValuePair<string, SerializableDictionary<string, int>> value = (KeyValuePair<string, SerializableDictionary<string, int>>)o;
                if (objectListView_multioperador.AllColumns[2].IsVisible)
                {

                    return value.Value[_parametrosBase.ColumnaMultioperador(1)];
                }
                else
                {
                    return "";
                }
            };
            objectListView_multioperador.AllColumns[3].AspectGetter = delegate(object o)
            {
                KeyValuePair<string, SerializableDictionary<string, int>> value = (KeyValuePair<string, SerializableDictionary<string, int>>)o;
                if (objectListView_multioperador.AllColumns[3].IsVisible)
                {

                    return value.Value[_parametrosBase.ColumnaMultioperador(2)];
                }
                else
                {
                    return "";
                }
            };
            objectListView_multioperador.AllColumns[4].AspectGetter = delegate(object o)
            {
                KeyValuePair<string, SerializableDictionary<string, int>> value = (KeyValuePair<string, SerializableDictionary<string, int>>)o;
                if (objectListView_multioperador.AllColumns[4].IsVisible)
                {

                    return value.Value[_parametrosBase.ColumnaMultioperador(3)];
                }
                else
                {
                    return "";
                }
            };
            objectListView_multioperador.AllColumns[5].AspectGetter = delegate(object o)
            {
                KeyValuePair<string, SerializableDictionary<string, int>> value = (KeyValuePair<string, SerializableDictionary<string, int>>)o;
                if (objectListView_multioperador.AllColumns[5].IsVisible)
                {

                    return value.Value[_parametrosBase.ColumnaMultioperador(4)];
                }
                else
                {
                    return "";
                }
            };
            objectListView_multioperador.AllColumns[6].AspectGetter = delegate(object o)
            {
                KeyValuePair<string, SerializableDictionary<string, int>> value = (KeyValuePair<string, SerializableDictionary<string, int>>)o;
                if (objectListView_multioperador.AllColumns[6].IsVisible)
                {

                    return value.Value[_parametrosBase.ColumnaMultioperador(5)];
                }
                else
                {
                    return "";
                }
            };
            objectListView_multioperador.AllColumns[7].AspectGetter = delegate(object o)
            {
                KeyValuePair<string, SerializableDictionary<string, int>> value = (KeyValuePair<string, SerializableDictionary<string, int>>)o;
                if (objectListView_multioperador.AllColumns[7].IsVisible)
                {

                    return value.Value[_parametrosBase.ColumnaMultioperador(6)];
                }
                else
                {
                    return "";
                }
            };
            objectListView_multioperador.AllColumns[8].AspectGetter = delegate(object o)
            {
                KeyValuePair<string, SerializableDictionary<string, int>> value = (KeyValuePair<string, SerializableDictionary<string, int>>)o;
                if (objectListView_multioperador.AllColumns[8].IsVisible)
                {

                    return value.Value[_parametrosBase.ColumnaMultioperador(7)];
                }
                else
                {
                    return "";
                }
            };
            objectListView_multioperador.AllColumns[9].AspectGetter = delegate(object o)
            {
                KeyValuePair<string, SerializableDictionary<string, int>> value = (KeyValuePair<string, SerializableDictionary<string, int>>)o;
                if (objectListView_multioperador.AllColumns[9].IsVisible)
                {
                    return value.Value[_parametrosBase.ColumnaMultioperador(8)];
                }
                else
                {
                    return "";
                }
            };

            #endregion

            #region ascpectPutters

            objectListView_multioperador.AllColumns[1].AspectPutter = delegate(object goal, object value)
            {
                if (objectListView_multioperador.AllColumns[1].IsVisible)
                {
                    if (Utilidades.EsUnoCero(value.ToString()))
                    {
                        KeyValuePair<string, SerializableDictionary<string, int>> obj = (KeyValuePair<string, SerializableDictionary<string, int>>)goal;
                        obj.Value[_parametrosBase.ColumnaMultioperador(0)] = Convert.ToInt16(value);
                    }
                }
            };

            objectListView_multioperador.AllColumns[2].AspectPutter = delegate(object goal, object value)
            {
                if (objectListView_multioperador.AllColumns[2].IsVisible)
                {
                    if (Utilidades.EsUnoCero(value.ToString()))
                    {
                        KeyValuePair<string, SerializableDictionary<string, int>> obj = (KeyValuePair<string, SerializableDictionary<string, int>>)goal;
                        obj.Value[_parametrosBase.ColumnaMultioperador(1)] = Convert.ToInt16(value);
                    }
                }
            };

            objectListView_multioperador.AllColumns[3].AspectPutter = delegate(object goal, object value)
            {
                if (objectListView_multioperador.AllColumns[3].IsVisible)
                {
                    if (Utilidades.EsUnoCero(value.ToString()))
                    {
                        KeyValuePair<string, SerializableDictionary<string, int>> obj = (KeyValuePair<string, SerializableDictionary<string, int>>)goal;
                        obj.Value[_parametrosBase.ColumnaMultioperador(2)] = Convert.ToInt16(value);
                    }
                }
            };

            objectListView_multioperador.AllColumns[4].AspectPutter = delegate(object goal, object value)
            {
                if (objectListView_multioperador.AllColumns[4].IsVisible)
                {
                    if (Utilidades.EsUnoCero(value.ToString()))
                    {
                        KeyValuePair<string, SerializableDictionary<string, int>> obj = (KeyValuePair<string, SerializableDictionary<string, int>>)goal;
                        obj.Value[_parametrosBase.ColumnaMultioperador(3)] = Convert.ToInt16(value);
                    }
                }
            };

            objectListView_multioperador.AllColumns[5].AspectPutter = delegate(object goal, object value)
            {
                if (objectListView_multioperador.AllColumns[5].IsVisible)
                {
                    if (Utilidades.EsUnoCero(value.ToString()))
                    {
                        KeyValuePair<string, SerializableDictionary<string, int>> obj = (KeyValuePair<string, SerializableDictionary<string, int>>)goal;
                        obj.Value[_parametrosBase.ColumnaMultioperador(4)] = Convert.ToInt16(value);
                    }
                }
            };

            objectListView_multioperador.AllColumns[6].AspectPutter = delegate(object goal, object value)
            {
                if (objectListView_multioperador.AllColumns[6].IsVisible)
                {
                    if (Utilidades.EsUnoCero(value.ToString()))
                    {
                        KeyValuePair<string, SerializableDictionary<string, int>> obj = (KeyValuePair<string, SerializableDictionary<string, int>>)goal;
                        obj.Value[_parametrosBase.ColumnaMultioperador(5)] = Convert.ToInt16(value);
                    }
                }
            };

            objectListView_multioperador.AllColumns[7].AspectPutter = delegate(object goal, object value)
            {
                if (objectListView_multioperador.AllColumns[7].IsVisible)
                {
                    if (Utilidades.EsUnoCero(value.ToString()))
                    {
                        KeyValuePair<string, SerializableDictionary<string, int>> obj = (KeyValuePair<string, SerializableDictionary<string, int>>)goal;
                        obj.Value[_parametrosBase.ColumnaMultioperador(6)] = Convert.ToInt16(value);
                    }
                }
            };

            objectListView_multioperador.AllColumns[8].AspectPutter = delegate(object goal, object value)
            {
                if (objectListView_multioperador.AllColumns[8].IsVisible)
                {
                    if (Utilidades.EsUnoCero(value.ToString()))
                    {
                        KeyValuePair<string, SerializableDictionary<string, int>> obj = (KeyValuePair<string, SerializableDictionary<string, int>>)goal;
                        obj.Value[_parametrosBase.ColumnaMultioperador(7)] = Convert.ToInt16(value);
                    }
                }
            };

            objectListView_multioperador.AllColumns[9].AspectPutter = delegate(object goal, object value)
            {
                if (objectListView_multioperador.AllColumns[9].IsVisible)
                {
                    if (Utilidades.EsUnoCero(value.ToString()))
                    {
                        KeyValuePair<string, SerializableDictionary<string, int>> obj = (KeyValuePair<string, SerializableDictionary<string, int>>)goal;
                        obj.Value[_parametrosBase.ColumnaMultioperador(8)] = Convert.ToInt16(value);
                    }
                }
            };

            #endregion

            #endregion

            #region MATRIZ FLOTA-FLOTA

            #region aspectGetters

            objectListView_flota_flota.AllColumns[0].AspectGetter = delegate(object o)
            {
                KeyValuePair<string, SerializableDictionary<string, double>> value = (KeyValuePair<string, SerializableDictionary<string, double>>)o;
                if (objectListView_flota_flota.AllColumns[0].IsVisible)
                {

                    return value.Key;
                }
                else
                {
                    return "";
                }
            };
            objectListView_flota_flota.AllColumns[1].AspectGetter = delegate(object o)
            {
                KeyValuePair<string, SerializableDictionary<string, double>> value = (KeyValuePair<string, SerializableDictionary<string, double>>)o;
                if (objectListView_flota_flota.AllColumns[1].IsVisible)
                {

                    string indice = _parametrosBase.IndiceFlotaFlota(0);
                    return value.Value.ContainsKey(indice) ? value.Value[indice].ToString() : "";
                }
                else
                {
                    return "";
                }
            };
            objectListView_flota_flota.AllColumns[2].AspectGetter = delegate(object o)
            {
                KeyValuePair<string, SerializableDictionary<string, double>> value = (KeyValuePair<string, SerializableDictionary<string, double>>)o;
                if (objectListView_flota_flota.AllColumns[2].IsVisible)
                {
                    string indice = _parametrosBase.IndiceFlotaFlota(1);
                    return value.Value.ContainsKey(indice) ? value.Value[indice].ToString() : "";
                }
                else
                {
                    return "";
                }
            };
            objectListView_flota_flota.AllColumns[3].AspectGetter = delegate(object o)
            {
                KeyValuePair<string, SerializableDictionary<string, double>> value = (KeyValuePair<string, SerializableDictionary<string, double>>)o;
                if (objectListView_flota_flota.AllColumns[3].IsVisible)
                {
                    string indice = _parametrosBase.IndiceFlotaFlota(2);
                    return value.Value.ContainsKey(indice) ? value.Value[indice].ToString() : "";
                }
                else
                {
                    return "";
                }
            };
            objectListView_flota_flota.AllColumns[4].AspectGetter = delegate(object o)
            {
                KeyValuePair<string, SerializableDictionary<string, double>> value = (KeyValuePair<string, SerializableDictionary<string, double>>)o;
                if (objectListView_flota_flota.AllColumns[4].IsVisible)
                {
                    string indice = _parametrosBase.IndiceFlotaFlota(3);
                    return value.Value.ContainsKey(indice) ? value.Value[indice].ToString() : "";
                }
                else
                {
                    return "";
                }
            };
            objectListView_flota_flota.AllColumns[5].AspectGetter = delegate(object o)
            {
                KeyValuePair<string, SerializableDictionary<string, double>> value = (KeyValuePair<string, SerializableDictionary<string, double>>)o;
                if (objectListView_flota_flota.AllColumns[5].IsVisible)
                {
                    string indice = _parametrosBase.IndiceFlotaFlota(4);
                    return value.Value.ContainsKey(indice) ? value.Value[indice].ToString() : "";
                }
                else
                {
                    return "";
                }
            };
            objectListView_flota_flota.AllColumns[6].AspectGetter = delegate(object o)
            {
                KeyValuePair<string, SerializableDictionary<string, double>> value = (KeyValuePair<string, SerializableDictionary<string, double>>)o;
                if (objectListView_flota_flota.AllColumns[6].IsVisible)
                {
                    string indice = _parametrosBase.IndiceFlotaFlota(5);
                    return value.Value.ContainsKey(indice) ? value.Value[indice].ToString() : "";
                }
                else
                {
                    return "";
                }
            };
            objectListView_flota_flota.AllColumns[7].AspectGetter = delegate(object o)
            {
                KeyValuePair<string, SerializableDictionary<string, double>> value = (KeyValuePair<string, SerializableDictionary<string, double>>)o;
                if (objectListView_flota_flota.AllColumns[7].IsVisible)
                {
                    string indice = _parametrosBase.IndiceFlotaFlota(6);
                    return value.Value.ContainsKey(indice) ? value.Value[indice].ToString() : "";
                }
                else
                {
                    return "";
                }
            };
            objectListView_flota_flota.AllColumns[8].AspectGetter = delegate(object o)
            {
                KeyValuePair<string, SerializableDictionary<string, double>> value = (KeyValuePair<string, SerializableDictionary<string, double>>)o;
                if (objectListView_flota_flota.AllColumns[8].IsVisible)
                {
                    string indice = _parametrosBase.IndiceFlotaFlota(7);
                    return value.Value.ContainsKey(indice) ? value.Value[indice].ToString() : "";
                }
                else
                {
                    return "";
                }
            };
            objectListView_flota_flota.AllColumns[9].AspectGetter = delegate(object o)
            {
                KeyValuePair<string, SerializableDictionary<string, double>> value = (KeyValuePair<string, SerializableDictionary<string, double>>)o;
                if (objectListView_flota_flota.AllColumns[9].IsVisible)
                {
                    string indice = _parametrosBase.IndiceFlotaFlota(8);
                    return value.Value.ContainsKey(indice) ? value.Value[indice].ToString() : "";
                }
                else
                {
                    return "";
                }
            };

            #endregion

            #region ascpectPutters

            objectListView_flota_flota.AllColumns[1].AspectPutter = delegate(object goal, object value)
            {
                if (objectListView_flota_flota.AllColumns[1].IsVisible)
                {
                    if (Utilidades.EsProbabilidad(value.ToString()))
                    {
                        KeyValuePair<string, SerializableDictionary<string, double>> obj = (KeyValuePair<string, SerializableDictionary<string, double>>)goal;
                        obj.Value[_parametrosBase.IndiceFlotaFlota(0)] = Utilidades.GetDouble(value.ToString());
                    }
                }
            };

            objectListView_flota_flota.AllColumns[2].AspectPutter = delegate(object goal, object value)
            {
                if (objectListView_flota_flota.AllColumns[2].IsVisible)
                {
                    if (Utilidades.EsProbabilidad(value.ToString()))
                    {
                        KeyValuePair<string, SerializableDictionary<string, double>> obj = (KeyValuePair<string, SerializableDictionary<string, double>>)goal;
                        obj.Value[_parametrosBase.IndiceFlotaFlota(1)] = Utilidades.GetDouble(value.ToString());
                    }
                }
            };

            objectListView_flota_flota.AllColumns[3].AspectPutter = delegate(object goal, object value)
            {
                if (objectListView_flota_flota.AllColumns[3].IsVisible)
                {
                    if (Utilidades.EsProbabilidad(value.ToString()))
                    {
                        KeyValuePair<string, SerializableDictionary<string, double>> obj = (KeyValuePair<string, SerializableDictionary<string, double>>)goal;
                        obj.Value[_parametrosBase.IndiceFlotaFlota(2)] = Utilidades.GetDouble(value.ToString());
                    }
                }
            };

            objectListView_flota_flota.AllColumns[4].AspectPutter = delegate(object goal, object value)
            {
                if (objectListView_flota_flota.AllColumns[4].IsVisible)
                {
                    if (Utilidades.EsProbabilidad(value.ToString()))
                    {
                        KeyValuePair<string, SerializableDictionary<string, double>> obj = (KeyValuePair<string, SerializableDictionary<string, double>>)goal;
                        obj.Value[_parametrosBase.IndiceFlotaFlota(3)] = Utilidades.GetDouble(value.ToString());
                    }
                }
            };

            objectListView_flota_flota.AllColumns[5].AspectPutter = delegate(object goal, object value)
            {
                if (objectListView_flota_flota.AllColumns[5].IsVisible)
                {
                    if (Utilidades.EsProbabilidad(value.ToString()))
                    {
                        KeyValuePair<string, SerializableDictionary<string, double>> obj = (KeyValuePair<string, SerializableDictionary<string, double>>)goal;
                        obj.Value[_parametrosBase.IndiceFlotaFlota(4)] = Utilidades.GetDouble(value.ToString());
                    }
                }
            };

            objectListView_flota_flota.AllColumns[6].AspectPutter = delegate(object goal, object value)
            {
                if (objectListView_flota_flota.AllColumns[6].IsVisible)
                {
                    if (Utilidades.EsProbabilidad(value.ToString()))
                    {
                        KeyValuePair<string, SerializableDictionary<string, double>> obj = (KeyValuePair<string, SerializableDictionary<string, double>>)goal;
                        obj.Value[_parametrosBase.IndiceFlotaFlota(5)] = Utilidades.GetDouble(value.ToString());
                    }
                }
            };

            objectListView_flota_flota.AllColumns[7].AspectPutter = delegate(object goal, object value)
            {
                if (objectListView_flota_flota.AllColumns[7].IsVisible)
                {
                    if (Utilidades.EsProbabilidad(value.ToString()))
                    {
                        KeyValuePair<string, SerializableDictionary<string, double>> obj = (KeyValuePair<string, SerializableDictionary<string, double>>)goal;
                        obj.Value[_parametrosBase.IndiceFlotaFlota(6)] = Utilidades.GetDouble(value.ToString());
                    }
                }
            };

            objectListView_flota_flota.AllColumns[8].AspectPutter = delegate(object goal, object value)
            {
                if (objectListView_flota_flota.AllColumns[8].IsVisible)
                {
                    if (Utilidades.EsProbabilidad(value.ToString()))
                    {
                        KeyValuePair<string, SerializableDictionary<string, double>> obj = (KeyValuePair<string, SerializableDictionary<string, double>>)goal;
                        obj.Value[_parametrosBase.IndiceFlotaFlota(7)] = Utilidades.GetDouble(value.ToString());
                    }
                }
            };

            objectListView_flota_flota.AllColumns[9].AspectPutter = delegate(object goal, object value)
            {
                if (objectListView_flota_flota.AllColumns[9].IsVisible)
                {
                    if (Utilidades.EsProbabilidad(value.ToString()))
                    {
                        KeyValuePair<string, SerializableDictionary<string, double>> obj = (KeyValuePair<string, SerializableDictionary<string, double>>)goal;
                        obj.Value[_parametrosBase.IndiceFlotaFlota(8)] = Utilidades.GetDouble(value.ToString());
                    }
                }
            };

            #endregion

            #endregion
        }

        /// <summary>
        /// Setea la visibilidad de las filas y columnas de las matrices en función de la información disponible.
        /// </summary>
        private void SetVisibilidadColumnasMatrices()
        {
            for (int i = 1; i < objectListView_multioperador.AllColumns.Count; i++)
            {
                objectListView_multioperador.AllColumns[i].Text = _parametrosBase.ColumnaMultioperador(i - 1);
                objectListView_multioperador.AllColumns[i].IsVisible = _parametrosBase.IndiceFilaMultioperador(i) != null ? (i <= _parametrosBase.MatrizMultioperador[_parametrosBase.IndiceFilaMultioperador(i)].Count) : false;
            }
            for (int i = 1; i < objectListView_flota_flota.AllColumns.Count; i++)
            {
                objectListView_flota_flota.AllColumns[i].Text = _parametrosBase.IndiceFlotaFlota(i - 1);
                objectListView_flota_flota.AllColumns[i].IsVisible = (i <= _parametrosBase.MatrizFlotaFlota.Count);

            }
            objectListView_flota_flota.RebuildColumns();
            objectListView_multioperador.RebuildColumns();
        }

        #endregion

        #region TAB Pairings

        /// <summary>
        /// Agrega nuevo pairing a la tabla
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AgregarPairing(object sender, EventArgs e)
        {
            if (ValidaValoresNuevoPairing())
            {
                ConexionPairing pairing = new ConexionPairing(integerTextBox1.Text, integerTextBox2.Text, TipoConexion.Pairing);
                pairing.SetAplicaDiaSemana(DayOfWeek.Monday, checkBox2.Checked);
                pairing.SetAplicaDiaSemana(DayOfWeek.Tuesday, checkBox3.Checked);
                pairing.SetAplicaDiaSemana(DayOfWeek.Wednesday, checkBox4.Checked);
                pairing.SetAplicaDiaSemana(DayOfWeek.Thursday, checkBox5.Checked);
                pairing.SetAplicaDiaSemana(DayOfWeek.Friday, checkBox6.Checked);
                pairing.SetAplicaDiaSemana(DayOfWeek.Saturday, checkBox7.Checked);
                pairing.SetAplicaDiaSemana(DayOfWeek.Sunday, checkBox8.Checked);
                if (!_parametrosBase.Conexiones.Pairings.ContainsValue(pairing))
                {
                    integerTextBox1.Text = "";
                    integerTextBox2.Text = "";
                    _parametrosBase.Conexiones.Pairings.Add(ConexionPairing.Serial, pairing);
                }
                CargarTabPairings();
            }
        }

        /// <summary>
        /// Borra fila seleccionada al presionar DEL o BACK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorrarFilaPairing(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                object o = objectListView_pairings.GetSelectedObject();
                if (o != null)
                {
                    ConexionPairing selected = (ConexionPairing)o;
                    _parametrosBase.Conexiones.Pairings.Remove(_parametrosBase.Conexiones.KeyOfPairings(selected));
                    CargarTabPairings();
                }
            }
        }

        /// <summary>
        /// Carga conexiones reales entre legs obtenidas en itinerario en visor de la derecha en función de la selección de pairings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CargarConexionesPairingsResultantes(object sender, EventArgs e)
        {
            ArrayList pairings = objectListView_pairings.GetSelectedObjects();
            if (pairings != null && pairings.Count > 0)
            {
                objectListView_pairings_resultantes.ClearObjects();
                foreach (object o in pairings)
                {
                    ConexionPairing pairing = (ConexionPairing)o;
                    List<ConexionLegs> conexiones = _itinerarioBase.GetConexionsPairing(pairing).ToList();
                    objectListView_pairings_resultantes.AddObjects(conexiones);
                }
            }
        }        

        /// <summary>
        /// Carga tabla de pairings
        /// </summary>
        private void CargarTabPairings()
        {
            if (_parametrosBase != null && _parametrosBase.Conexiones != null && _parametrosBase.Conexiones.Pairings != null && _parametrosBase.Conexiones.Pairings.Count > 0)
            {
                objectListView_pairings.ClearObjects();
                foreach (ConexionPairing conex in _parametrosBase.Conexiones.Pairings.Values)
                {
                    objectListView_pairings.AddObject(conex);
                }
            }
            else
            {
                objectListView_pairings.ClearObjects();
            }
        }

        /// <summary>
        /// Setea delegados sobre los campos de la tabla de pairing. 
        /// Los delegados permiten interactuar de manera sencilla entre los objetos de las listas y los campos que se despliegan.
        /// </summary>
        private void SetAspectGettersVisorPairings()
        {
            #region TABLA 1 PARAMETROS

            #region AspectGetters

            objectListView_pairings.AllColumns[0].AspectGetter = delegate(object o)
            {
                ConexionPairing conex = (ConexionPairing)o;
                return _parametrosBase.Conexiones.KeyOfPairings(conex);
            };

            objectListView_pairings.AllColumns[1].AspectGetter = delegate(object o)
            {
                ConexionPairing conex = (ConexionPairing)o;
                return conex.IdVuelo1;
            };
            objectListView_pairings.AllColumns[2].AspectGetter = delegate(object o)
            {
                ConexionPairing conex = (ConexionPairing)o;
                return conex.IdVuelo2;
            };
            objectListView_pairings.AllColumns[3].AspectGetter = delegate(object o)
            {
                ConexionPairing conex = (ConexionPairing)o;
                return conex.GetAplicaDiaSemana(0);
            };
            objectListView_pairings.AllColumns[4].AspectGetter = delegate(object o)
            {
                ConexionPairing conex = (ConexionPairing)o;
                return conex.GetAplicaDiaSemana(1);
            };
            objectListView_pairings.AllColumns[5].AspectGetter = delegate(object o)
            {
                ConexionPairing conex = (ConexionPairing)o;
                return conex.GetAplicaDiaSemana(2);
            };
            objectListView_pairings.AllColumns[6].AspectGetter = delegate(object o)
            {
                ConexionPairing conex = (ConexionPairing)o;
                return conex.GetAplicaDiaSemana(3);
            };
            objectListView_pairings.AllColumns[7].AspectGetter = delegate(object o)
            {
                ConexionPairing conex = (ConexionPairing)o;
                return conex.GetAplicaDiaSemana(4);
            };
            objectListView_pairings.AllColumns[8].AspectGetter = delegate(object o)
            {
                ConexionPairing conex = (ConexionPairing)o;
                return conex.GetAplicaDiaSemana(5);
            };
            objectListView_pairings.AllColumns[9].AspectGetter = delegate(object o)
            {
                ConexionPairing conex = (ConexionPairing)o;
                return conex.GetAplicaDiaSemana(6);
            };
            #endregion

            #region AspectPutters

            objectListView_pairings.AllColumns[1].AspectPutter = delegate(object goal, object value)
            {
                ConexionPairing conex = (ConexionPairing)goal;
                string aux = value.ToString();
                if (Utilidades.EsEnteroPositivo(aux))
                {
                    conex.IdVuelo1 = aux;
                }
                CargarConexionesPairingsResultantes(objectListView_pairings, new EventArgs());
            };

            objectListView_pairings.AllColumns[2].AspectPutter = delegate(object goal, object value)
            {
                ConexionPairing conex = (ConexionPairing)goal;
                string aux = value.ToString();
                if (Utilidades.EsEnteroPositivo(aux))
                {
                    conex.IdVuelo2 = aux;
                }
                CargarConexionesPairingsResultantes(objectListView_pairings, new EventArgs());
            };

            objectListView_pairings.AllColumns[3].AspectPutter = delegate(object goal, object value)
            {
                ConexionPairing conex = (ConexionPairing)goal;
                conex.SetAplicaDiaSemana(DayOfWeek.Monday, (bool)value);
                CargarConexionesPairingsResultantes(objectListView_pairings, new EventArgs());
            };

            objectListView_pairings.AllColumns[4].AspectPutter = delegate(object goal, object value)
            {
                ConexionPairing conex = (ConexionPairing)goal;
                conex.SetAplicaDiaSemana(DayOfWeek.Tuesday, (bool)value);
                CargarConexionesPairingsResultantes(objectListView_pairings, new EventArgs());
            };

            objectListView_pairings.AllColumns[5].AspectPutter = delegate(object goal, object value)
            {
                ConexionPairing conex = (ConexionPairing)goal;
                conex.SetAplicaDiaSemana(DayOfWeek.Wednesday, (bool)value);
                CargarConexionesPairingsResultantes(objectListView_pairings, new EventArgs());
            };

            objectListView_pairings.AllColumns[6].AspectPutter = delegate(object goal, object value)
            {
                ConexionPairing conex = (ConexionPairing)goal;
                conex.SetAplicaDiaSemana(DayOfWeek.Thursday, (bool)value);
                CargarConexionesPairingsResultantes(objectListView_pairings, new EventArgs());
            };

            objectListView_pairings.AllColumns[7].AspectPutter = delegate(object goal, object value)
            {
                ConexionPairing conex = (ConexionPairing)goal;
                conex.SetAplicaDiaSemana(DayOfWeek.Friday, (bool)value);
                CargarConexionesPairingsResultantes(objectListView_pairings, new EventArgs());
            };

            objectListView_pairings.AllColumns[8].AspectPutter = delegate(object goal, object value)
            {
                ConexionPairing conex = (ConexionPairing)goal;
                conex.SetAplicaDiaSemana(DayOfWeek.Saturday, (bool)value);
                CargarConexionesPairingsResultantes(objectListView_pairings, new EventArgs());
            };

            objectListView_pairings.AllColumns[9].AspectPutter = delegate(object goal, object value)
            {
                ConexionPairing conex = (ConexionPairing)goal;
                conex.SetAplicaDiaSemana(DayOfWeek.Sunday, (bool)value);
                CargarConexionesPairingsResultantes(objectListView_pairings, new EventArgs());
            };

            #endregion

            #endregion

            #region TABLA 2 RESULTANTES

            #region GroupKeyGetters

            this.objectListView_pairings_resultantes.AllColumns[0].GroupKeyGetter = delegate(object o)
            {
                ConexionLegs t = (ConexionLegs)o;
                return t.ConexionBase.IdVuelo1 + "-" + t.ConexionBase.IdVuelo2;
            };            

            #endregion

            #region AspectGetters

            objectListView_pairings_resultantes.AllColumns[0].AspectGetter = delegate(object o)
            {
                int contador = 0;
                foreach (object o2 in objectListView_pairings_resultantes.Objects)
                {
                    contador++;
                    if (o == o2)
                        return contador;
                }
                return -1;
            };

            objectListView_pairings_resultantes.AllColumns[1].AspectGetter = delegate(object o)
            {
                ConexionLegs conexion = (ConexionLegs)o;
                Tramo ini = conexion.GetTramo(conexion.NumTramoIni);
                return ini.TramoBase.Fecha_Llegada.ToString("dddd, dd/MM");
            };
            objectListView_pairings_resultantes.AllColumns[2].AspectGetter = delegate(object o)
            {
                ConexionLegs conexion = (ConexionLegs)o;
                Tramo ini = conexion.GetTramo(conexion.NumTramoIni);
                return ini.ParOD;
            };
            objectListView_pairings_resultantes.AllColumns[3].AspectGetter = delegate(object o)
            {
                ConexionLegs conexion = (ConexionLegs)o;
                Tramo fin = conexion.GetTramo(conexion.NumTramoFin);
                return fin.ParOD;
            };
            objectListView_pairings_resultantes.AllColumns[4].AspectGetter = delegate(object o)
            {
                ConexionLegs conexion = (ConexionLegs)o;
                Tramo ini = conexion.GetTramo(conexion.NumTramoIni);
                return Utilidades.GetHora(ini.TramoBase.Hora_Llegada);
            };
            objectListView_pairings_resultantes.AllColumns[5].AspectGetter = delegate(object o)
            {
                ConexionLegs conexion = (ConexionLegs)o;
                Tramo fin = conexion.GetTramo(conexion.NumTramoFin);
                return Utilidades.GetHora(fin.TramoBase.Hora_Salida);
            };
            objectListView_pairings_resultantes.AllColumns[6].AspectGetter = delegate(object o)
            {
                ConexionLegs conexion = (ConexionLegs)o;
                return conexion.Separacion;
            };

            #endregion

            #endregion
        }

        /// <summary>
        /// Valida que los valores del nuevo pairing pertenezcan a su dominio.
        /// </summary>
        private bool ValidaValoresNuevoPairing()
        {
            bool textBoxsValidos = integerTextBox1.Text.Length > 0 && integerTextBox2.Text.Length > 0 && integerTextBox1.Int > 0 && integerTextBox2.Int > 0;
            bool checkBoxesValidos = checkBox2.Checked || checkBox3.Checked || checkBox4.Checked || checkBox5.Checked || checkBox6.Checked || checkBox7.Checked || checkBox8.Checked;
            return textBoxsValidos && checkBoxesValidos;
        }

        #endregion

        #region TAB Paxs

        /// <summary>
        /// Agrega fila a tabla de pasajeros en conexión
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AgregarPaxConex(object sender, EventArgs e)
        {
            if (ValidaValoresNuevoPax())
            {
                ConexionPasajeros conex_pax = new ConexionPasajeros(integerTextBox5.Text, integerTextBox6.Text, TipoConexion.Pasajeros, Convert.ToDouble(numericTextBox1.Text), Convert.ToDouble(numericTextBox2.Text));

                if (!_parametrosBase.Conexiones.PaxConex.ContainsValue(conex_pax))
                {
                    integerTextBox5.Text = "";
                    integerTextBox6.Text = "";
                    numericTextBox1.Text = "";
                    numericTextBox2.Text = "";
                    _parametrosBase.Conexiones.PaxConex.Add(ConexionPasajeros.Serial, conex_pax);
                }
                CargarTabPaxs();
            }
        }

        /// <summary>
        /// Borra fila seleccionada al presionar DEL o BACK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorrarFilaPaxs(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                object o = objectListView_paxs.GetSelectedObject();
                if (o != null)
                {
                    ConexionPasajeros selected = (ConexionPasajeros)o;
                    _parametrosBase.Conexiones.PaxConex.Remove(_parametrosBase.Conexiones.KeyOfPaxs(selected));
                    CargarTabPaxs();
                }
            }
        }

        /// <summary>
        /// Carga conexiones reales entre legs obtenidas en itinerario en visor de la derecha en función de la selección de conexiones realizada
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CargarConexionesPaxsResultantes(object sender, EventArgs e)
        {
            ArrayList paxs = objectListView_paxs.GetSelectedObjects();
            if (paxs != null && paxs.Count > 0)
            {
                objectListView_paxs_resultantes.ClearObjects();
                foreach (object o in paxs)
                {
                    ConexionPasajeros pax = (ConexionPasajeros)o;
                    List<ConexionLegs> conexiones = _itinerarioBase.GetConexionsPaxs(pax, _parametrosBase.Conexiones.Hubs, _parametrosBase.Escalares.Semilla).ToList();
                    objectListView_paxs_resultantes.AddObjects(conexiones);
                }
            }
        }

        /// <summary>
        /// Carga tabla de pasajeros en conexión
        /// </summary>
        private void CargarTabPaxs()
        {
            if (_parametrosBase != null && _parametrosBase.Conexiones != null && _parametrosBase.Conexiones.PaxConex != null && _parametrosBase.Conexiones.PaxConex.Count > 0)
            {
                objectListView_paxs.ClearObjects();
                foreach (ConexionPasajeros conex in _parametrosBase.Conexiones.PaxConex.Values)
                {
                    objectListView_paxs.AddObject(conex);
                }
            }
            else
            {
                objectListView_paxs.ClearObjects();
            }
        }

        /// <summary>
        /// Setea delegados sobre los campos de la tabla de visor de pasajeros.
        /// Los delegados permiten interactuar de manera sencilla entre los objetos de las listas y los campos que se despliegan.
        /// </summary>
        private void SetAspectGettersVisorPaxs()
        {
            #region TABLA 1 PARAMETROS

            #region AspectGetters

            objectListView_paxs.AllColumns[0].AspectGetter = delegate(object o)
            {
                ConexionPasajeros conex = (ConexionPasajeros)o;
                return _parametrosBase.Conexiones.KeyOfPaxs(conex);
            };

            objectListView_paxs.AllColumns[1].AspectGetter = delegate(object o)
            {
                ConexionPasajeros conex = (ConexionPasajeros)o;
                return conex.IdVuelo1;
            };
            objectListView_paxs.AllColumns[2].AspectGetter = delegate(object o)
            {
                ConexionPasajeros conex = (ConexionPasajeros)o;
                return conex.IdVuelo2;
            };
            objectListView_paxs.AllColumns[3].AspectGetter = delegate(object o)
            {
                ConexionPasajeros conex = (ConexionPasajeros)o;
                return conex.Paxs_Promedio;
            };
            objectListView_paxs.AllColumns[4].AspectGetter = delegate(object o)
            {
                ConexionPasajeros conex = (ConexionPasajeros)o;
                return conex.Pax_Desvest;
            };           
            #endregion

            #region AspectPutters

            objectListView_paxs.AllColumns[1].AspectPutter = delegate(object goal, object value)
            {
                ConexionPasajeros conex = (ConexionPasajeros)goal;
                string aux = value.ToString();
                if (Utilidades.EsEnteroPositivo(aux))
                {
                    conex.IdVuelo1 = aux;
                }
                CargarConexionesPaxsResultantes(objectListView_paxs, new EventArgs());
            };

            objectListView_paxs.AllColumns[2].AspectPutter = delegate(object goal, object value)
            {
                ConexionPasajeros conex = (ConexionPasajeros)goal;
                string aux = value.ToString();
                if (Utilidades.EsEnteroPositivo(aux))
                {
                    conex.IdVuelo2 = aux;
                }
                CargarConexionesPaxsResultantes(objectListView_paxs, new EventArgs());
            };

            objectListView_paxs.AllColumns[3].AspectPutter = delegate(object goal, object value)
            {
                ConexionPasajeros conex = (ConexionPasajeros)goal;
                string aux = value.ToString();
                if (Utilidades.EsNumeroPositivo(aux, true))
                {
                    conex.Paxs_Promedio = Convert.ToDouble(aux);
                }
                CargarConexionesPaxsResultantes(objectListView_paxs, new EventArgs());
            };

            objectListView_paxs.AllColumns[4].AspectPutter = delegate(object goal, object value)
            {
                ConexionPasajeros conex = (ConexionPasajeros)goal;
                string aux = value.ToString();
                if (Utilidades.EsNumeroPositivo(aux, true))
                {
                    conex.Pax_Desvest = Convert.ToDouble(aux);
                }
                CargarConexionesPaxsResultantes(objectListView_paxs, new EventArgs());
            };

            #endregion

            #endregion

            #region TABLA 2 RESULTANTES

            #region GroupKeyGetters

            this.objectListView_paxs_resultantes.AllColumns[0].GroupKeyGetter = delegate(object o)
            {
                ConexionLegs t = (ConexionLegs)o;
                return t.ConexionBase.IdVuelo1 + "-" + t.ConexionBase.IdVuelo2;
            };

            #endregion

            #region AspectGetters

            objectListView_paxs_resultantes.AllColumns[0].AspectGetter = delegate(object o)
            {
                int contador = 0;
                foreach (object o2 in objectListView_paxs_resultantes.Objects)
                {
                    contador++;
                    if (o == o2)
                        return contador;
                }
                return -1;
            };

            objectListView_paxs_resultantes.AllColumns[1].AspectGetter = delegate(object o)
            {
                ConexionLegs conexion = (ConexionLegs)o;
                Tramo ini = conexion.GetTramo(conexion.NumTramoIni);
                return ini.TramoBase.Fecha_Llegada.ToString("dddd, dd/MM");
            };
            objectListView_paxs_resultantes.AllColumns[2].AspectGetter = delegate(object o)
            {
                ConexionLegs conexion = (ConexionLegs)o;
                Tramo ini = conexion.GetTramo(conexion.NumTramoIni);
                return ini.ParOD;
            };
            objectListView_paxs_resultantes.AllColumns[3].AspectGetter = delegate(object o)
            {
                ConexionLegs conexion = (ConexionLegs)o;
                Tramo fin = conexion.GetTramo(conexion.NumTramoFin);
                return fin.ParOD;
            };
            objectListView_paxs_resultantes.AllColumns[4].AspectGetter = delegate(object o)
            {
                ConexionLegs conexion = (ConexionLegs)o;
                Tramo ini = conexion.GetTramo(conexion.NumTramoIni);
                return Utilidades.GetHora(ini.TramoBase.Hora_Llegada);
            };
            objectListView_paxs_resultantes.AllColumns[5].AspectGetter = delegate(object o)
            {
                ConexionLegs conexion = (ConexionLegs)o;
                Tramo fin = conexion.GetTramo(conexion.NumTramoFin);
                return Utilidades.GetHora(fin.TramoBase.Hora_Salida);
            };
            objectListView_paxs_resultantes.AllColumns[6].AspectGetter = delegate(object o)
            {
                ConexionLegs conexion = (ConexionLegs)o;
                return conexion.Separacion;
            };

            #endregion

            #endregion
        }

        /// <summary>
        /// Valida que los valores de la nueva conexión pertenezcan a su dominio.
        /// </summary>
        private bool ValidaValoresNuevoPax()
        {
            bool textBoxsValidos = integerTextBox5.Text.Length > 0 && integerTextBox6.Text.Length > 0 && integerTextBox5.Int > 0 && integerTextBox6.Int > 0;
            bool numerosValidos = numericTextBox1.Text.Length > 0 && numericTextBox2.Text.Length > 0;
            return textBoxsValidos && numerosValidos;
        }

        #endregion

        #endregion

        #endregion

        #region METODOS DE ACCION ASOCIADOS A BOTONES Y MENÚES

        #region BOTON ITINERARIO

        /// <summary>
        /// Abre diálogo para cargar itinerario
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Itin_Click(object sender, EventArgs e)
        {
            openFileDialog_abrirItinerarioXLS.ShowDialog();
        }

        #endregion

        #region BOTON PARAMETROS

        /// <summary>
        /// Lleva a visor/editar de parámetros
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editarParametrosToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 2;
        }

        /// <summary>
        /// Abre diálogo para cargar parámetros
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem_cargar_parametros_Click(object sender, EventArgs e)
        {
            openFileDialog_parametros.ShowDialog();
        }

        /// <summary>
        ///  Abre diálogo para guardar parámetros
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem_guardar_parametros_Click(object sender, EventArgs e)
        {
            if (_parametrosBase != null && _OK_Parametros)
            {
                saveFileDialog_parametros.ShowDialog();
            }
            else
            {
                mensajes.Invoke(_enviarMensajeInterfaz, "Error, no hay parámetros cargados");
            }
        }

        #endregion

        #region BOTON CURVAS

        /// <summary>
        /// Abre ventana de actualizador de curvas.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void actualizarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_actualizadorAbierto)
            {
                ActualizadorCurvas ac = new ActualizadorCurvas(this);
                ac.Show();
            }
        }

        /// <summary>
        /// Lleva a visor/editar de curvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editarCurvasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 1;
        }

        /// <summary>
        /// Abre diálogo para cargar curvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem_cargar_curvas_Click(object sender, EventArgs e)
        {
            openFileDialog_curvas.ShowDialog();
        }

        /// <summary>
        ///  Abre diálogo para guardar curvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem_guardar_curvas_Click(object sender, EventArgs e)
        {
            if (_modeloDisrupcionesBase != null && _modeloDisrupcionesBase.ColeccionDisrupciones != null && _modeloDisrupcionesBase.ColeccionDisrupciones.Count > 0)
            {
                saveFileDialog_curvas.ShowDialog();
            }
            else
            {
                mensajes.Invoke(_enviarMensajeInterfaz, "Error, no hay curvas cargadas");
            }
        }

        #endregion

        #region BOTON CURVAS WXS HISTORICAS

        /// <summary>
        /// Borra la información cargada de curvas de wxs históricas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void limpiarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _modeloDisrupcionesBase.CurvasAeropuerto.Clear();
        }

        /// <summary>
        ///  Abre diálogo para cargar curvas de wxs históricas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem_cargar_wxs_Click(object sender, EventArgs e)
        {
            openFileDialog_wxs.ShowDialog();
        }

        /// <summary>
        ///  Abre diálogo para guardar curvas de wxs históricas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem_guardar_wxs_Click(object sender, EventArgs e)
        {
            saveFileDialog_wxs.ShowDialog();
        }

        #endregion

        #region BOTON SIMULACION NORMAL

        /// <summary>
        /// Se ejecuta al hacer click sobre botón de simulación normal. Hace validación. Si hay problemas abre validador; si no, abre menú de simulación normal.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Simular_Normal_Click(object sender, EventArgs e)
        {
            if (!_validadorAbierto)
            {
                if (TodoListoParaSimular)
                {
                    RefreshTables(null, new ItemsChangedEventArgs());
                    ControladorInformacion controlador = new ControladorInformacion(_itinerarioBase, _parametrosBase, _modeloDisrupcionesBase);
                    controlador.Validar();
                    if (!_simulando && TodoListoParaSimular && !_simulacionSimpleAbierta && !_simulacionMultiAbierta && !controlador.HayFaltas)
                    {
                        _simulacionSimpleAbierta = true;
                        _menuSimulacionNormal.Actualizar();
                        _menuSimulacionNormal.CargarFechas();
                        _menuSimulacionNormal.Show();
                    }
                    else if (controlador.HayFaltas)
                    {
                        Validador validador = new Validador(this, controlador);
                        validador.Show();
                        _validadorAbierto = true;
                    }
                }
            }

        }

        #endregion

        #region BOTON SIMULACION MULTIESCENARIO

        /// <summary>
        /// Se ejecuta al hacer click sobre botón de simulación multiescenario. Hace validación. Si hay problemas abre validador; si no, abre menú de simulación multiescenario.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_multi_sim_Click(object sender, EventArgs e)
        {
            if (TodoListoParaSimular)
            {
                ControladorInformacion controlador = new ControladorInformacion(_itinerarioBase, _parametrosBase, _modeloDisrupcionesBase);
                controlador.Validar();
                if (!_simulando && TodoListoParaSimular && !_simulacionMultiAbierta && !_simulacionSimpleAbierta && !controlador.HayFaltas)
                {
                    _simulacionMultiAbierta = true;
                    _menuSimulacionMultiescenario.Actualizar();
                    _menuSimulacionMultiescenario.CargarFechas();
                    _menuSimulacionMultiescenario.Show();
                }
                else if (controlador.HayFaltas)
                {
                    Validador validador = new Validador(this, controlador);
                    validador.Show();
                }
            }
        }

        #endregion


        #region TOOL STRIP MENU PRINCIPAL

        /// <summary>
        /// Despliega ventana de información sobre la aplicación
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void acercaDeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About(this);
            about.Show();
        }

        /// <summary>
        /// Carga último itinerario cargado desde rutas guardadas en appConfig
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cargarUltimoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filenameItin = _config.GetParametro("itin_file").ToString();
            string filenameCurvas = _config.GetParametro("curvas_file").ToString();
            string filenameWXS = _config.GetParametro("wxs_file").ToString();
            string filenameParams = _config.GetParametro("params_file").ToString();
            CargarItinerario(filenameItin);
            CargarCurvas(filenameCurvas);
            CargarCurvasWXS(_modeloDisrupcionesBase, filenameWXS);
            CargarParametros(filenameParams);
        }

        /// <summary>
        /// Cierra SimuLAN
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion
        #endregion

        #region CREACION DE REPORTES

        /// <summary>
        /// Crea reportes de simulación multiescenario
        /// </summary>
        /// <param name="outputSimulacionMultiescenario">Salida de simulación multiescenario general</param>
        /// <param name="outputSimulacionMultiescenarioNegocios">Salida de simulación multiescenario por negocios</param>
        /// <param name="negocios">Lista de los negocios definidos en la simulación</param>
        /// <param name="enviarMsj">Referencia a delegado para escribir mensajes en la interfaz</param>
        private void CrearReportesMultiescenario(Dictionary<string, Dictionary<int, Dictionary<int, double>>> outputSimulacionMultiescenario, Dictionary<string, Dictionary<string, Dictionary<int, Dictionary<int, double>>>> outputSimulacionMultiescenarioNegocios, List<string> negocios, EnviarMensajeEventHandler enviarMsj)
        {
            try
            {
                ReportManager rm = new ReportManager(_outputPath, _escribeReporte, _escribeGrupo, _stds, _menuSimulacionMultiescenario.ID_Reportes,_menuSimulacionMultiescenario.FechaInicioReportes,_menuSimulacionMultiescenario.FechaTerminoReportes);
                rm.CreaReportesSimulacionMultiescenario(outputSimulacionMultiescenario, outputSimulacionMultiescenarioNegocios, negocios, enviarMsj);
            }
            catch (Exception e)
            {
                throw e;
            }
            if (_abrirCarpetaOutput)
            {
                System.Diagnostics.Process.Start(_outputPath);
            }
        }

        /// <summary>
        /// Crea reportes de simulación normal
        /// </summary>
        /// <param name="informacionReplicas">Lista con la información de todas las réplicas</param>
        /// <param name="enviarMsj">Referencia a delegado para escribir mensajes en la interfaz</param>
        private void CrearReportesNormal(List<Simulacion> informacionReplicas, EnviarMensajeEventHandler enviarMsj)
        {
            try
            {
                ReportManager rm = new ReportManager(_outputPath, _escribeReporte, _escribeGrupo, _stds, _menuSimulacionNormal.ID_Reportes, _menuSimulacionNormal.FechaInicioReportes, _menuSimulacionNormal.FechaTerminoReportes);
                rm.CreaReportesSimulacionNormal(informacionReplicas, enviarMsj);
            }
            catch (Exception e)
            {
                throw e;
            }
            if (_abrirCarpetaOutput)
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo(_outputPath));
            }
        }
                
        #endregion

        #region CARGAR/GUARDAR DATA

        #region CURVAS

        /// <summary>
        /// Carga archivo de curvas desde ruta de acceso
        /// </summary>
        /// <param name="filename">Ruta de acceso al archivo de curvas</param>
        private void CargarCurvas(string filename)
        {
            FileStream fs = null;
            try
            {
                bool exitoso = false;
                fs = new FileStream(filename, FileMode.Open);
                ExcelDataReader edr = new ExcelDataReader(fs);
                DataSet ds = edr.WorkbookData;
                int cantidadCargadas;
                SimuLAN_DAO.DataSetToModeloDisrupciones(_modeloDisrupcionesBase, ds, out exitoso, true, out cantidadCargadas);
                _modeloDisrupcionesBase.CargarTipoDistribucionEnDisrupciones();
                _modeloDisrupcionesBase.LimpiarDiccionario();
                _modeloDisrupcionesBase.SetColumnNamesDataTables();
                fs.Close();
                if (exitoso)
                {
                    mensajes.Invoke(_enviarMensajeInterfaz, "Curvas cargadas exitosamente (" + cantidadCargadas + " curvas cargadas)");
                    _OK_Distribuciones = true;
                    CargarComboBoxCurvas();
                    PostProcesarInformacion();
                }
                else
                {
                    mensajes.Invoke(_enviarMensajeInterfaz, "Error al cargar el archivo de curvas.");
                }
            }
            catch
            {
                if (fs != null)
                {
                    fs.Close();
                }
                mensajes.Invoke(_enviarMensajeInterfaz, "Error al cargar el archivo de curvas.");
            }
        }

        /// <summary>
        /// Carga curvas de clima históricas
        /// </summary>
        /// <param name="modelo">Referencia a modelo de disrupciones</param>
        /// <param name="filename">Ruta del archivo que contiene las curvas</param>
        private void CargarCurvasWXS(ModeloDisrupciones modelo, string filename)
        {
            bool exitoso = false;
            string msg;
            SimuLAN_DAO.CargarCurvasWXS(_modeloDisrupcionesBase, filename, out exitoso, out msg);
            if (exitoso)
            {
                mensajes.Invoke(_enviarMensajeInterfaz, "Curvas WXS cargadas correctamente");
            }
            else
            {
                mensajes.Invoke(_enviarMensajeInterfaz, msg);
            }
        }

        /// <summary>
        /// Guarda las curvas en hoja de cálculo excel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveFileDialog_curvas_FileOk(object sender, CancelEventArgs e)
        {
            Invoke(new Action<Cursor>(CambiarCursor), Cursors.WaitCursor);
            string[] partsFileName = saveFileDialog_curvas.FileName.Split('.');
            _modeloDisrupcionesBase.Refresh();
            ExcelExport.ExportToExcel(_modeloDisrupcionesBase.DictionaryDisrupcionesToDataset(), saveFileDialog_curvas.FileName);            
            mensajes.Invoke(_enviarMensajeInterfaz, "Curvas guardadas exitosamente");
            Invoke(new Action<Cursor>(CambiarCursor), Cursors.Default);
        }

        /// <summary>
        /// Guarda las curvas históricas de clima en hoja de cálculo excel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveFileDialog_wxs_FileOk(object sender, CancelEventArgs e)
        {
            Invoke(new Action<Cursor>(CambiarCursor), Cursors.WaitCursor);
            try
            {

                DataSet data = _modeloDisrupcionesBase.DictionaryInfoWXSToDataset();
                ExcelExport.ExportToExcel(data, saveFileDialog_wxs.FileName);
                mensajes.Invoke(_enviarMensajeInterfaz, "Curvas guardadas exitosamente.");
                Invoke(new Action<Cursor>(CambiarCursor), Cursors.Default);
            }
            catch
            {
                Invoke(new Action<Cursor>(CambiarCursor), Cursors.Default);
                throw new Exception("No se pudo guardar curvas de WXS.");
            }
        }

        /// <summary>
        /// Método de acción ajecutado cuando se selecciona un archivo de curvas para ser cargado en SimuLAN
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openFileDialog_curvas_FileOk(object sender, CancelEventArgs e)
        {
            Invoke(new Action<Cursor>(CambiarCursor), Cursors.WaitCursor);
            string filename = openFileDialog_curvas.FileName;
            _config.ReplaceSetting("curvas_dir", Path.GetDirectoryName(filename));
            _config.ReplaceSetting("curvas_file", filename);
            CargarCurvas(filename);
            Invoke(new Action<Cursor>(CambiarCursor), Cursors.Default);
        }

        /// <summary>
        /// Método de acción ajecutado cuando se selecciona un archivo de curvas históricas de wxs para ser cargado en SimuLAN
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openFileDialog_wxs_FileOk(object sender, CancelEventArgs e)
        {
            Invoke(new Action<Cursor>(CambiarCursor), Cursors.WaitCursor);
            string filename = openFileDialog_wxs.FileName;
            _config.ReplaceSetting("wxs_dir", Path.GetDirectoryName(filename));
            _config.ReplaceSetting("wxs_file", filename);
            CargarCurvasWXS(_modeloDisrupcionesBase, filename);
            Invoke(new Action<Cursor>(CambiarCursor), Cursors.Default);
        }

        #endregion

        #region ITINERARIO

        /// <summary>
        /// Carga itinerario en escenario
        /// </summary>
        /// <param name="filename">Ruta que contiene el archivo de itinerario a ser cargado</param>
        private void CargarItinerario(string filename)
        {
            try
            {
                openFileDialog_abrirItinerarioXLS.FileName = filename;
                string extension = filename.Substring(filename.Length - 3, 3);
                openFileDialog_abrirItinerarioXLS.FileName = filename;
                Stream fs = openFileDialog_abrirItinerarioXLS.OpenFile();
                _itinerarioBase = SimuLAN_DAO.CargarItinerario(filename, extension, _parametrosBase, fs, ref _OK_Itinerario);

                if (_OK_Itinerario)
                {
                    mensajes.Invoke(_enviarMensajeInterfaz, "Itinerario cargado exitosamente");
                }
                else
                {
                    mensajes.Invoke(_enviarMensajeInterfaz, "Error al cargar itinerario");
                }
            }
            catch (IOException e)
            {
                mensajes.Invoke(_enviarMensajeInterfaz, e.Message);
            }
            catch
            {
                mensajes.Invoke(_enviarMensajeInterfaz, "Error al cargar itinerario");
            }
            PostProcesarInformacion();
        }

        /// <summary>
        ///Método de acción ajecutado cuando se selecciona un archivo de itinerario para ser cargado en SimuLAN
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openFileDialog_abrirItinerarioXLS_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                Invoke(new Action<Cursor>(CambiarCursor), Cursors.WaitCursor);
                string filename = openFileDialog_abrirItinerarioXLS.FileName;
                _config.ReplaceSetting("itin_dir", Path.GetDirectoryName(filename));
                _config.ReplaceSetting("itin_file", filename);
                CargarItinerario(filename);
                Invoke(new Action<Cursor>(CambiarCursor), Cursors.Default);
            }
            catch (Exception ex)
            {
                Invoke(new Action<Cursor>(CambiarCursor), Cursors.Default);
                mensajes.Text = "No se puede cargar itinerario. Error: " + ex.Message;
            }
        }

        #endregion

        #region PARAMETROS

        /// <summary>
        /// Carga parámetros con valores de tabla desde excel
        /// </summary>
        /// <param name="filename">>Ruta de acceso al archivo de parámetros</param>
        private void CargarParametros(string filename)
        {
            try
            {
                Invoke(new Action<Cursor>(CambiarCursor), Cursors.WaitCursor);
                FileStream fs = new FileStream(filename, FileMode.Open);
                ExcelDataReader edr = new ExcelDataReader(fs);
                DataSet ds = edr.WorkbookData;
                fs.Close();
                SimuLAN_DAO.CargarParametros(ds, _parametrosBase, _factoresEscenarios, filename);
                _OK_Parametros = true;
                tabControl2.Visible = true;
                CargarComboBoxTablasParametros();
                SetAspectGettersVisorPairings();
                SetAspectGettersVisorPaxs();
                SetGettersVisorMatrices();
                mensajes.Invoke(_enviarMensajeInterfaz, "Parámetros cargados exitosamente");
                PostProcesarInformacion();

            }
            catch (FileNotFoundException)
            {
                Button_multi_sim.Visible = false;
                Button_Simular_Normal.Visible = false;
                _OK_Parametros = false;
                mensajes.Invoke(_enviarMensajeInterfaz, "No se encuentra el archivo de parámetros en " + filename);
            }
            catch (IOException e)
            {
                mensajes.Invoke(_enviarMensajeInterfaz, e.Message);
            }
            catch (Exception e)
            {
                Button_multi_sim.Visible = false;
                Button_Simular_Normal.Visible = false;
                _OK_Parametros = false;                
            }
            finally
            {
                Invoke(new Action<Cursor>(CambiarCursor), Cursors.Default);
            }
        }

        /// <summary>
        /// Crea tablas de factores de escenarios
        /// </summary>
        /// <param name="factores">Información con los factores de escenarios</param>
        /// <param name="nombre">Nombre de la tabla</param>
        /// <returns></returns>
        private DataTable CrearDataTableFactorEscenario(Dictionary<string, Dictionary<TipoEscenarioDisrupcion, double>> factores, string nombre)
        {
            DataTable retorno = new DataTable(nombre);
            retorno.Columns.Add(new DataColumn("Agregación"));
            retorno.Columns.Add(new DataColumn("Factor Bueno"));
            retorno.Columns.Add(new DataColumn("Factor Malo"));
            foreach (string s in factores.Keys)
            {
                object valorAgregacion = s;
                object valorFactorBueno = factores[s][TipoEscenarioDisrupcion.Bueno];
                object valorFactorMalo = factores[s][TipoEscenarioDisrupcion.Malo];
                object[] itemsFila = { valorAgregacion, valorFactorBueno, valorFactorMalo };
                retorno.Rows.Add(itemsFila);
            }
            return retorno;
        }

        /// <summary>
        ///Método de acción ajecutado cuando se selecciona un archivo de parámetros para ser cargado en SimuLAN
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openFileDialog_parametros_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                Invoke(new Action<Cursor>(CambiarCursor), Cursors.WaitCursor);
                string filename = openFileDialog_parametros.FileName;
                _config.ReplaceSetting("params_dir", Path.GetDirectoryName(filename));
                _config.ReplaceSetting("params_file", filename);
                CargarParametros(filename);
                Invoke(new Action<Cursor>(CambiarCursor), Cursors.Default);
            }
            catch (Exception)
            {
                Invoke(new Action<Cursor>(CambiarCursor), Cursors.Default);
                mensajes.Text = "Error al cargar parámetros";
            }
        }

        /// <summary>
        /// Guarda los parámetros en hoja de cálculo excel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveFileDialog_parametros_FileOk(object sender, CancelEventArgs e)
        {
            Invoke(new Action<Cursor>(CambiarCursor), Cursors.WaitCursor);
            DataSet parametrosDataSet = new DataSet();
            parametrosDataSet.Tables.Add(_parametrosBase.MapFlotas.SerializableDictionaryToDataTable());
            parametrosDataSet.Tables.Add(_parametrosBase.MatrizFlotaFlotaToDataTable());
            parametrosDataSet.Tables.Add(_parametrosBase.MatrizMultioperadorToDataTable());
            parametrosDataSet.Tables.Add(_parametrosBase.MapGruposFlotas.SerializableDictionaryToDataTable());
            parametrosDataSet.Tables.Add(_parametrosBase.MapSubFlotasMatriculas.SerializableDictionaryToDataTable());
            parametrosDataSet.Tables.Add(_parametrosBase.MapVuelosRutas.SerializableDictionaryToDataTable());
            parametrosDataSet.Tables.Add(_parametrosBase.InfoGruposFlotasToDataTable());
            parametrosDataSet.Tables.Add(_parametrosBase.TurnAroundMin.SerializableDictionaryToDataTable());
            parametrosDataSet.Tables.Add(_parametrosBase.Conexiones.InputPairingsToDataTable());
            parametrosDataSet.Tables.Add(_parametrosBase.Conexiones.InputHubsToDataTable());
            parametrosDataSet.Tables.Add(_parametrosBase.Conexiones.ControladorConexionesPax.InputIntervalosDecisionToDataTable());
            parametrosDataSet.Tables.Add(_parametrosBase.Conexiones.ControladorConexionesPax.InputMinutosEsperaToDataTable());
            parametrosDataSet.Tables.Add(_parametrosBase.Conexiones.InputPaxConexToDataTable());
            parametrosDataSet.Tables.Add(CrearDataTableFactorEscenario(_factoresEscenarios[TipoDisrupcion.METEREOLOGIA], "fd_WXS"));
            parametrosDataSet.Tables.Add(CrearDataTableFactorEscenario(_factoresEscenarios[TipoDisrupcion.MANTENIMIENTO], "fd_Mantto"));
            parametrosDataSet.Tables.Add(_parametrosBase.InfoAOGToDataTable());
            ExcelExport.ExportToExcel(parametrosDataSet, saveFileDialog_parametros.FileName);
            mensajes.Invoke(_enviarMensajeInterfaz, "Parámetros guardados exitosamente");
            Invoke(new Action<Cursor>(CambiarCursor), Cursors.Default);
        }

       
        #endregion

        #endregion

        #region CLOSE

        /// <summary>
        /// Controla proceso de cierre de la interfaz
        /// </summary>
        /// <param name="e">Argumentos de cancelación</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Dispose(true);
        }

        #endregion

        #endregion
    }
}
