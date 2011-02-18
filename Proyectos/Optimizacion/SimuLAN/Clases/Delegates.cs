using System;
using System.Collections.Generic;
using System.Text;
using SimuLAN.Utils;
using SimuLAN.Clases.Disrupciones;
using SimuLAN.Clases.Recovery;

namespace SimuLAN.Clases
{
    /// <summary>
    /// Delegado que encapsula el método de actualización del tiempo de simulación
    /// </summary>
    /// <param name="t"></param>
    public delegate void ActualizarTiempoSimulacionEventHandler(int t);

    /// <summary>
    /// Delegado que encapsula método para el cálculo de la utilización de slots de backups
    /// </summary>
    /// <param name="matricula">Matrícula programada</param>
    /// <param name="ini">Tiempo inicio</param>
    /// <param name="fin">Tiempo fin</param>
    /// <param name="conTA">True si se consideran los T/A internos en como utilizados por el avión</param>
    /// <returns>Porcentaje de utilización de un slot</returns>
    public delegate double EstimarUtilizacionSlotEventHandler(string matricula, int ini,int fin, bool conTA);
    
    /// <summary>
    /// Delegado que obtiene el objeto Aeropuerto correspondiente a id_aeropuerto
    /// </summary>
    /// <param name="id_aeropuerto">Identificador de un aeropuerto</param>
    /// <returns>Aeropuerto</returns>
    public delegate Aeropuerto GetAeropuertoEventHandler(string id_aeropuerto);   

    /// <summary>
    /// Delegado que obtiene el objeto Avión correspondiente a id_avion
    /// </summary>
    /// <param name="id_avion">Identificador de un avión</param>
    /// <returns>Avión</returns>
    public delegate Avion GetAvionEventHandler(string id_avion);

    /// <summary>
    /// Delegado que encapsula método para obtener la lista de slots de backup asignada inicialmente a una matrícula
    /// </summary>
    /// <param name="id_avion">Matrícula de avión</param>
    /// <returns>Lista de unidades de backup asignadas a id_avion</returns>
    public delegate List<UnidadBackup> GetBackupsAvionEventHandler(string id_avion);

    /// <summary>
    /// Delegado que encapsula un método de búsqueda para obtener las conexiones que conectan a un tramo
    /// </summary>
    /// <param name="num_tramo">Número global del tramo</param>
    /// <param name="tipo">Tipo de conexión buscada</param>
    /// <param name="segundoTramo">True es un el segundo tramo de la conexión</param>
    /// <returns>Lista con las conexiones encontradas.</returns>
    public delegate SerializableList<ConexionLegs> GetConexionEventHandler(int num_tramo, TipoConexion tipo, bool segundoTramo);

    /// <summary>
    /// Delegado que obtiene la flota correspondiente a "acType"
    /// </summary>
    /// <param name="id_avion">Identificador de un acType</param>
    /// <returns>Flota</returns>
    public delegate string GetFlotaEventHandler(string acType);

    /// <summary>
    /// Delegado para obtener la cantidad de minutos que se esperarán por causa de una conexión de pasajeros particular.
    /// </summary>
    /// <param name="num_pax_conexion">Número de pasajeros en conexión</param>
    /// <param name="min_hasta_proximo_vuelo">Cantidad de minutos hasta el próximo vuelo</param>
    /// <returns></returns>
    public delegate int GetMinutosEsperaConexionEventHandler(double num_pax_conexion, int min_hasta_proximo_vuelo);

    /// <summary>
    /// Delegado que encapsula un método para obtener los minutos que faltan para que se repita un nuevo 
    /// par origen destino posterior al tramo objetivo
    /// </summary>
    /// <param name="tramo_objetivo">Tramo objetivo</param>
    /// <returns>Minutos que faltan para el próximo tramo</returns>
    public delegate int GetMinutosHastaProximoVueloEventHandler(Tramo tramo_objetivo);

    /// <summary>
    /// Delegado que encapsula método para obtener las curvas de un tramo en operación
    /// </summary>
    /// <param name="tipoDisrupcion">Disrupción que se requiere consultar</param>
    /// <param name="keys">Argumentos de búsqueda</param>
    /// <param name="factorEscenario">Factor de variación según escenario simulado</param>
    /// <param name="infoAtrasoTramo">Estructura con los parámetros de la curva</param>
    /// <param name="distribucion">Distribucion de probabilidades de la curva</param>
    public delegate void GetInformacionAtrasosEventHandler(TipoDisrupcion tipoDisrupcion, List<string> keys, out double factorEscenario, out DataDisrupcion infoAtrasoTramo, out DistribucionesEnum distribucion);

    /// <summary>
    /// Delagado que encapsula método para obtener la probabilidad de WXS para un mes, periodo y aeropuerto específico.
    /// </summary>
    /// <param name="mes">Mes del año</param>
    /// <param name="estacion">Estación de origen</param>
    /// <param name="hora">Hora de la simulación (UTC)</param>
    /// <returns></returns>
    public delegate double GetProbabilidadClimaEventHandler(string mes, string estacion, int hora);

    /// <summary>
    /// Delegado que encapsula método para obtener los slots de backup entre dos tramos.
    /// </summary>
    /// <param name="tramoIni">Tramo inicio</param>
    /// <param name="tramoFin">Tramo fin</param>
    /// <returns>Lista de slots de backup</returns>
    public delegate List<SlotBackup> GetSlotsBackupEventHander(Tramo tramoIni, Tramo tramoFin);

    /// <summary>
    /// Delegado para obtener un tramo específico del itinerario
    /// </summary>
    /// <param name="num_tramo">Número global del tramo buscado</param>
    /// <returns>Tramo de vuelo</returns>
    public delegate Tramo GetTramoEventHandler(int num_tramo);

    /// <summary>
    /// Delegado que obtiene el Turn Around correspondiente al aeropuerto de origen de "tramo"
    /// </summary>
    /// <param name="tramo">Tramo de vuelo objetivo</param>
    /// <returns>Minutos de T/A en el origen del tramo objetivo</returns>
    public delegate int GetTurnAroundMinEventHandler(Tramo tramo);

    /// <summary>
    /// Delegado para ejecutar la acción asociada a cada evento
    /// </summary>
    /// <returns></returns>
    public delegate bool MetodoEventoEventHandler();

    /// <summary>
    /// Delegado que encapsula algoritmo de recovery
    /// </summary>
    /// <param name="avion">Avion a recuperar</param>
    /// <param name="bloqueObjetivo">Tramo atrasado que se intentará recuperar</param>
    /// <param name="atrasoReaccionario">Total de atraso reaccionario</param>
    /// <returns>True si el recovery fue exitoso</returns>
    public delegate bool RecoveryEventHandler(Avion avion, Tramo tramoObjetivo, int atrasoReaccionario);

    /// <summary>
    /// Delegado que encapsula método para intentar usar un turno de backup
    /// </summary>
    /// <param name="grupo_flota">Grupo de flota que puede operar el turno de backup</param>
    /// <param name="fecha">Fecha de uso</param>
    /// <param name="hora_local_peticion">Hora local de uso</param>
    /// <param name="exitoso">Out que indica si se pudo usar el turno</param>
    public delegate void UsarTurnoBackupEventHandler(string grupo_flota,DateTime fecha, int hora_local_peticion, out bool exitoso);
}
