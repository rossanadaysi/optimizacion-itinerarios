using System;
using System.Collections.Generic;
using System.Text;
using SimuLAN.Clases.Disrupciones;
using System.IO;

namespace SimuLAN.Clases.Optimizacion
{
    public enum CriterioOptimizacion { MinutosAtraso, EstandarPuntualidad}
    public class Optimizador
    {
        private List<int> _stds;

        private int _std_objetivo;

        private DateTime _fecha_fin;

        private DateTime _fecha_ini;

        private int _variacion_permitida;

        private LogOptimizacion _log_info_optimizacion;

        private OrganizadorTramos _tramos_optimizacion;

        private int _salto_variaciones;

        private DateTime _marcador;

        private TimeSpan _tiempo_simulacion;

        private TimeSpan _tiempo_optimizacion;

        public Itinerario ItinerarioBase { get; set; }

        public ParametrosSimuLAN Parametros { get; set; }

        public ModeloDisrupciones Disrupciones { get; set; }

        public double MinutosOptimizacion
        {
            get{
                return _tiempo_optimizacion.TotalMinutes;
            }
        }

        public double MinutosSimulacion
        {
            get
            {
                return _tiempo_simulacion.TotalMinutes;
            }
        }

        public Optimizador(Itinerario itinerario_base,ParametrosSimuLAN parametros, ModeloDisrupciones disrupciones, List<int> stds,int std_objetivo, DateTime fechaIni, DateTime fechaFin, int variacion_permitida)
        {
            this.ItinerarioBase = itinerario_base;
            this.Parametros = parametros;
            this.Disrupciones = disrupciones;
            this._stds = stds;
            this._variacion_permitida = variacion_permitida;
            this._fecha_ini = fechaIni;
            this._fecha_fin = fechaFin;
            this._tramos_optimizacion = new OrganizadorTramos(itinerario_base, variacion_permitida);
            this._salto_variaciones = 5;
            this._log_info_optimizacion = new LogOptimizacion();
            this._std_objetivo = std_objetivo;
            this._marcador = DateTime.Now;
            this._tiempo_simulacion = TimeSpan.Zero;
            this._tiempo_optimizacion = TimeSpan.Zero;
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

        public void OptimizarReaccionarios2(CambiarVistaSimularEventHandler cambiarVista, EnviarMensajeEventHandler enviarMensaje, ActualizarPorcentajeEventHandler actualizarPorcentaje, ref bool optimizacionCancelada,int total_iteraciones)
        {      
            //Optimizacion inicial
            enviarMensaje("Generando simulación base");            
            ManagerSimulacion manager = new ManagerSimulacion(ItinerarioBase, Parametros, Disrupciones, _stds, _fecha_ini, _fecha_fin, enviarMensaje, actualizarPorcentaje, ref optimizacionCancelada);            
            List<Simulacion> replicasBase = manager.SimularNormal();
            Dictionary<int, ExplicacionImpuntualidad> impuntualidades_base = _tramos_optimizacion.EstimarImpuntualidades(replicasBase, _fecha_ini, _fecha_fin, _stds);
            this._tramos_optimizacion.CargarImpuntualidadesBase(impuntualidades_base);            
            int iteraciones = 1;
            this._log_info_optimizacion.AgregarInfoImpuntualidad(iteraciones, FaseOptimizacion.Inicio, impuntualidades_base);
            _tiempo_simulacion = _tiempo_simulacion.Add(DateTime.Now - _marcador);
            _marcador = DateTime.Now;
            while (iteraciones < total_iteraciones)
            {
                enviarMensaje("Optimización número: " + iteraciones);
                this._tramos_optimizacion.OptimizarCurvasAtrasoPropagado(_salto_variaciones);
                _tiempo_optimizacion = _tiempo_optimizacion.Add(DateTime.Now - _marcador);
                _marcador = DateTime.Now;
                //Aplica variaciones en itinerario (siempre que no viole restricciones)
                manager.ItinerarioBase = _tramos_optimizacion.GenerarNuevoItinerarioConCambios(Parametros.Escalares.Semilla);                
                List<Simulacion> replicas_sim_1 = manager.SimularNormal();
                _tiempo_simulacion = _tiempo_simulacion.Add(DateTime.Now - _marcador);
                _marcador = DateTime.Now;
                Dictionary<int, ExplicacionImpuntualidad> impuntualidades_sim_1 = _tramos_optimizacion.EstimarImpuntualidades(replicas_sim_1, _fecha_ini, _fecha_fin, _stds);
                Dictionary<int, int> variaciones_1 = _tramos_optimizacion.ObtenerVariacionesPropuestas();
                this._tramos_optimizacion.CargarImpuntualidadesIteraciones(impuntualidades_sim_1);
                iteraciones++;
                _log_info_optimizacion.AgregarInfoImpuntualidad(iteraciones, FaseOptimizacion.Optimizacion, impuntualidades_sim_1);
                _log_info_optimizacion.AgregarInfoVariaciones(iteraciones, FaseOptimizacion.Optimizacion, variaciones_1);
                _tiempo_optimizacion = _tiempo_optimizacion.Add(DateTime.Now - _marcador);
                _marcador = DateTime.Now;
                //if (iteraciones <= total_iteraciones / 1.25)
                //{
                //    this._tramos_optimizacion.DeshacerCambiosQueEmpeoranPuntualidad(out cambios, this._historial_puntualidades_optimizacion,_historial_variaciones_optimizacion[iteraciones-1], true);
                //}
                //else
                //{
                //    this._tramos_optimizacion.VolverAEstadoDeMejorPuntualidad(_historial_variaciones_optimizacion, _historial_puntualidades_optimizacion, _historial_variaciones_regresion, _historial_puntualidades_regresion);
                //}
                if (iteraciones <= total_iteraciones / 2.0)
                {
                    //this._tramos_optimizacion.DeshacerCambiosQueEmpeoranAtrasoPropagado(out cambios, this._historial_puntualidades_optimizacion, _historial_variaciones_optimizacion[iteraciones - 1], true);
                }
                else if(iteraciones % 2 == 0)
                {
                    this._tramos_optimizacion.VolverAEstadoDeMenorAtrasoPropagado(_log_info_optimizacion);
                    manager.ItinerarioBase = _tramos_optimizacion.GenerarNuevoItinerarioConCambios(Parametros.Escalares.Semilla);
                    List<Simulacion> replicas_sim_2 = manager.SimularNormal();
                    _tiempo_simulacion = _tiempo_simulacion.Add(DateTime.Now - _marcador);
                    _marcador = DateTime.Now;
                    Dictionary<int, ExplicacionImpuntualidad> impuntualidades_sim_2 = _tramos_optimizacion.EstimarImpuntualidades(replicas_sim_2, _fecha_ini, _fecha_fin, _stds);
                    Dictionary<int, int> variaciones_2 = _tramos_optimizacion.ObtenerVariacionesPropuestas();
                    _log_info_optimizacion.AgregarInfoVariaciones(iteraciones, FaseOptimizacion.Ajuste, variaciones_2);
                    _log_info_optimizacion.AgregarInfoImpuntualidad(iteraciones, FaseOptimizacion.Ajuste, impuntualidades_sim_2);
                    _tiempo_optimizacion = _tiempo_optimizacion.Add(DateTime.Now - _marcador);
                    _marcador = DateTime.Now;
                }
                
            }
            _log_info_optimizacion.ObtenerIteracionOptima(CriterioOptimizacion.MinutosAtraso, _std_objetivo);            
        }

        public void OptimizarReaccionarios(CambiarVistaSimularEventHandler cambiarVista, EnviarMensajeEventHandler enviarMensaje, ActualizarPorcentajeEventHandler actualizarPorcentaje, ref bool optimizacionCancelada, int total_iteraciones)
        {
            //Optimizacion inicial

            enviarMensaje("Generando simulación base");
            ManagerSimulacion manager = new ManagerSimulacion(ItinerarioBase, Parametros, Disrupciones, _stds, _fecha_ini, _fecha_fin, enviarMensaje, actualizarPorcentaje, ref optimizacionCancelada);
            List<Simulacion> replicasBase = manager.SimularNormal();
            _tiempo_simulacion = _tiempo_simulacion.Add(DateTime.Now - _marcador);
            _marcador = DateTime.Now;
            Dictionary<int, ExplicacionImpuntualidad> impuntualidades_base = _tramos_optimizacion.EstimarImpuntualidades(replicasBase, _fecha_ini, _fecha_fin, _stds);
            this._tramos_optimizacion.CargarImpuntualidadesBase(impuntualidades_base);
            int iteraciones = 1;
            _log_info_optimizacion.AgregarInfoImpuntualidad(iteraciones, FaseOptimizacion.Inicio, impuntualidades_base);

            while (iteraciones < total_iteraciones)
            {
                this._tramos_optimizacion.OptimizarCurvasAtrasoPropagado( _salto_variaciones);
                //Aplica variaciones en itinerario (siempre que no viole restricciones)
                manager.ItinerarioBase = _tramos_optimizacion.GenerarNuevoItinerarioConCambios(Parametros.Escalares.Semilla);
                enviarMensaje("Optimización número: " + iteraciones);
                _tiempo_optimizacion = _tiempo_optimizacion.Add(DateTime.Now - _marcador);
                _marcador = DateTime.Now;
                List<Simulacion> replicas_sim_1 = manager.SimularNormal();
                _tiempo_simulacion = _tiempo_simulacion.Add(DateTime.Now - _marcador);
                _marcador = DateTime.Now;
                Dictionary<int, ExplicacionImpuntualidad> impuntualidades_sim_1 = _tramos_optimizacion.EstimarImpuntualidades(replicas_sim_1, _fecha_ini, _fecha_fin, _stds);
                Dictionary<int, int> variaciones_1 = _tramos_optimizacion.ObtenerVariacionesPropuestas();
                this._tramos_optimizacion.CargarImpuntualidadesIteraciones(impuntualidades_sim_1);
                iteraciones++;
                _log_info_optimizacion.AgregarInfoVariaciones(iteraciones, FaseOptimizacion.Optimizacion, variaciones_1);
                _log_info_optimizacion.AgregarInfoImpuntualidad(iteraciones, FaseOptimizacion.Optimizacion, impuntualidades_sim_1);
                _tiempo_optimizacion = _tiempo_optimizacion.Add(DateTime.Now - _marcador);
                _marcador = DateTime.Now;
            }
            _log_info_optimizacion.ObtenerIteracionOptima(CriterioOptimizacion.MinutosAtraso, _std_objetivo);
        }

        public void ImprimirReportes(string path)
        {
            if (_log_info_optimizacion != null)
            {
                _log_info_optimizacion.ImprimirDetalles(path + @"\Optimizacion\Detalles" + DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".xls");
                _log_info_optimizacion.ImprimirResumenIteraciones(path + @"\Optimizacion\Iteraciones" + DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".xls", GetDominio(), _stds);
                _log_info_optimizacion.ImprimirOptimo(path + @"\Optimizacion\Optimo" + DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".xls", GetDominio(), _stds, this.ItinerarioBase, MinutosSimulacion, MinutosOptimizacion);            
            }
           
        }

        private List<int> GetDominio()
        {
            List<int> variaciones = new List<int>();
            for (int i = -_variacion_permitida; i <= _variacion_permitida; i = i + 5)
            {
                variaciones.Add(i);
            }
            return variaciones;
        }
    
    }

     
}
