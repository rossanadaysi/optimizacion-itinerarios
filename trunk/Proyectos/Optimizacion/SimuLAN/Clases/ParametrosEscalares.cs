using System;
using System.Collections.Generic;
using System.Text;

namespace SimuLAN.Clases
{
    /// <summary>
    /// Clase que encapsula parámetros de 1x1.
    /// </summary>
    public class ParametrosEscalares
    {        
        #region ATRIBUTES

        /// <summary>
        /// Profundidad de búsqueda en algoritmo de recovery
        /// </summary>
        private int _gap;

        /// <summary>
        /// Minutos máximos entre una conexión hasta los cuales se crea un pairing
        /// </summary>
        private int _max_pairing;

        /// <summary>
        /// Minutos a partir de los cuales se intenta utilizar un avión de backup
        /// </summary>
        private int _min_backup;

        /// <summary>
        /// Minutos mínimos entre una conexión a partir de los cuales se crea un pairing
        /// </summary>
        private int _min_pairing;

        /// <summary>
        /// Réplicas de la simulación
        /// </summary>
        private int _replicas;

        /// <summary>
        /// Número base para la generación de números aleatorios
        /// </summary>
        private int _semilla;

        /// <summary>
        /// Minutos a partir de los cuales se hacen esfuerzos de recovery
        /// </summary>
        private int _tolerancia;

        /// <summary>
        /// Minutos de atraso a partir de los cuales se usa un turno de tripulación de backup
        /// </summary>
        private int _tolerancia_turno;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Profundidad de búsqueda en algoritmo de recovery
        /// </summary>
        public int Gap
        {
            set { _gap = value; }
            get { return _gap; }
        }

        /// <summary>
        /// Minutos máximos entre una conexión hasta los cuales se crea un pairing
        /// </summary>
        public int MaxPairing
        {
            set { _max_pairing = value; }
            get { return _max_pairing; }
        }

        /// <summary>
        /// Minutos a partir de los cuales se intenta utilizar un avión de backup
        /// </summary>
        public int MinBackup
        {
            set { _min_backup = value; }
            get { return _min_backup; }
        }

        /// <summary>
        /// Minutos mínimos entre una conexión a partir de los cuales se crea un pairing
        /// </summary>
        public int MinPairing
        {
            set { _min_pairing = value; }
            get { return _min_pairing; }
        }

        /// <summary>
        /// Réplicas de la simulación
        /// </summary>
        public int Replicas
        {
            set { _replicas = value; }
            get { return _replicas; }
        }

        /// <summary>
        /// Número base para la generación de números aleatorios
        /// </summary>
        public int Semilla
        {
            set { _semilla = value; }
            get { return _semilla; }
        }

        /// <summary>
        /// Minutos a partir de los cuales se hacen esfuerzos de recovery
        /// </summary>
        public int Tolerancia
        {
            set { _tolerancia = value; }
            get { return _tolerancia; }
        }

        /// <summary>
        /// Minutos de atraso a partir de los cuales se usa un turno de tripulación de backup
        /// </summary>
        public int ToleranciaTurno
        {
            set { _tolerancia_turno = value; }
            get { return _tolerancia_turno; }
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor de objeto ParametrosSimuLAN.
        /// </summary>
        /// <param name="replicas">Número de réplicas de la simulación</param>
        /// <param name="semilla">Base para la generación de números aleatorios</param>
        /// <param name="gap">Profundidad de búsqueda de algoritmo de recovery</param>
        /// <param name="tolerancia">Minutos a partir de los cuales se hacen esfuerzos de recovery</param>
        /// <param name="min_backup">Minutos a partir de los cuales se hacen esfuerzos de backup</param>
        /// <param name="minimo_pairing">Minutos mínimos entre una conexión a partir de los cuales se crea un pairing</param>
        /// <param name="maximo_pairing">Minutos máximos entre una conexión hasta los cuales se crea un pairing</param>
        /// <param name="tolerancia_turno">Minutos de atraso a partir de los cuales se usa un turno de tripulación de backup</param>
        public ParametrosEscalares(int replicas, int semilla, int gap, int tolerancia, int min_backup, int minimo_pairing, int maximo_pairing, int tolerancia_turno)
        {
            this._replicas = replicas;
            this._semilla = semilla;
            this._gap = gap;
            this._tolerancia = tolerancia;
            this._min_backup = min_backup;
            this._min_pairing = minimo_pairing;
            this._max_pairing = maximo_pairing;
            this._tolerancia_turno = tolerancia_turno;
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Actualiza el valor de un parámetro
        /// </summary>
        /// <param name="tag">Identificador</param>
        /// <param name="valor">Valor</param>
        public void SetValor(object tag, int valor)
        {
            string nombre = tag.ToString();
            if(nombre == "replicas") _replicas = valor;
            if(nombre == "semilla") _semilla = valor;
            if(nombre == "gap") _gap = valor;
            if(nombre == "maxConex") _max_pairing = valor;
            if(nombre == "minConex") _min_pairing = valor;
            if(nombre == "toleranciaRecovery") _tolerancia = valor;
            if(nombre == "toleranciaTurnos") _tolerancia_turno = valor;
            if (nombre == "minutosBackup") _min_backup = valor;
        }
    
        #endregion
    }
}
