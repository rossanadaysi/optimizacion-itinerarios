using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.IO;

namespace AnalisisDisrupciones
{
    public class ControladorExperimentos
    {
        #region ATRIBUTES
        private List<string> camposAtrasos;

        private List<string> camposCondicionantes;

        private int totalData;

        private MySqlConnection connection;

        private int agnoIni;

        private int agnoFin;

        private string campo;

        #endregion

        #region PROPERTIES
        public int AgnoIni
        {
            get { return agnoIni; }
            set { agnoIni = value; }
        }

        public int AgnoFin
        {
            get { return agnoFin; }
            set { agnoFin = value; }
        }

        public string Campo
        {
            get { return campo; }
            set { campo = value; }
        }

        public List<string> CamposAtrasos
        {
            get { return camposAtrasos; }
            set { camposAtrasos = value; }
        }

        public MySqlConnection Connection
        {
            get { return connection; }
            set { connection = value; }
        }

        public int TotalData
        {
            get { return totalData; }
            set { totalData = value; }
        }
        #endregion

        #region CONSTRUCTOR
        public ControladorExperimentos(MySqlConnection connection, string campo, int agnoIni, int agnoFin)
        {
            this.campo = campo;
            this.agnoIni = agnoIni;
            this.agnoFin = agnoFin;
            this.connection = connection;
            totalData = LeerTotalData(agnoIni, agnoFin);
            camposAtrasos = CargarCamposAtrasos(campo, totalData);
            camposCondicionantes = CargarCamposCondicionantes();
        }

        #endregion

        #region PUBLIC METHODS

        public void EjecutarTestIndependenciaGlobal(string outputPath)
        {
            connection.Open();
            for (int i = agnoFin; i >= agnoIni; i--)
            {                
                DateTime t_ini = DateTime.Now;
                List<ExperimentoIndependencia> experimentos = new List<ExperimentoIndependencia>();
                Console.WriteLine("Iniciando creación de experimentos de Independencia");
                CargarExperimentosIndependecia(experimentos,i);
                EjecutarExperimientosIndependencia(experimentos, outputPath + "ExperimentoIndependencia" + campo + "" + i + "" + agnoFin + ".xls");
                DateTime t_fin = DateTime.Now;
                double total_time = (t_fin - t_ini).TotalSeconds;
                Console.WriteLine("Tiempo de ejecución: " + total_time + "segs");             
            }
            connection.Close();
        }

        public void EjecutarTestANOVAGlobal(string outputPath)
        {
            connection.Open();
            try
            {
                camposAtrasos.Remove("RC\r");
                camposAtrasos.Remove("CONEX\r");
            }
            catch { }
            for (int i = agnoFin; i >= agnoIni; i--)
            {                
                DateTime t_ini = DateTime.Now;                
                List<ExperimentoANOVA> experimentos = new List<ExperimentoANOVA>();
                Console.WriteLine("Iniciando creación de experimentos ANOVA");
                CargarExperimentosAnova(experimentos,i);
                EjecutarExperimientosAnova(experimentos, outputPath + "ExperimentoANOVA" + campo + "" + i + "" + agnoFin + ".xls");
                DateTime t_fin = DateTime.Now;
                double total_time = (t_fin - t_ini).TotalSeconds;
                Console.WriteLine("Tiempo de ejecución: " + total_time + "segs");
                
            }
            connection.Close();
            
        }

        #endregion

        #region PRIVATE METHODS
        
        private List<string> CargarCamposCondicionantes()
        {
            List<string> campoCondicionanteRetorno = new List<string>();
            campoCondicionanteRetorno.Add("origen");
            campoCondicionanteRetorno.Add("ruta");
            campoCondicionanteRetorno.Add("flota_op");
            campoCondicionanteRetorno.Add("HOUR(STD_UTC)");
            campoCondicionanteRetorno.Add("MONTH(fecha)");
            return campoCondicionanteRetorno;
        }
        
        private List<string> CargarCamposAtrasos(string campo, int totalVuelos)
        {
            List<string> listaAtrasos = new List<string>();
            connection.Open();
            MySqlCommand command = connection.CreateCommand();
            MySqlDataReader Reader;
            command.CommandText = "SELECT distinct " + campo;
            command.CommandText += " FROM `test_vuelos_atrasos`.`causas_atraso` ";
            Reader = command.ExecuteReader();
            while (Reader.Read())
            {
                string nombreAtraso = Reader.GetValue(0).ToString();
                if (nombreAtraso != "NULL")
                {
                    listaAtrasos.Add(nombreAtraso);
                }
            }
            Reader.Close();
            connection.Close();
            return listaAtrasos;
        }

        private int LeerTotalData(int agnoIni, int agnoFin)
        {
            connection.Open();
            MySqlCommand command = connection.CreateCommand();
            MySqlDataReader Reader;
            command.CommandText = "SELECT count(distinct id_tramo) ";
            command.CommandText += "FROM `test_vuelos_atrasos`.`vuelos_atrasos` ";
            command.CommandText += "WHERE YEAR(fecha) between " + agnoIni + " and " + agnoFin + ";";
            Reader = command.ExecuteReader();
            object obj;
            if (Reader.Read())
            {
                obj = Reader.GetValue(0);
                Reader.Close();
            }
            else
            {
                obj = 0;
            }
            connection.Close();
            return Convert.ToInt32(obj);
        }

        private void CargarExperimentosIndependecia(List<ExperimentoIndependencia> experimentos,int agnoIni2)
        {
            for (int i = 0; i < camposAtrasos.Count; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    experimentos.Add(new ExperimentoIndependencia(totalData, camposAtrasos[i], camposAtrasos[j], campo, connection, agnoIni2, agnoFin));
                }
            }
        }

        private void EjecutarExperimientosAnova(List<ExperimentoANOVA> experimentos, string filename)
        {
            FileStream fs2 = new FileStream(filename, FileMode.Create);
            StreamWriter sw2 = new StreamWriter(fs2);
            foreach (ExperimentoANOVA e in experimentos)
            {
                e.EjecutarExperimiento();
                e.ImprimirExperimentoEnConsola();
                string s = e.Id_experimiento.ToString();
                s += "\t" + e.TipoDisrupcion.Substring(0, e.TipoDisrupcion.Length - 1);
                s += "\t" + e.CampoParticionante;
                s += "\t" + e.SSR;
                s += "\t" + e.SSE;
                s += "\t" + e.N;
                s += "\t" + e.K;
                s += "\t" + e.F_Calculado;
                sw2.WriteLine(s);
            }
            sw2.Close();
            fs2.Close();
        }

        private void CargarExperimentosAnova(List<ExperimentoANOVA> experimentos, int agnoIni2)
        {
            for (int i = 0; i < camposAtrasos.Count; i++)
            {
                for (int j = 0; j < camposCondicionantes.Count; j++)
                {
                    experimentos.Add(new ExperimentoANOVA(camposAtrasos[i], camposCondicionantes[j], campo, connection, agnoIni2, agnoFin));
                }
            }
        }

        private void EjecutarExperimientosIndependencia(List<ExperimentoIndependencia> experimentos, string filename)
        {
            FileStream fs2 = new FileStream(filename, FileMode.Create);
            StreamWriter sw2 = new StreamWriter(fs2);
            foreach (ExperimentoIndependencia e in experimentos)
            {
                e.EjecutarExperimiento();
                e.ImprimirExperimentoEnConsola();
                sw2.WriteLine(e.Id_experimiento + "\t" + e.TiposElementos + "\t" + e.Resultado + "\t" + e.Elementos[0, 0].Contador + "\t" + e.Elementos[0, 1].Contador + "\t" + e.Elementos[1, 0].Contador + "\t" + e.Elementos[1, 1].Contador + "\t" + e.ChiCalculado + "\t" + e.ChiSq1gl);
            }
            sw2.Close();
            fs2.Close();
        }
        #endregion
    }
}
