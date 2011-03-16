using System;
using System.Collections.Generic;
using System.Text;
using NPOI.HSSF.UserModel;
using NPOI.HPSF;
using NPOI.POIFS.FileSystem;
using NPOI.SS.UserModel;
using NPOI.HSSF.Util;
using SimuLAN.Clases;
using SimuLAN.Utils;

namespace InterfazSimuLAN.Reportes
{
    /// <summary>
    /// Reporte de puntualidad general
    /// </summary>
    internal class ReportePuntualidadGeneral:ReportBuilder
    {
        #region ATRIBUTES

        /// <summary>
        /// Encabezados de las columnas del reporte.
        /// </summary>
        private string[] _headers;

        /// <summary>
        /// Detalles de la simulación por réplica.
        /// </summary>
        private List<Simulacion> _info_simulacion;

        /// <summary>
        /// Lista de los negocios considerados en la simulación
        /// </summary>
        private List<string> _negocios;

        /// <summary>
        /// Diccionario con los valores a ser impresos en el reporte (int replica, int estándar, double puntualidad)
        /// </summary>
        private Dictionary<int, Dictionary<int, double>> _valores_reporte_general;

        /// <summary>
        /// Diccionario con los valores a ser impresos en el reporte (int replica, int estándar, double puntualidad)
        /// </summary>
        private Dictionary<string,Dictionary<int, Dictionary<int, double>>> _valores_reporte_negocios;
        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Construye una instancia del reporte general de puntualidad
        /// </summary>
        /// <param name="path">Ruta donde se guardará el reporte</param>
        /// <param name="filename">Nombre del reporte</param>
        /// <param name="sheetsNames">Nombre de las hojas del reporte</param>
        /// <param name="infoSimulacion">Detalles de la simulación por réplica.</param>
        /// <param name="negocios">Lista de los negocios considerados en la simulación</param>
        public ReportePuntualidadGeneral(string path, string filename, string[] sheetsNames, List<Simulacion> infoSimulacion, string[] headers, List<string> negocios)
            : base(path, filename, sheetsNames)
        {
            this._headers = headers;
            this._info_simulacion = infoSimulacion;
            this._negocios = negocios;
            this._primera_columna = 1;
            this._primera_fila = 3;
            this._valores_reporte_general = ObtenerPuntualidadGeneralPorReplica();
            this._valores_reporte_negocios = ObtenerPuntualidadNegociosPorReplica();
           
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Escribe los valores en el reporte
        /// </summary>
        /// <param name="titulo">Título de cada hoja del reporte</param>
        /// <param name="colHeaders">Encabezados de las columnas</param>
        public override void CrearReporte(string titulo,bool juntaTitulos)
        {
            base.CrearReporte(titulo, juntaTitulos);
            base.SetSheetsHeaders(_headers, 0);            
            ImprimirValores("Puntualidad General", _valores_reporte_general);
            foreach (string negocio in _negocios)
            {
                ImprimirValores("Puntualidad " + negocio, _valores_reporte_negocios[negocio]);
            }
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Crea un resumen estadísticos por estándar de puntualidad
        /// </summary>
        /// <param name="fila_ini">Número de fila inicial de la tabla</param>
        /// <param name="col_ini">Número de columna inicial de la tabla</param>
        /// <param name="puntualidad_por_estandar">Diccionario con la puntualidad agrupada por estándar</param>
        /// <param name="ultima_fila_creada">Última fila creada al momento de imprimirse los valores de puntualidad por réplica</param>
        /// <param name="sheet">Referencia a sheet donde se imprimen los valores</param>
        private void ImprimirResumen(int fila_ini, int col_ini, Dictionary<int, List<double>> puntualidad_por_estandar, int ultima_fila_creada, Sheet sheet)
        {
            Cell cell2;
            if (fila_ini > ultima_fila_creada)
            {
                cell2 = sheet.CreateRow(fila_ini).CreateCell(col_ini);
            }
            else
            {
                cell2 = sheet.GetRow(fila_ini).CreateCell(col_ini);
            }
            cell2.CellStyle = GetEstilo(EstilosTexto.EncabezadoColumnas);
            cell2.SetCellType(CellType.STRING);
            cell2.SetCellValue("Estadísticos");
            int index_col = 0;
            foreach (int std in puntualidad_por_estandar.Keys)
            {
                index_col++;
                if (fila_ini > ultima_fila_creada)
                {
                    cell2 = sheet.CreateRow(fila_ini).CreateCell(col_ini + index_col);
                }
                else
                {
                    cell2 = sheet.GetRow(fila_ini).CreateCell(col_ini + index_col);
                }
                cell2.CellStyle = GetEstilo(EstilosTexto.EncabezadoColumnas);
                cell2.SetCellType(CellType.STRING);
                cell2.SetCellValue("STD" + std);
            }

            int index_fila = fila_ini + 1;

            if (index_fila > ultima_fila_creada)
            {
                cell2 = sheet.CreateRow(index_fila).CreateCell(col_ini);
            }
            else
            {
                cell2 = sheet.GetRow(index_fila).CreateCell(col_ini);
            }
            cell2.CellStyle = GetEstilo(EstilosTexto.EncabezadoFila);
            cell2.SetCellType(CellType.STRING);
            cell2.SetCellValue("Promedio");
            index_fila++;

            if (index_fila > ultima_fila_creada)
            {
                cell2 = sheet.CreateRow(index_fila).CreateCell(col_ini);
            }
            else
            {
                cell2 = sheet.GetRow(index_fila).CreateCell(col_ini);
            }
            cell2.CellStyle = GetEstilo(EstilosTexto.EncabezadoFila);
            cell2.SetCellType(CellType.STRING);
            cell2.SetCellValue("Desvest");
            index_fila++;

            if (index_fila > ultima_fila_creada)
            {
                cell2 = sheet.CreateRow(index_fila).CreateCell(col_ini);
            }
            else
            {
                cell2 = sheet.GetRow(index_fila).CreateCell(col_ini);
            }
            cell2.CellStyle = GetEstilo(EstilosTexto.EncabezadoFila);
            cell2.SetCellType(CellType.STRING);
            cell2.SetCellValue("Mínimo");
            index_fila++;

            if (index_fila > ultima_fila_creada)
            {
                cell2 = sheet.CreateRow(index_fila).CreateCell(col_ini);
            }
            else
            {
                cell2 = sheet.GetRow(index_fila).CreateCell(col_ini);
            }
            cell2.CellStyle = GetEstilo(EstilosTexto.EncabezadoFila);
            cell2.SetCellType(CellType.STRING);
            cell2.SetCellValue("Máximo");
            index_fila++;

            if (index_fila > ultima_fila_creada)
            {
                cell2 = sheet.CreateRow(index_fila).CreateCell(col_ini);
            }
            else
            {
                cell2 = sheet.GetRow(index_fila).CreateCell(col_ini);
            }
            cell2.CellStyle = GetEstilo(EstilosTexto.EncabezadoFila);
            cell2.SetCellType(CellType.STRING);
            cell2.SetCellValue("Intervalo-");
            index_fila++;

            if (index_fila > ultima_fila_creada)
            {
                cell2 = sheet.CreateRow(index_fila).CreateCell(col_ini);
            }
            else
            {
                cell2 = sheet.GetRow(index_fila).CreateCell(col_ini);
            }
            cell2.CellStyle = GetEstilo(EstilosTexto.EncabezadoFila);
            cell2.SetCellType(CellType.STRING);
            cell2.SetCellValue("Intervalo+");
            index_fila++;

            int contador = 0;
            foreach (int std in puntualidad_por_estandar.Keys)
            {
                contador++;
                index_fila = fila_ini + 1;
                EstadisticosGenerales estadisticos = new EstadisticosGenerales(puntualidad_por_estandar[std]);

                cell2 = sheet.GetRow(index_fila).CreateCell(col_ini + contador);
                cell2.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                cell2.SetCellType(CellType.NUMERIC);
                cell2.SetCellValue(estadisticos.Media);
                index_fila++;

                cell2 = sheet.GetRow(index_fila).CreateCell(col_ini + contador);
                cell2.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                cell2.SetCellType(CellType.NUMERIC);
                cell2.SetCellValue(estadisticos.Desvest);
                index_fila++;

                cell2 = sheet.GetRow(index_fila).CreateCell(col_ini + contador);
                cell2.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                cell2.SetCellType(CellType.NUMERIC);
                cell2.SetCellValue(estadisticos.Min);
                index_fila++;

                cell2 = sheet.GetRow(index_fila).CreateCell(col_ini + contador);
                cell2.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                cell2.SetCellType(CellType.NUMERIC);
                cell2.SetCellValue(estadisticos.Max);
                index_fila++;

                cell2 = sheet.GetRow(index_fila).CreateCell(col_ini + contador);
                cell2.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                cell2.SetCellType(CellType.NUMERIC);
                int N = Convert.ToInt32(estadisticos.N);
                string formula1 = "INDIRECT(ADDRESS(" + (fila_ini + 2) + "," + (col_ini + contador + 1) + "))";
                string formula2 = "INDIRECT(ADDRESS(" + (fila_ini + 3) + "," + (col_ini + contador + 1) + "))" + "/SQRT(" + N + ")* TINV(" + ReportManager.CONFIANZA + "," + (N - 1) + ")";
                cell2.CellFormula = "Max(" + formula1 + "-" + formula2 + ",0)";
                index_fila++;

                cell2 = sheet.GetRow(index_fila).CreateCell(col_ini + contador);
                cell2.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                cell2.SetCellType(CellType.NUMERIC);
                cell2.CellFormula = "Min(" + formula1 + "+" + formula2 + ",1)";
                index_fila++;
            }
        }

        /// <summary>
        /// Imprime de manera genérica la puntualidad por réplicas para cierta agrupación, ya sea general o por negocios.
        /// </summary>
        /// <param name="nombre_hoja">Nombre de Sheet.</param>
        /// <param name="valores_reporte">Valores a imprimir, por réplica y estándar.</param>
        private void ImprimirValores(string nombre_hoja, Dictionary<int, Dictionary<int, double>> valores_reporte)
        {
            int contadorReplica = 0;
            Sheet sheet = base.Workbook.GetSheet(nombre_hoja);
            Dictionary<int, List<double>> puntualidad_por_estandar = new Dictionary<int, List<double>>();
            foreach (int replica in valores_reporte.Keys)
            {
                int col = 0;
                Cell cell = sheet.CreateRow(_primera_fila + contadorReplica).CreateCell(_primera_columna);
                cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                cell.SetCellType(CellType.NUMERIC);
                cell.SetCellValue(replica);
                col++;
                foreach (int estandar in valores_reporte[replica].Keys)
                {
                    if (!puntualidad_por_estandar.ContainsKey(estandar))
                    {
                        puntualidad_por_estandar.Add(estandar, new List<double>());
                    }
                    puntualidad_por_estandar[estandar].Add(valores_reporte[replica][estandar]);
                    cell = sheet.GetRow(_primera_fila + contadorReplica).CreateCell(_primera_columna + col);
                    cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(valores_reporte[replica][estandar]);
                    col++;
                }
                contadorReplica++;
            }
            ImprimirResumen(2, _primera_columna + puntualidad_por_estandar.Count + 2, puntualidad_por_estandar, _primera_fila + contadorReplica - 1, sheet);

        }
        
        /// <summary>
        /// Genera diccionario con las puntualidades generales por réplica y std.
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, Dictionary<int, double>> ObtenerPuntualidadGeneralPorReplica()
        {
            Dictionary<int, Dictionary<int, double>> puntualidadPorReplica = new Dictionary<int, Dictionary<int, double>>();
            int contador = 1;
            foreach (Simulacion s in _info_simulacion)
            {
                puntualidadPorReplica.Add(contador, s.StdCalculado);
                contador++;
            }
            return puntualidadPorReplica;
        }

        /// <summary>
        /// Genera diccionario con las puntualidades por negocio, réplica y std.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, Dictionary<int, Dictionary<int, double>>> ObtenerPuntualidadNegociosPorReplica()
        {
            Dictionary<string, Dictionary<int, Dictionary<int, double>>> puntualidadNegociosPorReplica = new Dictionary<string, Dictionary<int, Dictionary<int, double>>>();
            foreach (string negocio in _negocios)
            {
                puntualidadNegociosPorReplica.Add(negocio, new Dictionary<int, Dictionary<int, double>>());
            }
            int contador = 1;
            foreach (Simulacion s in _info_simulacion)
            {
                foreach (string negocio in _negocios)
                {
                    puntualidadNegociosPorReplica[negocio].Add(contador, s.EstimarPuntualidadNegocio(negocio));
                }
                contador++;
            }
            return puntualidadNegociosPorReplica;
        }

        #endregion
    }
}
