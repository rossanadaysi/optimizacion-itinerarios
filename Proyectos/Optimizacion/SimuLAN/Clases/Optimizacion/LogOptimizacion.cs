﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SimuLAN.Clases.Recovery;

namespace SimuLAN.Clases.Optimizacion
{
    public enum FaseOptimizacion{Optimizacion, Ajuste,Inicio};

    public class LogOptimizacion
    {
        private Dictionary<FaseOptimizacion, Dictionary<int, Dictionary<int, ExplicacionImpuntualidad>>> _historial_impuntualidades;

        private Dictionary<FaseOptimizacion,Dictionary<int,Dictionary<int,int>>> _historial_variaciones_tramos;

        private Dictionary<FaseOptimizacion,Dictionary<int,Dictionary<int,List<Swap>>>> _historial_recovery;

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

        public int Simulaciones
        {
            get
            {
                int simulaciones = 0;
                foreach (FaseOptimizacion fase in _historial_impuntualidades.Keys)
                {
                    simulaciones += _historial_impuntualidades[fase].Count;
                }
                return simulaciones;
            }
        }

        public int Optimizaciones
        {
            get
            {
                int optimizaciones = 0;
                if (_historial_impuntualidades != null && _historial_impuntualidades.ContainsKey(FaseOptimizacion.Optimizacion))
                {
                    optimizaciones = _historial_impuntualidades[FaseOptimizacion.Optimizacion].Count;
                }
                return optimizaciones;
            }
        }

        public int Ajustes
        {
            get
            {
                int ajustes = 0;
                if (_historial_impuntualidades != null && _historial_impuntualidades.ContainsKey(FaseOptimizacion.Ajuste))
                {
                    ajustes = _historial_impuntualidades[FaseOptimizacion.Ajuste].Count;
                }
                return ajustes;
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
            this._historial_recovery = new Dictionary<FaseOptimizacion, Dictionary<int, Dictionary<int, List<Swap>>>>();
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
        public void AgregarInfoRecovery(int iteracion,FaseOptimizacion fase, Dictionary<int, List<Swap>> swaps)
        {
            if (!_historial_recovery.ContainsKey(fase))
            {
                _historial_recovery.Add(fase, new Dictionary<int, Dictionary<int, List<Swap>>>());
            }
            _historial_recovery[fase].Add(iteracion, swaps);
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

        internal void ImprimirDetallesPuntualidad(string path)
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

        internal void ImprimirOptimo(string path,List<int> dominio, List<int> stds, Itinerario itinerario, double minutos_simulacion, double minutos_optimizacion)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            StringBuilder sb = new StringBuilder();
            sb.Append("\tCampo");
            sb.Append("Valor");            
            sw.WriteLine(sb.ToString());
            sw.WriteLine("Tramos\t" + itinerario.Tramos.Count.ToString());
            sw.WriteLine("Aviones\t" + itinerario.AvionesDictionary.Count.ToString());
            sw.WriteLine("Dias\t" + itinerario.FechaTermino.Subtract(itinerario.FechaInicio).TotalDays.ToString());
            sw.WriteLine("Minutos simulacion\t" + minutos_simulacion.ToString());
            sw.WriteLine("Minutos optimizacion\t" + minutos_optimizacion.ToString());
            sw.WriteLine("Tiempo total\t" + (minutos_optimizacion + minutos_simulacion).ToString());
            sw.WriteLine("Simulaciones\t" + Simulaciones.ToString());
            sw.WriteLine("Optimizaciones\t" + Optimizaciones.ToString());
            sw.WriteLine("Ajustes\t" + Ajustes.ToString());
            sw.WriteLine(IteracionOptima.ImprimirResumenVertical(dominio, stds));
            sw.WriteLine(MejorasDeOptimo.EscribirResumenVertical(stds));            
            sw.Close();
            fs.Close();
        }

        internal void ImprimirDetallesRecovery(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            StringBuilder sb = new StringBuilder();
            string tab = "\t";
            sb.Append("Fase");
            sb.Append("\tIteracion");
            sb.Append("\tRéplica");
            sb.Append("\tId_swap");
            sb.Append("\tFechaIni");
            sb.Append("\tId_Avion_Emisor");
            sb.Append("\tId_Avion_Receptor");
            sb.Append("\tId_Tramo_ini_1");
            sb.Append("\tId_Tramo_ini_2");
            sb.Append("\tId-Tramo_fin_1");
            sb.Append("\tId_Tramo_fin_2");
            sb.Append("\tPunto Rotación");
            sb.Append("\tNum_Legs_Emisor");
            sb.Append("\tNum_Legs_Receptor");
            sb.Append("\tRC_Ini");
            sb.Append("\tMnts_Atraso--");
            sb.Append("\tMnts_Atraso++");
            sb.Append("\tMnts_Ganancia_Neta");
            sb.Append("\tNum_Legs_Beneficiadas");
            sb.Append("\tNum_Legs_Perjudicadas");
            sw.WriteLine(sb.ToString());
            foreach (FaseOptimizacion fase in _historial_recovery.Keys)
            {
                foreach (int iteracion in _historial_recovery[fase].Keys)
                {
                    foreach (int replica in _historial_recovery[fase][iteracion].Keys)
                    {
                        foreach (Swap swap in _historial_recovery[fase][iteracion][replica])
                        {
                            sb = new StringBuilder();
                            sb.Append(fase);
                            sb.Append(tab + iteracion);
                            sb.Append(tab + replica);
                            sb.Append(tab + swap.InfoParaReporte());                            
                            sw.WriteLine(sb.ToString());
                        }                        
                    }
                }
            }
            sw.Close();
            fs.Close();
        }
    }
}
