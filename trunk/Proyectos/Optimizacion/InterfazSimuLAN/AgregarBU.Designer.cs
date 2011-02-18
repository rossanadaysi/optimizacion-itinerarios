namespace InterfazSimuLAN
{
    partial class AgregarBU
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
            this.button_aceptar = new System.Windows.Forms.Button();
            this.button_cancelar = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.TextBox_Duracion = new AMS.TextBox.IntegerTextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.TextBox_Termino_Max = new System.Windows.Forms.TextBox();
            this.TextBox_Inicio_Min = new System.Windows.Forms.TextBox();
            this.dateTimePicker_fecha_termino = new System.Windows.Forms.DateTimePicker();
            this.dateTimePicker_fecha_ini = new System.Windows.Forms.DateTimePicker();
            this.label8 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.TextBox_Matricula = new AMS.TextBox.AlphanumericTextBox();
            this.TextBox_Estacion = new AMS.TextBox.AlphanumericTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_aceptar
            // 
            this.button_aceptar.Location = new System.Drawing.Point(12, 251);
            this.button_aceptar.Name = "button_aceptar";
            this.button_aceptar.Size = new System.Drawing.Size(75, 23);
            this.button_aceptar.TabIndex = 0;
            this.button_aceptar.Text = "Aceptar";
            this.toolTip1.SetToolTip(this.button_aceptar, "Genera slot de backup.");
            this.button_aceptar.UseVisualStyleBackColor = true;
            this.button_aceptar.Click += new System.EventHandler(this.Aceptar_Click);
            // 
            // button_cancelar
            // 
            this.button_cancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancelar.Location = new System.Drawing.Point(180, 251);
            this.button_cancelar.Name = "button_cancelar";
            this.button_cancelar.Size = new System.Drawing.Size(75, 23);
            this.button_cancelar.TabIndex = 1;
            this.button_cancelar.Text = "Cancelar";
            this.toolTip1.SetToolTip(this.button_cancelar, "Cancela la generación del slot de backup.");
            this.button_cancelar.UseVisualStyleBackColor = true;
            this.button_cancelar.Click += new System.EventHandler(this.Cancelar_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.TextBox_Duracion);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.TextBox_Termino_Max);
            this.groupBox1.Controls.Add(this.TextBox_Inicio_Min);
            this.groupBox1.Controls.Add(this.dateTimePicker_fecha_termino);
            this.groupBox1.Controls.Add(this.dateTimePicker_fecha_ini);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.TextBox_Matricula);
            this.groupBox1.Controls.Add(this.TextBox_Estacion);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(243, 223);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Información slot de backup";
            // 
            // TextBox_Duracion
            // 
            this.TextBox_Duracion.AllowNegative = false;
            this.TextBox_Duracion.DigitsInGroup = 0;
            this.TextBox_Duracion.Flags = 65536;
            this.TextBox_Duracion.Location = new System.Drawing.Point(101, 191);
            this.TextBox_Duracion.MaxDecimalPlaces = 0;
            this.TextBox_Duracion.MaxWholeDigits = 9;
            this.TextBox_Duracion.Name = "TextBox_Duracion";
            this.TextBox_Duracion.Prefix = "";
            this.TextBox_Duracion.RangeMax = 2147483647;
            this.TextBox_Duracion.RangeMin = -2147483648;
            this.TextBox_Duracion.Size = new System.Drawing.Size(131, 20);
            this.TextBox_Duracion.TabIndex = 28;
            this.toolTip1.SetToolTip(this.TextBox_Duracion, "Duración en minutos del slot de backup");
            this.TextBox_Duracion.Leave += new System.EventHandler(this.CambiarDuracion);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 197);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(50, 13);
            this.label6.TabIndex = 27;
            this.label6.Text = "Duración";
            // 
            // TextBox_Termino_Max
            // 
            this.TextBox_Termino_Max.Location = new System.Drawing.Point(101, 101);
            this.TextBox_Termino_Max.Name = "TextBox_Termino_Max";
            this.TextBox_Termino_Max.ReadOnly = true;
            this.TextBox_Termino_Max.Size = new System.Drawing.Size(131, 20);
            this.TextBox_Termino_Max.TabIndex = 26;
            this.TextBox_Termino_Max.TabStop = false;
            // 
            // TextBox_Inicio_Min
            // 
            this.TextBox_Inicio_Min.Location = new System.Drawing.Point(101, 75);
            this.TextBox_Inicio_Min.Name = "TextBox_Inicio_Min";
            this.TextBox_Inicio_Min.ReadOnly = true;
            this.TextBox_Inicio_Min.Size = new System.Drawing.Size(131, 20);
            this.TextBox_Inicio_Min.TabIndex = 25;
            this.TextBox_Inicio_Min.TabStop = false;
            // 
            // dateTimePicker_fecha_termino
            // 
            this.dateTimePicker_fecha_termino.CustomFormat = "yyyy-MM-dd HH:mm";
            this.dateTimePicker_fecha_termino.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker_fecha_termino.Location = new System.Drawing.Point(101, 165);
            this.dateTimePicker_fecha_termino.Name = "dateTimePicker_fecha_termino";
            this.dateTimePicker_fecha_termino.Size = new System.Drawing.Size(131, 20);
            this.dateTimePicker_fecha_termino.TabIndex = 24;
            this.toolTip1.SetToolTip(this.dateTimePicker_fecha_termino, "Fecha/Hora de término de slot de backup");
            this.dateTimePicker_fecha_termino.ValueChanged += new System.EventHandler(this.ValidarFechas);
            this.dateTimePicker_fecha_termino.CloseUp += new System.EventHandler(this.ValidarFechas);
            // 
            // dateTimePicker_fecha_ini
            // 
            this.dateTimePicker_fecha_ini.CustomFormat = "yyyy-MM-dd HH:mm";
            this.dateTimePicker_fecha_ini.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker_fecha_ini.Location = new System.Drawing.Point(101, 138);
            this.dateTimePicker_fecha_ini.Name = "dateTimePicker_fecha_ini";
            this.dateTimePicker_fecha_ini.Size = new System.Drawing.Size(131, 20);
            this.dateTimePicker_fecha_ini.TabIndex = 20;
            this.toolTip1.SetToolTip(this.dateTimePicker_fecha_ini, "Fecha/Hora de inicio de slot de backup");
            this.dateTimePicker_fecha_ini.ValueChanged += new System.EventHandler(this.ValidarFechas);
            this.dateTimePicker_fecha_ini.CloseUp += new System.EventHandler(this.ValidarFechas);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 171);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(45, 13);
            this.label8.TabIndex = 14;
            this.label8.Text = "Término";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 104);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(83, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Término máximo";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 144);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Inicio";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 78);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Inicio mínimo";
            // 
            // TextBox_Matricula
            // 
            this.TextBox_Matricula.Flags = 0;
            this.TextBox_Matricula.InvalidChars = new char[] {
        '%',
        '\'',
        '*',
        '\"',
        '+',
        '?',
        '>',
        '<',
        ':',
        '\\'};
            this.TextBox_Matricula.Location = new System.Drawing.Point(101, 49);
            this.TextBox_Matricula.Name = "TextBox_Matricula";
            this.TextBox_Matricula.ReadOnly = true;
            this.TextBox_Matricula.Size = new System.Drawing.Size(131, 20);
            this.TextBox_Matricula.TabIndex = 3;
            // 
            // TextBox_Estacion
            // 
            this.TextBox_Estacion.Flags = 0;
            this.TextBox_Estacion.InvalidChars = new char[] {
        '%',
        '\'',
        '*',
        '\"',
        '+',
        '?',
        '>',
        '<',
        ':',
        '\\'};
            this.TextBox_Estacion.Location = new System.Drawing.Point(101, 23);
            this.TextBox_Estacion.Name = "TextBox_Estacion";
            this.TextBox_Estacion.ReadOnly = true;
            this.TextBox_Estacion.Size = new System.Drawing.Size(131, 20);
            this.TextBox_Estacion.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Matrícula";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Estación";
            // 
            // AgregarBU
            // 
            this.AcceptButton = this.button_aceptar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_cancelar;
            this.ClientSize = new System.Drawing.Size(266, 286);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button_cancelar);
            this.Controls.Add(this.button_aceptar);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AgregarBU";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Crear Slot de Backup";
            this.TopMost = true;
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_aceptar;
        private System.Windows.Forms.Button button_cancelar;
        private System.Windows.Forms.GroupBox groupBox1;
        private AMS.TextBox.AlphanumericTextBox TextBox_Matricula;
        private AMS.TextBox.AlphanumericTextBox TextBox_Estacion;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DateTimePicker dateTimePicker_fecha_ini;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.DateTimePicker dateTimePicker_fecha_termino;
        private System.Windows.Forms.TextBox TextBox_Termino_Max;
        private System.Windows.Forms.TextBox TextBox_Inicio_Min;
        private System.Windows.Forms.Label label6;
        private AMS.TextBox.IntegerTextBox TextBox_Duracion;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}