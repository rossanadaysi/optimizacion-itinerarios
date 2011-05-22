using System;
using System.Collections.Generic;
using System.Text;

namespace SimuLAN.Clases.Optimizacion
{
    public enum FaseOptimizacion{Optimizacion, Ajuste,Inicio};

    public class LogOptimizacion
    {
        private Dictionary<FaseOptimizacion, Dictionary<int, Dictionary<int, ExplicacionImpuntualidad>>> _historial_impuntualidades;

        private Dictionary<FaseOptimizacion,Dictionary<int,Dictionary<int,int>>> _historial_variaciones_tramos;

        public Dictionary<FaseOptimizacion, Dictionary<int, Dictionary<int, ExplicacionImpuntualidad>>> HistorialImpuntualidad
        {
            get
            {
                return _historial_impuntualidades;
            }
        }

        public Dictionary<FaseOptimizacion, Dictionary<int, Dictionary<int, int>>> HistorialVariaciones
        {
            get
            {
                return _historial_variaciones_tramos;
            }
        }

        public LogOptimizacion()
        {
            this._historial_impuntualidades = new Dictionary<FaseOptimizacion, Dictionary<int, Dictionary<int, ExplicacionImpuntualidad>>>();
            this._historial_variaciones_tramos = new Dictionary<FaseOptimizacion, Dictionary<int, Dictionary<int, int>>>();
        }

        public void AgregarInfoImpuntualidad(int iteracion, FaseOptimizacion fase, Dictionary<int, ExplicacionImpuntualidad> impuntualidades)
        {
            if (!_historial_impuntualidades.ContainsKey(fase))
            {
                _historial_impuntualidades.Add(fase, new Dictionary<int, Dictionary<int, ExplicacionImpuntualidad>>());
            }
            _historial_impuntualidades[fase].Add(iteracion, impuntualidades);
        }

        public void AgregarInfoVariaciones(int iteracion, FaseOptimizacion fase, Dictionary<int, int> variaciones)
        {
            if (!_historial_variaciones_tramos.ContainsKey(fase))
            {
                _historial_variaciones_tramos.Add(fase, new Dictionary<int, Dictionary<int, int>>());
            }
            _historial_variaciones_tramos[fase].Add(iteracion, variaciones);
        }
    }
}
