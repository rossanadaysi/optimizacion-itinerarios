using System;
using System.Collections.Generic;
using SimuLAN.Utils;

namespace SimuLAN.Clases.Optimizacion
{
    public class OrganizadorTramos
    {
        private Dictionary<int, InfoTramoParaOptimizacion> _tramos;
        private Dictionary<string, List<InfoTramoParaOptimizacion>> _tramos_por_avion;

        public Dictionary<string, List<InfoTramoParaOptimizacion>> TramosPorAvion
        {
            get { return _tramos_por_avion; }
        }

        public EstadisticosGenerales PuntualidadBase
        {
            get
            {
                List<double> valores = new List<double>();
                foreach (int id_tramo in _tramos.Keys)
                {
                    valores.Add(_tramos[id_tramo].ExplicacionImpuntualidadBase.PuntualidadTotal);                 
                }
                return new EstadisticosGenerales(valores);
            }
        }

        public EstadisticosGenerales ImpuntualidadReaccionariosBase
        {
            get
            {
                List<double> valores = new List<double>();
                foreach (int id_tramo in _tramos.Keys)
                {
                    valores.Add(_tramos[id_tramo].ExplicacionImpuntualidadBase.ImpuntualidadReaccionarios);
                }
                return new EstadisticosGenerales(valores);
            }
        }

        public EstadisticosGenerales PuntualidadActual
        {
            get
            {
                List<double> valores = new List<double>();
                foreach (int id_tramo in _tramos.Keys)
                {
                    valores.Add(_tramos[id_tramo].ExplicacionImpuntualidadActual.PuntualidadTotal);
                }
                return new EstadisticosGenerales(valores);
            }
        }

        public EstadisticosGenerales ImpuntualidadReaccionariosActual
        {
            get
            {
                List<double> valores = new List<double>();
                foreach (int id_tramo in _tramos.Keys)
                {
                    valores.Add(_tramos[id_tramo].ExplicacionImpuntualidadActual.ImpuntualidadReaccionarios);
                }
                return new EstadisticosGenerales(valores);
            }
        }

        public int CantidadTramosOptimizables
        {
            get 
            {
                int num = 0;
                foreach (int id in _tramos.Keys)
                {
                    if (_tramos[id].TramoOptimizable)
                    {
                        num++;
                    }
                }
                return num;
            }
        }

        public Dictionary<string,int> CantidadTramosOptimizablesPorAvion
        {
            get
            {
                Dictionary<string, int> contador = new Dictionary<string, int>();                
                foreach (string key in _tramos_por_avion.Keys)
                {
                    contador.Add(key, 0);
                   
                    foreach (InfoTramoParaOptimizacion tramo in _tramos_por_avion[key])
                    {
                        if (tramo.TramoOptimizable)
                        {
                            contador[key]++;
                        }
                    }
                }
                return contador;
            }
        }

        public Itinerario ItinerarioBase { get; set; }

        public OrganizadorTramos(Itinerario itinerarioBase)
        {
            this.ItinerarioBase = itinerarioBase;
            _tramos = new Dictionary<int, InfoTramoParaOptimizacion>();
            _tramos_por_avion = new Dictionary<string, List<InfoTramoParaOptimizacion>>();
            CargarTramos();
        }

        private void CargarTramos()
        {
            foreach (Avion a in ItinerarioBase.AvionesDictionary.Values)
            {
                _tramos_por_avion.Add(a.IdAvion, new List<InfoTramoParaOptimizacion>());
                Tramo aux = a.Tramo_Raiz;
                InfoTramoParaOptimizacion tramo_previo = null ;
                while (aux != null)
                {
                    InfoTramoParaOptimizacion tramo_new = new InfoTramoParaOptimizacion(aux, tramo_previo);
                    tramo_previo = tramo_new;
                    _tramos.Add(tramo_new.IdTramo, tramo_new);
                    _tramos_por_avion[a.IdAvion].Add(tramo_new);
                    aux = aux.Tramo_Siguiente;
                }
            }
        }

        internal Dictionary<int, ExplicacionImpuntualidad> EstimarImpuntualidades(List<Simulacion> replicasBase, DateTime fechaIni, DateTime fechaFin, int std)
        {
            Dictionary<int, ExplicacionImpuntualidad> impuntualidades_tramo = new Dictionary<int, ExplicacionImpuntualidad>();
            Dictionary<int, Dictionary<TipoDisrupcion, double[]>> contadorImpuntualidad = new Dictionary<int, Dictionary<TipoDisrupcion, double[]>>();
            Dictionary<int, Dictionary<TipoDisrupcion, double[]>> sumaAtrasosTramo = new Dictionary<int, Dictionary<TipoDisrupcion, double[]>>();
            foreach (Simulacion simReplica in replicasBase)
            {  
                //Cuenta tramos de impuntualidad para cada estándar y tipo de disrupción                
                foreach (Avion a in simReplica.Itinerario.AvionesDictionary.Values)
                {
                    List<Tramo> listaTramos = a.ObtenerListaTramos(a.Tramo_Raiz);
                    foreach (Tramo tramo in listaTramos)
                    {
                        if (tramo.TramoBase.Fecha_Salida >= fechaIni && tramo.TramoBase.Fecha_Salida <= fechaFin)
                        {
                            int id = tramo.TramoBase.Numero_Global;
                            if (!sumaAtrasosTramo.ContainsKey(id))
                            {
                                contadorImpuntualidad.Add(id, new Dictionary<TipoDisrupcion, double[]>());
                                sumaAtrasosTramo.Add(id, new Dictionary<TipoDisrupcion, double[]>());
                                foreach (TipoDisrupcion tipo in Enum.GetValues(typeof(TipoDisrupcion)))
                                {
                                    if (tipo != TipoDisrupcion.ADELANTO)
                                    {
                                        contadorImpuntualidad[id].Add(tipo, new double[2]);
                                        sumaAtrasosTramo[id].Add(tipo, new double[2]);
                                    }
                                }   
                            }
                            int atraso = tramo.TInicialRst - tramo.TInicialProg;
                            foreach (TipoDisrupcion tipo in Enum.GetValues(typeof(TipoDisrupcion)))
                            {
                                if (tipo != TipoDisrupcion.ADELANTO)
                                {
                                    if (atraso > std && tramo.CausasAtraso.ContainsKey(tipo))
                                    {
                                        contadorImpuntualidad[id][tipo][0] += tramo.CausasAtraso[tipo] / ((double)(atraso));
                                        sumaAtrasosTramo[id][tipo][0] += tramo.CausasAtraso[tipo];
                                    }
                                    contadorImpuntualidad[id][tipo][1]++;
                                    sumaAtrasosTramo[id][tipo][1]++;
                                }
                            }               
                        }
                    }
                }
            }

            foreach (int id_tramo in contadorImpuntualidad.Keys)
            {
                ExplicacionImpuntualidad explicacionImpuntualidad = new ExplicacionImpuntualidad();
                foreach (TipoDisrupcion tipo in Enum.GetValues(typeof(TipoDisrupcion)))
                {
                    if (tipo != TipoDisrupcion.ADELANTO)
                    {
                        double impuntualidad_promedio = contadorImpuntualidad[id_tramo][tipo][0] / contadorImpuntualidad[id_tramo][tipo][1];
                        double atraso_promedio =  sumaAtrasosTramo[id_tramo][tipo][0] / sumaAtrasosTramo[id_tramo][tipo][1];
                        explicacionImpuntualidad.AgregarDisrupcion(tipo, impuntualidad_promedio, atraso_promedio );
                    }
                }
                impuntualidades_tramo.Add(id_tramo, explicacionImpuntualidad);                
            }
            return impuntualidades_tramo;
        }

        internal void OrdernarTramoSegunReaccionarios()
        {
            foreach (string avion in _tramos_por_avion.Keys)
            {
                _tramos_por_avion[avion].Sort(new Comparison<InfoTramoParaOptimizacion>(CompararTramosSegunReaccionario));
            }
        }

        internal void OrdernarTramoSegunPosicion()
        {
            foreach (string avion in _tramos_por_avion.Keys)
            {
                _tramos_por_avion[avion].Sort(new Comparison<InfoTramoParaOptimizacion>(CompararTramosSegunPosicion));
            }
        }

        internal int CompararTramosSegunReaccionario(InfoTramoParaOptimizacion t1, InfoTramoParaOptimizacion t2)
        {
            if (t1.ComparadorPrioridadOptimizacion > t2.ComparadorPrioridadOptimizacion)
            {
                return 1;
            }
            else if (t1.ComparadorPrioridadOptimizacion < t2.ComparadorPrioridadOptimizacion)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        internal int CompararTramosSegunPosicion(InfoTramoParaOptimizacion t1, InfoTramoParaOptimizacion t2)
        {
            if (t1.IdTramo < t2.IdTramo)
            {
                return -1;
            }
            else if (t1.IdTramo > t2.IdTramo)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        internal Itinerario GenerarNuevoItinerarioConCambios(int semilla)
        {
            Itinerario clonadoBase = ItinerarioBase.Clonar(semilla);
            clonadoBase.AplicarCambiosEnTiemposProgramados(_tramos_por_avion);

            return clonadoBase;            
        }

        internal void CargarImpuntualidadesBase(Dictionary<int, ExplicacionImpuntualidad> impuntualidades)
        {
            foreach (int tramo in impuntualidades.Keys)
            {
                _tramos[tramo].ExplicacionImpuntualidadBase = impuntualidades[tramo];
                _tramos[tramo].ExplicacionImpuntualidadActual = impuntualidades[tramo];
            }
        }

        internal void CargarImpuntualidadesIteraciones(Dictionary<int, ExplicacionImpuntualidad> impuntualidades)
        {
            foreach (int tramo in impuntualidades.Keys)
            {
                _tramos[tramo].ExplicacionImpuntualidadPrevia = _tramos[tramo].ExplicacionImpuntualidadActual;
                _tramos[tramo].ExplicacionImpuntualidadActual = impuntualidades[tramo];
            }
        }

        //internal void OptimizarVariacionesReaccionarios(out int variaciones, out int cambios_deshechos, out int tramos_cerrados)
        //{
        //    //Ordena tramo en avión según atraso reaccionario
        //    int salto_variaciones = 10;
        //    double porcentaje_tolerancia = 0.1;
        //    variaciones = 0;
        //    cambios_deshechos = 0;
        //    tramos_cerrados = 0;
        //    this.OrdernarTramoSegunReaccionarios();
        //    foreach (string avion in this.TramosPorAvion.Keys)
        //    {

        //        foreach (InfoTramoParaOptimizacion infoTramo in this.TramosPorAvion[avion])
        //        {
        //            if (infoTramo.Optimizable)
        //            {
        //                bool convieneOptimizar = infoTramo.ConvieneOptimizar();
        //                bool aplicaExitoso = false;
        //                if (convieneOptimizar)
        //                {
        //                    //Genera variaciones en tramos
        //                    if (infoTramo.ExplicacionImpuntualidadActual.RazonImpuntualidadReaccionarios < 0.5)
        //                    {
        //                        aplicaExitoso = infoTramo.IncrementarVariacionMenosAplicada(salto_variaciones);                               
        //                    }
        //                    else if(infoTramo.ExplicacionImpuntualidadActual.RazonImpuntualidadReaccionarios >1 - porcentaje_tolerancia && infoTramo.ExplicacionImpuntualidadActual.AtrasoReaccionarios>20)
        //                    {
        //                        aplicaExitoso = infoTramo.IncrementarVariacionMasAplicada(salto_variaciones);                                
        //                    }
        //                    if (aplicaExitoso)
        //                    {
        //                        variaciones++;
        //                    }
        //                }
        //                else if(!aplicaExitoso)
        //                {
        //                    cambios_deshechos++;
        //                    tramos_cerrados++;
        //                    infoTramo.IncrementarVariacionMasAplicada(-salto_variaciones);
        //                    infoTramo.IncrementarVariacionMenosAplicada(-salto_variaciones);
        //                    infoTramo.TramoAbierto = false;
        //                }
        //            }
        //        }
        //    }
        //}

        public int ObtenerMinimaVariacionDelanteTramo(InfoTramoParaOptimizacion info_tramo)
        {
            if (info_tramo.TramoSiguiente != null)
            {
                Dictionary<int, int> holguras_tramos_posteriores = info_tramo.TramoOriginal.HolgurasDelanteParaCadaConexion;
                int ta_destino = info_tramo.TramoOriginal.TurnAroundMinimoDestino;
                int minimo = int.MaxValue;
                foreach (int id_tramo_destino in holguras_tramos_posteriores.Keys)
                {
                    int aux_minima_variacion = holguras_tramos_posteriores[id_tramo_destino] - ta_destino + _tramos[id_tramo_destino].VariacionAplicada;
                    if (aux_minima_variacion < minimo)
                    {
                        minimo = aux_minima_variacion;
                    }
                }
                return minimo;
            }
            else
            {
                return 0;
            }
        }

        public int ObtenerMinimaVariacionAtrasTramo(InfoTramoParaOptimizacion info_tramo)
        {
            if (info_tramo.TramoPrevio != null)
            {
                Dictionary<int, int> holguras_tramos_anteriores = info_tramo.TramoOriginal.HolgurasAtrasParaCadaConexion;
                int ta_origen = info_tramo.TramoOriginal.TurnAroundMinimoOrigen;
                int minimo = int.MaxValue;
                foreach (int id_tramo_destino in holguras_tramos_anteriores.Keys)
                {
                    int aux_minima_variacion = holguras_tramos_anteriores[id_tramo_destino] - ta_origen - _tramos[id_tramo_destino].VariacionAplicada;
                    if (aux_minima_variacion < minimo)
                    {
                        minimo = aux_minima_variacion;
                    }
                }
                return minimo;
            }
            else
            {
                return 0;
            }
        }

        internal void OptimizarCurvasAtrasoPropagado(out int variaciones,int salto_variaciones)
        {
            double ganancia_total = 0;
            variaciones = 0;
            //Ordenar tramo (si no se recorren de manera cronológica
            this.OrdernarTramoSegunPosicion();
            foreach (string avion in this.TramosPorAvion.Keys)
            {
                Dictionary<int, bool> infoTramoOptimizado = new Dictionary<int, bool>();
                int contador = 0;
                foreach (InfoTramoParaOptimizacion infoTramo in this.TramosPorAvion[avion])
                {
                    infoTramoOptimizado.Add(contador, !infoTramo.Optimizable);
                    contador++;
                }
                int cantidad_tramos_optimizados = 0;
                while (cantidad_tramos_optimizados < this.TramosPorAvion[avion].Count)
                {
                    Dictionary<int, double> infoTramoDisminucionAtraso = new Dictionary<int, double>();
                    Dictionary<int, double> infoTramoVariacionPropuesta = new Dictionary<int, double>();
                    int index = 0;
                    foreach (InfoTramoParaOptimizacion infoTramo in this.TramosPorAvion[avion])
                    {
                        if (!infoTramoOptimizado[index])
                        {
                            //Obtener rangos posibles de variación de la curva
                            int minimaVariacionDelante = ObtenerMinimaVariacionDelanteTramo(infoTramo);
                            int minimaVariacionAtras = ObtenerMinimaVariacionAtrasTramo(infoTramo);
                            int rangoMenos = Math.Min(infoTramo.VariacionMenosMaximaComercial, minimaVariacionAtras);
                            rangoMenos = rangoMenos - rangoMenos % salto_variaciones;
                            int rangoMas = Math.Min(infoTramo.VariacionMasMaximaComercial, minimaVariacionDelante);
                            rangoMas = rangoMas - rangoMas % salto_variaciones;
                            if (rangoMas<0 || rangoMenos < 0)
                            {

                            }
                            if (rangoMas > 0 || rangoMenos > 0)
                            {
                                Dictionary<double, double> curva_atrasos_propagados_globales = new Dictionary<double, double>();
                                for (int i = -rangoMenos; i <= rangoMas; i = i + salto_variaciones)
                                {
                                    double atraso_previo = infoTramo.AtrasoTramoPrevio;
                                    double atraso_propagado = infoTramo.EstimarAtrasoPropagadoAvion(atraso_previo, i);
                                    curva_atrasos_propagados_globales.Add(i, atraso_propagado);
                                }
                                Curva c = new Curva(curva_atrasos_propagados_globales, -rangoMenos, rangoMas, salto_variaciones);
                                infoTramoDisminucionAtraso.Add(index, c.DiferenciaValorOptimoConValorEnCero);
                                infoTramoVariacionPropuesta.Add(index, c.PuntoOptimo);
                                if (c.PuntoMasCercanoCero != 0 && c.DiferenciaValorOptimoConValorEnCero <= 0)
                                {
                                    infoTramo.VariacionAplicada = Convert.ToInt32(c.PuntoMasCercanoCero);
                                    infoTramoOptimizado[index] = true;
                                }
                            }
                            else
                            {
                                infoTramoDisminucionAtraso.Add(index, 0);
                                infoTramoVariacionPropuesta.Add(index, 0);
                            }
                        }
                        index++;
                    }
                    int index_optimo = 0;
                    double ganancia_maxima = double.MinValue;
                    foreach (int index_tramo in infoTramoVariacionPropuesta.Keys)
                    {
                        if (infoTramoDisminucionAtraso[index_tramo] > ganancia_maxima)
                        {
                            index_optimo = index_tramo;
                            ganancia_maxima =infoTramoDisminucionAtraso[index_tramo];
                        }
                    }
                    if (ganancia_maxima > 0)
                    {
                        int variacion_aplicada_final = Convert.ToInt32(infoTramoVariacionPropuesta[index_optimo]);
                        this.TramosPorAvion[avion][index_optimo].VariacionAplicada = variacion_aplicada_final;
                        ganancia_total += ganancia_maxima;
                        if (variacion_aplicada_final != 0)
                        {
                            variaciones++;
                        }
                        else
                        {

                        }
                        infoTramoOptimizado[index_optimo] = true;
                        cantidad_tramos_optimizados = 0;
                        foreach (int i in infoTramoOptimizado.Keys)
                        {
                            if (infoTramoOptimizado[i])
                            {
                                cantidad_tramos_optimizados++;
                            }
                        }

                    }
                    else
                    {
                        
                        cantidad_tramos_optimizados = this.TramosPorAvion[avion].Count;
                    }                  
                 }
            }


        }

        internal void DeshacerCambiosQueEmpeoranPuntualidad(out int cambios, Dictionary<int, Dictionary<int, ExplicacionImpuntualidad>> historial,Dictionary<int,int> variacion_penultima, bool revisaPrimero)
        {
            //definir de manera flexible si hacer en análisis respecto a situación inicial o la inmediatamente anterior.
            cambios = 0;
            int index_ultimo= historial.Count;
            int index_penultimo = historial.Count-1;
            int index_ini = revisaPrimero ? 1 : historial.Count - 1;
            //Estudiar forma de deshacer: mirando a tramo previo o no.
            foreach (string id_avion in _tramos_por_avion.Keys)
            {
                foreach (InfoTramoParaOptimizacion tramo in _tramos_por_avion[id_avion])
                {
                    int id_tramo = tramo.IdTramo;
                    double impuntualidad_ultima = historial[index_ultimo][id_tramo].ImpuntualidadTotal;
                    double impuntualidad_penultima = historial[index_penultimo][id_tramo].ImpuntualidadTotal;
                    double impuntualidad_inicial = historial[index_ini][id_tramo].ImpuntualidadTotal;
                    InfoTramoParaOptimizacion tramo_siguiente = tramo.TramoSiguiente;
                    while (tramo_siguiente!=null && tramo_siguiente.VariacionAplicada == 0)
                    {
                        impuntualidad_ultima += historial[index_ultimo][tramo_siguiente.IdTramo].ImpuntualidadTotal;
                        impuntualidad_penultima += historial[index_penultimo][tramo_siguiente.IdTramo].ImpuntualidadTotal;
                        impuntualidad_inicial += historial[index_ini][tramo_siguiente.IdTramo].ImpuntualidadTotal;
                        tramo_siguiente = tramo_siguiente.TramoSiguiente;
                    }
                    //Opcion: agregar rango de tolerancia
                    if (impuntualidad_penultima >= impuntualidad_inicial && impuntualidad_ultima > impuntualidad_inicial)
                    {
                        if (tramo.VariacionAplicada > 0)
                        {
                            cambios++;
                        }
                        tramo.VariacionAplicada = 0;

                    }
                    else if (impuntualidad_penultima <= impuntualidad_inicial && impuntualidad_penultima < impuntualidad_ultima)
                    {
                        if (tramo.VariacionAplicada > 0)
                        {
                            cambios++;
                        }
                        tramo.VariacionAplicada = variacion_penultima[id_tramo];
                    }
                }                
            }
        }

        internal void VolverAEstadoDeMejorPuntualidad(Dictionary<int, Dictionary<int, int>> historial_variaciones_1, Dictionary<int, Dictionary<int, ExplicacionImpuntualidad>> historial_puntualidad_1, Dictionary<int, Dictionary<int, int>> historial_variaciones_2, Dictionary<int, Dictionary<int, ExplicacionImpuntualidad>> historial_puntualidad_2)
        {
            //definir de manera flexible si hacer en análisis respecto a situación inicial o la inmediatamente anterior.
            
            //Estudiar forma de deshacer: mirando a tramo previo o no.
            foreach (string id_avion in _tramos_por_avion.Keys)
            {
                double impuntualidad_minima_1, impuntualidad_minima_2;
                int iteracion_mejor_puntualidad_1 = ObtenerIteracionDeMejorPuntualidad(historial_puntualidad_1, id_avion,out impuntualidad_minima_1);
                int iteracion_mejor_puntualidad_2 = ObtenerIteracionDeMejorPuntualidad(historial_puntualidad_2, id_avion, out impuntualidad_minima_2);
                if (impuntualidad_minima_1 < impuntualidad_minima_2)
                {
                    foreach (InfoTramoParaOptimizacion tramo in _tramos_por_avion[id_avion])
                    {
                        int id_tramo = tramo.IdTramo;
                        if (historial_variaciones_1.ContainsKey(iteracion_mejor_puntualidad_1))
                        {
                            tramo.VariacionAplicada = historial_variaciones_1[iteracion_mejor_puntualidad_1][id_tramo];
                        }
                        else
                        {
                            tramo.VariacionAplicada = 0;
                        }
                    }
                }
                else
                {
                    foreach (InfoTramoParaOptimizacion tramo in _tramos_por_avion[id_avion])
                    {
                        int id_tramo = tramo.IdTramo;
                        if (historial_variaciones_2.ContainsKey(iteracion_mejor_puntualidad_2))
                        {
                            tramo.VariacionAplicada = historial_variaciones_2[iteracion_mejor_puntualidad_2][id_tramo];
                        }
                        else
                        {
                            tramo.VariacionAplicada = 0;
                        }
                    }
                }
            }
        }

        private int ObtenerIteracionDeMejorPuntualidad(Dictionary<int, Dictionary<int, ExplicacionImpuntualidad>> historial_puntualidad, string id_avion, out double impuntualidad_minima)
        {
            Dictionary<int, double> impuntualidad_acumulada = new Dictionary<int, double>();
            int total_tramos = this._tramos_por_avion[id_avion].Count;
            foreach (int i in historial_puntualidad.Keys)
            {
                impuntualidad_acumulada.Add(i, 0);
                foreach (InfoTramoParaOptimizacion tramo in this._tramos_por_avion[id_avion])
                {
                    impuntualidad_acumulada[i]+= historial_puntualidad[i][tramo.IdTramo].ImpuntualidadTotal;
                }
                impuntualidad_acumulada[i] /= total_tramos;
            }
            double minimo = double.MaxValue;
            int index_min = -1;
            foreach (int index in impuntualidad_acumulada.Keys)
            {
                if (impuntualidad_acumulada[index] < minimo)
                {
                    minimo = impuntualidad_acumulada[index];
                    index_min = index;
                }
            }
            impuntualidad_minima = minimo;
            return index_min;            
        }

        internal Dictionary<int, int> ObtenerVariacionesPropuestas()
        {
            Dictionary<int, int> variaciones = new Dictionary<int, int>();
            foreach (string id_avion in _tramos_por_avion.Keys)
            {
                foreach (InfoTramoParaOptimizacion tramo in _tramos_por_avion[id_avion])
                {
                    variaciones.Add(tramo.IdTramo, tramo.VariacionAplicada);
                }
            }
            return variaciones;
        }
    }
}
