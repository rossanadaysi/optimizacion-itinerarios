using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace InterfazSimuLAN
{
    /// <summary>
    /// Clase que provee una interfaz entre entre la GUI de SimuLAN y el appconfig
    /// </summary>
    public class Configuracion
    {
        #region ATRIBUTES

        /// <summary>
        /// Objeto de configuración que provee acceso a appConfig
        /// </summary>
        private Configuration _config;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor
        /// </summary>
        public Configuracion()
        {
            this._config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Obtiene un parámetro desde appconfig
        /// </summary>
        /// <param name="key">Key del parámetro</param>
        /// <returns></returns>
        public object GetParametro(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// Reemplaza el valor de un setting del appconfig
        /// </summary>
        /// <param name="key">Key del setting</param>
        /// <param name="newValue">Valor del setting</param>
        public void ReplaceSetting(string key, string newValue)
        {
            this._config.AppSettings.Settings.Remove(key);
            this._config.AppSettings.Settings.Add(key, newValue);
            SaveConfiguracion();
            Refresh();
        }

        /// <summary>
        /// Reemplaza el valor de un setting del appconfig
        /// </summary>
        /// <param name="key">Key del setting</param>
        /// <param name="newValue">Valor del setting</param>
        public void ReplaceSetting(string key, int newValue)
        {
            this._config.AppSettings.Settings.Remove(key);
            this._config.AppSettings.Settings.Add(key, newValue.ToString());
            SaveConfiguracion();
        }

        /// <summary>
        /// Guarda el appconfig
        /// </summary>
        public void SaveConfiguracion()
        {
            this._config.Save(ConfigurationSaveMode.Modified);
        }

        /// <summary>
        /// Actualiza los valores retornados con los nuevos valores actualizados.
        /// </summary>
        public void Refresh()
        {
            ConfigurationManager.RefreshSection("appSettings");
        }

        #endregion
    }
}
