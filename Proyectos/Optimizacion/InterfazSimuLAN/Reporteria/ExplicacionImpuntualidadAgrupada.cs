using System;
using System.Collections.Generic;
using System.Text;
using SimuLAN.Clases;
using SimuLAN.Utils;

namespace InterfazSimuLAN.Reportes
{
    /// <summary>
    /// Clase que encapsula información desplegada en reporte de explicación de la impuntualidad por grupos.
    /// </summary>
    public class ExplicacionImpuntualidadAgrupada
    {
        #region ATRIBUTES

        /// <summary>
        /// Indica el total de ocurrencias de cierto grupo y tipo de disrupcion respecto a tramos en el itinerario
        /// </summary>
        private Dictionary<string,double> _contador_totales_por_grupo;

        /// <summary>
        /// Estructura que almacena estadísticos cuantitativos de puntulidad por grupo, estándar de puntualidad y tipo de disrupcion.
        /// </summary>
        private Dictionary<string, Dictionary<int, Dictionary<TipoDisrupcion,EstadisticosGenerales>>> _estadisticos;

        /// <summary>
        /// Almacena la puntualidad por réplica por grupos. Key1: valor grupo; key2: réplica; key3:estándar de puntualidad, key4: tipo disrupcion
        /// </summary>
        private Dictionary<string, Dictionary<int, Dictionary<int, Dictionary<TipoDisrupcion,double>>>> _impuntualidad_por_replica;

        /// <summary>
        /// Cantidad total de grupos
        /// </summary>
        private int _cantidad_total_grupos;
        
        #endregion

        #region PROPERTIES

        /// <summary>
        /// Indica el total de ocurrencias de cierto grupo respecto a tramos en el itinerario
        /// </summary>
        public Dictionary<string,double> ContadorTotalesPorGrupo
        {
            get { return _contador_totales_por_grupo; }
            set { _contador_totales_por_grupo = value; }
        }

        /// <summary>
        /// Estructura que almacena estadísticos cuantitativos de puntulidad por grupo y estándar de puntualidad.
        /// </summary>
        public Dictionary<string, Dictionary<int, Dictionary<TipoDisrupcion,EstadisticosGenerales>>> Estadisticos
        {
            get { return _estadisticos; }
            set { _estadisticos = value; }
        }

        /// <summary>
        /// Almacena la puntualidad por réplica por grupos. Key1: valor grupo; key2: réplica; key3:estándar de puntualidad
        /// </summary>
        public Dictionary<string, Dictionary<int, Dictionary<int, Dictionary<TipoDisrupcion,double>>>> ImpuntualidadPorReplica
        {
            get { return _impuntualidad_por_replica; }
            set { _impuntualidad_por_replica = value; }
        }

        /// <summary>
        /// Cantidad total de grupos
        /// </summary>
        public int TotalGrupos
        {
            get { return _cantidad_total_grupos; }
            set { _cantidad_total_grupos = value; }
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor del objeto ExplicacionImpuntualidadAgrupada
        /// </summary>
        public ExplicacionImpuntualidadAgrupada()
        {
            this._cantidad_total_grupos = 0;
            this._impuntualidad_por_replica = new Dictionary<string,Dictionary<int,Dictionary<int,Dictionary<TipoDisrupcion,double>>>>();
            this._estadisticos = new Dictionary<string,Dictionary<int,Dictionary<TipoDisrupcion,EstadisticosGenerales>>>();
            this._contador_totales_por_grupo = new Dictionary<string, double>();
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Agrega un nuevo grupo a los diccionarios que almacenan la información sobre la impuntualidad por tipo de disrupción
        /// </summary>
        /// <param name="grupo"></param>
        public void AgregarGrupo(string grupo)
        {
            if (!_impuntualidad_por_replica.ContainsKey(grupo))
            {
                _impuntualidad_por_replica.Add(grupo, new Dictionary<int, Dictionary<int, Dictionary<TipoDisrupcion, double>>>());
                _estadisticos.Add(grupo, new Dictionary<int, Dictionary<TipoDisrupcion, EstadisticosGenerales>>());
                _cantidad_total_grupos++;
            }
        }

        /// <summary>
        /// Para cada grupo se estiman los estadísticos cuantitativos promedio entre las réplicas
        /// </summary>
        public void EstimarEstadisticosGrupo()
        {
            _estadisticos.Clear();

            //Se recorre cada grupo
            foreach (string grupo in _impuntualidad_por_replica.Keys)
            {
                //Para cada grupo crea un diccionario de estándares con todos los valores por réplica.
                Dictionary<int, Dictionary<TipoDisrupcion, List<double>>> valoresPorEstandar = new Dictionary<int, Dictionary<TipoDisrupcion, List<double>>>();
                
                //Llena el diccionario valoresPorEstandar
                foreach (int replica in _impuntualidad_por_replica[grupo].Keys)
                {
                    foreach (int estandar in _impuntualidad_por_replica[grupo][replica].Keys)
                    { 
                        if(!valoresPorEstandar.ContainsKey(estandar))
                        {
                            valoresPorEstandar.Add(estandar, new Dictionary<TipoDisrupcion, List<double>>());
                        }
                        foreach (TipoDisrupcion tipo in _impuntualidad_por_replica[grupo][replica][estandar].Keys)
                        {
                            if (!valoresPorEstandar[estandar].ContainsKey(tipo))
                            {
                                valoresPorEstandar[estandar].Add(tipo, new List<double>());
                            }
                            valoresPorEstandar[estandar][tipo].Add(_impuntualidad_por_replica[grupo][replica][estandar][tipo]);
                        }                        
                    }                
                }

                //Se estiman los estadísticos por grupo - estándar.
                _estadisticos.Add(grupo, new Dictionary<int, Dictionary<TipoDisrupcion, EstadisticosGenerales>>());
                foreach(int estandar in valoresPorEstandar.Keys)
                {
                    _estadisticos[grupo].Add(estandar, new Dictionary<TipoDisrupcion, EstadisticosGenerales>());
                    foreach (TipoDisrupcion tipo in valoresPorEstandar[estandar].Keys)
                    {
                        _estadisticos[grupo][estandar].Add(tipo, new EstadisticosGenerales(valoresPorEstandar[estandar][tipo]));
                        _estadisticos[grupo][estandar][tipo].EstimarEstadisticos();
                    }
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
