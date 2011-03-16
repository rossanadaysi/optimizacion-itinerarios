using System;
using System.Collections.Generic;
using System.Text;
using InterfazSimuLAN.Reportes;
using NPOI.HSSF.UserModel;
using NPOI.HPSF;
using NPOI.POIFS.FileSystem;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using SimuLAN.Utils;

namespace InterfazSimuLAN.Reportes
{
    /// <summary>
    /// Reporte de puntualidad multiescenario por separada por negocios.
    /// </summary>
    internal class ReportePuntualidadMultiescenarioNegocio : ReportBuilder
    {
        #region CONSTANTS

        private const int NUM_ESCENARIOS = 3;

        #endregion

        #region ATRIBUTES

        /// <summary>
        /// Matriz con varianzas por negocio-estándar y escenarios simulado
        /// </summary>
        private Dictionary<string, Dictionary<int, double[,]>> _desvest;

        /// <summary>
        /// Lista con los negocios incorporados en el itinerario.
        /// </summary>
        private List<string> _negocios;

        /// <summary>
        /// Matriz con los promedios por negocio-estándar y escenarios simulado
        /// </summary>
        private Dictionary<string,Dictionary<int, double[,]>> _promedios;

        /// <summary>
        /// Lista con los estándares de puntualidad estimados.
        /// </summary>
        private List<int> _stds;

        /// <summary>
        /// Diccionario con los valores a usados para construir el reporte(string escenario,string negocio, int replica, int estándar, double puntualidad)
        /// </summary>
        private Dictionary<string, Dictionary<string, Dictionary<int, Dictionary<int, double>>>> _valores_reporte;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Construye una instancia del reporte general de puntualidad
        /// </summary>
        /// <param name="path">Ruta donde se guardará el reporte</param>
        /// <param name="filename">Nombre del reporte</param>
        /// <param name="sheetsNames">Nombre de las hojas del reporte</param>
        /// <param name="valoresReporte">Diccionario con los valores a imprimir por escenario-negocio-replica y estándar de puntualidad</param>
        public ReportePuntualidadMultiescenarioNegocio(string path, string filename, string[] sheetsNames, List<int> stds,List<string> negocios, Dictionary<string, Dictionary<string,Dictionary<int, Dictionary<int, double>>>> valoresReporte)
            : base(path, filename, sheetsNames)
        {
            this._valores_reporte = valoresReporte;
            this._primera_columna = 1;
            this._primera_fila = 3;
            this._stds = stds;
            this._negocios = negocios;
            this._promedios = new Dictionary<string, Dictionary<int, double[,]>>();
            this._desvest = new Dictionary<string, Dictionary<int, double[,]>>();
            foreach (string negocio in negocios)
            {
                _promedios.Add(negocio, new Dictionary<int, double[,]>());
                _desvest.Add(negocio, new Dictionary<int, double[,]>());
                foreach (int std in stds)
                {
                    _promedios[negocio].Add(std, new double[NUM_ESCENARIOS, NUM_ESCENARIOS]);
                    _desvest[negocio].Add(std, new double[NUM_ESCENARIOS, NUM_ESCENARIOS]);
                }
            }
            ConstruirPromedios();
            ConstruirDesvest();
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Escribe los valores en el reporte
        /// </summary>
        /// <param name="titulo">Título de cada hoja del reporte</param>
        /// <param name="colHeaders">Encabezados de las columnas</param>
        public override void CrearReporte(string titulo, bool juntaTitulos)
        {
            base.CrearReporte(titulo, juntaTitulos);
            string[] headers = {};
            base.SetSheetsHeaders(headers, 5);
            foreach (string negocio in _negocios)
            {
                foreach (int estandar in _stds)
                {
                    Sheet sheet = base.Workbook.GetSheet(negocio + "-STD" + estandar);
                    ImprimirTabla(sheet, _primera_fila, _primera_columna, "Promedio");
                    ImprimirValores(sheet, _primera_fila, _primera_columna, _promedios[negocio][estandar]);
                    ImprimirTabla(sheet, _primera_fila + 8, _primera_columna, "Desvest");
                    ImprimirValores(sheet, _primera_fila + 8, _primera_columna, _desvest[negocio][estandar]);
                    ImprimirConsolidado(sheet, _primera_fila + 5, _primera_columna, "Desvest consolidada", _promedios[negocio][estandar]);
                    sheet.SetColumnWidth(1, 4300);
                }
            }
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// LLena matriz con la desviación estándar de la puntualidad por escenario
        /// </summary>
        private void ConstruirDesvest()
        {
            foreach (string key in _valores_reporte.Keys)
            {
                string[] partes = key.Split('-');
                int indexClima = Convert.ToInt16(partes[0]);
                int indexMantto = Convert.ToInt16(partes[1]);
                foreach (string negocio in _negocios)
                {
                    int nReplicas = _valores_reporte[key][negocio].Count;
                    foreach (int std in _stds)
                    {
                        double varianza = 0;
                        if (nReplicas <= 1)
                        {
                            varianza = 0;
                        }
                        else
                        {
                            double promedio = 0;
                            for (int i = 0; i < nReplicas; i++)
                            {
                                promedio += _valores_reporte[key][negocio][i][std];
                            }
                            promedio /= nReplicas;
                            for (int i = 0; i < nReplicas; i++)
                            {
                                varianza += Math.Pow(Math.Abs(_valores_reporte[key][negocio][i][std] - promedio), 2);
                            }
                            varianza /= (nReplicas - 1);
                        }
                        _desvest[negocio][std][indexClima - 1, indexMantto - 1] = Math.Sqrt(varianza);
                    }
                }
            }
        }

        /// <summary>
        /// LLena matriz con promedio de la puntualidad por escenario
        /// </summary>
        private void ConstruirPromedios()
        {
            foreach (string key in _valores_reporte.Keys)
            {
                string[] partes = key.Split('-');
                int indexClima = Convert.ToInt16(partes[0]);
                int indexMantto = Convert.ToInt16(partes[1]);
                foreach (string negocio in _negocios)
                {
                    int nReplicas = _valores_reporte[key][negocio].Count;
                    foreach (int std in _stds)
                    {
                        double promedio = 0;
                        for (int i = 0; i < nReplicas; i++)
                        {
                            promedio += _valores_reporte[key][negocio][i][std];
                        }
                        promedio /= nReplicas;
                        _promedios[negocio][std][indexClima - 1, indexMantto - 1] = promedio;
                    }
                }
            }
        }

        /// <summary>
        /// Genera la desviación estándar consolidada del itinerario 
        /// </summary>
        /// <param name="sheet">Hoja donde se escribirá la tabla</param>
        /// <param name="row_ini">Fila inicial</param>
        /// <param name="col_ini">Columna inicial</param>
        /// <param name="titulo">Título</param>
        /// <param name="valores">Valores de la tabla</param>
        private void ImprimirConsolidado(Sheet sheet, int row_ini, int col_ini, string titulo, double[,] valores)
        {
            Cell cell = sheet.CreateRow(row_ini).CreateCell(col_ini);
            cell.CellStyle = GetEstilo(EstilosTexto.EncabezadoColumnas);
            cell.SetCellType(CellType.STRING);
            cell.SetCellValue(titulo);
            cell = sheet.GetRow(row_ini).CreateCell(col_ini + 1);
            cell.CellStyle = GetEstilo(EstilosTexto.EncabezadoColumnas);
            cell.SetCellType(CellType.STRING);
            sheet.AddMergedRegion(new CellRangeAddress(row_ini, row_ini, col_ini, col_ini + 1));

            EstadisticosGenerales estadisticos = new EstadisticosGenerales(valores);

            cell = sheet.GetRow(row_ini).CreateCell(col_ini + 2);
            cell.CellStyle = GetEstilo(EstilosTexto.PorcentajesNegrita);
            cell.SetCellType(CellType.NUMERIC);
            cell.SetCellValue(estadisticos.Desvest);
            cell = sheet.GetRow(row_ini).CreateCell(col_ini + 3);
            cell.CellStyle = GetEstilo(EstilosTexto.PorcentajesNegrita);
            cell = sheet.GetRow(row_ini).CreateCell(col_ini + 4);
            cell.CellStyle = GetEstilo(EstilosTexto.PorcentajesNegrita);

            sheet.AddMergedRegion(new CellRangeAddress(row_ini, row_ini, col_ini + 2, col_ini + 4));
        }

        /// <summary>
        /// Método genérico para imprimir la tabla multiescenario
        /// </summary>
        /// <param name="sheet">Hoja donde se escribirá la tabla</param>
        /// <param name="row_ini">Fila inicial</param>
        /// <param name="col_ini">Columna inicial</param>
        /// <param name="nombreTabla">Título de la tabla</param>
        private void ImprimirTabla(Sheet sheet, int row_ini, int col_ini, string nombreTabla)
        {
            Cell cell = sheet.CreateRow(row_ini + 1).CreateCell(col_ini + 2);
            cell.CellStyle = GetEstilo(EstilosTexto.EncabezadoColumnas);
            cell.SetCellType(CellType.STRING);
            cell.SetCellValue("Bueno");

            cell = sheet.GetRow(row_ini + 1).CreateCell(col_ini + 3);
            cell.CellStyle = GetEstilo(EstilosTexto.EncabezadoColumnas);
            cell.SetCellType(CellType.STRING);
            cell.SetCellValue("Normal");

            cell = sheet.GetRow(row_ini + 1).CreateCell(col_ini + 4);
            cell.CellStyle = GetEstilo(EstilosTexto.EncabezadoColumnas);
            cell.SetCellType(CellType.STRING);
            cell.SetCellValue("Malo");

            cell = sheet.CreateRow(row_ini + 2).CreateCell(col_ini + 1);
            cell.CellStyle = GetEstilo(EstilosTexto.EncabezadoColumnas);
            cell.SetCellType(CellType.STRING);
            cell.SetCellValue("Bueno");

            cell = sheet.CreateRow(row_ini + 3).CreateCell(col_ini + 1);
            cell.CellStyle = GetEstilo(EstilosTexto.EncabezadoColumnas);
            cell.SetCellType(CellType.STRING);
            cell.SetCellValue("Normal");

            cell = sheet.CreateRow(row_ini + 4).CreateCell(col_ini + 1);
            cell.CellStyle = GetEstilo(EstilosTexto.EncabezadoColumnas);
            cell.SetCellType(CellType.STRING);
            cell.SetCellValue("Malo");


            cell = sheet.CreateRow(row_ini).CreateCell(col_ini + 2);
            cell.CellStyle = GetEstilo(EstilosTexto.EncabezadoColumnas);
            cell.SetCellType(CellType.STRING);
            cell.SetCellValue("Escenarios Mantto");
            cell = sheet.GetRow(row_ini).CreateCell(col_ini + 3);
            cell.CellStyle = GetEstilo(EstilosTexto.EncabezadoColumnas);
            cell = sheet.GetRow(row_ini).CreateCell(col_ini + 4);
            cell.CellStyle = GetEstilo(EstilosTexto.EncabezadoColumnas);
            sheet.AddMergedRegion(new CellRangeAddress(row_ini, row_ini, col_ini + 2, col_ini + 4));

            cell = sheet.GetRow(row_ini + 2).CreateCell(col_ini);
            cell.CellStyle = GetEstilo(EstilosTexto.EncabezadoColumnas);
            cell.SetCellType(CellType.STRING);
            cell.SetCellValue("Escenarios WXS");
            cell = sheet.GetRow(row_ini + 3).CreateCell(col_ini);
            cell.CellStyle = GetEstilo(EstilosTexto.EncabezadoColumnas);
            cell = sheet.GetRow(row_ini + 4).CreateCell(col_ini);
            cell.CellStyle = GetEstilo(EstilosTexto.EncabezadoColumnas);
            sheet.AddMergedRegion(new CellRangeAddress(row_ini + 2, row_ini + 4, col_ini, col_ini));

            cell = sheet.GetRow(row_ini).CreateCell(col_ini);
            cell.CellStyle = GetEstilo(EstilosTexto.EncabezadoColumnas);
            cell.SetCellType(CellType.STRING);
            cell.SetCellValue(nombreTabla);
            cell = sheet.GetRow(row_ini + 1).CreateCell(col_ini);
            cell.CellStyle = GetEstilo(EstilosTexto.EncabezadoColumnas);
            cell = sheet.GetRow(row_ini + 1).CreateCell(col_ini + 1);
            cell.CellStyle = GetEstilo(EstilosTexto.EncabezadoColumnas);
            cell = sheet.GetRow(row_ini).CreateCell(col_ini + 1);
            cell.CellStyle = GetEstilo(EstilosTexto.EncabezadoColumnas);
            sheet.AddMergedRegion(new CellRangeAddress(row_ini, row_ini + 1, col_ini, col_ini + 1));
        }

        /// <summary>
        /// Método genérico para llenar una tabla multiescenario
        /// </summary>
        /// <param name="sheet">Hoja donde se escribirá la tabla</param>
        /// <param name="row_ini">Fila inicial</param>
        /// <param name="col_ini">Columna inicial</param>
        /// <param name="valores">Valores de la tabla</param>
        private void ImprimirValores(Sheet sheet, int row_ini, int col_ini, double[,] valores)
        {
            Cell cell;
            for (int i = 0; i < NUM_ESCENARIOS; i++)
            {
                for (int j = 0; j < NUM_ESCENARIOS; j++)
                {
                    cell = sheet.GetRow(row_ini + 2 + i).CreateCell(col_ini + 2 + j);
                    cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(valores[i, j]);
                }
            }
        }

        #endregion
    }
}
