using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using SimuLAN.Clases;
using SimuLAN.Clases.Disrupciones;
using InterfazSimuLAN.AccesoData;
using SimuLAN.Utils;
using BrightIdeasSoftware;

namespace InterfazSimuLAN
{
    /// <summary>
    /// Form del actualizador de curvas
    /// </summary>
    public partial class ActualizadorCurvas : Form
    {
        #region ATRIBUTES

        /// <summary>
        /// Conexión a SQL Server
        /// </summary>
        private SqlConnection _connection;
        
        /// <summary>
        /// Referencia a Form principal de SimuLAN
        /// </summary>
        internal InterfazSimuLAN _main;

        /// <summary>
        /// DataSet temporal para las curvas actualizadas
        /// </summary>
        private DataSet _curvas_actualizadas;

        /// <summary>
        /// DataSet temporal para los factores actualizados
        /// </summary>
        private DataSet _factores_actualizados;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="main">Refencia a la interfaz principal de SimuLAN</param>
        public ActualizadorCurvas(InterfazSimuLAN main)
        {
            InitializeComponent();
            this._main = main;
            this.textBox1.Text = main._config.GetParametro("connectionString").ToString();
            this._curvas_actualizadas = new DataSet();
            this._factores_actualizados = new DataSet();
            InicializarConexion(this.textBox1.Text);
            SetDelegatesOLVCurvas();
            SetDelegatesOLVFactores();
            CargarOLVCurvas();
            CargarOLVFactores();
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Actualiza desde BD las curvas de disrupciones
        /// Acción desencadenada al hacer click en botton Actualizar en tabpage curvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActualizarCurvas(object sender, EventArgs e)
        {
            if (_connection.State == ConnectionState.Open)
            {
                this.Cursor = Cursors.WaitCursor;
                DateTime ini = DateTime.Now;
                if (objectListView1.CheckedObjects != null)
                {
                    foreach (object o in objectListView1.CheckedObjects)
                    {
                        ObjetoActualizacionCurvas a = (ObjetoActualizacionCurvas)o;
                        AgregarTableCurvas(a);
                    }
                    CargarDataTableEnVisorCurvas(null, new EventArgs());
                }
                DateTime termino = DateTime.Now;
                int duracion = Convert.ToInt16((termino - ini).TotalSeconds);
                EnviarMensaje("Actualización terminada en " + duracion + " segundos.");
                this.Cursor = Cursors.Default;
            }
            else
            {
                EnviarMensaje("Error en la conexión a la base de datos.");
            }
        }

        /// <summary>
        /// actualiza desde BD los factores de desvación de probabilidad de escenarios
        /// Acción desencadenada al hacer click en botton Actualizar en tabpage factores escenarios
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActualizarFactores(object sender, EventArgs e)
        {
            if (_connection.State == ConnectionState.Open)
            {
                this.Cursor = Cursors.WaitCursor;
                DateTime ini = DateTime.Now;
                if (objectListView2.CheckedObjects != null)
                {
                    foreach (object o in objectListView2.CheckedObjects)
                    {
                        ObjetoActualizacionFactores a = (ObjetoActualizacionFactores)o;
                        AgregarTableFactores(a);
                    }
                    CargarDataTableEnVisorFactores(null, new EventArgs());
                }
                DateTime termino = DateTime.Now;
                int duracion = Convert.ToInt16((termino - ini).TotalSeconds);
                EnviarMensaje("Actualización terminada en " + duracion + " segundos.");
                this.Cursor = Cursors.Default;
            }
            else
            {
                EnviarMensaje("Error en la conexión a la base de datos.");
            }
        }

        /// <summary>
        /// Agrega un DaTaTable al DataSet de curvas con información desde BD
        /// </summary>
        /// <param name="a"></param>
        private void AgregarTableCurvas(ObjetoActualizacionCurvas a)
        {
            if (_curvas_actualizadas.Tables.Contains(a.Nombre))
            {
                _curvas_actualizadas.Tables.Remove(a.Nombre);
            }
            _curvas_actualizadas.Tables.Add(DataTableCurvasFromDataBase(a));
        }

        /// <summary>
        /// Agrega un DaTaTable al DataSet de factores con información desde BD
        /// </summary>
        /// <param name="a"></param>
        private void AgregarTableFactores(ObjetoActualizacionFactores a)
        {
            if (_factores_actualizados.Tables.Contains(a.Nombre.ToString()))
            {
                _factores_actualizados.Tables.Remove(a.Nombre.ToString());
            }
            _factores_actualizados.Tables.Add(DataTableFactoresFromDataBase(a));
        }

        /// <summary>
        /// Aplica las curvas de BD sobre el modelo principal de la simulación
        /// Acción desencadenada al hacer click en botton Aplicar en tabpage curvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AplicarCurvas(object sender, EventArgs e)
        {
            bool exitoso;
            int cantidadCargadas;
            SimuLAN_DAO.DataSetToModeloDisrupciones(_main._modeloDisrupcionesBase, _curvas_actualizadas, out exitoso, false, out cantidadCargadas);
            EnviarMensaje(((exitoso && cantidadCargadas > 0) ? ("Proceso " + (exitoso ? "exitoso" : "fallido") + ". " + (exitoso ? ("Se carg" + (cantidadCargadas == 1 ? "ó" : "aron") + " " + cantidadCargadas + " curva" + (cantidadCargadas == 1 ? "" : "s") + " de disrupci" + (cantidadCargadas == 1 ? "ón." : "ones.")) : "")) : "No se cargaron curvas."));
        }

        /// <summary>
        /// Aplica los factores de BD sobre el modelo principal de la simulación
        /// Acción desencadenada al hacer click en botton Aplicar en tabpage factores escenarios
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AplicarFactores(object sender, EventArgs e)
        {
            List<string> actualizados = new List<string>();
            if (_factores_actualizados.Tables.Contains(TipoDisrupcion.MANTENIMIENTO.ToString()))
            {
                actualizados.Add(TipoDisrupcion.MANTENIMIENTO.ToString());
                if (_main._factoresEscenarios.ContainsKey(TipoDisrupcion.MANTENIMIENTO))
                {
                    _main._factoresEscenarios[TipoDisrupcion.MANTENIMIENTO] = SimuLAN_DAO.CargarFactoresDesviacion(_factores_actualizados.Tables[TipoDisrupcion.MANTENIMIENTO.ToString()], true);
                }
                else
                {
                    _main._factoresEscenarios.Add(TipoDisrupcion.MANTENIMIENTO, SimuLAN_DAO.CargarFactoresDesviacion(_factores_actualizados.Tables[TipoDisrupcion.MANTENIMIENTO.ToString()], true));
                }
            }
            if (_factores_actualizados.Tables.Contains(TipoDisrupcion.METEREOLOGIA.ToString()))
            {
                actualizados.Add(TipoDisrupcion.METEREOLOGIA.ToString());
                if (_main._factoresEscenarios.ContainsKey(TipoDisrupcion.METEREOLOGIA))
                {
                    _main._factoresEscenarios[TipoDisrupcion.METEREOLOGIA] = SimuLAN_DAO.CargarFactoresDesviacion(_factores_actualizados.Tables[TipoDisrupcion.METEREOLOGIA.ToString()], true);
                }
                else
                {
                    _main._factoresEscenarios.Add(TipoDisrupcion.METEREOLOGIA, SimuLAN_DAO.CargarFactoresDesviacion(_factores_actualizados.Tables[TipoDisrupcion.METEREOLOGIA.ToString()], true));
                }
            }
            if (actualizados.Count > 0)
            {
                EnviarMensaje("Se actualizaron los factores de desvación multiescenario de " + ((actualizados.Count == 2) ? (actualizados[0] + " y " + actualizados[1]) : actualizados[0]) + ".");
            }
            else
            {
                EnviarMensaje("No se han actualizado factores.");
            }
        }

        /// <summary>
        /// Cambiar el connectionString del appconfig
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CambiarConnectionString(object sender, EventArgs e)
        {
            _main._config.ReplaceSetting("connectionString", textBox1.Text);
        }

        /// <summary>
        /// Carga el ObjectListView de curvas
        /// </summary>
        private void CargarOLVCurvas()
        {
            foreach (TipoDisrupcion tipo in Enum.GetValues(typeof(TipoDisrupcion)))
            {
                if (_main._modeloDisrupcionesBase != null && _main._modeloDisrupcionesBase.ColeccionDisrupciones != null && _main._modeloDisrupcionesBase.ColeccionDisrupciones.Count > 0 && _main._modeloDisrupcionesBase.ColeccionDisrupciones.ContainsKey(tipo.ToString()))
                {
                    InfoDisrupcion info = _main._modeloDisrupcionesBase.ColeccionDisrupciones[tipo.ToString()];
                    ObjetoActualizacionCurvas o = new ObjetoActualizacionCurvas(tipo.ToString(), info.Dimension, info.Headers, DateTime.Now.Year - 5, GetPeriodos(tipo), GetFuncion(tipo));
                    objectListView1.AddObject(o);
                }
                if (objectListView1.Objects != null)
                {
                    bool primero = true;
                    foreach (object o in objectListView1.Objects)
                    {
                        objectListView1.CheckObject(o);
                        if (primero)
                        {
                            primero = false;
                            objectListView1.SelectObject(o);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Carga el ObjectListView de factores
        /// </summary>
        private void CargarOLVFactores()
        {
            List<string> headers = new List<string>();
            headers.Add("Agregación");
            headers.Add("Factor Bueno");
            headers.Add("Factor Malo");
            objectListView2.AddObject(new ObjetoActualizacionFactores(TipoDisrupcion.MANTENIMIENTO, headers, DateTime.Now.Year - 5, "Fnc_Factores_Mantto"));
            objectListView2.AddObject(new ObjetoActualizacionFactores(TipoDisrupcion.METEREOLOGIA, headers, DateTime.Now.Year - 5, "Fnc_Factores_WXS"));
        }

        /// <summary>
        /// Carga una curva de BD seleccionada en el DataListView de curvas
        /// Acción desencadenada al hacer click una de las curvas del OLV curvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CargarDataTableEnVisorCurvas(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            this.dataListView_curvas.Visible = true;
            if (objectListView1.GetSelectedObject() != null)
            {
                ObjetoActualizacionCurvas a = (ObjetoActualizacionCurvas)objectListView1.GetSelectedObject();
                ConfigurarDataObjectList(dataListView_curvas, a.Dimension, a.Headers);
                if (_curvas_actualizadas.Tables.Contains(a.Nombre))
                {
                    dataListView_curvas.DataSource = _curvas_actualizadas.Tables[a.Nombre];
                }
                dataListView_curvas.Refresh();
            }
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Carga un factor de BD seleccionado en el DataListView de factores
        /// Acción desencadenada al hacer click una de los factores del OLV factores
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CargarDataTableEnVisorFactores(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            this.dataListView1.Visible = true;
            if (objectListView2.GetSelectedObject() != null)
            {
                ObjetoActualizacionFactores a = (ObjetoActualizacionFactores)objectListView2.GetSelectedObject();
                ConfigurarDataObjectList(dataListView1, 1, a.Headers);
                if (_factores_actualizados.Tables.Contains(a.Nombre.ToString()))
                {
                    dataListView1.DataSource = _factores_actualizados.Tables[a.Nombre.ToString()];
                }
                dataListView1.Refresh();
            }
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Cierra la ventana.
        /// Acción ejecutada al hacer click sobre el botón "cerrar"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cerrar(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Inicia o reinicia una conección a la base de datos.
        /// </summary>
        /// <param name="connectionString">Cadena de conexión</param>
        private void Conectar(string connectionString)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                _connection = new SqlConnection(connectionString);
                _connection.Open();
                pictureBox1.Image = imageList1.Images[0];
            }
            catch
            {
                pictureBox1.Image = imageList1.Images[1];
            }
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Configura un dataListView
        /// </summary>
        /// <param name="dataListView">DataListView objetivo</param>
        /// <param name="dimension">Número de columnas que no se editan</param>
        /// <param name="headers">Título de las columnas</param>
        private void ConfigurarDataObjectList(DataListView dataListView,  int dimension, List<string> headers)
        {
            dataListView.ClearObjects();
            dataListView.Groups.Clear();
            dataListView.DataSource = null;
            dataListView.DataMember = null;

            for (int i = 0; i < dataListView.AllColumns.Count; i++)
            {
                if (i < headers.Count)
                {
                    dataListView.AllColumns[i].Text = headers[i];
                    dataListView.AllColumns[i].AspectName = headers[i];

                }
                else
                {
                    dataListView.AllColumns[i].Text = "(Vacía)";
                    dataListView.AllColumns[i].AspectName = "Column" + (i + 1);
                }
                dataListView.AllColumns[i].IsEditable = !(i < dimension);
                dataListView.AllColumns[i].IsVisible = (i < headers.Count);
            }
            dataListView.RebuildColumns();
            dataListView.Refresh();
        }

        /// <summary>
        /// Ejecuta una consulta para obtener las curvas y carga un DataTable con los resultados
        /// </summary>
        /// <param name="a">Objeto con la consulta</param>
        /// <returns></returns>
        private DataTable DataTableCurvasFromDataBase(ObjetoActualizacionCurvas a)
        {
            DataTable data = new DataTable(a.Nombre);
            foreach (string header in a.Headers)
                data.Columns.Add(header);
            Invoke(new Action<string>(EnviarMensaje), "Actualizando curvas " + a.Nombre);
            SqlCommand command = new SqlCommand(a.Query, _connection);
            command.CommandTimeout = 300;
            SqlDataReader r = command.ExecuteReader();
            while (r.Read())
            {
                object[] o = new object[r.FieldCount];
                for (int i = 0; i < r.FieldCount; i++)
                {
                    o[i] = r[i];
                }
                data.Rows.Add(o);
            }
            r.Close();
            return data;
        }

        /// <summary>
        /// Ejecuta una consulta para obtener las curvas y carga un DataTable con los resultados
        /// </summary>
        /// <param name="a">Objeto con la consulta</param>
        /// <returns></returns>
        private DataTable DataTableFactoresFromDataBase(ObjetoActualizacionFactores a)
        {
            DataTable data = new DataTable(a.Nombre.ToString());
            foreach (string header in a.Headers)
                data.Columns.Add(header);
            Invoke(new Action<string>(EnviarMensaje), "Actualizando factores " + a.Nombre);
            SqlCommand command = new SqlCommand(a.Query, _connection);
            command.CommandTimeout = 300;
            SqlDataReader r = command.ExecuteReader();
            while (r.Read())
            {
                object[] o = new object[r.FieldCount];
                for (int i = 0; i < r.FieldCount; i++)
                {
                    o[i] = r[i];
                }
                data.Rows.Add(o);
            }
            r.Close();
            return data;
        }

        /// <summary>
        /// Carga un mensaje para ser desplegado en el label de información
        /// </summary>
        /// <param name="p"></param>
        private void EnviarMensaje(string p)
        {
            label1.Text = p;
            label1.Refresh();
        }

        /// <summary>
        /// Retorna la función de BD correspondiente a cada tipo de disrupción
        /// </summary>
        /// <param name="tipo">Tipo de disrupción</param>
        /// <returns></returns>
        private string GetFuncion(TipoDisrupcion tipo)
        {
            if (tipo == TipoDisrupcion.ADELANTO)
                return "Fnc_Adelanto";
            else if (tipo == TipoDisrupcion.ATC)
                return "Fnc_Calibracion_Mes_Origen_Periodo";
            else if (tipo == TipoDisrupcion.HBT)
                return "Fnc_Calibracion_HBT";
            else if (tipo == TipoDisrupcion.MANTENIMIENTO)
                return "Fnc_Calibracion_Mantto";
            else if (tipo == TipoDisrupcion.METEREOLOGIA)
                return "Fnc_Calibracion_Mes_Origen_Periodo";
            else if (tipo == TipoDisrupcion.OTROS)
                return "Func_Calibracion_Origen_Periodo";
            else if (tipo == TipoDisrupcion.RECURSOS_DEL_APTO)
                return "Func_Calibracion_Origen_Periodo";
            else if (tipo == TipoDisrupcion.TA_BAJO_ALA)
                return "Func_Calibracion_Origen_Periodo";
            else if (tipo == TipoDisrupcion.TA_SOBRE_ALA)
                return "Func_Calibracion_Origen_Periodo";
            else if (tipo == TipoDisrupcion.TRIPULACIONES)
                return "Func_Calibracion_Origen_Periodo";
            else return "";
        }

        /// <summary>
        /// Retorna la cantidad de periodos del día en que se divide la curva de un tipo de disrupción
        /// </summary>
        /// <param name="tipo">Tipo de disrupción</param>
        /// <returns></returns>
        private int GetPeriodos(TipoDisrupcion tipo)
        {
            if (tipo == TipoDisrupcion.ATC)
                return 8;
            else if (tipo == TipoDisrupcion.METEREOLOGIA)
                return 4;
            else if (tipo == TipoDisrupcion.OTROS)
                return 12;
            else if (tipo == TipoDisrupcion.RECURSOS_DEL_APTO)
                return 6;
            else if (tipo == TipoDisrupcion.TA_BAJO_ALA)
                return 12;
            else if (tipo == TipoDisrupcion.TA_SOBRE_ALA)
                return 12;
            else if (tipo == TipoDisrupcion.TRIPULACIONES)
                return 6;
            else return 1;            
        }

        /// <summary>
        /// Método para inicializar una conexión
        /// </summary>
        /// <param name="connectionString">Cadena de conexión</param>
        private void InicializarConexion(string connectionString)
        {
            try
            {
                Invoke(new Action<string>(Conectar), connectionString);
            }
            catch
            {
                Conectar(connectionString);
            }
        }

        /// <summary>
        /// Quita las tablas chequeadas
        /// Acción ejecutada al hacer click sobre el botón Limpiar en tabpage Curvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LimpiarChecksCurvas(object sender, EventArgs e)
        {
            if (objectListView1.CheckedObjects != null)
            {
                foreach (object o in objectListView1.CheckedObjects)
                {
                    ObjetoActualizacionCurvas a = (ObjetoActualizacionCurvas)o;
                    if (_curvas_actualizadas.Tables.Contains(a.Nombre))
                    {
                        _curvas_actualizadas.Tables.Remove(a.Nombre);
                    }
                }
            }
            CargarDataTableEnVisorCurvas(null, new EventArgs());
        }

        /// <summary>
        /// Quita las tablas chequeadas
        /// Acción ejecutada al hacer click sobre el botón Limpiar en tabpage factores escenarios
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LimpiarChecksFactores(object sender, EventArgs e)
        {
            if (objectListView2.CheckedObjects != null)
            {
                foreach (object o in objectListView2.CheckedObjects)
                {
                    ObjetoActualizacionFactores a = (ObjetoActualizacionFactores)o;
                    if (_factores_actualizados.Tables.Contains(a.Nombre.ToString()))
                    {
                        _factores_actualizados.Tables.Remove(a.Nombre.ToString());
                    }
                }
            }
            CargarDataTableEnVisorFactores(null, new EventArgs());
        }

        /// <summary>
        /// Cierra la conexión y cambia flag de ventana abierta de Actualizador en main.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (_connection != null) _connection.Close();
            _main._actualizadorAbierto = false;
            CambiarConnectionString(null, new EventArgs());
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
        /// Carga ascpectGetters y ascpectPutters en OLV Curvas
        /// </summary>
        private void SetDelegatesOLVCurvas()
        {
            objectListView1.AllColumns[0].AspectGetter = delegate(object o)
            {
                ObjetoActualizacionCurvas aux = (ObjetoActualizacionCurvas)o;
                return aux.Nombre;
            };
            objectListView1.AllColumns[1].AspectGetter = delegate(object o)
            {
                ObjetoActualizacionCurvas aux = (ObjetoActualizacionCurvas)o;
                return aux.AgnoIni;
            };
            objectListView1.AllColumns[2].AspectGetter = delegate(object o)
            {
                ObjetoActualizacionCurvas aux = (ObjetoActualizacionCurvas)o;
                return aux.Periodos;
            };
            objectListView1.AllColumns[1].AspectPutter = delegate(object goal, object value)
            {
                ObjetoActualizacionCurvas aux = (ObjetoActualizacionCurvas)goal;
                int valor = Utilidades.EsEnteroPositivo(value.ToString()) ? ((int)value) : aux.AgnoIni;
                aux.AgnoIni = (valor >= 2000 && valor <= DateTime.Now.Year) ? valor : aux.AgnoIni;
            };
            objectListView1.AllColumns[2].AspectPutter = delegate(object goal, object value)
            {
                ObjetoActualizacionCurvas aux = (ObjetoActualizacionCurvas)goal;
                int valor = Utilidades.EsEnteroPositivo(value.ToString()) ? ((int)value) : aux.Periodos;
                aux.Periodos = ((valor == 1) || (valor == 2) || (valor == 3) || (valor == 4) || (valor == 6) || (valor == 8) || (valor == 12) || (valor == 24)) ? valor : aux.Periodos;
            };
        }

        /// <summary>
        /// Carga ascpectGetters y ascpectPutters en OLV factores
        /// </summary>
        private void SetDelegatesOLVFactores()
        {
            objectListView2.AllColumns[0].AspectGetter = delegate(object o)
            {
                ObjetoActualizacionFactores aux = (ObjetoActualizacionFactores)o;
                return aux.Nombre;
            };
            objectListView2.AllColumns[1].AspectGetter = delegate(object o)
            {
                ObjetoActualizacionFactores aux = (ObjetoActualizacionFactores)o;
                return aux.AgnoIni;
            };
            objectListView2.AllColumns[1].AspectPutter = delegate(object goal, object value)
            {
                ObjetoActualizacionFactores aux = (ObjetoActualizacionFactores)goal;
                int valor = Utilidades.EsEnteroPositivo(value.ToString()) ? ((int)value) : aux.AgnoIni;
                aux.AgnoIni = (valor >= 2000 && valor <= DateTime.Now.Year) ? valor : aux.AgnoIni;
            };
        }

        /// <summary>
        /// Determina si el string de conexión es válido        
        /// Acción ejecutada al hacer click sobre botón "Validar Conexión"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValidarConexion(object sender, EventArgs e)
        {
            InicializarConexion(this.textBox1.Text);
            CambiarConnectionString(null, new EventArgs());
        }

        #endregion        
    }

    /// <summary>
    /// Objeto para ObjectListView de curvas
    /// </summary>
    public class ObjetoActualizacionCurvas
    {
        #region ATRIBUTES

        /// <summary>
        /// Año de inicio de la consulta
        /// </summary>
        private int _agnoIni;
        
        /// <summary>
        /// Cantidad de columnas no editables
        /// </summary>
        private int _dimension;
        
        /// <summary>
        /// Nombre de la función SQL de consulta
        /// </summary>
        private string _funcion;

        /// <summary>
        /// Encabezados de las columnas de la tabla de consulta
        /// </summary>
        private List<string> _headers;

        /// <summary>
        /// Cantidad de periodos en que se divide un día
        /// </summary>
        private int _periodos;

        /// <summary>
        /// Nombre de  la disrupción
        /// </summary>
        private string _nombre;

        #endregion

        #region PROPERTIES
        
        /// <summary>
        /// Año de inicio de la consulta
        /// </summary>
        public int AgnoIni
        {
            get { return _agnoIni; }
            set { _agnoIni = value; }
        }
        
        /// <summary>
        /// Cantidad de columnas no editables
        /// </summary>
        public int Dimension
        {
            get { return _dimension; }
        }
        
        /// <summary>
        /// Nombre de la función SQL de consulta
        /// </summary>
        public string Funcion
        {
            get { return _funcion; }
        }
        
        /// <summary>
        /// Encabezados de las columnas de la tabla de consulta
        /// </summary>
        public List<string> Headers
        {
            get { return _headers; }
        }
        
        /// <summary>
        /// Nombre de  la disrupción
        /// </summary>
        public string Nombre
        {
            get { return _nombre; }
        }

        /// <summary>
        /// Retorna nombre de tabla de disrupción
        /// </summary>
        public string NombreTabla
        {
            get
            {

                if (_nombre == "TA_BAJO_ALA")
                {
                    return "T/A BAJO ALA";
                }
                else if (_nombre == "TA_SOBRE_ALA")
                {
                    return "T/A SOBRE ALA";
                }
                else if (_nombre == "RECURSOS_DEL_APTO")
                {
                    return "RECURSOS DEL APTO";
                }
                else return _nombre;
            }
        }
        
        /// <summary>
        /// Cantidad de periodos en que se divide un día
        /// </summary>
        public int Periodos
        {
            get { return _periodos; }
            set { _periodos = value; }
        }

        /// <summary>
        /// String de consulta
        /// </summary>
        public string Query
        {
            get            
            {
                string query = "SELECT * FROM " + _funcion;
                string orderBy = " order by ";
                if (TipoDisrupcion.ADELANTO.ToString() == _nombre || TipoDisrupcion.MANTENIMIENTO.ToString() == _nombre || TipoDisrupcion.HBT.ToString() == _nombre)
                {
                    query += "('" + _agnoIni + "0101')";                    
                }
                else
                {
                    int div = 24 / _periodos;
                    query += "(" + div + ",'" + NombreTabla + "','" + _agnoIni + "0101')";                   
                }
                for (int i = 1; i < _dimension; i++)
                {
                    orderBy += "col" + i + ", ";
                }
                orderBy += "col" + _dimension;

                query += orderBy + ";";
                return query;
            }
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor del objecto
        /// </summary>
        /// <param name="nombre">Nombre de la disrucpión asociada</param>
        /// <param name="dimension">Cantidad de columnas no editables</param>
        /// <param name="headers">Encabezados</param>
        /// <param name="agnoIni">Año inicial de consulta</param>
        /// <param name="periodos">Número de periodos en que se divide el día</param>
        /// <param name="funcion">Función de BD</param>        
        public ObjetoActualizacionCurvas(string nombre, int dimension, List<string> headers, int agnoIni, int periodos, string funcion)
        {
            this._dimension = dimension;
            this._headers = headers;
            this._agnoIni = agnoIni;
            this._funcion = funcion;
            this._nombre = nombre;
            this._periodos = periodos;
        }

        #endregion
    }

    /// <summary>
    /// Objeto para ObjectListView de factores
    /// </summary>
    public class ObjetoActualizacionFactores
    {
        #region ATRIBUTES
        /// <summary>
        /// Año de inicio de la consulta
        /// </summary>
        private int _agnoIni;
        
        /// <summary>
        /// Nombre de la función SQL de consulta
        /// </summary>
        private string _funcion;
        
        /// <summary>
        /// Encabezados de las columnas de la tabla de consulta
        /// </summary>
        private List<string> _headers;

        /// <summary>
        /// Tipo de disrupción asociado
        /// </summary>
        private TipoDisrupcion _nombre;

        #endregion

        #region PROPERTIES
        
        /// <summary>
        /// Año de inicio de la consulta
        /// </summary>
        public int AgnoIni
        {
            get { return _agnoIni; }
            set { _agnoIni = value; }
        }
        
        /// <summary>
        /// Nombre de la función SQL de consulta
        /// </summary>
        public string Funcion
        {
            get { return _funcion; }
        }
        
        /// <summary>
        /// Encabezados de las columnas de la tabla de consulta
        /// </summary>
        public List<string> Headers
        {
            get { return _headers; }
        }
        
        /// <summary>
        /// Tipo de disrupción asociado
        /// </summary>
        public TipoDisrupcion Nombre
        {
            get { return _nombre; }
        }

        /// <summary>
        /// String de consulta
        /// </summary>
        public string Query
        {
            get
            {
                string query = "SELECT * FROM " + _funcion + "('" + _agnoIni + "0101') order by col1;";
                return query;
            }
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Constructor del objecto
        /// </summary>
        /// <param name="nombre">Nombre de la disrucpión asociada</param>
        /// <param name="headers">Encabezados</param>
        /// <param name="agnoIni">Año inicial de consulta</param>
        /// <param name="funcion">Función de BD</param>
        public ObjetoActualizacionFactores(TipoDisrupcion nombre, List<string> headers, int agnoIni, string funcion)
        {
            this._headers = headers;
            this._agnoIni = agnoIni;
            this._funcion = funcion;
            this._nombre = nombre;
        }

        #endregion
    }

}
