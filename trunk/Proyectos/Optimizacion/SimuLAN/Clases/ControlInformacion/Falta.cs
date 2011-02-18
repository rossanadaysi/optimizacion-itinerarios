using System;
using System.Collections.Generic;
using System.Text;

namespace SimuLAN.Clases.ControlInformacion
{
    /// <summary>
    /// Clase que encapsula la información de una falta de información
    /// </summary>
    public class Falta
    {
        #region ATRIBUTES
        
        /// <summary>
        /// Tipo de falta de información
        /// </summary>
        private TipoFaltaInformacion _tipo;

        /// <summary>
        /// Lista de claves relacionadas con la falta de información
        /// </summary>
        private List<string> _keys;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Tipo de falta de información
        /// </summary>
        public TipoFaltaInformacion Tipo
        {
            get { return _tipo; }
        }

        /// <summary>
        /// Lista de claves relacionadas con la falta de información
        /// </summary>
        public List<string> Keys
        {
            get { return _keys; }
        }
        
        #endregion

        #region CONSTRUCTOR
        
        /// <summary>
        /// Constructor de un objeto de falta de información
        /// </summary>
        /// <param name="tipo">Tipo de falta</param>
        /// <param name="keys">Claves de la información faltante</param>
        public Falta(TipoFaltaInformacion tipo, List<string> keys)
        {
            this._tipo = tipo;
            this._keys = keys;
        }
        
        #endregion

        #region PUBLIC METHODS
        
        /// <summary>
        /// Retorna un mensaje con la falta de información dependiente del tipo de falta.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            switch(_tipo)
            {
                case TipoFaltaInformacion.AcType_Flota:
                    {
                        return "No hay flota definida para AcType " + _keys[0];
                    }
                case TipoFaltaInformacion.Multioperador_Operador:
                    {
                        return "No está definido el operador " + _keys[0] + " en la matriz multioperador";
                    }
                case TipoFaltaInformacion.Multioperador_SubFlota:
                    {
                        return "No está definida la sublfota " + _keys[0] + " en la matriz multioperador";
                    }
                case TipoFaltaInformacion.SubFlota_Matricula:
                    {
                        return "No hay matrícula definida para la subflota " + _keys[0];
                    }
                case TipoFaltaInformacion.Flota_Grupo:
                    {
                        return "No hay grupo de flota definido para la flota " + _keys[0];
                    }
                case TipoFaltaInformacion.Curva_HBT:
                    {
                        return "No hay HBT para el tramo " + _keys[0] + ", grupo " + _keys[1];
                    }
                case TipoFaltaInformacion.TurnAround:
                    {
                        return "No hay turn around para la flota " + _keys[0] + ", estación " + _keys[1];
                    }
                case TipoFaltaInformacion.Tramo_Ruta:
                    {
                        return "No hay información de ruta comercial para el vuelo " + _keys[0];
                    }
                case TipoFaltaInformacion.Curva_Adelanto:
                    {
                        return "No hay información de curvas de ADELANTO para la estación " + _keys[0];
                    }
                case TipoFaltaInformacion.Curva_ATC:
                    {
                        return "No hay información de curvas de ATC para la estación " + _keys[0];
                    }
                case TipoFaltaInformacion.Curva_Mantto:
                    {
                        return "No hay información de curvas de MANTTO para la flota " + _keys[0];
                    }
                case TipoFaltaInformacion.Curva_OTROS:
                    {
                        return "No hay información de curvas de OTROS para la estación " + _keys[0];
                    }
                case TipoFaltaInformacion.Curva_Recursos_Apto:
                    {
                        return "No hay información de curvas de RECURSOS APTO para la estación " + _keys[0];
                    }
                case TipoFaltaInformacion.Curva_TA_BA:
                    {
                        return "No hay información de curvas de TA BAJO ALA para la estación " + _keys[0];
                    }
                case TipoFaltaInformacion.Curva_TA_SA:
                    {
                        return "No hay información de curvas de TA SOBRE ALA para la estación " + _keys[0];
                    }
                case TipoFaltaInformacion.Curva_Trip:
                    {
                        return "No hay información de curvas de TRIPULACION para la estación " + _keys[0];
                    }
                case TipoFaltaInformacion.Curva_WXS:
                    {
                        return "No hay información de curvas de METEOROLOGIA para la estación " + _keys[0];
                    }
                default:
                    {
                        string keystring = "";
                        for (int i = 0; i < _keys.Count; i++)
                        {
                            keystring += _keys[0] + "-";
                        }
                        return "Error tipo  " + _tipo.ToString() + " en  " + keystring;
                    }
            }

        }
        
        #endregion
    }
}
