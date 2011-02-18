using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using SimuLAN.Utils;

namespace SimuLAN.Clases
{   
    /// <summary>
    /// Esta clase representa un aeropuerto en la simulación. Un aeropuerto es independiente en términos de meteorología y límite técnico para la compañía.
    /// </summary>
    [XmlRoot("aeropuerto")]
    public class Aeropuerto:ICloneable, IDisposable
    {
        #region ATRIBUTES
        
        /// <summary>
        /// Variable de estado del clima del aeropuerto. True si se puede operar.
        /// </summary>
        private bool _buen_clima;

        /// <summary>
        /// Indica si el aeropuerto es del tipo HUB
        /// </summary>
        private bool _es_hub;

        /// <summary>
        /// Diccionario con el estado del clima por hora dentro de todo el tiempo de simulación.
        /// </summary>
        private Dictionary<DateTime, bool> _estado_wxs_hora;

        /// <summary>
        /// Delegado para acceder a las curvas de clima
        /// </summary>
        private GetProbabilidadClimaEventHandler _get_probabilidad_clima;

        /// <summary>
        /// Horas de desfase con respecto a la hora UTC desfase = (UTC-Local)
        /// </summary>
        private int _horas_desfase_UTC;

        /// <summary>
        /// Tiempo de conexión de pasajeros
        /// </summary>
        private int _minutos_conexion_pax;

        /// <summary>
        /// Tiempo que tarda un turno en llegar desde que es solicitado
        /// </summary>
        private int _minutos_llega_turno;
        
        /// <summary>
        /// Nombre del aeropuerto
        /// </summary>
        private string _nombre;

        /// <summary>
        /// Variable aleatoria propia de cada aeropuerto,usada en la actualización del estado del clima.
        /// </summary>
        private Random _rdm;

        #endregion
        
        #region PROPERTIES

        /// <summary>
        /// Variable de estado del clima del aeropuerto. True si se puede operar.
        /// </summary>
        public bool BuenClima
        {
            get { return _buen_clima; }
            set { _buen_clima = value; }
        }

        /// <summary>
        /// Indica si el aeropuerto es del tipo HUB
        /// </summary>
        public bool Es_Hub
        {
            get { return _es_hub; }
            set { _es_hub = value; }
        }

        /// <summary>
        /// Delegado para acceder a las curvas de clima
        /// </summary>
        public GetProbabilidadClimaEventHandler GetProbabilidadClima
        {
            get { return _get_probabilidad_clima; }
            set { _get_probabilidad_clima = value; }        
        }

        /// <summary>
        /// Nombre del aeropuerto
        /// </summary>
        public string Nombre
        {
            get { return _nombre; }
            set { _nombre = value; }
        }

        /// <summary>
        /// Horas de desfase con respecto a la hora UTC desfase = (UTC-Local)
        /// </summary>
        public int Horas_Desfase_UTC
        {
            get { return _horas_desfase_UTC; }
            set { _horas_desfase_UTC = value; }
        }

        /// <summary>
        /// Tiempo de conexión de pasajeros
        /// </summary>
        public int Minutos_Conexion_Pax
        {
            get { return _minutos_conexion_pax; }
            set { _minutos_conexion_pax = value; }
        }

        /// <summary>
        /// Tiempo que tarda un turno en llegar desde que es solicitado
        /// </summary>
        public int Minutos_Llega_Turno
        {
            get { return _minutos_llega_turno; }
            set { _minutos_llega_turno = value; }
        }

        /// <summary>
        /// True si hay info histórica para el aeropuerto
        /// </summary>
        public bool Tiene_Info_Historica_WXS
        {
            get { return _estado_wxs_hora != null; }
        }

        #endregion

        #region CONSTRUCTORS
        
        /// <summary>
        /// Constructor de aeropuertos
        /// </summary>
        /// <param name="nombre">Nombre del aeropuerto</param>
        /// <param name="semilla">Semilla usada para la generación de números aleatorios</param>
        public Aeropuerto(string nombre, int semilla)
        {
            this._buen_clima = true;
            this._nombre = nombre;
            this._rdm = new Random(semilla+Math.Abs(nombre.GetHashCode()));
            this._es_hub = false;
            this._minutos_conexion_pax = int.MaxValue;
            this._minutos_llega_turno = int.MaxValue;
            this._horas_desfase_UTC = 0;
        }

        /// <summary>
        /// Constructor para serialización
        /// </summary>
        public Aeropuerto()
        {

        }

        #endregion 

        #region INTERNAL METHODS

        /// <summary>
        /// Carga a priori toda la evolución del tiempo en función de la información histórica del aeropuerto
        /// </summary>
        /// <param name="info">Información histórica del estado del clima</param>
        /// <param name="fecha_ini">Fecha inicio del itinerario</param>
        /// <param name="fecha_fin">Fecha fin del itinerario</param>
        internal void CargarInfoHistoricaWXS(double[,] info, DateTime fecha_ini, DateTime fecha_fin)
        {
            _estado_wxs_hora = new Dictionary<DateTime, bool>();
            DateTime fecha_aux = fecha_ini.AddDays(-1);
            while (fecha_aux <= fecha_fin.AddDays(2))
            {
                int mes = fecha_aux.Month;
                int hora_utc = fecha_aux.Hour;
                bool estado_wxs = true;
                if (_rdm.NextDouble() < info[mes - 1, hora_utc])
                {
                    estado_wxs = false;
                }
                _estado_wxs_hora.Add(fecha_aux, estado_wxs);
                fecha_aux = fecha_aux.AddHours(1);
            }
        }

        /// <summary>
        /// Método para clonar aeropuertos
        /// </summary>
        /// <param name="semilla">Semilla para la generación de números aleatorios</param>
        /// <returns>Aeropuerto clonado</returns>
        internal Aeropuerto Clonar(int semilla)
        {
            Aeropuerto clonado = new Aeropuerto();
            clonado._nombre = this._nombre;
            clonado._minutos_conexion_pax = this._minutos_conexion_pax;
            clonado._minutos_llega_turno = this._minutos_llega_turno;
            clonado._horas_desfase_UTC = this._horas_desfase_UTC;
            clonado._rdm = new Random(semilla + Math.Abs(_nombre.GetHashCode()));
            clonado._es_hub = this._es_hub;
            clonado.GetProbabilidadClima = new GetProbabilidadClimaEventHandler(GetProbabilidadClima);
            //clonado._gestor_turnos.Turnos_Manana_Max = this._gestor_turnos.Turnos_Manana_Max;
            //clonado._gestor_turnos.Turnos_Tarde_Max = this._gestor_turnos.Turnos_Tarde_Max;
            return clonado;
        }

        /// <summary>
        /// Estima los minutos de atraso por WXS para un tramo
        /// </summary>
        /// <param name="tramo">Tramo objetivo</param>
        /// <returns>Minutos de atraso por WXS</returns>
        internal int EstimarMinutosAtrasoWXSHistorico(Tramo tramo)
        {
            int atraso = 0;
            if (tramo != null)
            {
                DateTime t_ini = tramo.DtIniProg.AddMinutes(tramo.TInicialRst - tramo.TInicialProg);
                DateTime t_key = new DateTime(t_ini.Year, t_ini.Month, t_ini.Day, t_ini.Hour, 0, 0);
                int minutos_ini = t_ini.Minute;
                int horas_atraso = 0;
                bool busqueda = true;
                while (busqueda)
                {
                    if (_estado_wxs_hora.ContainsKey(t_key))
                    {
                        //Si estado del clima es malo (false) se aumenta en 1 las horas de atraso.
                        if (!_estado_wxs_hora[t_key])
                        {
                            horas_atraso++;
                        }
                        else
                        {
                            busqueda = false;
                        }
                    }
                    else
                    {
                        busqueda = false;
                    }
                    t_key = t_key.AddHours(1);
                }
                atraso = Convert.ToInt16(Math.Max(60 * (horas_atraso - 1)+ (60 - minutos_ini) * _rdm.NextDouble(), 0));
            }
            return atraso;
        }

        /// <summary>
        /// Se actualiza el estado del clima.
        /// </summary>
        internal void RevisarClima(int tiempoSimulacion, int mes)
        {
            /*Se considera que:
             *  -la noche va desde las 21:00 (inclusive) hasta las 05:00. (1)
             *  -la mañana va desde las 05:00 (inclusive) hasta las 13:00.(0)
             *  -la tarde va desde las 13:00 (inclusive) hasta las 21:00. (2) */
            int hora = Convert.ToInt32(tiempoSimulacion / 60.0) % 24;
            double p = GetProbabilidadClima(mes.ToString(), this._nombre, hora);
            double u = _rdm.NextDouble();
            if (p <= u)
            {
                _buen_clima = true;
            }
            else
            {
                _buen_clima = false;
            }
        }

        #endregion

        #region PUBLIC METHODS
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns>string con el nombre del aeropuerto</returns>
        public override string ToString()
        {
            return _nombre.ToString();
        }
        
        #endregion

        #region ICloneable Members

        /// <summary>
        /// Método para clonar un aeropuerto
        /// </summary>
        /// <returns>Aeropuerto clonado</returns>
        public object Clone()
        {
            Aeropuerto clonado = new Aeropuerto();
            return clonado;
        }

        #endregion
    
        #region IDisposable Members

        private bool IsDisposed=false;

        /// <summary>
        /// Destructor de instancias de la clase
        /// </summary>
        ~Aeropuerto()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool Disposing)
        {
            if(!IsDisposed)
            {
                if(Disposing)
                {
                    if (_estado_wxs_hora != null)
                    {
                        _estado_wxs_hora.Clear();
                    }
                }
                _get_probabilidad_clima = null;
                _rdm = null;
                _nombre = null;
                _estado_wxs_hora = null;
            }
            IsDisposed=true;
        }
        #endregion
    }
}
