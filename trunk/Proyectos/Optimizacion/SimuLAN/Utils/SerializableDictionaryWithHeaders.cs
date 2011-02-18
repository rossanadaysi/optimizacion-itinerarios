using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace SimuLAN.Utils
{
    /// <summary>
    /// Estructura de datos que encapsula un diccionario serializable, una tabla con la misma información 
    /// y  un arreglo con los encabezados de las columnas
    /// </summary>
    public class SerializableDictionaryWithHeaders
    {
        #region ATRIBUTES

        /// <summary>
        /// Datos en DataTable
        /// </summary>
        private DataTable _data;

        /// <summary>
        /// Datos en diccionario
        /// </summary>
        private SerializableDictionary<string, string> _dict;

        /// <summary>
        /// Encabezados de las columnas del DataTable
        /// </summary>
        private string[] _headers;

        /// <summary>
        /// Nombre de la instancia
        /// </summary>
        private string _nombre;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Datos en DataTable
        /// </summary>
        public DataTable Data
        {
            get { return _data; }
            set { _data = value; }
        }

        /// <summary>
        /// Datos en diccionario
        /// </summary>
        public SerializableDictionary<string, string> Dict
        {
            get { return _dict; }
            set { _dict = value; }
        }

        /// <summary>
        /// Encabezados de las columnas del DataTable
        /// </summary>
        public string[] Headers
        {
            get { return _headers; }
            set { _headers = value; }
        }

        /// <summary>
        /// Nombre de la instancia
        /// </summary>
        public string Nombre
        {
            get { return _nombre; }
            set { _nombre = value; }
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="headers">Encabezados de columnas</param>
        /// <param name="nombre">Identificador</param>
        public SerializableDictionaryWithHeaders(string[] headers, string nombre)
        {
            this._headers = headers;
            this._nombre = nombre;
            _dict = new SerializableDictionary<string, string>();
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Transforma un DataTable a SerializableDictionary
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="inicioData">Número de fila desde donde comienzan los datos</param>
        /// <returns>SerializableDictionary de una dimensión</returns>
        public SerializableDictionary<string, string> DataTableToDictionary(DataTable dt, int inicioData)
        {
            SerializableDictionary<string, string> retorno = new SerializableDictionary<string, string>();
            int dimension = _headers.Length;
            Utilidades.LimpiarDataTable(dimension, dt);
            _data = dt;
            for (int i = 0; i < dimension; i++)
            {
                _data.Columns[i].ColumnName = _headers[i];
            }
            for (int i = inicioData-1; i >= 0; i--)
            {
                _data.Rows.RemoveAt(i);
            }
            foreach (DataRow row in dt.Rows)
            {                
                object[] valores = row.ItemArray;
                string key1 = valores[0].ToString(); ;
                if (key1.ToString().ToCharArray().Length > 0)
                {
                    retorno.Add(key1, valores[1].ToString());
                }                
            }
            return retorno;
        }

        /// <summary>
        /// Recarga los valores del diccionario a partir del DataTable
        /// </summary>
        public void Refresh()
        {
            _dict = DataTableToDictionary(_data, 0);
        }

        /// <summary>
        /// Genera un DataTable a partir del diccionario serializable almacenado en la instancia actual.
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable SerializableDictionaryToDataTable()
        {
            DataTable data = new DataTable();
            data.TableName = this.Nombre;
            int dimension = _headers.Length;
            for (int i = 0; i <dimension; i++)
            {
                DataColumn dc = new DataColumn(_headers[i]);
                data.Columns.Add(dc);
            }
            if (_dict !=null && _dict.Count != 0)
            {
                foreach (string s1 in _dict.Keys)
                {
                    string[] fila = new string[dimension];
                    fila[0] = s1;
                    fila[1] = _dict[s1];
                    data.Rows.Add(fila);
                }
            }
            return data;
        }

        #endregion
    }

    /// <summary>
    /// Estructura de datos que encapsula un diccionario serializable con un nido, una tabla con la misma información 
    /// y  un arreglo con los encabezados de las columnas
    /// </summary>
    public class SerializableDictionaryWithHeaders2D
    {        
        #region ATRIBUTES

        /// <summary>
        /// Datos en DataTable
        /// </summary>
        private DataTable _data;

        /// <summary>
        /// Datos en diccionarios anidados
        /// </summary>
        SerializableDictionary<string, SerializableDictionary<string, string>> _dict;

        /// <summary>
        /// Encabezados de las columnas del DataTable
        /// </summary>
        private string[] _headers;

        /// <summary>
        /// Nombre de la instancia
        /// </summary>
        private string _nombre;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Datos en DataTable
        /// </summary>
        public DataTable Data
        {
            get { return _data; }
            set { _data = value; }
        }

        /// <summary>
        /// Datos en diccionarios anidados
        /// </summary>
        public SerializableDictionary<string, SerializableDictionary<string, string>> Dict
        {
            get { return _dict; }
            set { _dict = value; }
        }

        /// <summary>
        /// Encabezados de las columnas del DataTable
        /// </summary>
        public string[] Headers
        {
            get { return _headers; }
            set { _headers = value; }
        }

        /// <summary>
        /// Nombre de la instancia
        /// </summary>
        public string Nombre
        {
            get { return _nombre; }
            set { _nombre = value; }
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="headers">Encabezados de columnas</param>
        /// <param name="nombre">Identificador</param>
        public SerializableDictionaryWithHeaders2D(string[] headers, string nombre)
        {
            this._headers = headers;
            this._nombre = nombre;
            _dict = new SerializableDictionary<string, SerializableDictionary<string, string>>();
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Transforma un DataTable a SerializableDictionary
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="inicioData">Número de fila desde donde comienzan los datos</param>
        /// <returns>SerializableDictionary de dos dimensiones</returns>
        public SerializableDictionary<string, SerializableDictionary<string, string>> DataTableToDictionary(DataTable dt, int inicioData)
        {
            SerializableDictionary<string, SerializableDictionary<string, string>> retorno = new SerializableDictionary<string, SerializableDictionary<string, string>>();
            string key1 = null;
            string key2 = null;
            int dimension = _headers.Length;
            Utilidades.LimpiarDataTable(dimension, dt);
            _data = dt;
            for (int i = 0; i < dimension; i++)
            {
                _data.Columns[i].ColumnName = _headers[i];
            }
            for (int i = inicioData - 1; i >= 0; i--)
            {
                _data.Rows.RemoveAt(i);
            }
            foreach (DataRow row in dt.Rows)
            {
                object[] valores = row.ItemArray;                    
                string auxKey1 = valores[0].ToString();
                if (auxKey1.ToString().ToCharArray().Length > 0)
                {
                    if (key1 == null || !retorno.ContainsKey(auxKey1))
                    {
                        key1 = auxKey1;
                        retorno.Add(key1, new SerializableDictionary<string, string>());                            
                    }
                    else
                    {
                        key1 = auxKey1;
                    }

                    key2 = valores[1].ToString();
                    if (!retorno[key1].ContainsKey(key2))
                    {
                        retorno[key1].Add(key2, valores[2].ToString());
                    }
                }                
            }
            return retorno;
        }

        /// <summary>
        /// Recarga los valores del diccionario a partir del DataTable
        /// </summary>
        public void Refresh()
        {
            _dict = DataTableToDictionary(_data, 0);
        }

        /// <summary>
        /// Genera un DataTable a partir del diccionario serializable almacenado en la instancia actual.
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable SerializableDictionaryToDataTable()
        {
            DataTable data = new DataTable();
            data.TableName = this.Nombre;
            int dimension = _headers.Length;
            for (int i = 0; i < dimension; i++)
            {
                DataColumn dc = new DataColumn(_headers[i]);
                data.Columns.Add(dc);
            }

            if (_dict != null && _dict.Count != 0)
            {
                foreach (string s1 in _dict.Keys)
                {
                    foreach (string s2 in _dict[s1].Keys)
                    {
                        string[] fila = new string[dimension];
                        fila[0] = s1;
                        fila[1] = s2;
                        fila[2] = _dict[s1][s2];
                        data.Rows.Add(fila);
                    }
                }
            }
            return data;
        }

        #endregion
    }

    /// <summary>
    /// Estructura de datos que encapsula un diccionario serializable con dos nidos, una tabla con la misma información 
    /// y  un arreglo con los encabezados de las columnas
    /// </summary>
    public class SerializableDictionaryWithHeaders3D
    {
        #region ATRIBUTES

        /// <summary>
        /// Datos en DataTable
        /// </summary>
        private DataTable _data;

        /// <summary>
        /// Datos en diccionarios anidados
        /// </summary>
        SerializableDictionary<string, SerializableDictionary<string, SerializableDictionary<string, string>>> _dict;

        /// <summary>
        /// Encabezados de las columnas del DataTable
        /// </summary>
        private string[] _headers;

        /// <summary>
        /// Nombre de la instancia
        /// </summary>
        private string _nombre;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Datos en DataTable
        /// </summary>
        public DataTable Data
        {
            get { return _data; }
            set { _data = value; }
        }

        /// <summary>
        /// Datos en diccionarios anidados
        /// </summary>
        public SerializableDictionary<string, SerializableDictionary<string, SerializableDictionary<string, string>>> Dict
        {
            get { return _dict; }
            set { _dict = value; }
        }

        /// <summary>
        /// Encabezados de las columnas del DataTable
        /// </summary>
        public string[] Headers
        {
            get { return _headers; }
            set { _headers = value; }
        }

        /// <summary>
        /// Nombre de la instancia
        /// </summary>
        public string Nombre
        {
            get { return _nombre; }
            set { _nombre = value; }
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="headers">Encabezados de columnas</param>
        /// <param name="nombre">Identificador</param>
        public SerializableDictionaryWithHeaders3D(string[] headers, string nombre)
        {
            this._headers = headers;
            this._nombre = nombre;
            _dict = new SerializableDictionary<string, SerializableDictionary<string, SerializableDictionary<string, string>>>();
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Transforma un DataTable a SerializableDictionary
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="inicioData">Número de fila desde donde comienzan los datos</param>
        /// <returns>SerializableDictionary de tres dimensiones</returns>
        public SerializableDictionary<string, SerializableDictionary<string,SerializableDictionary<string, string>>> DataTableToDictionary(DataTable dt, int inicioData)
        {
            SerializableDictionary<string, SerializableDictionary<string, SerializableDictionary<string, string>>> retorno = new SerializableDictionary<string, SerializableDictionary<string, SerializableDictionary<string, string>>>();
            string key1 = null;
            string key2 = null;
            string key3 = null;
            int dimension = _headers.Length;
            Utilidades.LimpiarDataTable(dimension, dt);
            _data = dt;
            for (int i = 0; i < dimension; i++)
            {
                _data.Columns[i].ColumnName = _headers[i];
            }
            for (int i = inicioData - 1; i >= 0; i--)
            {
                _data.Rows.RemoveAt(i);
            }
            foreach (DataRow row in dt.Rows)
            {
                object[] valores = row.ItemArray;
                string auxKey1 = valores[0].ToString();                    
                if (auxKey1.ToString().ToCharArray().Length > 0)
                {
                    if (key1 == null || (key1 != auxKey1 && !retorno.ContainsKey(auxKey1)))
                    {
                        key1 = auxKey1;
                        retorno.Add(key1, new SerializableDictionary<string, SerializableDictionary<string, string>>());
                    }
                    string auxKey2 = valores[1].ToString();
                    if (auxKey2.ToString().ToCharArray().Length > 0)
                    {
                        if (key2 == null || (key2.ToString() != auxKey2.ToString() && !retorno[key1].ContainsKey(auxKey2)))
                        {
                            key2 = auxKey2;
                            retorno[key1].Add(key2, new SerializableDictionary<string, string>());
                        }
                        key3 = valores[2].ToString();
                        retorno[key1][key2].Add(key3, valores[3].ToString());
                    }
                }                
            }
            return retorno;
        }

        /// <summary>
        /// Recarga los valores del diccionario a partir del DataTable
        /// </summary>
        public void Refresh()
        {
            _dict = DataTableToDictionary(_data, 0);
        }

        /// <summary>
        /// Genera un DataTable a partir del diccionario serializable almacenado en la instancia actual.
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable SerializableDictionaryToDataTable()
        {
            DataTable data = new DataTable();
            data.TableName = this.Nombre;
            int dimension = _headers.Length;
            if (_dict != null && _dict.Count != 0)
            {
                foreach (string s1 in _dict.Keys)
                {
                    foreach (string s2 in _dict[s1].Keys)
                    {
                        foreach (string s3 in _dict[s1][s2].Keys)
                        {
                            string[] fila = new string[dimension];
                            fila[0] = s1;
                            fila[1] = s2;
                            fila[2] = s3;
                            fila[3] = _dict[s1][s2][s3];
                            data.Rows.Add(fila);
                        }
                    }
                }
            }
            return data;
        }

        #endregion
    }
}
