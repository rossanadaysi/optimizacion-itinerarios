using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SimuLAN.Clases.Recovery
{
    /// <summary>
    /// Clase que representa un slot de backup
    /// </summary>
    public class SlotBackup : ICloneable
    {
        #region ATRIBUTES

        /// <summary>
        /// Unidad de backup contenedora
        /// </summary>
        private UnidadBackup _contenedor;

        /// <summary>
        /// Estación
        /// </summary>
        private string _estacion;

        /// <summary>
        /// Matrícula que tiene asignado el slot
        /// </summary>
        private string _matricula;

        /// <summary>
        /// Tiempo de término programado del slot de backup
        /// </summary>
        private int _t_fin_prg;

        /// <summary>
        /// Tiempo de término resultante del slot de backup
        /// </summary>
        private int _t_fin_rst;

        /// <summary>
        /// Tiempo de término de uso del slot de backup
        /// </summary>
        private int _t_fin_uso;

        /// <summary>
        /// Tiempo de inicio programado del slot de backup
        /// </summary>
        private int _t_ini_prg;

        /// <summary>
        /// Tiempo de inicio resultante del slot de backup
        /// </summary>
        private int _t_ini_rst;

        /// <summary>
        /// Tiempo de inicio de uso del slot de backup
        /// </summary>
        private int _t_ini_uso;

        /// <summary>
        /// Tipo de uso que se dio al slot de backup
        /// </summary>
        private TipoUsoBackup _tipo_uso;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Unidad de backup contenedora
        /// </summary>
        public UnidadBackup Contenedor
        {
            get { return _contenedor; }
            set { _contenedor = value; }
        }

        /// <summary>
        /// Estación
        /// </summary>
        public string Estacion
        {
            get { return _estacion; }
            set { _estacion = value; }
        }

        /// <summary>
        /// Matrícula que tiene asignado el slot
        /// </summary>
        public string Matricula
        {
            get { return _matricula; }
            set { _matricula = value; }
        }

        /// <summary>
        /// Tiempo de término programado del slot de backup
        /// </summary>
        public int TiempoFinPrg
        {
            get { return _t_fin_prg; }
            set { _t_fin_prg = value; }
        }

        /// <summary>
        /// Tiempo de término resultante del slot de backup
        /// </summary>
        public int TiempoFinRst
        {
            get { return _t_fin_rst; }
            set { _t_fin_rst = value; }
        }

        /// <summary>
        /// Tiempo de término de uso del slot de backup
        /// </summary>
        public int TiempoFinUso
        {
            get { return _t_fin_uso; }
            set { _t_fin_uso = value; }
        }

        /// <summary>
        /// Tiempo de inicio resultante del slot de backup
        /// </summary>
        public int TiempoIniRst
        {
            get { return _t_ini_rst; }
            set { _t_ini_rst = value; }
        }

        /// <summary>
        /// Tiempo de inicio de uso del slot de backup
        /// </summary>
        public int TiempoIniUso
        {
            get { return _t_ini_uso; }
            set { _t_ini_uso = value; }
        }

        /// <summary>
        /// Tipo de uso que se dio al slot de backup
        /// </summary>
        public TipoUsoBackup TipoUso
        {
            get { return _tipo_uso; }
            set { _tipo_uso = value; }
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor de slot de backup
        /// </summary>
        /// <param name="bu">Unidad contenedora</param>
        /// <param name="_tiempo_ini_programado">Tiempo de inicio programado</param>
        /// <param name="_tiempo_fin_programado">Tiempo de término programado</param>
        /// <param name="_estacion">Estación</param>
        /// <param name="_matricula">Matrícula</param>
        public SlotBackup(UnidadBackup bu, int _tiempo_ini_programado, int _tiempo_fin_programado, string _estacion, string _matricula)
        {
            this._contenedor = bu;
            this._t_ini_prg = _tiempo_ini_programado;
            this._t_ini_rst = _tiempo_ini_programado;
            this._t_fin_prg = _tiempo_fin_programado;
            this._t_fin_rst = _tiempo_fin_programado;
            this._t_ini_uso = _tiempo_ini_programado;
            this._t_fin_uso = _tiempo_ini_programado;
            this._estacion = _estacion;
            this._matricula = _matricula;
            this._tipo_uso = TipoUsoBackup.SinUso;
        }

        #endregion

        #region ICloneable Members

        /// <summary>
        /// Genera una copia del slot
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            SlotBackup s = new SlotBackup(null, this._t_ini_prg, this._t_fin_prg, this._estacion, this._matricula);
            return s;
        }

        #endregion
    }
}
