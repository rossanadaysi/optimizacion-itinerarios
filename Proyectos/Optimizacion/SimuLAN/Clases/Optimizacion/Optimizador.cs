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

        private Dictionary<int, Dictionary<int, ExplicacionImpuntualidad>> _historial_puntualidades;

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
            this._salto_variaciones = 15;
            this._historial_puntualidades = new Dictionary<int, Dictionary<int, ExplicacionImpuntualidad>>();
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

        public void OptimizarReaccionarios(CambiarVistaSimularEventHandler cambiarVista, EnviarMensajeEventHandler enviarMensaje, ActualizarPorcentajeEventHandler actualizarPorcentaje, ref bool optimizacionCancelada)
        {           
            
            List<int> stds = new List<int>();
            stds.Add(_std);
            //Optimizacion inicial
            enviarMensaje("Generando simulación base");            
            ManagerSimulacion manager = new ManagerSimulacion(ItinerarioBase, Parametros, Disrupciones, stds, _fecha_ini, _fecha_fin, enviarMensaje, actualizarPorcentaje, ref optimizacionCancelada);            
            List<Simulacion> replicasBase = manager.SimularNormal();
            Dictionary<int,ExplicacionImpuntualidad> impuntualidades_base = _tramos_optimizacion.EstimarImpuntualidades(replicasBase, _fecha_ini, _fecha_fin, _std);
            this._tramos_optimizacion.CargarImpuntualidadesBase(impuntualidades_base);            
            int iteraciones = 1;
            this._historial_puntualidades.Add(iteraciones, impuntualidades_base);
            while (iteraciones < 10)
            {
                int variaciones, cambios_deshechos, tramos_cerrados;
                this._tramos_optimizacion.OptimizarVariacionesReaccionarios(out variaciones, out cambios_deshechos, out tramos_cerrados);
                //Aplica variaciones en itinerario (siempre que no viole restricciones)
                manager.ItinerarioBase = _tramos_optimizacion.GenerarNuevoItinerarioConCambios(Parametros.Escalares.Semilla);
                enviarMensaje("Optimización número: " + iteraciones + ", variaciones: " + variaciones + ", tramos cerrados: " + tramos_cerrados);
                List<Simulacion> replicas = manager.SimularNormal();
                Dictionary<int, ExplicacionImpuntualidad> impuntualidades = _tramos_optimizacion.EstimarImpuntualidades(replicas, _fecha_ini, _fecha_fin, _std);
                this._tramos_optimizacion.CargarImpuntualidadesIteraciones(impuntualidades);
                iteraciones++;
                this._historial_puntualidades.Add(iteraciones, impuntualidades);
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
            sb.Append("\tid_tramo");
            sb.Append("\tImpuntualidadTotal");
            sb.Append("\tImpuntualidadReaccionarios");
            sb.Append("\tImpuntualidadSinReaccionarios");
            sb.Append("\tRazonReaccionarios");
            sb.Append("\tCausasAtraso");
            sw.WriteLine(sb.ToString());
            foreach (int iteracion in this._historial_puntualidades.Keys)
            {
                
                foreach (int id_tramo in this._historial_puntualidades[iteracion].Keys)
                {
                    sb = new StringBuilder();
                    sb.Append(iteracion);
                    sb.Append("\t" + id_tramo);
                    sb.Append("\t" + this._historial_puntualidades[iteracion][id_tramo].InfoParaReporte());
                    sw.WriteLine(sb.ToString());
                }
            }
            sw.Close();
            fs.Close();
        }
    
    }

     
}
