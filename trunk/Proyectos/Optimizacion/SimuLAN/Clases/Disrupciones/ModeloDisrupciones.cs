using System;
using System.Collections.Generic;
using System.Text;
using SimuLAN.Clases.Disrupciones;
using System.Xml.Serialization;
using System.IO;
using System.Data;
using System.Xml;
using System.Xml.Xsl;
using SimuLAN.Utils;

namespace SimuLAN.Clases.Disrupciones
{   
    /// <summary>
    /// Objeto que encapsula el modelo de disrupciones
    /// </summary>
    [XmlRoot("modeloDisrupciones")]
    public class ModeloDisrupciones: ICloneable
    {
        #region ATRIBUTES
        
        /// <summary>
        /// Diccionario que guarda la información de las disrupciones.
        /// </summary>
        private SerializableDictionary<string, InfoDisrupcion> _coleccion_disrupciones;

        /// <summary>
        /// Diccionario con los aeropuertos que tienen disponible sus curvas históricas de probabilidad de mal tiempo.
        /// </summary>
        private SerializableDictionary<string, double[,]> _curvas_aeropuerto;

        /// <summary>
        /// Diccionario que entrega para cada tipo de disrupción el escenario simulado.
        /// </summary>
        private Dictionary<TipoDisrupcion, TipoEscenarioDisrupcion> _map_disrupciones_escenario;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Diccionario que guarda la información de las disrupciones.
        /// </summary>
        public SerializableDictionary<string, InfoDisrupcion> ColeccionDisrupciones
        {
            get { return _coleccion_disrupciones; }
            set { _coleccion_disrupciones = value; }
        }

        /// <summary>
        /// Diccionario con los aeropuertos que tienen disponible sus curvas históricas de probabilidad de mal tiempo.
        /// </summary>
        public SerializableDictionary<string, double[,]> CurvasAeropuerto
        {
            get { return _curvas_aeropuerto; }
            set { _curvas_aeropuerto = value; }
        }

        /// <summary>
        /// Diccionario que entrega para cada tipo de disrupción el escenario simulado.
        /// </summary>
        public Dictionary<TipoDisrupcion, TipoEscenarioDisrupcion> MapDisrupcionesEscenario
        {
            get { return _map_disrupciones_escenario; }
            set { _map_disrupciones_escenario = value; }
        }        

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Constructor de la clase
        /// </summary>
        /// <param name="o">Diferenciador</param>
        public ModeloDisrupciones(object o)
        {            
            _coleccion_disrupciones = new SerializableDictionary<string, InfoDisrupcion>();
            _map_disrupciones_escenario = DefaultMapDisrupcionesEscenario();
            _curvas_aeropuerto = new SerializableDictionary<string, double[,]>();
        }
       
        /// <summary>
        /// Constructor para serialización
        /// </summary>
        public ModeloDisrupciones()
        { }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Carga la información de WXS histórica desde una tabla con la información
        /// </summary>
        /// <param name="dt">Tabla de datos</param>
        /// <param name="omitirPrimeraFila">Indica si la primera fila de la tabla contiene encabezados</param>
        /// <returns>Matriz con las probabilidades históricas de WXS</returns>
        private double[,] CurvasWXSFromDateTable(DataTable dt, bool omitirPrimeraFila)
        {
            double[,] info = new double[12, 24];
            int omitirPrimeraFilaInt = 0;
            if (omitirPrimeraFila)
                omitirPrimeraFilaInt++;
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (dt.Rows.IndexOf(row) >= omitirPrimeraFilaInt)
                    {
                        object[] valores = row.ItemArray;
                        string mes_string = valores[0].ToString();
                        if (mes_string.ToCharArray().Length > 0)
                        {
                            int mes = MesDesdeString(mes_string);
                            for (int i = 0; i < 24; i++)
                            {
                                info[mes, i] = Convert.ToDouble(valores[i + 1].ToString().Replace('.', ','));
                            }
                        }
                    }
                }
                return info;
            }
            return null;
        }

        /// <summary>
        /// Valores por defecto para el escenario simulado de cada disrupción
        /// </summary>
        /// <returns></returns>
        private Dictionary<TipoDisrupcion, TipoEscenarioDisrupcion> DefaultMapDisrupcionesEscenario()
        {
            Dictionary<TipoDisrupcion, TipoEscenarioDisrupcion> dict = new Dictionary<TipoDisrupcion, TipoEscenarioDisrupcion>();
            dict.Add(TipoDisrupcion.ADELANTO, TipoEscenarioDisrupcion.Normal);
            dict.Add(TipoDisrupcion.ATC, TipoEscenarioDisrupcion.Normal);
            dict.Add(TipoDisrupcion.HBT, TipoEscenarioDisrupcion.Normal);
            dict.Add(TipoDisrupcion.MANTENIMIENTO, TipoEscenarioDisrupcion.Normal);
            dict.Add(TipoDisrupcion.OTROS, TipoEscenarioDisrupcion.Normal);
            dict.Add(TipoDisrupcion.METEREOLOGIA, TipoEscenarioDisrupcion.Normal);
            dict.Add(TipoDisrupcion.RECURSOS_DEL_APTO, TipoEscenarioDisrupcion.Normal);
            dict.Add(TipoDisrupcion.TA_BAJO_ALA, TipoEscenarioDisrupcion.Normal);
            dict.Add(TipoDisrupcion.TA_SOBRE_ALA, TipoEscenarioDisrupcion.Normal);
            dict.Add(TipoDisrupcion.TRIPULACIONES, TipoEscenarioDisrupcion.Normal);
            return dict;
        }

        /// <summary>
        /// Entrega la probabilidad de disrupcion por clima para un mes, aeropuerto y periodo del día específicos.
        /// </summary>
        /// <param name="mes">Mes del año</param>
        /// <param name="estacion">Estación</param>
        /// <param name="periodo">Periodo del día</param>
        /// <returns>Probabilidad de que haya una disrupción climática</returns>
        private double GetProbabilidadClima(string mes, string estacion, int hora)
        {
            if (this != null && this.ColeccionDisrupciones != null && this.ColeccionDisrupciones.ContainsKey(TipoDisrupcion.METEREOLOGIA.ToString()))
            {
                InfoDisrupcion3D infoWXS = (InfoDisrupcion3D)this.ColeccionDisrupciones[TipoDisrupcion.METEREOLOGIA.ToString()];
                if (infoWXS.Parametros != null)
                {
                    if (infoWXS.Parametros.ContainsKey(mes))
                    {
                        if (infoWXS.Parametros[mes].ContainsKey(estacion))
                        {
                            string periodo = Utilidades.GetPeriodo(hora, infoWXS.CantidadDeValoresPorColumna(2));
                            if (infoWXS.Parametros[mes][estacion].ContainsKey(periodo))
                            {
                                double factor = 1;
                                if (infoWXS.FactorDesviacionEscenario.ContainsKey(estacion))
                                {
                                    factor = infoWXS.FactorDesviacionEscenario[estacion][MapDisrupcionesEscenario[TipoDisrupcion.METEREOLOGIA]];
                                }                                
                                return infoWXS.Parametros[mes][estacion][periodo].Prob *factor ;
                            }
                        }
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// Permite obtener el mes int desde un string
        /// </summary>
        /// <param name="mes_string">string con información del mes en números o siglas</param>
        /// <returns>Mes entero en base cero</returns>
        private int MesDesdeString(string mes_string)
        {
            try
            {
                return (Convert.ToInt16(mes_string) - 1);
            }
            catch
            {
                switch (mes_string)
                {
                    case "ENE": return 0;
                    case "FEB": return 1;
                    case "MAR": return 2;
                    case "ABR": return 3;
                    case "MAY": return 4;
                    case "JUN": return 5;
                    case "JUL": return 6;
                    case "AGO": return 7;
                    case "SEP": return 8;
                    case "OCT": return 9;
                    case "NOV": return 10;
                    case "DIC": return 11;
                    case "JAN": return 0;
                    case "APR": return 3;
                    case "AUG": return 7;
                    case "DEC": return 11;
                    default: return -1;
                }
            }
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Carga el tipo de distribución de probabilidades para cada disrupción en la colección de disrupciones
        /// </summary>
        public void CargarTipoDistribucionEnDisrupciones()
        {
            _coleccion_disrupciones[TipoDisrupcion.ADELANTO.ToString()].Distribucion = DistribucionesEnum.LogNormal;
            _coleccion_disrupciones[TipoDisrupcion.ATC.ToString()].Distribucion = DistribucionesEnum.LogNormal;
            _coleccion_disrupciones[TipoDisrupcion.HBT.ToString()].Distribucion = DistribucionesEnum.Logística;
            _coleccion_disrupciones[TipoDisrupcion.MANTENIMIENTO.ToString()].Distribucion = DistribucionesEnum.LogNormal;
            _coleccion_disrupciones[TipoDisrupcion.OTROS.ToString()].Distribucion = DistribucionesEnum.LogNormal;
            _coleccion_disrupciones[TipoDisrupcion.METEREOLOGIA.ToString()].Distribucion = DistribucionesEnum.LogNormal;
            _coleccion_disrupciones[TipoDisrupcion.RECURSOS_DEL_APTO.ToString()].Distribucion = DistribucionesEnum.LogNormal;
            _coleccion_disrupciones[TipoDisrupcion.TA_BAJO_ALA.ToString()].Distribucion = DistribucionesEnum.LogNormal;
            _coleccion_disrupciones[TipoDisrupcion.TA_SOBRE_ALA.ToString()].Distribucion = DistribucionesEnum.LogNormal;
            _coleccion_disrupciones[TipoDisrupcion.TRIPULACIONES.ToString()].Distribucion = DistribucionesEnum.LogNormal;
        }

        /// <summary>
        /// Tranforma la información histórica de WXS de un aeropuerto particular en DataTable.
        /// </summary>
        /// <param name="aeropuerto">Estación</param>
        /// <param name="curvas">Matriz con parámetros</param>
        /// <returns></returns>
        private DataTable DataCurvasWXSToDataTable(string aeropuerto, double[,] curvas)
        {
            DataTable data = new DataTable("aeropuerto");
            DataColumn[] columnas = new DataColumn[25];
            columnas[0] = new DataColumn("Mes");
            for (int i = 1; i <= 24; i++)
            {
                columnas[i] = new DataColumn(i.ToString());
            }
            data.Columns.AddRange(columnas);
            for (int i = 0; i < 12; i++)
            {
                object[] obj = new object[25];
                obj[0] = (i + 1);
                for (int j = 0; j < 24; j++)
                {
                    obj[j + 1] = curvas[i, j].ToString();
                }
                data.Rows.Add(obj);
            }
            return data;
        }

        /// <summary>
        /// Carga la información histórica de WXS en diccionarios.
        /// </summary>
        /// <param name="ds">DataSet con la información</param>
        /// <param name="exitoso">Indica si el proceso resultó exitoso</param>
        /// <param name="omitirPrimeraFila">True si la primera fila no contiene información</param>
        public void DataSetToCurvasWxs(DataSet ds, out bool exitoso, bool omitirPrimeraFila)
        {
            exitoso = false;
            foreach (DataTable dt in ds.Tables)
            {
                string key_aeropuerto = dt.TableName;
                if (!this._curvas_aeropuerto.ContainsKey(key_aeropuerto))
                {
                    this._curvas_aeropuerto.Add(key_aeropuerto, CurvasWXSFromDateTable(dt, omitirPrimeraFila));
                }
                else
                {
                    this._curvas_aeropuerto[key_aeropuerto] = CurvasWXSFromDateTable(dt, omitirPrimeraFila);
                }
            }
            exitoso = true;
        }

        /// <summary>
        /// Método para deserializar un xml a un objeto Modelo de Disrupciones
        /// </summary>
        /// <param name="pathXmlFile">Ruta de archivo .xml</param>
        /// <param name="exitoso">True si el proceso de deserialización resulta exitoso</param>
        /// <returns>Objeto de ModeloDisrupciones</returns>
        public static ModeloDisrupciones Deserialize(string pathXmlFile, out bool exitoso)
        {
            exitoso = false;
            XmlSerializer s = new XmlSerializer(typeof(ModeloDisrupciones));
            ModeloDisrupciones newModelo;
            TextReader r = new StreamReader(pathXmlFile);
            newModelo = (ModeloDisrupciones)s.Deserialize(r);
            exitoso = true;
            r.Close();

            return newModelo;
        }

        /// <summary>
        /// Transforma la información de disrupciones a un DataSet.
        /// </summary>
        /// <returns>DataSet con la infomación de disrupciones</returns>
        public DataSet DictionaryDisrupcionesToDataset()
        {
            DataSet d = new DataSet();
            foreach (string disrupcion in _coleccion_disrupciones.Keys)
            {
                _coleccion_disrupciones[disrupcion].CargarDataTable();
                d.Tables.Add(_coleccion_disrupciones[disrupcion].Data);
            }
            return d;
        }

        /// <summary>
        /// Transforma la información histórica de WXS a un DataSet.
        /// </summary>
        /// <returns>DataSet con la infomación histórica de WXS</returns>
        public DataSet DictionaryInfoWXSToDataset()
        {
            DataSet d = new DataSet();
            foreach (string aeropuerto in _curvas_aeropuerto.Keys)
            {
                DataTable data = DataCurvasWXSToDataTable(aeropuerto, _curvas_aeropuerto[aeropuerto]);
                d.Tables.Add(data);
            }
            return d;
        }

        /// <summary>
        /// Retorna un delegado para obtener la probabilidad WXS
        /// </summary>
        public GetProbabilidadClimaEventHandler GetProbabilidadClimaAeropuerto()
        {
            return new GetProbabilidadClimaEventHandler(GetProbabilidadClima);
        }

        /// <summary>
        /// Limpia la colección de disrupciones de posibles valores nulos
        /// </summary>
        public void LimpiarDiccionario()
        {
            List<string> keysForRemove = new List<string>();
            foreach (string id in _coleccion_disrupciones.Keys)
            {
                if (_coleccion_disrupciones[id] == null)
                {
                    keysForRemove.Add(id);
                }
            }
            foreach (string id in keysForRemove)
            {
                _coleccion_disrupciones.Remove(id);
            }
        }

        /// <summary>
        /// Actualiza los diccionarios de disrupciones
        /// </summary>
        public void Refresh()
        {
            foreach (InfoDisrupcion info in _coleccion_disrupciones.Values)
            {
                if (info is InfoDisrupcion1D)
                {
                    ((InfoDisrupcion1D)info).Refresh();
                }
                else if (info is InfoDisrupcion2D)
                {
                    ((InfoDisrupcion2D)info).Refresh();
                }
                else if (info is InfoDisrupcion3D)
                {
                    ((InfoDisrupcion3D)info).Refresh();
                }
            }
        }

        /// <summary>
        /// Serializa la clase ModeloDisrupciones
        /// </summary>
        /// <param name="pathXmlFile">Ruta de archivo .xml</param>
        public void Serialize(string pathXmlFile)
        {
            XmlSerializer s = new XmlSerializer(typeof(ModeloDisrupciones));
            TextWriter w = new StreamWriter(pathXmlFile);
            s.Serialize(w, this);
            w.Close();
        }

        /// <summary>
        /// Pone nombre a las columnas de los DataTables de la colección de disrupciones del modelo
        /// </summary>
        public void SetColumnNamesDataTables()
        {
            foreach (InfoDisrupcion d in _coleccion_disrupciones.Values)
            {
                d.SetColumnNames();
            }
        }

        #region ICloneable Members

        /// <summary>
        /// Método para crear una nueva instancia de ModeloDisrupciones a partir de la actual.
        /// </summary>
        /// <returns>Objeto de ModeloDisrupciones</returns>
        public object Clone()
        {
            ModeloDisrupciones m = null;
            foreach (string s in this.ColeccionDisrupciones.Keys)
            {

                int dim = ColeccionDisrupciones[s].Dimension;
                if (dim == 1)
                {
                    InfoDisrupcion1D info = ColeccionDisrupciones[s] as InfoDisrupcion1D;
                    m.ColeccionDisrupciones.Add(s, (InfoDisrupcion1D)info.Clone());
                }
                else if (dim == 2)
                {
                    InfoDisrupcion2D info = (Disrupciones.InfoDisrupcion2D)ColeccionDisrupciones[s];
                    m.ColeccionDisrupciones.Add(s, (InfoDisrupcion2D)info.Clone());
                }
                else if (dim == 3)
                {
                    InfoDisrupcion3D info = (Disrupciones.InfoDisrupcion3D)ColeccionDisrupciones[s];
                    m.ColeccionDisrupciones.Add(s, (InfoDisrupcion3D)info.Clone());
                }
            }
            return m;
        }

        #endregion

        #endregion
    }
}
