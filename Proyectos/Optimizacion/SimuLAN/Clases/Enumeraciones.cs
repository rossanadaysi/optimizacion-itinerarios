using System;
using System.Collections.Generic;
using System.Text;

namespace SimuLAN.Clases
{
    /// <summary>
    /// Tipos de estado de tramo con respecto al tiempo de simulación
    /// </summary>
    public enum EstadoTramo { NoIniciado, Iniciado, EnProceso, Finalizado }

    /// <summary>
    /// Tipos de avión con respecto al tipo de planificación
    /// </summary>
    public enum TipoAvion { BackupVacio, Normal, BackupParcial }

    /// <summary>
    /// Tipos de conexiones entre vuelos soportadas
    /// </summary>
    public enum TipoConexion { Pasajeros, Pairing }

    /// <summary>
    /// Tipos de conexiones con respecto a los aviones conectados
    /// </summary>
    public enum TipoConexionAvion { CambiaAvion, MantieneAvion }

    /// <summary>
    /// Enumeración con las causas de atraso registradas en lista de causas de atraso
    /// </summary>
    public enum TipoDisrupcion { ADELANTO, METEREOLOGIA, HBT, MANTENIMIENTO, OTROS, ATC, RC, RC_TRIP, RC_PAX, RECURSOS_DEL_APTO, TA_BAJO_ALA, TA_SOBRE_ALA, TRIPULACIONES }

    /// <summary>
    /// Enumeración de los tipos de escenarios que pueden haber por cada disrupción
    /// </summary>
    public enum TipoEscenarioDisrupcion { Bueno, Malo, Normal };

    /// <summary>
    /// Tipos de eventos soportados
    /// </summary>
    public enum TipoEvento { InicioTramo, Despegue, Aterrizaje, InicioMantenimiento, FinMantenimiento }

    /// <summary>
    /// Tipos de casos en que se presenta carencia de información
    /// </summary>
    public enum TipoFaltaInformacion { AcType_Flota, SubFlota_Matricula, Flota_Grupo, Multioperador_SubFlota, Multioperador_Operador,Flota_Flota, Tramo_Ruta, TurnAround, Curva_HBT, Curva_WXS, Curva_Mantto, Curva_Adelanto, Curva_ATC, Curva_Trip, Curva_TA_BA, Curva_TA_SA, Curva_OTROS, Curva_Recursos_Apto }

    /// <summary>
    /// Tipos de pairings con respecto a si cambian o mantienen el avión
    /// </summary>
    public enum TipoPairing { Mantiene, Cambia }

    /// <summary>
    /// Tipo de swap dependiente de la cantidad de tramos del avión receptor. Si cero es inserción.
    /// </summary>
    public enum TipoSwap { Normal, Insercion }

    /// <summary>
    /// Tipos de tramos base leidos desde itinerario
    /// </summary>
    public enum TipoTramoBase { Leg, Mantto,Backup }

    /// <summary>
    /// Tipos de uso que se pueden dar a un slot de backup
    /// </summary>
    public enum TipoUsoBackup { SinUso, Swap, AOG};

    /// <summary>
    /// Indica la parte donde se usa un avión de backup en un swap.
    /// </summary>
    public enum UsoBackup { IniReceptor, FinEmisor, FinReceptor, NoUsa}


}
