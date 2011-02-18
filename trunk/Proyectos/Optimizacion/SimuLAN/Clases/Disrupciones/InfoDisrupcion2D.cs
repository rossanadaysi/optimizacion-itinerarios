using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using SimuLAN.Utils;
using System.Data;

namespace SimuLAN.Clases.Disrupciones
{   
    /// <summary>
    /// Objeto que encapsula disrupciones con dos factores explicativos
    /// </summary>
    [XmlRoot("infoDisrucion2D")]
    public class InfoDisrupcion2D : InfoDisrupcion,ICloneable
    {
        #region ATRIBUTES

        /// <summary>
        /// Diccionario serializable que guarda la información de la disrupción
        /// </summary>
        private SerializableDictionary<string, SerializableDictionary<string, DataDisrupcion>> _parametros;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Diccionario serializable que guarda la información de la disrupción
        /// </summary>
        public SerializableDictionary<string, SerializableDictionary<string, DataDisrupcion>> Parametros
        {
            get { return _parametros; }
            set { _parametros = value; }
        }

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Crea instancia de la clase sin datos
        /// </summary>
        /// <param name="nombre">Nombre de la disrupción</param>
        /// <param name="dimension">Cantidad de factores explicativos</param>
        public InfoDisrupcion2D(string nombre, int dimension, bool aplicaDesviacionEnEscenarios)
            : base(nombre, dimension, aplicaDesviacionEnEscenarios)
        {
            this._parametros = new SerializableDictionary<string, SerializableDictionary<string, DataDisrupcion>>();
        }

        /// <summary>
        /// Crea instancia de la clase con una tabla de datos
        /// </summary>
        /// <param name="nombre">Nombre de la disrupción</param>
        /// <param name="dimension">Cantidad de factores explicativos</param>
        /// <param name="dt">Tabla de datos</param>
        /// <param name="inicioData">Fila a partir de los cuales hay información útil</param>
        public InfoDisrupcion2D(string nombre, int dimension, bool aplicaDesviacionEnEscenarios, DataTable dt)
            : base(nombre, dimension, aplicaDesviacionEnEscenarios)
        {
            this._parametros = new SerializableDictionary<string, SerializableDictionary<string, DataDisrupcion>>();
            base.Data = dt;
            this._parametros = DataTableToDictionary(dt);
        }

        /// <summary>
        /// Constructor usado para serialización
        /// </summary>
        public InfoDisrupcion2D()
        { }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Convierte una tabla de datos a diccionario
        /// </summary>
        /// <param name="dt">Tabla de datos</param>
        /// <param name="inicioData">Fila a partir de la que existen datos útiles</param>
        /// <returns>Diccionario de datos</returns>
        private SerializableDictionary<string, SerializableDictionary<string, DataDisrupcion>> DataTableToDictionary(DataTable dt)
        {
            SerializableDictionary<string, SerializableDictionary<string, DataDisrupcion>> retorno = new SerializableDictionary<string, SerializableDictionary<string, DataDisrupcion>>();
            foreach (DataRow row in dt.Rows)
            {
                object[] valores = row.ItemArray;
                DataDisrupcion parametrosLocal = new DataDisrupcion();
                string auxKey1 = valores[0].ToString();
                if (auxKey1.ToCharArray().Length > 0)
                {
                    if (!retorno.ContainsKey(auxKey1))
                    {
                        retorno.Add(auxKey1, new SerializableDictionary<string, DataDisrupcion>());
                    }
                    string key2 = valores[1].ToString();
                    parametrosLocal.Prob = Convert.ToDouble(valores[2].ToString().Replace('.', ','));
                    parametrosLocal.Media = Convert.ToDouble(valores[3].ToString().Replace('.', ','));
                    parametrosLocal.Desvest = Convert.ToDouble(valores[4].ToString().Replace('.', ','));
                    if (this.TieneMinMax)
                    {
                        parametrosLocal.Min = Convert.ToDouble(valores[5].ToString().Replace('.', ','));
                        parametrosLocal.Max = Convert.ToDouble(valores[6].ToString().Replace('.', ','));
                    }
                    else
                    {
                        parametrosLocal.Min = 0;
                        parametrosLocal.Max = int.MaxValue;
                    }
                    retorno[auxKey1].Add(key2, parametrosLocal);
                }                    
            }
            return retorno;
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Nombre de la disrupción
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Nombre;
        }

        #region ICloneable Members

        /// <summary>
        /// Crea una copia de la instancia actual
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            InfoDisrupcion2D a = new InfoDisrupcion2D(this.Nombre, this.Dimension, this.AplicaDesviacionEnEscenarios);
            foreach (string s1 in this.Parametros.Keys)
            {
                a.Parametros.Add(s1, new SerializableDictionary<string, DataDisrupcion>());
                foreach (string s2 in this.Parametros[s1].Keys)
                {
                    a.Parametros[s1].Add(s2, new DataDisrupcion());
                    a.Parametros[s1][s2].Prob = this.Parametros[s1][s2].Prob;
                    a.Parametros[s1][s2].Media = this.Parametros[s1][s2].Media;
                    a.Parametros[s1][s2].Desvest = this.Parametros[s1][s2].Desvest;
                }
            }
            return a;
        }

        #endregion


        #endregion

        #region INTERNAL METHODS

        /// <summary>
        /// Carga la tabla de datos
        /// </summary>
        internal override void CargarDataTable()
        {
            base.CargarDataTable();
            
            foreach (string s1 in _parametros.Keys)
            {
                foreach (string s2 in _parametros[s1].Keys)
                {
                    
                    if (this.TieneMinMax)
                    {
                        object[] fila = new object[7];
                        fila[0] = s1;
                        fila[1] = s2;
                        fila[2] = _parametros[s1][s2].Prob;
                        fila[3] = _parametros[s1][s2].Media;
                        fila[4] = _parametros[s1][s2].Desvest;
                        fila[5] = _parametros[s1][s2].Min;
                        fila[6] = _parametros[s1][s2].Max;
                        Data.Rows.Add(fila);
                    }
                    else
                    {
                        object[] fila = new object[5];
                        fila[0] = s1;
                        fila[1] = s2;
                        fila[2] = _parametros[s1][s2].Prob;
                        fila[3] = _parametros[s1][s2].Media;
                        fila[4] = _parametros[s1][s2].Desvest;
                        Data.Rows.Add(fila);
                    }
                   
                }               
            }
        }

        /// <summary>
        /// Regarga la vista dictionary de los parámetros de la disrupción
        /// </summary>
        internal override void Refresh()
        {
            base.Refresh();
            _parametros.Clear();
            _parametros = DataTableToDictionary(Data);
        }

        #endregion
    }
}
