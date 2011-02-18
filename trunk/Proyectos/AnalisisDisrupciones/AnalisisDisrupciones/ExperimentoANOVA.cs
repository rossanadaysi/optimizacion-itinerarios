using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace AnalisisDisrupciones
{
    public class ExperimentoANOVA
    {
        #region GLOBALS
        public const int MIN_VALUE = 1;
        public const int MAX_VALUE = 900;
        #endregion

        #region ATRIBUTES
        private int n;
        private int k;
        private string campoParticionante;
        private string tipoDisrupcion;
        private double f_Calculado;
        private int id_experimiento;
        private string resultadoString;
        private Dictionary<string, double[]> dataAgrupada;
        private Dictionary<string, List<int>> dataTotal;
        private double ssr;
        private double sse;
        #endregion

        #region PROPERTIES

        public string CampoParticionante
        {
            get { return campoParticionante; }
        }

        public double F_Calculado
        {
            get { return f_Calculado; }
        }
        
        public double SSR
        {
            get { return ssr; }
        }

        public double SSE
        {
            get { return sse; }
        }
        public int Id_experimiento
        {
            get { return id_experimiento; }
        }
        
        public int K
        {
            get { return k; }
        }
        
        public int N
        {
            get { return n; }
        }

        public string ResultadoString
        {
            get { return resultadoString; }
        }

        public string TipoDisrupcion
        {
            get { return tipoDisrupcion; }
        }
        
        #endregion

        #region CONSTRUCTOR
        
        public ExperimentoANOVA(string tipoDisrupcion, string campoParticionante, string campo, MySqlConnection connection, int agnoIni, int agnoFin)
        {
            Program.totalExperimentos++;
            Console.WriteLine("Cargando Experimiento: " + Program.totalExperimentos);
            this.id_experimiento = Program.totalExperimentos;
            this.campoParticionante = campoParticionante;
            this.tipoDisrupcion = tipoDisrupcion;
            this.dataAgrupada = new Dictionary<string, double[]>();
            this.dataTotal = new Dictionary<string, List<int>>();
            this.n = 0;
            this.k = 0;
            this.ssr = 0;
            this.sse = 0;
            CargarElementos(connection,campo, agnoIni,agnoFin);
            f_Calculado = 0;
        }
        
        #endregion

        #region PUBLIC METHODS

        public void ImprimirExperimentoEnConsola()
        {            
            resultadoString ="\nExperimento " + id_experimiento;
            resultadoString += "\nDisrupcion: " + tipoDisrupcion.Substring(0, tipoDisrupcion.Length - 1);
            resultadoString += "\nParticion: " + campoParticionante;
            resultadoString += "\nN: " + n;
            resultadoString += "\nK: " + k;
            resultadoString += "\nSSR: " + ssr;
            resultadoString += "\nSSE: " + sse;
            resultadoString += "\nF: " + f_Calculado +"\n\n";
            Console.Write(resultadoString);
        }

        public void EjecutarExperimiento()
        {
            double mediaGlobal = 0;
            double mediaSimple = 0;
            double contador = 0;
            mediaGlobal = EstimarMediaPonderada(dataAgrupada);
            foreach (string key in dataAgrupada.Keys)
            {
                if (dataAgrupada[key][2] > 10)
                {
                    ssr += Math.Pow(dataAgrupada[key][0] - mediaGlobal, 2) * dataAgrupada[key][2];
                    contador += dataAgrupada[key][2];
                    foreach (int valor in dataTotal[key])
                    {
                        sse += Math.Pow(dataAgrupada[key][0] - valor, 2);                        
                    }
                }            
            }

            f_Calculado = (SSR / k) / (SSE / (n - k - 1));
        }

       

        public override string ToString()
        {
            return tipoDisrupcion.Substring(0, tipoDisrupcion.Length - 1) + " - " + campoParticionante;
        }

        #endregion

        #region PRIVATE METHODS
        
        private void CargarElementos(MySqlConnection connection,string campo, int agnoIni, int agnoFin)
        {
            Console.WriteLine("Inicio Consulta 1");
            MySqlCommand command = connection.CreateCommand();
            MySqlDataReader Reader;
            command.CommandText = "SELECT " + campoParticionante + " ,avg(min_atraso) as promedio, std(min_atraso) as desvest, ";
            command.CommandText += "count(" + campoParticionante + ") as contador ";
            command.CommandText += "FROM `test_vuelos_atrasos`.`vuelos_atrasos`,`test_vuelos_atrasos`.`causas_atraso` ";
            command.CommandText += "WHERE `causas_atraso`.id = `vuelos_atrasos`.cod_atraso ";
            command.CommandText += "and " + campo + " ='" + tipoDisrupcion + "' ";
            command.CommandText += "and min_atraso between " + ExperimentoANOVA.MIN_VALUE + " and " + ExperimentoANOVA.MAX_VALUE + " ";
            command.CommandText += "and YEAR(fecha) between + " + agnoIni + " and " + agnoFin + " ";
            command.CommandText += "GROUP BY " + campoParticionante + " ";
            command.CommandText += "ORDER BY count(" + campoParticionante + ");";

            Reader = command.ExecuteReader();
            while (Reader.Read())
            {
                string id = Reader.GetValue(0).ToString();
                double[] valores = new double[3];
                valores[0] = Convert.ToDouble(Reader.GetValue(1));
                valores[1] = Convert.ToDouble(Reader.GetValue(2));
                valores[2] = Convert.ToDouble(Reader.GetValue(3));
                dataAgrupada.Add(id, valores);
                k++;
            }
            Reader.Close();
           
            Console.WriteLine("Inicio Consulta 2");
            command.CommandText = "SELECT " + campoParticionante + " ,min_atraso ";
            command.CommandText += "FROM `test_vuelos_atrasos`.`vuelos_atrasos`,`test_vuelos_atrasos`.`causas_atraso` ";
            command.CommandText += "WHERE `causas_atraso`.id = `vuelos_atrasos`.cod_atraso ";
            command.CommandText += "and " + campo + " ='" + tipoDisrupcion + "' ";
            command.CommandText += "and min_atraso between " + ExperimentoANOVA.MIN_VALUE + " and " + ExperimentoANOVA.MAX_VALUE + " ";
            command.CommandText += "and YEAR(fecha) between + " + agnoIni + " and " + agnoFin + " ";
            command.CommandText += "ORDER BY " + campoParticionante + ";";

            Reader = command.ExecuteReader();
            while (Reader.Read())
            {
                string id = Reader.GetValue(0).ToString();
                int valor = Convert.ToInt32(Reader.GetValue(1));
                if (!dataTotal.ContainsKey(id))
                {
                    dataTotal.Add(id, new List<int>());
                }
                dataTotal[id].Add(valor);
                n++;
            }
            Reader.Close();
        }

        private double EstimarMediaPonderada(Dictionary<string, double[]> data)
        {
            double retorno = 0;
            double contador = 0;
            double sumaParcial = 0;
            foreach (string key in dataAgrupada.Keys)
            {
                if (dataAgrupada[key][2] > 10)
                {
                    double pi = data[key][0];
                    double ni = data[key][2];
                    sumaParcial += ni * pi;
                    contador += ni;
                }
            }
            retorno = sumaParcial / contador;
            return retorno;
        }

        #endregion
    }
}
