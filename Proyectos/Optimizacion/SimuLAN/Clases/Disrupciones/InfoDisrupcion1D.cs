using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Data;
using SimuLAN.Utils;

namespace SimuLAN.Clases.Disrupciones
{
    /// <summary>
    /// Objeto que encapsula disrupciones con un factor explicativo
    /// </summary>
    [XmlRoot("infoDisrucion1D")]
    public class InfoDisrupcion1D: InfoDisrupcion, ICloneable
    {
        #region ATRIBUTES

        /// <summary>
        /// Diccionario serializable que guarda la información de la disrupción
        /// </summary>
        private SerializableDictionary<string, DataDisrupcion> _parametros;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Diccionario serializable que guarda la información de la disrupción
        /// </summary>
        public SerializableDictionary<string, DataDisrupcion> Parametros
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
        public InfoDisrupcion1D(string nombre, int dimension, bool aplicaDesviacionEnEscenarios)
            : base(nombre,dimension,aplicaDesviacionEnEscenarios)
        {
            this._parametros = new SerializableDictionary<string, DataDisrupcion>();
        }

        /// <summary>
        /// Crea instancia de la clase con una tabla de datos
        /// </summary>
        /// <param name="nombre">Nombre de la disrupción</param>
        /// <param name="dimension">Cantidad de factores explicativos</param>
        /// <param name="dt">Tabla de datos</param>
        /// <param name="inicioData">Fila a partir de los cuales hay información útil</param>
        public InfoDisrupcion1D(string nombre, int dimension, bool aplicaDesviacionEnEscenarios, DataTable dt)
            : base(nombre, dimension, aplicaDesviacionEnEscenarios)
        {
            this._parametros = new SerializableDictionary<string, DataDisrupcion>();
            base.Data = dt;
            this._parametros = DataTableToDictionary(dt);
        }
        
        /// <summary>
        /// Constructor usado para serialización
        /// </summary>
        public InfoDisrupcion1D()
        { }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Convierte una tabla de datos a diccionario
        /// </summary>
        /// <param name="dt">Tabla de datos</param>
        /// <param name="inicioData">Fila a partir de la que existen datos útiles</param>
        /// <returns>Diccionario de datos</returns>
        private SerializableDictionary<string, DataDisrupcion> DataTableToDictionary(DataTable dt)
        {
           SerializableDictionary<string, DataDisrupcion> retorno = new SerializableDictionary<string, DataDisrupcion>();
           foreach(DataRow row in dt.Rows)
           {
               object[] valores = row.ItemArray;
               string key1 = null;
               DataDisrupcion parametrosLocal = new DataDisrupcion();
               key1 = valores[0].ToString();
               if (key1.ToCharArray().Length>0)
               {
                   parametrosLocal.Prob = Convert.ToDouble(valores[1].ToString().Replace('.', ','));
                   parametrosLocal.Media = Convert.ToDouble(valores[2].ToString().Replace('.', ','));
                   parametrosLocal.Desvest = Convert.ToDouble(valores[3].ToString().Replace('.', ','));
                   if (this.TieneMinMax)
                   {
                       parametrosLocal.Min = Convert.ToDouble(valores[4].ToString().Replace('.', ','));
                       parametrosLocal.Max = Convert.ToDouble(valores[5].ToString().Replace('.', ','));
                   }
                   else
                   {
                       parametrosLocal.Min = 0;
                       parametrosLocal.Max = int.MaxValue;
                   }
                   retorno.Add(key1, parametrosLocal);
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
            InfoDisrupcion1D a = new InfoDisrupcion1D(this.Nombre, this.Dimension, this.AplicaDesviacionEnEscenarios);
            foreach (string s1 in this.Parametros.Keys)
            {
                a.Parametros.Add(s1, new DataDisrupcion());
                a.Parametros[s1].Prob = this.Parametros[s1].Prob;
                a.Parametros[s1].Media = this.Parametros[s1].Media;
                a.Parametros[s1].Desvest = this.Parametros[s1].Desvest;
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

            foreach (string s in _parametros.Keys)
            {
                if (this.TieneMinMax)
                {
                    object[] fila = new object[6];
                    fila[0] = s;
                    fila[1] = _parametros[s].Prob;
                    fila[2] = _parametros[s].Media;
                    fila[3] = _parametros[s].Desvest;
                    fila[4] = _parametros[s].Min;
                    fila[5] = _parametros[s].Max;
                    Data.Rows.Add(fila);
                }
                else
                {
                    object[] fila = new object[4];
                    fila[0] = s;
                    fila[1] = _parametros[s].Prob;
                    fila[2] = _parametros[s].Media;
                    fila[3] = _parametros[s].Desvest;
                    Data.Rows.Add(fila);
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
