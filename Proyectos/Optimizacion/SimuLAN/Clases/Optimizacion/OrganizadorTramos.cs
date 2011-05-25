using System;
using System.Collections.Generic;
using SimuLAN.Utils;

namespace SimuLAN.Clases.Optimizacion
{
    public delegate List<InfoTramoParaOptimizacion> BuscarTramosConectadosEventHandler(SerializableList<ConexionLegs> conexiones_posteriores, bool busca_conexiones_posteriores);

    public class OrganizadorTramos
    {
        private Dictionary<int, InfoTramoParaOptimizacion> _tramos;
        private Dictionary<string, List<InfoTramoParaOptimizacion>> _tramos_por_avion;
        private int _variacion_permitida;
        private BuscarTramosConectadosEventHandler _buscar_tramos;

        public Dictionary<string, List<InfoTramoParaOptimizacion>> TramosPorAvion
        {
            get { return _tramos_por_avion; }
        }

        public Itinerario ItinerarioBase { get; set; }
        
        public OrganizadorTramos(Itinerario itinerarioBase, int variacion_permitida)
        {
            this.ItinerarioBase = itinerarioBase;
            this._variacion_permitida = variacion_permitida;
            _tramos = new Dictionary<int, InfoTramoParaOptimizacion>();
            _tramos_por_avion = new Dictionary<string, List<InfoTramoParaOptimizacion>>();
            this._buscar_tramos = new BuscarTramosConectadosEventHandler(BuscarTramos);
            CargarTramos();
        }

        public List<InfoTramoParaOptimizacion> BuscarTramos(SerializableList<ConexionLegs> conexiones, bool busca_conexiones_posteriores)
        {
            List<InfoTramoParaOptimizacion> tramos_encontrados = new List<InfoTramoParaOptimizacion>();
            if (conexiones != null && conexiones.Count > 0)
            {
                foreach (ConexionLegs conexion in conexiones)
                {
                    int numero_conexion = 0;
                    if (busca_conexiones_posteriores)
                    {
                        numero_conexion = conexion.NumTramoFin;
                    }
                    else
                    {
                        numero_conexion = conexion.NumTramoIni;
                    }
                    Tramo tramo_siguiente = conexion.GetTramo(numero_conexion);
                    bool tramo_encontrado = false;
                    foreach (List<InfoTramoParaOptimizacion> info_tramos in _tramos_por_avion.Values)
                    {
                        foreach (InfoTramoParaOptimizacion info_tramo in info_tramos)
                        {
                            if (tramo_siguiente.TramoBase.Numero_Global == info_tramo.TramoOriginal.TramoBase.Numero_Global)
                            {
                                tramos_encontrados.Add(info_tramo);
                                tramo_encontrado = true;
                                continue;
                            }
                        }
                        if (tramo_encontrado)
                        {
                            continue;
                        }
                    }
                }
            }
            return tramos_encontrados;
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
                    InfoTramoParaOptimizacion tramo_new = new InfoTramoParaOptimizacion(aux, tramo_previo, _variacion_permitida, _buscar_tramos);
                    tramo_previo = tramo_new;
                    _tramos.Add(tramo_new.IdTramo, tramo_new);
                    _tramos_por_avion[a.IdAvion].Add(tramo_new);
                    aux = aux.Tramo_Siguiente;
                }
            }
        }

        internal Dictionary<int, ExplicacionImpuntualidad> EstimarImpuntualidades(List<Simulacion> replicasBase, DateTime fechaIni, DateTime fechaFin, List<int> stds)
        {
            Dictionary<int, ExplicacionImpuntualidad> impuntualidades_tramo = new Dictionary<int, ExplicacionImpuntualidad>();
            Dictionary<int, Dictionary<int, Dictionary<TipoDisrupcion, double[]>>> contadorImpuntualidad = new Dictionary<int, Dictionary<int, Dictionary<TipoDisrupcion, double[]>>>();
            Dictionary<int, Dictionary<TipoDisrupcion, double[]>> sumaAtrasosTramo = new Dictionary<int, Dictionary<TipoDisrupcion, double[]>>();
            List<int> keys_tramos = new List<int>();
            foreach (int std in stds)
            {                
                contadorImpuntualidad.Add(std, new Dictionary<int, Dictionary<TipoDisrupcion, double[]>>());
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
                                if (!contadorImpuntualidad[std].ContainsKey(id))
                                {
                                    contadorImpuntualidad[std].Add(id, new Dictionary<TipoDisrupcion, double[]>());
                                    foreach (TipoDisrupcion tipo in Enum.GetValues(typeof(TipoDisrupcion)))
                                    {
                                        if (tipo != TipoDisrupcion.ADELANTO)
                                        {
                                            contadorImpuntualidad[std][id].Add(tipo, new double[2]);
                                        }
                                    }
                                }
                                int atraso = tramo.TInicialRst - tramo.TInicialProg;
                                foreach (TipoDisrupcion tipo in Enum.GetValues(typeof(TipoDisrupcion)))
                                {
                                    if (tipo != TipoDisrupcion.ADELANTO)
                                    {
                                        if (tramo.CausasAtraso.ContainsKey(tipo))
                                        {
                                            if (atraso > std)
                                            {
                                                contadorImpuntualidad[std][id][tipo][0] += tramo.CausasAtraso[tipo] / ((double)(atraso));
                                            }
                                        }
                                        contadorImpuntualidad[std][id][tipo][1]++;
                                    }
                                }
                            }
                        }
                    }
                }
            }

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
                            if (!keys_tramos.Contains(id))
                            {
                                keys_tramos.Add(id);
                            }
                            if (!sumaAtrasosTramo.ContainsKey(id))
                            {                                
                                sumaAtrasosTramo.Add(id, new Dictionary<TipoDisrupcion, double[]>());
                                foreach (TipoDisrupcion tipo in Enum.GetValues(typeof(TipoDisrupcion)))
                                {
                                    if (tipo != TipoDisrupcion.ADELANTO)
                                    {                                        
                                        sumaAtrasosTramo[id].Add(tipo, new double[2]);
                                    }
                                }
                            }
                            int atraso = tramo.TInicialRst - tramo.TInicialProg;
                            foreach (TipoDisrupcion tipo in Enum.GetValues(typeof(TipoDisrupcion)))
                            {
                                if (tipo != TipoDisrupcion.ADELANTO)
                                {
                                    if (tramo.CausasAtraso.ContainsKey(tipo))
                                    {
                                        sumaAtrasosTramo[id][tipo][0] += tramo.CausasAtraso[tipo];
                                    }                                    
                                    sumaAtrasosTramo[id][tipo][1]++;
                                }
                            }
                        }
                    }
                }
            }
            
            foreach (int id_tramo in keys_tramos)
            {                
                ExplicacionImpuntualidad explicacionImpuntualidad = new ExplicacionImpuntualidad();
                impuntualidades_tramo.Add(id_tramo, explicacionImpuntualidad);
                foreach (int std in contadorImpuntualidad.Keys)
                {
                    Dictionary<TipoDisrupcion, double> impuntualidad_tramo = new Dictionary<TipoDisrupcion, double>();
                    foreach (TipoDisrupcion tipo in Enum.GetValues(typeof(TipoDisrupcion)))
                    {
                        if (tipo != TipoDisrupcion.ADELANTO)
                        {
                            double impuntualidad_promedio = contadorImpuntualidad[std][id_tramo][tipo][0] / contadorImpuntualidad[std][id_tramo][tipo][1];                            
                            impuntualidad_tramo.Add(tipo, impuntualidad_promedio);

                        }
                    }
                    explicacionImpuntualidad.AgregarDisrupcion(std, impuntualidad_tramo);
                }
                foreach (TipoDisrupcion tipo in Enum.GetValues(typeof(TipoDisrupcion)))
                {
                    if (tipo != TipoDisrupcion.ADELANTO)
                    {                        
                        double atraso_promedio = sumaAtrasosTramo[id_tramo][tipo][0] / sumaAtrasosTramo[id_tramo][tipo][1];
                        explicacionImpuntualidad.AgregarAtraso(tipo, atraso_promedio);
                    }
                }
            }
            
            return impuntualidades_tramo;
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
        private enum TipoPriorizacion { Lista, Atraso, Puntualidad, Random }
        internal void OptimizarCurvasAtrasoPropagado(int salto_variaciones)
        {
            double ganancia_total = 0;
            List<string> lista_priorizada_aviones = PriorizarListaAviones(TipoPriorizacion.Random);
            foreach (string avion in lista_priorizada_aviones)
            {
                Dictionary<int, bool> infoTramoOptimizado = new Dictionary<int, bool>();
                int contador = 0;
                foreach (InfoTramoParaOptimizacion infoTramo in this.TramosPorAvion[avion])
                {
                    infoTramoOptimizado.Add(contador, !infoTramo.Optimizable(0));
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
                                    double atraso_propagado = infoTramo.EstimarAtrasoArbolPropagacion(atraso_previo, i, 10);
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

        private List<string> PriorizarListaAviones(TipoPriorizacion tipoPriorizacion)
        {
            List<string> lista_aviones = new List<string>();
            List<string> lista_aviones_priorizada = new List<string>();
            foreach (string avion in _tramos_por_avion.Keys)
            {
                lista_aviones.Add(avion);
            }
            if (tipoPriorizacion == TipoPriorizacion.Lista)
            {
                foreach (string avion in lista_aviones)
                {
                    lista_aviones_priorizada.Add(avion);
                }
            }
            else if (tipoPriorizacion == TipoPriorizacion.Random)
            {
                Random r = new Random(DateTime.Now.Millisecond);
                List<int> indices_agregados = new List<int>();
               
                int total_aviones = lista_aviones.Count;
                while (lista_aviones.Count > indices_agregados.Count)
                {
                    int indice = r.Next(0, total_aviones);
                    if (!indices_agregados.Contains(indice))
                    {
                        indices_agregados.Add(indice);
                    }
                }
                foreach (int indice_random in indices_agregados)
                {
                    lista_aviones_priorizada.Add(lista_aviones[indice_random]);
                }
            }
            else if (tipoPriorizacion == TipoPriorizacion.Atraso)
            {
                List<KeyValuePair<string,double>> atraso_por_avion = new List<KeyValuePair<string,double>>();
                foreach (string avion in lista_aviones)
                {
                    double atraso = 0;
                    foreach (InfoTramoParaOptimizacion tramo in _tramos_por_avion[avion])
                    {
                        atraso += tramo.ExplicacionImpuntualidadActual.AtrasoTotal;
                    }
                    atraso_por_avion.Add(new KeyValuePair<string, double>(avion, atraso));
                }
                atraso_por_avion.Sort(new Comparison<KeyValuePair<string, double>>(CompararAtrasoEntreAviones));
                foreach (KeyValuePair<string, double> kv in atraso_por_avion)
                {
                    lista_aviones_priorizada.Add(kv.Key);
                }
            }
            else if (tipoPriorizacion == TipoPriorizacion.Puntualidad)
            {
                List<KeyValuePair<string, double>> impuntualidad_por_avion = new List<KeyValuePair<string, double>>();
                foreach (string avion in lista_aviones)
                {
                    double impuntualidad = 0;
                    int tramos_avion = _tramos_por_avion[avion].Count;
                    foreach (InfoTramoParaOptimizacion tramo in _tramos_por_avion[avion])
                    {
                        impuntualidad += tramo.ExplicacionImpuntualidadActual.ImpuntualidadTotal[0];
                    }
                    if(tramos_avion>0)
                    {
                        impuntualidad /= tramos_avion;
                    }
                    impuntualidad_por_avion.Add(new KeyValuePair<string, double>(avion, impuntualidad));
                }
                impuntualidad_por_avion.Sort(new Comparison<KeyValuePair<string, double>>(CompararAtrasoEntreAviones));
                foreach (KeyValuePair<string, double> kv in impuntualidad_por_avion)
                {
                    lista_aviones_priorizada.Add(kv.Key);
                }
            }
            return lista_aviones_priorizada;
        }
        internal int CompararAtrasoEntreAviones(KeyValuePair<string, double> o1, KeyValuePair<string, double> o2)
        {
            if (o1.Value < o2.Value)
            {
                return 1;
            }
            else if (o1.Value > o2.Value)
            {
                return -1;
            }
            else return 0;

        }
        internal void DeshacerCambiosQueEmpeoranPuntualidad(out int cambios, Dictionary<int, Dictionary<int, ExplicacionImpuntualidad>> historial,Dictionary<int,int> variacion_penultima, bool revisaPrimero,int std)
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
                    double impuntualidad_ultima = historial[index_ultimo][id_tramo].ImpuntualidadTotal[std];
                    double impuntualidad_penultima = historial[index_penultimo][id_tramo].ImpuntualidadTotal[std];
                    double impuntualidad_inicial = historial[index_ini][id_tramo].ImpuntualidadTotal[std];
                    InfoTramoParaOptimizacion tramo_siguiente = tramo.TramoSiguiente;
                    while (tramo_siguiente!=null && tramo_siguiente.VariacionAplicada == 0)
                    {
                        impuntualidad_ultima += historial[index_ultimo][tramo_siguiente.IdTramo].ImpuntualidadTotal[std];
                        impuntualidad_penultima += historial[index_penultimo][tramo_siguiente.IdTramo].ImpuntualidadTotal[std];
                        impuntualidad_inicial += historial[index_ini][tramo_siguiente.IdTramo].ImpuntualidadTotal[std];
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

        internal void DeshacerCambiosQueEmpeoranAtrasoPropagado(out int cambios, Dictionary<int, Dictionary<int, ExplicacionImpuntualidad>> historial, Dictionary<int, int> variacion_penultima, bool revisaPrimero)
        {
            //definir de manera flexible si hacer en análisis respecto a situación inicial o la inmediatamente anterior.
            cambios = 0;
            int index_ultimo = historial.Count;
            int index_penultimo = historial.Count - 1;
            int index_ini = revisaPrimero ? 1 : historial.Count - 1;
            //Estudiar forma de deshacer: mirando a tramo previo o no.
            foreach (string id_avion in _tramos_por_avion.Keys)
            {
                foreach (InfoTramoParaOptimizacion tramo in _tramos_por_avion[id_avion])
                {
                    int id_tramo = tramo.IdTramo;
                    double atraso_ultimo = historial[index_ultimo][id_tramo].AtrasoTotal;
                    double atraso_penultimo = historial[index_penultimo][id_tramo].AtrasoTotal;
                    double atraso_inicial = historial[index_ini][id_tramo].AtrasoTotal;
                    InfoTramoParaOptimizacion tramo_siguiente = tramo.TramoSiguiente;
                    while (tramo_siguiente != null && tramo_siguiente.VariacionAplicada == 0)
                    {
                        atraso_ultimo += historial[index_ultimo][tramo_siguiente.IdTramo].AtrasoTotal;
                        atraso_penultimo += historial[index_penultimo][tramo_siguiente.IdTramo].AtrasoTotal;
                        atraso_inicial += historial[index_ini][tramo_siguiente.IdTramo].AtrasoTotal;
                        tramo_siguiente = tramo_siguiente.TramoSiguiente;
                    }
                    //Opcion: agregar rango de tolerancia
                    if (atraso_penultimo >= atraso_inicial && atraso_ultimo > atraso_inicial)
                    {
                        if (tramo.VariacionAplicada > 0)
                        {
                            cambios++;
                        }
                        tramo.VariacionAplicada = 0;

                    }
                    else if (atraso_penultimo <= atraso_inicial && atraso_penultimo < atraso_ultimo)
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

        internal void VolverAEstadoDeMejorPuntualidad(Dictionary<int, Dictionary<int, int>> historial_variaciones_1, Dictionary<int, Dictionary<int, ExplicacionImpuntualidad>> historial_puntualidad_1, Dictionary<int, Dictionary<int, int>> historial_variaciones_2, Dictionary<int, Dictionary<int, ExplicacionImpuntualidad>> historial_puntualidad_2,int std_objetivo)
        {
            //definir de manera flexible si hacer en análisis respecto a situación inicial o la inmediatamente anterior.
            
            //Estudiar forma de deshacer: mirando a tramo previo o no.
            foreach (string id_avion in _tramos_por_avion.Keys)
            {
                double impuntualidad_minima_1, impuntualidad_minima_2;
                int iteracion_mejor_puntualidad_1 = ObtenerIteracionDeMejorPuntualidad(historial_puntualidad_1, id_avion, out impuntualidad_minima_1, std_objetivo);
                int iteracion_mejor_puntualidad_2 = ObtenerIteracionDeMejorPuntualidad(historial_puntualidad_2, id_avion, out impuntualidad_minima_2, std_objetivo);
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

        internal void VolverAEstadoDeMenorAtrasoPropagado(LogOptimizacion log_proceso)
        {          
            foreach (string id_avion in _tramos_por_avion.Keys)
            {
                FaseOptimizacion mejor_fase;
                int mejor_iteracion;
                ObtenerIteracionDeMenorAtraso(log_proceso.HistorialImpuntualidad, id_avion, out mejor_fase,out mejor_iteracion);
                foreach (InfoTramoParaOptimizacion tramo in _tramos_por_avion[id_avion])
                {
                    int id_tramo = tramo.IdTramo;
                    if (log_proceso.HistorialVariaciones.ContainsKey(mejor_fase) && log_proceso.HistorialVariaciones[mejor_fase].ContainsKey(mejor_iteracion))
                    {
                        tramo.VariacionAplicada = log_proceso.HistorialVariaciones[mejor_fase][mejor_iteracion][id_tramo];
                    }
                    else
                    {
                        tramo.VariacionAplicada = 0;
                    }
                }                
            }
        }

        private void ObtenerIteracionDeMenorAtraso(Dictionary<FaseOptimizacion, Dictionary<int, Dictionary<int, ExplicacionImpuntualidad>>> historial_puntualidad, string id_avion, out FaseOptimizacion mejor_fase, out int mejor_iteracion)
        {
            mejor_fase = FaseOptimizacion.Inicio;
            mejor_iteracion = -1;
            Dictionary<FaseOptimizacion, Dictionary<int, double>> atraso_acumulado = new Dictionary<FaseOptimizacion, Dictionary<int, double>>();
            int total_tramos = this._tramos_por_avion[id_avion].Count;
            foreach (FaseOptimizacion fase in historial_puntualidad.Keys)
            {
                atraso_acumulado.Add(fase, new Dictionary<int,double>());
                foreach (int i in historial_puntualidad[fase].Keys)
                {
                    atraso_acumulado[fase].Add(i, 0);
                    foreach (InfoTramoParaOptimizacion tramo in this._tramos_por_avion[id_avion])
                    {
                        atraso_acumulado[fase][i] += historial_puntualidad[fase][i][tramo.IdTramo].AtrasoTotal;
                    }
                    atraso_acumulado[fase][i] /= total_tramos;
                }
            }
            double minimo = double.MaxValue;
            foreach (FaseOptimizacion fase in historial_puntualidad.Keys)
            {
                foreach (int index in atraso_acumulado[fase].Keys)
                {
                    if (atraso_acumulado[fase][index] < minimo)
                    {
                        minimo = atraso_acumulado[fase][index];
                        mejor_iteracion = index;
                        mejor_fase = fase;
                    }
                }
            }
        }

        private int ObtenerIteracionDeMejorPuntualidad(Dictionary<int, Dictionary<int, ExplicacionImpuntualidad>> historial_puntualidad, string id_avion, out double impuntualidad_minima,int std_objetivo)
        {
            Dictionary<int, double> impuntualidad_acumulada = new Dictionary<int, double>();
            int total_tramos = this._tramos_por_avion[id_avion].Count;
            foreach (int i in historial_puntualidad.Keys)
            {
                impuntualidad_acumulada.Add(i, 0);
                foreach (InfoTramoParaOptimizacion tramo in this._tramos_por_avion[id_avion])
                {
                    impuntualidad_acumulada[i]+= historial_puntualidad[i][tramo.IdTramo].ImpuntualidadTotal[std_objetivo];
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

        private int ObtenerIteracionDeMenorAtraso(Dictionary<int, Dictionary<int, ExplicacionImpuntualidad>> historial_puntualidad, string id_avion, out double atraso_minimo)
        {
            Dictionary<int, double> atraso_acumulado = new Dictionary<int, double>();
            int total_tramos = this._tramos_por_avion[id_avion].Count;
            foreach (int i in historial_puntualidad.Keys)
            {
                atraso_acumulado.Add(i, 0);
                foreach (InfoTramoParaOptimizacion tramo in this._tramos_por_avion[id_avion])
                {
                    atraso_acumulado[i] += historial_puntualidad[i][tramo.IdTramo].AtrasoTotal;
                }
                atraso_acumulado[i] /= total_tramos;
            }
            double minimo = double.MaxValue;
            int index_min = -1;
            foreach (int index in atraso_acumulado.Keys)
            {
                if (atraso_acumulado[index] < minimo)
                {
                    minimo = atraso_acumulado[index];
                    index_min = index;
                }
            }
            atraso_minimo = minimo;
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
