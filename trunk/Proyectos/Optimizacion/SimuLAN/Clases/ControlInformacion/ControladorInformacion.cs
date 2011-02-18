using System;
using System.Collections.Generic;
using System.Text;
using SimuLAN.Utils;
using SimuLAN.Clases.Disrupciones;

namespace SimuLAN.Clases.ControlInformacion
{
    /// <summary>
    /// Objeto que encapsula el proceso de detección de información faltante
    /// </summary>
    public class ControladorInformacion
    {
        #region ATRIBUTES

        /// <summary>
        /// Itinerario analizado
        /// </summary>
        private Itinerario _itinerario;

        /// <summary>
        /// Objeto de parámetros
        /// </summary>
        private ParametrosSimuLAN _parametros;

        /// <summary>
        /// Objeto de modelo de disrupciones
        /// </summary>
        private ModeloDisrupciones _modelo_disrupciones;

        /// <summary>
        /// Objeto que almacena las faltas encontradas en la estructura
        /// </summary>
        private Dictionary<TipoFaltaInformacion, List<Falta>> _faltas;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// True si existen faltas de información
        /// </summary>
        public bool HayFaltas
        {
            get 
            {
                foreach (TipoFaltaInformacion tipo in Enum.GetValues(typeof(TipoFaltaInformacion)))
                {
                    if (_faltas[tipo].Count > 0)
                    {
                        return true;
                    }                    
                }
                return false;                
            }
        }
        
        /// <summary>
        /// Objeto que almacena las faltas encontradas en la estructura
        /// </summary>
        public Dictionary<TipoFaltaInformacion, List<Falta>> Faltas
        {
            get{return _faltas;}
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor de objeto controlador de información
        /// </summary>
        /// <param name="itin"> Itinerario analizado</param>
        /// <param name="parametros">Objeto de parámetros de la simulación</param>
        /// <param name="disrupciones">Objeto de modelo de disrupciones</param>
        public ControladorInformacion(Itinerario itin, ParametrosSimuLAN parametros, ModeloDisrupciones disrupciones)
        {
            this._itinerario = itin;
            this._parametros = parametros;
            this._modelo_disrupciones = disrupciones;
            this._faltas = new Dictionary<TipoFaltaInformacion, List<Falta>>();
            CargarTiposFalta();
        }

        private void CargarTiposFalta()
        {
            foreach (TipoFaltaInformacion tipo in Enum.GetValues(typeof(TipoFaltaInformacion)))
            {
                _faltas.Add(tipo, new List<Falta>());
            }
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Valida la información consolidada de itinearios, parámetros y curvas.
        /// </summary>
        public void Validar()
        {
            _faltas.Clear();
            CargarTiposFalta();
            ValidarFlotas();
            ValidarMatriculas();
            ValidarGruposFlota();
            ValidarMultioperador();
            ValidarTurnAround();
            ValidarRutas();
            ValidarFlotaFlota();
            ValidarCurvas();
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Agrega una falta al diccionario de faltas
        /// </summary>
        /// <param name="falta">Falta encontrada en la simulación</param>
        private void AgregarFalta(Falta falta)
        {
            _faltas[falta.Tipo].Add(falta);
        }

        /// <summary>
        /// Validación de curvas
        /// </summary>
        private void ValidarCurvas()
        {
            ValidarCurvasHBT((InfoDisrupcion2D)_modelo_disrupciones.ColeccionDisrupciones[TipoDisrupcion.HBT.ToString()]);
            ValidarCurvasMantto((InfoDisrupcion1D)_modelo_disrupciones.ColeccionDisrupciones[TipoDisrupcion.MANTENIMIENTO.ToString()]);
            ValidarCurvasAeropuerto((InfoDisrupcion1D)_modelo_disrupciones.ColeccionDisrupciones[TipoDisrupcion.ADELANTO.ToString()], TipoFaltaInformacion.Curva_Adelanto);
            ValidarCurvasAeropuerto((InfoDisrupcion3D)_modelo_disrupciones.ColeccionDisrupciones[TipoDisrupcion.ATC.ToString()], TipoFaltaInformacion.Curva_ATC);
            ValidarCurvasAeropuerto((InfoDisrupcion3D)_modelo_disrupciones.ColeccionDisrupciones[TipoDisrupcion.METEREOLOGIA.ToString()], TipoFaltaInformacion.Curva_WXS);
            ValidarCurvasAeropuerto((InfoDisrupcion2D)_modelo_disrupciones.ColeccionDisrupciones[TipoDisrupcion.OTROS.ToString()], TipoFaltaInformacion.Curva_OTROS);
            ValidarCurvasAeropuerto((InfoDisrupcion2D)_modelo_disrupciones.ColeccionDisrupciones[TipoDisrupcion.RECURSOS_DEL_APTO.ToString()], TipoFaltaInformacion.Curva_Recursos_Apto);
            ValidarCurvasAeropuerto((InfoDisrupcion2D)_modelo_disrupciones.ColeccionDisrupciones[TipoDisrupcion.TA_BAJO_ALA.ToString()], TipoFaltaInformacion.Curva_TA_BA);
            ValidarCurvasAeropuerto((InfoDisrupcion2D)_modelo_disrupciones.ColeccionDisrupciones[TipoDisrupcion.TA_SOBRE_ALA.ToString()], TipoFaltaInformacion.Curva_TA_SA);
            ValidarCurvasAeropuerto((InfoDisrupcion2D)_modelo_disrupciones.ColeccionDisrupciones[TipoDisrupcion.TRIPULACIONES.ToString()], TipoFaltaInformacion.Curva_Trip);
        }

        /// <summary>
        /// Validación de curvas que dependen del aeropuerto
        /// </summary>
        /// <param name="info">Información de curvas</param>
        /// <param name="tipo">Tipo de falta buscada</param>
        private void ValidarCurvasAeropuerto(InfoDisrupcion1D info, TipoFaltaInformacion tipo)
        {
            List<string> analizados = new List<string>();
            SerializableDictionary<string, DataDisrupcion> data = info.Parametros;
            foreach (Avion a in _itinerario.AvionesDictionary.Values)
            {
                Tramo tramoAux = a.Tramo_Raiz;
                while (tramoAux != null)
                {
                    string key1 = tramoAux.TramoBase.Origen;
                    List<string> lista = new List<string>();
                    lista.Add(key1);
                    string key = key1;
                    if (!analizados.Contains(key))
                    {
                        analizados.Add(key);
                        if (!data.ContainsKey(key1))
                        {
                            AgregarFalta(new Falta(tipo, lista));
                        }
                    }
                    tramoAux = tramoAux.Tramo_Siguiente;
                }
            }
        }

        /// <summary>
        /// Validación de curvas que dependen del aeropuerto
        /// </summary>
        /// <param name="info">Información de curvas</param>
        /// <param name="tipo">Tipo de falta buscada</param>
        private void ValidarCurvasAeropuerto(InfoDisrupcion2D info, TipoFaltaInformacion tipo)
        {
            List<string> analizados = new List<string>();
            SerializableDictionary<string, SerializableDictionary<string, DataDisrupcion>> data = info.Parametros;
            foreach (Avion a in _itinerario.AvionesDictionary.Values)
            {
                Tramo tramoAux = a.Tramo_Raiz;
                while (tramoAux != null)
                {
                    string key1 = tramoAux.TramoBase.Origen;
                    List<string> lista = new List<string>();
                    lista.Add(key1);
                    string key = key1;
                    if (!analizados.Contains(key))
                    {
                        analizados.Add(key);
                        if (!data.ContainsKey(key1))
                        {
                            AgregarFalta(new Falta(tipo, lista));
                        }
                    }
                    tramoAux = tramoAux.Tramo_Siguiente;
                }
            }
        }

        /// <summary>
        /// Validación de curvas que dependen del aeropuerto
        /// </summary>
        /// <param name="info">Información de curvas</param>
        /// <param name="tipo">Tipo de falta buscada</param>
        private void ValidarCurvasAeropuerto(InfoDisrupcion3D info, TipoFaltaInformacion tipo)
        {
            List<string> analizados = new List<string>();
            SerializableDictionary<string, SerializableDictionary<string, SerializableDictionary<string, DataDisrupcion>>> data = info.Parametros;
            foreach (Avion a in _itinerario.AvionesDictionary.Values)
            {
                Tramo tramoAux = a.Tramo_Raiz;
                while (tramoAux != null)
                {
                    string key1 = tramoAux.TramoBase.Origen;
                    List<string> lista = new List<string>();
                    lista.Add(key1);
                    string key = key1;
                    if (!analizados.Contains(key))
                    {
                        analizados.Add(key);
                        if (!data["1"].ContainsKey(key1))
                        {
                            AgregarFalta(new Falta(tipo, lista));
                        }
                    }
                    tramoAux = tramoAux.Tramo_Siguiente;
                }
            }
        }

        /// <summary>
        /// Validación de curvas HBT
        /// </summary>
        /// <param name="info">Información de curvas HBT</param>
        private void ValidarCurvasHBT(InfoDisrupcion2D info)
        {
            List<string> analizados = new List<string>();
            SerializableDictionary<string, SerializableDictionary<string, DataDisrupcion>> data = info.Parametros;
            foreach (Avion a in _itinerario.AvionesDictionary.Values)
            {
                Tramo tramoAux = a.Tramo_Raiz;
                while (tramoAux != null)
                {
                    string key1 = tramoAux.ParOD;
                    string key2 = a.GrupoAvion != null ? a.GrupoAvion.Nombre : null;
                    List<string> lista = new List<string>();
                    lista.Add(key1);
                    lista.Add(key2 != null ? key2 : " ");
                    string key = key1 + "_" + key2;
                    if (key2 != null && !analizados.Contains(key))
                    {
                        analizados.Add(key);
                        if (!data.ContainsKey(key1))
                        {
                            AgregarFalta(new Falta(TipoFaltaInformacion.Curva_HBT, lista));
                        }
                        else if (!data[key1].ContainsKey(key2))
                        {
                            AgregarFalta(new Falta(TipoFaltaInformacion.Curva_HBT, lista));
                        }
                    }
                    tramoAux = tramoAux.Tramo_Siguiente;
                }
            }
        }

        /// <summary>
        /// Validación de curvas de Mantto
        /// </summary>
        /// <param name="info">Información de curvas de Mantto</param>
        private void ValidarCurvasMantto(InfoDisrupcion1D info)
        {
            List<string> analizados = new List<string>();
            SerializableDictionary<string, DataDisrupcion> data = info.Parametros;
            foreach (Avion a in _itinerario.AvionesDictionary.Values)
            {
                string key1 = a.Matricula;
                string key2 = a.GetFlota(a.AcType);
                List<string> lista = new List<string>();
                string key = key1 + " " + key2;
                lista.Add(key);
                if (!analizados.Contains(key) && key1 != null && key1.Length > 0 && key2 != null && key2.Length > 0)
                {
                    analizados.Add(key);
                    if (!data.ContainsKey(key))
                    {
                        AgregarFalta(new Falta(TipoFaltaInformacion.Curva_Mantto, lista));
                    }
                }
            }
        }

        /// <summary>
        /// Validación de matriz de compatibilidad flota-flota.
        /// </summary>
        private void ValidarFlotaFlota()
        {
            List<string> analizados = new List<string>();
            SerializableDictionary<string, SerializableDictionary<string, double>> data = _parametros.MatrizFlotaFlota;
            foreach (Avion a in _itinerario.AvionesDictionary.Values)
            {
                string key = a.GetFlota(a.AcType);
                List<string> lista = new List<string>();
                lista.Add(key);
                if (!analizados.Contains(key) && key != null && key.Length > 0)
                {
                    analizados.Add(key);
                    if (!data.ContainsKey(key))
                    {
                        AgregarFalta(new Falta(TipoFaltaInformacion.Flota_Flota, lista));
                    }
                }
            }
        }

        /// <summary>
        /// Validación de pareo entre AcTypes y flotas
        /// </summary>
        private void ValidarFlotas()
        {
            List<string> analizados = new List<string>();
            SerializableDictionary<string, string> data = _parametros.MapFlotas.Dict;
            foreach (Avion a in _itinerario.AvionesDictionary.Values)
            {
                List<string> lista = new List<string>();
                string key = a.AcType;
                lista.Add(key);
                if (!analizados.Contains(key))
                {
                    analizados.Add(key);
                    if (!data.ContainsKey(key))
                    {
                        AgregarFalta(new Falta(TipoFaltaInformacion.AcType_Flota, lista));
                    }
                }
            }
        }

        /// <summary>
        /// Validación de pareo entre flotas y grupos de flota
        /// </summary>
        private void ValidarGruposFlota()
        {
            List<string> analizados = new List<string>();
            SerializableDictionary<string, string> data_flota = _parametros.MapFlotas.Dict;
            SerializableDictionary<string, string> data_grupo = _parametros.MapGruposFlotas.Dict;
            foreach (string flota in data_flota.Values)
            {
                List<string> lista = new List<string>();
                lista.Add(flota);
                if (!analizados.Contains(flota))
                {
                    analizados.Add(flota);
                    if (!data_grupo.ContainsKey(flota))
                    {
                        AgregarFalta(new Falta(TipoFaltaInformacion.Flota_Grupo,lista));
                    }
                }
            }   
        }

        /// <summary>
        /// Validación de pareo entre subflotas y matrículas
        /// </summary>
        private void ValidarMatriculas()
        {
            List<string> analizados = new List<string>();
            SerializableDictionary<string, string> data = _parametros.MapSubFlotasMatriculas.Dict;
            foreach (Avion a in _itinerario.AvionesDictionary.Values)
            {
                List<string> lista = new List<string>();
                string key = a.SubFlota;
                lista.Add(key);
                if (!analizados.Contains(key))
                {
                    analizados.Add(key);
                    if (!data.ContainsKey(key))
                    {
                        AgregarFalta(new Falta(TipoFaltaInformacion.SubFlota_Matricula, lista));
                    }
                }
            }
        }

        /// <summary>
        /// Validación de matriz multioperador con respecto a subflotas y operadores.
        /// </summary>
        private void ValidarMultioperador()
        {
            List<string> analizados_subflotas = new List<string>();
            List<string> analizados_operadores = new List<string>();
            SerializableDictionary<string, SerializableDictionary<string, int>> data = _parametros.MatrizMultioperador;
            foreach (Avion a in _itinerario.AvionesDictionary.Values)
            {
                List<string> lista = new List<string>();
                string key = a.SubFlota;
                lista.Add(key);
                if (!analizados_subflotas.Contains(key))
                {
                    analizados_subflotas.Add(key);
                    if (!data.ContainsKey(key))
                    {
                        AgregarFalta(new Falta(TipoFaltaInformacion.Multioperador_SubFlota, lista));
                    }
                    else
                    {
                        Tramo tramoAux = a.Tramo_Raiz;
                        while (tramoAux != null)
                        {
                            string key1 = tramoAux.TramoBase.Ac_Owner;
                            List<string> lista2 = new List<string>();
                            lista2.Add(key1);
                            if (!analizados_operadores.Contains(key1))
                            {
                                analizados_operadores.Add(key1);
                                if (!data[key].ContainsKey(key1))
                                {
                                    AgregarFalta(new Falta(TipoFaltaInformacion.Multioperador_Operador, lista2));
                                }
                            }
                            tramoAux = tramoAux.Tramo_Siguiente;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validación de pareo entre tramos de vuelo e información de negocio o ruta comercial
        /// </summary>
        private void ValidarRutas()
        {
            List<string> analizados = new List<string>();
            SerializableDictionary<string,SerializableDictionary<string,string>> data = _parametros.MapVuelosRutas.Dict;
            foreach (Avion a in _itinerario.AvionesDictionary.Values)
            {
                Tramo tramoAux = a.Tramo_Raiz;
                while (tramoAux != null)
                {
                    string key1 = tramoAux.KeyHUB;
                    if (!analizados.Contains(key1))
                    {
                        analizados.Add(key1);
                        if (!data.ContainsKey(key1))
                        { 
                            List<string> lista = new List<string>();
                            lista.Add(key1);
                            AgregarFalta(new Falta(TipoFaltaInformacion.Tramo_Ruta, lista));
                        }
                    }
                    tramoAux = tramoAux.Tramo_Siguiente;
                }
            }
        }

        /// <summary>
        /// Validación de T/A mínimos por aeropuerto y flota.
        /// </summary>
        private void ValidarTurnAround()
        {
            List<string> analizados = new List<string>();

            SerializableDictionary<string, SerializableDictionary<string, string>> data = _parametros.TurnAroundMin.Dict;
            foreach (Avion a in _itinerario.AvionesDictionary.Values)
            {
                Tramo tramoAux = a.Tramo_Raiz;
                while (tramoAux != null)
                {
                    string key1 = tramoAux.FlotaProgramada;
                    string key2 = tramoAux.TramoBase.Origen;
                    List<string> lista = new List<string>();
                    lista.Add(key1);
                    lista.Add(key2);
                    string key = key1 + key2;
                    if (key != null && !analizados.Contains(key))
                    {
                        analizados.Add(key);
                        if (key1 != null && !data.ContainsKey(key1))
                        {
                            AgregarFalta(new Falta(TipoFaltaInformacion.TurnAround, lista));
                        }
                        else if (key1!=null && key2 != null && !data[key1].ContainsKey(key2))
                        {
                            AgregarFalta(new Falta(TipoFaltaInformacion.TurnAround, lista));
                        }
                    }
                    tramoAux = tramoAux.Tramo_Siguiente;
                }
            }
        }

        #endregion
    
    }
}
