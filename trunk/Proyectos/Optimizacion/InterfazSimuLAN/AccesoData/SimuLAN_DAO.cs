using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using SimuLAN.Clases;
using System.IO;
using SimuLAN.Clases.Disrupciones;
using InterfazSimuLAN.Utils;
using SimuLAN.Clases.Recovery;

namespace InterfazSimuLAN.AccesoData
{
    /// <summary>
    /// Interfaz para la carga de datos
    /// </summary>
    public class SimuLAN_DAO
    {
        /// <summary>
        /// Enumeración para distinguir el formato de fecha del archivo de itinerario
        /// </summary>
        private enum TipoFecha { Texto, Serial };

        /// <summary>
        /// Flag para saber si se está leyendo la primera fila del itinerario.
        /// </summary>
        private static bool _primera_fila = true;

        /// <summary>
        /// Variable global que indica el tipo de fecha que tiene el itinerario actual en proceso.
        /// </summary>
        private static TipoFecha _tipo_fecha = TipoFecha.Texto;

        #region CURVAS

        /// <summary>
        /// Método para cargar curvas de WXS históricas desde Excel
        /// </summary>
        /// <param name="modelo">Modelo de disrupciones en construcción</param>
        /// <param name="filename">Ruta del archivo con la información</param>
        /// <param name="exitoso">True si el proceso se completó correctamente</param>
        internal static void CargarCurvasWXS(ModeloDisrupciones modelo, string filename, out bool exitoso, out string msg)
        {
            try
            {
                FileStream fs = new FileStream(filename, FileMode.Open);
                ExcelDataReader edr = new ExcelDataReader(fs);
                DataSet ds = edr.WorkbookData;
                fs.Close();
                modelo.DataSetToCurvasWxs(ds, out exitoso, true);
                msg = "";
            }
            catch (IOException e)
            {
                exitoso = false;
                msg = e.Message;
            }
            catch
            {
                exitoso = false;
                msg = "Error al intentar cargar archivo de curvas.";
            }
        }

        /// <summary>
        /// Carga un DataSet con la información de disrupciones.
        /// </summary>
        /// <param name="ds">DataSet objetivo</param>
        /// <param name="exitoso">True si el casteo es exitoso</param>
        /// <param name="omitirPrimeraFila">Indica si se omite la primera fila de las tablas del DataSet</param>
        /// <param name="cantidadCargadas">Retorna cantidad de disrupciones cargadas</param>
        internal static void DataSetToModeloDisrupciones(ModeloDisrupciones modelo, DataSet ds, out bool exitoso, bool omitirPrimeraFila, out int cantidadCargadas)
        {
            exitoso = false;
            cantidadCargadas = 0;
            foreach (DataTable dt in ds.Tables)
            {
                if (!modelo.ColeccionDisrupciones.ContainsKey(dt.TableName))
                {
                    foreach (TipoDisrupcion tipo in Enum.GetValues(typeof(TipoDisrupcion)))
                    {
                        if (tipo.ToString() == dt.TableName)
                        {                            
                            modelo.ColeccionDisrupciones.Add(tipo.ToString(), new InfoDisrupcion());
                            modelo.ColeccionDisrupciones[tipo.ToString()] = modelo.ColeccionDisrupciones[tipo.ToString()].InfoDisrupcionFromDateTable(dt, omitirPrimeraFila);
                            cantidadCargadas++;
                        }
                    }
                }
                else
                {
                    foreach(TipoDisrupcion tipo in Enum.GetValues(typeof(TipoDisrupcion)))
                    {
                        if (tipo.ToString() == dt.TableName)
                        {
                            modelo.ColeccionDisrupciones[tipo.ToString()] = modelo.ColeccionDisrupciones[tipo.ToString()].InfoDisrupcionFromDateTable(dt, omitirPrimeraFila);
                            cantidadCargadas++;
                        }
                    }
                }
            }
            exitoso = true;
        }

        #endregion

        #region ITINERARIO

        /// <summary>
        /// Procesa una fila del archivo de itinerario
        /// </summary>
        /// <param name="items">Arreglo con la información</param>
        /// <param name="camposIndices">Diccionario con el número de columna para cada campo leido del itinerario</param>
        /// <param name="itinerario">Itinerario en construcción</param>
        /// <param name="fuente">Tipo de origen del itinerario</param>
        /// <param name="parametros">Objeto de parámetros de la simulación</param>
        private static void AgregarFila(object[] items, Dictionary<CamposArchivoItinerario, int> camposIndices, Itinerario itinerario, OrigenItinerario fuente, ParametrosSimuLAN parametros)
        {
            try
            {
                //Carga objeto buffer con la información de la fila.
                DataObjetoTramoXLS dataObjeto = new DataObjetoTramoXLS(items, camposIndices);
                
                #region Carga aeropuertos y aviones
                
                //Actualiza aeropuertos
                if (!itinerario.AeropuertosDictionary.ContainsKey(dataObjeto.origen))
                {
                    Aeropuerto a = new Aeropuerto(dataObjeto.origen, 0);
                    itinerario.AgregarAeropuerto(a, dataObjeto.origen);
                }                
                if (!itinerario.AeropuertosDictionary.ContainsKey(dataObjeto.destino))
                {
                    Aeropuerto a = new Aeropuerto(dataObjeto.destino, 0);
                    itinerario.AgregarAeropuerto(a, dataObjeto.destino);
                }

                //Carga aviones
                if (!itinerario.AvionesDictionary.ContainsKey(dataObjeto.idAvion))
                {
                    string subFlota = dataObjeto.operador + " " + dataObjeto.acType;
                    //Cargar AcTypes
                    if (!itinerario.AcTypeDictionary.ContainsKey(dataObjeto.acType))
                    {
                        itinerario.AcTypeDictionary.Add(dataObjeto.acType, new AcType(dataObjeto.acType));
                    }
                    Avion a = new Avion(dataObjeto.acType, subFlota, dataObjeto.idAvion);
                    itinerario.AgregarAvion(dataObjeto.idAvion, a);
                }

                #endregion

                #region Carga tramos/mantto/backups

                //procesa las fechas
                itinerario.ContadorTramos++;
                DateTime fecha_ini_base = new DateTime();
                DateTime fecha_fin_base = new DateTime();
                if (_primera_fila)
                {
                    _primera_fila = false;
                    try
                    {
                        fecha_ini_base = Itinerario.FechaDesdeNumeroSerieExcel(dataObjeto.fechaInicio);
                        fecha_fin_base = Itinerario.FechaDesdeNumeroSerieExcel(dataObjeto.fechaTermino);
                        _tipo_fecha = TipoFecha.Serial;

                    }
                    catch
                    {
                        fecha_ini_base = Convert.ToDateTime(dataObjeto.fechaInicio);
                        fecha_fin_base = Convert.ToDateTime(dataObjeto.fechaTermino);
                        _tipo_fecha = TipoFecha.Texto;
                    }
                }
                else
                {
                    if (_tipo_fecha == TipoFecha.Serial)
                    {
                        fecha_ini_base = Itinerario.FechaDesdeNumeroSerieExcel(dataObjeto.fechaInicio);
                        fecha_fin_base = Itinerario.FechaDesdeNumeroSerieExcel(dataObjeto.fechaTermino);
                    }
                    else
                    {
                        fecha_ini_base = Convert.ToDateTime(dataObjeto.fechaInicio);
                        fecha_fin_base = Convert.ToDateTime(dataObjeto.fechaTermino);
                    }
                }

                //Carga tramo base
                TramoBase tramoBase = new TramoBase(itinerario.ContadorTramos, dataObjeto.idAvion, dataObjeto.idTramoAvion, dataObjeto.numSubFlota, dataObjeto.acType, dataObjeto.operador,
                    dataObjeto.carrier, dataObjeto.idTramoGlobal, dataObjeto.op_suf, dataObjeto.stc, dataObjeto.config_asientos, dataObjeto.origen,
                    fecha_ini_base, dataObjeto.STD.ToString(), dataObjeto.domInt, dataObjeto.destino, fecha_fin_base, dataObjeto.STA.ToString());

                //Agrega Backup
                if (tramoBase.Tipo == TipoTramoBase.Backup)
                {
                    UnidadBackup bu = new UnidadBackup(tramoBase, itinerario.FechaInicio);
                    itinerario.ControladorBackups.BackupsLista.Add(bu);
                }

                //Agrega Mantto
                else if (tramoBase.Tipo == TipoTramoBase.Mantto)
                {
                    Avion avion_actual = itinerario.AvionesDictionary[dataObjeto.idAvion];
                    SlotMantenimiento slotMantto = new SlotMantenimiento(tramoBase, itinerario.AcTypeDictionary[tramoBase.AcType].Flota, avion_actual.Ultimo_Tramo_Agregado, itinerario.FechaInicio);
                    avion_actual.SlotsMantenimiento.Add(slotMantto);                    
                    //Caso en que hay un mantto programado al inicio del itinerario
                    if (avion_actual.Ultimo_Tramo_Agregado == null)
                    {
                        avion_actual.PrimerSlotEsMantenimiento = true;
                    }
                    else
                    {
                        slotMantto.TramoPrevio.MantenimientoPosterior = slotMantto;
                    }
                }

                //Agrega Tramo
                else
                {
                    Tramo tramo = new Tramo(tramoBase, itinerario.AvionesDictionary[dataObjeto.idAvion].UltimoTramoAgregado, itinerario.FechaInicio);
                    tramo.GetAvion = itinerario.GetAvion;
                    tramo.GetTurnAroundMinimo = parametros.GetDelegateTurnAroundMin();
                    itinerario.AvionesDictionary[dataObjeto.idAvion].AgregarTramoEnOrden(tramo);
                    itinerario.Tramos.Add(itinerario.ContadorTramos, tramo);
                }

            #endregion

            }
            catch (Exception)
            {
                throw new Exception("Problemas al cargar itinerario de Excel");
            }
        }

        /// <summary>
        /// Carga los campos de cada columna del archivo de itinerario basado en el reporte de Schedule Manager
        /// </summary>
        /// <returns></returns>
        private static Dictionary<CamposArchivoItinerario, int> CargarCamposArchivoItinerario()
        {
            Dictionary<CamposArchivoItinerario, int> indices = new Dictionary<CamposArchivoItinerario, int>();
            indices.Add(CamposArchivoItinerario.Id_Avion, 0);
            indices.Add(CamposArchivoItinerario.Id_Tramo_AC, 1);
            indices.Add(CamposArchivoItinerario.NumSubflota, 2);
            indices.Add(CamposArchivoItinerario.AC_Type, 3);
            indices.Add(CamposArchivoItinerario.Operador, 4);
            indices.Add(CamposArchivoItinerario.Carrier, 5);
            indices.Add(CamposArchivoItinerario.Leg_ID_Global, 6);
            indices.Add(CamposArchivoItinerario.Op_Suf, 7);
            indices.Add(CamposArchivoItinerario.STC, 9);
            indices.Add(CamposArchivoItinerario.Config_Asientos, 10);
            indices.Add(CamposArchivoItinerario.Origen, 11);
            indices.Add(CamposArchivoItinerario.Fecha_Ini, 13);
            indices.Add(CamposArchivoItinerario.Fecha_X, 14);
            indices.Add(CamposArchivoItinerario.STD, 15);
            indices.Add(CamposArchivoItinerario.Dom_Int, 16);
            indices.Add(CamposArchivoItinerario.Destino, 17);
            indices.Add(CamposArchivoItinerario.Fecha_Fin, 19);
            indices.Add(CamposArchivoItinerario.STA, 20);
            return indices;
        }

        /// <summary>
        /// Constrye un itinerario
        /// </summary>
        /// <param name="filename">Nombre del itinerario</param>
        /// <param name="extension">Extensión del archivo de origen del itinerario</param>
        /// <param name="parametros">Objeto de parámetros de la simulación</param>
        /// <param name="fs">Stream con referencia al archivo de origen del itinerario</param>
        /// <param name="OK_Itinerario">True si la carga finalizó correctamente</param>
        /// <returns>Itinerario</returns>
        public static Itinerario CargarItinerario(string filename, string extension, ParametrosSimuLAN parametros, Stream fs, ref bool OK_Itinerario)
        {
            //Inicializa objetos
            DataTable datosItinerarioProgramado = new DataTable();
            Itinerario itinerario = new Itinerario(filename);
            _primera_fila = true;
            //Si ya se cargaron los parámetros consolida los AcTypes en el itinerario
            if (parametros != null && parametros.MapFlotas != null && parametros.MapFlotas.Dict != null && parametros.MapFlotas.Dict.Count > 0)
            {
                itinerario.CargarFlotasEnAcTypes(parametros.MapFlotas.Dict);
            }

            //Para archivos de MS Excel
            if (extension == "xls" || extension == "XLS")
            {
                //Se obtiene el libro
                Utils.ExcelDataReader spreadsheet = new Utils.ExcelDataReader(fs);
                fs.Close();
                //Se obtiene la primera hoja con el itinerario
                datosItinerarioProgramado = spreadsheet.WorkbookData.Tables[0];
                //Se carga el itinerario
                CrearItinerarioDeDataTable(datosItinerarioProgramado, itinerario, OrigenItinerario.XLS, parametros);
                OK_Itinerario = true;
                return itinerario;
            }

            //Para archivos CSV y sin extensión
            else
            {
                try
                {
                    //Lee tabla
                    datosItinerarioProgramado = CrearDataTableItinerarioDesdeArchivo(new StreamReader(fs));
                    fs.Close();
                    //Se carga el itinerario
                    CrearItinerarioDeDataTable(datosItinerarioProgramado, itinerario, OrigenItinerario.CSV, parametros);
                    OK_Itinerario = true;
                    return itinerario;
                }
                catch
                {
                    OK_Itinerario = false;
                    return null;
                }
            }
        }

        /// <summary>
        /// Construye un DataTable desde un archivo de itinerario
        /// </summary>
        /// <param name="streamReader">Stream que apunta al archivo</param>
        /// <returns></returns>
        private static DataTable CrearDataTableItinerarioDesdeArchivo(StreamReader streamReader)
        {
            DataTable itin = new DataTable();
            object[] fila1;
            object[] fila;
            string[] datos_fila1;
            string[] datos;
            int n_fila1 = 0;
            int n_fila = 0;
            datos_fila1 = streamReader.ReadLine().Split(',');
            datos = streamReader.ReadLine().Split(',');
            n_fila1 = datos_fila1.Length;
            n_fila = datos.Length;
            fila1 = new object[n_fila];
            fila = new object[n_fila];
            DataColumn[] columnas = new DataColumn[n_fila];
            for (int i = 0; i < n_fila; i++)
            {
                columnas[i] = new DataColumn();
                if (i < n_fila1)
                {
                    fila1[i] = datos_fila1[i];
                }
                fila[i] = datos[i];
            }
            itin.Columns.AddRange(columnas);
            itin.Rows.Add(fila1);
            itin.Rows.Add(fila);


            while (!streamReader.EndOfStream)
            {
                datos = streamReader.ReadLine().Split(',');
                fila = datos;
                itin.Rows.Add(fila);
            }
            return itin;

        }

        /// <summary>
        /// Construye un itinerario desde un DataTable
        /// </summary>
        /// <param name="datosItinerarioProgramado">DataTable con el itinerario</param>
        /// <param name="itinerario">Itinerario en construcción</param>
        /// <param name="fuente">Tipo de archivo de origen del itinerario</param>
        /// <param name="parametros">Objeto de parámetros de la simulación</param>
        private static void CrearItinerarioDeDataTable(DataTable datosItinerarioProgramado, Itinerario itinerario, OrigenItinerario fuente, ParametrosSimuLAN parametros)
        {
            Dictionary<CamposArchivoItinerario, int> camposIndices = CargarCamposArchivoItinerario();
            bool primeraFila = true;
            foreach (DataRow row in datosItinerarioProgramado.Rows)
            {
                object[] items = row.ItemArray;
                //La primera fila contiene las fechas de inicio y término del itinerario
                if (primeraFila)
                {
                    primeraFila = false;
                    itinerario.FechaInicioString = items[3].ToString();
                    itinerario.FechaTerminoString = items[4].ToString();
                    try
                    {
                        itinerario.FechaInicio = Itinerario.FechaDesdeNumeroSerieExcel(itinerario.FechaInicioString);
                        itinerario.FechaTermino = Itinerario.FechaDesdeNumeroSerieExcel(itinerario.FechaTerminoString);
                        _tipo_fecha = TipoFecha.Serial;

                    }
                    catch
                    {
                        itinerario.FechaInicio = Convert.ToDateTime(itinerario.FechaInicioString);
                        itinerario.FechaTermino = Convert.ToDateTime(itinerario.FechaTerminoString);
                        _tipo_fecha = TipoFecha.Texto;
                    }

                    //Se setea a un día de distancia desde la fecha leída.
                    itinerario.FechaInicio = itinerario.FechaInicio.AddDays(-1);
                    itinerario.FechaTermino = itinerario.FechaTermino.AddDays(1);
                }
                //Para el resto de las filas se valida que tenga información y se agrega al itinerario en construcción.
                else
                {
                    if (FilaValida(items, camposIndices.Count - 5))
                    {
                        AgregarFila(items, camposIndices, itinerario, fuente, parametros);
                    }
                }
            }
        }

        /// <summary>
        /// Valida que el contenido de una fila tenga información útil.
        /// </summary>
        /// <param name="items">Arreglo con la información de una fila del archivo de itinerario</param>
        /// <param name="cantidadMinima">Cantidad mínima de campos no vacíos que debe tener la fila para que sea válida</param>
        /// <returns>True si la fila es válida</returns>
        private static bool FilaValida(object[] items, int cantidadMinima)
        {
            int contador = 0;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].ToString().ToCharArray().Length > 0)
                {
                    contador++;
                }
            }
            if (contador > cantidadMinima)
                return true;
            else return false;
        }

        #endregion

        #region PARAMETROS

        /// <summary>
        /// Carga la información de factores de desviación de probabilidades por escenarios
        /// </summary>
        /// <param name="dt">Datatable con la información</param>
        /// <param name="primeraFilaValida">True si la primera fila tiene datos válidos</param>
        /// <returns>Diccionario con la información</returns>
        public static Dictionary<string, Dictionary<TipoEscenarioDisrupcion, double>> CargarFactoresDesviacion(DataTable dt, bool primeraFilaValida)
        {
            Dictionary<string, Dictionary<TipoEscenarioDisrupcion, double>> retorno = new Dictionary<string, Dictionary<TipoEscenarioDisrupcion, double>>();
            if (!primeraFilaValida)
            {
                dt.Rows.RemoveAt(0);
            }
            foreach (DataRow row in dt.Rows)
            { 
                object[] valores = row.ItemArray;
                string key1 = valores[0].ToString();
                if (key1.ToString().ToCharArray().Length > 0)
                {
                    retorno.Add(key1, new Dictionary<TipoEscenarioDisrupcion, double>());
                    double valor_f_bueno = Convert.ToDouble(valores[1].ToString().Replace('.', ','));
                    double valor_f_malo = Convert.ToDouble(valores[2].ToString().Replace('.', ','));
                    retorno[key1].Add(TipoEscenarioDisrupcion.Bueno, valor_f_bueno);
                    retorno[key1].Add(TipoEscenarioDisrupcion.Malo, valor_f_malo);
                    retorno[key1].Add(TipoEscenarioDisrupcion.Normal, 1.0);
                }
                
            }
            return retorno;
        }

        /// <summary>
        /// Carga los parámetros de la simulación.
        /// </summary>
        /// <param name="ds">DataSet con las hojas de parámetros</param>
        /// <param name="parametros">Parámetros en construcción</param>
        /// <param name="factoresEscenarios">Factores de desviación de probabilidades</param>
        /// <param name="filename">Path de origen del DataSet</param>
        internal static void CargarParametros(DataSet ds, ParametrosSimuLAN parametros, Dictionary<TipoDisrupcion, Dictionary<string, Dictionary<TipoEscenarioDisrupcion, double>>> factoresEscenarios, string filename)
        {
            factoresEscenarios.Clear();
            parametros.Conexiones.Hubs.Clear();
            parametros.Conexiones.PaxConex.Clear();
            parametros.Conexiones.Pairings.Clear();
            parametros.InfoGruposFlotas.Clear();
            parametros.InfoAOG.Clear();

            #region MAP ACTYPE-FLOTA
            try
            {
                parametros.MapFlotas.Dict = parametros.MapFlotas.DataTableToDictionary(ds.Tables[parametros.MapFlotas.Nombre], 1);
            }
            catch (Exception)
            {
                throw new Exception("No se puede cargar tabla de actypes-flotas desde archivo de parámetros en " + filename);
            }
            #endregion

            #region MAP FLOTA-GRUPO
            try
            {
                parametros.MapGruposFlotas.Dict = parametros.MapGruposFlotas.DataTableToDictionary(ds.Tables[parametros.MapGruposFlotas.Nombre], 1);
            }
            catch (Exception)
            {
                throw new Exception("No se puede cargar tabla de grupos de flotas desde archivo de parámetros en " + filename);
            }
            #endregion

            #region MAP SUBFLOTA-MATRICULA
            try
            {
                parametros.MapSubFlotasMatriculas.Dict = parametros.MapSubFlotasMatriculas.DataTableToDictionary(ds.Tables[parametros.MapSubFlotasMatriculas.Nombre], 1);
            }
            catch (Exception)
            {
                throw new Exception("No se puede cargar tabla de grupos de flotas desde archivo de parámetros en " + filename);
            }
            #endregion

            #region MAP VUELO_RUTA
            try
            {
                parametros.MapVuelosRutas.Dict = parametros.MapVuelosRutas.DataTableToDictionary(ds.Tables[parametros.MapVuelosRutas.Nombre], 1);
            }
            catch (Exception)
            {
                throw new Exception("No se puede cargar tabla de vuelos-rutas desde archivo de parámetros en " + filename);
            }
            #endregion

            #region TURN AROUND MIN
            try
            {
                parametros.TurnAroundMin.Dict = parametros.TurnAroundMin.DataTableToDictionary(ds.Tables[parametros.TurnAroundMin.Nombre], 1);
            }
            catch (Exception)
            {
                throw new Exception("No se puede cargar tabla de Turn Around desde archivo de parámetros en " + filename);
            }
            #endregion

            #region MATRIZ FLOTA-FLOTA
            try
            {
                parametros.CargarMatrizFlotaFlota(ds.Tables["matrizFlotaFlota"]);
            }
            catch (Exception)
            {
                throw new Exception("No se puede cargar matriz flota-flota desde archivo de parámetros en " + filename);
            }
            #endregion

            #region MATRIZ MULTIOPERADOR
            try
            {
                parametros.CargarMatrizMultioperador(ds.Tables["matrizMultioperador"]);
            }
            catch (Exception)
            {
                throw new Exception("No se puede cargar matriz flota-flota desde archivo de parámetros en " + filename);
            }
            #endregion

            #region INFO HUBS
            try
            {
                parametros.Conexiones.LlenarInputHubs(ds.Tables["hubs"]);
            }
            catch (Exception)
            {
                throw new Exception("No se puede cargar tabla de HUBs desde archivo de parámetros en " + filename);
            }
            #endregion

            #region VUELOS EN CONEXION
            try
            {
                parametros.Conexiones.LlenarInputPaxConex(ds.Tables["conexiones"]);
            }
            catch (Exception)
            {
                throw new Exception("No se puede cargar tabla de vuelos en conexión desde archivo de parámetros en " + filename);
            }
            #endregion

            #region PAIRINGS
            try
            {
                parametros.Conexiones.LlenarInputPairings(ds.Tables["pairings"]);
            }
            catch (Exception)
            {
                throw new Exception("No se puede cargar tabla de pairings desde archivo de parámetros en " + filename);
            }
            #endregion

            #region FACTORES ESCENARIOS METEREOLOGIA
            try
            {
                factoresEscenarios.Add(TipoDisrupcion.METEREOLOGIA, CargarFactoresDesviacion(ds.Tables["fd_WXS"], false));
            }
            catch (Exception)
            {
                throw new Exception("No se puede cargar tabla de factores de desviación de clima desdesde archivo de parámetros en " + filename);
            }
            #endregion

            #region FACTORES ESCENARIOS MANTTO
            try
            {
                factoresEscenarios.Add(TipoDisrupcion.MANTENIMIENTO, CargarFactoresDesviacion(ds.Tables["fd_Mantto"], false));
            }
            catch (Exception)
            {
                throw new Exception("No se puede cargar tabla de factores de desviación de mantto desdesde archivo de parámetros en " + filename);
            }
            #endregion

            #region GRUPOS FLOTAS
            try
            {
                parametros.CargarInputGruposFlotas(ds.Tables["turnos"]);
            }
            catch (Exception)
            {
                throw new Exception("No se puede cargar informacion de turnos por grupos de flota desdesde archivo de parámetros en " + filename);
            }
            #endregion

            #region DECISION VUELOS EN CONEXION
            try
            {
                parametros.Conexiones.LlenarParametrosDecisionVuelosConexion(ds.Tables["conex_intervalos"], ds.Tables["conex_espera"]);
            }
            catch (Exception)
            {
                throw new Exception("No se puede cargar informacion de turnos por grupos de flota desdesde archivo de parámetros en " + filename);
            }
            #endregion

            #region AOG

            try
            {
                parametros.CargarInfoAOG(ds.Tables["AOG"]);
            }
            catch (Exception)
            {
                throw new Exception("No se puede cargar informacion de AOG's en " + filename);
            }

            #endregion
        }

        #endregion
    }
}
