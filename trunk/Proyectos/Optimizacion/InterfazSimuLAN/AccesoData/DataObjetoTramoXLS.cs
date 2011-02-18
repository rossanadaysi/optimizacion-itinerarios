using System;
using System.Collections.Generic;
using System.Text;

namespace InterfazSimuLAN
{
    /// <summary>
    /// Enumeración con los campos de archivo de itinerario
    /// </summary>
    public enum CamposArchivoItinerario{Id_Avion, Id_Tramo_AC, NumSubflota, AC_Type,
    Operador, Carrier, Leg_ID_Global, Op_Suf, STC,Config_Asientos, Origen, Fecha_Ini, 
        Fecha_X, STD, Dom_Int, Destino, Fecha_Fin, STA }

    /// <summary>
    /// Clase que encapsula la información del archivo del itinerario
    /// </summary>
    public class DataObjetoTramoXLS
    {
        #region PROPERTIES

        public string idAvion;
        public int idTramoAvion;
        public string numSubFlota;
        public string acType;
        public string operador;
        public string carrier;
        public string idTramoGlobal;
        public string op_suf;
        public string stc;
        public string config_asientos;
        public string origen;
        public string fechaInicio;
        public string fechaUnknown;
        public int STD;
        public string domInt;
        public string destino;
        public string fechaTermino;
        public int STA;

        #endregion

        #region CONSTRUCTOR
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Objeto con los valores de una tupla del itinerario</param>
        /// <param name="camposIndices">Diccionario con los índices de cada item</param>
        public DataObjetoTramoXLS(object[] items, Dictionary<CamposArchivoItinerario, int> camposIndices)
        {
            idAvion = Convert.ToString(items[camposIndices[CamposArchivoItinerario.Id_Avion]]);
            idTramoAvion = Convert.ToInt32(items[camposIndices[CamposArchivoItinerario.Id_Tramo_AC]]);
            numSubFlota = Convert.ToString(items[camposIndices[CamposArchivoItinerario.NumSubflota]]);
            acType = Convert.ToString(items[camposIndices[CamposArchivoItinerario.AC_Type]]);
            operador = Convert.ToString(items[camposIndices[CamposArchivoItinerario.Operador]]);
            carrier = Convert.ToString(items[camposIndices[CamposArchivoItinerario.Carrier]]);
            idTramoGlobal = Convert.ToString(items[camposIndices[CamposArchivoItinerario.Leg_ID_Global]]);
            op_suf = Convert.ToString(items[camposIndices[CamposArchivoItinerario.Op_Suf]]);
            stc = Convert.ToString(items[camposIndices[CamposArchivoItinerario.STC]]);
            config_asientos = Convert.ToString(items[camposIndices[CamposArchivoItinerario.Config_Asientos]]);
            origen = Convert.ToString(items[camposIndices[CamposArchivoItinerario.Origen]]);
            fechaInicio = Convert.ToString(items[camposIndices[CamposArchivoItinerario.Fecha_Ini]]);
            fechaUnknown = Convert.ToString(items[camposIndices[CamposArchivoItinerario.Fecha_X]]);
            STD = Convert.ToInt32(items[camposIndices[CamposArchivoItinerario.STD]]);
            domInt = Convert.ToString(items[camposIndices[CamposArchivoItinerario.Dom_Int]]);
            destino = Convert.ToString(items[camposIndices[CamposArchivoItinerario.Destino]]);
            fechaTermino = Convert.ToString(items[camposIndices[CamposArchivoItinerario.Fecha_Fin]]);
            STA = Convert.ToInt32(items[camposIndices[CamposArchivoItinerario.STA]]);
        }

        #endregion
    }
}
