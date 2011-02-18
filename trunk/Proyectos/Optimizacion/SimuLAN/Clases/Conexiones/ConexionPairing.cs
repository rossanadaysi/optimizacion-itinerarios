using System;
using System.Collections.Generic;
using System.Text;
using SimuLAN.Utils;

namespace SimuLAN.Clases
{
    /// <summary>
    /// Conexión de pairing
    /// </summary>
    public class ConexionPairing:Conexion
    {
        #region STATIC ATRIBUTES

        /// <summary>
        /// Tiempo mínimo de cambio de avión en la conexión
        /// </summary>
        public static int TIEMPO_CAMBIO_AVION = 55;

        /// <summary>
        /// Tiempo máximo de cambio de avión en la conexión
        /// </summary>
        public static int TIEMPO_MAXIMO_PAIRING = 180;

        /// <summary>
        /// Variable que cuenta la cantidad de pairings que se han agregado
        /// </summary>
        public static int Serial = 0;

        #endregion

        #region ATRIBUTES
        /// <summary>
        /// Diccionario que indica la existencia del pairing en los 7 días de la semana
        /// </summary>
        private Dictionary<DayOfWeek, bool> _aplica_dia_semana;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id_vuelo_1">Número de vuelo inicial</param>
        /// <param name="id_vuelo_2">Número de vuelo final</param>
        /// <param name="tipo">Tipo de conexión</param>
        public ConexionPairing(string id_vuelo_1, string id_vuelo_2, TipoConexion tipo)
            : base(id_vuelo_1, id_vuelo_2,tipo)
        {
            _aplica_dia_semana = new Dictionary<DayOfWeek, bool>();
            Serial++;
        }

        #endregion

        #region INTERNAL METHODS

        /// <summary>
        /// Transforma la información del pairing en una fila de tabla de información
        /// </summary>
        /// <returns>Arreglo con la información del pairing</returns>
        internal object[] PairingToRowData()
        {
            object[] obj = new object[9];
            obj[0] = base.IdVuelo1;
            obj[1] = base.IdVuelo2;
            for (int i = 0; i < 7; i++)
            {
                obj[2 + i] = Convert.ToInt16(_aplica_dia_semana[Utilidades.IntToDiaSemanaEnum(i)]);

            }
            return obj;
        }

        /// <summary>
        /// Transforma la información de fila de tabla de información en un pairing
        /// </summary>
        /// <returns>Pairing</returns>
        internal static ConexionPairing RowDataToPairing(object[] obj)
        {
            ConexionPairing pairing = new ConexionPairing(obj[0].ToString(), obj[1].ToString(), TipoConexion.Pairing);
            bool[] aplica = new bool[7];
            for (int i = 0; i < 7; i++)
            {
                aplica[i] = Utilidades.IntToBool(Convert.ToInt16(obj[i + 2]));
            }
            pairing.LlenarAplicaDiaSemana(aplica);
            return pairing;
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Retorna la existencia del pairing para un día de la semana particular
        /// </summary>
        /// <param name="d">Dia de la semana</param>
        /// <returns>True si el pairing está activo</returns>
        public bool GetAplicaDiaSemana(DayOfWeek d)
        {
            return _aplica_dia_semana[d];        
        }
               
        /// <summary>
        /// Retorna la existencia del pairing para un día de la semana particular
        /// </summary>
        /// <param name="dia">Número del día de la semana. Cero para día Lunes</param>
        /// <returns>True si el pairing está activo</returns>
        public bool GetAplicaDiaSemana(int dia)
        {
            return _aplica_dia_semana[Utilidades.IntToDiaSemanaEnum(dia)];
        }

        /// <summary>
        /// Llena diccionario de existencia de pairings
        /// </summary>
        /// <param name="input">Arreglo de 7x1 con valores true o false para la existencia de un pairing</param>
        public void LlenarAplicaDiaSemana(bool[] input)
        {
            for (int i = 0; i < 7; i++)
            {
                _aplica_dia_semana.Add(Utilidades.IntToDiaSemanaEnum(i), input[i]);
            }
        }

        /// <summary>
        /// Setea si existe conexión un día de la semana
        /// </summary>
        /// <param name="dia">Día de la semana</param>
        /// <param name="aplica">True si la conexión existe ese día</param>
        public void SetAplicaDiaSemana(DayOfWeek dia, bool aplica)
        {
            _aplica_dia_semana[dia] = aplica;
        }

        #region Miembros de IComparable

        public override bool Equals(object obj)
        {
            return this.IdVuelo1 == ((ConexionPairing)obj).IdVuelo1 && this.IdVuelo2 == ((ConexionPairing)obj).IdVuelo2;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion

        #endregion
    }
}
