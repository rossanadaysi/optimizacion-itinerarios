using System;
using System.Collections.Generic;
using System.Text;
using SimuLAN.Utils;

namespace InterfazSimuLAN.Reportes
{
    /// <summary>
    /// Clase que encapsula los estadísticos almacenados durante un experimento de simulación
    /// </summary>
    internal class PuntualidadAgrupada
    {
        #region ATRIBUTES

        /// <summary>
        /// Indica el total de ocurrencias de cierto grupo respecto a tramos en el itinerario
        /// </summary>
        private Dictionary<string, double> _contador_totales_por_grupo;

        /// <summary>
        /// Estructura que almacena estadísticos cuantitativos de puntulidad por grupo y estándar de puntualidad.
        /// </summary>
        private Dictionary<string, Dictionary<int, EstadisticosGenerales>> _estadisticos;

        /// <summary>
        /// Almacena la puntualidad por réplica por grupos. Key1: valor grupo; key2: réplica; key3:estándar de puntualidad
        /// </summary>
        private Dictionary<string, Dictionary<int, Dictionary<int, double>>> _puntualidad_por_replica;

        /// <summary>
        /// Cantidad total de grupos
        /// </summary>
        private int _total_grupos;
        
        #endregion

        #region PROPERTIES

        /// <summary>
        /// Indica el total de ocurrencias de cierto grupo respecto a tramos en el itinerario
        /// </summary>
        public Dictionary<string, double> ContadorTotalesPorGrupo
        {
            get { return _contador_totales_por_grupo; }
            set { _contador_totales_por_grupo = value; }
        }

        /// <summary>
        /// Estructura que almacena estadísticos cuantitativos de puntulidad por grupo y estándar de puntualidad.
        /// </summary>
        public Dictionary<string, Dictionary<int, EstadisticosGenerales>> Estadisticos
        {
            get { return _estadisticos; }
            set { _estadisticos = value; }
        }

        /// <summary>
        /// Almacena la puntualidad por réplica por grupos. Key1: valor grupo; key2: réplica; key3:estándar de puntualidad
        /// </summary>
        public Dictionary<string, Dictionary<int, Dictionary<int, double>>> PuntualidadPorReplica
        {
            get { return _puntualidad_por_replica; }
            set { _puntualidad_por_replica = value; }
        }

        /// <summary>
        /// Cantidad total de grupos
        /// </summary>
        public int TotalGrupos
        {
            get { return _total_grupos; }
            set { _total_grupos = value; }
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor del objeto PuntualidadAgrupada
        /// </summary>
        public PuntualidadAgrupada()
        {
            this._total_grupos = 0;
            this._puntualidad_por_replica = new Dictionary<string, Dictionary<int, Dictionary<int, double>>>();
            this._estadisticos = new Dictionary<string, Dictionary<int, EstadisticosGenerales>>();
            this._contador_totales_por_grupo = new Dictionary<string, double>();
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Agrega un nuevo grupo a los diccionarios que almacenan la información sobre la puntualidad
        /// </summary>
        /// <param name="grupo"></param>
        public void AgregarGrupo(string grupo)
        {
            if (!_puntualidad_por_replica.ContainsKey(grupo))
            {
                _puntualidad_por_replica.Add(grupo, new Dictionary<int, Dictionary<int, double>>());
                _estadisticos.Add(grupo, new Dictionary<int, EstadisticosGenerales>());
                _total_grupos++;
            }
        }

        /// <summary>
        /// Para cada grupo se estiman los estadísticos cuantitativos promedio entre las réplicas
        /// </summary>
        public void EstimarEstadisticosGrupo()
        {
            _estadisticos.Clear();

            //Se recorre cada grupo
            foreach (string grupo in _puntualidad_por_replica.Keys)
            {
                //Para cada grupo crea un diccionario de estándares con todos los valores por réplica.
                Dictionary<int, List<double>> valoresPorEstandar = new Dictionary<int,List<double>>();
                
                //Llena el diccionario valoresPorEstandar
                foreach (int replica in _puntualidad_por_replica[grupo].Keys)
                {
                    foreach (int estandar in _puntualidad_por_replica[grupo][replica].Keys)
                    { 
                        if(!valoresPorEstandar.ContainsKey(estandar))
                        {
                            valoresPorEstandar.Add(estandar, new List<double>());
                        }
                        valoresPorEstandar[estandar].Add(_puntualidad_por_replica[grupo][replica][estandar]);
                    }                    
                }

                //Se estiman los estadísticos por grupo - estándar.
                _estadisticos.Add(grupo, new Dictionary<int, EstadisticosGenerales>());
                foreach(int estandar in valoresPorEstandar.Keys)
                {
                    _estadisticos[grupo].Add(estandar, new EstadisticosGenerales(valoresPorEstandar[estandar]));
                    _estadisticos[grupo][estandar].EstimarEstadisticos();
                }            
            }
        }

        #endregion

        #region INTERNAL METHODS

        /// <summary>
        /// Agrega el total de ocurrencias de cierto id a diccionario de contadores.
        /// </summary>
        /// <param name="id">Id de grupo</param>
        /// <param name="contador">Total de ocurrencias de grupo en itinario</param>
        internal void AgregarValorTotalGrupo(string id, double contador)
        {
            if (!_contador_totales_por_grupo.ContainsKey(id))
            {
                _contador_totales_por_grupo.Add(id, contador);
            }
        }

        #endregion
    }
}
