using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using SimuLAN.Utils;

namespace SimuLAN.Clases.Recovery
{
    /// <summary>
    /// Objeto que encapsula el concepto de utilización de un slot de backup
    /// </summary>
    [XmlRoot("UnidadBackup")]
    public class UnidadBackup : ICloneable, IDisposable
    {
        #region ATRIBUTES

        /// <summary>
        /// Delegado que encapsula método para el cálculo de la utilización de slots de backups
        /// </summary>
        [XmlIgnore]
        private EstimarUtilizacionSlotEventHandler _estimar_utilizacion_slot;

        /// <summary>
        /// Lista de slots de backups producidos en la unidad
        /// </summary>
        [XmlIgnore]
        private List<SlotBackup> _slots;

        /// <summary>
        /// Lista de swaps producidos en la unidad
        /// </summary>
        [XmlIgnore]
        private Dictionary<string, Swap> _swaps;

        /// <summary>
        /// Tiempo de término programado de la unidad
        /// </summary>
        private int _t_fin_prg;

        /// <summary>
        /// Tiempo de término resultante de la unidad
        /// </summary>
        private int _t_fin_rst;

        /// <summary>
        /// Tiempo de inicio programado de la unidad
        /// </summary>
        private int _t_ini_prg;

        /// <summary>
        /// Tiempo de inicio resultante de la unidad
        /// </summary>
        private int _t_ini_rst;

        /// <summary>
        /// Información leida desde el itinerario
        /// </summary>
        private TramoBase _tramo_base;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Estación donde fue asignada la unidad de backup
        /// </summary>
        [XmlIgnore]
        public string Estacion
        {
            get { return _tramo_base.Origen; }
            set { _tramo_base.Origen = value; }
        }

        /// <summary>
        /// Estadísticos de utilización de la unidad de backup
        /// </summary>
        [XmlIgnore]
        public EstadisticosUtilizacion Estadisticos
        {
            get
            {
                EstadisticosUtilizacion estadisticos = new EstadisticosUtilizacion(this.Id);
                estadisticos.minutosRecuperados = EstimarTotalMinutosRecuperados();
                estadisticos.porcentajeUtilizacionNeta = EstimarUtilizacionNeta();
                estadisticos.tramosRecuperados = EstimarTramosSalvados();
                estadisticos.porcentajeUtilizacionSlotConTA = EstimarPorcentajeUtilizacionSlotMatriculaConTA();
                estadisticos.porcentajeUtilizacionSlotSinTA = EstimarPorcentajeUtilizacionSlotMatriculaSinTA();
                return estadisticos;
            }
        }

        /// <summary>
        /// Delegado que encapsula método para el cálculo de la utilización de slots de backups
        /// </summary>
        [XmlIgnore]
        public EstimarUtilizacionSlotEventHandler EstimarUtilizacionSlot
        {
            get { return _estimar_utilizacion_slot; }
            set { _estimar_utilizacion_slot = value; }
        }

        /// <summary>
        /// Hour Block Time en formato hh:mm
        /// </summary>
        [XmlIgnore]
        public string HBTProg
        {
            get
            {
                int duracion = _t_fin_prg - _t_ini_prg;
                int minutos = duracion % 60;
                int horas = Convert.ToInt16(Math.Floor(duracion / 60.0));
                string minutos_str = (minutos.ToString().Length == 0) ? "00" : (minutos.ToString().Length == 1) ? "0" + minutos.ToString() : minutos.ToString();
                string horas_str = (horas.ToString().Length == 0) ? "00" : (horas.ToString().Length == 1) ? "0" + horas.ToString() : horas.ToString();
                return horas_str + ":" + minutos_str;
            }
        }

        /// <summary>
        /// Identificador de la unidad de backup
        /// </summary>
        [XmlIgnore]
        public string Id
        {
            get { return TramoBase.Carrier + "-" + TramoBase.Numero_Ac + "-" + TramoBase.Fecha_Salida.ToShortDateString(); }
        }

        /// <summary>
        /// Lista de slots de backups
        /// </summary>
        [XmlIgnore]
        public List<SlotBackup> Slots
        {
            get { return _slots; }
        }

        /// <summary>
        /// Lista de swaps producidos en la unidad
        /// </summary>
        [XmlIgnore]
        public Dictionary<string, Swap> Swaps
        {
            get { return _swaps; }
        }

        /// <summary>
        /// Tiempo de término programado de la unidad
        /// </summary>
        public int TiempoFinPrg
        {
            get { return _t_fin_prg; }
            set { _t_fin_prg = value; }
        }

        /// <summary>
        /// Tiempo de término resultante de la unidad
        /// </summary>
        public int TiempoFinRst
        {
            get { return _t_fin_rst; }
            set { _t_fin_rst = value; }
        }

        /// <summary>
        /// Tiempo de inicio programado de la unidad
        /// </summary>
        public int TiempoIniPrg
        {
            get { return _t_ini_prg; }
            set { _t_ini_rst = value; }
        }

        /// <summary>
        /// Tiempo de inicio resultante de la unidad
        /// </summary>
        public int TiempoIniRst
        {
            get { return _t_ini_rst; }
            set { _t_ini_rst = value; }
        }

        /// <summary>
        /// Información leida desde el itinerario
        /// </summary>
        public TramoBase TramoBase
        {
            get { return _tramo_base; }
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor para serialización
        /// </summary>
        public UnidadBackup()
        {
        }

        /// <summary>
        /// Constructor de una unidad de backup
        /// </summary>
        /// <param name="tramoBase">Objeto con la información del itinerario</param>
        /// <param name="fechaBase">Fecha inicial del itinerario</param>
        public UnidadBackup(TramoBase tramoBase, DateTime fechaBase)
        {
            this._tramo_base = tramoBase;
            int diasDesfaseSalida = Convert.ToInt32((tramoBase.Fecha_Salida - fechaBase).TotalDays);
            int diasDesfaseLlegada = Convert.ToInt32((tramoBase.Fecha_Llegada - fechaBase).TotalDays);
            int horaSalidaNum = Utilidades.ConvertirMinutosDesdeHoraString(tramoBase.Hora_Salida);
            int horaLlegadaNum = Utilidades.ConvertirMinutosDesdeHoraString(tramoBase.Hora_Llegada);
            this._t_ini_prg = diasDesfaseSalida * 24 * 60 + horaSalidaNum;
            this._t_ini_rst = _t_ini_prg;
            this._t_fin_prg = diasDesfaseLlegada * 24 * 60 + horaLlegadaNum;
            this._t_fin_rst = _t_fin_prg;
            this._swaps = new Dictionary<string, Swap>();
            this._slots = new List<SlotBackup>();
            string matricula = tramoBase.Numero_Ac;
            string estacion = tramoBase.Origen;
            //string key = tramoBase.Numero_Ac + "-" + _tiempo_ini_programado;
            SlotBackup slot_base = new SlotBackup(this, _t_ini_prg, _t_fin_prg, estacion, matricula);
            _slots.Add(slot_base);
        }

        #endregion

        #region INTERNAL METHODS

        /// <summary>
        /// Agrega un swap a la lista de la unidad
        /// </summary>
        /// <param name="key">Id del swap</param>
        /// <param name="s">Swap agregado</param>
        internal void AddSwap(string key, Swap s)
        {
            _swaps.Add(key, s);
            DividirSlot(s);
        }

        /// <summary>
        /// Método para buscar un slot de backup dentro de la unidad
        /// </summary>
        /// <param name="ini">Tiempo de inicio resultante límite</param>
        /// <param name="fin">Tiempo de término resultante límite</param>
        /// <param name="matricula">Matrícula buscada</param>
        /// <param name="estacion">Estación buscada</param>
        /// <returns>Slot de backup en el intervalo, matrícula y estación buscados</returns>
        internal SlotBackup BuscarSlot(int ini, int fin, string matricula, string estacion)
        {
            foreach (SlotBackup s in _slots)
            {
                if (s.Matricula == matricula && Estacion == estacion && fin >= s.TiempoIniRst && ini <= s.TiempoFinRst)
                {
                    return s;
                }
            }
            return null;
        }

        /// <summary>
        /// Usa a priori el primer slot de backup a causa de AOG
        /// </summary>
        /// <param name="horasAOG">Horas de AOG necesarias</param>
        /// <returns>Cantidad de horas usados para el AOG</returns>
        internal double UsarPorAOG(double horas_AOG)
        {
            if (horas_AOG == 0)
            {
                return 0;
            }
            else
            {
                int minutos_AOG_usados = Convert.ToInt16(Math.Min(horas_AOG * 60, this.Slots[0].TiempoFinRst - this.Slots[0].TiempoIniRst));
                this.Slots[0].TiempoFinUso = this.Slots[0].TiempoIniUso + minutos_AOG_usados;
                this.Slots[0].TipoUso = TipoUsoBackup.AOG;
                int tiempo_ini_nuevo = this.Slots[0].TiempoFinUso;
                int tiempo_fin_nuevo = this.Slots[0].TiempoFinPrg;
                if (tiempo_fin_nuevo - tiempo_ini_nuevo > 0)
                {
                    this.Slots.Add(new SlotBackup(this, tiempo_ini_nuevo, tiempo_fin_nuevo, this.Estacion, this.Slots[0].Matricula));
                }
                return minutos_AOG_usados / 60.0;
            }            
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Busca el primer slot útil en un intervalo de tiempo.
        /// </summary>
        /// <param name="ini">Inicio del intervalo de búsqueda</param>
        /// <param name="fin">Fin del intervalo de búsqueda</param>
        /// <returns></returns>
        private SlotBackup BuscarSlot(int ini, int fin)
        {
            foreach (SlotBackup s in _slots)
            {
                if (fin >= s.TiempoIniRst && ini <= s.TiempoFinRst)
                {
                    return s;
                }
            }
            return null;
        }

        /// <summary>
        /// Particiona el slot de backup utilizado en función del swap ejecutado.
        /// </summary>
        /// <param name="s">Swap ejecutado sobre el slot de backup en operación</param>
        private void DividirSlot(Swap s)
        {
            int tiempo_corte_swap; //= s.Tramo_Ini_Emisor.TInicialRst + s.Minutos_Atraso_Reaccionario_Inicial;
            int tiempo_inicio_uso_swap;// = s.Tramo_Ini_Emisor.TFinProgMasTATramoPrevio;
            s.EstimarTiemposCorteBackup(out tiempo_corte_swap, out tiempo_inicio_uso_swap);
            SlotBackup slotUsado = BuscarSlot(s.TramoPreBackup.TInicialProg, s.TramoPostBackup.TFinalRst);
            if (slotUsado != null)
            {
                slotUsado.TipoUso = TipoUsoBackup.Swap;
                slotUsado.TiempoIniUso = Math.Max(slotUsado.TiempoIniUso, Math.Min(slotUsado.TiempoFinPrg, tiempo_inicio_uso_swap));
                slotUsado.TiempoFinRst = Math.Min(Math.Max(tiempo_corte_swap, slotUsado.TiempoIniRst), slotUsado.TiempoFinPrg);
                tiempo_corte_swap = slotUsado.TiempoFinRst;
                slotUsado.TiempoFinUso = slotUsado.TiempoFinRst;
                int tiempoFinNuevo = Math.Min(slotUsado.TiempoFinPrg, s.TiempoFinHolguraEnEmisor);
                if (tiempoFinNuevo - tiempo_corte_swap > 0)
                {
                    string matricula = null;
                    if (s.TipoUsoBackup == UsoBackup.FinEmisor)
                    {
                        matricula = s.IdAvionReceptor;
                    }
                    else
                    {
                        matricula = s.IdAvionEmisor;
                    }
                    SlotBackup nuevo_slot = new SlotBackup(this, tiempo_corte_swap, tiempoFinNuevo, Estacion, matricula);
                    _slots.Add(nuevo_slot);
                }
            }
        }

        /// <summary>
        /// Método para estimar el tiempo utilizado neto en minutos de la unidad de backup.
        /// El tiempo utilizado neto considera sólo el atraso absorvido por la unidad de backup
        /// </summary>
        /// <returns></returns>
        private int EstimarEspacioUtilizadoNeto()
        {
            int total = 0;           
            foreach (SlotBackup sb in _slots)
            {
                total += sb.TiempoFinUso - sb.TiempoIniUso;
            }
            return total;
        }

        /// <summary>
        /// Método para estimar el porcentaje de utilización de la matrícula inicial asignada a la unidad de backup, considerando útiles los T/A mínimos de los extremos.
        /// La utilización se estima como el porcentaje de minutos usados en la operación durante el tiempo programado para la unidad.
        /// </summary>
        /// <returns></returns>
        private double EstimarPorcentajeUtilizacionSlotMatriculaConTA()
        {
            SlotBackup slotIni = _slots[0];
            string matricula = slotIni.Matricula;
            return _estimar_utilizacion_slot(matricula, TiempoIniPrg, TiempoFinPrg, true) + ((slotIni.TipoUso == TipoUsoBackup.AOG) ? (0.0 + slotIni.TiempoFinUso - slotIni.TiempoIniUso) / (TiempoFinPrg - TiempoIniPrg + 0.0) : 0);
        }

        /// <summary>
        /// Método para estimar el porcentaje de utilización de la matrícula inicial asignada a la unidad de backup, sin considerar útiles los T/A mínimos de los extremos.
        /// La utilización se estima como el porcentaje de minutos usados en la operación durante el tiempo programado para la unidad.
        /// </summary>
        /// <returns></returns>
        private double EstimarPorcentajeUtilizacionSlotMatriculaSinTA()
        {
            SlotBackup slotIni = _slots[0];
            string matricula = slotIni.Matricula;
            return _estimar_utilizacion_slot(matricula, TiempoIniPrg, TiempoFinPrg, false) + ((slotIni.TipoUso == TipoUsoBackup.AOG) ? (0.0 + slotIni.TiempoFinUso - slotIni.TiempoIniUso) / (TiempoFinPrg - TiempoIniPrg + 0.0) : 0);
        }

        /// <summary>
        /// Método para calcular la cantidad total de minutos recuperados por causa de la unidad de backup
        /// </summary>
        /// <returns></returns>
        private int EstimarTotalMinutosRecuperados()
        {
            int total = 0;
            foreach (Swap s in _swaps.Values)
            {
                total += s.MinutosGananciaNeta;
            }
            return total;
        }

        /// <summary>
        /// Método para calcular la cantidad total de tramos beneficiados por causa de la unidad de backup
        /// </summary>
        /// <returns></returns>
        private int EstimarTramosSalvados()
        {
            int total = 0;
            foreach (Swap s in _swaps.Values)
            {
                total += s.NumTramosDisminuyenAtraso - s.NumTramosAumentanAtraso;
            }
            return total;
        }

        /// <summary>
        /// Método para estimar el porcentaje de utilización neta de la unidad de backup. 
        /// Utilización neta cuenta sólo el atraso absorvido por la unidad de backup.
        /// </summary>
        /// <returns></returns>
        private double EstimarUtilizacionNeta()
        {
            double totalEspacioUtil = _t_fin_rst - _t_ini_rst;
            double totalEspacioUtilizado = EstimarEspacioUtilizadoNeto();
            double porcentajeUtilizacion = totalEspacioUtilizado / totalEspacioUtil;
            return porcentajeUtilizacion;
        }

        #endregion

        #region PUBLIC METHODS

        public override string ToString()
        {
            return Id.ToString();
        }

        #endregion

        #region ICloneable Members

        /// <summary>
        /// Genera una copia de la unidad de backup
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            UnidadBackup bu = new UnidadBackup();
            bu._tramo_base = this._tramo_base;
            bu._slots = new List<SlotBackup>();
            bu._swaps = new Dictionary<string, Swap>();
            bu._t_fin_prg = this._t_fin_prg;
            bu._t_fin_rst = this._t_fin_prg;
            bu._t_ini_prg = this._t_ini_prg;
            bu._t_ini_rst = this._t_ini_prg;
            foreach (SlotBackup s in this._slots)
            {
                SlotBackup clonado = (SlotBackup)s.Clone();
                clonado.Contenedor = bu;
                bu._slots.Add(clonado);
            }
            return bu;
        }

        #region IDisposable Members

        private bool IsDisposed = false;

        /// <summary>
        /// Destructor de instancias de la clase
        /// </summary>
        ~UnidadBackup()
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
            if (!IsDisposed)
            {
                if (Disposing)
                {
                    _slots.Clear();
                    _swaps.Clear();
                }
                _swaps = null;
                _slots = null;
                _estimar_utilizacion_slot = null;
                _tramo_base = null;
            }
            IsDisposed = true;
        }
        #endregion

        #endregion
    }

    /// <summary>
    /// Estructura que encapsula estadísticos de B/UPs para reportes.
    /// </summary>
    public struct EstadisticosUtilizacion
    {
        #region ATRIBUTES
        
        /// <summary>
        /// Id de la unidad de backup
        /// </summary>
        public string id;

        /// <summary>
        /// Total de minutos de atraso recuperados gracias al slot de backup
        /// </summary>
        public double minutosRecuperados;

        /// <summary>
        /// Porcentaje de utilización considerando sólo lo usado por atrasos, ya que el slot se traspasa a otros aviones
        /// </summary>
        public double porcentajeUtilizacionNeta;

        /// <summary>
        /// Porcentaje de utilización del slot inicial programado consirando los T/A como útilies
        /// </summary>
        public double porcentajeUtilizacionSlotConTA;

        /// <summary>
        /// Porcentaje de utilización del slot inicial programado considerando sólo tiempos de vuelo
        /// </summary>
        public double porcentajeUtilizacionSlotSinTA;

        /// <summary>
        /// Total de tramos recuperados gracias al slot de backup
        /// </summary>
        public double tramosRecuperados;

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id de la unidad de backup</param>
        public EstadisticosUtilizacion(string id)
        {
            this.id = id;
            porcentajeUtilizacionNeta = 0;
            minutosRecuperados = 0;
            tramosRecuperados = 0;
            porcentajeUtilizacionSlotConTA = 0;
            porcentajeUtilizacionSlotSinTA = 0;
        }

        #endregion
    }

    
}
