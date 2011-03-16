using System;
using System.Collections.Generic;
using System.Text;
using SimuLAN.Clases;
using SimuLAN.Clases.Disrupciones;

namespace SimuLAN.Clases
{
    public class ManagerSimulacion
    {
        #region ATRIBUTES

        private Itinerario _itinerario_base;
        private ParametrosSimuLAN _parametros_base;
        private ModeloDisrupciones _modeloDisrupciones_base;
        private DateTime _fecha_ini;
        private DateTime _fecha_fin;
        private bool _simulacion_cancelada;
        private EnviarMensajeEventHandler _enviarMensaje_simulacion;
        private ActualizarPorcentajeEventHandler _actualizar_porcentaje;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Lista de estándares con los que se estimará la puntualidad en los diferentes reportes.
        /// </summary>
        internal List<int> _stds;

        /// <summary>
        /// Itinerario base de la simulación
        /// </summary>
        internal Itinerario ItinerarioBase
        {
            get { return _itinerario_base; }
            set { _itinerario_base = value; }
        }

        #endregion

        #region CONSTRUCTOR

        public ManagerSimulacion(Itinerario _itinerarioBase, ParametrosSimuLAN _parametrosBase, ModeloDisrupciones _modeloDisrupcionesBase, List<int> _stds, DateTime _fechaIni, DateTime _fechaFin, EnviarMensajeEventHandler _enviarMensajeSimulacion, ActualizarPorcentajeEventHandler _actualizarPorcentaje, ref bool _simulacion_cancelada)
        {
            this._itinerario_base = _itinerarioBase;
            this._parametros_base = _parametrosBase;
            this._modeloDisrupciones_base = _modeloDisrupcionesBase;
            this._fecha_ini = _fechaIni;
            this._fecha_fin = _fechaFin;
            this._simulacion_cancelada = _simulacion_cancelada;
            this._actualizar_porcentaje = _actualizarPorcentaje;
            this._enviarMensaje_simulacion = _enviarMensajeSimulacion;
            this._stds = _stds;

        }
        
        #endregion

        #region METHODS

        public List<Simulacion> SimularNormal()
        {
            List<Simulacion> informacionReplicas = new List<Simulacion>();
            int[] semillaReplicas = GenerarSemillas(_parametros_base.Escalares.Replicas, _parametros_base.Escalares.Semilla);
            _actualizar_porcentaje("0%");
            _enviarMensaje_simulacion("Simulando...");        
            CargarDelegadosWxsEnAeropuertos();
            _modeloDisrupciones_base.Refresh();
            //Crea conexiones
            _itinerario_base.CrearConexiones(_parametros_base);
            //Simulaciones por réplica
            for (int i = 0; i < _parametros_base.Escalares.Replicas; i++)
            {
                Simulacion sim;

                //Realiza proceso de simulación
                SimularReplica(semillaReplicas[i], out sim, _fecha_ini, _fecha_fin);
                _actualizar_porcentaje(Convert.ToString(Convert.ToInt32(100 * (i + 1) / _parametros_base.Escalares.Replicas)) + "%");
                //Agrega información de réplica en lista de simulaciones.
                informacionReplicas.Add(sim);

                if (_simulacion_cancelada)
                {
                    break;
                }
            }                    
            GC.Collect();
            return informacionReplicas;
        }

        /// <summary>
        /// Simula uno de los escenarios que componen el proceso de simulación multiescenario.
        /// </summary>
        /// <param name="data_reporte_multiescenario">Diccionario con la información almacenada para los reportes de simulación multiescenario general</param>
        /// <param name="data_reporte_multiescenario_negocios">Diccionario con la información almacenada para los reportes de simulación multiescenario por negocios</param>
        internal void SimularEscenario(out Dictionary<int, Dictionary<int, double>> data_reporte_multiescenario, out Dictionary<string, Dictionary<int, Dictionary<int, double>>> data_reporte_multiescenario_negocios)
        {
            //Inicializa diccionarios de output
            data_reporte_multiescenario = new Dictionary<int, Dictionary<int, double>>();
            data_reporte_multiescenario_negocios = new Dictionary<string, Dictionary<int, Dictionary<int, double>>>();
            foreach (string negocio in _itinerario_base.Negocios)
            {
                data_reporte_multiescenario_negocios.Add(negocio, new Dictionary<int, Dictionary<int, double>>());
            }

            //Genera semillas
            int[] semillaReplicas = GenerarSemillas(_parametros_base.Escalares.Replicas, _parametros_base.Escalares.Semilla);

            //Carga delegadoss de control del clima en aeropuertos
            CargarDelegadosWxsEnAeropuertos();

            //Crea conexiones
            _itinerario_base.CrearConexiones(_parametros_base);

            //Simulaciones por réplica
            for (int i = 0; i < _parametros_base.Escalares.Replicas; i++)
            {
                Simulacion sim;

                //Realiza proceso de simulación
                SimularReplica(semillaReplicas[i], out sim, _fecha_ini, _fecha_fin);

                //Estima puntualidad
                data_reporte_multiescenario.Add(i, sim.StdCalculado);
                foreach (string negocio in _itinerario_base.Negocios)
                {
                    data_reporte_multiescenario_negocios[negocio].Add(i, sim.EstimarPuntualidadNegocio(negocio));
                }

                //Libera memoria
                sim.Dispose();
            }
            GC.Collect();
        }

        /// <summary>
        ///  Inicia proceso de simulación multiescenario por clima y mantenimiento.
        /// </summary>
        public void SimularMultiescenario(out Dictionary<string, Dictionary<int, Dictionary<int, double>>> outputSimulacionMultiescenario, out Dictionary<string, Dictionary<string, Dictionary<int, Dictionary<int, double>>>> outputSimulacionMultiescenarioNegocios)
        {
            //Carga escenarios
            Dictionary<int, TipoEscenarioDisrupcion> escenarios = new Dictionary<int, TipoEscenarioDisrupcion>();
            escenarios.Add(1, TipoEscenarioDisrupcion.Bueno);
            escenarios.Add(2, TipoEscenarioDisrupcion.Normal);
            escenarios.Add(3, TipoEscenarioDisrupcion.Malo);
            int contador = 1;

            //Carga diccionarios que almacenan output del proceso para la generación de reportes.
            outputSimulacionMultiescenario = new Dictionary<string, Dictionary<int, Dictionary<int, double>>>();
            outputSimulacionMultiescenarioNegocios = new Dictionary<string, Dictionary<string, Dictionary<int, Dictionary<int, double>>>>();

            //Itera sobre escenario de metereología
            foreach (int key1 in escenarios.Keys)
            {
                _modeloDisrupciones_base.MapDisrupcionesEscenario[TipoDisrupcion.METEREOLOGIA] = escenarios[key1];

                //Itera sobre escenario de mantto
                foreach (int key2 in escenarios.Keys)
                {
                    string nombreExperimento = "Escenario Clima " + escenarios[key1] + " - Escenario Mantto " + escenarios[key2];
                    string keyExperimento = key1 + "-" + key2;
                    _modeloDisrupciones_base.MapDisrupcionesEscenario[TipoDisrupcion.MANTENIMIENTO] = escenarios[key2];
                    this._enviarMensaje_simulacion("Inicio experimento " + contador + ":\n" + nombreExperimento + ".");
                    _actualizar_porcentaje(Convert.ToString(Convert.ToInt32(100.0 * (contador - 1) / (escenarios.Count * escenarios.Count))) + "%");
                    Dictionary<int, Dictionary<int, double>> data_reporte_multiescenario;
                    Dictionary<string, Dictionary<int, Dictionary<int, double>>> data_reporte_multiescenario_rutas;
                    //ManagerSimulacion manager = new ManagerSimulacion(_itinerarioBase, _parametrosBase, _modeloDisrupcionesBase, _stds, _menuSimulacionMultiescenario.FechaInicioReportes, _menuSimulacionMultiescenario.FechaTerminoReportes, cambiarVista, this._enviarMensajeSimulacion, actualizarPorcentaje, ref _simulacion_cancelada);
                    ////Hace proceso de simulación de un escenario
                    SimularEscenario(out data_reporte_multiescenario, out data_reporte_multiescenario_rutas);
                    //Post proceso output de la simulación del escenario
                    outputSimulacionMultiescenario.Add(keyExperimento, data_reporte_multiescenario);
                    outputSimulacionMultiescenarioNegocios.Add(keyExperimento, data_reporte_multiescenario_rutas);
                    contador++;
                    //Libera memoria 
                    GC.Collect();
                    if (_simulacion_cancelada)
                    {
                        break;
                    }
                }
                if (_simulacion_cancelada)
                {
                    break;
                }
            }          
        }

        /// <summary>
        /// Realiza una réplica de simulación        
        /// </summary>
        /// <param name="semilla">Semilla de la réplica</param>
        /// <param name="sim">Referencia a objeto que encapsula la simulación</param>
        /// <param name="fechaIni">Fecha de inicio del procesamiento de los reportes.</param>
        /// <param name="fechaFin">Fecha de término del procesamiento de los reportes.</param>       
        private void SimularReplica(int semilla, out Simulacion sim, DateTime fechaIni, DateTime fechaFin)
        {
            //Clona itinerario
            Itinerario itinerarioReplica = _itinerario_base.Clonar(semilla);

            //Carga turnos de backup
            itinerarioReplica.CargarTurnosBackup(_parametros_base.InfoGruposFlotas);

            //Carga info de WXS histórica
            itinerarioReplica.CargarInfoWXSHistoricaEnAeropuertos(_modeloDisrupciones_base.CurvasAeropuerto);

            //Crea instancia de simulación
            sim = new Simulacion(itinerarioReplica, _parametros_base, _modeloDisrupciones_base, _stds, fechaIni, fechaFin);

            //Inicia la simulación
            sim.Simular();

            //Procesa atrasos reaccionarios de  slots de mantto
            sim.Itinerario.PostProcesarSlotsMantto();

            //Estima puntualidad de la réplica recién procesada
            sim.EstimarPuntualidadReplica();
        }
        
        /// <summary>
        /// Genera una secuencia de números aleatorios sobre las distintas réplicas en función de una semilla principal 
        /// </summary>
        /// <param name="nReplicas">Cantidad de réplicas de simulación</param>
        /// <param name="semillaPrincipal">Semilla principal de la simulación</param>
        /// <returns>Arreglo con las semillas usadas por réplica</returns>
        private int[] GenerarSemillas(int nReplicas, int semillaPrincipal)
        {
            Random rdm = new Random(semillaPrincipal);
            int[] semillas = new int[nReplicas];
            for (int i = 0; i < nReplicas; i++)
            {
                semillas[i] = rdm.Next();
            }
            return semillas;
        }

        /// <summary>
        /// Carga delegados de para control climático sobre aeropuertos del itinerario.
        /// </summary>
        private void CargarDelegadosWxsEnAeropuertos()
        {
            List<string> keysAeropuertos = new List<string>();
            foreach (string s in _itinerario_base.AeropuertosDictionary.Keys)
            {
                keysAeropuertos.Add(s);
            }
            foreach (string s in keysAeropuertos)
            {
                _itinerario_base.AeropuertosDictionary[s].GetProbabilidadClima = _modeloDisrupciones_base.GetProbabilidadClimaAeropuerto();
            }
        }

        #endregion
        
    }
}
