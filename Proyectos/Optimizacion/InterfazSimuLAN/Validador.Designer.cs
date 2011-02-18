namespace InterfazSimuLAN
{
    partial class Validador
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Validador));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.objectListView_problemas = new BrightIdeasSoftware.ObjectListView();
            this.olvColumn1 = new BrightIdeasSoftware.OLVColumn();
            this.olvColumn2 = new BrightIdeasSoftware.OLVColumn();
            this.olvColumn3 = new BrightIdeasSoftware.OLVColumn();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.objectListView_edicion = new BrightIdeasSoftware.ObjectListView();
            this.olvColumn4 = new BrightIdeasSoftware.OLVColumn();
            this.olvColumn5 = new BrightIdeasSoftware.OLVColumn();
            this.olvColumn6 = new BrightIdeasSoftware.OLVColumn();
            this.olvColumn7 = new BrightIdeasSoftware.OLVColumn();
            this.olvColumn8 = new BrightIdeasSoftware.OLVColumn();
            this.olvColumn9 = new BrightIdeasSoftware.OLVColumn();
            this.olvColumn10 = new BrightIdeasSoftware.OLVColumn();
            this.olvColumn11 = new BrightIdeasSoftware.OLVColumn();
            this.olvColumn12 = new BrightIdeasSoftware.OLVColumn();
            this.olvColumn13 = new BrightIdeasSoftware.OLVColumn();
            this.buttonAceptar = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.objectListView_problemas)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.objectListView_edicion)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.objectListView_problemas);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(286, 498);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Problemas";
            this.toolTip1.SetToolTip(this.groupBox1, "Muestra los problemas detectados de falta de información agrupados por tabla de i" +
                    "nformación.");
            // 
            // objectListView_problemas
            // 
            this.objectListView_problemas.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.objectListView_problemas.AllColumns.Add(this.olvColumn1);
            this.objectListView_problemas.AllColumns.Add(this.olvColumn2);
            this.objectListView_problemas.AllColumns.Add(this.olvColumn3);
            this.objectListView_problemas.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn1,
            this.olvColumn2,
            this.olvColumn3});
            this.objectListView_problemas.Cursor = System.Windows.Forms.Cursors.Default;
            this.objectListView_problemas.FullRowSelect = true;
            this.objectListView_problemas.IsSearchOnSortColumn = false;
            this.objectListView_problemas.Location = new System.Drawing.Point(6, 19);
            this.objectListView_problemas.MultiSelect = false;
            this.objectListView_problemas.Name = "objectListView_problemas";
            this.objectListView_problemas.SelectAllOnControlA = false;
            this.objectListView_problemas.SelectColumnsMenuStaysOpen = false;
            this.objectListView_problemas.ShowImagesOnSubItems = true;
            this.objectListView_problemas.ShowSortIndicators = false;
            this.objectListView_problemas.Size = new System.Drawing.Size(274, 444);
            this.objectListView_problemas.SmallImageList = this.imageList1;
            this.objectListView_problemas.SortGroupItemsByPrimaryColumn = false;
            this.objectListView_problemas.TabIndex = 0;
            this.objectListView_problemas.UseCompatibleStateImageBehavior = false;
            this.objectListView_problemas.UseHotItem = true;
            this.objectListView_problemas.UseTranslucentHotItem = true;
            this.objectListView_problemas.UseTranslucentSelection = true;
            this.objectListView_problemas.View = System.Windows.Forms.View.Details;
            this.objectListView_problemas.SelectedIndexChanged += new System.EventHandler(this.EditarProblema);
            // 
            // olvColumn1
            // 
            this.olvColumn1.IsEditable = false;
            this.olvColumn1.Text = "Status";
            this.olvColumn1.Width = 45;
            // 
            // olvColumn2
            // 
            this.olvColumn2.HeaderTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.olvColumn2.IsEditable = false;
            this.olvColumn2.Text = "Tipo";
            this.olvColumn2.Width = 140;
            // 
            // olvColumn3
            // 
            this.olvColumn3.HeaderTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.olvColumn3.IsEditable = false;
            this.olvColumn3.Text = "Cantidad";
            this.olvColumn3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.olvColumn3.Width = 55;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "ok.ico");
            this.imageList1.Images.SetKeyName(1, "error.ico");
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.objectListView_edicion);
            this.groupBox2.Location = new System.Drawing.Point(299, 13);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(473, 498);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Edición";
            this.toolTip1.SetToolTip(this.groupBox2, "Muestra los valores de tabla sin información que deben ser editados.");
            this.groupBox2.Visible = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(392, 469);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Aplicar";
            this.toolTip1.SetToolTip(this.button1, "Actualiza los valores digitados en su tabla correspondiente.");
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.AplicarCambiosEdicion);
            // 
            // objectListView_edicion
            // 
            this.objectListView_edicion.AllColumns.Add(this.olvColumn4);
            this.objectListView_edicion.AllColumns.Add(this.olvColumn5);
            this.objectListView_edicion.AllColumns.Add(this.olvColumn6);
            this.objectListView_edicion.AllColumns.Add(this.olvColumn7);
            this.objectListView_edicion.AllColumns.Add(this.olvColumn8);
            this.objectListView_edicion.AllColumns.Add(this.olvColumn9);
            this.objectListView_edicion.AllColumns.Add(this.olvColumn10);
            this.objectListView_edicion.AllColumns.Add(this.olvColumn11);
            this.objectListView_edicion.AllColumns.Add(this.olvColumn12);
            this.objectListView_edicion.AllColumns.Add(this.olvColumn13);
            this.objectListView_edicion.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.SingleClick;
            this.objectListView_edicion.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn4,
            this.olvColumn5,
            this.olvColumn6,
            this.olvColumn7,
            this.olvColumn8,
            this.olvColumn9,
            this.olvColumn10,
            this.olvColumn11,
            this.olvColumn12,
            this.olvColumn13});
            this.objectListView_edicion.FullRowSelect = true;
            this.objectListView_edicion.GridLines = true;
            this.objectListView_edicion.Location = new System.Drawing.Point(6, 19);
            this.objectListView_edicion.Name = "objectListView_edicion";
            this.objectListView_edicion.SelectColumnsMenuStaysOpen = false;
            this.objectListView_edicion.SelectColumnsOnRightClick = false;
            this.objectListView_edicion.ShowGroups = false;
            this.objectListView_edicion.ShowSortIndicators = false;
            this.objectListView_edicion.Size = new System.Drawing.Size(461, 444);
            this.objectListView_edicion.SortGroupItemsByPrimaryColumn = false;
            this.objectListView_edicion.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.objectListView_edicion.TabIndex = 1;
            this.objectListView_edicion.TintSortColumn = true;
            this.objectListView_edicion.UseCompatibleStateImageBehavior = false;
            this.objectListView_edicion.UseExplorerTheme = true;
            this.objectListView_edicion.View = System.Windows.Forms.View.Details;
            this.objectListView_edicion.Visible = false;
            // 
            // olvColumn4
            // 
            this.olvColumn4.Width = 100;
            // 
            // buttonAceptar
            // 
            this.buttonAceptar.Location = new System.Drawing.Point(691, 527);
            this.buttonAceptar.Name = "buttonAceptar";
            this.buttonAceptar.Size = new System.Drawing.Size(75, 23);
            this.buttonAceptar.TabIndex = 2;
            this.buttonAceptar.Text = "Aceptar";
            this.toolTip1.SetToolTip(this.buttonAceptar, "Cierra el validador.");
            this.buttonAceptar.UseVisualStyleBackColor = true;
            this.buttonAceptar.Click += new System.EventHandler(this.Aceptar_Click);
            // 
            // Validador
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.buttonAceptar);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(800, 600);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "Validador";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Validador";
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.objectListView_problemas)).EndInit();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.objectListView_edicion)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button buttonAceptar;
        private BrightIdeasSoftware.ObjectListView objectListView_problemas;
        private BrightIdeasSoftware.ObjectListView objectListView_edicion;
        private System.Windows.Forms.ImageList imageList1;
        private BrightIdeasSoftware.OLVColumn olvColumn1;
        private BrightIdeasSoftware.OLVColumn olvColumn2;
        private BrightIdeasSoftware.OLVColumn olvColumn3;
        private BrightIdeasSoftware.OLVColumn olvColumn4;
        private BrightIdeasSoftware.OLVColumn olvColumn5;
        private BrightIdeasSoftware.OLVColumn olvColumn6;
        private BrightIdeasSoftware.OLVColumn olvColumn7;
        private BrightIdeasSoftware.OLVColumn olvColumn8;
        private BrightIdeasSoftware.OLVColumn olvColumn9;
        private BrightIdeasSoftware.OLVColumn olvColumn10;
        private BrightIdeasSoftware.OLVColumn olvColumn11;
        private BrightIdeasSoftware.OLVColumn olvColumn12;
        private BrightIdeasSoftware.OLVColumn olvColumn13;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}