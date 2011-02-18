using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SimuLAN.Clases.ControlInformacion;
using SimuLAN.Clases;
using BrightIdeasSoftware;
using SimuLAN.Utils;

namespace InterfazSimuLAN
{
    /// <summary>
    /// Form de validación del input de la simulación.
    /// </summary>
    public partial class Validador : Form
    {
        #region ENUMS

        /// <summary>
        /// Clasifiación de las tablas validadas 
        /// </summary>
        private enum Clase { Parametros, Curvas };

        #endregion

        #region ATRIBUTES

        /// <summary>
        /// Referencia a la interfaz princial de SimuLAN
        /// </summary>
        private InterfazSimuLAN _main;

        /// <summary>
        /// Objeto que encapsula la información faltante.
        /// </summary>
        private ControladorInformacion _controlador;

        /// <summary>
        /// Diccionario de la información editada para cada tipo de falta.
        /// </summary>
        private Dictionary<TipoFaltaInformacion, List<object[]>> _valores_editados;

        /// <summary>
        ///Referencia a la última selección de TipoFaltaInformacion.
        /// </summary>
        private KeyValuePair<TipoFaltaInformacion, List<Falta>> _ultima_seleccionada;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Construye ventana de validación
        /// </summary>
        /// <param name="main">Referencia a la ventana principal de SimuLAN</param>
        /// <param name="controlador">Objeto con la información faltante</param>
        public Validador(InterfazSimuLAN main, ControladorInformacion controlador)
        {
            InitializeComponent();
            this._main = main;
            this._controlador = controlador;
            this._valores_editados = new Dictionary<TipoFaltaInformacion, List<object[]>>();
            SetDelegatesOLVProblemas();
            CargarListaProblemas();
        }
        
        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Cierra la ventana.
        /// Se ejecuta cuando se presiona el botón "Aceptar".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Aceptar_Click(object sender, EventArgs e)
        {
            Close();
        }

        //SOBREESCRIBIR
        /// <summary>
        /// Aplica los cambios editados sobre los objetos del modelo de simulación.
        /// Se ejecuta cuando se presiona el botón "Aplicar". 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AplicarCambiosEdicion(object sender, EventArgs e)
        {
            #region PARAMETROS
            
            if (_ultima_seleccionada.Key == TipoFaltaInformacion.TurnAround)
            {
                foreach (object row in objectListView_edicion.Objects)
                {
                    object[] obj = (object[])row;
                    if (ValoresIngresadosValidos(_ultima_seleccionada.Key, obj))
                    {
                        _main._parametrosBase.TurnAroundMin.Data.Rows.Add(obj);
                    }
                }
                _main._parametrosBase.TurnAroundMin.Refresh();
            }
            else if (_ultima_seleccionada.Key == TipoFaltaInformacion.Tramo_Ruta)
            {
                foreach (object row in objectListView_edicion.Objects)
                {
                    object[] obj = (object[])row;
                    if (ValoresIngresadosValidos(_ultima_seleccionada.Key, obj))
                    {
                        _main._parametrosBase.MapVuelosRutas.Data.Rows.Add(obj);
                    }
                }
                _main._parametrosBase.MapVuelosRutas.Refresh();
                _main.CargarTablasEnItinerario();
            }
            else if (_ultima_seleccionada.Key == TipoFaltaInformacion.AcType_Flota)
            {
                foreach (object row in objectListView_edicion.Objects)
                {
                    object[] obj = (object[])row;
                    if (ValoresIngresadosValidos(_ultima_seleccionada.Key, obj))
                    {
                        _main._parametrosBase.MapFlotas.Data.Rows.Add(obj);
                    }
                }
                _main._parametrosBase.MapFlotas.Refresh();
                _main.CargarTablasEnItinerario();
            }
            else if (_ultima_seleccionada.Key == TipoFaltaInformacion.Flota_Grupo)
            {
                foreach (object row in objectListView_edicion.Objects)
                {
                    object[] obj = (object[])row;
                    if (ValoresIngresadosValidos(_ultima_seleccionada.Key, obj))
                    {
                        _main._parametrosBase.MapGruposFlotas.Data.Rows.Add(obj);
                    }
                }
                _main._parametrosBase.MapGruposFlotas.Refresh();
                _main.CargarTablasEnItinerario();
            }
            else if (_ultima_seleccionada.Key == TipoFaltaInformacion.SubFlota_Matricula)
            {
                foreach (object row in objectListView_edicion.Objects)
                {
                    object[] obj = (object[])row;
                    if (ValoresIngresadosValidos(_ultima_seleccionada.Key, obj))
                    {
                        _main._parametrosBase.MapSubFlotasMatriculas.Data.Rows.Add(obj);
                    }
                }
                _main._parametrosBase.MapSubFlotasMatriculas.Refresh();
                _main.CargarTablasEnItinerario();
            }
            else if (_ultima_seleccionada.Key == TipoFaltaInformacion.Multioperador_SubFlota || _ultima_seleccionada.Key == TipoFaltaInformacion.Multioperador_Operador)
            {
                bool todoValido = true;                
                foreach (object row in objectListView_edicion.Objects)
                {
                    object[] obj = (object[])row;                    
                    todoValido = todoValido && ValoresIngresadosValidos(_ultima_seleccionada.Key, obj);
                }
                if (todoValido)
                {
                    DataTable tablaMultioperador = _main._parametrosBase.MatrizMultioperadorToDataTable();
                    tablaMultioperador.Rows.Clear();
                    foreach (Falta f in _controlador.Faltas[TipoFaltaInformacion.Multioperador_Operador])
                    {
                        tablaMultioperador.Columns.Add(f.Keys[0]);
                    }
                    foreach (object row in objectListView_edicion.Objects)
                    {
                        object[] obj = (object[])row;
                        tablaMultioperador.Rows.Add(obj);
                    }
                    Dictionary<int, string> operadores = new Dictionary<int, string>();
                    int contador = 0;
                    foreach (DataColumn column in tablaMultioperador.Columns)
                    {
                        if (contador > 0)
                        {
                            operadores.Add(contador, column.ColumnName);
                        }
                        contador++;
                    }
                    _main._parametrosBase.CargarMatrizMultioperador(tablaMultioperador, operadores);
                }        
            }

            else if (_ultima_seleccionada.Key == TipoFaltaInformacion.Flota_Flota)
            {
                bool todoValido = true;
                foreach (object row in objectListView_edicion.Objects)
                {
                    object[] obj = (object[])row;
                    todoValido = todoValido && ValoresIngresadosValidos(_ultima_seleccionada.Key, obj);
                }
                if (todoValido)
                {
                    DataTable tabla = _main._parametrosBase.MatrizFlotaFlotaToDataTable();
                    tabla.Rows.Clear();
                    foreach (Falta f in _controlador.Faltas[TipoFaltaInformacion.Flota_Flota])
                    {
                        tabla.Columns.Add(f.Keys[0]);
                    }
                    foreach (object row in objectListView_edicion.Objects)
                    {
                        object[] obj = (object[])row;
                        tabla.Rows.Add(obj);
                    }
                    List<string> flotas = new List<string>();
                    int contador = 0;
                    foreach (DataColumn column in tabla.Columns)
                    {
                        if (contador > 0)
                        {
                            flotas.Add(column.ColumnName);
                        }
                        contador++;
                    }
                    _main._parametrosBase.CargarMatrizFlotaFlota(tabla, flotas);
                }
            }
            #endregion
            
            #region CURVAS
            else if (_ultima_seleccionada.Key == TipoFaltaInformacion.Curva_HBT)
            {
                foreach (object row in objectListView_edicion.Objects)
                {
                    object[] obj = (object[])row;
                    if (ValoresIngresadosValidos(_ultima_seleccionada.Key, obj))
                    {
                        _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.HBT.ToString()].Data.Rows.Add(obj);
                    }
                }
                _main._modeloDisrupcionesBase.Refresh();
            }
            else if (_ultima_seleccionada.Key == TipoFaltaInformacion.Curva_Adelanto)
            {
                foreach (object row in objectListView_edicion.Objects)
                {
                    object[] obj = (object[])row;
                    if (ValoresIngresadosValidos(_ultima_seleccionada.Key, obj))
                    {
                        _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.ADELANTO.ToString()].Data.Rows.Add(obj);
                    }
                }
                _main._modeloDisrupcionesBase.Refresh();
            }
            else if (_ultima_seleccionada.Key == TipoFaltaInformacion.Curva_Mantto)
            {
                foreach (object row in objectListView_edicion.Objects)
                {
                    object[] obj = (object[])row;
                    if (ValoresIngresadosValidos(_ultima_seleccionada.Key, obj))
                    {
                        _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.MANTENIMIENTO.ToString()].Data.Rows.Add(obj);
                    }
                }
                _main._modeloDisrupcionesBase.Refresh();
            }
            else if (_ultima_seleccionada.Key == TipoFaltaInformacion.Curva_ATC)
            {
                foreach (object row in objectListView_edicion.Objects)
                {
                    object[] obj = (object[])row;
                    if (ValoresIngresadosValidos(_ultima_seleccionada.Key, obj))
                    {
                        _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.ATC.ToString()].Data.Rows.Add(obj);
                    }
                }
                _main._modeloDisrupcionesBase.Refresh();
            }
            else if (_ultima_seleccionada.Key == TipoFaltaInformacion.Curva_OTROS)
            {
                foreach (object row in objectListView_edicion.Objects)
                {
                    object[] obj = (object[])row;
                    if (ValoresIngresadosValidos(_ultima_seleccionada.Key, obj))
                    {
                        _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.OTROS.ToString()].Data.Rows.Add(obj);
                    }
                }
                _main._modeloDisrupcionesBase.Refresh();
            }
            else if (_ultima_seleccionada.Key == TipoFaltaInformacion.Curva_Recursos_Apto)
            {
                foreach (object row in objectListView_edicion.Objects)
                {
                    object[] obj = (object[])row;
                    if (ValoresIngresadosValidos(_ultima_seleccionada.Key, obj))
                    {
                        _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.RECURSOS_DEL_APTO.ToString()].Data.Rows.Add(obj);
                    }
                }
                _main._modeloDisrupcionesBase.Refresh();
            }
            else if (_ultima_seleccionada.Key == TipoFaltaInformacion.Curva_TA_BA)
            {
                foreach (object row in objectListView_edicion.Objects)
                {
                    object[] obj = (object[])row;
                    if (ValoresIngresadosValidos(_ultima_seleccionada.Key, obj))
                    {
                        _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.TA_BAJO_ALA.ToString()].Data.Rows.Add(obj);
                    }
                }
                _main._modeloDisrupcionesBase.Refresh();
            }
            else if (_ultima_seleccionada.Key == TipoFaltaInformacion.Curva_TA_SA)
            {
                foreach (object row in objectListView_edicion.Objects)
                {
                    object[] obj = (object[])row;
                    if (ValoresIngresadosValidos(_ultima_seleccionada.Key, obj))
                    {
                        _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.TA_SOBRE_ALA.ToString()].Data.Rows.Add(obj);
                    }
                }
                _main._modeloDisrupcionesBase.Refresh();
            }
            else if (_ultima_seleccionada.Key == TipoFaltaInformacion.Curva_Trip)
            {
                foreach (object row in objectListView_edicion.Objects)
                {
                    object[] obj = (object[])row;
                    if (ValoresIngresadosValidos(_ultima_seleccionada.Key, obj))
                    {
                        _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.TRIPULACIONES.ToString()].Data.Rows.Add(obj);
                    }
                }
                _main._modeloDisrupcionesBase.Refresh();
            }
            else if (_ultima_seleccionada.Key == TipoFaltaInformacion.Curva_WXS)
            {
                foreach (object row in objectListView_edicion.Objects)
                {
                    object[] obj = (object[])row;
                    if (ValoresIngresadosValidos(_ultima_seleccionada.Key, obj))
                    {
                        _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.METEREOLOGIA.ToString()].Data.Rows.Add(obj);
                    }
                }
                _main._modeloDisrupcionesBase.Refresh();
            }
            #endregion
            
            _controlador.Validar();
            CargarListaProblemas();
            EditarProblema(_ultima_seleccionada.Key);            
        }
        
        /// <summary>
        /// Carla la lista de faltas en tabla resumen
        /// </summary>
        private void CargarListaProblemas()
        {
            this.objectListView_edicion.Visible = true;
            this.objectListView_problemas.ClearObjects();
            foreach (TipoFaltaInformacion tipo in _controlador.Faltas.Keys)
            {
                this.objectListView_problemas.AddObject(new KeyValuePair<TipoFaltaInformacion, List<Falta>>(tipo, _controlador.Faltas[tipo]));
            }
            this.objectListView_problemas.Sort(olvColumn2, SortOrder.Ascending);
        }

        //SOBREESCIBIR
        /// <summary>
        /// Carga la tabla de edición en función del tipo de falta de información requerido
        /// </summary>
        /// <param name="tipo">Tipo de falta de información</param>
        /// <param name="list">Lista de faltas</param>
        private void CargarOLVEdicion(TipoFaltaInformacion tipo, List<Falta> list)
        {
            objectListView_edicion.ClearObjects();
            InicializarTempValores(tipo);

            #region PARAMETROS
            
            if (tipo == TipoFaltaInformacion.TurnAround)
            {                
                foreach (Falta f in list)
                {
                    object[] parametros = new object[3];
                    parametros[0] = f.Keys[0];
                    parametros[1] = f.Keys[1];
                    parametros[2] = "";
                    _valores_editados[tipo].Add(parametros);
                    objectListView_edicion.AddObject(parametros);
                }
            }
            else if (tipo == TipoFaltaInformacion.Tramo_Ruta)
            {
                foreach (Falta f in list)
                {
                    object[] parametros = new object[3];
                    parametros[0] = f.Keys[0];
                    parametros[1] = "";
                    parametros[2] = "";
                    _valores_editados[tipo].Add(parametros);
                    objectListView_edicion.AddObject(parametros);
                }
            }
            else if (tipo == TipoFaltaInformacion.AcType_Flota || tipo == TipoFaltaInformacion.Flota_Grupo || tipo == TipoFaltaInformacion.SubFlota_Matricula)
            {
                foreach (Falta f in list)
                {
                    object[] parametros = new object[2];
                    parametros[0] = f.Keys[0];
                    parametros[1] = "";
                    _valores_editados[tipo].Add(parametros);
                    objectListView_edicion.AddObject(parametros);
                }
            }
            else if (tipo == TipoFaltaInformacion.Multioperador_SubFlota || tipo == TipoFaltaInformacion.Multioperador_Operador)
            {
                if (_controlador.Faltas[TipoFaltaInformacion.Multioperador_Operador].Count + _controlador.Faltas[TipoFaltaInformacion.Multioperador_SubFlota].Count > 0)
                {
                    //Carga primero existentes
                    DataTable tablaMatrizMultioperador = _main._parametrosBase.MatrizMultioperadorToDataTable();
                    int operadores_nuevos = _controlador.Faltas[TipoFaltaInformacion.Multioperador_Operador].Count;
                    int operadores_existentes = tablaMatrizMultioperador.Columns.Count;
                    foreach (DataRow data in tablaMatrizMultioperador.Rows)
                    {
                        object[] agregado = new object[operadores_nuevos + operadores_existentes];
                        object[] anteriores = data.ItemArray;
                        for (int i = 0; i < operadores_existentes; i++)
                        {
                            agregado[i] = anteriores[i];
                        }
                        for (int i = 0; i < operadores_nuevos; i++)
                        {
                            agregado[operadores_existentes + i] = "";
                        }
                        _valores_editados[tipo].Add(agregado);
                        objectListView_edicion.AddObject(agregado);
                    }
                    //Carga luego los nuevos
                    foreach (Falta f in _controlador.Faltas[TipoFaltaInformacion.Multioperador_SubFlota])
                    {
                        object[] agregado = new object[operadores_nuevos + operadores_existentes];
                        agregado[0] = f.Keys[0];
                        for (int i = 1; i < operadores_nuevos + operadores_existentes; i++)
                        {
                            agregado[i] = "";
                        }
                        _valores_editados[tipo].Add(agregado);
                        objectListView_edicion.AddObject(agregado);
                    }
                }
            }
            else if (tipo == TipoFaltaInformacion.Flota_Flota)
            {
                if (_controlador.Faltas[TipoFaltaInformacion.Flota_Flota].Count> 0)
                {
                    //Carga primero existentes
                    DataTable tablaMatrizFlotaFlota = _main._parametrosBase.MatrizFlotaFlotaToDataTable();
                    int operadores_nuevos = _controlador.Faltas[TipoFaltaInformacion.Flota_Flota].Count;
                    int operadores_existentes = tablaMatrizFlotaFlota.Columns.Count;
                    foreach (DataRow data in tablaMatrizFlotaFlota.Rows)
                    {
                        object[] agregado = new object[operadores_nuevos + operadores_existentes];
                        object[] anteriores = data.ItemArray;
                        for (int i = 0; i < operadores_existentes; i++)
                        {
                            agregado[i] = anteriores[i];
                        }
                        for (int i = 0; i < operadores_nuevos; i++)
                        {
                            agregado[operadores_existentes + i] = "";
                        }
                        _valores_editados[tipo].Add(agregado);
                        objectListView_edicion.AddObject(agregado);
                    }
                    //Carga luego los nuevos
                    foreach (Falta f in _controlador.Faltas[TipoFaltaInformacion.Flota_Flota])
                    {
                        object[] agregado = new object[operadores_nuevos + operadores_existentes];
                        agregado[0] = f.Keys[0];
                        for (int i = 1; i < operadores_nuevos + operadores_existentes; i++)
                        {
                            agregado[i] = "";
                        }
                        _valores_editados[tipo].Add(agregado);
                        objectListView_edicion.AddObject(agregado);
                    }
                }
            }

            #endregion

            #region CURVAS
            else if (tipo == TipoFaltaInformacion.Curva_HBT)
            {
                foreach (Falta f in list)
                {
                    object[] parametros = new object[7];
                    parametros[0] = f.Keys[0];
                    parametros[1] = f.Keys[1];
                    parametros[2] = "";
                    parametros[3] = "";
                    parametros[4] = "";
                    parametros[5] = "";
                    parametros[6] = "";
                    _valores_editados[tipo].Add(parametros);
                    objectListView_edicion.AddObject(parametros);
                }
            }
            else if (tipo == TipoFaltaInformacion.Curva_Adelanto || tipo == TipoFaltaInformacion.Curva_Mantto)
            {
                foreach (Falta f in list)
                {
                    object[] parametros = new object[4];
                    parametros[0] = f.Keys[0];
                    parametros[1] = "0";
                    parametros[2] = "0";
                    parametros[3] = "0";
                    _valores_editados[tipo].Add(parametros);
                    objectListView_edicion.AddObject(parametros);
                }
            }
            else if (tipo == TipoFaltaInformacion.Curva_ATC)
            {
                int meses = _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.ATC.ToString()].CantidadDeValoresPorColumna(0);
                int periodos = _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.ATC.ToString()].CantidadDeValoresPorColumna(2);
                for (int i = 1; i <= meses; i++)
                {
                    foreach (Falta f in list)
                    {
                        for (int j = 0; j < periodos; j++)
                        {
                            object[] parametros = new object[6];
                            parametros[0] = i;
                            parametros[1] = f.Keys[0];
                            parametros[2] = j;
                            parametros[3] = "0";
                            parametros[4] = "0";
                            parametros[5] = "0";
                            _valores_editados[tipo].Add(parametros);
                            objectListView_edicion.AddObject(parametros);
                        }
                    }
                }
            }
            else if (tipo == TipoFaltaInformacion.Curva_WXS)
            {
                int meses = _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.METEREOLOGIA.ToString()].CantidadDeValoresPorColumna(0);
                int periodos = _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.METEREOLOGIA.ToString()].CantidadDeValoresPorColumna(2);
                for (int i = 1; i <= meses; i++)
                {
                    foreach (Falta f in list)
                    {
                        for (int j = 0; j < periodos; j++)
                        {
                            object[] parametros = new object[6];
                            parametros[0] = i;
                            parametros[1] = f.Keys[0];
                            parametros[2] = j;
                            parametros[3] = "0";
                            parametros[4] = "0";
                            parametros[5] = "0";
                            _valores_editados[tipo].Add(parametros);
                            objectListView_edicion.AddObject(parametros);
                        }
                    }
                }
            }
            else if (tipo == TipoFaltaInformacion.Curva_OTROS)
            {
                foreach (Falta f in list)
                {
                    int periodos = _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.OTROS.ToString()].CantidadDeValoresPorColumna(1);
                    for (int i = 0; i < periodos; i++)
                    {
                        object[] parametros = new object[5];
                        parametros[0] = f.Keys[0];
                        parametros[1] = i;
                        parametros[2] = "0";
                        parametros[3] = "0";
                        parametros[4] = "0";
                        _valores_editados[tipo].Add(parametros);
                        objectListView_edicion.AddObject(parametros);
                    }
                }
            }
            else if (tipo == TipoFaltaInformacion.Curva_Recursos_Apto)
            {
                foreach (Falta f in list)
                {
                    int periodos = _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.RECURSOS_DEL_APTO.ToString()].CantidadDeValoresPorColumna(1);
                    for (int i = 0; i < periodos; i++)
                    {
                        object[] parametros = new object[5];
                        parametros[0] = f.Keys[0];
                        parametros[1] = i;
                        parametros[2] = "0";
                        parametros[3] = "0";
                        parametros[4] = "0";
                        _valores_editados[tipo].Add(parametros);
                        objectListView_edicion.AddObject(parametros);
                    }
                }
            }
            else if (tipo == TipoFaltaInformacion.Curva_TA_BA)
            {
                foreach (Falta f in list)
                {
                    int periodos = _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.TA_BAJO_ALA.ToString()].CantidadDeValoresPorColumna(1);
                    for (int i = 0; i < periodos; i++)
                    {
                        object[] parametros = new object[5];
                        parametros[0] = f.Keys[0];
                        parametros[1] = i;
                        parametros[2] = "0";
                        parametros[3] = "0";
                        parametros[4] = "0";
                        _valores_editados[tipo].Add(parametros);
                        objectListView_edicion.AddObject(parametros);
                    }
                }
            }
            else if (tipo == TipoFaltaInformacion.Curva_TA_SA)
            {
                foreach (Falta f in list)
                {
                    int periodos = _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.TA_SOBRE_ALA.ToString()].CantidadDeValoresPorColumna(1);
                    for (int i = 0; i < periodos; i++)
                    {
                        object[] parametros = new object[5];
                        parametros[0] = f.Keys[0];
                        parametros[1] = i;
                        parametros[2] = "0";
                        parametros[3] = "0";
                        parametros[4] = "0";
                        _valores_editados[tipo].Add(parametros);
                        objectListView_edicion.AddObject(parametros);
                    }
                }
            }
            else if (tipo == TipoFaltaInformacion.Curva_Trip)
            {
                foreach (Falta f in list)
                {
                    int periodos = _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.TRIPULACIONES.ToString()].CantidadDeValoresPorColumna(1);
                    for (int i = 0; i < periodos; i++)
                    {
                        object[] parametros = new object[5];
                        parametros[0] = f.Keys[0];
                        parametros[1] = i;
                        parametros[2] = "0";
                        parametros[3] = "0";
                        parametros[4] = "0";
                        _valores_editados[tipo].Add(parametros);
                        objectListView_edicion.AddObject(parametros);
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// Carga las faltas de un tipo de problema para ser editadas.
        /// Se ejecuta cuando se hace click sobre uno de los problemas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditarProblema(object sender, EventArgs e)
        {
            object o = objectListView_problemas.GetSelectedObject();
            if (o != null)
            {
                groupBox2.Visible = true;
                _ultima_seleccionada = (KeyValuePair<TipoFaltaInformacion, List<Falta>>)o;
                SetColumnasOLVEdicion(_ultima_seleccionada.Key);
                SetDelegatesOLVEdicion(GetNumeroColumnas(_ultima_seleccionada.Key));
                CargarOLVEdicion(_ultima_seleccionada.Key, _ultima_seleccionada.Value);
            }
        }

        /// <summary>
        /// Carga las faltas de un tipo de problema para ser editadas.
        /// </summary>
        /// <param name="selected">Tipo de falta de información</param>
        private void EditarProblema(TipoFaltaInformacion selected)
        {
            List<Falta> valor = _controlador.Faltas[selected];
            SetColumnasOLVEdicion(selected);
            SetDelegatesOLVEdicion(GetNumeroColumnas(selected));
            CargarOLVEdicion(selected, valor);
        }

        /// <summary>
        /// Retorna la clase de información de un tipo particular de problema
        /// </summary>
        /// <param name="tipo">Tipo de falta de información</param>
        /// <returns>Clasificación</returns>
        private Clase GetClase(TipoFaltaInformacion tipo)
        {
            switch (tipo)
            {
                case TipoFaltaInformacion.AcType_Flota:
                    {
                        return Clase.Parametros;
                    }
                case TipoFaltaInformacion.Multioperador_Operador:
                    {
                        return Clase.Parametros;
                    }
                case TipoFaltaInformacion.Multioperador_SubFlota:
                    {
                        return Clase.Parametros;
                    }
                case TipoFaltaInformacion.SubFlota_Matricula:
                    {
                        return Clase.Parametros;
                    }
                case TipoFaltaInformacion.Flota_Flota:
                    {
                        return Clase.Parametros;
                    }
                case TipoFaltaInformacion.Flota_Grupo:
                    {
                        return Clase.Parametros;
                    }
                case TipoFaltaInformacion.Curva_HBT:
                    {
                        return Clase.Curvas;
                    }
                case TipoFaltaInformacion.TurnAround:
                    {
                        return Clase.Parametros;
                    }
                case TipoFaltaInformacion.Tramo_Ruta:
                    {
                        return Clase.Parametros;
                    }
                case TipoFaltaInformacion.Curva_Adelanto:
                    {
                        return Clase.Curvas;
                    }
                case TipoFaltaInformacion.Curva_ATC:
                    {
                        return Clase.Curvas;
                    }
                case TipoFaltaInformacion.Curva_Mantto:
                    {
                        return Clase.Curvas;
                    }
                case TipoFaltaInformacion.Curva_OTROS:
                    {
                        return Clase.Curvas;
                    }
                case TipoFaltaInformacion.Curva_Recursos_Apto:
                    {
                        return Clase.Curvas;
                    }
                case TipoFaltaInformacion.Curva_TA_BA:
                    {
                        return Clase.Curvas;
                    }
                case TipoFaltaInformacion.Curva_TA_SA:
                    {
                        return Clase.Curvas;
                    }
                case TipoFaltaInformacion.Curva_Trip:
                    {
                        return Clase.Curvas;
                    }
                case TipoFaltaInformacion.Curva_WXS:
                    {
                        return Clase.Curvas;
                    }
                default:
                    {
                        return Clase.Parametros;
                    }
            }
        }

        /// <summary>
        /// Retorna la descripción de un tipo particular de problema
        /// </summary>
        /// <param name="tipo">Tipo de falta de información</param>
        /// <returns>Clasificación</returns>
        private string GetDescripcionFalta(TipoFaltaInformacion tipo)
        {
            switch (tipo)
            {
                case TipoFaltaInformacion.AcType_Flota:
                    {
                        return "Flota (Ac Type)";
                    }
                case TipoFaltaInformacion.Multioperador_Operador:
                    {
                        return "Matriz Multioperador (Operador)";
                    }
                case TipoFaltaInformacion.Multioperador_SubFlota:
                    {
                        return "Matriz Multioperador (Subflota)";
                    }
                case TipoFaltaInformacion.Flota_Flota:
                    {
                        return "Matriz Flota - Flota";
                    }
                case TipoFaltaInformacion.SubFlota_Matricula:
                    {
                        return "Matrícula (Subflota)";
                    }
                case TipoFaltaInformacion.Flota_Grupo:
                    {
                        return "Grupo (Flota)";
                    }
                case TipoFaltaInformacion.Curva_HBT:
                    {
                        return "HBT";
                    }
                case TipoFaltaInformacion.TurnAround:
                    {
                        return "T/A mínimo";
                    }
                case TipoFaltaInformacion.Tramo_Ruta:
                    {
                        return "Negocio";
                    }
                case TipoFaltaInformacion.Curva_Adelanto:
                    {
                        return "Adelanto";
                    }
                case TipoFaltaInformacion.Curva_ATC:
                    {
                        return "ATC";
                    }
                case TipoFaltaInformacion.Curva_Mantto:
                    {
                        return "Mantto";
                    }
                case TipoFaltaInformacion.Curva_OTROS:
                    {
                        return "Otros";
                    }
                case TipoFaltaInformacion.Curva_Recursos_Apto:
                    {
                        return "Recursos aeropuerto";
                    }
                case TipoFaltaInformacion.Curva_TA_BA:
                    {
                        return "T/A bajo ala";
                    }
                case TipoFaltaInformacion.Curva_TA_SA:
                    {
                        return "T/A sobre ala";
                    }
                case TipoFaltaInformacion.Curva_Trip:
                    {
                        return "Tripulación";
                    }
                case TipoFaltaInformacion.Curva_WXS:
                    {
                        return "Meteorología";
                    }
                default:
                    {
                        return "";
                    }
            }
        }

        /// <summary>
        /// Retorna el total de columnas de la tabla de edición para un tipo particular de problema
        /// </summary>
        /// <param name="tipo">Tipo de falta de información</param>
        /// <returns>Clasificación</returns>
        private int GetNumeroColumnas(TipoFaltaInformacion tipo)
        {
            switch (tipo)
            {
                case TipoFaltaInformacion.AcType_Flota:
                    {
                        return 2;
                    }
                case TipoFaltaInformacion.Multioperador_Operador:
                    {
                        return _main._parametrosBase.MatrizMultioperadorToDataTable().Columns.Count + _controlador.Faltas[TipoFaltaInformacion.Multioperador_Operador].Count;
                    }
                case TipoFaltaInformacion.Multioperador_SubFlota:
                    {
                        return _main._parametrosBase.MatrizMultioperadorToDataTable().Columns.Count + _controlador.Faltas[TipoFaltaInformacion.Multioperador_Operador].Count;
                    }
                case TipoFaltaInformacion.Flota_Flota:
                    {
                        return _main._parametrosBase.MatrizFlotaFlotaToDataTable().Columns.Count + _controlador.Faltas[TipoFaltaInformacion.Flota_Flota].Count;
                    }
                case TipoFaltaInformacion.SubFlota_Matricula:
                    {
                        return 2;
                    }
                case TipoFaltaInformacion.Flota_Grupo:
                    {
                        return 2;
                    }
                case TipoFaltaInformacion.Curva_HBT:
                    {
                        return 7;
                    }
                case TipoFaltaInformacion.TurnAround:
                    {
                        return 3;
                    }
                case TipoFaltaInformacion.Tramo_Ruta:
                    {
                        return 3;
                    }
                case TipoFaltaInformacion.Curva_Adelanto:
                    {
                        return 4;
                    }
                case TipoFaltaInformacion.Curva_ATC:
                    {
                        return 6;
                    }
                case TipoFaltaInformacion.Curva_Mantto:
                    {
                        return 4;
                    }
                case TipoFaltaInformacion.Curva_OTROS:
                    {
                        return 5;
                    }
                case TipoFaltaInformacion.Curva_Recursos_Apto:
                    {
                        return 5;
                    }
                case TipoFaltaInformacion.Curva_TA_BA:
                    {
                        return 5;
                    }
                case TipoFaltaInformacion.Curva_TA_SA:
                    {
                        return 5;
                    }
                case TipoFaltaInformacion.Curva_Trip:
                    {
                        return 5;
                    }
                case TipoFaltaInformacion.Curva_WXS:
                    {
                        return 6;
                    }
                default:
                    {
                        return 0;
                    }
            }
        }

        /// <summary>
        /// Inicializa diccionario de valores temporales
        /// </summary>
        /// <param name="tipo"></param>
        private void InicializarTempValores(TipoFaltaInformacion tipo)
        {
            if (_valores_editados.ContainsKey(tipo))
            {
                _valores_editados[tipo].Clear();
            }
            else
            {
                _valores_editados.Add(tipo, new List<object[]>());
            }
        }

        /// <summary>
        /// Sobreescribe método Close() del form
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _main._validadorAbierto = false;
            _main.RefreshTables(null, new ItemsChangedEventArgs());
            _main.SelectTab(0);
            _main.Refresh();
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
        /// Carga las columnas del OLV de edición
        /// </summary>
        /// <param name="headers">Encabezados de las columnas</param>
        /// <param name="cols">Cantidad de columnas con valores no editables</param>
        /// <param name="values">Cantidad de columnas con valores editables</param>
        private void SetColumnasOLVEdicion(string[] headers, int cols, int values)
        {
            for (int i = 0; i < cols; i++)
            {
                objectListView_edicion.AllColumns[i].IsEditable = false;
                objectListView_edicion.AllColumns[i].IsVisible = true;
                objectListView_edicion.AllColumns[i].Text = headers[i];
            }
            for (int i = cols; i < cols + values; i++)
            {
                objectListView_edicion.AllColumns[i].IsEditable = true;
                objectListView_edicion.AllColumns[i].IsVisible = true;
                objectListView_edicion.AllColumns[i].Text = headers[i];
            }
            for (int i = cols + values; i < objectListView_edicion.AllColumns.Count; i++)
            {
                objectListView_edicion.AllColumns[i].IsEditable = false;
                objectListView_edicion.AllColumns[i].IsVisible = false;
            }
        }

        //SOBREESCRIBIR
        /// <summary>
        /// Setea las columnas de edición para un tipo particular de falta de información
        /// </summary>
        /// <param name="tipo"></param>
        private void SetColumnasOLVEdicion(TipoFaltaInformacion tipo)
        {
            #region PARAMETROS

            #region TurnAround

            if (tipo == TipoFaltaInformacion.TurnAround)
            {
                int cols = 2;
                int values = 1;
                string[] headers = _main._parametrosBase.TurnAroundMin.Headers;
                SetColumnasOLVEdicion(headers, cols, values);
            }

            #endregion

            #region Negocio

            else if (tipo == TipoFaltaInformacion.Tramo_Ruta)
            {
                int cols = 1;
                int values = 2;
                string[] headers = _main._parametrosBase.MapVuelosRutas.Headers;
                SetColumnasOLVEdicion(headers, cols, values);
            }

            #endregion

            #region AcType_Flota

            else if (tipo == TipoFaltaInformacion.AcType_Flota)
            {
                int cols = 1;
                int values = 1;
                string[] headers = _main._parametrosBase.MapFlotas.Headers;
                SetColumnasOLVEdicion(headers, cols, values);
            }

            #endregion

            #region Flota_Grupo

            else if (tipo == TipoFaltaInformacion.Flota_Grupo)
            {
                int cols = 1;
                int values = 1;
                string[] headers = _main._parametrosBase.MapGruposFlotas.Headers;
                SetColumnasOLVEdicion(headers, cols, values);
            }

            #endregion

            #region SubFlota_Matricula

            else if (tipo == TipoFaltaInformacion.SubFlota_Matricula)
            {
                int cols = 1;
                int values = 1;
                string[] headers = _main._parametrosBase.MapSubFlotasMatriculas.Headers;
                SetColumnasOLVEdicion(headers, cols, values);
            }

            #endregion

            #region Multioperador

            else if (tipo == TipoFaltaInformacion.Multioperador_SubFlota || tipo == TipoFaltaInformacion.Multioperador_Operador)
            {
                DataTable tablaMatrizMultioperador = _main._parametrosBase.MatrizMultioperadorToDataTable();
                int cols = 1;
                int operadores_nuevos = _controlador.Faltas[TipoFaltaInformacion.Multioperador_Operador].Count;
                int operadores_existentes = tablaMatrizMultioperador.Columns.Count;
                string[] headers = new string[operadores_nuevos + operadores_existentes];
                for (int i = 0; i < operadores_existentes; i++)
                {
                    headers[i] = tablaMatrizMultioperador.Columns[i].ColumnName;
                }
                for (int i = 0; i < operadores_nuevos; i++)
                {
                    headers[i + operadores_existentes] = _controlador.Faltas[TipoFaltaInformacion.Multioperador_Operador][i].Keys[0];
                }
                SetColumnasOLVEdicion(headers, cols, operadores_existentes + operadores_nuevos - cols);
            }

            #endregion

            #region Flota-Flota

            else if (tipo == TipoFaltaInformacion.Flota_Flota)
            {
                DataTable tablaMatrizFlotaFlota = _main._parametrosBase.MatrizFlotaFlotaToDataTable();
                int cols = 1;
                int operadores_nuevos = _controlador.Faltas[TipoFaltaInformacion.Flota_Flota].Count;
                int operadores_existentes = tablaMatrizFlotaFlota.Columns.Count;
                string[] headers = new string[operadores_nuevos + operadores_existentes];
                for (int i = 0; i < operadores_existentes; i++)
                {
                    headers[i] = tablaMatrizFlotaFlota.Columns[i].ColumnName;
                }
                headers[0] = "Flota";
                for (int i = 0; i < operadores_nuevos; i++)
                {
                    headers[i + operadores_existentes] = _controlador.Faltas[TipoFaltaInformacion.Flota_Flota][i].Keys[0];
                }
                SetColumnasOLVEdicion(headers, cols, operadores_existentes + operadores_nuevos - cols);
            }

            #endregion

            #endregion

            #region CURVAS

            #region HBT

            else if (tipo == TipoFaltaInformacion.Curva_HBT)
            {
                int cols = 2;
                int values = 5;
                string[] headers = _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.HBT.ToString()].Headers.ToArray();
                SetColumnasOLVEdicion(headers, cols, values);
            }

            #endregion

            #region Adelanto

            else if (tipo == TipoFaltaInformacion.Curva_Adelanto)
            {
                int cols = 1;
                int values = 3;
                string[] headers = _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.ADELANTO.ToString()].Headers.ToArray();
                SetColumnasOLVEdicion(headers, cols, values);
            }

            #endregion

            #region Mantto

            else if (tipo == TipoFaltaInformacion.Curva_Mantto)
            {
                int cols = 1;
                int values = 3;
                string[] headers = _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.MANTENIMIENTO.ToString()].Headers.ToArray();
                SetColumnasOLVEdicion(headers, cols, values);
            }

            #endregion

            #region Otros

            else if (tipo == TipoFaltaInformacion.Curva_OTROS)
            {
                int cols = 2;
                int values = 3;
                string[] headers = _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.OTROS.ToString()].Headers.ToArray();
                SetColumnasOLVEdicion(headers, cols, values);
            }

            #endregion

            #region Recursos apto

            else if (tipo == TipoFaltaInformacion.Curva_Recursos_Apto)
            {
                int cols = 2;
                int values = 3;
                string[] headers = _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.RECURSOS_DEL_APTO.ToString()].Headers.ToArray();
                SetColumnasOLVEdicion(headers, cols, values);
            }

            #endregion

            #region T/A BA

            else if (tipo == TipoFaltaInformacion.Curva_TA_BA)
            {
                int cols = 2;
                int values = 3;
                string[] headers = _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.TA_BAJO_ALA.ToString()].Headers.ToArray();
                SetColumnasOLVEdicion(headers, cols, values);
            }

            #endregion

            #region T/A SA

            else if (tipo == TipoFaltaInformacion.Curva_TA_SA)
            {
                int cols = 2;
                int values = 3;
                string[] headers = _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.TA_SOBRE_ALA.ToString()].Headers.ToArray();
                SetColumnasOLVEdicion(headers, cols, values);
            }

            #endregion

            #region Tripulaciones

            else if (tipo == TipoFaltaInformacion.Curva_Trip)
            {
                int cols = 2;
                int values = 3;
                string[] headers = _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.TRIPULACIONES.ToString()].Headers.ToArray();
                SetColumnasOLVEdicion(headers, cols, values);
            }

            #endregion

            #region ATC

            else if (tipo == TipoFaltaInformacion.Curva_ATC)
            {
                int cols = 3;
                int values = 3;
                string[] headers = _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.ATC.ToString()].Headers.ToArray();
                SetColumnasOLVEdicion(headers, cols, values);
            }

            #endregion

            #region WXS

            else if (tipo == TipoFaltaInformacion.Curva_WXS)
            {
                int cols = 3;
                int values = 3;
                string[] headers = _main._modeloDisrupcionesBase.ColeccionDisrupciones[TipoDisrupcion.METEREOLOGIA.ToString()].Headers.ToArray();
                SetColumnasOLVEdicion(headers, cols, values);
            }

            #endregion

            #endregion

            objectListView_edicion.RebuildColumns();
        }

        /// <summary>
        /// Setea lo delegates del OLV de edición en función del número total de columnas
        /// </summary>
        /// <param name="nCols">Cantidad total de columnas</param>
        private void SetDelegatesOLVEdicion(int nCols)
        {
            #region AspectGetter

            objectListView_edicion.AllColumns[0].AspectGetter = delegate(object o)
            {
                object[] parametros = (object[])o;
                return nCols > 0 ? parametros[0] : "";
            };
            objectListView_edicion.AllColumns[1].AspectGetter = delegate(object o)
            {
                object[] parametros = (object[])o;
                return nCols > 1 ? parametros[1] : "";
            };
            objectListView_edicion.AllColumns[2].AspectGetter = delegate(object o)
            {
                object[] parametros = (object[])o;
                return nCols > 2 ? parametros[2] : "";
            };
            objectListView_edicion.AllColumns[3].AspectGetter = delegate(object o)
            {
                object[] parametros = (object[])o;
                return nCols > 3 ? parametros[3] : "";
            };
            objectListView_edicion.AllColumns[4].AspectGetter = delegate(object o)
            {
                object[] parametros = (object[])o;
                return nCols > 4 ? parametros[4] : "";
            };
            objectListView_edicion.AllColumns[5].AspectGetter = delegate(object o)
            {
                object[] parametros = (object[])o;
                return nCols > 5 ? parametros[5] : "";
            };
            objectListView_edicion.AllColumns[6].AspectGetter = delegate(object o)
            {
                object[] parametros = (object[])o;
                return nCols > 6 ? parametros[6] : "";
            };
            objectListView_edicion.AllColumns[7].AspectGetter = delegate(object o)
            {
                object[] parametros = (object[])o;
                return nCols > 7 ? parametros[7] : "";
            };
            objectListView_edicion.AllColumns[8].AspectGetter = delegate(object o)
            {
                object[] parametros = (object[])o;
                return nCols > 8 ? parametros[8] : "";
            };
            objectListView_edicion.AllColumns[9].AspectGetter = delegate(object o)
            {
                object[] parametros = (object[])o;
                return nCols > 9 ? parametros[9] : "";
            };

            #endregion

            #region AspectPutter

            objectListView_edicion.AllColumns[0].AspectPutter = delegate(object goal, object value)
            {
                object[] parametros = (object[])goal;
                parametros[0] = value;
            };
            objectListView_edicion.AllColumns[1].AspectPutter = delegate(object goal, object value)
            {
                object[] parametros = (object[])goal;
                parametros[1] = value;
            };
            objectListView_edicion.AllColumns[2].AspectPutter = delegate(object goal, object value)
            {
                object[] parametros = (object[])goal;
                parametros[2] = value;
            };
            objectListView_edicion.AllColumns[3].AspectPutter = delegate(object goal, object value)
            {
                object[] parametros = (object[])goal;
                parametros[3] = value;
            };
            objectListView_edicion.AllColumns[4].AspectPutter = delegate(object goal, object value)
            {
                object[] parametros = (object[])goal;
                parametros[4] = value;
            };
            objectListView_edicion.AllColumns[5].AspectPutter = delegate(object goal, object value)
            {
                object[] parametros = (object[])goal;
                parametros[5] = value;
            };
            objectListView_edicion.AllColumns[6].AspectPutter = delegate(object goal, object value)
            {
                object[] parametros = (object[])goal;
                parametros[6] = value;
            };
            objectListView_edicion.AllColumns[7].AspectPutter = delegate(object goal, object value)
            {
                object[] parametros = (object[])goal;
                parametros[7] = value;
            };
            objectListView_edicion.AllColumns[8].AspectPutter = delegate(object goal, object value)
            {
                object[] parametros = (object[])goal;
                parametros[8] = value;
            };
            objectListView_edicion.AllColumns[9].AspectPutter = delegate(object goal, object value)
            {
                object[] parametros = (object[])goal;
                parametros[9] = value;
            };

            #endregion
        }
        
        /// <summary>
        /// Carga delegates en OLV del lista de problemas
        /// </summary>
        private void SetDelegatesOLVProblemas()
        {
            objectListView_problemas.AllColumns[0].ImageGetter = delegate(object o)
            {
                KeyValuePair<TipoFaltaInformacion, List<Falta>> info = (KeyValuePair<TipoFaltaInformacion, List<Falta>>)o;
                return info.Value.Count > 0 ? 1 : 0;
            };
            objectListView_problemas.AllColumns[1].AspectGetter = delegate(object o)
            {
                KeyValuePair<TipoFaltaInformacion, List<Falta>> info = (KeyValuePair<TipoFaltaInformacion, List<Falta>>)o;
                return GetDescripcionFalta(info.Key);
            };
            objectListView_problemas.AllColumns[2].AspectGetter = delegate(object o)
            {
                KeyValuePair<TipoFaltaInformacion, List<Falta>> info = (KeyValuePair<TipoFaltaInformacion, List<Falta>>)o;
                return info.Value.Count;
            };

            objectListView_problemas.AllColumns[0].GroupKeyGetter = delegate(object o)
            {
                KeyValuePair<TipoFaltaInformacion, List<Falta>> info = (KeyValuePair<TipoFaltaInformacion, List<Falta>>)o;
                return GetClase(info.Key);
            };
            objectListView_problemas.AllColumns[1].GroupKeyGetter = delegate(object o)
            {
                KeyValuePair<TipoFaltaInformacion, List<Falta>> info = (KeyValuePair<TipoFaltaInformacion, List<Falta>>)o;
                return GetClase(info.Key);
            };
            objectListView_problemas.AllColumns[2].GroupKeyGetter = delegate(object o)
            {
                KeyValuePair<TipoFaltaInformacion, List<Falta>> info = (KeyValuePair<TipoFaltaInformacion, List<Falta>>)o;
                return GetClase(info.Key);
            };
        }

        //SOBREESCRIBIR
        /// <summary>
        /// Valida que los valores ingresados al OLV edición sean correctos en función del tipo de falta de información editado.
        /// </summary>
        /// <param name="tipo">Tipo de falta de información</param>
        /// <param name="obj">Elementos de la fila editada</param>
        /// <returns></returns>
        private bool ValoresIngresadosValidos(TipoFaltaInformacion tipo, object[] obj)
        {
            if (obj != null)
            {
                if (tipo == TipoFaltaInformacion.TurnAround)
                {
                    return Utilidades.EsEnteroPositivo(obj[2].ToString());
                }
                else if (tipo == TipoFaltaInformacion.AcType_Flota || tipo == TipoFaltaInformacion.SubFlota_Matricula)
                {
                    return obj[1] != null && obj[1].ToString().Length>0;
                }
                else if (tipo == TipoFaltaInformacion.Tramo_Ruta)
                {
                    return obj[1] != null && obj[1].ToString().Length > 0 && Utilidades.EsUnoCero(obj[2].ToString());
                }
                else if (tipo == TipoFaltaInformacion.Flota_Grupo)
                {
                    //Requisito es que el grupo de flota esté definido.
                    return obj[1] != null && obj[1].ToString().Length > 0 && _main._parametrosBase.InfoGruposFlotas.ContainsKey(obj[1].ToString());
                }
                else if (tipo == TipoFaltaInformacion.Curva_HBT)
                {
                    return Utilidades.EsUnoCero(obj[2].ToString()) && Utilidades.EsNumeroPositivo(obj[3].ToString(), true) && Utilidades.EsNumeroPositivo(obj[4].ToString(), true) && Utilidades.EsNumeroPositivo(obj[5].ToString(), true) && Utilidades.EsNumeroPositivo(obj[6].ToString(), true);
                }
                else if (tipo == TipoFaltaInformacion.Curva_Adelanto || tipo == TipoFaltaInformacion.Curva_Mantto)
                {
                    return Utilidades.EsProbabilidad(obj[1].ToString()) && Utilidades.EsNumeroPositivo(obj[2].ToString(), true) && Utilidades.EsNumeroPositivo(obj[3].ToString(), true);
                }
                else if (tipo == TipoFaltaInformacion.Curva_ATC || tipo == TipoFaltaInformacion.Curva_WXS)
                {
                    return Utilidades.EsProbabilidad(obj[3].ToString()) && Utilidades.EsNumeroPositivo(obj[4].ToString(), true) && Utilidades.EsNumeroPositivo(obj[5].ToString(), true);
                }
                else if (tipo == TipoFaltaInformacion.Curva_OTROS || tipo == TipoFaltaInformacion.Curva_Recursos_Apto || tipo == TipoFaltaInformacion.Curva_TA_BA || tipo == TipoFaltaInformacion.Curva_TA_SA || tipo == TipoFaltaInformacion.Curva_Trip)
                {
                    return Utilidades.EsProbabilidad(obj[2].ToString()) && Utilidades.EsNumeroPositivo(obj[3].ToString(), true) && Utilidades.EsNumeroPositivo(obj[4].ToString(), true);
                }
                else if (tipo == TipoFaltaInformacion.Multioperador_Operador || tipo == TipoFaltaInformacion.Multioperador_SubFlota)
                {
                    bool valido = true;
                    for (int i = 1; i < obj.Length; i++)
                    {
                        valido = valido && Utilidades.EsUnoCero(obj[i].ToString());
                    }
                    return valido;
                }
                else if (tipo == TipoFaltaInformacion.Flota_Flota)
                {
                    bool valido = true;
                    for (int i = 1; i < obj.Length; i++)
                    {
                        valido = valido && Utilidades.EsProbabilidad(obj[i].ToString());
                    }
                    return valido;
                }
            }
            return false;
        }

        #endregion
    }
}
