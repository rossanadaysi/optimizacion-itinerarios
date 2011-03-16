using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SimuLAN.Clases;
using SimuLAN.Utils;
using SimuLAN.Clases.Recovery;

namespace InterfazSimuLAN
{
    /// <summary>
    /// Form con formulario para agregar un nuevo slot de backup
    /// </summary>
    public partial class AgregarBU : Form
    {
        #region ATRIBUTES

        /// <summary>
        /// Fecha de inicio mínima permitida para el slot
        /// </summary>
        private DateTime _fecha_ini_min;

        /// <summary>
        /// Fecha de término máxima permitida para el slot
        /// </summary>
        private DateTime _fecha_termino_max;

        /// <summary>
        /// Referencia a la interfaz
        /// </summary>
        internal InterfazSimuLAN _main;

        /// <summary>
        /// Tramo base del slot
        /// </summary>
        private TramoBase _tramo_base_slot;

        /// <summary>
        /// Tramo inicial del slot
        /// </summary>
        private Tramo _tramo_ini;

        /// <summary>
        /// Tramo final del slot
        /// </summary>
        private Tramo _tramo_final;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tramo_previo">Tramo previo al slot</param>
        /// <param name="tramo_final">Tramo siguiente al slot</param>
        /// <param name="main">Referncia a la interfaz de SimuLAN</param>
        public AgregarBU(Tramo tramo_previo, Tramo tramo_final, InterfazSimuLAN main)
        {
            InitializeComponent();
            this._tramo_ini = tramo_previo;
            this._tramo_final = tramo_final;
            this._tramo_base_slot = new TramoBase();
            this._fecha_ini_min = new DateTime();
            this._fecha_termino_max = new DateTime();
            this._main = main;
            CargarInfoBaseSlot();
            CargarTextBoxes();
            ValidarFechas(null, new EventArgs());
        }

        #endregion

        #region PRIVATE METHODS
        
        /// <summary>
        /// Agrega unidad de backup.
        /// Se ejecuta al hacer click sobre el botón aceptar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Aceptar_Click(object sender, EventArgs e)
        {
            _main._itinerarioBase.ContadorTramos++;
            _tramo_base_slot.Numero_Global++;
            UnidadBackup bu = new UnidadBackup(_tramo_base_slot, _main._itinerarioBase.FechaInicio);
            _main._itinerarioBase.ControladorBackups.BackupsLista.Add(bu);
            Avion avionSeleccionado = _tramo_ini.GetAvion(_tramo_ini.TramoBase.Numero_Ac.ToString());
            if (_tramo_ini.Tramo_Previo == null)
            {
                if (_tramo_ini.Tramo_Siguiente == null)
                {
                    avionSeleccionado.Tramo_Raiz = null;
                    avionSeleccionado.Tramo_Actual = null;
                }
                else
                {
                    avionSeleccionado.Tramo_Raiz = _tramo_final.Tramo_Siguiente;
                    avionSeleccionado.Tramo_Actual = _tramo_final.Tramo_Siguiente;
                    if (_tramo_final.Tramo_Siguiente != null)
                    {
                        _tramo_final.Tramo_Siguiente.Tramo_Previo = null;
                    }
                }
            }
            else
            {
                if (_tramo_final.Tramo_Siguiente == null)
                {
                    _tramo_ini.Tramo_Previo.Tramo_Siguiente = null;
                }
                else
                {
                    _tramo_ini.Tramo_Previo.Tramo_Siguiente = _tramo_final.Tramo_Siguiente;
                    _tramo_final.Tramo_Siguiente.Tramo_Previo = _tramo_ini.Tramo_Previo;
                    Tramo aux = _tramo_ini;
                    Avion a = _tramo_ini.GetAvion(_tramo_ini.IdAvionProgramado);
                    while(aux!=null && aux!=_tramo_final.Tramo_Siguiente)
                    {
                        a.Legs.Remove(aux);
                        aux = aux.Tramo_Siguiente;
                    }
                    
                }
            }
            _tramo_ini.Tramo_Previo = null;
            _tramo_final.Tramo_Siguiente = null;
            _main.CargarItinerarioEnListView(null, new EventArgs());
            _main.Refresh();
            Close();
        }
        
        /// <summary>
        /// Ajusta el valor de la duración en función de la fecha ingresada
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CambiarDuracion(object sender, EventArgs e)
        {
            int minutosDuracion = TextBox_Duracion.Int;
            dateTimePicker_fecha_termino.Value = dateTimePicker_fecha_ini.Value.AddMinutes(minutosDuracion);
            ValidarFechas(null, new EventArgs());
        }

        /// <summary>
        /// Cierra el formulario
        /// Acción ejecutada con el botón cancelar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancelar_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Carga información de tramos base del slot
        /// </summary>
        private void CargarInfoBaseSlot()
        {
            _tramo_base_slot.Origen = _tramo_ini.TramoBase.Origen;
            _tramo_base_slot.Destino = _tramo_ini.TramoBase.Origen;
            _tramo_base_slot.Ac_Owner = _tramo_ini.TramoBase.Ac_Owner;
            _tramo_base_slot.AcType = _tramo_ini.TramoBase.AcType;
            _tramo_base_slot.Carrier = "BU";
            _tramo_base_slot.Config_Asientos = _tramo_ini.TramoBase.Config_Asientos;
            _tramo_base_slot.Dom_Int = _tramo_ini.TramoBase.Dom_Int;
            _tramo_base_slot.Fecha_Salida = (_tramo_ini.Tramo_Previo!=null) ? _tramo_ini.Tramo_Previo.TramoBase.Fecha_Llegada : _tramo_ini.TramoBase.Fecha_Salida;
            _tramo_base_slot.Fecha_Llegada = (_tramo_final.Tramo_Siguiente!=null)?_tramo_final.Tramo_Siguiente.TramoBase.Fecha_Salida: _tramo_final.TramoBase.Fecha_Llegada;
            _tramo_base_slot.Hora_Salida = (_tramo_ini.Tramo_Previo != null) ? _tramo_ini.Tramo_Previo.TramoBase.Hora_Llegada : "0";
            _tramo_base_slot.Hora_Llegada = (_tramo_final.Tramo_Siguiente != null) ? _tramo_final.Tramo_Siguiente.TramoBase.Hora_Salida : "2359";
            _tramo_base_slot.Numero_Ac = _tramo_ini.TramoBase.Numero_Ac;
            _tramo_base_slot.Numero_Global = _main._itinerarioBase.ContadorTramos;//Cuando se cree, aumentarlo a uno acá y en el itinerario.
            _tramo_base_slot.Numero_Tramo = 0;
            _tramo_base_slot.Numero_Vuelo = "0";
            _tramo_base_slot.NumSubFlota = _tramo_ini.TramoBase.NumSubFlota;
            _tramo_base_slot.Op_Suf = "Z";
            _tramo_base_slot.Stc = "Z";                 
        }

        /// <summary>
        /// Carga valores por defecto en los TextBoxes del formulario
        /// </summary>
        private void CargarTextBoxes()
        {
            this.TextBox_Estacion.Text = _tramo_base_slot.Origen;
            this.TextBox_Matricula.Text = _tramo_base_slot.Numero_Ac;            
            this.TextBox_Inicio_Min.Text = _tramo_base_slot.Fecha_Salida.ToShortDateString() + " " + Utilidades.GetHora(_tramo_base_slot.Hora_Salida);
            this.TextBox_Termino_Max.Text = _tramo_base_slot.Fecha_Llegada.ToShortDateString() + " " + Utilidades.GetHora(_tramo_base_slot.Hora_Llegada);
            int turn_around_min = this._tramo_final.GetTurnAroundMinimo(this._tramo_final);
            this._fecha_ini_min = Convert.ToDateTime(TextBox_Inicio_Min.Text).AddMinutes(turn_around_min);
            this._fecha_termino_max = Convert.ToDateTime(TextBox_Termino_Max.Text).AddMinutes(-turn_around_min);
            this.TextBox_Inicio_Min.Text = this._fecha_ini_min.ToString("yyyy-MM-dd HH:mm");
            this.TextBox_Termino_Max.Text = this._fecha_termino_max.ToString("yyyy-MM-dd HH:mm");
            this.dateTimePicker_fecha_ini.Value = this._fecha_ini_min;
            this.dateTimePicker_fecha_termino.Value = this._fecha_termino_max;
            
        }

        /// <summary>
        /// Hace que la ventana quede centrada.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            int x = Convert.ToInt16(_main.Bounds.X + _main.Bounds.Size.Width / 2 - this.Size.Width / 2);
            int y = Convert.ToInt16(_main.Bounds.Y + _main.Bounds.Size.Height / 2 - this.Size.Height / 2);
            Rectangle r = new Rectangle(x, y, this.Bounds.Width, this.Bounds.Height);
            this.Bounds = r;            
        }

        /// <summary>
        /// Hace validación de las fechas ingresadas al formulario
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValidarFechas(object sender, EventArgs e)
        {
            DateTime aux1 = (dateTimePicker_fecha_ini.Value < _fecha_ini_min) ? _fecha_ini_min : dateTimePicker_fecha_ini.Value;
            DateTime aux2 = (dateTimePicker_fecha_termino.Value > _fecha_termino_max) ? _fecha_termino_max : dateTimePicker_fecha_termino.Value;
            dateTimePicker_fecha_ini.Value = (aux1 >= _fecha_ini_min && aux1 > aux2) ? aux2.AddMinutes(-1) : aux1;
            dateTimePicker_fecha_termino.Value = (aux2 <= _fecha_termino_max && aux2 < aux1) ? aux1.AddMinutes(1) : aux2;
            _tramo_base_slot.Fecha_Salida = Convert.ToDateTime(dateTimePicker_fecha_ini.Value.ToShortDateString());
            _tramo_base_slot.Fecha_Llegada = Convert.ToDateTime(dateTimePicker_fecha_termino.Value.ToShortDateString());
            _tramo_base_slot.Hora_Salida = String.Concat(dateTimePicker_fecha_ini.Value.ToShortTimeString().Split(':'));
            _tramo_base_slot.Hora_Llegada = String.Concat(dateTimePicker_fecha_termino.Value.ToShortTimeString().Split(':'));
            TextBox_Duracion.Int = Convert.ToInt32((dateTimePicker_fecha_termino.Value - dateTimePicker_fecha_ini.Value).TotalMinutes);
        }

        #endregion
    }
}
