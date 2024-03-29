﻿namespace InterfazSimuLAN
{
    partial class MenuSimulacionMultiescenario
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuSimulacionMultiescenario));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_sim_simple_params = new System.Windows.Forms.TabPage();
            this.groupBox_turnos = new System.Windows.Forms.GroupBox();
            this.integerTextBox_max_pairing = new AMS.TextBox.IntegerTextBox();
            this.integerTextBox_min_pairing = new AMS.TextBox.IntegerTextBox();
            this.integerTextBox_turnos = new AMS.TextBox.IntegerTextBox();
            this.label_max_conex = new System.Windows.Forms.Label();
            this.label_min_conex = new System.Windows.Forms.Label();
            this.label_tolerancia_turnos = new System.Windows.Forms.Label();
            this.groupBox_recovery = new System.Windows.Forms.GroupBox();
            this.integerTextBox_backup = new AMS.TextBox.IntegerTextBox();
            this.integerTextBox_gap = new AMS.TextBox.IntegerTextBox();
            this.integerTextBox_tolerancia = new AMS.TextBox.IntegerTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox_sim = new System.Windows.Forms.GroupBox();
            this.integerTextBox_semilla = new AMS.TextBox.IntegerTextBox();
            this.integerTextBox_replica = new AMS.TextBox.IntegerTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.Replicas = new System.Windows.Forms.Label();
            this.tabPage_sim_simple_reportes = new System.Windows.Forms.TabPage();
            this.groupBox_reportes = new System.Windows.Forms.GroupBox();
            this.checkBoxr11_punt_mult_negocio = new System.Windows.Forms.CheckBox();
            this.checkBoxr10_punt_mult = new System.Windows.Forms.CheckBox();
            this.groupBox_directorio = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox_dir = new System.Windows.Forms.TextBox();
            this.checkBox_abrir_carpeta_output = new System.Windows.Forms.CheckBox();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox_estandares = new System.Windows.Forms.GroupBox();
            this.integerTextBox_add_std = new AMS.TextBox.IntegerTextBox();
            this.button_eliminar_std = new System.Windows.Forms.Button();
            this.button_agregar_std = new System.Windows.Forms.Button();
            this.listBox_stds = new System.Windows.Forms.ListBox();
            this.ribbonMenuButton_simular = new RibbonStyle.RibbonMenuButton();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labelBarra = new System.Windows.Forms.Label();
            this.folderBrowserDialogOutput = new System.Windows.Forms.FolderBrowserDialog();
            this.label_msg = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dateTimePicker2 = new System.Windows.Forms.DateTimePicker();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.tabControl1.SuspendLayout();
            this.tabPage_sim_simple_params.SuspendLayout();
            this.groupBox_turnos.SuspendLayout();
            this.groupBox_recovery.SuspendLayout();
            this.groupBox_sim.SuspendLayout();
            this.tabPage_sim_simple_reportes.SuspendLayout();
            this.groupBox_reportes.SuspendLayout();
            this.groupBox_directorio.SuspendLayout();
            this.groupBox_estandares.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage_sim_simple_params);
            this.tabControl1.Controls.Add(this.tabPage_sim_simple_reportes);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(560, 415);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage_sim_simple_params
            // 
            this.tabPage_sim_simple_params.Controls.Add(this.groupBox_turnos);
            this.tabPage_sim_simple_params.Controls.Add(this.groupBox_recovery);
            this.tabPage_sim_simple_params.Controls.Add(this.groupBox_sim);
            this.tabPage_sim_simple_params.Location = new System.Drawing.Point(4, 22);
            this.tabPage_sim_simple_params.Name = "tabPage_sim_simple_params";
            this.tabPage_sim_simple_params.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_sim_simple_params.Size = new System.Drawing.Size(552, 389);
            this.tabPage_sim_simple_params.TabIndex = 0;
            this.tabPage_sim_simple_params.Text = "Parámetros";
            this.tabPage_sim_simple_params.UseVisualStyleBackColor = true;
            // 
            // groupBox_turnos
            // 
            this.groupBox_turnos.Controls.Add(this.integerTextBox_max_pairing);
            this.groupBox_turnos.Controls.Add(this.integerTextBox_min_pairing);
            this.groupBox_turnos.Controls.Add(this.integerTextBox_turnos);
            this.groupBox_turnos.Controls.Add(this.label_max_conex);
            this.groupBox_turnos.Controls.Add(this.label_min_conex);
            this.groupBox_turnos.Controls.Add(this.label_tolerancia_turnos);
            this.groupBox_turnos.Location = new System.Drawing.Point(6, 154);
            this.groupBox_turnos.Name = "groupBox_turnos";
            this.groupBox_turnos.Size = new System.Drawing.Size(267, 142);
            this.groupBox_turnos.TabIndex = 1;
            this.groupBox_turnos.TabStop = false;
            this.groupBox_turnos.Text = "Turnos";
            // 
            // integerTextBox_max_pairing
            // 
            this.integerTextBox_max_pairing.AllowNegative = false;
            this.integerTextBox_max_pairing.DigitsInGroup = 0;
            this.integerTextBox_max_pairing.Flags = 65536;
            this.integerTextBox_max_pairing.Location = new System.Drawing.Point(185, 79);
            this.integerTextBox_max_pairing.MaxDecimalPlaces = 0;
            this.integerTextBox_max_pairing.MaxWholeDigits = 9;
            this.integerTextBox_max_pairing.Name = "integerTextBox_max_pairing";
            this.integerTextBox_max_pairing.Prefix = "";
            this.integerTextBox_max_pairing.RangeMax = 2147483647;
            this.integerTextBox_max_pairing.RangeMin = 0;
            this.integerTextBox_max_pairing.Size = new System.Drawing.Size(54, 20);
            this.integerTextBox_max_pairing.TabIndex = 25;
            this.integerTextBox_max_pairing.Tag = "maxConex";
            this.toolTip1.SetToolTip(this.integerTextBox_max_pairing, "Máxima cantidad de minutos de conexión.");
            this.integerTextBox_max_pairing.Leave += new System.EventHandler(this.ValidarNoVacio);
            // 
            // integerTextBox_min_pairing
            // 
            this.integerTextBox_min_pairing.AllowNegative = false;
            this.integerTextBox_min_pairing.DigitsInGroup = 0;
            this.integerTextBox_min_pairing.Flags = 65536;
            this.integerTextBox_min_pairing.Location = new System.Drawing.Point(185, 52);
            this.integerTextBox_min_pairing.MaxDecimalPlaces = 0;
            this.integerTextBox_min_pairing.MaxWholeDigits = 9;
            this.integerTextBox_min_pairing.Name = "integerTextBox_min_pairing";
            this.integerTextBox_min_pairing.Prefix = "";
            this.integerTextBox_min_pairing.RangeMax = 2147483647;
            this.integerTextBox_min_pairing.RangeMin = 0;
            this.integerTextBox_min_pairing.Size = new System.Drawing.Size(54, 20);
            this.integerTextBox_min_pairing.TabIndex = 24;
            this.integerTextBox_min_pairing.Tag = "minConex";
            this.toolTip1.SetToolTip(this.integerTextBox_min_pairing, "Mínima cantidad de minutos de conexión.");
            this.integerTextBox_min_pairing.Leave += new System.EventHandler(this.ValidarNoVacio);
            // 
            // integerTextBox_turnos
            // 
            this.integerTextBox_turnos.AllowNegative = false;
            this.integerTextBox_turnos.DigitsInGroup = 0;
            this.integerTextBox_turnos.Flags = 65536;
            this.integerTextBox_turnos.Location = new System.Drawing.Point(185, 26);
            this.integerTextBox_turnos.MaxDecimalPlaces = 0;
            this.integerTextBox_turnos.MaxWholeDigits = 9;
            this.integerTextBox_turnos.Name = "integerTextBox_turnos";
            this.integerTextBox_turnos.Prefix = "";
            this.integerTextBox_turnos.RangeMax = 2147483647;
            this.integerTextBox_turnos.RangeMin = 0;
            this.integerTextBox_turnos.Size = new System.Drawing.Size(54, 20);
            this.integerTextBox_turnos.TabIndex = 17;
            this.integerTextBox_turnos.Tag = "toleranciaTurnos";
            this.toolTip1.SetToolTip(this.integerTextBox_turnos, "Minutos de atraso a partir de los cuales se activa un turno de backup.");
            this.integerTextBox_turnos.Leave += new System.EventHandler(this.ValidarNoVacio);
            // 
            // label_max_conex
            // 
            this.label_max_conex.AutoSize = true;
            this.label_max_conex.Location = new System.Drawing.Point(3, 79);
            this.label_max_conex.Name = "label_max_conex";
            this.label_max_conex.Size = new System.Drawing.Size(167, 13);
            this.label_max_conex.TabIndex = 23;
            this.label_max_conex.Text = "Minutos máximos conexión pairing";
            this.label_max_conex.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label_min_conex
            // 
            this.label_min_conex.AutoSize = true;
            this.label_min_conex.Location = new System.Drawing.Point(3, 53);
            this.label_min_conex.Name = "label_min_conex";
            this.label_min_conex.Size = new System.Drawing.Size(166, 13);
            this.label_min_conex.TabIndex = 22;
            this.label_min_conex.Text = "Minutos mínimos conexión pairing";
            // 
            // label_tolerancia_turnos
            // 
            this.label_tolerancia_turnos.AutoSize = true;
            this.label_tolerancia_turnos.Location = new System.Drawing.Point(3, 26);
            this.label_tolerancia_turnos.Name = "label_tolerancia_turnos";
            this.label_tolerancia_turnos.Size = new System.Drawing.Size(89, 13);
            this.label_tolerancia_turnos.TabIndex = 21;
            this.label_tolerancia_turnos.Text = "Tolerancia turnos";
            // 
            // groupBox_recovery
            // 
            this.groupBox_recovery.Controls.Add(this.integerTextBox_backup);
            this.groupBox_recovery.Controls.Add(this.integerTextBox_gap);
            this.groupBox_recovery.Controls.Add(this.integerTextBox_tolerancia);
            this.groupBox_recovery.Controls.Add(this.label3);
            this.groupBox_recovery.Controls.Add(this.label4);
            this.groupBox_recovery.Controls.Add(this.label5);
            this.groupBox_recovery.Location = new System.Drawing.Point(279, 6);
            this.groupBox_recovery.Name = "groupBox_recovery";
            this.groupBox_recovery.Size = new System.Drawing.Size(267, 142);
            this.groupBox_recovery.TabIndex = 1;
            this.groupBox_recovery.TabStop = false;
            this.groupBox_recovery.Text = "Recovery";
            // 
            // integerTextBox_backup
            // 
            this.integerTextBox_backup.AllowNegative = false;
            this.integerTextBox_backup.DigitsInGroup = 0;
            this.integerTextBox_backup.Flags = 65536;
            this.integerTextBox_backup.Location = new System.Drawing.Point(160, 71);
            this.integerTextBox_backup.MaxDecimalPlaces = 0;
            this.integerTextBox_backup.MaxWholeDigits = 9;
            this.integerTextBox_backup.Name = "integerTextBox_backup";
            this.integerTextBox_backup.Prefix = "";
            this.integerTextBox_backup.RangeMax = 2147483647;
            this.integerTextBox_backup.RangeMin = 0;
            this.integerTextBox_backup.Size = new System.Drawing.Size(54, 20);
            this.integerTextBox_backup.TabIndex = 16;
            this.integerTextBox_backup.Tag = "minutosBackup";
            this.toolTip1.SetToolTip(this.integerTextBox_backup, "Minutos de atraso a partir de los cuales se intenta usar un avión de backup.");
            this.integerTextBox_backup.Leave += new System.EventHandler(this.ValidarNoVacio);
            // 
            // integerTextBox_gap
            // 
            this.integerTextBox_gap.AllowNegative = false;
            this.integerTextBox_gap.DigitsInGroup = 0;
            this.integerTextBox_gap.Flags = 65536;
            this.integerTextBox_gap.Location = new System.Drawing.Point(160, 45);
            this.integerTextBox_gap.MaxDecimalPlaces = 0;
            this.integerTextBox_gap.MaxWholeDigits = 9;
            this.integerTextBox_gap.Name = "integerTextBox_gap";
            this.integerTextBox_gap.Prefix = "";
            this.integerTextBox_gap.RangeMax = 2147483647;
            this.integerTextBox_gap.RangeMin = 0;
            this.integerTextBox_gap.Size = new System.Drawing.Size(54, 20);
            this.integerTextBox_gap.TabIndex = 15;
            this.integerTextBox_gap.Tag = "gap";
            this.toolTip1.SetToolTip(this.integerTextBox_gap, "Máxima cantidad de legs que se pueden rotar por avión en un swap.");
            this.integerTextBox_gap.Leave += new System.EventHandler(this.ValidarNoVacio);
            // 
            // integerTextBox_tolerancia
            // 
            this.integerTextBox_tolerancia.AllowNegative = false;
            this.integerTextBox_tolerancia.DigitsInGroup = 0;
            this.integerTextBox_tolerancia.Flags = 65536;
            this.integerTextBox_tolerancia.Location = new System.Drawing.Point(160, 18);
            this.integerTextBox_tolerancia.MaxDecimalPlaces = 0;
            this.integerTextBox_tolerancia.MaxWholeDigits = 9;
            this.integerTextBox_tolerancia.Name = "integerTextBox_tolerancia";
            this.integerTextBox_tolerancia.Prefix = "";
            this.integerTextBox_tolerancia.RangeMax = 2147483647;
            this.integerTextBox_tolerancia.RangeMin = 0;
            this.integerTextBox_tolerancia.Size = new System.Drawing.Size(54, 20);
            this.integerTextBox_tolerancia.TabIndex = 11;
            this.integerTextBox_tolerancia.Tag = "toleranciaRecovery";
            this.toolTip1.SetToolTip(this.integerTextBox_tolerancia, "Minutos de atraso a partir de los cuales se hacen esfuerzos de recovery.");
            this.integerTextBox_tolerancia.Leave += new System.EventHandler(this.ValidarNoVacio);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 75);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "Tolerancia backup";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 49);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(135, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Profundidad máxima swaps";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(101, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Tolerancia recovery";
            // 
            // groupBox_sim
            // 
            this.groupBox_sim.Controls.Add(this.integerTextBox_semilla);
            this.groupBox_sim.Controls.Add(this.integerTextBox_replica);
            this.groupBox_sim.Controls.Add(this.label1);
            this.groupBox_sim.Controls.Add(this.Replicas);
            this.groupBox_sim.Location = new System.Drawing.Point(6, 6);
            this.groupBox_sim.Name = "groupBox_sim";
            this.groupBox_sim.Size = new System.Drawing.Size(267, 142);
            this.groupBox_sim.TabIndex = 0;
            this.groupBox_sim.TabStop = false;
            this.groupBox_sim.Text = "Simulación";
            // 
            // integerTextBox_semilla
            // 
            this.integerTextBox_semilla.AllowNegative = false;
            this.integerTextBox_semilla.DigitsInGroup = 0;
            this.integerTextBox_semilla.Flags = 65536;
            this.integerTextBox_semilla.Location = new System.Drawing.Point(60, 52);
            this.integerTextBox_semilla.MaxDecimalPlaces = 0;
            this.integerTextBox_semilla.MaxWholeDigits = 9;
            this.integerTextBox_semilla.Name = "integerTextBox_semilla";
            this.integerTextBox_semilla.Prefix = "";
            this.integerTextBox_semilla.RangeMax = 2147483647;
            this.integerTextBox_semilla.RangeMin = 0;
            this.integerTextBox_semilla.Size = new System.Drawing.Size(54, 20);
            this.integerTextBox_semilla.TabIndex = 10;
            this.integerTextBox_semilla.Tag = "semilla";
            this.toolTip1.SetToolTip(this.integerTextBox_semilla, "Número que determina las secuencias de aletoriedad en la simulación.");
            this.integerTextBox_semilla.Leave += new System.EventHandler(this.ValidarNoVacio);
            // 
            // integerTextBox_replica
            // 
            this.integerTextBox_replica.AllowNegative = false;
            this.integerTextBox_replica.DigitsInGroup = 0;
            this.integerTextBox_replica.Flags = 65536;
            this.integerTextBox_replica.Location = new System.Drawing.Point(60, 23);
            this.integerTextBox_replica.MaxDecimalPlaces = 0;
            this.integerTextBox_replica.MaxWholeDigits = 9;
            this.integerTextBox_replica.Name = "integerTextBox_replica";
            this.integerTextBox_replica.Prefix = "";
            this.integerTextBox_replica.RangeMax = 2147483647;
            this.integerTextBox_replica.RangeMin = 0;
            this.integerTextBox_replica.Size = new System.Drawing.Size(54, 20);
            this.integerTextBox_replica.TabIndex = 9;
            this.integerTextBox_replica.Tag = "replicas";
            this.toolTip1.SetToolTip(this.integerTextBox_replica, "Número de réplicas a efectuar en la simulación.");
            this.integerTextBox_replica.Leave += new System.EventHandler(this.ValidarNoVacioMayorQueCero);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Semilla";
            // 
            // Replicas
            // 
            this.Replicas.AutoSize = true;
            this.Replicas.Location = new System.Drawing.Point(6, 25);
            this.Replicas.Name = "Replicas";
            this.Replicas.Size = new System.Drawing.Size(48, 13);
            this.Replicas.TabIndex = 5;
            this.Replicas.Text = "Réplicas";
            // 
            // tabPage_sim_simple_reportes
            // 
            this.tabPage_sim_simple_reportes.Controls.Add(this.groupBox1);
            this.tabPage_sim_simple_reportes.Controls.Add(this.groupBox_reportes);
            this.tabPage_sim_simple_reportes.Controls.Add(this.groupBox_directorio);
            this.tabPage_sim_simple_reportes.Controls.Add(this.groupBox_estandares);
            this.tabPage_sim_simple_reportes.Location = new System.Drawing.Point(4, 22);
            this.tabPage_sim_simple_reportes.Name = "tabPage_sim_simple_reportes";
            this.tabPage_sim_simple_reportes.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_sim_simple_reportes.Size = new System.Drawing.Size(552, 389);
            this.tabPage_sim_simple_reportes.TabIndex = 1;
            this.tabPage_sim_simple_reportes.Text = "Reportes";
            this.tabPage_sim_simple_reportes.UseVisualStyleBackColor = true;
            // 
            // groupBox_reportes
            // 
            this.groupBox_reportes.Controls.Add(this.checkBoxr11_punt_mult_negocio);
            this.groupBox_reportes.Controls.Add(this.checkBoxr10_punt_mult);
            this.groupBox_reportes.Location = new System.Drawing.Point(6, 154);
            this.groupBox_reportes.Name = "groupBox_reportes";
            this.groupBox_reportes.Size = new System.Drawing.Size(335, 229);
            this.groupBox_reportes.TabIndex = 2;
            this.groupBox_reportes.TabStop = false;
            this.groupBox_reportes.Text = "Reportes";
            // 
            // checkBoxr11_punt_mult_negocio
            // 
            this.checkBoxr11_punt_mult_negocio.AutoSize = true;
            this.checkBoxr11_punt_mult_negocio.Checked = true;
            this.checkBoxr11_punt_mult_negocio.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxr11_punt_mult_negocio.Location = new System.Drawing.Point(6, 42);
            this.checkBoxr11_punt_mult_negocio.Name = "checkBoxr11_punt_mult_negocio";
            this.checkBoxr11_punt_mult_negocio.Size = new System.Drawing.Size(193, 17);
            this.checkBoxr11_punt_mult_negocio.TabIndex = 32;
            this.checkBoxr11_punt_mult_negocio.Tag = "11";
            this.checkBoxr11_punt_mult_negocio.Text = "Puntualidad multiescenario negocio";
            this.checkBoxr11_punt_mult_negocio.UseVisualStyleBackColor = true;
            this.checkBoxr11_punt_mult_negocio.CheckedChanged += new System.EventHandler(this.CambiarEstadoReporte);
            // 
            // checkBoxr10_punt_mult
            // 
            this.checkBoxr10_punt_mult.AutoSize = true;
            this.checkBoxr10_punt_mult.Checked = true;
            this.checkBoxr10_punt_mult.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxr10_punt_mult.Location = new System.Drawing.Point(6, 19);
            this.checkBoxr10_punt_mult.Name = "checkBoxr10_punt_mult";
            this.checkBoxr10_punt_mult.Size = new System.Drawing.Size(152, 17);
            this.checkBoxr10_punt_mult.TabIndex = 31;
            this.checkBoxr10_punt_mult.Tag = "10";
            this.checkBoxr10_punt_mult.Text = "Puntualidad multiescenario";
            this.checkBoxr10_punt_mult.UseVisualStyleBackColor = true;
            this.checkBoxr10_punt_mult.CheckedChanged += new System.EventHandler(this.CambiarEstadoReporte);
            // 
            // groupBox_directorio
            // 
            this.groupBox_directorio.Controls.Add(this.label6);
            this.groupBox_directorio.Controls.Add(this.textBox1);
            this.groupBox_directorio.Controls.Add(this.textBox_dir);
            this.groupBox_directorio.Controls.Add(this.checkBox_abrir_carpeta_output);
            this.groupBox_directorio.Controls.Add(this.button2);
            this.groupBox_directorio.Location = new System.Drawing.Point(6, 6);
            this.groupBox_directorio.Name = "groupBox_directorio";
            this.groupBox_directorio.Size = new System.Drawing.Size(335, 141);
            this.groupBox_directorio.TabIndex = 1;
            this.groupBox_directorio.TabStop = false;
            this.groupBox_directorio.Text = "Directorio de salida";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 99);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(57, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Id reportes";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(6, 115);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(323, 20);
            this.textBox1.TabIndex = 11;
            // 
            // textBox_dir
            // 
            this.textBox_dir.Location = new System.Drawing.Point(6, 48);
            this.textBox_dir.Name = "textBox_dir";
            this.textBox_dir.Size = new System.Drawing.Size(323, 20);
            this.textBox_dir.TabIndex = 8;
            this.toolTip1.SetToolTip(this.textBox_dir, "Ruta donde se guardarán los reportes.");
            this.textBox_dir.Leave += new System.EventHandler(this.CambiarOutputPath);
            // 
            // checkBox_abrir_carpeta_output
            // 
            this.checkBox_abrir_carpeta_output.AutoSize = true;
            this.checkBox_abrir_carpeta_output.Checked = true;
            this.checkBox_abrir_carpeta_output.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_abrir_carpeta_output.Location = new System.Drawing.Point(6, 74);
            this.checkBox_abrir_carpeta_output.Name = "checkBox_abrir_carpeta_output";
            this.checkBox_abrir_carpeta_output.Size = new System.Drawing.Size(135, 17);
            this.checkBox_abrir_carpeta_output.TabIndex = 7;
            this.checkBox_abrir_carpeta_output.Text = "Abrir carpeta al finalizar";
            this.toolTip1.SetToolTip(this.checkBox_abrir_carpeta_output, "Abre la carpeta que contiene los reportes al finalizar la simulación.");
            this.checkBox_abrir_carpeta_output.UseVisualStyleBackColor = true;
            this.checkBox_abrir_carpeta_output.CheckedChanged += new System.EventHandler(this.CambiarEstadoAbrirCarpetaOutput);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(6, 19);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(129, 23);
            this.button2.TabIndex = 6;
            this.button2.Text = "Seleccionar Carpeta";
            this.toolTip1.SetToolTip(this.button2, "Selección de carpeta donde se guardarán los reportes.");
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.Seleccionar_Carpeta_Output_Click);
            // 
            // groupBox_estandares
            // 
            this.groupBox_estandares.Controls.Add(this.integerTextBox_add_std);
            this.groupBox_estandares.Controls.Add(this.button_eliminar_std);
            this.groupBox_estandares.Controls.Add(this.button_agregar_std);
            this.groupBox_estandares.Controls.Add(this.listBox_stds);
            this.groupBox_estandares.Location = new System.Drawing.Point(356, 6);
            this.groupBox_estandares.Name = "groupBox_estandares";
            this.groupBox_estandares.Size = new System.Drawing.Size(190, 141);
            this.groupBox_estandares.TabIndex = 0;
            this.groupBox_estandares.TabStop = false;
            this.groupBox_estandares.Text = "Estándares de puntualidad";
            // 
            // integerTextBox_add_std
            // 
            this.integerTextBox_add_std.AllowNegative = false;
            this.integerTextBox_add_std.DigitsInGroup = 0;
            this.integerTextBox_add_std.Flags = 65536;
            this.integerTextBox_add_std.Location = new System.Drawing.Point(9, 48);
            this.integerTextBox_add_std.MaxDecimalPlaces = 0;
            this.integerTextBox_add_std.MaxWholeDigits = 9;
            this.integerTextBox_add_std.Name = "integerTextBox_add_std";
            this.integerTextBox_add_std.Prefix = "";
            this.integerTextBox_add_std.RangeMax = 2147483647;
            this.integerTextBox_add_std.RangeMin = 0;
            this.integerTextBox_add_std.Size = new System.Drawing.Size(92, 20);
            this.integerTextBox_add_std.TabIndex = 26;
            this.integerTextBox_add_std.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Add_std_KeyDown);
            // 
            // button_eliminar_std
            // 
            this.button_eliminar_std.Location = new System.Drawing.Point(9, 104);
            this.button_eliminar_std.Name = "button_eliminar_std";
            this.button_eliminar_std.Size = new System.Drawing.Size(92, 23);
            this.button_eliminar_std.TabIndex = 16;
            this.button_eliminar_std.Text = "Eliminar";
            this.toolTip1.SetToolTip(this.button_eliminar_std, "Quita estándar de puntualidad seleccionado.");
            this.button_eliminar_std.UseVisualStyleBackColor = true;
            this.button_eliminar_std.Click += new System.EventHandler(this.BorrarStd);
            // 
            // button_agregar_std
            // 
            this.button_agregar_std.Location = new System.Drawing.Point(9, 19);
            this.button_agregar_std.Name = "button_agregar_std";
            this.button_agregar_std.Size = new System.Drawing.Size(92, 23);
            this.button_agregar_std.TabIndex = 15;
            this.button_agregar_std.Text = "Agregar";
            this.toolTip1.SetToolTip(this.button_agregar_std, "Agrega estándar de puntualidad.");
            this.button_agregar_std.UseVisualStyleBackColor = true;
            this.button_agregar_std.Click += new System.EventHandler(this.AgregarStd);
            // 
            // listBox_stds
            // 
            this.listBox_stds.FormattingEnabled = true;
            this.listBox_stds.Location = new System.Drawing.Point(117, 19);
            this.listBox_stds.Name = "listBox_stds";
            this.listBox_stds.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBox_stds.Size = new System.Drawing.Size(67, 108);
            this.listBox_stds.Sorted = true;
            this.listBox_stds.TabIndex = 13;
            this.listBox_stds.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Del_stds_KeyDown);
            // 
            // ribbonMenuButton_simular
            // 
            this.ribbonMenuButton_simular.Arrow = RibbonStyle.RibbonMenuButton.e_arrow.None;
            this.ribbonMenuButton_simular.BackColor = System.Drawing.Color.Transparent;
            this.ribbonMenuButton_simular.ColorBase = System.Drawing.Color.Transparent;
            this.ribbonMenuButton_simular.ColorBaseStroke = System.Drawing.Color.Transparent;
            this.ribbonMenuButton_simular.ColorOn = System.Drawing.Color.Transparent;
            this.ribbonMenuButton_simular.ColorOnStroke = System.Drawing.Color.Transparent;
            this.ribbonMenuButton_simular.ColorPress = System.Drawing.Color.Transparent;
            this.ribbonMenuButton_simular.ColorPressStroke = System.Drawing.Color.Transparent;
            this.ribbonMenuButton_simular.FadingSpeed = 35;
            this.ribbonMenuButton_simular.FlatAppearance.BorderSize = 0;
            this.ribbonMenuButton_simular.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ribbonMenuButton_simular.GroupPos = RibbonStyle.RibbonMenuButton.e_groupPos.None;
            this.ribbonMenuButton_simular.Image = ((System.Drawing.Image)(resources.GetObject("ribbonMenuButton_simular.Image")));
            this.ribbonMenuButton_simular.ImageLocation = RibbonStyle.RibbonMenuButton.e_imagelocation.Top;
            this.ribbonMenuButton_simular.ImageOffset = 0;
            this.ribbonMenuButton_simular.IsPressed = false;
            this.ribbonMenuButton_simular.KeepPress = false;
            this.ribbonMenuButton_simular.Location = new System.Drawing.Point(264, 433);
            this.ribbonMenuButton_simular.MaxImageSize = new System.Drawing.Point(0, 0);
            this.ribbonMenuButton_simular.MenuPos = new System.Drawing.Point(0, 0);
            this.ribbonMenuButton_simular.Name = "ribbonMenuButton_simular";
            this.ribbonMenuButton_simular.Radius = 6;
            this.ribbonMenuButton_simular.ShowBase = RibbonStyle.RibbonMenuButton.e_showbase.Yes;
            this.ribbonMenuButton_simular.Size = new System.Drawing.Size(56, 75);
            this.ribbonMenuButton_simular.SplitButton = RibbonStyle.RibbonMenuButton.e_splitbutton.No;
            this.ribbonMenuButton_simular.SplitDistance = 0;
            this.ribbonMenuButton_simular.TabIndex = 1;
            this.ribbonMenuButton_simular.Text = "Simular";
            this.ribbonMenuButton_simular.Title = "";
            this.ribbonMenuButton_simular.UseVisualStyleBackColor = true;
            this.ribbonMenuButton_simular.Click += new System.EventHandler(this.Simular_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(253, 445);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(78, 80);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 9;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Visible = false;
            // 
            // labelBarra
            // 
            this.labelBarra.AutoSize = true;
            this.labelBarra.BackColor = System.Drawing.Color.Transparent;
            this.labelBarra.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelBarra.Location = new System.Drawing.Point(281, 429);
            this.labelBarra.Name = "labelBarra";
            this.labelBarra.Size = new System.Drawing.Size(33, 13);
            this.labelBarra.TabIndex = 8;
            this.labelBarra.Text = "100%";
            this.labelBarra.Visible = false;
            // 
            // label_msg
            // 
            this.label_msg.AutoSize = true;
            this.label_msg.Location = new System.Drawing.Point(13, 530);
            this.label_msg.Name = "label_msg";
            this.label_msg.Size = new System.Drawing.Size(0, 13);
            this.label_msg.TabIndex = 11;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dateTimePicker2);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.dateTimePicker1);
            this.groupBox1.Location = new System.Drawing.Point(356, 154);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(190, 82);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Fechas";
            // 
            // dateTimePicker2
            // 
            this.dateTimePicker2.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePicker2.Location = new System.Drawing.Point(68, 51);
            this.dateTimePicker2.Name = "dateTimePicker2";
            this.dateTimePicker2.Size = new System.Drawing.Size(110, 20);
            this.dateTimePicker2.TabIndex = 3;
            this.toolTip1.SetToolTip(this.dateTimePicker2, "Fecha de término desde la cual se considera información en los reportes (inclusiv" +
                    "e).");
            this.dateTimePicker2.ValueChanged += new System.EventHandler(this.ValidarFechaTermino);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 57);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(45, 13);
            this.label8.TabIndex = 2;
            this.label8.Text = "Término";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 26);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(32, 13);
            this.label7.TabIndex = 1;
            this.label7.Text = "Inicio";
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePicker1.Location = new System.Drawing.Point(68, 19);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(110, 20);
            this.dateTimePicker1.TabIndex = 0;
            this.toolTip1.SetToolTip(this.dateTimePicker1, "Fecha de inicio desde la cual se considera información en los reportes (inclusive" +
                    ").");
            this.dateTimePicker1.ValueChanged += new System.EventHandler(this.ValidarFechaInicio);
            // 
            // MenuSimulacionMultiescenario
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 564);
            this.Controls.Add(this.label_msg);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.labelBarra);
            this.Controls.Add(this.ribbonMenuButton_simular);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 600);
            this.Name = "MenuSimulacionMultiescenario";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Simulación multiescenario";
            this.tabControl1.ResumeLayout(false);
            this.tabPage_sim_simple_params.ResumeLayout(false);
            this.groupBox_turnos.ResumeLayout(false);
            this.groupBox_turnos.PerformLayout();
            this.groupBox_recovery.ResumeLayout(false);
            this.groupBox_recovery.PerformLayout();
            this.groupBox_sim.ResumeLayout(false);
            this.groupBox_sim.PerformLayout();
            this.tabPage_sim_simple_reportes.ResumeLayout(false);
            this.groupBox_reportes.ResumeLayout(false);
            this.groupBox_reportes.PerformLayout();
            this.groupBox_directorio.ResumeLayout(false);
            this.groupBox_directorio.PerformLayout();
            this.groupBox_estandares.ResumeLayout(false);
            this.groupBox_estandares.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_sim_simple_params;
        private System.Windows.Forms.GroupBox groupBox_recovery;
        private System.Windows.Forms.GroupBox groupBox_sim;
        private System.Windows.Forms.TabPage tabPage_sim_simple_reportes;
        private System.Windows.Forms.GroupBox groupBox_turnos;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label Replicas;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label_max_conex;
        private System.Windows.Forms.Label label_min_conex;
        private System.Windows.Forms.Label label_tolerancia_turnos;
        private RibbonStyle.RibbonMenuButton ribbonMenuButton_simular;
        private System.Windows.Forms.GroupBox groupBox_directorio;
        private System.Windows.Forms.GroupBox groupBox_estandares;
        private System.Windows.Forms.Button button_eliminar_std;
        private System.Windows.Forms.Button button_agregar_std;
        private System.Windows.Forms.ListBox listBox_stds;
        private System.Windows.Forms.CheckBox checkBox_abrir_carpeta_output;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox_dir;
        private System.Windows.Forms.GroupBox groupBox_reportes;
        private System.Windows.Forms.CheckBox checkBoxr11_punt_mult_negocio;
        private System.Windows.Forms.CheckBox checkBoxr10_punt_mult;
        private AMS.TextBox.IntegerTextBox integerTextBox_replica;
        private AMS.TextBox.IntegerTextBox integerTextBox_max_pairing;
        private AMS.TextBox.IntegerTextBox integerTextBox_min_pairing;
        private AMS.TextBox.IntegerTextBox integerTextBox_turnos;
        private AMS.TextBox.IntegerTextBox integerTextBox_backup;
        private AMS.TextBox.IntegerTextBox integerTextBox_gap;
        private AMS.TextBox.IntegerTextBox integerTextBox_tolerancia;
        private AMS.TextBox.IntegerTextBox integerTextBox_semilla;
        private AMS.TextBox.IntegerTextBox integerTextBox_add_std;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label labelBarra;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogOutput;
        private System.Windows.Forms.Label label_msg;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DateTimePicker dateTimePicker2;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
    }
}