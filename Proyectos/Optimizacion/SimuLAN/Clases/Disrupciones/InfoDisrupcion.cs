using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Data;
using SimuLAN.Utils;

namespace SimuLAN.Clases.Disrupciones
{
    /// <summary>
    /// Objeto que encapsula la información de un tipo de disrupción
    /// </summary>
    [XmlRoot("infoDisrucion")]
    [XmlInclude(typeof(InfoDisrupcion1D)), XmlInclude(typeof(InfoDisrupcion2D)), XmlInclude(typeof(InfoDisrupcion3D))]
    public class InfoDisrupcion
    {
        #region ATRIBUTES

        /// <summary>
        /// Indica si la disrupción se altera en distintos escenarios 
        /// </summary>
        private bool _aplica_desviacion_en_escenarios;
        
        /// <summary>
        /// Almacena los distintos elementos por cada columna;
        /// </summary>
        private SerializableDictionary<int, SerializableList<string>> _cantidad_valores_por_columna;

        /// <summary>
        /// Tabla de datos
        /// </summary>
        private DataTable _data;

        /// <summary>
        /// Cantidad de factores explicativos de la disrupción
        /// </summary>
        private int _dimension;
        
        /// <summary>
        /// Distribución de probabilidades aplicada a las curvas.
        /// </summary>
        private DistribucionesEnum _distribucion;
        
        /// <summary>
        /// Diccionario que almacena los valores de desviación de la probabilidad de ocurrencia de la disrupción para 
        /// cada tipo de escenario. Hay un set de factores para cada string del diccionario principal.
        /// </summary>
        private Dictionary<string, Dictionary<TipoEscenarioDisrupcion, double>> _factor_desviacion_escenario;

        /// <summary>
        /// Nombre de la disrupción
        /// </summary>
        private string _nombre;

        /// <summary>
        /// Indica si se guarda mínimo y máximo
        /// </summary>
        private bool _tiene_min_max;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Indica si la disrupción se altera en distintos escenarios 
        /// </summary>
        public bool AplicaDesviacionEnEscenarios
        {
            get { return _aplica_desviacion_en_escenarios; }
            set { _aplica_desviacion_en_escenarios = value; }
        }
        
        /// <summary>
        /// Tabla de datos
        /// </summary>
        [XmlIgnore]
        public DataTable Data
        {
            get { return _data; }
            set { _data = value; }
        }

        /// <summary>
        /// Cantidad de factores explicativos de la disrupción
        /// </summary>
        [XmlIgnore]
        public int Dimension
        {
            get { return _dimension; }
            set { _dimension = value; }
        }

        /// <summary>
        /// Distribución de probabilidades aplicada a las curvas.
        /// </summary>
        [XmlIgnore]
        public DistribucionesEnum Distribucion
        {
            get { return _distribucion; }
            set { _distribucion = value; }
        }

        /// <summary>
        /// Diccionario que almacena los valores de desviación de la probabilidad de ocurrencia de la disrupción para 
        /// cada tipo de escenario. Hay un set de factores para cada string del diccionario principal.
        /// </summary>
        [XmlIgnore]
        public Dictionary<string, Dictionary<TipoEscenarioDisrupcion, double>> FactorDesviacionEscenario
        {
            get { return _factor_desviacion_escenario; }
            set { _factor_desviacion_escenario = value; }
        }

        /// <summary>
        /// Encabezados de las columnas de la tabla de datos
        /// </summary>
        public List<string> Headers
        {
            get 
            {
                List<string> headers = new List<string>();
                if (_nombre == TipoDisrupcion.OTROS.ToString()
                || _nombre == TipoDisrupcion.RECURSOS_DEL_APTO.ToString()
                || _nombre == TipoDisrupcion.TA_BAJO_ALA.ToString()
                || _nombre == TipoDisrupcion.TA_SOBRE_ALA.ToString()
                || _nombre == TipoDisrupcion.TRIPULACIONES.ToString())
                {
                    headers.Add("Estación");
                    headers.Add("Hora UTC");
                }
                else if (_nombre == TipoDisrupcion.HBT.ToString())
                {
                    headers.Add("Tramo");
                    headers.Add("Grupo");
                }
                else if (_nombre == TipoDisrupcion.MANTENIMIENTO.ToString())
                {
                    headers.Add("Matrícula-Flota");
                }
                else if (_nombre == TipoDisrupcion.ADELANTO.ToString())
                {
                    headers.Add("Estación");
                }
                else if (_nombre == TipoDisrupcion.METEREOLOGIA.ToString()
                    || _nombre == TipoDisrupcion.ATC.ToString())
                {
                    headers.Add("Mes");
                    headers.Add("Estación");
                    headers.Add("Periodo");
                }

                headers.Add("Prob");
                headers.Add("Media");
                headers.Add("Desvest");
                if (_tiene_min_max)
                {
                    headers.Add("Min");
                    headers.Add("Max");
                }
                return headers;
            }
        }

        /// <summary>
        /// Nombre de la disrupción
        /// </summary>
        public string Nombre
        {
            get { return _nombre; }
            set { _nombre = value; }
        }

        /// <summary>
        /// Indica si se guarda mínimo y máximo
        /// </summary>
        public bool TieneMinMax
        {
            get { return _tiene_min_max; }
        }
        
        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nombre">Nombre de la disrupción</param>
        /// <param name="dimension">Cantidad de factores explicativos</param>
        public InfoDisrupcion(string nombre, int dimension, bool aplicaDesviacionEnEscenarios)
        {
            if (nombre == TipoDisrupcion.HBT.ToString())
            {
                _tiene_min_max = true;
            }
            this._nombre = nombre;
            this._dimension = dimension;
            this._data = new DataTable(nombre);
            this._factor_desviacion_escenario = new Dictionary<string, Dictionary<TipoEscenarioDisrupcion, double>>();
            this._cantidad_valores_por_columna = new SerializableDictionary<int, SerializableList<string>>();
            this._aplica_desviacion_en_escenarios = aplicaDesviacionEnEscenarios;
        }

        /// <summary>
        /// Constructor para serialización
        /// </summary>
        public InfoDisrupcion()
        { }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Retorna la cantidad de elementos distintos agregados en la columna 'col'
        /// </summary>
        /// <param name="col">Número de columna (desde cero)</param>
        /// <returns></returns>
        public int CantidadDeValoresPorColumna(int col)
        {
            if (_cantidad_valores_por_columna.Count > 0)
            {
                return _cantidad_valores_por_columna[col].Count;
            }
            else
            {
                int contador = 0;
                foreach (DataColumn c in _data.Columns)
                {
                    _cantidad_valores_por_columna.Add(contador, new SerializableList<string>());
                    contador++;
                }
                foreach (DataRow row in _data.Rows)
                {
                    object[] items = row.ItemArray;
                    for (int i = 0; i < items.Length; i++)
                    {
                        _cantidad_valores_por_columna[i].Add(items[i].ToString());
                    }
                }
                return _cantidad_valores_por_columna[col].Count;
            }
        }

        /// <summary>
        /// Crea una instancia de InfoDisrupcion desde una tabla de datos
        /// </summary>
        /// <param name="dt">Tabla de datos</param>
        /// <param name="omitirPrimeraFila">Indica si debe cargarse o no la primera fila, que puede contener texto de encabezados</param>
        /// <returns></returns>
        public InfoDisrupcion InfoDisrupcionFromDateTable(DataTable dt, bool omitirPrimeraFila)
        {
            if (omitirPrimeraFila)
            {
                dt.Rows.RemoveAt(0);
            }
            this._data = dt;
            if (dt.Rows.Count > 0)
            {
                int dimension = GetDimension(dt.Rows[0].ItemArray) - 3;
                if (dt.TableName == TipoDisrupcion.HBT.ToString())
                {
                    dimension -= 2;
                    _tiene_min_max = true;
                }               
                if (dimension == 1)
                {
                    return new InfoDisrupcion1D(dt.TableName,dimension,true, dt);
                }
                if (dimension == 2)
                {
                    return new InfoDisrupcion2D(dt.TableName, dimension, true, dt);
                }
                if (dimension == 3)
                {
                    return new InfoDisrupcion3D(dt.TableName, dimension, true, dt);
                }
            }
            return null;

        }
        
        #endregion
        
        #region PRIVATE STATIC METHODS

        /// <summary>
        /// Retorna la cantidad de elementos no vacíos de un arreglo de objetos.
        /// </summary>
        /// <param name="obj">Arreglo con objetos</param>
        /// <returns>Número de elementos no nulos del arreglo de objetos</returns>
        private static int GetDimension(object[] obj)
        {
            int contador=0;
            for (int i = 0; i < obj.Length; i++)
            {
                if (obj[i].ToString().ToCharArray().Length > 0)
                {
                    contador++;
                }
            }
            return contador;
        }
        
        #endregion
        
        #region INTERNAL METHODS

        /// <summary>
        /// Método para cargar la tabla de datos de la disrupción
        /// </summary>
        internal virtual void CargarDataTable()
        {
            DataColumn[] dc = new DataColumn[_dimension];;            
            Data = new DataTable(_nombre);
            Data.Columns.Clear();
            if (_nombre == TipoDisrupcion.OTROS.ToString()
                || _nombre == TipoDisrupcion.RECURSOS_DEL_APTO.ToString()
                || _nombre == TipoDisrupcion.TA_BAJO_ALA.ToString()
                || _nombre == TipoDisrupcion.TA_SOBRE_ALA.ToString()
                || _nombre == TipoDisrupcion.TRIPULACIONES.ToString())
            {
                dc[0] = new DataColumn("Estación");
                dc[1] = new DataColumn("Hora UTC");
            }
            else if (_nombre == TipoDisrupcion.HBT.ToString())
            {              
                dc[0] = new DataColumn("Tramo");
                dc[1] = new DataColumn("Grupo");
            }
            else if (_nombre == TipoDisrupcion.MANTENIMIENTO.ToString())
            {
                dc = new DataColumn[4];
                dc[0] = new DataColumn("Matrícula-Flota");
            }
            else if (_nombre == TipoDisrupcion.ADELANTO.ToString())
            {             
                dc[0] = new DataColumn("Estación");
            }
            else if (_nombre == TipoDisrupcion.METEREOLOGIA.ToString()
                || _nombre == TipoDisrupcion.ATC.ToString())
            {
                dc[0] = new DataColumn("Mes");
                dc[1] = new DataColumn("Estación");
                dc[2] = new DataColumn("Periodo");
            }
            else
            {
                for (int i = 0; i < _dimension; i++)
                {
                    dc[i] = new DataColumn();
                }
            }
            Data.Columns.AddRange(dc);
            Data.PrimaryKey = dc;

            Data.Columns.Add("Prob");
            Data.Columns.Add("Media");
            Data.Columns.Add("Desvest");
            if (_tiene_min_max)
            {
                Data.Columns.Add("Min");
                Data.Columns.Add("Max");
            }
            Data.Rows.Clear();
        }

        /// <summary>
        /// Entrega el factor de desviacion de la probabilidad para un detarminado escenario y key.
        /// </summary>
        /// <param name="key">Clave del diccionario de factores</param>
        /// <param name="escenario">Escenario de la disrupción</param>
        /// <returns>Factor de desviacion de la probabilidad de la disrupción</returns>
        internal double GetFactorDesviacionProb(string key, TipoEscenarioDisrupcion escenario)
        {
            double factor = 1;
            if (this._aplica_desviacion_en_escenarios && key!=null)
            {
                if (this.FactorDesviacionEscenario !=null && this.FactorDesviacionEscenario.ContainsKey(key))
                {
                    factor = FactorDesviacionEscenario[key][escenario];
                    return factor;
                }
            }
            return factor;
        }

        /// <summary>
        /// Regarga la vista dictionary de los parámetros de la disrupción
        /// </summary>
        internal virtual void Refresh()
        {

        }

        /// <summary>
        /// Agrega el nombre de las columnas a DataTable del objeto
        /// </summary>
        internal void SetColumnNames()
        {
            List<string> headers = Headers;
            for (int i = 0; i < headers.Count; i++)
            {
                _data.Columns[i].ColumnName = headers[i];
            }
            List<DataRow> remover = new List<DataRow>();
            for (int i = 0; i < _data.Rows.Count; i++)
            {
                object[] items = _data.Rows[i].ItemArray;
                if (items[0].ToString().ToCharArray().Length == 0)
                {
                    remover.Add(_data.Rows[i]);
                }
            }
            foreach (DataRow d in remover)
            {
                _data.Rows.Remove(d);
            }
            remover.Clear();
        }

        #endregion
    }
}
