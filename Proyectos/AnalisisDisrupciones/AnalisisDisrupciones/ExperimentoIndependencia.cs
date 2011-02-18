using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

public enum TipoElemento { Single, Double, Null}
public enum TipoResultado {Dependiente, Independiente}

namespace AnalisisDisrupciones
{
    public class ExperimentoIndependencia
    {
        #region GLOBALS
        static double chiSq1gl = 3.841459149;
        static int n = 2;
        #endregion

        #region ATRIBUTES
        private double chiCalculado;
        private int id_experimiento;
        private int totalData;

        private string[] tiposElementos;
        private ElementoTestIndependencia[,] elementos;
        private TipoResultado resultado;
        private string resultadoString;
        #endregion

        #region PROPERTIES
        public ElementoTestIndependencia[,] Elementos
        {
            get { return elementos; }
        }

        public TipoResultado Resultado
        {
            get { return resultado; }
        }

        public string ResultadoString
        {
            get { return resultadoString; }
        }

        public string TiposElementos
        {
            get { return tiposElementos[0].Remove(tiposElementos[0].Length - 1) + "\t" + tiposElementos[1].Remove(tiposElementos[1].Length - 1); }
        }

        public double ChiCalculado
        {
            get { return chiCalculado; }
        }

        public double ChiSq1gl
        {
            get { return chiSq1gl; }
        }

        public int Id_experimiento
        {
            get { return id_experimiento; }
        }

        #endregion

        #region CONSTRUCTOR
        public ExperimentoIndependencia(int totalData, string elemento1, string elemento2,string campo, MySqlConnection connection,int agnoIni, int agnoFin)
        {
            Program.totalExperimentos++;
            Console.WriteLine("Cargando Experimiento: " + Program.totalExperimentos);
            this.id_experimiento = Program.totalExperimentos;
            this.totalData = totalData;           
            this.tiposElementos = new string[n];
            tiposElementos[0] = elemento1;
            tiposElementos[1] = elemento2;
            this.elementos = new ElementoTestIndependencia[n, n];
            CargarElementos(connection, campo, agnoIni,agnoFin);
            chiCalculado = 0;
        }
        #endregion

        #region PUBLIC METHODS
        public void CargarElementos(MySqlConnection connection, string campo,int agnoIni, int agnoFin)
        {
            Console.WriteLine("Inicio Consulta 1");
            MySqlCommand command = connection.CreateCommand();
            MySqlDataReader Reader;
            int total1 = 0;
            int total2 = 0;
            int totalDoble = 0;
            int totalDobleMalo1 = 0;
            int totalDobleMalo2 = 0;
            //carga elemento doble Elemento[1,1]
            command.CommandText =  "SELECT 'DOBLE' as clas,count(id_tramo) FROM ";
            command.CommandText += "(SELECT id_tramo,count(id_tramo) as cuentaTramo ";
            command.CommandText += "FROM `test_vuelos_atrasos`.`vuelos_atrasos`,`test_vuelos_atrasos`.`causas_atraso` ";
            command.CommandText += "WHERE YEAR(fecha) between " + agnoIni + " and " + agnoFin + " ";
            command.CommandText += "AND " + campo + " in ('" + tiposElementos[0] + "','" + tiposElementos[1] + "') ";
            command.CommandText += "AND `causas_atraso`.id = `vuelos_atrasos`.cod_atraso ";
            command.CommandText += "GROUP BY id_tramo HAVING COUNT(id_tramo)>=2 ) as tt;";
            Reader = command.ExecuteReader();
            if(Reader.Read())
            {
                totalDoble = Convert.ToInt32(Reader.GetValue(1));                
            }
            Reader.Close();
            //Obtiene elementos dobles que no corresponden
            Console.WriteLine("Inicio Consulta 2");
            command.CommandText = "SELECT count(id_tramo) FROM ";
            command.CommandText += "(SELECT id_tramo,count(clase) as cuentaTramo FROM ";
            command.CommandText += "(SELECT id_tramo, " + campo + " as clase ";
            command.CommandText += "FROM `test_vuelos_atrasos`.`vuelos_atrasos`,`test_vuelos_atrasos`.`causas_atraso` ";
            command.CommandText += "WHERE YEAR(fecha) between " + agnoIni + " and " + agnoFin + " ";
            command.CommandText += "AND " + campo + " in ('" + tiposElementos[0] + "','" + tiposElementos[1] + "') ";
            command.CommandText += "AND `causas_atraso`.id = `vuelos_atrasos`.cod_atraso ) as tt ";
            command.CommandText += "WHERE clase not in ('" + tiposElementos[1] + "') ";
            command.CommandText += "GROUP BY id_tramo HAVING COUNT(clase)>=2 ) as tt2;";
            Reader = command.ExecuteReader();
            if (Reader.Read())
            {
                totalDobleMalo1 = Convert.ToInt32(Reader.GetValue(0));
            }
            Reader.Close();
            Console.WriteLine("Inicio Consulta 3");
            command.CommandText = "SELECT count(id_tramo) FROM ";
            command.CommandText += "(SELECT id_tramo,count(clase) as cuentaTramo FROM ";
            command.CommandText += "(SELECT id_tramo, " + campo + " as clase ";
            command.CommandText += "FROM `test_vuelos_atrasos`.`vuelos_atrasos`,`test_vuelos_atrasos`.`causas_atraso` ";
            command.CommandText += "WHERE YEAR(fecha) between " + agnoIni + " and " + agnoFin + " ";
            command.CommandText += "AND " + campo + " in ('" + tiposElementos[0] + "','" + tiposElementos[1] + "') ";
            command.CommandText += "AND `causas_atraso`.id = `vuelos_atrasos`.cod_atraso ) as tt ";
            command.CommandText += "WHERE clase not in ('" + tiposElementos[0] + "') ";
            command.CommandText += "GROUP BY id_tramo HAVING COUNT(clase)>=2 ) as tt2;";
            Reader = command.ExecuteReader();
            if (Reader.Read())
            {
                totalDobleMalo2 = Convert.ToInt32(Reader.GetValue(0));
            }
            Reader.Close();
            //Calcula el valor total de la doble ocurrencia
            totalDoble = totalDoble - totalDobleMalo1 - totalDobleMalo2;
            elementos[1, 1] = new ElementoTestIndependencia("DOBLE", TipoElemento.Double, totalDoble);
            //Carga elementos simples
            Console.WriteLine("Inicio Consulta 4");
            command.CommandText = "SELECT clase, count(clase) ";
            command.CommandText += "FROM (SELECT distinct id_tramo," + campo + " as clase ";
            command.CommandText += "FROM `test_vuelos_atrasos`.`vuelos_atrasos`,`test_vuelos_atrasos`.`causas_atraso` ";
            command.CommandText += "WHERE YEAR(fecha) between " + agnoIni + " and " + agnoFin + " ";
            command.CommandText += "AND " + campo + " in ('" + tiposElementos[0] + "','" + tiposElementos[1] + "') ";
            command.CommandText += "AND `causas_atraso`.id = `vuelos_atrasos`.cod_atraso) as tt  ";
            command.CommandText += "GROUP BY clase;";
            Reader = command.ExecuteReader();
            if (Reader.Read())
            {
                total1 = Convert.ToInt32(Reader.GetValue(1)) - totalDoble;
                elementos[0, 1] = new ElementoTestIndependencia(Reader.GetValue(0).ToString(), TipoElemento.Single, total1);
            }
            if (Reader.Read())
            {
                total2 = Convert.ToInt32(Reader.GetValue(1)) - totalDoble;
                elementos[1, 0] = new ElementoTestIndependencia(Reader.GetValue(0).ToString(), TipoElemento.Single, total2);
            }
            Reader.Close();


            //Carga elemento nulo Elemento[0,0]
            int totalSinAtrasos = totalData - total1 - total2 - totalDoble;
            elementos[0, 0] = new ElementoTestIndependencia("NULO", TipoElemento.Null, totalSinAtrasos);
        }

        public void ImprimirExperimentoEnConsola()
        {            
            resultadoString ="Experimento " + id_experimiento + ": " + this.ToString();
            resultadoString += "\n\tNo Atraso " + tiposElementos[0] + "\tAtraso " + tiposElementos[0];
            resultadoString += "\nNo Atraso " + tiposElementos[1] + "\t" + elementos[0, 0].Contador + "\t" + elementos[0, 1].Contador;
            resultadoString += "\nAtraso " + tiposElementos[1] + "\t" + elementos[1, 0].Contador + "\t" + elementos[1, 1].Contador;
            resultadoString += "\nConclusion" + this.resultado.ToString();
            resultadoString += "\nChi-calculado: \t" + chiCalculado;
            resultadoString += "\nChi-comparado: \t" + chiSq1gl + "\n\n";

            Console.Write(resultadoString);
        }

        public void EjecutarExperimiento()
        {
            int[,] matrizContadores = new int[n, n];
            double[,] pCaracteristica = new double[n, n];
            
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrizContadores[i, j] = elementos[i, j].Contador;
                }
            }
            pCaracteristica[0, 0] = Convert.ToDouble(matrizContadores[0, 0] + matrizContadores[0, 1]) / totalData;
            pCaracteristica[0, 1] = Convert.ToDouble(matrizContadores[1, 0] + matrizContadores[1, 1]) / totalData;
            pCaracteristica[1, 0] = Convert.ToDouble(matrizContadores[0, 0] + matrizContadores[1, 0]) / totalData;
            pCaracteristica[1, 1] = Convert.ToDouble(matrizContadores[0, 1] + matrizContadores[1, 1]) / totalData;
            chiCalculado = CalcularChi(matrizContadores, pCaracteristica);

            if (chiCalculado > chiSq1gl)
            {
                resultado = TipoResultado.Dependiente;
            }
            else
            {
                resultado = TipoResultado.Independiente;
            }

        }

        public override string ToString()
        {
            string s = tiposElementos[0].ToString();
            for (int i = 1; i < tiposElementos.Length; i++)
            {
                s += "_" + tiposElementos[i].ToString();
            }
            return s;
        }

        #endregion

        #region PRIVATE METHODS
        private double CalcularChi(int[,] matrizContadores, double[,] pCaracteristica)
        {
            double valorChi = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    valorChi += Math.Pow(matrizContadores[i, j] - totalData * pCaracteristica[0, i] * pCaracteristica[1, j], 2) / (totalData * pCaracteristica[0, i] * pCaracteristica[1, j]);
                }
            }
            return valorChi;
        }
        #endregion
       
    }

    public class ElementoTestIndependencia
    {
        #region ATRIBUTES
        
        private string nombre;
        
        private int contador;
        
        private TipoElemento tipo;
        
        #endregion

        #region PROPERTIES
        public int Contador
        {
            get { return contador; }
            set { contador = value; }
        }
        
        public string Nombre
        {
            get { return nombre; }
            set { nombre = value; }
        }

        public TipoElemento Tipo
        {
            get { return tipo; }
            set { tipo = value; }
        }
        
        #endregion

        #region CONSTRUCTOR

        public ElementoTestIndependencia(string nombre, TipoElemento tipo,int contador)
        {
            this.nombre = nombre;
            this.tipo = tipo;
            this.contador = contador;
        }
        
        #endregion
    }
}
