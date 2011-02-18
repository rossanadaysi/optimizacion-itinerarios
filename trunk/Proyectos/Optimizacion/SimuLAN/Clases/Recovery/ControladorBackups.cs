using System;
using System.Collections.Generic;
using System.Text;
using SimuLAN.Utils;
using SimuLAN.Clases.Disrupciones;

namespace SimuLAN.Clases.Recovery
{
    /// <summary>
    /// Clase que encapsula manejo de backups y AOG
    /// </summary>
    public class ControladorBackups
    {
        #region ATRIBUTES

        /// <summary>
        /// AOGs por flota, origen y día de simulación.
        /// </summary>
        private Dictionary<string, Dictionary<string, Dictionary<DateTime, double>>> _AOGs;
        
        /// <summary>
        /// Backups clasificados por flota, origen y día de simulación.
        /// </summary>
        private Dictionary<string, Dictionary<string, Dictionary<DateTime, List<UnidadBackup>>>> _backups_clasificados;

        /// <summary>
        /// Lista con los backups definidos en itinerio e interfaz.
        /// </summary>
        private SerializableList<UnidadBackup> _backups_lista;

        /// <summary>
        /// Delegado para obtener la flota de un determinado AcType.
        /// </summary>
        private GetFlotaEventHandler _get_flota;

        /// <summary>
        /// Objeto para la generación de números aleatorios
        /// </summary>
        private Random _rdm;

        #endregion

        #region PROPERTIES
        
        /// <summary>
        /// Lista con los backups definidos en itinerio e interfaz.
        /// </summary>
        public SerializableList<UnidadBackup> BackupsLista
        {
            get { return _backups_lista; }
        }
        
        #endregion
        
        #region CONSTRUCTOR
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="getFlota">delegado para obtener la flota de un AcType</param>
        public ControladorBackups(GetFlotaEventHandler getFlota)
        {
            this._backups_lista = new SerializableList<UnidadBackup>();
            this._backups_clasificados = new Dictionary<string, Dictionary<string, Dictionary<DateTime, List<UnidadBackup>>>>();
            this._AOGs = new Dictionary<string, Dictionary<string, Dictionary<DateTime, double>>>();
            this._get_flota = getFlota;
            this._rdm = new Random();
        }
        
        #endregion

        #region INTERNAL METHODS

        /// <summary>
        /// Genera AOG de manera aleatoria entre dos fechas.
        /// </summary>
        /// <param name="fechaIni">Fecha inicio</param>
        /// <param name="fechaFin">Fecha término</param>
        /// <param name="infoAOG">Información de AOG</param>
        internal void GenerarAOGs(DateTime fechaIni, DateTime fechaFin, SerializableDictionary<string, DataDisrupcion> infoAOG)
        {            
            ClasificarBackups(fechaIni, fechaFin);
            foreach (string flota in _backups_clasificados.Keys)
            {
                _AOGs.Add(flota, new Dictionary<string, Dictionary<DateTime, double>>());
                foreach (string origen in _backups_clasificados[flota].Keys)
                {
                    _AOGs[flota].Add(origen, new Dictionary<DateTime, double>());
                    foreach (DateTime fecha in _backups_clasificados[flota][origen].Keys)
                    {
                        _AOGs[flota][origen].Add(fecha, 0);
                        string key = flota + "_" + origen + "_" + fecha.Month.ToString();
                        if (infoAOG.ContainsKey(key))
                        {
                            DataDisrupcion data = infoAOG[key];
                            double dias_AOG = Distribuciones.GenerarAleatorio(_rdm, DistribucionesEnum.Normal, data.Prob, data.Media, data.Desvest, 0, 1);
                            _AOGs[flota][origen][fecha] = dias_AOG * 24;
                        }
                        else
                        { 
                            //No hay info de AOG para cierta flota - origen y mes. No se hace nada.
                        }
                        UsarBackupsPorAOG(_backups_clasificados[flota][origen][fecha], _AOGs[flota][origen][fecha]);
                    }
                }
            }
        }

        /// <summary>
        /// Setea la semilla del Random del objeto
        /// </summary>
        /// <param name="semilla">Semilla</param>
        internal void SetSemilla(int semilla)
        {
            this._rdm = new Random(semilla);
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        ///Separa los backups del itinerario por flota, origen y fecha.
        /// </summary>
        /// <param name="fechaIni">Fecha inicio</param>
        /// <param name="fechaFin">Fecha término</param>
        private void ClasificarBackups(DateTime fechaIni, DateTime fechaFin)
        {
            foreach (UnidadBackup bu in _backups_lista)
            {
                string origen = bu.Estacion;
                string flota = _get_flota(bu.TramoBase.AcType);
                DateTime fecha = bu.TramoBase.Fecha_Salida;
                if (!_backups_clasificados.ContainsKey(flota))
                {
                    _backups_clasificados.Add(flota, new Dictionary<string, Dictionary<DateTime, List<UnidadBackup>>>());
                }
                if (!_backups_clasificados[flota].ContainsKey(origen))
                {
                    _backups_clasificados[flota].Add(origen, new Dictionary<DateTime, List<UnidadBackup>>());
                }
                if (!_backups_clasificados[flota][origen].ContainsKey(fecha))
                {
                    _backups_clasificados[flota][origen].Add(fecha, new List<UnidadBackup>());
                }
                _backups_clasificados[flota][origen][fecha].Add(bu);
            }
        }

        /// <summary>
        /// Resta horas backups por causa de AOG
        /// </summary>
        /// <param name="lista_backups">Lista de BU utilizadas</param>
        /// <param name="horas_AOG">Horas de AOG restadas</param>
        private void UsarBackupsPorAOG(List<UnidadBackup> lista_backups, double horas_AOG)
        {
            foreach (UnidadBackup bu in lista_backups)
            {
                horas_AOG -= bu.UsarPorAOG(horas_AOG);          
            }
        }
        
        #endregion
    }
}
