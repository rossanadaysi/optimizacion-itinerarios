using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SimuLAN.Clases.Optimizacion
{
    public enum FaseOptimizacion{Optimizacion, Ajuste,Inicio};

    public class LogOptimizacion
    {
        private Dictionary<FaseOptimizacion, Dictionary<int, Dictionary<int, ExplicacionImpuntualidad>>> _historial_impuntualidades;

        private Dictionary<FaseOptimizacion,Dictionary<int,Dictionary<int,int>>> _historial_variaciones_tramos;

        private ResumenIteracion _iteracion_optima;

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

        public ResumenIteracion IteracionOptima
        {
            get
            {
                return _iteracion_optima;
            }
        }

        public ResumenIteracion IteracionBase
        {
            get
            {
                return new ResumenIteracion(FaseOptimizacion.Inicio, 1, _historial_impuntualidades[FaseOptimizacion.Inicio][1]);
            }
        }

        public ComparadorIteraciones MejorasDeOptimo
        {
            get
            {
                return new ComparadorIteraciones(IteracionBase,IteracionOptima);
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

        internal void ObtenerIteracionOptima(CriterioOptimizacion criterioOptimizacion, int _std_objetivo)
        {
            if (criterioOptimizacion == CriterioOptimizacion.EstandarPuntualidad)
            {
                double impuntualidad_total = double.MaxValue;
                foreach (FaseOptimizacion fase in _historial_impuntualidades.Keys)
                {
                    foreach (int iteracion in _historial_impuntualidades[fase].Keys)
                    {
                        ResumenIteracion resumen = ResumenIteracion.ConstruirResumenIteracion(fase, iteracion, _historial_impuntualidades, _historial_variaciones_tramos);
                        if (resumen.ImpuntualidadTotal[_std_objetivo] < impuntualidad_total)
                        {
                            _iteracion_optima = resumen;
                            impuntualidad_total = resumen.ImpuntualidadTotal[_std_objetivo];
                        }
                    }
                }
            }
            else if (criterioOptimizacion == CriterioOptimizacion.MinutosAtraso)
            {
                double atraso_total = double.MaxValue;
                foreach (FaseOptimizacion fase in _historial_impuntualidades.Keys)
                {
                    foreach (int iteracion in _historial_impuntualidades[fase].Keys)
                    {
                        ResumenIteracion resumen = ResumenIteracion.ConstruirResumenIteracion(fase, iteracion, _historial_impuntualidades, _historial_variaciones_tramos);
                        if (resumen.AtrasoTotal < atraso_total)
                        {
                            _iteracion_optima = resumen;
                            atraso_total = resumen.AtrasoTotal;
                        }
                    }
                }
            }
        }

        internal void ImprimirDetalles(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            StringBuilder sb = new StringBuilder();
            string tab = "\t";
            sb.Append("\tFase");
            sb.Append("Iteracion");            
            sb.Append("\tId_tramo");
            sb.Append("\tImpuntualidadTotal");
            sb.Append("\tImpuntualidadReaccionarios");
            sb.Append("\tImpuntualidadSinReaccionarios");
            sb.Append("\tAtrasoTotal");
            sb.Append("\tAtrasoReaccionarios");
            sb.Append("\tAtrasoSinReaccionarios");
            sb.Append("\tVariacion");
            sw.WriteLine(sb.ToString());
            foreach (FaseOptimizacion fase in _historial_impuntualidades.Keys)
            {
                foreach (int iteracion in _historial_impuntualidades[fase].Keys)
                {
                    foreach (int id_tramo in _historial_impuntualidades[fase][iteracion].Keys)
                    {
                        int variacion = 0;
                        if (_historial_variaciones_tramos.ContainsKey(fase) && _historial_variaciones_tramos[fase].ContainsKey(iteracion) && _historial_variaciones_tramos[fase][iteracion].ContainsKey(id_tramo))
                        {
                            variacion = _historial_variaciones_tramos[fase][iteracion][id_tramo];

                        }
                        sb = new StringBuilder();
                        sb.Append(fase);
                        sb.Append(tab + iteracion);
                        sb.Append(tab + id_tramo);
                        sb.Append(tab + _historial_impuntualidades[fase][iteracion][id_tramo].InfoParaReporte());
                        sb.Append(tab + variacion);
                        sw.WriteLine(sb.ToString());
                    }
                }
            }
            sw.Close();
            fs.Close();
        }

        internal void ImprimirResumenIteraciones(string path, List<int> dominio, List<int> stds)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            StringBuilder sb = new StringBuilder();
            sb.Append("\tFase");
            sb.Append("Iteracion");
            sb.Append("\tAtrasoTotal");
            sb.Append("\tAtrasoReaccionarios");
            sb.Append("\tAtrasoSinReaccionarios");
            foreach (int std in stds)
            {
                sb.Append("\tImpuntualidadTotalSTD" + std);
                sb.Append("\tImpuntualidadReaccionariosSTD" + std);
                sb.Append("\tImpuntualidadSinReaccionariosSTD" + std);
            }
            sb.Append("\tCantidadVariacionesTotales");
            sb.Append("\tcantidad_variaciones_positivas");
            sb.Append("\tcantidad_variaciones_negativas");
            sb.Append("\tCantidadTramosNoVariados");
            sb.Append("\tpromedio_total_variaciones_absolutas");
            sb.Append("\tpromedio_total_variaciones_absolutas_con_ceros");
            sb.Append("\tpromedio_variaciones_positivos");
            sb.Append("\tpromedio_variaciones_negativos");
            foreach (int dom in dominio)
            {
                sb.Append("\tVariaciones " + dom.ToString());
            }
            sw.WriteLine(sb.ToString());
            foreach (FaseOptimizacion fase in _historial_impuntualidades.Keys)
            {
                foreach (int iteracion in _historial_impuntualidades[fase].Keys)
                {
                    ResumenIteracion resumen_iteracion = ResumenIteracion.ConstruirResumenIteracion(fase, iteracion, _historial_impuntualidades, _historial_variaciones_tramos);
                    sb = new StringBuilder();
                    sb.Append(resumen_iteracion.ImprimirResumen(dominio, stds));
                    sw.WriteLine(sb.ToString());                    
                }
            }
            sw.Close();
            fs.Close();
        }

        internal void ImprimirOptimo(string path,List<int> dominio, List<int> stds)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            StringBuilder sb = new StringBuilder();
            sb.Append("\tCampo");
            sb.Append("Valor");            
            sw.WriteLine(sb.ToString());
            sw.WriteLine(IteracionOptima.ImprimirResumenVertical(dominio, stds));
            sw.WriteLine(MejorasDeOptimo.EscribirResumenVertical(stds));            
            sw.Close();
            fs.Close();
        }
    }
}
