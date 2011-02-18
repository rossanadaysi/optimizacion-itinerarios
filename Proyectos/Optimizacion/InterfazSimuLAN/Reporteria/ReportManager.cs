using System;
using System.Collections.Generic;
using System.Text;
using SimuLAN.Clases;
using System.Data;
using System.IO;
using SimuLAN.Clases.Recovery;

namespace InterfazSimuLAN.Reportes
{
    /// <summary>
    /// Enumeración con los nombres de los reportes que genera SimuLAN
    /// </summary>
    public enum NombreReporte { PuntualidadGeneral, PuntualidadGrupos, ExplicacionImpuntualidadGeneral, ExplicacionImpuntualidadGrupos, UtilizacionBackup, UtilizacionTurnos, PuntualidadMantto, Recovery, Detalles, PuntualidadMultiescenarioGeneral, PuntualidadMultiescenarioNegocio }

    /// <summary>
    /// Enumeración con los grupos de puntualidad/impuntualidad que genera SimuLAN
    /// </summary>
    public enum GruposReporte { Operador, Negocio, Flota, Subflota, Matricula, Estacion, ParOD, Vuelo, HubSalida };

    /// <summary>
    /// Clase que encapsula la lógica para generar informes.
    /// </summary>
    public class ReportManager
    {
        #region CONSTANTS

        /// <summary>
        /// Nivel de confianza utilizado para los intervalos de confianza en los reportes.
        /// </summary>
        public static string CONFIANZA = "5%";

        #endregion

        #region ATRIBUTES

        /// <summary>
        /// Diccionario que indica para cada grupo si será creado
        /// </summary>
        private Dictionary<GruposReporte, bool> _crea_grupo;

        /// <summary>
        /// Diccionario que indica para cada reporte si será creado
        /// </summary>
        private Dictionary<NombreReporte, bool> _crea_reporte;

        /// <summary>
        /// Fecha de inicio del procesamiento de los reportes.
        /// </summary>
        private DateTime _fecha_ini;

        /// <summary>
        /// Fecha de término del procesamiento de los reportes.
        /// </summary>
        private DateTime _fecha_termino;

        /// <summary>
        /// String con el sufijo especial para los reportes
        /// </summary>
        private string _id_reportes;

        /// <summary>
        /// Directorio donde se crearán los reportes
        /// </summary>
        private string _path;

        /// <summary>
        /// Lista de estándares de puntualidad soportados
        /// </summary>
        private List<int> _stds;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor de la clase
        /// </summary>
        /// <param name="path">Ruta donde se escriben los reportes</param>
        /// <param name="creaReporte">Diccionario para la creación de reportes</param>
        /// <param name="creaGrupo">Diccionario para la creación de grupos</param>
        /// <param name="stds">Lista de estándares de puntualidad soportados</param>
        /// <param name="id_reportes">String con el sufijo especial para los reportes</param>
        /// <param name="fechaIni">Fecha de inicio del procesamiento de los reportes.</param>
        /// <param name="fechaFin">Fecha de término del procesamiento de los reportes.</param>
        public ReportManager(string path, Dictionary<NombreReporte, bool> creaReporte, Dictionary<GruposReporte, bool> creaGrupo, List<int> stds, string id_reportes, DateTime fechaIni, DateTime fechaFin)
        {
            DateTime now = DateTime.Now;
            int contador = 1;
            this._path = path + "\\" + now.ToShortDateString() + "(" + contador + ")";
            this._id_reportes = id_reportes;
            this._fecha_ini = fechaIni;
            this._fecha_termino = fechaFin;
            while (Directory.Exists(this._path))
            {
                contador++;
                this._path = path + "\\" + now.ToShortDateString() + "(" + contador + ")";
            }
            Directory.CreateDirectory(this._path);
            this._crea_reporte = creaReporte;
            this._crea_grupo = creaGrupo;            
            this._stds = stds;  
        }

        #endregion

        #region INTERNAL METHODS

        /// <summary>
        /// Método para la generación de reportes en la modalidad de simulación multiescenario
        /// </summary>
        /// <param name="outputSimulacionMultiescenario">Diccionario que almecena la información a imprimir en reporte de puntualidad multiescenario general</param>
        /// <param name="outputSimulacionMultiescenarioNegocios">Diccionario que almecena la información a imprimir en reporte de puntualidad multiescenario por negocios</param>
        /// <param name="negocios">Lista con los nombres de los negocios definidos</param>
        /// <param name="enviarMsj">Delegado para mostrar mensajes en el Form de simulación multiescenario</param>
        internal void CreaReportesSimulacionMultiescenario(Dictionary<string, Dictionary<int, Dictionary<int, double>>> outputSimulacionMultiescenario, Dictionary<string, Dictionary<string, Dictionary<int, Dictionary<int, double>>>> outputSimulacionMultiescenarioNegocios, List<string> negocios, EnviarMensajeEventHandler enviarMsj)
        {
            if (_crea_reporte[NombreReporte.PuntualidadMultiescenarioGeneral])
            {
                enviarMsj("Construyendo reporte de puntualidad multiescenario general.");
                EscribirReportePuntualidadMultiescenario(outputSimulacionMultiescenario);
            }
            if (_crea_reporte[NombreReporte.PuntualidadMultiescenarioNegocio])
            {
                enviarMsj("Construyendo reporte de puntualidad multiescenario por negocio.");
                EscribirReportePuntualidadMultiescenarioNegocios(outputSimulacionMultiescenarioNegocios, negocios);
            }
        }

        /// <summary>
        /// Método para la generación de reportes en la modalidad de simulación normal
        /// </summary>
        /// <param name="informacionReplicas">Diccinario con los detalles de cada réplica</param>
        /// <param name="enviarMsj">Delegado para mostrar mensajes en el Form de simulación normal</param>
        internal void CreaReportesSimulacionNormal(List<Simulacion> informacionReplicas, EnviarMensajeEventHandler enviarMsj)
        {
            if (_crea_reporte[NombreReporte.PuntualidadGeneral])
            {
                enviarMsj("Construyendo reporte de puntualidad general.");
                EscribirReportePuntualidadGeneral(informacionReplicas);
            }
            if (_crea_reporte[NombreReporte.PuntualidadGrupos])
            {
                enviarMsj("Construyendo reporte de puntualidad por grupos.");
                EscribirReportePuntualidadGrupos(EstimarPuntualidadPorGrupo(informacionReplicas));
            }
            if (_crea_reporte[NombreReporte.UtilizacionBackup])
            {
                enviarMsj("Construyendo reporte de utilización de backups.");
                EscribirReporteUtilizacionBackups(AgruparBackupsPorReplica(informacionReplicas));
            }
            if (_crea_reporte[NombreReporte.Recovery])
            {
                enviarMsj("Construyendo reporte de recovery.");
                EscribirReporteRecovery(AgruparSwapsPorReplica(informacionReplicas));
            }
            if (_crea_reporte[NombreReporte.UtilizacionTurnos])
            {
                enviarMsj("Construyendo reporte de utilización de turnos de backup.");
                EscribirReporteUtilizacionTurnos(AgruparTurnosPorGrupo(informacionReplicas));
            }
            if (_crea_reporte[NombreReporte.PuntualidadMantto])
            {
                enviarMsj("Construyendo reporte de puntualidad de mantenimiento programado.");
                EscribirReporteMantto(AgruparManttosPorReplica(informacionReplicas));
            }
            if (_crea_reporte[NombreReporte.Detalles])
            {
                enviarMsj("Construyendo informe de detalles de la simulación.");
                EscribirInfoDetalle(informacionReplicas);
            }
            if (_crea_reporte[NombreReporte.ExplicacionImpuntualidadGeneral])
            {
                enviarMsj("Construyendo reporte de explicación de la impuntualidad general.");
                EscribirReporteExplicacionImpuntualidadGeneral(EstimarImpuntualidadReplica(informacionReplicas));
            }
            if (_crea_reporte[NombreReporte.ExplicacionImpuntualidadGrupos])
            {
                enviarMsj("Construyendo reporte de explicación de la impuntualidad por grupos.");
                EscribirReporteExplicacionImpuntualidadGrupos(EstimarImpuntualidadPorGrupo(informacionReplicas));
            }            
        }
        
        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Genera un diccionario indexado por las réplicas de la simulación que contiene una lista de los slots de backups.
        /// </summary>
        /// <param name="informacionReplicas">Detalles de cada réplica de la simulación</param>
        /// <returns></returns>
        private Dictionary<int, List<UnidadBackup>> AgruparBackupsPorReplica(List<Simulacion> informacionReplicas)
        {
            Dictionary<int, List<UnidadBackup>> backups = new Dictionary<int, List<UnidadBackup>>();
            int contador = 1;
            foreach (Simulacion s in informacionReplicas)
            {
                backups.Add(contador,new List<UnidadBackup>());
                foreach (UnidadBackup bu in s.Itinerario.ControladorBackups.BackupsLista)
                {
                    if (bu.TramoBase.Fecha_Salida >= _fecha_ini && bu.TramoBase.Fecha_Salida <= _fecha_termino)
                    {
                        backups[contador].Add(bu);
                    }
                }
                contador++;
            }
            return backups;
        }

        /// <summary>
        /// Genera un diccionario indexado por las réplicas de la simulación que contiene una lista de los slots de mantenimiento.
        /// </summary>
        /// <param name="informacionReplicas">Detalles de cada réplica de la simulación</param>
        /// <returns></returns>
        private Dictionary<int, List<SlotMantenimiento>> AgruparManttosPorReplica(List<Simulacion> informacionReplicas)
        {
            Dictionary<int, List<SlotMantenimiento>> retorno = new Dictionary<int, List<SlotMantenimiento>>();
            int contador = 1;
            foreach (Simulacion sim in informacionReplicas)
            {
                retorno.Add(contador, new List<SlotMantenimiento>());
                foreach (Avion a in sim.Itinerario.AvionesDictionary.Values)
                {
                    foreach (SlotMantenimiento sm in a.SlotsMantenimiento)
                    {
                        if (sm.TramoBase.Fecha_Salida >= _fecha_ini && sm.TramoBase.Fecha_Salida <= _fecha_termino)
                        {
                            retorno[contador].Add(sm);
                        }
                    }                    
                }
                contador++;
            }
            return retorno;
        }

        /// <summary>
        /// Genera un diccionario indexado por las réplicas de la simulación que contiene una lista de los swaps realizados.
        /// </summary>
        /// <param name="informacionReplicas">Detalles de cada réplica de la simulación</param>
        /// <returns></returns>
        private Dictionary<int, List<Swap>> AgruparSwapsPorReplica(List<Simulacion> informacionReplicas)
        {
            Dictionary<int, List<Swap>> swaps_replica = new Dictionary<int, List<Swap>>();
            int contador = 1;
            foreach (Simulacion s in informacionReplicas)
            {
                swaps_replica.Add(contador, s.Swaps);
                contador++;
            }
            return swaps_replica;
        }

        /// <summary>
        /// Genera la información necesaria para el reporte de utilizacion de turnos a partir de los  detalles de las réplicas de simulación.
        /// </summary>
        /// <param name="infoReplicas">Detalles de cada réplica de la simulación</param>
        /// <returns>Diccionario indexado por grupo de flota y con valores de la estructura de datos del reporte de utilización</returns>
        private Dictionary<string, InfoReporteTurnos> AgruparTurnosPorGrupo(List<Simulacion> infoReplicas)
        {
            Dictionary<string, InfoReporteTurnos> estadisticos_turnos = new Dictionary<string, InfoReporteTurnos>();
            Dictionary<int, Dictionary<string, ControladorTurnosBackup>> detalles_turnos = new Dictionary<int, Dictionary<string, ControladorTurnosBackup>>();
            Dictionary<string, double[]> capacidad = new Dictionary<string, double[]>();
            Dictionary<string, List<Dictionary<DateTime, int[]>>> data = new Dictionary<string, List<Dictionary<DateTime, int[]>>>();
            int contador = 1;
            foreach (Simulacion s in infoReplicas)
            {
                detalles_turnos.Add(contador, s.Itinerario.TurnosBackup);
                contador++;
            }
            foreach (int replica in detalles_turnos.Keys)
            {
                foreach (string id in detalles_turnos[replica].Keys)
                {
                    if (!capacidad.ContainsKey(id))
                    {
                        int turnos_manana = detalles_turnos[replica][id].TurnosMananaMax;
                        int turnos_tarde = detalles_turnos[replica][id].TurnosTardeMax;
                        if (turnos_manana > 0 || turnos_tarde > 0)
                        {
                            capacidad.Add(id, new double[] { turnos_manana, turnos_tarde });
                        }
                    }
                    if (capacidad.ContainsKey(id))
                    {
                        if (!data.ContainsKey(id))
                        {
                            data.Add(id, new List<Dictionary<DateTime, int[]>>());
                        }
                        Dictionary<DateTime, int[]> aux = new Dictionary<DateTime, int[]>();
                        foreach (DateTime dt in detalles_turnos[replica][id].TurnosManana.Keys)
                        {
                            if (dt >= _fecha_ini && dt <= _fecha_termino)
                            {
                                aux.Add(dt, new int[] { detalles_turnos[replica][id].TurnosManana[dt], detalles_turnos[replica][id].TurnosTarde[dt] });
                            }
                        }
                        data[id].Add(aux);
                    }
                }
            }

            foreach (string s in data.Keys)
            {
                InfoReporteTurnos info = new InfoReporteTurnos(s, data[s], capacidad[s]);
                estadisticos_turnos.Add(s, info);
            }
            return estadisticos_turnos;
        }

        /// <summary>
        /// Carga columnas de DataTable que contiene los detalles de la simulación para el reporte InfoDetalles.
        /// </summary>
        /// <param name="outputDetalle">DataTable con los detalles</param>
        /// <param name="stds">Lista de estándares de simulación cargados</param>
        private void CargarColumnasOutputDetalle(DataTable outputDetalle, List<int> stds)
        {
            outputDetalle.Columns.Clear();
            outputDetalle.Columns.Add(new DataColumn("Replica", typeof(string)));            
            outputDetalle.Columns.Add(new DataColumn("idTramo", typeof(string)));
            outputDetalle.Columns.Add(new DataColumn("Operador", typeof(string)));
            outputDetalle.Columns.Add(new DataColumn("Negocio", typeof(string)));
            outputDetalle.Columns.Add(new DataColumn("Num_vuelo", typeof(string)));
            outputDetalle.Columns.Add(new DataColumn("Flota Operada", typeof(string)));
            outputDetalle.Columns.Add(new DataColumn("AcType Operado", typeof(string)));
            outputDetalle.Columns.Add(new DataColumn("Avion Operado", typeof(string)));
            outputDetalle.Columns.Add(new DataColumn("Flota Programada", typeof(string)));
            outputDetalle.Columns.Add(new DataColumn("AcType Programada", typeof(string)));
            outputDetalle.Columns.Add(new DataColumn("Avion Programado", typeof(string)));
            outputDetalle.Columns.Add(new DataColumn("Origen", typeof(string)));
            outputDetalle.Columns.Add(new DataColumn("Destino", typeof(string)));
            outputDetalle.Columns.Add(new DataColumn("Tramo", typeof(string)));
            outputDetalle.Columns.Add(new DataColumn("Dia", typeof(int)));
            outputDetalle.Columns.Add(new DataColumn("Fecha_STD", typeof(DateTime)));
            outputDetalle.Columns.Add(new DataColumn("Fecha_ATD", typeof(DateTime)));
            outputDetalle.Columns.Add(new DataColumn("STD", typeof(string)));
            outputDetalle.Columns.Add(new DataColumn("ATD", typeof(string)));
            outputDetalle.Columns.Add(new DataColumn("DTD", typeof(int)));
            outputDetalle.Columns.Add(new DataColumn("Fecha_STA", typeof(DateTime)));
            outputDetalle.Columns.Add(new DataColumn("Fecha_ATA", typeof(DateTime)));
            outputDetalle.Columns.Add(new DataColumn("STA", typeof(string)));
            outputDetalle.Columns.Add(new DataColumn("ATA", typeof(string)));
            outputDetalle.Columns.Add(new DataColumn("DTA", typeof(int)));
            outputDetalle.Columns.Add(new DataColumn("Atraso", typeof(int)));
            outputDetalle.Columns.Add(new DataColumn("Causa de Atraso", typeof(string)));
            for (int i = 0; i < stds.Count; i++)
            {
                outputDetalle.Columns.Add(new DataColumn("CumpleSTD" + stds[i], typeof(int)));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grupo">Grupo de reporte objetivo</param>
        /// <param name="contadorReplicas">Cantidad de réplicas de la simulación</param>
        /// <param name="info_reporte">Estructura de datos que almacena la información a ser impresa en el reporte de explicación de la impuntualidad por grupos.</param>
        /// <param name="contadorImpuntualidad">Estructura de datos que guarda la información de impuntualidad por estándar para cada grupo</param>
        private void ConsolidaImpuntualidadGrupo(GruposReporte grupo, int contadorReplicas, ref Dictionary<GruposReporte, ExplicacionImpuntualidadAgrupada> info_reporte, ref Dictionary<GruposReporte, Dictionary<string, Dictionary<int, Dictionary<TipoDisrupcion, double[]>>>> contadorImpuntualidad)
        {
            if (_crea_grupo[grupo])
            {
                foreach (string id in contadorImpuntualidad[grupo].Keys)
                {
                    info_reporte[grupo].ImpuntualidadPorReplica[id].Add(contadorReplicas, new Dictionary<int, Dictionary<TipoDisrupcion, double>>());
                    foreach (int std in contadorImpuntualidad[grupo][id].Keys)
                    {
                        info_reporte[grupo].ImpuntualidadPorReplica[id][contadorReplicas].Add(std, new Dictionary<TipoDisrupcion, double>());
                        foreach (TipoDisrupcion tipo in contadorImpuntualidad[grupo][id][std].Keys)
                        {
                            double impuntualidad = Convert.ToDouble(contadorImpuntualidad[grupo][id][std][tipo][0] / contadorImpuntualidad[grupo][id][std][tipo][1]);
                            info_reporte[grupo].ImpuntualidadPorReplica[id][contadorReplicas][std].Add(tipo, impuntualidad);
                            info_reporte[grupo].AgregarValorTotalGrupo(id, contadorImpuntualidad[grupo][id][std][tipo][1]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Estima la impuntualidad que aporta un tramo a un determinado grupo
        /// </summary>
        /// <param name="tramo">Tramo objetivo</param>
        /// <param name="valor_grupo">Elemento de un grupo: por ejemplo "SCL"</param>
        /// <param name="grupo">Grupo objetivo: por ejemplo "Estación"</param>
        /// <param name="filtro">True si hay que filtrar el tramo</param>
        /// <param name="info_reporte">Estructura de datos que almacena la información a ser impresa en el reporte de explicación de la impuntualidad por grupos.</param>
        /// <param name="contadorImpuntualidad">Estructura de datos que guarda la información de impuntualidad por estándar para cada grupo</param>
        private void CuentaImpuntualidadEnGrupo(Tramo tramo, string valor_grupo, GruposReporte grupo, bool filtro, ref Dictionary<GruposReporte, ExplicacionImpuntualidadAgrupada> info_reporte, ref Dictionary<GruposReporte, Dictionary<string, Dictionary<int, Dictionary<TipoDisrupcion, double[]>>>> contadorImpuntualidad)
        {
            if (filtro)
            {
                if (!info_reporte[grupo].ImpuntualidadPorReplica.ContainsKey(valor_grupo))
                {
                    info_reporte[grupo].AgregarGrupo(valor_grupo);
                }
                if (!contadorImpuntualidad[grupo].ContainsKey(valor_grupo))
                {
                    contadorImpuntualidad[grupo].Add(valor_grupo, new Dictionary<int, Dictionary<TipoDisrupcion, double[]>>());
                }
                foreach (int std in _stds)
                {
                    if (!contadorImpuntualidad[grupo][valor_grupo].ContainsKey(std))
                    {
                        contadorImpuntualidad[grupo][valor_grupo].Add(std, new Dictionary<TipoDisrupcion, double[]>());
                        foreach (TipoDisrupcion tipo in Enum.GetValues(typeof(TipoDisrupcion)))
                        {
                            if (tipo != TipoDisrupcion.ADELANTO)
                            {
                                contadorImpuntualidad[grupo][valor_grupo][std].Add(tipo, new double[2]);
                            }
                        }
                    }
                    int atraso = tramo.TInicialRst - tramo.TInicialProg;
                    foreach (TipoDisrupcion tipo in Enum.GetValues(typeof(TipoDisrupcion)))
                    {
                        if (tipo != TipoDisrupcion.ADELANTO)
                        {
                            if (atraso > std && tramo.CausasAtraso.ContainsKey(tipo))
                            {
                                contadorImpuntualidad[grupo][valor_grupo][std][tipo][0] += tramo.CausasAtraso[tipo] / ((double)(atraso));
                            }
                            contadorImpuntualidad[grupo][valor_grupo][std][tipo][1]++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Método para estimar la impuntualidad por grupos
        /// </summary>
        /// <param name="informacionReplicas">Detalles de la simulación por réplica</param>
        private Dictionary<GruposReporte, ExplicacionImpuntualidadAgrupada> EstimarImpuntualidadPorGrupo(List<Simulacion> informacionReplicas)
        {
            int contadorReplicas = 0;
            Dictionary<GruposReporte, ExplicacionImpuntualidadAgrupada> retorno = new Dictionary<GruposReporte, ExplicacionImpuntualidadAgrupada>();
            foreach (GruposReporte grupo in Enum.GetValues(typeof(GruposReporte)))
            {
                if (_crea_grupo[grupo])
                {
                    retorno.Add(grupo, new ExplicacionImpuntualidadAgrupada());
                }
            }

            foreach (Simulacion simReplica in informacionReplicas)
            {
                contadorReplicas++;
                Dictionary<GruposReporte, Dictionary<string, Dictionary<int, Dictionary<TipoDisrupcion, double[]>>>> contadorImpuntualidad = new Dictionary<GruposReporte, Dictionary<string, Dictionary<int, Dictionary<TipoDisrupcion, double[]>>>>();
                foreach (GruposReporte grupo in Enum.GetValues(typeof(GruposReporte)))
                {
                    contadorImpuntualidad.Add(grupo, new Dictionary<string, Dictionary<int, Dictionary<TipoDisrupcion, double[]>>>());
                }

                //Cuenta tramos de impuntualidad para cada estándar y tipo de disrupción                
                foreach (Avion a in simReplica.Itinerario.AvionesDictionary.Values)
                {
                    List<Tramo> listaTramos = a.ObtenerListaTramos(a.Tramo_Raiz);
                    foreach (Tramo tramo in listaTramos)
                    {
                        if (tramo.TramoBase.Fecha_Salida >= _fecha_ini && tramo.TramoBase.Fecha_Salida <= _fecha_termino)
                        {
                            foreach (GruposReporte grupo in Enum.GetValues(typeof(GruposReporte)))
                            {
                                if (_crea_grupo[grupo])
                                {
                                    string id = GetIdGrupoReporte(tramo, grupo);
                                    CuentaImpuntualidadEnGrupo(tramo, id, grupo, GetFiltro(tramo, grupo), ref retorno, ref contadorImpuntualidad);
                                }
                            }
                        }
                    }
                }

                //Calculo de puntualidad
                foreach (GruposReporte grupo in Enum.GetValues(typeof(GruposReporte)))
                {
                    ConsolidaImpuntualidadGrupo(grupo, contadorReplicas, ref retorno, ref contadorImpuntualidad);
                }
            }

            foreach (GruposReporte key in retorno.Keys)
            {
                retorno[key].EstimarEstadisticosGrupo();
            }

            return retorno;
        }

        /// <summary>
        /// Escribe reporte de detalles de la simulación
        /// </summary>
        /// <param name="informacionReplicas">Detalles de la simulación por réplica</param>
        private void EscribirInfoDetalle(List<Simulacion> informacionReplicas)
        {
            int contReplicas = 0;
            FileStream fs = new FileStream(_path + "\\Detalles_Replicas_" + _id_reportes +".xls", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            string header = "Replica\tidTramo\tOperador\tNegocio\tNum_Vuelo\tFlota Operada\tAcTypeOperado\tAvion Operado\tFlota Programada\tAcTypeProgramado\tAvion Programado\tOrigen\tDestino\tTramo\t";
            header += "Dia\tFecha_Std\tFecha_Atd\tStd\tAtd\tDtd\tFecha_Sta\tFecha_Ata\tSta\tAta\tDta\tMinutosAtraso\tCausaAtraso";
            for (int i = 0; i < _stds.Count; i++)
            {
                header += "\tCumpleSTD" + _stds[i].ToString();
            }

            sw.WriteLine(header);
            DataTable dt = new DataTable("Informe_Detalles");
            CargarColumnasOutputDetalle(dt, _stds);


            foreach (Simulacion sim in informacionReplicas)
            {
                contReplicas++;
                foreach (Avion av in sim.Itinerario.AvionesDictionary.Values)
                {
                    Tramo tramo = av.Tramo_Raiz;
                    while (tramo != null)
                    {
                        List<string> stringTramo = tramo.DatosItinerarioToString(tramo, contReplicas, _stds);
                        foreach (string s in stringTramo)
                        {
                            sw.WriteLine(s);
                            string[] cells = s.Split('\t');
                            dt.Rows.Add(cells);
                        }
                        tramo = tramo.Tramo_Siguiente;
                    }
                }
            }
            sw.Close();
            fs.Close();
        }

        /// <summary>
        /// Escribe reporte de explicación de la impuntualidad por réplica
        /// </summary>
        /// <param name="info">Estructura de datos con la información para el reporte. Key 1: réplica. Key 2: std. Key 3: tipo de disrupcion. Valor: impuntualidad porcentual</param>
        private void EscribirReporteExplicacionImpuntualidadGeneral(Dictionary<int, Dictionary<int, Dictionary<TipoDisrupcion, double>>> info)
        {
            string[] sheets = new string[_stds.Count];            
            for (int i = 0; i < _stds.Count; i++)
            {
                sheets[i] = "STD" + _stds[i];
            }
            string[] headers = new string[Enum.GetValues(typeof(TipoDisrupcion)).Length];
            headers[0] = "Réplica";
            int contador = 1;
            foreach (TipoDisrupcion tipo in Enum.GetValues(typeof(TipoDisrupcion)))
            {
                if (tipo != TipoDisrupcion.ADELANTO)
                {
                    headers[contador] = tipo.ToString();
                    contador++;
                }
            }
            ReporteExplicacionImpuntualidadGeneral report = new ReporteExplicacionImpuntualidadGeneral(_path, "//Rep_Explicacion_Impunt_Global_" + _id_reportes, sheets, info, headers);
            report.InitializeWorkbook();
            report.CrearReporte("Explicación de Impuntualidad: ", true);
            report.AutoSizeColumns();
            report.CargarLogoEnSheets(true);
            report.WriteToFile();
        }

        /// <summary>
        ///  Escribe reporte de explicación de la impuntualidad consolidada por grupos
        /// </summary>
        /// <param name="impuntualidadGrupo">>Estructura de datos con la información para el reporte. Key: grupo. Valor: impuntualidad</param>
        private void EscribirReporteExplicacionImpuntualidadGrupos(Dictionary<GruposReporte, ExplicacionImpuntualidadAgrupada> impuntualidadGrupo)
        {
            //una hoja por grupo
            string[] sheets = new string[impuntualidadGrupo.Count * _stds.Count];

            int i = 0;
            foreach (GruposReporte key in impuntualidadGrupo.Keys)
            {
                foreach (int std in _stds)
                {
                    sheets[i] = key.ToString() + " - STD" + std;
                    i++;
                }
            }

            ReporteExplicacionImpuntualidadGrupos report = new ReporteExplicacionImpuntualidadGrupos(_path, "//Rep_Explicación_Impunt_Grupos_" + _id_reportes, sheets, impuntualidadGrupo, _stds);
            report.InitializeWorkbook();
            report.CrearReporte("Explicación de impuntualidad promedio por componentes: ", true);
            report.AutoSizeColumns();
            report.CargarLogoEnSheets(false);
            report.WriteToFile();
        }

        /// <summary>
        /// Escribe reporte de puntualidad de mantenimiento. 
        /// </summary>
        /// <param name="lista_slots_mantto">Lista con todos los slots de las réplicas</param>
        private void EscribirReporteMantto(Dictionary<int, List<SlotMantenimiento>> lista_slots_mantto)
        {
            string[] sheets = { "Puntualidad Mantto" };
            string[] headers = { "Número", "ID", "Estacion", "Subflota", "Matrícula", "Hora_ini", "Hora_fin", "Duración", "Puntualidad_Prom", "Puntualidad_Desv", "Minutos_Atraso_Prom", "Minutos_Atraso_Desv" };
            ReportePuntualidadMantto report = new ReportePuntualidadMantto(_path, "//Rep_Punt_Mantto_" + _id_reportes, sheets, lista_slots_mantto, headers);
            report.InitializeWorkbook();
            report.CrearReporte("Reporte Puntualidad de Mantenimiento (STD0)", false);
            report.AutoSizeColumns();
            report.CargarLogoEnSheets(true);
            report.WriteToFile();
        }

        /// <summary>
        /// Escribe reporte de puntualidad general por réplica.
        /// </summary>
        /// <param name="informacionReplicas">Detalles de la simulación por réplica.</param>
        private void EscribirReportePuntualidadGeneral(List<Simulacion> informacionReplicas)
        {
            List<string> negocios = informacionReplicas[0].Itinerario.Negocios;
            string[] sheets = new string[1 + negocios.Count];
            sheets[0] = "Puntualidad General";
            for(int i = 0;i<negocios.Count;i++)
            {
                sheets[i + 1] = "Puntualidad " + negocios[i];
            }
            string[] headers = new string[_stds.Count + 1];
            headers[0] = "Réplica";
            int contador = 1;
            foreach (int std in _stds)
            {
                headers[contador] = "STD" + std.ToString();
                contador++;
            }
            ReportePuntualidadGeneral report = new ReportePuntualidadGeneral(_path, "//Rep_Punt_Global_" + _id_reportes, sheets, informacionReplicas, headers, negocios);
            report.InitializeWorkbook();
            report.CrearReporte("", true);
            report.AutoSizeColumns();
            report.CargarLogoEnSheets(true);
            report.WriteToFile();
        }

        /// <summary>
        ///  Escribe reporte de puntualidad consolidada por grupos
        /// </summary>
        /// <param name="puntualidadGrupo">>Estructura de datos con la información para el reporte. Key: grupo. Valor: puntualidad</param>
        private void EscribirReportePuntualidadGrupos(Dictionary<GruposReporte, PuntualidadAgrupada> puntualidadGrupo)
        {
            //una hoja por grupo
            string[] sheets = new string[puntualidadGrupo.Count * _stds.Count];

            int i = 0;
            foreach (GruposReporte key in puntualidadGrupo.Keys)
            {
                foreach (int std in _stds)
                {
                    sheets[i] = key.ToString() + " - STD" + std;
                    i++;
                }
            }
            string[] headers = { "Num", "Grupo", "Legs", "Promedio", "Desvest", "Mínimo", "Máximo", "Intervalo_min", "Intervalo_max" };
            ReportePuntualidadGrupos report = new ReportePuntualidadGrupos(_path, "//Rep_Punt_Grupos_" + _id_reportes, sheets, puntualidadGrupo, _stds, headers);
            report.InitializeWorkbook();
            report.CrearReporte("Puntualidad por grupos: ", true);
            report.AutoSizeColumns();
            report.CargarLogoEnSheets(true);
            report.WriteToFile();
        }

        /// <summary>
        ///  Escribe reporte de puntualidad multiescenario general
        /// </summary>
        /// <param name="outputSimulacionMultiescenario">Estructura de datos con la información para el reporte. Key1: escenario. Key2: replica. Key 3: std. Valor: puntualidad</param>
        private void EscribirReportePuntualidadMultiescenario(Dictionary<string, Dictionary<int, Dictionary<int, double>>> outputSimulacionMultiescenario)
        {
            string[] sheets = new string[_stds.Count];
            for (int i = 0; i < _stds.Count; i++)
            {
                sheets[i] = "STD" + _stds[i];
            }
            string[] headers = { };
            ReportePuntualidadMultiescenario report = new ReportePuntualidadMultiescenario(_path, "//Rep_Punt_Multi_" + _id_reportes, sheets, _stds, outputSimulacionMultiescenario);
            report.InitializeWorkbook();
            report.CrearReporte("Puntualidad Multiescenario: ", true);
            report.AutoSizeColumns();
            report.CargarLogoEnSheets(true);
            report.WriteToFile();
        }

        /// <summary>
        ///  Escribe reporte de puntualidad multiescenario por negocio
        /// </summary>
        /// <param name="outputSimulacionMultiescenario">Estructura de datos con la información para el reporte. Key1: escenario. Key2: negocio. Key3: replica. Key 4: std. Valor: puntualidad</param>
        /// <param name="negocios">Lista con los negocios definidos</param>
        private void EscribirReportePuntualidadMultiescenarioNegocios(Dictionary<string, Dictionary<string, Dictionary<int, Dictionary<int, double>>>> outputSimulacionMultiescenario, List<string> negocios)
        {
            string[] sheets = new string[_stds.Count * negocios.Count];
            int contador = 0;
            foreach (string negocio in negocios)
            {
                foreach (int std in _stds)
                {
                    sheets[contador] = negocio + "-STD" + std;
                    contador++;
                }
            }
            string[] headers = { };
            ReportePuntualidadMultiescenarioNegocio report = new ReportePuntualidadMultiescenarioNegocio(_path, "//Rep_Punt_Multi_Negocios_" + _id_reportes, sheets, _stds, negocios, outputSimulacionMultiescenario);
            report.InitializeWorkbook();
            report.CrearReporte("Puntualidad Multiescenario: ", true);
            report.AutoSizeColumns();
            report.CargarLogoEnSheets(true);
            report.WriteToFile();
        }

        /// <summary>
        /// Escribe reporte de detalles de recovery
        /// </summary>
        /// <param name="lista_swaps_replica">Lista de swaps de todas las réplica</param>
        private void EscribirReporteRecovery(Dictionary<int, List<Swap>> lista_swaps_replica)
        {
            string[] sheets = { "Reporte Detalles Recovery" };
            string[] headers = { "Número", "Réplica", "FechaIni", "Id_Emisor", "Id_Receptor", "Flota_Em", "Flota_Rc", "SubFlota_Em", "SubFlot_Rc", "Punto Rotación", "Num_Legs_Emisor", "Num_Legs_Receptor", "RC_Ini", "Mnts_Atraso--", "Mnts_Atraso++", "Mnts_Ganancia_Neta", "Num_Legs_Beneficiadas", "Num_Legs_Perjudicadas", "Usa Backup" };
            ReporteRecovery report = new ReporteRecovery(_path, "//Rep_Recovery_" + _id_reportes, sheets, lista_swaps_replica, headers);
            report.InitializeWorkbook();
            report.CrearReporte("Reporte Detalles de Recovery", false);
            report.AutoSizeColumns();
            report.CargarLogoEnSheets(true);
            report.WriteToFile();
        }

        /// <summary>
        /// Escribe reporte de utilización de aviones de backup
        /// </summary>
        /// <param name="lista_backups_por_replica">Slots de backups por réplica</param>
        private void EscribirReporteUtilizacionBackups(Dictionary<int, List<UnidadBackup>> lista_backups_por_replica)
        {
            string[] sheets = { "Reporte Utilización" };
            string[] headers = { "Número", "ID", "Estacion", "Subflota", "Hora_ini", "Hora_fin", "Duración", "Utilización_Neta_Prom", "Utilización_Neta_Desv", "Minutos_Recuperados_Prom", "Minutos_Recuperados_Desv", "Tramos_Recuperados_Prom", "Tramos_Recuperados_Desv" };
            ReporteUtilizacionBackups report = new ReporteUtilizacionBackups(_path, "//Rep_Util_Backups_" + _id_reportes, sheets, lista_backups_por_replica, headers);
            report.InitializeWorkbook();
            report.CrearReporte("Reporte Utilización de Slots de Backup", false);
            report.AutoSizeColumns();
            report.CargarLogoEnSheets(true);
            report.WriteToFile();
        }

        /// <summary>
        /// Escribe reporte de utilización de turnos
        /// </summary>
        /// <param name="detalles_turnos">Estructura de datos con la información para el reporte. Key: grupo de flota. Valor: estadísticos de utilización de turnos</param>
        private void EscribirReporteUtilizacionTurnos(Dictionary<string, InfoReporteTurnos> detalles_turnos)
        {
            string[] sheets = new string[detalles_turnos.Count];
            int contador = 0;
            foreach (string grupo in detalles_turnos.Keys)
            {
                sheets[contador] = "Grupo " + grupo;
                contador++;
            }
            string[] headers = { "Fecha", "Turnos Mañana Prom", "Turnos Mañana %", "Turnos Tarde Prom", "Turnos Tarde %" };
            ReporteUtilizacionTurnos report = new ReporteUtilizacionTurnos(_path, "//Rep_Util_Turnos_" + _id_reportes, sheets, detalles_turnos, headers);
            report.InitializeWorkbook();
            report.CrearReporte("Reporte Utilización de Turnos de Backup: ", true);
            report.AutoSizeColumns();
            report.CargarLogoEnSheets(true);
            report.WriteToFile();
        }

        /// <summary>
        /// Estima impuntualidad por réplica.
        /// </summary>
        /// <param name="informacionReplicas">Información de la simulación por réplica</param>
        /// <returns>Diccionario con la impuntualidad. Key1: réplica. Key2: estándar. Key3: Tipo de disrupción. Valor: porcentaje de impuntualidad </returns>
        private Dictionary<int, Dictionary<int, Dictionary<TipoDisrupcion, double>>> EstimarImpuntualidadReplica(List<Simulacion> informacionReplicas)
        {
            Dictionary<int, Dictionary<int, Dictionary<TipoDisrupcion, double>>> impuntualidadReplica = new Dictionary<int, Dictionary<int, Dictionary<TipoDisrupcion, double>>>();

            int contador = 0;
            foreach (Simulacion sim in informacionReplicas)
            {
                impuntualidadReplica.Add(contador, sim.ImpuntualidadPorCausasDeAtraso());
                contador++;
            }

            return impuntualidadReplica;
        }

        /// <summary>
        /// Estima puntualidad por grupo.
        /// </summary>
        /// <param name="informacionReplicas">Detalles de la simulación por réplica.</param>
        /// <returns>Diccionario con la puntualidad por grupo.</returns>
        private Dictionary<GruposReporte, PuntualidadAgrupada> EstimarPuntualidadPorGrupo(List<Simulacion> informacionReplicas)
        {
            int contadorReplicas = 0;
            Dictionary<GruposReporte, PuntualidadAgrupada> retorno = new Dictionary<GruposReporte, PuntualidadAgrupada>();
            if (_crea_grupo[GruposReporte.Negocio])
                retorno.Add(GruposReporte.Negocio, new PuntualidadAgrupada());
            if (_crea_grupo[GruposReporte.Operador])
                retorno.Add(GruposReporte.Operador, new PuntualidadAgrupada());
            if (_crea_grupo[GruposReporte.Flota])
                retorno.Add(GruposReporte.Flota, new PuntualidadAgrupada());
            if (_crea_grupo[GruposReporte.Subflota])
                retorno.Add(GruposReporte.Subflota, new PuntualidadAgrupada());
            if (_crea_grupo[GruposReporte.Estacion])
                retorno.Add(GruposReporte.Estacion, new PuntualidadAgrupada());
            if (_crea_grupo[GruposReporte.Matricula])
                retorno.Add(GruposReporte.Matricula, new PuntualidadAgrupada());
            if (_crea_grupo[GruposReporte.ParOD])
                retorno.Add(GruposReporte.ParOD, new PuntualidadAgrupada());
            if (_crea_grupo[GruposReporte.Vuelo])
                retorno.Add(GruposReporte.Vuelo, new PuntualidadAgrupada());
            if (_crea_grupo[GruposReporte.HubSalida])
                retorno.Add(GruposReporte.HubSalida, new PuntualidadAgrupada());

            foreach (Simulacion simReplica in informacionReplicas)
            {
                contadorReplicas++;
                Dictionary<string, Dictionary<int, int[]>> contadorCumpleStdAeropuerto = new Dictionary<string, Dictionary<int, int[]>>();
                Dictionary<string, Dictionary<int, int[]>> contadorCumpleStdFlota = new Dictionary<string, Dictionary<int, int[]>>();
                Dictionary<string, Dictionary<int, int[]>> contadorCumpleStdMatricula = new Dictionary<string, Dictionary<int, int[]>>();
                Dictionary<string, Dictionary<int, int[]>> contadorCumpleStdAcType = new Dictionary<string, Dictionary<int, int[]>>();
                Dictionary<string, Dictionary<int, int[]>> contadorCumpleStdOperador = new Dictionary<string, Dictionary<int, int[]>>();
                Dictionary<string, Dictionary<int, int[]>> contadorCumpleStdParOD = new Dictionary<string, Dictionary<int, int[]>>();
                Dictionary<string, Dictionary<int, int[]>> contadorCumpleStdVuelo = new Dictionary<string, Dictionary<int, int[]>>();
                Dictionary<string, Dictionary<int, int[]>> contadorCumpleStdNegocio = new Dictionary<string, Dictionary<int, int[]>>();
                Dictionary<string, Dictionary<int, int[]>> contadorCumpleStdVuelosHUB = new Dictionary<string, Dictionary<int, int[]>>();
                #region Cuenta tramos para cada estándar
                foreach (Avion a in simReplica.Itinerario.AvionesDictionary.Values)
                {
                    List<Tramo> listaTramos = a.ObtenerListaTramos(a.Tramo_Raiz);
                    foreach (Tramo t in listaTramos)
                    {
                        if (t.TramoBase.Fecha_Salida >= _fecha_ini && t.TramoBase.Fecha_Salida <= _fecha_termino)
                        {
                            #region Negocio
                            if (_crea_grupo[GruposReporte.Negocio])
                            {
                                string id = t.Negocio;
                                if (!retorno[GruposReporte.Negocio].PuntualidadPorReplica.ContainsKey(id))
                                {
                                    retorno[GruposReporte.Negocio].AgregarGrupo(id);
                                }
                                if (!contadorCumpleStdNegocio.ContainsKey(id))
                                {
                                    contadorCumpleStdNegocio.Add(id, new Dictionary<int, int[]>());
                                }
                                foreach (int std in _stds)
                                {
                                    if (!contadorCumpleStdNegocio[id].ContainsKey(std))
                                    {
                                        contadorCumpleStdNegocio[id].Add(std, new int[2]);
                                    }
                                    if (t.TInicialRst - t.TInicialProg <= std)
                                    {
                                        contadorCumpleStdNegocio[id][std][0]++;
                                    }
                                    contadorCumpleStdNegocio[id][std][1]++;
                                }
                            }
                            #endregion

                            #region Aeropuerto
                            if (_crea_grupo[GruposReporte.Estacion])
                            {
                                string id_Aeropuerto = t.TramoBase.Origen;
                                if (!retorno[GruposReporte.Estacion].PuntualidadPorReplica.ContainsKey(id_Aeropuerto))
                                {
                                    retorno[GruposReporte.Estacion].AgregarGrupo(id_Aeropuerto);
                                }
                                if (!contadorCumpleStdAeropuerto.ContainsKey(id_Aeropuerto))
                                {
                                    contadorCumpleStdAeropuerto.Add(id_Aeropuerto, new Dictionary<int, int[]>());
                                }
                                foreach (int std in _stds)
                                {
                                    if (!contadorCumpleStdAeropuerto[id_Aeropuerto].ContainsKey(std))
                                    {
                                        contadorCumpleStdAeropuerto[id_Aeropuerto].Add(std, new int[2]);
                                    }
                                    if (t.TInicialRst - t.TInicialProg <= std)
                                    {
                                        contadorCumpleStdAeropuerto[id_Aeropuerto][std][0]++;
                                    }
                                    contadorCumpleStdAeropuerto[id_Aeropuerto][std][1]++;
                                }
                            }
                            #endregion

                            #region Flota
                            if (_crea_grupo[GruposReporte.Flota])
                            {
                                string id_Flota = t.FlotaOperada;
                                if (!retorno[GruposReporte.Flota].PuntualidadPorReplica.ContainsKey(id_Flota))
                                {
                                    retorno[GruposReporte.Flota].AgregarGrupo(id_Flota);
                                }
                                if (!contadorCumpleStdFlota.ContainsKey(id_Flota))
                                {
                                    contadorCumpleStdFlota.Add(id_Flota, new Dictionary<int, int[]>());
                                }
                                foreach (int std in _stds)
                                {
                                    if (!contadorCumpleStdFlota[id_Flota].ContainsKey(std))
                                    {
                                        contadorCumpleStdFlota[id_Flota].Add(std, new int[2]);
                                    }
                                    if (t.TInicialRst - t.TInicialProg <= std)
                                    {
                                        contadorCumpleStdFlota[id_Flota][std][0]++;
                                    }
                                    contadorCumpleStdFlota[id_Flota][std][1]++;
                                }
                            }
                            #endregion

                            #region Matricula

                            if (_crea_grupo[GruposReporte.Matricula])
                            {
                                string id_Matricula = t.IdAvionOperado;
                                if (!retorno[GruposReporte.Matricula].PuntualidadPorReplica.ContainsKey(id_Matricula))
                                {
                                    retorno[GruposReporte.Matricula].AgregarGrupo(id_Matricula);
                                }
                                if (!contadorCumpleStdMatricula.ContainsKey(id_Matricula))
                                {
                                    contadorCumpleStdMatricula.Add(id_Matricula, new Dictionary<int, int[]>());
                                }
                                foreach (int std in _stds)
                                {
                                    if (!contadorCumpleStdMatricula[id_Matricula].ContainsKey(std))
                                    {
                                        contadorCumpleStdMatricula[id_Matricula].Add(std, new int[2]);
                                    }
                                    if (t.TInicialRst - t.TInicialProg <= std)
                                    {
                                        contadorCumpleStdMatricula[id_Matricula][std][0]++;
                                    }
                                    contadorCumpleStdMatricula[id_Matricula][std][1]++;
                                }
                            }

                            #endregion

                            #region SubFlotas

                            if (_crea_grupo[GruposReporte.Subflota])
                            {
                                string id_subFlota = t.GetAvion(t.IdAvionOperado).SubFlota;
                                if (!retorno[GruposReporte.Subflota].PuntualidadPorReplica.ContainsKey(id_subFlota))
                                {
                                    retorno[GruposReporte.Subflota].AgregarGrupo(id_subFlota);
                                }
                                if (!contadorCumpleStdAcType.ContainsKey(id_subFlota))
                                {
                                    contadorCumpleStdAcType.Add(id_subFlota, new Dictionary<int, int[]>());
                                }
                                foreach (int std in _stds)
                                {
                                    if (!contadorCumpleStdAcType[id_subFlota].ContainsKey(std))
                                    {
                                        contadorCumpleStdAcType[id_subFlota].Add(std, new int[2]);
                                    }
                                    if (t.TInicialRst - t.TInicialProg <= std)
                                    {
                                        contadorCumpleStdAcType[id_subFlota][std][0]++;
                                    }
                                    contadorCumpleStdAcType[id_subFlota][std][1]++;
                                }
                            }

                            #endregion

                            #region Operador

                            if (_crea_grupo[GruposReporte.Operador])
                            {
                                string id_Operador = t.TramoBase.Ac_Owner;
                                if (!retorno[GruposReporte.Operador].PuntualidadPorReplica.ContainsKey(id_Operador))
                                {
                                    retorno[GruposReporte.Operador].AgregarGrupo(id_Operador);
                                }
                                if (!contadorCumpleStdOperador.ContainsKey(id_Operador))
                                {
                                    contadorCumpleStdOperador.Add(id_Operador, new Dictionary<int, int[]>());
                                }
                                foreach (int std in _stds)
                                {
                                    if (!contadorCumpleStdOperador[id_Operador].ContainsKey(std))
                                    {
                                        contadorCumpleStdOperador[id_Operador].Add(std, new int[2]);
                                    }
                                    if (t.TInicialRst - t.TInicialProg <= std)
                                    {
                                        contadorCumpleStdOperador[id_Operador][std][0]++;
                                    }
                                    contadorCumpleStdOperador[id_Operador][std][1]++;
                                }
                            }
                            #endregion

                            #region Par OD
                            if (_crea_grupo[GruposReporte.ParOD])
                            {
                                string id_parOD = t.ParOD;
                                if (!retorno[GruposReporte.ParOD].PuntualidadPorReplica.ContainsKey(id_parOD))
                                {
                                    retorno[GruposReporte.ParOD].AgregarGrupo(id_parOD);
                                }
                                if (!contadorCumpleStdParOD.ContainsKey(id_parOD))
                                {
                                    contadorCumpleStdParOD.Add(id_parOD, new Dictionary<int, int[]>());
                                }
                                foreach (int std in _stds)
                                {
                                    if (!contadorCumpleStdParOD[id_parOD].ContainsKey(std))
                                    {
                                        contadorCumpleStdParOD[id_parOD].Add(std, new int[2]);
                                    }
                                    if (t.TInicialRst - t.TInicialProg <= std)
                                    {
                                        contadorCumpleStdParOD[id_parOD][std][0]++;
                                    }
                                    contadorCumpleStdParOD[id_parOD][std][1]++;
                                }
                            }
                            #endregion

                            #region Vuelo
                            if (_crea_grupo[GruposReporte.Vuelo])
                            {
                                string id_vuelo = t.IdVueloReporte;

                                if (!retorno[GruposReporte.Vuelo].PuntualidadPorReplica.ContainsKey(id_vuelo))
                                {
                                    retorno[GruposReporte.Vuelo].AgregarGrupo(id_vuelo);
                                }
                                if (!contadorCumpleStdVuelo.ContainsKey(id_vuelo))
                                {
                                    contadorCumpleStdVuelo.Add(id_vuelo, new Dictionary<int, int[]>());
                                }
                                foreach (int std in _stds)
                                {
                                    if (!contadorCumpleStdVuelo[id_vuelo].ContainsKey(std))
                                    {
                                        contadorCumpleStdVuelo[id_vuelo].Add(std, new int[2]);
                                    }
                                    if (t.TInicialRst - t.TInicialProg <= std)
                                    {
                                        contadorCumpleStdVuelo[id_vuelo][std][0]++;
                                    }
                                    contadorCumpleStdVuelo[id_vuelo][std][1]++;
                                }
                            }
                            #endregion

                            #region Vuelos HUB
                            if (_crea_grupo[GruposReporte.HubSalida])
                            {
                                if (t.VueloHUB)
                                {
                                    string id = t.KeyHUB;

                                    if (!retorno[GruposReporte.HubSalida].PuntualidadPorReplica.ContainsKey(id))
                                    {
                                        retorno[GruposReporte.HubSalida].AgregarGrupo(id);
                                    }
                                    if (!contadorCumpleStdVuelosHUB.ContainsKey(id))
                                    {
                                        contadorCumpleStdVuelosHUB.Add(id, new Dictionary<int, int[]>());
                                    }
                                    foreach (int std in _stds)
                                    {
                                        if (!contadorCumpleStdVuelosHUB[id].ContainsKey(std))
                                        {
                                            contadorCumpleStdVuelosHUB[id].Add(std, new int[2]);
                                        }
                                        if (t.TInicialRst - t.TInicialProg <= std)
                                        {
                                            contadorCumpleStdVuelosHUB[id][std][0]++;
                                        }
                                        contadorCumpleStdVuelosHUB[id][std][1]++;
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                }
                #endregion

                #region Calculo de puntualidad

                //Negocio
                if (_crea_grupo[GruposReporte.Negocio])
                {
                    foreach (string id in contadorCumpleStdNegocio.Keys)
                    {
                        retorno[GruposReporte.Negocio].PuntualidadPorReplica[id].Add(contadorReplicas, new Dictionary<int, double>());
                        foreach (int std in contadorCumpleStdNegocio[id].Keys)
                        {
                            double numerador = contadorCumpleStdNegocio[id][std][0];
                            double denominador = contadorCumpleStdNegocio[id][std][1];
                            double puntualidad = Convert.ToDouble(numerador / denominador);
                            retorno[GruposReporte.Negocio].PuntualidadPorReplica[id][contadorReplicas].Add(std, puntualidad);
                            retorno[GruposReporte.Negocio].AgregarValorTotalGrupo(id, denominador);
                        }
                    }
                }

                //Aeropuerto
                if (_crea_grupo[GruposReporte.Estacion])
                {
                    foreach (string id in contadorCumpleStdAeropuerto.Keys)
                    {
                        retorno[GruposReporte.Estacion].PuntualidadPorReplica[id].Add(contadorReplicas, new Dictionary<int, double>());
                        foreach (int std in contadorCumpleStdAeropuerto[id].Keys)
                        {
                            double numerador = contadorCumpleStdAeropuerto[id][std][0];
                            double denominador = contadorCumpleStdAeropuerto[id][std][1];
                            double puntualidad = Convert.ToDouble(numerador / denominador);
                            retorno[GruposReporte.Estacion].PuntualidadPorReplica[id][contadorReplicas].Add(std, puntualidad);
                            retorno[GruposReporte.Estacion].AgregarValorTotalGrupo(id, denominador);
                        }
                    }
                }

                //Matricula
                if (_crea_grupo[GruposReporte.Matricula])
                {
                    foreach (string id in contadorCumpleStdMatricula.Keys)
                    {
                        retorno[GruposReporte.Matricula].PuntualidadPorReplica[id].Add(contadorReplicas, new Dictionary<int, double>());
                        foreach (int std in contadorCumpleStdMatricula[id].Keys)
                        {
                            double numerador = contadorCumpleStdMatricula[id][std][0];
                            double denominador = contadorCumpleStdMatricula[id][std][1];
                            double puntualidad = Convert.ToDouble(numerador / denominador);
                            retorno[GruposReporte.Matricula].PuntualidadPorReplica[id][contadorReplicas].Add(std, puntualidad);
                            retorno[GruposReporte.Matricula].AgregarValorTotalGrupo(id, denominador);
                        }
                    }
                }

                //Ac Type
                if (_crea_grupo[GruposReporte.Subflota])
                {
                    foreach (string id in contadorCumpleStdAcType.Keys)
                    {
                        retorno[GruposReporte.Subflota].PuntualidadPorReplica[id].Add(contadorReplicas, new Dictionary<int, double>());
                        foreach (int std in contadorCumpleStdAcType[id].Keys)
                        {
                            double numerador = contadorCumpleStdAcType[id][std][0];
                            double denominador = contadorCumpleStdAcType[id][std][1];
                            double puntualidad = Convert.ToDouble(numerador / denominador);
                            retorno[GruposReporte.Subflota].PuntualidadPorReplica[id][contadorReplicas].Add(std, puntualidad);
                            retorno[GruposReporte.Subflota].AgregarValorTotalGrupo(id, denominador);
                        }
                    }
                }

                //Flota
                if (_crea_grupo[GruposReporte.Flota])
                {
                    foreach (string id in contadorCumpleStdFlota.Keys)
                    {
                        retorno[GruposReporte.Flota].PuntualidadPorReplica[id].Add(contadorReplicas, new Dictionary<int, double>());
                        foreach (int std in contadorCumpleStdFlota[id].Keys)
                        {
                            double numerador = contadorCumpleStdFlota[id][std][0];
                            double denominador = contadorCumpleStdFlota[id][std][1];
                            double puntualidad = Convert.ToDouble(numerador / denominador);
                            retorno[GruposReporte.Flota].PuntualidadPorReplica[id][contadorReplicas].Add(std, puntualidad);
                            retorno[GruposReporte.Flota].AgregarValorTotalGrupo(id, denominador);
                        }
                    }
                }

                //Operador
                if (_crea_grupo[GruposReporte.Operador])
                {
                    foreach (string id in contadorCumpleStdOperador.Keys)
                    {
                        retorno[GruposReporte.Operador].PuntualidadPorReplica[id].Add(contadorReplicas, new Dictionary<int, double>());
                        foreach (int std in contadorCumpleStdOperador[id].Keys)
                        {
                            double numerador = contadorCumpleStdOperador[id][std][0];
                            double denominador = contadorCumpleStdOperador[id][std][1];
                            double puntualidad = Convert.ToDouble(numerador / denominador);
                            retorno[GruposReporte.Operador].PuntualidadPorReplica[id][contadorReplicas].Add(std, puntualidad);
                            retorno[GruposReporte.Operador].AgregarValorTotalGrupo(id, denominador);
                        }
                    }
                }

                //Tramo OD
                if (_crea_grupo[GruposReporte.ParOD])
                {
                    foreach (string id in contadorCumpleStdParOD.Keys)
                    {
                        retorno[GruposReporte.ParOD].PuntualidadPorReplica[id].Add(contadorReplicas, new Dictionary<int, double>());
                        foreach (int std in contadorCumpleStdParOD[id].Keys)
                        {
                            double numerador = contadorCumpleStdParOD[id][std][0];
                            double denominador = contadorCumpleStdParOD[id][std][1];
                            double puntualidad = Convert.ToDouble(numerador / denominador);
                            retorno[GruposReporte.ParOD].PuntualidadPorReplica[id][contadorReplicas].Add(std, puntualidad);
                            retorno[GruposReporte.ParOD].AgregarValorTotalGrupo(id, denominador);
                        }
                    }
                }

                //Vuelo
                if (_crea_grupo[GruposReporte.Vuelo])
                {
                    foreach (string id in contadorCumpleStdVuelo.Keys)
                    {
                        retorno[GruposReporte.Vuelo].PuntualidadPorReplica[id].Add(contadorReplicas, new Dictionary<int, double>());
                        foreach (int std in contadorCumpleStdVuelo[id].Keys)
                        {
                            double numerador = contadorCumpleStdVuelo[id][std][0];
                            double denominador = contadorCumpleStdVuelo[id][std][1];
                            double puntualidad = Convert.ToDouble(numerador / denominador);
                            retorno[GruposReporte.Vuelo].PuntualidadPorReplica[id][contadorReplicas].Add(std, puntualidad);
                            retorno[GruposReporte.Vuelo].AgregarValorTotalGrupo(id, denominador);
                        }
                    }
                }

                //Vuelos HUB
                if (_crea_grupo[GruposReporte.HubSalida])
                {
                    foreach (string id in contadorCumpleStdVuelosHUB.Keys)
                    {
                        retorno[GruposReporte.HubSalida].PuntualidadPorReplica[id].Add(contadorReplicas, new Dictionary<int, double>());
                        foreach (int std in contadorCumpleStdVuelosHUB[id].Keys)
                        {
                            double numerador = contadorCumpleStdVuelosHUB[id][std][0];
                            double denominador = contadorCumpleStdVuelosHUB[id][std][1];
                            double puntualidad = Convert.ToDouble(numerador / denominador);
                            retorno[GruposReporte.HubSalida].PuntualidadPorReplica[id][contadorReplicas].Add(std, puntualidad);
                            retorno[GruposReporte.HubSalida].AgregarValorTotalGrupo(id, denominador);
                        }
                    }
                }
                #endregion
            }

            foreach (GruposReporte key in retorno.Keys)
            {
                retorno[key].EstimarEstadisticosGrupo();
            }

            return retorno;
        }        
        
        /// <summary>
        /// Indica si un tramo es válido en los cálculos de la impuntualidad por grupo.
        /// </summary>
        /// <param name="tramo">Tramo objetivo</param>
        /// <param name="grupo">Grupo objetivo</param>
        /// <returns>True si el tramo es válido. Es decir, se incluye en los cálculos.</returns>
        private bool GetFiltro(Tramo tramo, GruposReporte grupo)
        {
            switch (grupo)
            {
                case (GruposReporte.Negocio):
                    {
                        return tramo.Negocio != "SIN";
                    }
                case (GruposReporte.Estacion):
                    {
                        return true;
                    }
                case (GruposReporte.Flota):
                    {
                        return true;
                    }
                case (GruposReporte.Matricula):
                    {
                        return true;
                    }
                case (GruposReporte.Subflota):
                    {
                        return true;
                    }
                case (GruposReporte.Operador):
                    {
                        return true;
                    }
                case (GruposReporte.ParOD):
                    {
                        return true;
                    }
                case (GruposReporte.Vuelo):
                    {
                        return true;
                    }
                case (GruposReporte.HubSalida):
                    {
                        return tramo.VueloHUB;
                    }
                default: return false;
            }
        }

        /// <summary>
        /// Retorna el id de un tramo según sea el grupo de reporte. Por ejemplo: para el grupo "Estación" 
        /// se retorna el aeropuerto de origen del tramo.
        /// </summary>
        /// <param name="tramo">Tramo objetivo</param>
        /// <param name="grupo">Grupo objetivo</param>
        private string GetIdGrupoReporte(Tramo tramo, GruposReporte grupo)
        {
            switch (grupo)
            {
                case (GruposReporte.Negocio):
                    {
                        return tramo.Negocio;
                    }
                case (GruposReporte.Estacion):
                    {
                        return tramo.TramoBase.Origen;
                    }
                case (GruposReporte.Flota):
                    {
                        return tramo.FlotaOperada;
                    }
                case (GruposReporte.Matricula):
                    {
                        return tramo.IdAvionOperado;
                    }
                case (GruposReporte.Subflota):
                    {
                        return tramo.GetAvion(tramo.IdAvionOperado).SubFlota;
                    }
                case (GruposReporte.Operador):
                    {
                        return tramo.TramoBase.Ac_Owner;
                    }
                case (GruposReporte.ParOD):
                    {
                        return tramo.ParOD;
                    }
                case (GruposReporte.Vuelo):
                    {
                        return tramo.IdVueloReporte;
                    }
                case (GruposReporte.HubSalida):
                    {
                        return tramo.KeyHUB;
                    }
                default: return null;
            }
        }

        #endregion        
    }
}
