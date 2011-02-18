using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SimuLAN.Utils;
using System.Data;
using SimuLAN.Clases.Disrupciones;

namespace SimuLAN.Clases
{
    /// <summary>
    /// Clase que encapsula los parámetros que requiere SimuLAN
    /// </summary>
    public class ParametrosSimuLAN
    {
        #region ATRIBUTES

        /// <summary>
        /// Información de conexiones.
        /// </summary>
        private InputConexiones _conexiones;

        /// <summary>
        /// Parámetros simples de SimuLAN
        /// </summary>
        private ParametrosEscalares _escalares;

        /// <summary>
        /// Diccionario que guarda la información de los AOG
        /// </summary>
        private SerializableDictionary<string, DataDisrupcion> _info_AOG;

        /// <summary>
        /// Diccionario que guarda la información de los grupos de flota
        /// </summary>
        private SerializableDictionary<string, GrupoFlota> _info_grupos_flotas;

        /// <summary>
        /// Tabla que relaciona Flotas con Grupos de Flotas.
        /// </summary>
        private SerializableDictionaryWithHeaders _map_grupos_flotas;

        /// <summary>
        /// Tabla que relaciona Flotas con AcTypes.
        /// </summary>
        private SerializableDictionaryWithHeaders _map_flotas;

        /// <summary>
        /// Diccionario con la información que liga una ruta comercial o negocio a un tramo en formato Operador&NumVuelo&ParOD
        /// </summary>
        private SerializableDictionaryWithHeaders2D _map_vuelos_rutas;

        /// <summary>
        /// Tabla que relaciona SubFlotas con Matrículas (prefijos).
        /// </summary>
        private SerializableDictionaryWithHeaders _map_subFlotas_matriculas; 

        /// <summary>
        /// Matriz de compatibilidad operacional entre flotas para restringir swaps algoritmo de recovery.
        /// </summary>
        private SerializableDictionary<string, SerializableDictionary<string, double>> _matriz_flota_flota;

        /// <summary>
        /// Matriz de compatibilidad operacional entre subflota y operador comercial para restringir swaps en algoritmo de recovery.
        /// </summary>
        private SerializableDictionary<string, SerializableDictionary<string, int>> _matriz_multioperador;

        /// <summary>
        /// Diccionario con los Turn Around Mínimos especiales por tramo
        /// </summary>
        private SerializableDictionary<int, int> _turn_around_custom;

        /// <summary>
        /// Infomración de tiempos mínimos de Turn Around.
        /// </summary>
        private SerializableDictionaryWithHeaders2D _turn_around_min;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Información de pairings y vuelos en conexion
        /// </summary>
        public InputConexiones Conexiones
        {
            get { return _conexiones; }
            set { _conexiones = value; }
        }

        /// <summary>
        /// Parámetros simples de SimuLAN
        /// </summary>
        public ParametrosEscalares Escalares
        {
            get { return _escalares; }
            set { _escalares = value; }
        }

        /// <summary>
        /// Diccionario que guarda la información de los AOG
        /// </summary>
        public SerializableDictionary<string, DataDisrupcion> InfoAOG
        {
            get { return _info_AOG; }
            set { _info_AOG = value; }
        }

        /// <summary>
        /// Diccionario que guarda la información de los grupos de flota
        /// </summary>
        public SerializableDictionary<string, GrupoFlota> InfoGruposFlotas
        {
            get { return _info_grupos_flotas; }
            set { _info_grupos_flotas = value; }
        }

        /// <summary>
        /// Tabla que relaciona Flotas con AcTypes
        /// </summary>
        public SerializableDictionaryWithHeaders MapFlotas
        {
            get { return _map_flotas; }
            set { _map_flotas = value; }
        }

        /// <summary>
        /// Tabla que relaciona Flotas con Grupos de Flotas.
        /// </summary>
        public SerializableDictionaryWithHeaders MapGruposFlotas
        {
            get { return _map_grupos_flotas; }
            set { _map_grupos_flotas = value; }
        }

        /// <summary>
        /// Tabla que relaciona SubFlotas con Matrículas (prefijos)
        /// </summary>
        public SerializableDictionaryWithHeaders MapSubFlotasMatriculas
        {
            get { return _map_subFlotas_matriculas; }
            set { _map_subFlotas_matriculas = value; }
        }

        /// <summary>
        /// Diccionario con la información que liga una ruta comercial o negocio a un tramo en formato Operador&NumVuelo&ParOD
        /// </summary>
        public SerializableDictionaryWithHeaders2D MapVuelosRutas
        {
            get { return _map_vuelos_rutas; }
            set { _map_vuelos_rutas = value; }
        }

        /// <summary>
        /// Matriz de compatibilidad operacional entre flotas para algoritmo de recovery
        /// </summary>
        public SerializableDictionary<string, SerializableDictionary<string, double>> MatrizFlotaFlota
        {
            get { return _matriz_flota_flota; }
            set { _matriz_flota_flota = value; }
        }

        /// <summary>
        /// Matriz de compatibilidad operacional entre subflota y operador comercial para restringir swaps en algoritmo de recovery.
        /// </summary>
        public SerializableDictionary<string, SerializableDictionary<string, int>> MatrizMultioperador
        {
            get { return _matriz_multioperador; }
            set { _matriz_multioperador = value; }
        }

        /// <summary>
        /// Infomración de tiempos mínimos de Turn Around
        /// </summary>
        public SerializableDictionaryWithHeaders2D TurnAroundMin
        {
            get { return _turn_around_min; }
            set { _turn_around_min = value; }
        }
     
        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Constructor de objeto ParametrosSimuLAN.
        /// </summary>        
        public ParametrosSimuLAN(ParametrosEscalares escalares)
        {
            this._escalares = escalares;
            string[][] headers = GetHeadersParametros();
            this._map_flotas = new SerializableDictionaryWithHeaders(headers[0],"mapFlotas");
            this._turn_around_min = new SerializableDictionaryWithHeaders2D(headers[1],"turnAround");
            this._matriz_flota_flota = new SerializableDictionary<string, SerializableDictionary<string, double>>();
            this._matriz_multioperador = new SerializableDictionary<string, SerializableDictionary<string, int>>();
            this._conexiones = new InputConexiones();
            this._map_grupos_flotas = new SerializableDictionaryWithHeaders(headers[5], "mapGruposFlotas");
            this._map_subFlotas_matriculas = new SerializableDictionaryWithHeaders(headers[6], "mapSubFlotasMatriculas");
            this._info_grupos_flotas = new SerializableDictionary<string, GrupoFlota>();
            this._map_vuelos_rutas = new SerializableDictionaryWithHeaders2D(headers[2], "vuelos_rutas");
            this._turn_around_custom = new SerializableDictionary<int, int>();
            this._info_AOG = new SerializableDictionary<string, DataDisrupcion>();
        }

        #endregion 

        #region PUBLIC METHODS

        /// <summary>
        /// Agrega un turn around customizado
        /// </summary>
        /// <param name="idTramo">Id del tramo de vuelo posterior al turn around</param>
        /// <param name="turnAround">Tiempo mínimo de T/A</param>
        public void AgregarTurnAroundTramo(int idTramo, int turnAround)
        {
            if (_turn_around_custom.ContainsKey(idTramo))
            {
                _turn_around_custom[idTramo] = turnAround;
            }
            else
            {
                _turn_around_custom.Add(idTramo, turnAround);
            }
        }
        
        /// <summary>
        /// Busca la Tabla de nombreTabla
        /// </summary>
        /// <param name="nombreTabla">Nombre de la tabla buscada</param>
        /// <param name="dim">Cantidad de columnas útiles de la tabla</param>
        /// <returns></returns>
        public DataTable BuscarTabla(string nombreTabla, out int dim)
        {
            dim = 0;
            if (_map_flotas.Nombre == nombreTabla)
            {
                dim = _map_flotas.Headers.Length;
                return _map_flotas.Data;
            }
            else if (_map_grupos_flotas.Nombre == nombreTabla)
            {
                dim = _map_grupos_flotas.Headers.Length;
                return _map_grupos_flotas.Data;
            }
            else if (_map_subFlotas_matriculas.Nombre == nombreTabla)
            {
                dim = _map_subFlotas_matriculas.Headers.Length;
                return _map_subFlotas_matriculas.Data;
            }
            else if (_map_vuelos_rutas.Nombre == nombreTabla)
            {
                dim = _map_vuelos_rutas.Headers.Length;
                return _map_vuelos_rutas.Data;
            }
            else if (_turn_around_min.Nombre == nombreTabla)
            {
                dim = _turn_around_min.Headers.Length;
                return _turn_around_min.Data;
            }
            return null;
        }

        /// <summary>
        /// Carga información de AOG
        /// </summary>
        /// <param name="data">Tabla con la información de AOG's</param>
        public void CargarInfoAOG(DataTable data)
        {
            foreach (DataRow row in data.Rows)
            {
                if (data.Rows.IndexOf(row) >= 1 && row[0].ToString().ToCharArray().Length > 0)
                {
                    object[] valores = row.ItemArray;
                    string key = valores[0].ToString() + "_" + valores[1].ToString() + "_" + valores[2].ToString();
                    DataDisrupcion info = new DataDisrupcion(Convert.ToDouble(valores[3].ToString().Replace('.', ',')), Convert.ToDouble(valores[4].ToString().Replace('.', ',')), Convert.ToDouble(valores[5].ToString().Replace('.', ',')));
                    _info_AOG.Add(key, info);
                }
            }
        }
        
        /// <summary>
        /// Carga data de grupos de flota
        /// </summary>
        /// <param name="data">Tabla con información de los grupos</param>
        public void CargarInputGruposFlotas(DataTable data)
        {
            foreach (DataRow row in data.Rows)
            {
                if (data.Rows.IndexOf(row) >= 1 && row[0].ToString().ToCharArray().Length > 0)
                {
                    object[] valores = row.ItemArray;
                    string grupo = valores[0].ToString();
                    int n1 = Convert.ToInt16(valores[1]);
                    int n2 = Convert.ToInt16(valores[2]);
                    _info_grupos_flotas.Add(grupo, new GrupoFlota(grupo, n1, n2));
                }
            }
        }

        /// <summary>
        /// Carga matriz flota-flota desde un DataTable con la información
        /// </summary>
        /// <param name="data">Tabla con la información</param>
        public void CargarMatrizFlotaFlota(DataTable data)
        {
            _matriz_flota_flota.Clear();
            object[] headersFlotas = data.Rows[0].ItemArray;
            int contador = 1;
            while (headersFlotas[contador].ToString().ToCharArray().Length > 0)
            {
                _matriz_flota_flota.Add(headersFlotas[contador].ToString(), new SerializableDictionary<string, double>());
                contador++;
            }

            foreach (DataRow row in data.Rows)
            {
                if (data.Rows.IndexOf(row) >= 1 && row[0].ToString().ToCharArray().Length > 0)
                {
                    object[] valores = row.ItemArray;
                    string flota1 = valores[0].ToString();
                    contador = 1;
                    foreach (string flota2 in _matriz_flota_flota.Keys)
                    {
                        _matriz_flota_flota[flota1].Add(flota2, Convert.ToDouble(valores[contador].ToString().Replace('.',',')));
                        contador++;
                    }
                }
            }
        }

        /// <summary>
        /// Carga matriz flota-flota desde un DataTable con la información
        /// </summary>
        /// <param name="data">Tabla con la información</param>
        /// <param name="flotas">Diccionario de flotas</param>
        public void CargarMatrizFlotaFlota(DataTable data, List<string> flotas)
        {
            _matriz_flota_flota.Clear();
            int contador;
            foreach(string flota in flotas)
            {
                _matriz_flota_flota.Add(flota, new SerializableDictionary<string, double>());
            }
            foreach (DataRow row in data.Rows)
            {
                if (row[0].ToString().ToCharArray().Length > 0)
                {
                    object[] valores = row.ItemArray;
                    string flota1 = valores[0].ToString();
                    contador = 1;
                    foreach (string flota2 in _matriz_flota_flota.Keys)
                    {
                        _matriz_flota_flota[flota1].Add(flota2, Convert.ToDouble(valores[contador].ToString().Replace('.', ',')));
                        contador++;
                    }
                }
            }
        }

        /// <summary>
        /// Carga matriz multioperador desde un DataTable con la información
        /// </summary>
        /// <param name="data">Tabla con la información</param>
        public void CargarMatrizMultioperador(DataTable data)
        {
            _matriz_multioperador.Clear();
            object[] headers = data.Rows[0].ItemArray;
            Dictionary<int, string> operadores = new Dictionary<int, string>();
            int contador = 1;
            while (headers[contador].ToString().ToCharArray().Length > 0)
            {
                operadores.Add(contador, headers[contador].ToString());
                contador++;
            }

            foreach (DataRow row in data.Rows)
            {
                if (data.Rows.IndexOf(row) >= 1 && row[0].ToString().ToCharArray().Length > 0)
                {
                    object[] valores = row.ItemArray;
                    string subflota = valores[0].ToString();
                    _matriz_multioperador.Add(subflota, new SerializableDictionary<string, int>());
                    contador = 1;
                    while (contador<valores.Length && valores[contador].ToString().ToCharArray().Length > 0)
                    {
                        _matriz_multioperador[subflota].Add(operadores[contador], Convert.ToInt16(valores[contador]));
                        contador++;
                    }
                }
            }
        }

        /// <summary>
        /// Carga matriz multioperador desde un DataTable con la información
        /// </summary>
        /// <param name="data">Tabla con la información</param>
        /// <param name="operadores">Diccionario de operadores</param>
        public void CargarMatrizMultioperador(DataTable data, Dictionary<int, string> operadores)
        {
            _matriz_multioperador.Clear();
            foreach (DataRow row in data.Rows)
            {
                if (row[0].ToString().ToCharArray().Length > 0)
                {
                    object[] valores = row.ItemArray;
                    string subflota = valores[0].ToString();
                    _matriz_multioperador.Add(subflota, new SerializableDictionary<string, int>());
                    int contador = 1;
                    while (contador<valores.Length && valores[contador].ToString().ToCharArray().Length > 0)
                    {
                        _matriz_multioperador[subflota].Add(operadores[contador], Convert.ToInt16(valores[contador]));
                        contador++;
                    }
                }
            }
        }

        /// <summary>
        /// Retorna el operador número "value" de la matriz multioperador
        /// </summary>
        /// <param name="value">Posición buscada</param>
        /// <returns></returns>
        public string ColumnaMultioperador(int value)
        {
            int counter = 0;
            SerializableDictionary<string, int> dict = _matriz_multioperador[IndiceFilaMultioperador(0)];
            if (dict != null && dict.Count > 0)
            {
                foreach (string operador in dict.Keys)
                {
                    if (counter == value)
                    {
                        return operador;
                    }
                    counter++;
                }
            }
            return "None";
        }
        
        /// <summary>
        /// Retorna delegado que encapsula método para obtener T/A mínimo
        /// </summary>
        public GetTurnAroundMinEventHandler GetDelegateTurnAroundMin()
        {
            return new GetTurnAroundMinEventHandler(GetTurnAroundMin);
        }
        
        /// <summary>
        /// Retorna la subFlota número "value" de la matriz multioperador
        /// </summary>
        /// <param name="value">Posición buscada</param>
        /// <returns></returns>
        public string IndiceFilaMultioperador(int value)
        {
            int counter = 0;
            foreach (string subFlota in _matriz_multioperador.Keys)
            {
                if (counter == value)
                {
                    return subFlota;
                }
                counter++;
            }
            return null;
        }
        
        /// <summary>
        /// Retorna la flota número "value" de la matriz flota-flota
        /// </summary>
        /// <param name="value">Posición buscada</param>
        /// <returns></returns>
        public string IndiceFlotaFlota(int value)
        {
            int counter = 0;
            foreach (string flota in _matriz_flota_flota.Keys)
            {
                if (counter == value)
                {
                    return flota;
                }
                counter++;
            }
            return "None";
        }

        /// <summary>
        /// Retorna una tabla con la info de AOG
        /// </summary>
        public DataTable InfoAOGToDataTable()
        {
            DataTable data = new DataTable("AOG");
            DataColumn[] columnas = new DataColumn[6];
            columnas[0] = new DataColumn("Flota");
            columnas[1] = new DataColumn("Estación");
            columnas[2] = new DataColumn("Mes");
            columnas[3] = new DataColumn("Probabilidad");
            columnas[4] = new DataColumn("Promedio");
            columnas[5] = new DataColumn("Desvación");
            data.Columns.AddRange(columnas);
            foreach (string key in _info_AOG.Keys)
            {
                object[] o = new object[6];
                string[] s = key.Split('_');
                o[0] = s[0];
                o[1] = s[1];
                o[2] = s[2];
                o[3] = _info_AOG[key].Prob;
                o[4] = _info_AOG[key].Media;
                o[5] = _info_AOG[key].Desvest;
                data.Rows.Add(o);
            }
            return data;
        }

        /// <summary>
        /// Retorna una tabla con los grupos de flota
        /// </summary>
        /// <returns></returns>
        public DataTable InfoGruposFlotasToDataTable()
        {
            DataTable data = new DataTable("turnos");
            DataColumn[] columnas = new DataColumn[3];
            columnas[0] = new DataColumn("Grupo");
            columnas[1] = new DataColumn("TurnosManana");
            columnas[2] = new DataColumn("TurnosTarde");
            data.Columns.AddRange(columnas);
            foreach (GrupoFlota g in _info_grupos_flotas.Values)
            {
                data.Rows.Add(new object[] { g.Nombre, g.Turnos_Manana, g.Turnos_Tarde });
            }
            return data;
        }

        /// <summary>
        /// Limpia diccionario de Turn Around Mínimos especiales por tramo
        /// </summary>
        public void LimpiarTurnAroundCustom()
        {
            _turn_around_custom.Clear();
        }

        /// <summary>
        /// Retorna un tabla con la matriz flota-flota
        /// </summary>
        /// <returns></returns>
        public DataTable MatrizFlotaFlotaToDataTable()
        {
            Dictionary<int, string> flotas = new Dictionary<int, string>();
            int contador = 1;
            foreach (string flota in _matriz_flota_flota.Keys)
            {
                flotas.Add(contador, flota);
                contador++;
            }
            DataTable data = new DataTable("matrizFlotaFlota");
            DataColumn[] columnas = new DataColumn[_matriz_flota_flota.Count + 1];
            for (int i = 0; i < columnas.Length; i++)
            {
                if (i == 0)
                {
                    columnas[i] = new DataColumn("");
                }
                else
                {
                    columnas[i] = new DataColumn(flotas[i]);
                }
            }
            data.Columns.AddRange(columnas);
            foreach (string flota1 in _matriz_flota_flota.Keys)
            {
                int contador2 = 1;
                object[] obj = new object[flotas.Count + 1];
                obj[0] = flota1;
                foreach (string flota2 in _matriz_flota_flota[flota1].Keys)
                {
                    obj[contador2] = _matriz_flota_flota[flota1][flota2];
                    contador2++;
                }
                data.Rows.Add(obj);
            }
            return data;
        }

        /// <summary>
        /// Retorna una tabla con la matriz multioperador
        /// </summary>
        /// <returns></returns>
        public DataTable MatrizMultioperadorToDataTable()
        {
            Dictionary<int, string> operadores = new Dictionary<int, string>();
            int contador = 1;
            foreach (string subflota in _matriz_multioperador.Keys)
            {
                foreach (string operador in _matriz_multioperador[subflota].Keys)
                {
                    operadores.Add(contador, operador);
                    contador++;
                }
                break;
            }
            DataTable data = new DataTable("matrizMultioperador");
            DataColumn[] columnas = new DataColumn[operadores.Count + 1];
            for (int i = 0; i < columnas.Length; i++)
            {
                if (i == 0)
                {
                    columnas[i] = new DataColumn("Subflota\\Operador");
                }
                else
                {
                    columnas[i] = new DataColumn(operadores[i]);
                }
            }
            data.Columns.AddRange(columnas);
            foreach (string subflota in _matriz_multioperador.Keys)
            {
                int contador2 = 1;
                object[] obj = new object[operadores.Count + 1];
                obj[0] = subflota;
                foreach (string op in _matriz_multioperador[subflota].Keys)
                {
                    obj[contador2] = _matriz_multioperador[subflota][op];
                    contador2++;
                }
                data.Rows.Add(obj);
            }
            return data;
        }

        /// <summary>
        /// Recarga los valores de los diccionarios asociados a DataTables
        /// </summary>
        public void Refresh()
        {
            _turn_around_min.Refresh();
            _map_flotas.Refresh();
            _map_grupos_flotas.Refresh();
            _map_subFlotas_matriculas.Refresh();
            _map_vuelos_rutas.Refresh();
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Retorna los encabezados de las columnas de las tablas de parámetros.
        /// </summary>
        private string[][] GetHeadersParametros()
        {
            string[][] retorno = new string[7][];
            retorno[0] = new string[2];
            retorno[0][0] = "Ac Type";
            retorno[0][1] = "Flota";
            retorno[1] = new string[3];
            retorno[1][0] = "Flota";
            retorno[1][1] = "Estación";
            retorno[1][2] = "Minutos";
            retorno[2] = new string[3];
            retorno[2][0] = "Code";
            retorno[2][1] = "Negocio";
            retorno[2][2] = "HUB Salida";

            retorno[3] = new string[2];
            retorno[3][0] = "Estación";
            retorno[3][1] = "Valor";

            retorno[4] = new string[2];
            retorno[4][0] = "Código";
            retorno[4][1] = "Número de Vuelo";

            retorno[5] = new string[2];
            retorno[5][0] = "Flota";
            retorno[5][1] = "Grupo";

            retorno[6] = new string[2];
            retorno[6][0] = "SubFlota";
            retorno[6][1] = "Matrícula";

            return retorno;
        }

        /// <summary>
        /// Retorna el T/A Mínimo para el tramo objetivo.
        /// </summary>
        /// <param name="tramo">Tramo objetivo</param>
        /// <returns>Minutos de T/A mínimo</returns>
        private int GetTurnAroundMin(Tramo tramo)
        {
            int taDict = 0;
            if (tramo == null)
            {
                throw new Exception("Error al obtener T/A mínimo");
            }
            else if (_turn_around_custom.ContainsKey(tramo.TramoBase.Numero_Global))
            {
                return _turn_around_custom[tramo.TramoBase.Numero_Global];
            }
            else if (tramo.FlotaOperada != null && tramo.TramoBase.Origen != null)
            {
                if (this != null && this.TurnAroundMin != null && this.TurnAroundMin.Dict != null)
                {
                    if (this.TurnAroundMin.Dict.ContainsKey(tramo.FlotaOperada))
                    {
                        if (this.TurnAroundMin.Dict[tramo.FlotaOperada].ContainsKey(tramo.TramoBase.Origen))
                        {
                            taDict = Convert.ToInt32(this._turn_around_min.Dict[tramo.FlotaOperada][tramo.TramoBase.Origen]);
                        }
                    }
                }
                int taProg = 1000;
                if (tramo.Tramo_Previo != null)
                {
                    if (tramo.IdAvionProgramadoActual == tramo.Tramo_Previo.IdAvionProgramadoActual)
                    {
                        taProg = tramo.TInicialProg - tramo.Tramo_Previo.TFinalProg;
                    }
                    else
                    {
                        taProg = -1000;
                    }
                }
                else
                {
                    taProg = tramo.TInicialRst;
                }
                int taMin = Math.Min(taProg, taDict);
                return taMin;
            }
            else
            {
                return 0;
            }
        }
        
        #endregion
    }
}
