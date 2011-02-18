using System;
using System.Collections.Generic;
using System.Text;

namespace SimuLAN.Clases
{    
    /// <summary>
    /// Clase Evento: usada para encapsular los eventos de la simulación y facilitar su enlistamiento.
    /// </summary>
    public class Evento: IComparable
    {
        #region ATRIBUTES

        /// <summary>
        /// Delegado que encapsula acción del evento
        /// </summary>
        private MetodoEventoEventHandler _accion_evento; 

        /// <summary>
        /// Tipo de evento
        /// </summary>
        private TipoEvento _tipo_evento;

        /// <summary>
        /// Tiempo de inicio del evento
        /// </summary>
        private int _tiempo_inicio_evento;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Delegado que encapsula acción del evento
        /// </summary>
        public MetodoEventoEventHandler AccionEvento
        {
            get { return _accion_evento; }
        }

        /// <summary>
        /// Tipo de evento
        /// </summary>
        public TipoEvento TipoEvento
        {
            get { return _tipo_evento; }
        }

        /// <summary>
        /// Tiempo de inicio del evento
        /// </summary>
        public int TiempoInicioEvento
        {
            get { return _tiempo_inicio_evento; }
            set { _tiempo_inicio_evento = value; }
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor de eventos
        /// </summary>
        /// <param name="tipoEvento">Tipo de evento</param>
        /// <param name="tiempoInicioEvento">Tiempo de inicio del evento</param>
        /// <param name="accionEvento">Delegado que encapsula acción del evento</param>
        public Evento(TipoEvento tipoEvento, int tiempoInicioEvento, MetodoEventoEventHandler accionEvento)
        {
            this._tipo_evento = tipoEvento;
            this._tiempo_inicio_evento = tiempoInicioEvento;
            this._accion_evento = accionEvento;
        }

        #endregion

        #region PUBLIC METHODS

        public bool EsDelTipo(TipoEvento tipo)
        {
            return this.TipoEvento == tipo;
        }

        #region IComparable Members

        /// <summary>
        /// Comparar dos eventos con respecto al tiempo de ejecución
        /// </summary>
        public int CompareTo(object obj)
        {
            Evento e = (Evento) obj;
            if (this._tiempo_inicio_evento < e._tiempo_inicio_evento)
            {
                return -1;
            }
            else if (this._tiempo_inicio_evento > e._tiempo_inicio_evento)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        #endregion

        /// <summary>
        /// Compara dos eventos respecto al tipo de acción que encapsula
        /// </summary>
        public bool esDelMismoTipo(Evento eventoComparado)
        {
            return this.TipoEvento == eventoComparado.TipoEvento;
        }

        #endregion

        #region IDisposable Members

        private bool IsDisposed=false;

        /// <summary>
        /// Destructor de instancias de la clase
        /// </summary>
        ~Evento()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool Disposing)
        {
            if(!IsDisposed)
            {
                if(Disposing)
                {

                }
                _accion_evento = null;
            }
            IsDisposed=true;
        }
        #endregion
    }
}
