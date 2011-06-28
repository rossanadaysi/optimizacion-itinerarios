using System;
using System.Collections.Generic;
using System.Text;
using SimuLAN.Utils;

namespace SimuLAN.Clases.Recovery
{
    /// <summary>
    /// Clase que representa las características relevantes de un swap de recovery.
    /// </summary>
    public class Swap: IComparable, IDisposable
    {
        #region CONSTANTS

        /// <summary>
        /// Constante que da peso al producto entre los tramos que involucra el swap.
        /// </summary>
        private const int FACTOR_DISCRIMINANTE = 2;

        #endregion 

        #region ATRIBUTES

        /// <summary>
        /// Indica si producto del swap algún mantenimiento programado es retrasado
        /// </summary>
        private bool _afecta_mantto_programado;

        /// <summary>
        /// Delegado para obtener un slot de backup desde el itinerario
        /// </summary>
        public GetSlotsBackupEventHander GetSlotsBackup;

        /// <summary>
        /// Id de avión emisor
        /// </summary>
        private string _id_avion_emisor;

        /// <summary>
        /// Id de avión receptor
        /// </summary>
        private string _id_avion_receptor;

        /// <summary>
        /// Minutos de atraso reaccionario iniciales
        /// </summary>
        private int _minutos_atraso_reaccionario_inicial;

        /// <summary>
        /// Minutos de atraso por activación de un turno de backup (demora por tiempo de viaje) 
        /// </summary>
        private int _minutos_atraso_turno;

        /// <summary>
        /// Minutos de atraso reducidos
        /// </summary>
        private int _minutos_ganancia;

        /// <summary>
        /// Minutos de atraso aumentados
        /// </summary>
        private int _minutos_perdida;
        
        /// <summary>
        /// Número de tramos del avión emisor s a rotar 
        /// </summary>
        private int _num_tramos_emisor;

        /// <summary>
        /// Número de tramos del avión receptor a rotar 
        /// </summary>
        private int _num_tramos_receptor;

        /// <summary>
        /// Número de tramos que resultan con una disminución en el atraso como consecuencia del swap.
        /// </summary>
        private int _num_tramos_disminuyen_atraso;
        
        /// <summary>
        /// Número de tramos que resultan con un aumento en el atraso como consecuencia del swap.
        /// </summary>
        private int _num_tramos_aumentan_atraso;

        /// <summary>
        /// Tiempo hasta donde quedaría holgura en el avion emisor
        /// </summary>
        private int _tiempo_fin_holgura_en_emisor;

        /// <summary>
        /// Tipo de swap dependiendo de la cantidad de tramos del 
        /// </summary>
        private TipoSwap _tipo;

        /// <summary>
        /// Indica la parte donde se usa un avión de backup en un swap.
        /// </summary>
        private UsoBackup _tipo_uso_backup;

        /// <summary>
        /// Tramo inicial del avión emisor pertenciente a la cadena de tramos a intercambiar 
        /// </summary>
        private Tramo _tramo_ini_emisor;

        /// <summary>
        /// Tramo inicial del avión receptor pertenciente a la cadena de tramos a intercambiar 
        /// </summary>
        private Tramo _tramo_ini_receptor;

        /// <summary>
        /// Tramo final del avión emisor pertenciente a la cadena de tramos a intercambiar 
        /// </summary>
        private Tramo _tramo_fin_emisor;

        /// <summary>
        /// Tramo final del avión receptor pertenciente a la cadena de tramos a intercambiar 
        /// </summary>
        private Tramo _tramo_fin_receptor;
       

        #endregion

        #region PROPERTIES
        public string Rotacion
        {
            get
            {
                string origen = TramoIniEmisor.TramoBase.Origen;
                string destino = TramoFinEmisor.TramoBase.Destino;
                string rotacion = (origen == destino) ? origen : (origen + "-" + destino);
                return rotacion;
            }

        }

        public string IdUnico
        {
            get
            {
                string s = "";
                s += this.TramoIniEmisor.TramoBase.Numero_Global;
                s += "-" + this.TramoFinEmisor.TramoBase.Numero_Global;
                s += "-" + this.TramoIniReceptor.TramoBase.Numero_Global;
                s += "-" + this.TramoFinReceptor.TramoBase.Numero_Global;
                return s;
            }

        }
        /// <summary>
        /// Indica si producto del swap algún mantenimiento programado es retrasado
        /// </summary>
        public bool AfectaManttoProgramado
        {
            get { return _afecta_mantto_programado; }
            set { _afecta_mantto_programado = value; }
        }

        /// <summary>
        /// Id de avión emisor
        /// </summary>
        public string IdAvionEmisor
        {
            get { return _id_avion_emisor; }           
        }

        /// <summary>
        /// Id de avión receptor
        /// </summary>
        public string IdAvionReceptor
        {
            get { return _id_avion_receptor; }
        }

        /// <summary>
        /// Minutos de atraso reaccionario iniciales
        /// </summary>
        public int MinutosAtrasoReaccionarioInicial
        {
            get{return _minutos_atraso_reaccionario_inicial;}
            set{_minutos_atraso_reaccionario_inicial = value;}
        }

        /// <summary>
        /// Minutos de atraso por activación de un turno de backup (demora por tiempo de viaje) 
        /// </summary>
        public int MinutosAtrasoTurno
        {
            get { return _minutos_atraso_turno; }
        }

        /// <summary>
        /// Minutos de atraso ganados menos minutos de atraso aumentados 
        /// </summary>
        public int MinutosGananciaNeta
        {
            get { return _minutos_ganancia - _minutos_perdida; }           
        }

        /// <summary>
        /// Minutos de atraso reducidos
        /// </summary>
        public int MinutosGanancia
        {
            get { return _minutos_ganancia; }
            set { _minutos_ganancia = value; }
        }

        /// <summary>
        /// Minutos de atraso aumentados
        /// </summary>
        public int MinutosPerdida
        {
            get { return _minutos_perdida; }
            set { _minutos_perdida = value; }
        }

        /// <summary>
        /// Indica si el swap es válido por no romper una cadena de vuelos.
        /// </summary>
        public bool NoRompeCadenaVuelo
        {
            get
            {
                bool ok_inicio_emisor = TramoIniEmisor.Tramo_Previo != null ? (TramoIniEmisor.TramoBase.Numero_Vuelo != TramoIniEmisor.Tramo_Previo.TramoBase.Numero_Vuelo) : true;
                bool ok_fin_emisor = TramoFinEmisor.Tramo_Siguiente != null ? (TramoFinEmisor.TramoBase.Numero_Vuelo != TramoFinEmisor.Tramo_Siguiente.TramoBase.Numero_Vuelo) : true;
                bool ok_inicio_receptor = TramoIniReceptor.Tramo_Previo != null ? (TramoIniReceptor.TramoBase.Numero_Vuelo != TramoIniReceptor.Tramo_Previo.TramoBase.Numero_Vuelo) : true;
                bool ok_fin_receptor = (TramoIniReceptor != TramoFinReceptor) ? (TramoFinReceptor.Tramo_Siguiente != null ? (TramoFinReceptor.TramoBase.Numero_Vuelo != TramoFinReceptor.Tramo_Siguiente.TramoBase.Numero_Vuelo) : true) : true;
                bool retorno = ok_fin_emisor && ok_fin_receptor && ok_inicio_emisor && ok_inicio_receptor;
                return retorno;
            }
        }

        /// <summary>
        /// Número de tramos del avión emisor a rotar 
        /// </summary>
        public int NumTramosEmisor
        {
            get { return _num_tramos_emisor; }
            set { _num_tramos_emisor = value; }
        }

        /// <summary>
        /// Número de tramos del avión receptor a rotar 
        /// </summary>
        public int NumTramosReceptor
        {
            get { return _num_tramos_receptor; }
            set { _num_tramos_receptor = value; }
        }

        /// <summary>
        /// Número de tramos que resultan con una disminución en el atraso como consecuencia del swap.
        /// </summary>
        public int NumTramosDisminuyenAtraso
        {
            get { return _num_tramos_disminuyen_atraso; }
            set { _num_tramos_disminuyen_atraso = value; }
        }

        /// <summary>
        /// Número de tramos que resultan con un aumento en el atraso como consecuencia del swap.
        /// </summary>
        public int NumTramosAumentanAtraso 
        {
            get { return _num_tramos_aumentan_atraso; }
            set { _num_tramos_disminuyen_atraso = value; }
        }

        /// <summary>
        /// Retorna un entero que representa la efectividad del swap en relación a su efecto en el 
        /// itinerario (ganancia - pérdida de minutos) y la cantidad de tramos involucrados, que en 
        /// sí tienen un costo no valorado en la simulación.
        /// </summary>
        public int ValorDiscriminante
        {
            get 
            {   if((_num_tramos_emisor + 1)*(_num_tramos_receptor + 1)==1)
                {
                    return int.MinValue;
                }
                else
                {
                    return MinutosGananciaNeta - FACTOR_DISCRIMINANTE * (_num_tramos_receptor + 1) * (_num_tramos_emisor + 1) - _minutos_perdida;
                }
            }
        }

        /// <summary>
        /// Tiempo hasta donde quedaría holgura en el avion emisor
        /// </summary>
        public int TiempoFinHolguraEnEmisor
        {
            get { return _tiempo_fin_holgura_en_emisor; }
        }

        /// <summary>
        /// Tipo de swap dependiendo de la cantidad de tramos del 
        /// </summary>
        public TipoSwap Tipo
        {
            get { return _tipo; }
        }

        /// <summary>
        /// Indica la parte donde se usa un avión de backup en un swap.
        /// </summary>
        public UsoBackup TipoUsoBackup
        {
            get { return _tipo_uso_backup; }
            set { _tipo_uso_backup = value; }
        }

        /// <summary>
        /// Tramo inicial del avión emisor pertenciente a la cadena de tramos a intercambiar 
        /// </summary>
        public Tramo TramoIniEmisor
        {
            get { return _tramo_ini_emisor; }
            set { _tramo_ini_emisor = value; }
        }

        /// <summary>
        /// Tramo inicial del avión receptor pertenciente a la cadena de tramos a intercambiar 
        /// </summary>
        public Tramo TramoIniReceptor
        {
            get { return _tramo_ini_receptor; }
            set { _tramo_ini_receptor = value; }
        }

        /// <summary>
        /// Tramo final del avión emisor pertenciente a la cadena de tramos a intercambiar 
        /// </summary>
        public Tramo TramoFinEmisor
        {
            get { return _tramo_fin_emisor; }
            set { _tramo_fin_emisor = value; }
        }

        /// <summary>
        /// Tramo final del avión receptor pertenciente a la cadena de tramos a intercambiar 
        /// </summary>
        public Tramo TramoFinReceptor
        {
            get { return _tramo_fin_receptor; }
            set { _tramo_fin_receptor = value; }
        }

        public Tramo TramoPostBackup
        {
            get 
            {
                if (TipoUsoBackup == UsoBackup.IniReceptor)
                {
                    return TramoIniEmisor;
                }
                else if (TipoUsoBackup == UsoBackup.FinEmisor)
                {
                    return TramoFinEmisor.Tramo_Siguiente ;
                }
                else if (TipoUsoBackup == UsoBackup.FinReceptor)
                {
                    return TramoFinReceptor.Tramo_Siguiente;
                }
                else return null;
            }
        }

        public Tramo TramoPreBackup
        {
            get
            {
                if (TipoUsoBackup == UsoBackup.IniReceptor)
                {
                    return TramoIniEmisor.Tramo_Previo;
                }
                else if (TipoUsoBackup == UsoBackup.FinEmisor)
                {
                    return TramoFinEmisor;
                }
                else if (TipoUsoBackup == UsoBackup.FinReceptor)
                {
                    return TramoFinReceptor;
                }
                else return null;
            }
        }

        /// <summary>
        /// Indica si en el swap se usó un avión de backup
        /// </summary>
        public bool UsaBackup
        {
            get 
            {
                return !(_tipo_uso_backup == UsoBackup.NoUsa);            
            }
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor para serializacion e inicialización
        /// </summary>
        public Swap()
        { 
        
        }
        
        /// <summary>
        /// Constructor de swaps
        /// </summary>
        /// <param name="tramo_ini_receptor">Tramo inicial de la cadena del avión receptor</param>
        /// <param name="tramo_fin_receptor">Tramo final de la cadena del avión receptor</param>
        /// <param name="tramo_ini_emisor">Tramo inicial de la cadena del avión emisor</param>
        /// <param name="tramo_fin_emisor">Tramo final de la cadena del avión emisor</param>
        /// <param name="atraso_reaccionario">Atraso reaccionario inicial del avión emisor</param>
        public Swap(Tramo tramo_ini_receptor,Tramo tramo_fin_receptor,Tramo tramo_ini_emisor,Tramo tramo_fin_emisor, int atraso_reaccionario, int minutos_atraso_turno,TipoSwap tipo, UsoBackup tipo_uso_backup)
        {
            this._afecta_mantto_programado = false;
            this._id_avion_emisor = tramo_ini_emisor.IdAvionProgramadoActual;
            this._id_avion_receptor = tramo_ini_receptor.IdAvionProgramadoActual;
            this._tramo_ini_receptor = tramo_ini_receptor;
            this._tramo_fin_emisor = tramo_fin_emisor;
            this._tramo_fin_receptor = tramo_fin_receptor;
            this._tramo_ini_emisor = tramo_ini_emisor;
            this._minutos_atraso_reaccionario_inicial = atraso_reaccionario;
            this._num_tramos_emisor = 0;
            this._num_tramos_receptor = 0;
            this._num_tramos_aumentan_atraso = 0;
            this._num_tramos_disminuyen_atraso = 0;
            this._tiempo_fin_holgura_en_emisor = EstimarTiempoFinHolguraEnEmisor();
            this._tipo = tipo;
            this._minutos_atraso_turno = minutos_atraso_turno;
            this._tipo_uso_backup = tipo_uso_backup;
            ContarTramos(); 
            EvaluarSwap();
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Cuenta los tramos involucrados en el swap del avión emisor y el receptor
        /// </summary>
        private void ContarTramos()
        {
            this._num_tramos_emisor = 1;
            this._num_tramos_receptor = 1;
            Tramo auxCuentaEmisor = this._tramo_ini_emisor;
            Tramo auxCuentaReceptor = this._tramo_ini_receptor;
            while (auxCuentaEmisor != _tramo_fin_emisor)
            {
                auxCuentaEmisor = auxCuentaEmisor.Tramo_Siguiente;
                _num_tramos_emisor++;
            }
            while (auxCuentaReceptor != _tramo_fin_receptor)
            {
                auxCuentaReceptor = auxCuentaReceptor.Tramo_Siguiente;
                _num_tramos_receptor++;
            }
            if (_tipo == TipoSwap.Insercion)
            {
                _num_tramos_receptor = 0;
            }
        }

        /// <summary>
        /// Contabiliza el impacto del swap en términos de ganancia de minutos.
        /// </summary>
        /// <param name="tramo_base">Tramo sobre el cual se inserta la cadena a rotar</param>
        /// <param name="tramo_inicial_cadena_movida">Tramo inicial de la cadena a rotar</param>
        /// <param name="min_atraso_reaccionario_ini">Minutos de atraso reaccionario que afectan al tramo inicial de la cadena</param>
        private void EstimarImpactoSwapCadena(Tramo tramo_base, Tramo tramo_inicial_cadena_movida, int min_atraso_reaccionario_ini,int min_atraso_por_activacion_turno,bool busquedaPorConexion)
        {
            int temp_minutos_ganancia_ini = 0;
            int minutos_atraso_reaccionario_restante = 0;
            int turn_around_min_tramo_ini_base = tramo_base.TurnAroundMinimoOrigen;
            
            int tiempo_inicio_sin_swap_tramo_ini_cadena = tramo_inicial_cadena_movida.TInicialRst + min_atraso_reaccionario_ini;

            //PRIMERO: se estima la variación del atraso producto del swap sobre el primer tramo rescatado del avión emisor.
            if (tramo_base.Tramo_Previo != null)
            {
                if (busquedaPorConexion)
                {
                    int turn_around_min_tramo_ini_cadena_movida = tramo_inicial_cadena_movida.TurnAroundMinimoOrigen;
                    int tiempo_inicio_tramo_ini_sin_conexion =  Math.Max(tramo_inicial_cadena_movida.TInicialProg, tramo_inicial_cadena_movida.TFinRstTramoPrevio + turn_around_min_tramo_ini_cadena_movida);
                    temp_minutos_ganancia_ini = Math.Max(tiempo_inicio_sin_swap_tramo_ini_cadena - tiempo_inicio_tramo_ini_sin_conexion, 0);
                }
                else
                {
                    int tiempo_inicio_con_swap_tramo_ini_cadena = Math.Max(tramo_inicial_cadena_movida.TInicialProg + min_atraso_por_activacion_turno, tramo_base.Tramo_Previo.TFinalRst + turn_around_min_tramo_ini_base);
                    temp_minutos_ganancia_ini = tiempo_inicio_sin_swap_tramo_ini_cadena - tiempo_inicio_con_swap_tramo_ini_cadena;
                    if (temp_minutos_ganancia_ini < 0)
                    { 
                    
                    }
                }
            }
            else
            {
                if (busquedaPorConexion)
                {
                    temp_minutos_ganancia_ini = min_atraso_reaccionario_ini;
                }
                else
                {
                    temp_minutos_ganancia_ini = tiempo_inicio_sin_swap_tramo_ini_cadena - tramo_inicial_cadena_movida.TInicialProg;
                }
            }
            //Se setea la ganancia de minutos inicial.
            this._minutos_ganancia += temp_minutos_ganancia_ini;
            //Se setea el atraso reaccionario que afectaría al segundo tramo de la cadena del avión emisor.
            minutos_atraso_reaccionario_restante = Math.Max(min_atraso_reaccionario_ini - temp_minutos_ganancia_ini, 0);

            //SEGUNDO: Se contabilizan las ganancias de minutos sobre el resto de la cadena del avión emisor.
            EstimarGanaciaSwapTramosPosteriores(tramo_inicial_cadena_movida, temp_minutos_ganancia_ini, minutos_atraso_reaccionario_restante);
        }

        /// <summary>
        /// Estima efecto del swap sobre los tramos posteriores 
        /// </summary>
        /// <param name="temp_tramo"></param>
        /// <param name="minutos_ganancia_ini"></param>
        /// <param name="minutos_atraso_reaccionario_restante"></param>
        private void EstimarGanaciaSwapTramosPosteriores(Tramo temp_tramo, int minutos_ganancia_ini, int minutos_atraso_reaccionario_restante)
        {
            //Se avanza hacia adelante en los tramos hasta que se acaben los tramos o se acabe el atraso reaccionario.
            while (minutos_ganancia_ini > 0 && temp_tramo.Tramo_Siguiente != null)
            {
                //Se evalua ganacia es posibles conexiones
                SerializableList<ConexionLegs> conexiones = temp_tramo.ConexionesPairingPosteriores;// temp_tramo.GetConexion(temp_tramo.TramoBase.Numero_Global, TipoConexion.Pairing, false);

                if (conexiones.Count > 0)
                {
                    foreach (ConexionLegs posibleConexion in conexiones)
                    {
                        Tramo tramo_ini_conexion = posibleConexion.GetTramo(posibleConexion.NumTramoIni);
                        Tramo tramo_fin_conexion = posibleConexion.GetTramo(posibleConexion.NumTramoFin);
                        int turn_around_conexion = ConexionPairing.TIEMPO_CAMBIO_AVION;
                        int tiempo_ini_esperado_tramo_fin = tramo_fin_conexion.TInicialRst;
                        int tiempo_fin_esperado_tramo_ini = tramo_ini_conexion.TFinalRst + _minutos_atraso_reaccionario_inicial;
                        int reaccionario_conexion = Math.Max(0, tiempo_ini_esperado_tramo_fin - tiempo_fin_esperado_tramo_ini - turn_around_conexion);
                        if (reaccionario_conexion > 0)
                        {
                            EstimarImpactoSwapCadena(tramo_ini_conexion, tramo_fin_conexion, reaccionario_conexion,0, true);
                        }
                    }
                }

                //Se setea tramo revisado
                temp_tramo = temp_tramo.Tramo_Siguiente;
                //Se setea T/A de tramo revisado
                int turn_around_tramo_temp = temp_tramo.TurnAroundMinimoOrigen;
                //Se setea minutos de holgura del tramo temporal.
                int minutos_holgura_tramo_temp = temp_tramo.TInicialRst - temp_tramo.Tramo_Previo.TFinalRst - turn_around_tramo_temp;
                //Minutos de ganancia inicial son reducidos en función de la holgura observada.
                minutos_ganancia_ini = Math.Max(minutos_ganancia_ini - minutos_holgura_tramo_temp, 0);
                //Se aumentan paulatinamente los minutos de ganancia del swap
                this._minutos_ganancia += minutos_ganancia_ini;
                //Se actualiza el atraso reaccionario
                minutos_atraso_reaccionario_restante = Math.Max(minutos_atraso_reaccionario_restante - minutos_holgura_tramo_temp, 0);
                //Aumentan los tramos favorecidos con el swap
                this._num_tramos_disminuyen_atraso++;                                
            }
        }

        /// <summary>
        /// Contabiliza el impacto del swap sobre los tramos posteriores a las cadenas a rotar a una cadena rotada.
        /// </summary>
        /// <param name="tramo_inicial_posterior_afectado">Tramo inicial posterior a una cadena</param>
        /// <param name="tramo_final_cadena_recibida">Tramo final de la cadena rotada</param>
        /// <param name="busquedaPorConexion">Bool que indica si el método se usará para evaluar el atraso en una conexión</param>
        /// <param name="reaccionarioConexion">Atraso reaccionario traspasado al vuelo en conexión. Válido sólo cuando busquedaPorConexion es true</param>
        private void EstimarImpactoSwapTramosPosteriores(Tramo tramo_inicial_posterior_afectado, Tramo tramo_final_cadena_recibida, bool busquedaPorConexion, int reaccionarioConexion)
        {
            //Secuencia de escape cuando no tiene sentido seguir evaluando un swap malo.
            if (this._minutos_perdida > this._minutos_ganancia) return;

            int profundidad = 0;
            if (tramo_inicial_posterior_afectado != null)
            {
                //Se setea turn around de tramo revisado
                int turn_around_tramo_temp_receptor = 0;
                int temp_minutos_perdida_ini = 0;
                if(busquedaPorConexion)
                {
                    turn_around_tramo_temp_receptor = tramo_inicial_posterior_afectado.TurnAroundMinimoOrigen;
                    temp_minutos_perdida_ini = Math.Max(tramo_final_cadena_recibida.TFinalRst + turn_around_tramo_temp_receptor - tramo_inicial_posterior_afectado.TInicialRst, 0);
                }
                else
                {
                    turn_around_tramo_temp_receptor = ConexionPairing.TIEMPO_CAMBIO_AVION;// +minutosFaltanTurnAround;
                    temp_minutos_perdida_ini = Math.Max(tramo_final_cadena_recibida.TFinalRst + turn_around_tramo_temp_receptor - tramo_inicial_posterior_afectado.TInicialRst + reaccionarioConexion, 0);
                }                
                
                //Se recorre hacia adelante hasta que se acaben los tramos o no quede atraso.
                while (temp_minutos_perdida_ini > 0 && tramo_inicial_posterior_afectado != null)
                {
                    //Aumentan los tramos perjudicados con el swap.
                    this._num_tramos_aumentan_atraso++;
                    //Aumentan los minutos de atraso producto del swap.
                    this._minutos_perdida += temp_minutos_perdida_ini;

                    if ((busquedaPorConexion && profundidad >= 0) || (!busquedaPorConexion && profundidad >= 1))
                    {
                        SerializableList<ConexionLegs> conexiones = tramo_inicial_posterior_afectado.ConexionesPairingPosteriores;// tramo_inicial_posterior_afectado.GetConexion(tramo_inicial_posterior_afectado.TramoBase.Numero_Global, TipoConexion.Pairing, false);
                        foreach (ConexionLegs conexion in conexiones)
                        {
                            if (conexion != null)
                            {
                                Tramo tramo_prev = conexion.GetTramo(conexion.NumTramoIni);
                                Tramo tramo_next = conexion.GetTramo(conexion.NumTramoFin);
                                EstimarImpactoSwapTramosPosteriores(tramo_next, tramo_prev, false, temp_minutos_perdida_ini);
                            }
                        }
                    }
                    //Se avanza al siguiente tramo
                    tramo_inicial_posterior_afectado = tramo_inicial_posterior_afectado.Tramo_Siguiente;
                    //Se actualizan minutos de atraso propagados
                    if (tramo_inicial_posterior_afectado != null)
                    {
                        temp_minutos_perdida_ini -= Math.Max(tramo_inicial_posterior_afectado.TInicialRst - (tramo_inicial_posterior_afectado.Tramo_Previo.TFinalRst + tramo_inicial_posterior_afectado.TurnAroundMinimoOrigen), 0);
                    }
                    else
                    {
                        temp_minutos_perdida_ini = 0;
                    }
                    profundidad++;
                }

                SlotMantenimiento slotMantto;
                if (HayMantenimientoDirectoDespuesTramo(tramo_inicial_posterior_afectado,out slotMantto))
                {                   
                    int minutos_perdida_ini_mantto = Math.Max(tramo_final_cadena_recibida.TFinalRst + turn_around_tramo_temp_receptor - slotMantto.TiempoInicioManttoRst, 0);
                    if(minutos_perdida_ini_mantto>0)
                    {
                        _afecta_mantto_programado = true;
                    }
                }
            }
        }

        /// <summary>
        /// Estima el término de la holgura posterior al tramo emisor.
        /// </summary>
        /// <returns></returns>
        private int EstimarTiempoFinHolguraEnEmisor()
        {
            Tramo tramo = _tramo_fin_emisor.Tramo_Siguiente;
            if (tramo != null)
            {
                return tramo.TInicialProg - tramo.TurnAroundMinimoOrigen;
            }
            else
            {
                return _tramo_fin_emisor.TFinalProg + _tramo_fin_emisor.TurnAroundMinimoOrigen;
            }
        }        

        /// <summary>
        /// Contabiliza el impacto del swap en el itinerario en términos de tramos afectados 
        /// y minutos de atraso reducidos.
        /// </summary>
        private void EvaluarSwap()
        {
            if (SwapCumpleRestriccionDePairings())
            {
                //Se estiman ganancias sobre tramos swapeados del avión amisor
                EstimarImpactoSwapCadena(_tramo_ini_receptor, _tramo_ini_emisor, _minutos_atraso_reaccionario_inicial,_minutos_atraso_turno, false);

                //Se estiman ganancias sobre tramos swapeados del avión receptor
                if (this._tipo == TipoSwap.Normal)
                {
                    EstimarImpactoSwapCadena(_tramo_ini_emisor, _tramo_ini_receptor, 0,0, false);
                }

                //Se contabilizan las pérdidas de minutos sobre los tramos posteriores a las cadenas intercambiadas
                if (this._tipo == TipoSwap.Normal)
                {
                    EstimarImpactoSwapTramosPosteriores(_tramo_fin_receptor.Tramo_Siguiente, _tramo_fin_emisor, true, 0);
                    EstimarImpactoSwapTramosPosteriores(_tramo_fin_emisor.Tramo_Siguiente, _tramo_fin_receptor, true, 0);
                }
                else
                {
                    EstimarImpactoSwapTramosPosteriores(_tramo_ini_receptor, _tramo_fin_emisor, true, 0);
                }               
            }
            else
            {
                this._minutos_perdida = int.MaxValue;               
            }
        }

        /// <summary>
        /// Cambia las matrículas programadas de los tramos del swap.
        /// </summary>
        private void IntercambiarMatriculasProgramacion()
        {
            string id_matricula_emisor = _tramo_ini_emisor.IdAvionProgramadoActual;
            string id_matricula_receptor = _tramo_ini_receptor.IdAvionProgramadoActual;
            List<SlotBackup> slotsContenidosCadenaEmisor = GetSlotsBackup(TramoIniEmisor, TramoFinEmisor);
            List<SlotBackup> slotsContenidosCadenaReceptor = GetSlotsBackup(TramoIniReceptor, TramoFinReceptor);

            Tramo auxTramo = _tramo_ini_emisor;
            for (int i = 0; i < _num_tramos_emisor; i++)
            {
                auxTramo.IdAvionProgramadoActual = id_matricula_receptor;
                auxTramo = auxTramo.Tramo_Siguiente;
            }

            auxTramo = _tramo_ini_receptor;
            for (int i = 0; i < _num_tramos_receptor; i++)
            {
                auxTramo.IdAvionProgramadoActual = id_matricula_emisor;
                auxTramo = auxTramo.Tramo_Siguiente;
            }
            foreach (SlotBackup sb in slotsContenidosCadenaEmisor)
            {
                sb.Matricula = id_matricula_receptor;
            }
            foreach (SlotBackup sb in slotsContenidosCadenaReceptor)
            {
                sb.Matricula = id_matricula_emisor;
            }
        }

        /// <summary>
        /// Se considera un swap válido para swap si las cadenas a rotar no rompen una conexión al inicio o término de cada cadena.
        /// </summary>
        /// <returns>True si el swap es factible: no contiene pairings que se rotan</returns>
        private bool SwapCumpleRestriccionDePairings()
        {
            return (_tramo_ini_emisor.PasaRestriccionConexionParaRecovery(true)
                && _tramo_ini_receptor.PasaRestriccionConexionParaRecovery(true)
                && _tramo_fin_emisor.PasaRestriccionConexionParaRecovery(false)
                && _tramo_fin_receptor.PasaRestriccionConexionParaRecovery(false));           
        }

        #endregion

        #region INTERNAL METHODS

        /// <summary>
        /// Intercambia tramos de vuelo entre dos aviones.
        /// </summary>
        /// <param name="emisor_tramo_inicial">Tramo afectado por un atraso reaccionario</param>
        /// <param name="avion_emisor">Avión afectado por un atraso reaccionario</param>
        /// <param name="swapElegido">Swap factible de realizar</param>
        internal void AplicarSwap(Avion avion_emisor, Avion avion_receptor)
        {
            if (TramoFinEmisor != null && TramoFinEmisor.Tramo_Siguiente != null)
            {
                TramoFinEmisor.Tramo_Siguiente.TramoPostCadenaSwap = true;
            }
            if (TramoFinReceptor != null && TramoFinReceptor.Tramo_Siguiente != null)
            {
                TramoFinReceptor.Tramo_Siguiente.TramoPostCadenaSwap = true;
            }
            
            #region Insercion

            if (_num_tramos_receptor == 0)
            {
                Tramo tramo_receptor_previo = _tramo_ini_receptor.Tramo_Previo;

                if (_tramo_fin_emisor.MantenimientoPosterior != null)
                { 
                }
                //Se reasigna el tramo actual del avión emisor
                if (avion_emisor.Tramo_Actual == _tramo_ini_emisor || avion_emisor.Tramo_Actual.Tramo_Siguiente == _tramo_ini_emisor)
                {
                    avion_emisor.Tramo_Actual = _tramo_fin_emisor.Tramo_Siguiente;
                }
                else
                {
                    avion_emisor.Tramo_Actual = avion_emisor.Tramo_Actual.Tramo_Siguiente;
                }

                //Se unen los extremos de los tramos extremos del emisor
                if (_tramo_fin_emisor.Tramo_Siguiente != null)
                {
                    //Se extrae del emisor los tramos, para esto se ligan los bloques extremos.
                    _tramo_ini_emisor.Tramo_Previo.Tramo_Siguiente = _tramo_fin_emisor.Tramo_Siguiente;
                    _tramo_fin_emisor.Tramo_Siguiente.Tramo_Previo = _tramo_ini_emisor.Tramo_Previo;
                }
                else
                {
                    _tramo_ini_emisor.Tramo_Previo.Tramo_Siguiente = null;
                }

                //Se insertan los tramos en el receptor                       
                if (tramo_receptor_previo != null)
                {
                    tramo_receptor_previo.Tramo_Siguiente = _tramo_ini_emisor;
                }
                else
                {
                    avion_receptor.Tramo_Raiz = _tramo_ini_emisor;
                    avion_receptor.Tramo_Actual = _tramo_ini_emisor;
                }
                _tramo_ini_receptor.Tramo_Previo = _tramo_fin_emisor;
                _tramo_ini_emisor.Tramo_Previo = tramo_receptor_previo;
                _tramo_fin_emisor.Tramo_Siguiente = _tramo_ini_receptor;

                if (_tramo_ini_receptor == avion_receptor.Tramo_Actual && tramo_receptor_previo != null)
                {
                    avion_receptor.Tramo_Actual = _tramo_ini_emisor;
                }

                SlotMantenimiento slotPostEmisor;
                SlotMantenimiento slotPostReceptor;

                if (HayMantenimientoDirectoDespuesTramo(_tramo_fin_emisor, out slotPostEmisor))
                {
                    //Cambia referencias de mantto.
                    _tramo_fin_emisor.MantenimientoPosterior = null;
                    if (_tramo_ini_emisor.Tramo_Previo != null)
                    {
                        //Caso normal
                        _tramo_ini_emisor.Tramo_Previo.MantenimientoPosterior = slotPostEmisor;
                        slotPostEmisor.TramoPrevio = _tramo_ini_emisor.Tramo_Previo;                        
                    }
                    else
                    {
                        //Caso especial: mantto queda antes de cualquier tramo

                    }
                }
                if (HayMantenimientoDirectoDespuesTramo(_tramo_fin_receptor, out slotPostReceptor))
                {
                    //Indiferente
                }
            }

            #endregion

            #region Swap
            else
            {
                //Se reasigna el tramo actual del avión emisor
                if (avion_emisor.Tramo_Actual == _tramo_ini_emisor
                    || avion_emisor.Tramo_Actual.Tramo_Siguiente == _tramo_ini_emisor)
                    avion_emisor.Tramo_Actual = _tramo_ini_receptor;
                else avion_emisor.Tramo_Actual = avion_emisor.Tramo_Actual.Tramo_Siguiente;

                #region Swap simple: un tramo del avión receptor con una cadena del avión emisor
                if (_num_tramos_receptor == 1)
                {
                    Tramo receptor_tramo_intercambiado = _tramo_ini_receptor;
                    Tramo aux1, aux2;
                    aux1 = _tramo_ini_emisor.Tramo_Previo;
                    aux2 = receptor_tramo_intercambiado.Tramo_Previo;

                    if (aux2 != null)
                    {
                        aux2.Tramo_Siguiente = _tramo_ini_emisor;
                    }
                    else
                    {
                        avion_receptor.Tramo_Raiz = _tramo_ini_emisor;
                        avion_receptor.Tramo_Actual = _tramo_ini_emisor;
                    }
                    aux1.Tramo_Siguiente = receptor_tramo_intercambiado;
                    receptor_tramo_intercambiado.Tramo_Previo = aux1;
                    _tramo_ini_emisor.Tramo_Previo = aux2;

                    aux1 = _tramo_fin_emisor.Tramo_Siguiente;
                    aux2 = receptor_tramo_intercambiado.Tramo_Siguiente;
                    if (aux1 != null)
                    {
                        aux1.Tramo_Previo = receptor_tramo_intercambiado;
                    }
                    if (aux2 != null)
                    {
                        aux2.Tramo_Previo = _tramo_fin_emisor;
                    }
                    receptor_tramo_intercambiado.Tramo_Siguiente = aux1;
                    _tramo_fin_emisor.Tramo_Siguiente = aux2;



                    SlotMantenimiento slotPostEmisor;
                    SlotMantenimiento slotPostReceptor;
                    if (HayMantenimientoDirectoDespuesTramo(_tramo_fin_emisor, out slotPostEmisor) && HayMantenimientoDirectoDespuesTramo(_tramo_fin_receptor, out slotPostReceptor))
                    {
                        //Asigna mantto a post receptor
                        _tramo_fin_receptor.MantenimientoPosterior = slotPostEmisor;
                        _tramo_fin_receptor.MantenimientoPosterior.TramoPrevio = _tramo_fin_receptor;

                        //Asigna mantto a post emisor
                        _tramo_fin_emisor.MantenimientoPosterior = slotPostReceptor;
                        _tramo_fin_emisor.MantenimientoPosterior.TramoPrevio = _tramo_fin_emisor;
                    }

                    else if (HayMantenimientoDirectoDespuesTramo(_tramo_fin_emisor, out slotPostEmisor))
                    {
                        //Asigna mantto a post receptor
                        _tramo_fin_receptor.MantenimientoPosterior = slotPostEmisor;
                        _tramo_fin_receptor.MantenimientoPosterior.TramoPrevio = _tramo_fin_receptor;
                        //Quita mantto de emisor
                        _tramo_fin_emisor.MantenimientoPosterior = null;
                    }
                    else if (HayMantenimientoDirectoDespuesTramo(_tramo_fin_receptor, out slotPostReceptor))
                    {
                        //Asigna mantto a post emisor
                        _tramo_fin_emisor.MantenimientoPosterior = slotPostReceptor;
                        _tramo_fin_emisor.MantenimientoPosterior.TramoPrevio = _tramo_fin_emisor;
                        //Quita mantto de emisor
                        _tramo_fin_receptor.MantenimientoPosterior = null;
                    }
                }

            #endregion

                #region Swap completo: varios tramo en emisor y receptor

                else
                {
                    Tramo aux1, aux2;
                    aux1 = _tramo_ini_emisor.Tramo_Previo;
                    aux2 = _tramo_ini_receptor.Tramo_Previo;

                    if (aux2 != null)
                    {
                        aux2.Tramo_Siguiente = _tramo_ini_emisor;
                    }
                    aux1.Tramo_Siguiente = _tramo_ini_receptor;
                    _tramo_ini_receptor.Tramo_Previo = aux1;
                    _tramo_ini_emisor.Tramo_Previo = aux2;

                    aux1 = _tramo_fin_emisor.Tramo_Siguiente;
                    aux2 = _tramo_fin_receptor.Tramo_Siguiente;
                    if (aux1 != null)
                    {
                        aux1.Tramo_Previo = _tramo_fin_receptor;
                    }
                    if (aux2 != null)
                    {
                        aux2.Tramo_Previo = _tramo_fin_emisor;
                    }
                    _tramo_fin_receptor.Tramo_Siguiente = aux1;
                    _tramo_fin_emisor.Tramo_Siguiente = aux2;

                    SlotMantenimiento slotPostEmisor;
                    SlotMantenimiento slotPostReceptor;
                    if (HayMantenimientoDirectoDespuesTramo(_tramo_fin_emisor, out slotPostEmisor) && HayMantenimientoDirectoDespuesTramo(_tramo_fin_receptor, out slotPostReceptor))
                    {
                        //Asigna mantto a post receptor
                        _tramo_fin_receptor.MantenimientoPosterior = slotPostEmisor;
                        _tramo_fin_receptor.MantenimientoPosterior.TramoPrevio = _tramo_fin_receptor;

                        //Asigna mantto a post emisor
                        _tramo_fin_emisor.MantenimientoPosterior = slotPostReceptor;
                        _tramo_fin_emisor.MantenimientoPosterior.TramoPrevio = _tramo_fin_emisor;
                    }

                    else if (HayMantenimientoDirectoDespuesTramo(_tramo_fin_emisor, out slotPostEmisor))
                    {
                        //Asigna mantto a post receptor
                        _tramo_fin_receptor.MantenimientoPosterior = slotPostEmisor;
                        _tramo_fin_receptor.MantenimientoPosterior.TramoPrevio = _tramo_fin_receptor;
                        //Quita mantto de emisor
                        _tramo_fin_emisor.MantenimientoPosterior = null;
                    }
                    else if (HayMantenimientoDirectoDespuesTramo(_tramo_fin_receptor, out slotPostReceptor))
                    {
                        //Asigna mantto a post emisor
                        _tramo_fin_emisor.MantenimientoPosterior = slotPostReceptor;
                        _tramo_fin_emisor.MantenimientoPosterior.TramoPrevio = _tramo_fin_emisor;
                        //Quita mantto de emisor
                        _tramo_fin_receptor.MantenimientoPosterior = null;
                    }
                }
                #endregion

                //Se actualiza current en avion receptor
                if (_tramo_ini_receptor == avion_receptor.Tramo_Actual)
                {
                    avion_receptor.Tramo_Actual = _tramo_ini_emisor;
                }
                //Se actualiza root en avion receptor
                if (_tramo_ini_receptor == avion_receptor.Tramo_Raiz)
                {
                    avion_receptor.Tramo_Raiz = _tramo_ini_emisor;
                }
            }
            #endregion
            //Se reasigna la matrícula programada
            IntercambiarMatriculasProgramacion();
        }

        /// <summary>
        /// Indica si la instacia actual es mejor que el swap_comparado.
        /// El criterio es que cumpla tres condiciones:
        /// 1) Tenga una mejor ganacia neta que el comparado
        /// 2) Para la instancia actual, los tramos que disminuyen su atraso superen a los que la mejoran.
        /// 3) Que la ganancia neta sea mayor que los minutos de pérdida
        /// </summary>
        /// <param name="swap_elegido">Swap a comparar</param>
        /// <returns>True si el swap actual es mejor que el swap_comparado</returns>
        internal bool EsMejorQue(Swap swap_comparado)
        {
            if (this.MinutosGananciaNeta > swap_comparado.MinutosGananciaNeta
                && this._num_tramos_disminuyen_atraso > this._num_tramos_aumentan_atraso
                && this.MinutosGananciaNeta > this._minutos_perdida)
                return true;
            return false;
        }
        
        /// <summary>
        /// Estima tiempo de corte de un slot de backup en función del swap
        /// </summary>
        /// <param name="tiempo_corte_backup">Tiempo hasta donde se usa el slot de backup por concepto de utilización neta</param>
        /// <param name="tiempo_inicio_uso_backup">Tiempo desde donde se usa el slot de backup</param>
        internal void EstimarTiemposCorteBackup(out int tiempo_corte_backup, out int tiempo_inicio_uso_backup)
        {
            if (this._tipo_uso_backup == UsoBackup.IniReceptor)
            {
                tiempo_inicio_uso_backup = Math.Max(_tramo_ini_emisor.TFinProgMasTATramoPrevio, _tramo_ini_emisor.TInicialProg);
                tiempo_corte_backup = _tramo_ini_emisor.TInicialRst + _minutos_atraso_reaccionario_inicial;
            }
            else if (this._tipo_uso_backup == UsoBackup.FinReceptor)
            {
                tiempo_inicio_uso_backup = _tramo_fin_emisor.TFinalRst;//Tiempo fin de cadena receptor recuperado 
                int reaccionario_tramo_final = _minutos_atraso_reaccionario_inicial;
                int atraso_recuperado = _minutos_atraso_reaccionario_inicial + _tramo_ini_emisor.TInicialRst - Math.Max(_tramo_ini_emisor.TInicialProg, _tramo_ini_receptor.TFinRstTramoPrevio + _tramo_ini_receptor.TurnAroundMinimoOrigen);
                int holgura = 0;
                Tramo tramoAux = _tramo_ini_emisor;
                while (tramoAux != _tramo_fin_emisor)
                {
                    holgura += tramoAux.Tramo_Siguiente.TInicialRst - tramoAux.TFinalRst + tramoAux.TurnAroundMinimoDestino;
                    tramoAux = tramoAux.Tramo_Siguiente;
                }
                tiempo_corte_backup = _tramo_fin_emisor.TFinalRst + Math.Max(0, reaccionario_tramo_final - holgura);//Tiempo fin de cadena receptor sin recuperar
            }
            else if (this._tipo_uso_backup == UsoBackup.FinEmisor)
            {
                int atraso_recuperado = _tramo_ini_receptor.TInicialRst - Math.Max(_tramo_ini_receptor.TInicialProg, _tramo_ini_emisor.TFinRstTramoPrevio + _tramo_ini_emisor.TurnAroundMinimoOrigen);
                tiempo_inicio_uso_backup = _tramo_fin_receptor.TFinalRst - atraso_recuperado;//Tiempo fin de cadena emisor recuperado 
                tiempo_corte_backup = _tramo_fin_receptor.TFinalRst;//Tiempo fin de cadena emisor sin recuperar
            }
            else
            {
                tiempo_corte_backup = 0;
                tiempo_inicio_uso_backup = 0;
            }
        }

        /// <summary>
        /// Indica si hay un mantenimiento programado antes de un tramo
        /// </summary>
        /// <param name="tramo_fin">Tramo objetivo</param>
        /// <returns>True si hay mantenimiento programado</returns>
        internal bool HayMantenimientoDirectoAntesTramo(Tramo tramo_ini, out SlotMantenimiento slotMantto)
        {
            slotMantto = null;
            if (tramo_ini != null)
            {
                if (tramo_ini.Tramo_Previo != null)
                {
                    if (tramo_ini.Tramo_Previo.MantenimientoPosterior != null)
                    {
                        slotMantto = tramo_ini.Tramo_Previo.MantenimientoPosterior;
                        return true;
                    }
                }
                else if (tramo_ini.GetAvion(tramo_ini.TramoBase.Numero_Ac.ToString()).PrimerSlotEsMantenimiento)
                {
                    slotMantto = tramo_ini.GetAvion(tramo_ini.TramoBase.Numero_Ac.ToString()).SlotsMantenimiento[0];
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Indica si hay un mantenimiento programado después de un tramo
        /// </summary>
        /// <param name="tramo_fin">Tramo objetivo</param>
        /// <returns>True si hay mantenimiento programado</returns>
        internal bool HayMantenimientoDirectoDespuesTramo(Tramo tramo_fin, out SlotMantenimiento slotMantto)
        {
            slotMantto = null;
            if (tramo_fin != null)
            {
                if (tramo_fin.MantenimientoPosterior != null)
                {
                    slotMantto = tramo_fin.MantenimientoPosterior;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Indica si el swap usa un slot de backup al final de alguna de sus cadenas
        /// </summary>
        /// <param name="Itinerario">Itinerario</param>
        /// <param name="usoBackup">Tipo de uso del backup</param>
        /// <returns></returns>
        internal bool UsaSlotDeBackupAlFinal(Itinerario Itinerario, out UsoBackup usoBackup)
        {
            usoBackup = UsoBackup.NoUsa;
            if (_tramo_fin_emisor.Tramo_Siguiente != null && !Itinerario.TramoSinBackup(_tramo_fin_emisor.Tramo_Siguiente))
            {
                int tiempo_fin_recuperado = _tramo_fin_receptor.TFinalRst;
                int tiempo_ini_slot = Itinerario.TramoSinBackup_SlotBackup(_tramo_fin_emisor.Tramo_Siguiente).TiempoIniRst;
                if (tiempo_ini_slot < tiempo_fin_recuperado)
                {
                    usoBackup = UsoBackup.FinEmisor;
                    return true;
                }
            }
            if (_tramo_fin_receptor.Tramo_Siguiente != null && !Itinerario.TramoSinBackup(_tramo_fin_receptor.Tramo_Siguiente))
            {
                int tiempo_fin_recuperado = _tramo_fin_emisor.TFinalRst;
                int tiempo_ini_slot = Itinerario.TramoSinBackup_SlotBackup(_tramo_fin_receptor.Tramo_Siguiente).TiempoIniRst;
                if (tiempo_ini_slot < tiempo_fin_recuperado)
                {
                    usoBackup = UsoBackup.FinReceptor;
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region IDisposable Members

        private bool IsDisposed=false;

        /// <summary>
        /// Destructor de instancias de la clase
        /// </summary>
        ~Swap()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected void Dispose(bool Disposing)
        {
            if(!IsDisposed)
            {
                if(Disposing)
                {
 
                }
                _tramo_ini_emisor = null;
                _tramo_fin_receptor = null;
                _tramo_fin_emisor = null;
                _tramo_ini_receptor = null;
            }
            IsDisposed=true;
        }
        
        #endregion

        #region Miembros de IComparable

        public int CompareTo(object obj)
        {
            Swap comparado = (Swap)obj;
            if (this.ValorDiscriminante > comparado.ValorDiscriminante)
            {
                return 1;
            }
            else if (this.ValorDiscriminante == comparado.ValorDiscriminante)
            {
                return 0;
            }
            else return -1;
        }

        #endregion

        internal string InfoParaReporte()
        {
            StringBuilder sb = new StringBuilder();
            string tab = "\t";
            sb.Append(this.IdUnico);
            sb.Append(tab + this.TramoIniEmisor.TramoBase.Fecha_Salida.ToShortDateString());
            sb.Append(tab + this.IdAvionEmisor);
            sb.Append(tab + this.IdAvionReceptor);
            sb.Append(tab + this.TramoIniEmisor.TramoBase.Numero_Global);
            sb.Append(tab + this.TramoFinEmisor.TramoBase.Numero_Global);
            sb.Append(tab + this.TramoIniReceptor.TramoBase.Numero_Global);
            sb.Append(tab + this.TramoFinReceptor.TramoBase.Numero_Global);
            sb.Append(tab + Rotacion);
            sb.Append(tab + NumTramosEmisor);
            sb.Append(tab + NumTramosReceptor);
            sb.Append(tab + MinutosAtrasoReaccionarioInicial);
            sb.Append(tab + MinutosGanancia);
            sb.Append(tab + MinutosPerdida);
            sb.Append(tab + MinutosGananciaNeta);
            sb.Append(tab + NumTramosDisminuyenAtraso);
            sb.Append(tab + NumTramosAumentanAtraso);            
            return sb.ToString();
        }
    }
}
