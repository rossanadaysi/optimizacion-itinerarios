using System;
using System.Collections.Generic;
using System.Text;

namespace SimuLAN.Clases
{
    /// <summary>
    /// Este objeto representa un tramo con la información directa leída desde el archivo xls de itinerario.
    /// </summary>
    public class TramoBase: ICloneable
    {
        #region CONSTANTS
        
        /// <summary>
        /// Código especial para detectar si un slot es de mantenimiento
        /// </summary>
        private const string CODIGO_MANTTO = "Z";
        
        /// <summary>
        /// Codigo especial para detectar si un slot es de backup
        /// </summary>
        private const string CODIGO_BACKUP = "BU";
        
        #endregion

        #region ATRIBUTES

        /// <summary>
        /// Donde esta matriculado el avión. En ocaciones se aplicará donde esta ubicado el avión.
        /// </summary>
        private string _ac_owner;

        /// <summary>
        /// Tipo de avión. Material específico utilizado.
        /// </summary>
        private string _ac_type;

        /// <summary>
        /// Carrier publicado. Tiene relación con la comercialización del tramo.
        /// </summary>
        private string _carrier;
        
        /// <summary>
        /// Configuración de asientos
        /// </summary>
        private string _config_asientos;
        
        /// <summary>
        /// Destino del tramo
        /// </summary>
        private string _destino;

        /// <summary>
        /// Operación Doméstica / Internacional
        /// </summary>
        private string _dom_int;       
        
        /// <summary>
        /// Fecha de llegada
        /// </summary>
        private DateTime _fecha_llegada;

        /// <summary>
        /// Fecha de despegue
        /// </summary>
        private DateTime _fecha_salida;

        /// <summary>
        /// Hora de llegada
        /// </summary>
        private string _hora_llegada;

        /// <summary>
        /// Hora de salida
        /// </summary>
        private string _hora_salida;

        /// <summary>
        /// Número de avión
        /// </summary>
        private string _numero_ac;

        /// <summary>
        /// Contador global del tramo. Es único por cada tramo del itinerario.
        /// </summary>
        private int _numero_global;
        
        /// <summary>
        /// Contador de tramo dentro de la matrícula
        /// </summary>
        private int _numero_tramo;
        
        /// <summary>
        /// Número de vuelo
        /// </summary>
        private string _numero_vuelo;

        /// <summary>
        /// Sufijo operacional
        /// </summary>
        private string _op_suf;

        /// <summary>
        /// Origen del tramo
        /// </summary>
        private string _origen;
        
        /// <summary>
        /// Service Type Code
        /// </summary>
        private string _stc;
        
        /// <summary>
        /// Subflota. Clasificación especial del avión.
        /// </summary>
        private string _num_subflota;
        
        #endregion

        #region PROPERTIES
        
        /// <summary>
        /// Donde esta matriculado el avión. En ocaciones se aplicará donde esta ubicado el avión.
        /// </summary>
        public string Ac_Owner { get { return _ac_owner; } set { _ac_owner = value; } }

        /// <summary>
        /// Tipo de avión. Material específico utilizado.
        /// </summary>
        public string AcType { get { return _ac_type; } set { _ac_type = value; } }    

        /// <summary>
        /// Carrier publicado. Tiene relación con la comercialización del tramo.
        /// </summary>
        public string Carrier { get { return _carrier; } set { _carrier = value; } }

        /// <summary>
        /// Configuración de asientos
        /// </summary>
        public string Config_Asientos { get { return _config_asientos; } set { _config_asientos = value; } }

        /// <summary>
        /// Destino del tramo
        /// </summary>
        public string Destino { get { return _destino; } set { _destino = value; } }
        
        /// <summary>
        /// Operación Doméstica / Internacional
        /// </summary>
        public string Dom_Int { get { return _dom_int; } set { _dom_int = value; } }

        /// <summary>
        /// Fecha de llegada
        /// </summary>
        public DateTime Fecha_Llegada { get { return _fecha_llegada; } set { _fecha_llegada = value; } }

        /// <summary>
        /// Fecha de despegue
        /// </summary>
        public DateTime Fecha_Salida { get { return _fecha_salida; } set { _fecha_salida = value; } }

        /// <summary>
        /// Hora de llegada
        /// </summary>
        public string Hora_Llegada { get { return _hora_llegada; } set { _hora_llegada = value; } }

        /// <summary>
        /// Hora de salida
        /// </summary>
        public string Hora_Salida { get { return _hora_salida; } set { _hora_salida = value; } }

        /// <summary>
        /// Número de avión
        /// </summary>
        public string Numero_Ac { get { return _numero_ac; } set { _numero_ac = value; } }

        /// <summary>
        /// Contador global del tramo. Es único por cada tramo del itinerario.
        /// </summary>
        public int Numero_Global { get { return _numero_global; } set { _numero_global = value; } }

        /// <summary>
        /// Número de Subflota. Clasificación especial del avión.
        /// </summary>
        public string NumSubFlota { get { return _num_subflota; } set { _num_subflota = value; } }

        /// <summary>
        /// Contador de tramo dentro de la matrícula
        /// </summary>
        public int Numero_Tramo { get { return _numero_tramo; } set { _numero_tramo = value; } }

        /// <summary>
        /// Número de vuelo
        /// </summary>
        public string Numero_Vuelo { get { return _numero_vuelo; } set { _numero_vuelo = value; } }
        
        /// <summary>
        /// Sufijo operacional
        /// </summary>
        public string Op_Suf { get { return _op_suf; } set { _op_suf = value; } }
        
        /// <summary>
        /// Origen del tramo
        /// </summary>
        public string Origen { get { return _origen; } set { _origen = value; } }

        /// <summary>
        /// Service Type Code
        /// </summary>
        public string Stc { get { return _stc; } set { _stc = value; } }

        public TipoTramoBase Tipo
        {
            get             
            {
                if (this._carrier == CODIGO_BACKUP && this._origen == this._destino)
                {
                    return TipoTramoBase.Backup;
                }
                else if (this._stc == CODIGO_MANTTO && this._origen == this._destino)
                {
                    return TipoTramoBase.Mantto;
                }
                else
                {
                    return TipoTramoBase.Leg;
                }
            }
        }

        #endregion

        #region CONSTRUCTORS

        public TramoBase()
        { }

        public TramoBase(int numero_global, string numero_ac, int numero_tramo, string num_subflota, string ac_type, string ac_owner, string carrier, string numero_vuelo, string op_suf, string stc, string config_asientos, string origen, DateTime fecha_salida, string hora_salida, string dom_int, string destino, DateTime fecha_llegada, string hora_llegada)
        {
            this._numero_ac = numero_ac;
            this._numero_tramo = numero_tramo;
            this._num_subflota = num_subflota;
            this._ac_type = ac_type;
            this._ac_owner = ac_owner;
            this._carrier = carrier;
            this._numero_vuelo = numero_vuelo;
            this._op_suf = op_suf;
            this._stc = stc;
            this._config_asientos = config_asientos;
            this._origen = origen;
            this._fecha_salida = fecha_salida;
            this._hora_salida = hora_salida;
            this._dom_int = dom_int;
            this._destino = destino;
            this._fecha_llegada = fecha_llegada;
            this._hora_llegada = hora_llegada;
            this._numero_global = numero_global;
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Retorna el número global en formato string ###
        /// </summary>
        /// <param name="dim">Número de dígitos</param>
        /// <returns></returns>
        public string GetNumGlobalString(int dim)
        {
            string num_ini = this._numero_global.ToString();
            for (int i = 1; i < dim; i++)
            {
                if (num_ini.Length == i)
                {
                    string retorno = "";
                    for (int j = i; j < dim; j++)
                    {
                        retorno += "0";
                    }
                    retorno += num_ini;
                    return retorno;
                }
            }
            return num_ini;

        }

        #endregion
        
        #region ICloneable Members

        /// <summary>
        /// Retorna una nueva instancia idéntica al tramo actual
        /// </summary>
        public object Clone()
        {
            TramoBase t = new TramoBase();
            t._numero_global = this._numero_global;
            t._numero_ac = this._numero_ac;
            t._numero_tramo = this._numero_tramo;
            t._num_subflota = this._num_subflota;
            t._ac_type = this._ac_type;
            t._ac_owner = this._ac_owner;
            t._carrier = this._carrier;
            t._numero_vuelo = this._numero_vuelo;
            t._op_suf = this._op_suf;
            t._stc = this._stc;
            t._config_asientos = this._config_asientos;
            t._origen = this._origen;
            t._fecha_salida = this._fecha_salida;
            t._hora_salida = this._hora_salida;
            t._dom_int = this._dom_int;
            t._destino = this._destino;
            t._fecha_llegada = this._fecha_llegada;
            t._hora_llegada = this._hora_llegada;
            return t;
        }

        #endregion
    }
}
