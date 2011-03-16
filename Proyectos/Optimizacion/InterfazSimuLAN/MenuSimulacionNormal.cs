using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using AMS.TextBox;
using SimuLAN.Clases;
using InterfazSimuLAN.Reportes;
using System.Collections;
using System.IO;

namespace InterfazSimuLAN
{
    public delegate void OcultarVentanaEventHandler();

    /// <summary>
    /// Form de simulación normal
    /// </summary>
    public partial class MenuSimulacionNormal : Form
    {
        #region ATRIBUTES

        /// <summary>
        /// Delegado para imprimir mensajes al pie del form
        /// </summary>
        private EnviarMensajeEventHandler _enviar_mensaje;

        /// <summary>
        /// Referencia a la interfaz de SimuLAN
        /// </summary>
        private InterfazSimuLAN _main;

        /// <summary>
        /// Thread que ejecuta proceso de simulación
        /// </summary>
        private Thread _thead_simulacion;


        public CambiarVistaSimularEventHandler GetCambiarVistaSimulacion
        {
            get { return new CambiarVistaSimularEventHandler(this.CambiarVistaSimulacion); }
        }
        public ActualizarPorcentajeEventHandler GetActualizarPorcentaje
        {
            get { return new ActualizarPorcentajeEventHandler(this.ActualizarPorcentaje); }
        }
        #endregion

        #region PROPERTIES

        /// <summary>
        /// Fecha de inicio del procesamiento de los reportes.
        /// </summary>
        public DateTime FechaInicioReportes
        {
            get { return dateTimePicker1.Value; }
        }

        /// <summary>
        /// Fecha de término del procesamiento de los reportes.
        /// </summary>
        public DateTime FechaTerminoReportes
        {
            get { return dateTimePicker2.Value; }
        }

        /// <summary>
        /// String con el sufijo especial para los reportes
        /// </summary>
        public string ID_Reportes
        {
            get { return this.textBox1.Text; }
        }

        /// <summary>
        /// Retorna delegado que encapsula método Hide() del form.
        /// </summary>
        public OcultarVentanaEventHandler OcultarVentana
        {
            get { return new OcultarVentanaEventHandler(Hide); }
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor del form
        /// </summary>
        /// <param name="main">Referencia a la interfaz de SimuLAN</param>
        public MenuSimulacionNormal(InterfazSimuLAN main)
        {
            InitializeComponent();
            this._main = main;
            Inicializar();
            this._enviar_mensaje = new EnviarMensajeEventHandler(EnviarMensajeLabelMensajes);
        }

        #endregion

        #region INTERNAL METHODS

        /// <summary>
        /// Actualiza la lista de estándares y el directorio de output.
        /// </summary>
        internal void Actualizar()
        {
            ActualizarListBoxStds();
            textBox_dir.Text = _main._outputPath;
            label_msg.Text = "";
        }

        /// <summary>
        /// Actualiza el porcentaje de progreso de la simulación
        /// </summary>
        /// <param name="s">String con el porcentaje actual</param>
        internal void ActualizarPorcentaje(string s)
        {
            this.Invoke(new ActualizarPorcentajeEventHandler(ActualizarPorcentaje2), s);

        }
        /// <summary>
        /// Actualiza el porcentaje de progreso de la simulación
        /// </summary>
        /// <param name="s">String con el porcentaje actual</param>
        internal void ActualizarPorcentaje2(string s)
        {
            labelBarra.Text = s;
            labelBarra.Refresh();
        }

        /// <summary>
        /// Actualiza setting de check box para abrir la carpeta de output al finalizar la generación de reportes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void CambiarEstadoAbrirCarpetaOutput(object sender, EventArgs e)
        {
            CheckBox selected = (CheckBox)sender;
            _main._abrirCarpetaOutput = selected.Checked;
            _main._config.ReplaceSetting("check_output_dir", selected.Checked.ToString());
        }

        /// <summary>
        /// Actualiza setting de check box para crear un grupo en los reportes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void CambiarEstadoGrupo(object sender, EventArgs e)
        {
            CheckBox selected = (CheckBox)sender;
            _main._escribeGrupo[(GruposReporte)Enum.GetValues(typeof(GruposReporte)).GetValue(Convert.ToInt16(Convert.ToInt16(selected.Tag) - 1))] = selected.Checked;
            _main._config.ReplaceSetting("check_g" + selected.Tag, selected.Checked.ToString());
        }

        /// <summary>
        /// Actualiza setting de check box para crear un reporte.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void CambiarEstadoReporte(object sender, EventArgs e)
        {
            CheckBox selected = (CheckBox)sender;
            _main._escribeReporte[(NombreReporte)Enum.GetValues(typeof(NombreReporte)).GetValue(Convert.ToInt16(selected.Tag) - 1)] = selected.Checked;
            _main._config.ReplaceSetting("check_r" + selected.Tag, selected.Checked.ToString());
        }

        /// <summary>
        /// Cambia la vista del form dependiendo si se está en proceso de simulación o no.
        /// </summary>
        /// <param name="cursor">Tipo de cursor seteado</param>
        internal void CambiarVistaSimulacion()
        {
            this.Invoke(new CambiarVistaSimularEventHandler(CambiarVistaSimulacion2));
        }

        private void CambiarVistaSimulacion2()
        {           
            this.pictureBox1.Visible = !this.pictureBox1.Visible;
            this.labelBarra.Visible = !this.labelBarra.Visible;
            this.ribbonMenuButton_simular.Visible = !this.labelBarra.Visible;
            this._main.Enabled = !this._main.Enabled;
            this.groupBox_esc.Enabled = !this.groupBox_esc.Enabled;
            this.groupBox_recovery.Enabled = !this.groupBox_recovery.Enabled;
            this.groupBox_sim.Enabled = !this.groupBox_sim.Enabled;
            this.groupBox_turnos.Enabled = !this.groupBox_turnos.Enabled;
            this.groupBox_estandares.Enabled = !this.groupBox_estandares.Enabled;
            Cursor actual = this._main.Cursor;
            if (actual == Cursors.Default)
            {
                this.Cursor = Cursors.WaitCursor;
                this._main.Cursor = Cursors.WaitCursor;
            }
            else
            {
                this.Cursor = Cursors.Default;
                this._main.Cursor = Cursors.Default;
            }            
        }

        /// <summary>
        /// Inicializa el selector de fechas para los reportes
        /// </summary>
        internal void CargarFechas()
        {
            dateTimePicker1.MinDate = _main._itinerarioBase.FechaInicio;
            dateTimePicker1.MaxDate = _main._itinerarioBase.FechaTermino;
            dateTimePicker1.Value = _main._itinerarioBase.FechaInicio;
            dateTimePicker2.MinDate = _main._itinerarioBase.FechaInicio;
            dateTimePicker2.MaxDate = _main._itinerarioBase.FechaTermino;
            dateTimePicker2.Value = _main._itinerarioBase.FechaTermino;
        }

        /// <summary>
        /// Actualiza el Form
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            ActualizarListBoxStds();
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Actualiza setting de estándares de puntualidad cargados por defecto.
        /// </summary>
        private void ActualizarListBoxStds()
        {
            listBox_stds.Items.Clear();
            if (_main._stds.Count == 0)
            {
                _main._stds.Add(0);
            }
            string s = "";
            foreach (int std in _main._stds)
            {
                listBox_stds.Items.Add(std);
                s += std.ToString();
                if (_main._stds.IndexOf(std) != _main._stds.Count - 1)
                {
                    s += ";";
                }
            }
            _main._config.ReplaceSetting("stds", s);
            listBox_stds.Refresh();
        }

        /// <summary>
        /// Metodo de acción para agregar un std a la lista de estándares de puntualidad presionando enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Add_std_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                AgregarStd(sender, new EventArgs());
            }
        }

        /// <summary>
        /// Metodo de acción para agregar un std a al lista de estándares de puntualidad presionando el botón "Agregar"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AgregarStd(object sender, EventArgs e)
        {
            int std = integerTextBox_add_std.Int;
            if (!_main._stds.Contains(std))
            {
                _main._stds.Add(std);
                ActualizarListBoxStds();
            }
            integerTextBox_add_std.Text = "";
        }

        /// <summary>
        /// Metodo de acción para borrar un std seleccionado de lista de estándares de puntualidad presionando el botón "Borrar"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorrarStd(object sender, EventArgs e)
        {
            if (listBox_stds.SelectedItems != null && listBox_stds.SelectedItems.Count > 0)
            {
                IEnumerator enumItems = listBox_stds.SelectedItems.GetEnumerator();
                string eliminados = "";
                int countEliminados = 0;
                while (enumItems.MoveNext())
                {
                    int valor = Convert.ToInt32(enumItems.Current);
                    eliminados += valor + " ";
                    _main._stds.Remove(valor);
                    countEliminados++;
                }
                ActualizarListBoxStds();
            }
        }

        /// <summary>
        /// Método para cambiar la carpeta del diálogo de selección de carpetas a partir del texto del TextBox de directorio
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CambiarOutputPath(object sender, EventArgs e)
        {
            folderBrowserDialogOutput.SelectedPath = textBox_dir.Text;
        }
        
        /// <summary>
        /// Método para cargar los valores iniciales de parámetros del Form
        /// </summary>
        private void CargarTextBoxes()
        {
            EventArgs e = new EventArgs();
            ValidarNoVacio(integerTextBox_backup, e);
            ValidarNoVacio(integerTextBox_gap, e);
            ValidarNoVacio(integerTextBox_max_pairing, e);
            ValidarNoVacio(integerTextBox_min_pairing, e);
            ValidarNoVacio(integerTextBox_replica, e);
            ValidarNoVacio(integerTextBox_semilla, e);
            ValidarNoVacio(integerTextBox_tolerancia, e);
            ValidarNoVacio(integerTextBox_turnos, e);
            textBox_dir.Text = _main._outputPath;
            folderBrowserDialogOutput.SelectedPath = _main._outputPath;
        }

        /// <summary>
        /// Método para cargar los valores iniciales de checkBoxes del Form
        /// </summary>
        private void CargarCheckBoxes()
        {
            checkBox_abrir_carpeta_output.Checked = _main._abrirCarpetaOutput;
            checkBoxg1_operador.Checked = _main._escribeGrupo[GruposReporte.Operador];
            checkBoxg2_negocio.Checked = _main._escribeGrupo[GruposReporte.Negocio];
            checkBoxg3_flota.Checked = _main._escribeGrupo[GruposReporte.Flota];
            checkBoxg4_subflota.Checked = _main._escribeGrupo[GruposReporte.Subflota];
            checkBoxg5_matricula.Checked = _main._escribeGrupo[GruposReporte.Matricula];
            checkBoxg6_estacion.Checked = _main._escribeGrupo[GruposReporte.Estacion];
            checkBoxg7_od.Checked = _main._escribeGrupo[GruposReporte.ParOD];
            checkBoxg8_vuelo.Checked = _main._escribeGrupo[GruposReporte.Vuelo];
            checkBoxg9_hub.Checked = _main._escribeGrupo[GruposReporte.HubSalida];
            checkBoxr1_punt_gral.Checked = _main._escribeReporte[NombreReporte.PuntualidadGeneral];
            checkBoxr2_punt_grupos.Checked = _main._escribeReporte[NombreReporte.PuntualidadGrupos];
            checkBoxr3_imp_gral.Checked = _main._escribeReporte[NombreReporte.ExplicacionImpuntualidadGeneral];
            checkBoxr4_imp_grup.Checked = _main._escribeReporte[NombreReporte.ExplicacionImpuntualidadGrupos];
            checkBoxr5_ut_bup.Checked = _main._escribeReporte[NombreReporte.UtilizacionBackup];
            checkBoxr6_ut_turnos.Checked = _main._escribeReporte[NombreReporte.UtilizacionTurnos];
            checkBoxr7_mantto.Checked = _main._escribeReporte[NombreReporte.PuntualidadMantto];
            checkBoxr8_recovery.Checked = _main._escribeReporte[NombreReporte.Recovery];
            checkBoxr9_detalles.Checked = _main._escribeReporte[NombreReporte.Detalles]; 
        }

        /// <summary>
        /// Método para cargar los valores iniciales de los comboBoxes de escenarios del Form
        /// </summary>
        private void CargarComboBoxEscenarios()
        {
            foreach (TipoEscenarioDisrupcion tipo in Enum.GetValues(typeof(TipoEscenarioDisrupcion)))
            {
                comboBox_esc_mantto.Items.Add(tipo);
                comboBox_esc_wxs.Items.Add(tipo);
            }
            comboBox_esc_mantto.SelectedItem = TipoEscenarioDisrupcion.Normal;
            comboBox_esc_wxs.SelectedItem = TipoEscenarioDisrupcion.Normal;
        }

        /// <summary>
        /// Metodo de acción para borrar un std de la lista de estándares de puntualidad presionando DEL o BACK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Del_stds_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                BorrarStd(sender, new EventArgs());
            }
        }

        /// <summary>
        /// Invoca método para enviar un mensaje al label de mensajes
        /// </summary>
        /// <param name="mensaje">String con el mensaje a desplegar</param>
        private void EnviarMensajeLabelMensajes(string mensaje)
        {
            Invoke(new EnviarMensajeEventHandler(EnviarMsj), mensaje);
        }

        /// <summary>
        /// Setea mensaje en label de mensajes
        /// </summary>
        /// <param name="mensaje">String con el mensaje a desplegar</param>
        private void EnviarMsj(string mensaje)
        {
            label_msg.Text = mensaje;
        }

        /// <summary>
        /// Inicializa valores del Form
        /// </summary>
        private void Inicializar()
        {
            CargarTextBoxes();
            CargarCheckBoxes();
            CargarComboBoxEscenarios();
            ActualizarListBoxStds();
        }

        /// <summary>
        /// Sobreescribe acción Close para esconder la ventana en vez de cerrarla.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true;
            _main._simulacionSimpleAbierta = false;
            if (Directory.Exists(textBox_dir.Text))
            {
                _main._outputPath = textBox_dir.Text;
            }
            if (_main._simulando)
            {
                _main._simulacion_cancelada = true;
            }
            if (!_main._simulacion_cancelada)
            {
                this.Hide();
            }
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

        private void OptimizarClick(object sender, EventArgs e)
        {
            _main._modeloDisrupcionesBase.MapDisrupcionesEscenario[TipoDisrupcion.METEREOLOGIA] = (TipoEscenarioDisrupcion)comboBox_esc_wxs.SelectedItem;
            _main._modeloDisrupcionesBase.MapDisrupcionesEscenario[TipoDisrupcion.MANTENIMIENTO] = (TipoEscenarioDisrupcion)comboBox_esc_mantto.SelectedItem;
            _main._config.ReplaceSetting("output_dir", folderBrowserDialogOutput.SelectedPath);
            _main._outputPath = folderBrowserDialogOutput.SelectedPath;
            _main.SetMensajeSimulacion = _enviar_mensaje;
            _thead_simulacion = new Thread(new ThreadStart(_main.Optimizar));
            _thead_simulacion.Start();
        }

        /// <summary>
        /// Método de acción al presionar boton de seleccionar carpera de output.
        /// Se abre diálogo de selección de carpetas y se actualzia string de path.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Seleccionar_Carpeta_Output_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialogOutput.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox_dir.Text = folderBrowserDialogOutput.SelectedPath;
            }
        }

        /// <summary>
        /// Método que ejecuta la simulación al presionar el botón simular.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Simular_Click(object sender, EventArgs e)
        {
            _main._modeloDisrupcionesBase.MapDisrupcionesEscenario[TipoDisrupcion.METEREOLOGIA] = (TipoEscenarioDisrupcion)comboBox_esc_wxs.SelectedItem;
            _main._modeloDisrupcionesBase.MapDisrupcionesEscenario[TipoDisrupcion.MANTENIMIENTO] = (TipoEscenarioDisrupcion)comboBox_esc_mantto.SelectedItem;
            _main._config.ReplaceSetting("output_dir", folderBrowserDialogOutput.SelectedPath);
            _main._outputPath = folderBrowserDialogOutput.SelectedPath;
            _main.SetMensajeSimulacion = _enviar_mensaje;
            _thead_simulacion = new Thread(new ThreadStart(_main.SimularNormal));
            _thead_simulacion.Start();
        }


        /// <summary>
        /// Valida que la fecha de inicio no sea mayor que la fecha de término. Si lo es, por defecto setea su el valor en el mínimo.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValidarFechaInicio(object sender, EventArgs e)
        {
            DateTime ini = dateTimePicker1.Value;
            DateTime fin = dateTimePicker2.Value;
            if (ini > fin)
            {
                dateTimePicker1.Value = dateTimePicker1.MinDate;
            }
        }

        /// <summary>
        /// Valida que la fecha de termino no sea menor que la fecha de inicio. Si lo es, por defecto setea su el valor en el máximo.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValidarFechaTermino(object sender, EventArgs e)
        {
            DateTime ini = dateTimePicker1.Value;
            DateTime fin = dateTimePicker2.Value;
            if (fin < ini)
            {
                dateTimePicker2.Value = dateTimePicker1.MaxDate;
            }
        }

        /// <summary>
        /// Valida que un textBox no esté vacío
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValidarNoVacio(object sender,EventArgs e)
        {
            IntegerTextBox tb = (IntegerTextBox)sender;
            if (tb.Text.Length == 0)
            {
                tb.Text = _main._config.GetParametro(tb.Tag.ToString()).ToString();
            }
            else
            {
                int valor = Convert.ToInt16(tb.Text);
                _main._parametrosBase.Escalares.SetValor(tb.Tag, valor);
                tb.Text = valor.ToString();
                _main._config.ReplaceSetting(tb.Tag.ToString(), valor.ToString());
                _main._config.SaveConfiguracion();
            }
        }

        /// <summary>
        /// Valida que un textBox no esté vacío y que tenga un número mayor que cero
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValidarNoVacioMayorQueCero(object sender, EventArgs e)
        {
            IntegerTextBox tb = (IntegerTextBox)sender;
            if (tb.Text.Length == 0 || Convert.ToInt32(tb.Text) == 0)
            {
                tb.Text = _main._config.GetParametro(tb.Tag.ToString()).ToString();
            }
            else
            {
                int valor = Convert.ToInt16(tb.Text);
                _main._parametrosBase.Escalares.SetValor(tb.Tag, valor);
                tb.Text = valor.ToString();
                _main._config.ReplaceSetting(tb.Tag.ToString(), valor.ToString());
                _main._config.SaveConfiguracion();           
            }
        }

        #endregion

       

    }
}
