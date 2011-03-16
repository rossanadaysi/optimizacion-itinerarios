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
                while (aux != null)
                {
                    int id = aux.TramoBase.Numero_Global;
                    InfoTramoParaOptimizacion tramo_new = new InfoTramoParaOptimizacion(id, aux.MinutosMaximaVariacionAtras, aux.MinutosMaximaVariacionDelante);
                    _tramos.Add(id,tramo_new);
                    _tramos_por_avion[a.IdAvion].Add(tramo_new);
                    aux = aux.Tramo_Siguiente;
                }
            }
        }

        internal Dictionary<int, ExplicacionImpuntualidad> EstimarImpuntualidades(List<Simulacion> replicasBase, DateTime fechaIni, DateTime fechaFin, int std)
        {
            Dictionary<int, ExplicacionImpuntualidad> impuntualidades_tramo = new Dictionary<int, ExplicacionImpuntualidad>();
            Dictionary<int, Dictionary<TipoDisrupcion, double[]>> contadorImpuntualidad = new Dictionary<int, Dictionary<TipoDisrupcion, double[]>>();
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
                            if (!contadorImpuntualidad.ContainsKey(id))
                            {
                                contadorImpuntualidad.Add(id, new Dictionary<TipoDisrupcion, double[]>());
                                foreach (TipoDisrupcion tipo in Enum.GetValues(typeof(TipoDisrupcion)))
                                {
                                    if (tipo != TipoDisrupcion.ADELANTO)
                                    {
                                        contadorImpuntualidad[id].Add(tipo, new double[2]);
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
                                    }
                                    contadorImpuntualidad[id][tipo][1]++;
                                }
                            }               
                        }
                    }
                }
            }

            foreach(int id_tramo in contadorImpuntualidad.Keys)
            {
                ExplicacionImpuntualidad explicacionImpuntualidad = new ExplicacionImpuntualidad();
                foreach (TipoDisrupcion tipo in Enum.GetValues(typeof(TipoDisrupcion)))
                {
                    if (tipo != TipoDisrupcion.ADELANTO)
                    {
                        explicacionImpuntualidad.AgregarDisrupcion(tipo, contadorImpuntualidad[id_tramo][tipo][0] / contadorImpuntualidad[id_tramo][tipo][1]);
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
            if (t1.ExplicacionImpuntualidadActual.ImpuntualidadReaccionarios > t2.ExplicacionImpuntualidadActual.ImpuntualidadReaccionarios)
            {
                return 1;
            }
            else if (t1.ExplicacionImpuntualidadActual.ImpuntualidadReaccionarios < t2.ExplicacionImpuntualidadActual.ImpuntualidadReaccionarios)
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
                return 1;
            }
            else if (t1.IdTramo > t2.IdTramo)
            {
                return -1;
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
            foreach (int id_tramo in impuntualidades.Keys)
            {
                _tramos[id_tramo].ExplicacionImpuntualidadBase = impuntualidades[id_tramo];
                _tramos[id_tramo].ExplicacionImpuntualidadActual = impuntualidades[id_tramo];
            }
        }

        internal void CargarImpuntualidadesIteraciones(Dictionary<int, ExplicacionImpuntualidad> impuntualidades)
        {
            foreach (int id_tramo in impuntualidades.Keys)
            {
                _tramos[id_tramo].ExplicacionImpuntualidadPrevia = _tramos[id_tramo].ExplicacionImpuntualidadActual;
                _tramos[id_tramo].ExplicacionImpuntualidadActual = impuntualidades[id_tramo];
            }
        }

        internal void OptimizarVariacionesReaccionarios()
        {
            //Ordena tramo en avión según atraso reaccionario
            int salto_variaciones = 5;
            this.OrdernarTramoSegunReaccionarios();
            foreach (string avion in this.TramosPorAvion.Keys)
            {

                foreach (InfoTramoParaOptimizacion infoTramo in this.TramosPorAvion[avion])
                {
                    if (infoTramo.ExplicacionImpuntualidadBase.TieneAtrasoReaccionario)
                    {
                        bool convieneOptimizar = infoTramo.ConvieneOptimizar();
                        if (convieneOptimizar)
                        {
                            //Genera variaciones en tramos
                            infoTramo.IncrementarVariacionMasAplicada(salto_variaciones);
                        }
                    }
                }
            }
        }
    }
}
