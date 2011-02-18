using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SimuLAN.Clases
{
    /// <summary>
    /// Esta clase representa un tipo particular de flota.
    /// </summary>
    [XmlRoot("AcType")]    
    public class AcType: IDisposable
    {
        #region ATRIBUTES
        
        /// <summary>
        /// Nombre del AcType
        /// </summary>
        private string _nombre;
        
        /// <summary>
        /// Flota a la que pertenece el AcType
        /// </summary>
        private string _flota;
        
        /// <summary>
        /// True si es un AcType presente en la simulación
        /// </summary>
        private bool _activo;

        #endregion

        #region PROPERTIES
        
        /// <summary>
        /// True si es un AcType presente en la simulación
        /// </summary>
        public bool Activo
        {
            get { return _activo; }
            set { _activo = value; }    
        }

        /// <summary>
        /// Flota a la que pertenece el AcType
        /// </summary>
        public string Flota
        {
            get { return _flota; }
            set { _flota = value; }
        }

        /// <summary>
        /// Nombre del AcType
        /// </summary>
        public string Nombre
        {
            get { return _nombre; }
            set { _nombre = value; }
        }

        #endregion

        #region CONSTRUCTORS
        
        /// <summary>
        /// Crea instancia vacía de AcType
        /// </summary>
        public AcType()
        { 
        }

        /// <summary>
        /// Crea instancia de AcType desde itienerario
        /// </summary>
        /// <param name="nombre"></param>
        public AcType(string nombre)
        {
            this._nombre = nombre;
            this._flota = null;
            this._activo = true;
        }

        /// <summary>
        /// Crea instancia de AcType desde tabla AcType-Flota
        /// </summary>
        /// <param name="nombre"></param>
        /// <param name="flota"></param>
        public AcType(string nombre, string flota)
        {
            this._nombre = nombre;
            this._flota = flota;
            this._activo = false;
        }

        #endregion

        #region PUBLIC METHODS
        
        /// <summary>
        /// ToString
        /// </summary>
        /// <returns>Retorna string con nombre de AcType</returns>
        public override string ToString()
        {
            return _nombre.ToString();
        }
        
        #endregion

        #region IDisposable Members

        private bool IsDisposed=false;

        /// <summary>
        /// Destructor de instancias de la clase
        /// </summary>
        ~AcType()
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
                _nombre = null;
                _flota = null;
            }
            IsDisposed=true;
        }
        #endregion
    }
}
