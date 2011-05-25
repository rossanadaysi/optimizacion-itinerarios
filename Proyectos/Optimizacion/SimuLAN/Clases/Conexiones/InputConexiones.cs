using System;
using System.Collections.Generic;
using System.Text;
using SimuLAN.Utils;
using System.Data;

namespace SimuLAN.Clases
{
    /// <summary>
    /// Clase que encapsula el I/O de la información de conexiones
    /// </summary>
    public class InputConexiones
    {
        #region ATRIBUTES

        /// <summary>
        /// Controlador para calcular los minutos que se está dispuesto a esperar un vuelo en conexión
        /// </summary>
        private ControladorConexionesPax _controlador_conexiones_pax;

        /// <summary>
        /// Diccionario con los aeropuertos Hubs y sus atributos.
        /// </summary>
        private SerializableDictionary<string, int[]> _hubs;

        /// <summary>
        /// Diccionario con los pairings del itinerario
        /// </summary>
        private SerializableDictionary<int, ConexionPairing> _pairings;
        
        /// <summary>
        /// Diccionario con los pasajeros en conexión del itinerario
        /// </summary>
        private SerializableDictionary<int, ConexionPasajeros> _pax_conex;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Controlador para calcular los minutos que se está dispuesto a esperar un vuelo en conexión
        /// </summary>
        public ControladorConexionesPax ControladorConexionesPax
        {
            get { return _controlador_conexiones_pax; }
        }

        /// <summary>
        /// Diccionario con los aeropuertos Hubs y sus atributos
        /// </summary>
        public SerializableDictionary<string, int[]> Hubs
        {
            get { return _hubs; }
            set { _hubs = value; }
        }

        /// <summary>
        /// Diccionario con los pairings del itinerario
        /// </summary>
        public SerializableDictionary<int, ConexionPairing> Pairings
        {
            get { return _pairings; }
        }

        /// <summary>
        /// Diccionario con los pasajeros en conexión del itinerario
        /// </summary>
        public SerializableDictionary<int, ConexionPasajeros> PaxConex
        {
            get { return _pax_conex; }
        }
        
        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor
        /// </summary>
        public InputConexiones()
        {
            _pairings = new SerializableDictionary<int, ConexionPairing>();
            _pax_conex = new SerializableDictionary<int, ConexionPasajeros>();
            _hubs = new SerializableDictionary<string, int[]>();
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Crea tabla de datos para información de hubs
        /// </summary>
        /// <returns></returns>
        public DataTable InputHubsToDataTable()
        {
            DataTable data = new DataTable("hubs");
            DataColumn[] columnas = new DataColumn[4];
            columnas[0] = new DataColumn("Aeropuerto");
            columnas[1] = new DataColumn("Minutos conexion");
            columnas[2] = new DataColumn("Minutos activacion turno");
            columnas[3] = new DataColumn("DesfaseUTC");
            data.Columns.AddRange(columnas);
            foreach (string aep in _hubs.Keys)
            {
                object[] obj = new object[4];
                obj[0] = aep;
                for (int i = 0; i < 3; i++)
                {
                    obj[i + 1] = _hubs[aep][i];
                }
                data.Rows.Add(obj);
            }
            return data;
        }

        /// <summary>
        /// Crea tabla de datos para información de pairings
        /// </summary>
        /// <returns></returns>
        public DataTable InputPairingsToDataTable()
        {
            DataTable data = new DataTable("pairings");
            DataColumn[] columnas = new DataColumn[9];
            columnas[0] = new DataColumn("Vuelo 1");
            columnas[1] = new DataColumn("Vuelo 2");
            for (int i = 0; i < 7; i++)
            {
                columnas[i + 2] = new DataColumn(Utilidades.IntToDiaSemanaEnum(i).ToString());
            }
            data.Columns.AddRange(columnas);
            foreach (ConexionPairing pairing in _pairings.Values)
            {
                data.Rows.Add(pairing.PairingToRowData());
            }
            return data;
        }

        /// <summary>
        /// Crea tabla de datos para información de pasajeros en conexión
        /// </summary>
        /// <returns></returns>
        public DataTable InputPaxConexToDataTable()
        {
            DataTable data = new DataTable("conexiones");
            DataColumn[] columnas = new DataColumn[4];
            columnas[0] = new DataColumn("Num_Vuelo_1");
            columnas[1] = new DataColumn("Num_Vuelo2");
            columnas[2] = new DataColumn("Promedio_pax");
            columnas[3] = new DataColumn("Desvest_pax");
            data.Columns.AddRange(columnas);
            foreach (ConexionPasajeros conexion in _pax_conex.Values)
            {
                data.Rows.Add(new object[] { conexion.IdVuelo1.ToString(), conexion.IdVuelo2.ToString(), conexion.Paxs_Promedio, conexion.Pax_Desvest });
            }
            return data;
        }

        /// <summary>
        /// Busca la clave de un pairing dentro del diccionario de pairings existente
        /// </summary>
        /// <param name="conex">Conexión buscada</param>
        /// <returns></returns>
        public int KeyOfPairings(ConexionPairing conex)
        {
            foreach (int key in _pairings.Keys)
            {
                if (_pairings[key] == conex)
                {
                    return key;
                }
            }
            return -1;
        }

        /// <summary>
        /// Busca la clave de una conexión de pasajeros dentro del diccionario de conexiones de pasajeros existente
        /// </summary>
        /// <param name="conex">Conexión buscada</param>
        /// <returns></returns>
        public int KeyOfPaxs(ConexionPasajeros conex)
        {
            foreach (int key in _pax_conex.Keys)
            {
                if (_pax_conex[key] == conex)
                {
                    return key;
                }
            }
            return -1;
        }

        /// <summary>
        /// Obtiene información de hubs a partir de tabla de datos
        /// </summary>
        /// <param name="data">Tabla de datos</param>
        public void LlenarInputHubs(DataTable data)
        {
            foreach (DataRow row in data.Rows)
            {
                if (data.Rows.IndexOf(row) >= 1 && row[0].ToString().ToCharArray().Length > 0)
                {
                    object[] valores = row.ItemArray;
                    int[] valores_hub = new int[3];
                    valores_hub[0] = Convert.ToInt16(valores[1]);
                    valores_hub[1] = Convert.ToInt16(valores[2]);
                    valores_hub[2] = Convert.ToInt16(valores[3]);
                    _hubs.Add(valores[0].ToString(), valores_hub);
                }
            }
        }

        /// <summary>
        /// Crea objetos de pairings en conexión a partir de tabla de datos
        /// </summary>
        /// <param name="data">Tabla de datos</param>
        public void LlenarInputPairings(DataTable data)
        {
            int contadorPairings = 1;
            foreach (DataRow row in data.Rows)
            {
                if (data.Rows.IndexOf(row) >= 1 && row[0].ToString().ToCharArray().Length > 0)
                {
                    object[] valores = row.ItemArray;
                    ConexionPairing pairing = ConexionPairing.RowDataToPairing(valores);
                    _pairings.Add(contadorPairings, pairing);
                    contadorPairings++;
                }
            }
        }

        /// <summary>
        /// Crea objetos de pasajeros en conexión a partir de tabla de datos
        /// </summary>
        /// <param name="data">Tabla de datos</param>
        public void LlenarInputPaxConex(DataTable data)
        {
            int contadorConexiones = 1;
            foreach (DataRow row in data.Rows)
            {
                List<string> vuelos_agregados = new List<string>();
                if (data.Rows.IndexOf(row) >= 1 && row[0].ToString().ToCharArray().Length > 0)
                {
                    object[] valores = row.ItemArray;
                    string vuelo_1 = valores[0].ToString();
                    string vuelo_2 = valores[1].ToString();
                    string key_conexion = vuelo_1 + "-" + vuelo_2;
                    
                    double prom_pax = Convert.ToDouble(valores[2].ToString().Replace('.', ','));
                    double desv_pax = Convert.ToDouble(valores[3].ToString().Replace('.',','));
                    if (!vuelos_agregados.Contains(key_conexion))
                    {
                        ConexionPasajeros conexion_pax = new ConexionPasajeros(vuelo_1, vuelo_2, TipoConexion.Pasajeros, prom_pax, desv_pax);
                        _pax_conex.Add(contadorConexiones, conexion_pax);
                        vuelos_agregados.Add(key_conexion);
                        contadorConexiones++;
                    }
                }
            }
        }

        /// <summary>
        /// Obtiene información de parametros de decisión para vuelos en conexión a partir de tabla de datos
        /// </summary>
        /// <param name="data">Tabla de datos</param>
        public void LlenarParametrosDecisionVuelosConexion(DataTable data_parametros, DataTable data_minutos_espera)
        {
            List<int> col1 = new List<int>();
            List<int> col2 = new List<int>();
            foreach (DataRow row in data_parametros.Rows)
            {                
                if (data_parametros.Rows.IndexOf(row) >= 1 && row[0].ToString().ToCharArray().Length > 0)
                {
                    object[] valores = row.ItemArray;
                    col1.Add(Convert.ToInt16(valores[0]));
                    col2.Add(Convert.ToInt16(valores[1]));
                }
            }
            col1.Sort();
            col2.Sort();
            int dim = col1.Count + 1;
            int[] limites_horas_espera = col1.ToArray();
            int[] limites_decision_pax = col2.ToArray();
            int[,] minutos_espera = new int[dim,dim];
            int cont_fila = 0;
            foreach (DataRow row in data_minutos_espera.Rows)
            {
                if (data_minutos_espera.Rows.IndexOf(row) >= 1 && row[0].ToString().ToCharArray().Length > 0)
                {
                    object[] valores = row.ItemArray;                    
                    for (int i = 0; i < dim; i++)
                    {
                        minutos_espera[cont_fila,i] = Convert.ToInt16(valores[i + 1]);
                    }
                    cont_fila++;
                }
            }            

            this._controlador_conexiones_pax = new ControladorConexionesPax(limites_horas_espera, limites_decision_pax, minutos_espera);
        }

        #endregion
    }
}
