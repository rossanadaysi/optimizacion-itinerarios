using System;
using System.Collections.Generic;
using System.Text;
using SimuLAN.Clases.Disrupciones;
using System.IO;

namespace SimuLAN.Clases.Optimizacion
{
    public class Optimizador
    {
        private int _std;

        private DateTime _fecha_fin;

        private DateTime _fecha_ini;

        private Dictionary<int, Dictionary<int, ExplicacionImpuntualidad>> _historial_puntualidades_optimizacion;
        private Dictionary<int, Dictionary<int, ExplicacionImpuntualidad>> _historial_puntualidades_regresion;
        private Dictionary<int, Dictionary<int, int>> _historial_variaciones_optimizacion;
        private Dictionary<int, Dictionary<int, int>> _historial_variaciones_regresion;

        private OrganizadorTramos _tramos_optimizacion;

        private int _salto_variaciones;

        public Itinerario ItinerarioBase { get; set; }

        public ParametrosSimuLAN Parametros { get; set; }

        public ModeloDisrupciones Disrupciones { get; set; }        

        public Optimizador(Itinerario itinerario_base,ParametrosSimuLAN parametros, ModeloDisrupciones disrupciones, int std, DateTime fechaIni, DateTime fechaFin)
        {
            this.ItinerarioBase = itinerario_base;
            this.Parametros = parametros;
            this.Disrupciones = disrupciones;
            this._std = std;
            this._fecha_ini = fechaIni;
            this._fecha_fin = fechaFin;
            this._tramos_optimizacion = new OrganizadorTramos(itinerario_base);
            this._salto_variaciones = 5;
            this._historial_puntualidades_optimizacion = new Dictionary<int, Dictionary<int, ExplicacionImpuntualidad>>();
            this._historial_puntualidades_regresion = new Dictionary<int, Dictionary<int, ExplicacionImpuntualidad>>();
            this._historial_variaciones_optimizacion = new Dictionary<int, Dictionary<int, int>>();
            this._historial_variaciones_regresion = new Dictionary<int, Dictionary<int, int>>();
        }
        //Para cada tramo estimar sus posibilidades de variación y asignarle un radio más y radio menos.
        //Deberá retornar un objeto Resultado con un itinerario propuesto y una estimación de sus beneficios en comparación al itinerario base.
        //Deberá recibir un escenario objetivo, condiciones de término, semilla y parámetros de optimización.
        //Condiciones de término: tiempo, número de iteraciones
        //Parámetros de optimización: variación mínima de STD en minutos, número de tramos modificables (o porcentaje de tramos), radio de variación máximo
        

        //Mover por avión, con prioridad para los tramos con mayor reacionario. Para todos los tramos con alta impuntualidad (después definir filtros por puntualidad objetivo).
        //Las variaciones serán desde el step mínimo. Luego, simular nuevamente y ver el progreso de cada tramo.
        //Si un tramo mejora, aumentar su variación. Si empeora, bajar su variación al valor anterior (o al negativo).
        //Iterar tantas veces como variaciones sean posibles.
   
        //Para problema de simultaneidades, aplicar los cambios, validar después que no se violen restricciones. Si se viola alguna, deshacer uno de los cambios bajo el criterio de búsqueda (eliminar el movimiento que menos aporte)

        public void OptimizarReaccionarios(CambiarVistaSimularEventHandler cambiarVista, EnviarMensajeEventHandler enviarMensaje, ActualizarPorcentajeEventHandler actualizarPorcentaje, ref bool optimizacionCancelada,int total_iteraciones)
        {           
            
            List<int> stds = new List<int>();
            stds.Add(_std);
            //Optimizacion inicial
            enviarMensaje("Generando simulación base");            
            ManagerSimulacion manager = new ManagerSimulacion(ItinerarioBase, Parametros, Disrupciones, stds, _fecha_ini, _fecha_fin, enviarMensaje, actualizarPorcentaje, ref optimizacionCancelada);            
            List<Simulacion> replicasBase = manager.SimularNormal();
            Dictionary<int, ExplicacionImpuntualidad> impuntualidades_base = _tramos_optimizacion.EstimarImpuntualidades(replicasBase, _fecha_ini, _fecha_fin, _std);
            this._tramos_optimizacion.CargarImpuntualidadesBase(impuntualidades_base);            
            int iteraciones = 1;
            this._historial_puntualidades_optimizacion.Add(iteraciones, impuntualidades_base);
            this._historial_variaciones_optimizacion.Add(iteraciones, new Dictionary<int, int>());
            for (int i = 0; i <= this.ItinerarioBase.Tramos.Count*2; i++)
            {
                this._historial_variaciones_optimizacion[iteraciones].Add(i,0);
            }

            while (iteraciones < total_iteraciones)
            {
                int variaciones,cambios;
                this._tramos_optimizacion.OptimizarCurvasAtrasoPropagado(out variaciones,_salto_variaciones);
                //Aplica variaciones en itinerario (siempre que no viole restricciones)
                manager.ItinerarioBase = _tramos_optimizacion.GenerarNuevoItinerarioConCambios(Parametros.Escalares.Semilla);
                enviarMensaje("Optimización número: " + iteraciones + ", variaciones: " + variaciones);
                List<Simulacion> replicas_sim_1 = manager.SimularNormal();
                Dictionary<int, ExplicacionImpuntualidad> impuntualidades_sim_1 = _tramos_optimizacion.EstimarImpuntualidades(replicas_sim_1, _fecha_ini, _fecha_fin, _std);
                Dictionary<int, int> variaciones_1 = _tramos_optimizacion.ObtenerVariacionesPropuestas();
                this._tramos_optimizacion.CargarImpuntualidadesIteraciones(impuntualidades_sim_1);
                iteraciones++;
                this._historial_puntualidades_optimizacion.Add(iteraciones, impuntualidades_sim_1);
                this._historial_variaciones_optimizacion.Add(iteraciones, variaciones_1);

                if (iteraciones <= total_iteraciones / 1.25)
                {
                    this._tramos_optimizacion.DeshacerCambiosQueEmpeoranPuntualidad(out cambios, this._historial_puntualidades_optimizacion,_historial_variaciones_optimizacion[iteraciones-1], true);
                }
                else
                {
                    this._tramos_optimizacion.VolverAEstadoDeMejorPuntualidad(_historial_variaciones_optimizacion, _historial_puntualidades_optimizacion, _historial_variaciones_regresion, _historial_puntualidades_regresion);
                }
                manager.ItinerarioBase = _tramos_optimizacion.GenerarNuevoItinerarioConCambios(Parametros.Escalares.Semilla);
                List<Simulacion> replicas_sim_2 = manager.SimularNormal();
                Dictionary<int, ExplicacionImpuntualidad> impuntualidades_sim_2 = _tramos_optimizacion.EstimarImpuntualidades(replicas_sim_2, _fecha_ini, _fecha_fin, _std);
                Dictionary<int, int> variaciones_2 = _tramos_optimizacion.ObtenerVariacionesPropuestas();
                this._historial_variaciones_regresion.Add(iteraciones, variaciones_2);
                this._historial_puntualidades_regresion.Add(iteraciones, impuntualidades_sim_2);
            }
            //Agregar los tramos previos (siempre que no estén en la lista): estos son los que generan el atraso reaccionario, y deben anticiparse

            //Dividir la lista en una lista para cada avión, y reordenar cronológicamente

            //Con LTFM, iterar:
                //Llevar al máximo las variaciones desde el primer tramo al último de cada avión, de manera que se minimice el atraso reaccionario. Los sin reaccionario, se adelantan, y los con reaccionario, se retrasan.
            ExportarHistorialPuntualidades(@"C:\Users\Rodolfo\Desktop\AnalisisSimuLAN\" + DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".xls");            
        }
    
        internal void ExportarHistorialPuntualidades(string dir)
        {
            FileStream fs = new FileStream(dir, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            StringBuilder sb = new StringBuilder();
            sb.Append("iteracion");
            sb.Append("\tfase");
            sb.Append("\tid_tramo");
            //sb.Append("\tid_avion");
            sb.Append("\tImpuntualidadTotal");
            sb.Append("\tImpuntualidadReaccionarios");
            sb.Append("\tImpuntualidadSinReaccionarios");
            sb.Append("\tVariacion");
            sw.WriteLine(sb.ToString());
            foreach (int iteracion in this._historial_puntualidades_optimizacion.Keys)
            {
                foreach (int tramo in this._historial_puntualidades_optimizacion[iteracion].Keys)
                {
                    sb = new StringBuilder();
                    sb.Append(iteracion);
                    sb.Append("\tOptimizacion");
                    sb.Append("\t" + tramo);
                    sb.Append("\t" + this._historial_puntualidades_optimizacion[iteracion][tramo].InfoParaReporte());
                    if (this._historial_variaciones_optimizacion.ContainsKey(iteracion))
                    {
                        sb.Append("\t" + this._historial_variaciones_optimizacion[iteracion][tramo]);
                    }
                    else
                    {
                        sb.Append("\t0");
                    }
                    sw.WriteLine(sb.ToString());
                    if (this._historial_puntualidades_regresion.ContainsKey(iteracion))
                    {
                        sb = new StringBuilder();
                        sb.Append(iteracion);
                        sb.Append("\tRegresion");
                        sb.Append("\t" + tramo);
                        sb.Append("\t" + this._historial_puntualidades_regresion[iteracion][tramo].InfoParaReporte());
                        if (this._historial_variaciones_regresion.ContainsKey(iteracion))
                        {
                            sb.Append("\t" + this._historial_variaciones_regresion[iteracion][tramo]);
                        }
                        else
                        {
                            sb.Append("\t0");
                        }
                        sw.WriteLine(sb.ToString());
                    }
                }
            }
            sw.Close();
            fs.Close();
        }
    
    }

     
}
