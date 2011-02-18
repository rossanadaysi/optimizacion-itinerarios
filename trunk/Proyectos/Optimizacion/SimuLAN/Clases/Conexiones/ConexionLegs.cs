using System;
using System.Collections.Generic;
using System.Text;
using SimuLAN.Utils;

namespace SimuLAN.Clases
{
    /// <summary>
    /// Conexión entre dos legs
    /// </summary>
    public class ConexionLegs
    {
        #region ATRIBUTES
        
        /// <summary>
        /// Conexión base
        /// </summary>
        private Conexion _conexion_base;

        /// <summary>
        /// Estación donde se efectúa la conexión
        /// </summary>
        private string _estacion_conexion;

        /// <summary>
        /// Delegado para obtener los minutos de espera por conexión
        /// </summary>
        private GetMinutosEsperaConexionEventHandler _get_minutos_espera;

        /// <summary>
        /// Delegado para obtener un tramo en función de su número global
        /// </summary>
        private GetTramoEventHandler _get_tramo;

        /// <summary>
        /// Número global de tramo final
        /// </summary>
        private int _num_tramo_fin;

        /// <summary>
        /// Número global de tramo inicial
        /// </summary>
        private int _num_tramo_ini;

        /// <summary>
        /// Pasajeros de la conexión
        /// </summary>
        private double _pasajeros_en_conexion;

        /// <summary>
        /// Tiempo de inicial de tramo final
        /// </summary>
        private int _tiempo_ini_tramo_fin;

        /// <summary>
        /// Tiempo de término de tramo inicial
        /// </summary>
        private int _tiempo_fin_tramo_ini;
        
        /// <summary>
        /// Tipo de conexión: pairing o pasajeros
        /// </summary>
        private TipoConexionAvion _tipo_conexion_avion;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Conexión base
        /// </summary>
        public Conexion ConexionBase
        {
            get { return _conexion_base; }
            set { _conexion_base = value; }
        }

        /// <summary>
        /// Delegado para obtener un tramo en función de su número global
        /// </summary>
        public GetTramoEventHandler GetTramo
        {
            get { return _get_tramo; }
            set { _get_tramo = value; }
        }

        /// <summary>
        /// Indica si los tramos de conexión son asignados al mismo avión.
        /// </summary>
        public bool MismoAvion
        {
            get { return (_get_tramo(_num_tramo_ini).IdAvionProgramado == _get_tramo(_num_tramo_fin).IdAvionProgramado); }
        }

        /// <summary>
        /// Número global de tramo final
        /// </summary>
        public int NumTramoFin
        {
            get { return _num_tramo_fin; }
            set { _num_tramo_fin = value; }
        }

        /// <summary>
        /// Número global de tramo inicial
        /// </summary>
        public int NumTramoIni
        {
            get { return _num_tramo_ini; }
            set { _num_tramo_ini = value; }
        }

        /// <summary>
        /// Total de pasajeros conectados
        /// </summary>
        public double PasajerosConectados
        {
            get { return _pasajeros_en_conexion; }
            set { _pasajeros_en_conexion = value; }
        }

        /// <summary>
        /// Minutos de holgura en la conexión
        /// </summary>
        public int Separacion
        {
            get { return _tiempo_ini_tramo_fin - _tiempo_fin_tramo_ini; }
        }

        /// <summary>
        /// Tiempo de inicial de tramo final
        /// </summary>
        public int TiempoIniTramoFin
        {
            get { return _tiempo_ini_tramo_fin; }
            set { _tiempo_ini_tramo_fin = value; }
        }

        /// <summary>
        /// Tiempo de término de tramo inicial
        /// </summary>
        public int TiempoFinTramoIni
        {
            get { return _tiempo_fin_tramo_ini; }
            set { _tiempo_fin_tramo_ini = value; }
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tramoPrevio">Referencia a tramo inicial</param>
        /// <param name="tramoSiguiente">Referencia a tramo final</param>
        /// <param name="conexionBase">Conexión base</param>
        public ConexionLegs(Tramo tramoPrevio, Tramo tramoSiguiente, Conexion conexionBase)
        {
            this._num_tramo_ini = tramoPrevio.TramoBase.Numero_Global;
            this._tiempo_fin_tramo_ini = tramoPrevio.TFinalProg;
            this._num_tramo_fin = tramoSiguiente.TramoBase.Numero_Global;
            this._tiempo_ini_tramo_fin = tramoSiguiente.TInicialProg;
            this._conexion_base = conexionBase;
            this._estacion_conexion = tramoPrevio.TramoBase.Destino;
            this._pasajeros_en_conexion = 0;
            if (tramoPrevio.IdAvionProgramado == tramoSiguiente.IdAvionProgramado)
            {
                this._tipo_conexion_avion = TipoConexionAvion.MantieneAvion;
            }
            else
            {
                this._tipo_conexion_avion = TipoConexionAvion.CambiaAvion;
            }            
        }

        #endregion

        #region INTERNAL METHODS

        /// <summary>
        /// Busca la conexión crítica de una lista de conexiones. 
        /// Se defien como conexión crítica a la de menor holgura.
        /// </summary>
        /// <param name="conexiones">Lista de conexiones. Tienen en común el segundo tramo de la conexión</param>
        /// <returns></returns>
        internal static ConexionLegs BuscaConexionCriticaPairings(SerializableList<ConexionLegs> conexiones)
        {
            int t_ini = int.MinValue;
            ConexionLegs conexion_critica = null;
            foreach (ConexionLegs c in conexiones)
            {
                if (c.TiempoFinTramoIni > t_ini)
                {
                    conexion_critica = c;
                    t_ini = c.TiempoFinTramoIni;
                }
            }
            return conexion_critica;
        }

        /// <summary>
        /// Indica si una conexión cumple con ser de cierto tipo y tener cierto un número de vuelo 
        /// al inicio o al final de la conexión.
        /// </summary>
        /// <param name="num_tramo_global">Número de vuelo buscado</param>
        /// <param name="tipoConexion">Tipo de conexión</param>
        /// <param name="segundoTramo">True si se busca en el tramo final de la conexión</param>
        /// <returns></returns>
        internal bool ConexionCumpleCondicion(int num_tramo_global, TipoConexion tipoConexion, bool segundoTramo)
        {
            if (this.ConexionBase.Tipo == tipoConexion)
            {
                if (segundoTramo)
                {
                    if (this.NumTramoFin == num_tramo_global)
                    {
                        return true;
                    }
                }
                else
                {
                    if (this.NumTramoIni == num_tramo_global)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
     
        /// <summary>
        /// Obtiene la cantidad de minutos máxima que se está dispuesto a esperar en una conexión de pasajeros.
        /// </summary>
        /// <param name="min_hasta_proximo_vuelo">Minutos hasta el próximo vuelo (Origen-Destino)</param>
        /// <returns></returns>
        internal int GetEspera(int min_hasta_proximo_vuelo)
        {
            return _get_minutos_espera(_pasajeros_en_conexion, min_hasta_proximo_vuelo);
        }
        
        /// <summary>
        /// Setea el delagado GetMinutosEsperaConexionEventHandler
        /// </summary>
        /// <param name="del">Referncia a delegado GetMinutosEsperaConexionEventHandler</param>
        internal void SetDelegate(GetMinutosEsperaConexionEventHandler del)
        {
            this._get_minutos_espera = del;
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Sobreescribe ToString()        
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _conexion_base.Tipo + " " + _estacion_conexion + " " + NumTramoIni + " - " + NumTramoFin + "( " + Separacion + ", " + _tipo_conexion_avion + ")";
        }

        #endregion
    }

    /// <summary>
    /// Clase que encapsula información de una conexión (ConexionLegs) y el tiempo máximo que se está 
    /// dispuesto a esperar la conexión (se asume que es de pasajeros).
    /// </summary>
    public class ConexionesConTiempoMaximoEspera: IComparable
    {
        #region ATRIBUTES

        /// <summary>
        /// Conexión base
        /// </summary>
        public ConexionLegs _conexion_base;

        /// <summary>
        /// Tiempo máximo de espera. Atraso máximo que se está dispuesto a asumir
        /// </summary>
        public int _tiempo_maximo_espera;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="conexion_base">Conexión base</param>
        /// <param name="tiempo_maximo_espera">Tiempo máximo de espera. Atraso máximo que se está dispuesto a asumir</param>
        public ConexionesConTiempoMaximoEspera(ConexionLegs conexion_base, int tiempo_maximo_espera)
        {
            this._conexion_base = conexion_base;
            this._tiempo_maximo_espera = tiempo_maximo_espera;
        }

        #endregion

        #region PUBLIC METHODS

        #region IComparable Members

        /// <summary>
        /// Compara la instancia actual con el objeto obj.
        /// </summary>
        /// <param name="obj">Objeto a ser comparado</param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            ConexionesConTiempoMaximoEspera con = (ConexionesConTiempoMaximoEspera)obj;
            if (this._tiempo_maximo_espera < con._tiempo_maximo_espera) return 1;
            else if (this._tiempo_maximo_espera > con._tiempo_maximo_espera) return -1;
            else return 0;
        }

        #endregion

        #endregion
    }
}
