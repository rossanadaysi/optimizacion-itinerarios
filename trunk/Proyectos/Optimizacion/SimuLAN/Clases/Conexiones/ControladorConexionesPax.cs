using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace SimuLAN.Clases
{
    /// <summary>
    /// Clase que encapsula la lógica de generación de atrasos por conexión de pasajeros
    /// </summary>
    public class ControladorConexionesPax
    {
        #region ATRIBUTES

        /// <summary>
        /// Delegado para obtener los minutos de espera por pasajeros en conexión
        /// </summary>
        private GetMinutosEsperaConexionEventHandler _get_minutos_espera_pax;

        /// <summary>
        /// Arreglo con los límites de los intervalos de horas hasta el próximo vuelo 
        /// para la decisión de minutos de espera.
        /// </summary>
        private int[] _limites_decision_horas_prox_vuelo;

        /// <summary>
        /// Arreglo con los límites de los intervalos de cantidad de pasajeros en conexión 
        /// para la decisión de minutos de espera.
        /// </summary>
        private int[] _limites_decision_pax;

        /// <summary>
        /// Matriz de decisión para los minutos de espera por pasajeros en conexión.
        /// </summary>
        private int[,] _matriz_minutos_espera;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Delegado para obtener los minutos de espera por pasajeros en conexión
        /// </summary>
        public GetMinutosEsperaConexionEventHandler GetMinutosEsperaPax
        {
            get { return _get_minutos_espera_pax; }
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="limites_decision_horas_espera">Arreglo con los límites de los intervalos de horas hasta 
        /// el próximo vuelo para la decisión de minutos de espera.</param>
        /// <param name="limites_decision_pax">Arreglo con los límites de los intervalos de cantidad de pasajeros 
        /// en conexión para la decisión de minutos de espera.</param>
        /// <param name="minutos_espera">Matriz de decisión para los minutos de espera por pasajeros en conexión.</param>
        public ControladorConexionesPax(int[] limites_decision_horas_espera, int[] limites_decision_pax, int[,] minutos_espera)
        {
            if (limites_decision_horas_espera == null || limites_decision_horas_espera == null || minutos_espera == null)
                throw new Exception("Error en definiciones de políticas de conexiones.");

            this._limites_decision_horas_prox_vuelo = limites_decision_horas_espera;
            this._limites_decision_pax = limites_decision_pax;
            this._matriz_minutos_espera = minutos_espera;
            this._get_minutos_espera_pax = new GetMinutosEsperaConexionEventHandler(GetEspera);
            if (!ValidarDimensiones())
            {
                throw new Exception("La tabla de decision no es consistente con los intervalos definidos");
            }
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Valida que las dimensiones de la matriz de decisión sean coherentes con la de los límites de intervalos de decisión.
        /// </summary>
        /// <returns></returns>
        private bool ValidarDimensiones()
        {
            int dim1 = _limites_decision_horas_prox_vuelo.Length;
            int dim2 = _limites_decision_pax.Length;
            int dim3 = _matriz_minutos_espera.Length;
            return ((dim1 + 1) * (dim2 + 1) == dim3);
        }

        #endregion

        #region INTERNAL METHODS

        /// <summary>
        /// Estima la cantidad de minutos que se está dispuesto a esperar a un vuelo en conexión
        /// </summary>
        /// <param name="pax_conex">Cantidad de pasajeros en conexión</param>
        /// <param name="minutos_prox_vuelo">Minutos hasta el próximo vuelo</param>
        /// <returns></returns>
        internal int GetEspera(double pax_conex, int minutos_prox_vuelo)
        {
            int index_pax = 0;
            int index_horas_espera = 0;
            for (int i = _limites_decision_pax.Length-1; i >= 0; i--)
            {
                if (pax_conex > _limites_decision_pax[i])
                {
                    index_pax = (i + 1);
                }
            }
            for (int i = _limites_decision_horas_prox_vuelo.Length-1; i >= 0; i--)
            {
                if (minutos_prox_vuelo / 60.0 > _limites_decision_horas_prox_vuelo[i])
                {
                    index_horas_espera = (i + 1);
                }
            }
            int minutos_espera = _matriz_minutos_espera[index_pax, index_horas_espera];
            return minutos_espera;
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Transforma los valores de los límites de intervalos de decisión en valores de tabla
        /// </summary>
        /// <returns></returns>
        public DataTable InputIntervalosDecisionToDataTable()
        {
            DataTable data = new DataTable("conex_intervalos");
            DataColumn[] columnas = new DataColumn[2];
            columnas[0] = new DataColumn("Prom_pax");
            columnas[1] = new DataColumn("Horas_prox_vuelo");
            data.Columns.AddRange(columnas);
            for (int i = 0; i < _limites_decision_horas_prox_vuelo.Length; i++)
            {
                data.Rows.Add(new object[] { _limites_decision_pax[i], _limites_decision_horas_prox_vuelo[i] });
            }
            return data;
        }

        /// <summary>
        /// Transforma los valores de la matriz de decisión en valores de tabla
        /// </summary>
        /// <returns></returns>
        public DataTable InputMinutosEsperaToDataTable()
        {
            DataTable data = new DataTable("conex_espera");
            int dim = _limites_decision_horas_prox_vuelo.Length;
            DataColumn[] columnas = new DataColumn[dim + 2];
            columnas[0] = new DataColumn("Pax1");
            columnas[1] = new DataColumn("[0, h1]");
            for (int i = 1; i < dim; i++)
            {
                columnas[i + 1] = new DataColumn("]h" + i + ", h" + (i + 1) + "]");
            }
            columnas[dim + 1] = new DataColumn("]h" + dim + "..");
            data.Columns.AddRange(columnas);
            for (int i = 0; i <= dim; i++)
            {
                object[] obj = new object[dim + 2];
                if(i == 0)
                {
                    obj[0] = "[0, p1]";
                }
                else if(i==dim)
                {
                    obj[0] = "]p" + dim + "..";
                }
                else
                {
                    obj[0] = "]p" + i + ", p" + (i + 1) + "]";
                }
                for (int j = 0; j <= dim; j++)
                {
                    obj[j +1] = _matriz_minutos_espera[i, j];
                }
                data.Rows.Add(obj);
            }
            return data;
        }

        #endregion
    }
}
