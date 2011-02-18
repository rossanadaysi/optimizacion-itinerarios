using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using System.IO;


namespace AnalisisDisrupciones
{
    class Program
    {
        #region GLOBALS
        public static string localHost ="localhost";
        public static string dataBaseName = "test_vuelos_atrasos";
        public static string user = "root";
        public static string password = "rltlk995";
        public static int totalExperimentos = 0;
        public static string conectionString = "SERVER=" + localHost + "; DATABASE=" + dataBaseName + ";UID=" + user + ";PASSWORD=" + password + ";";
        public static MySqlConnection connection = new MySqlConnection(conectionString);
        public static int totalVuelos = 0;
        public static string outputPath = @"C:\Users\Rodolfo\PUC\postgrado\Tesis\AnalisisDisrupciones\Output Proyecto C#\";

        #endregion
        
        
        static void Main(string[] args)
        {

            string campo = "clas2";
            int agnoIni = 2005;
            int agnoFin = 2008;
            ControladorExperimentos controlador = new ControladorExperimentos(connection, campo, agnoIni, agnoFin);
            //controlador.EjecutarTestIndependenciaGlobal(outputPath);
            controlador.EjecutarTestANOVAGlobal(outputPath);
            Console.ReadLine();
        }

    }
}
