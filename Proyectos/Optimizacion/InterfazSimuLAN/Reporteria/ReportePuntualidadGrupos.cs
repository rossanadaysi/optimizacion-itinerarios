using System;
using System.Collections.Generic;
using System.Text;
using NPOI.HSSF.UserModel;
using NPOI.HPSF;
using NPOI.POIFS.FileSystem;
using NPOI.SS.UserModel;
using NPOI.SS.Util;

namespace InterfazSimuLAN.Reportes
{
    /// <summary>
    /// Reporte de puntualidad por grupos
    /// </summary>
    internal class ReportePuntualidadGrupos:ReportBuilder
    {
        #region ATRIBUTES

        /// <summary>
        /// Diccionario con los valores a ser impresos en el reporte (string nombreHoja, estructura con los valores a imprimir)
        /// </summary>
        private Dictionary<GruposReporte, PuntualidadAgrupada> _valores_reporte;
        
        /// <summary>
        /// Lista con los estándares de puntualidad estimados
        /// </summary>
        private List<int> _stds;

        /// <summary>
        /// Encabezados de las columnas
        /// </summary>
        private string[] _headers;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Construye una instancia del reporte de puntualidad por grupos
        /// </summary>
        /// <param name="path">Ruta donde se guardará el reporte</param>
        /// <param name="filename">Nombre del reporte</param>
        /// <param name="sheetsNames">Nombre de las hojas del reporte</param>
        /// <param name="valoresReporte">Diccionario con los valores a imprimir</param>
        /// <param name="valoresReporte">Lista con los estándares calculados</param>
        public ReportePuntualidadGrupos(string path, string filename, string[] sheetsNames, Dictionary<GruposReporte, PuntualidadAgrupada> valoresReporte, List<int> stds, string[] headers)
            : base(path, filename, sheetsNames)
        {
            this._headers = headers;
            this._valores_reporte = valoresReporte;
            this._stds = stds;
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
            base.SetSheetsHeaders(_headers, 0);            
            foreach (GruposReporte grupo in _valores_reporte.Keys)
            {
                foreach (int std in _stds)
                {
                    string nombreHoja = grupo.ToString() + " - STD" + std;
                    int contadorRow = 0;
                    Sheet sheet = base.Workbook.GetSheet(nombreHoja);
                    foreach (string nombre in _valores_reporte[grupo].Estadisticos.Keys)
                    {
                        int col = _primera_columna;
                        Cell cell = sheet.CreateRow(_primera_fila + contadorRow).CreateCell(col);
                        cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                        cell.SetCellType(CellType.NUMERIC);
                        cell.SetCellValue(contadorRow + 1);
                        col++;

                        cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                        cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                        cell.SetCellType(CellType.STRING);
                        cell.SetCellValue(nombre);
                        col++;

                        cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                        cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                        cell.SetCellType(CellType.NUMERIC);
                        cell.SetCellValue(_valores_reporte[grupo].ContadorTotalesPorGrupo[nombre]);
                        col++;

                        cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                        cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                        cell.SetCellType(CellType.NUMERIC);
                        cell.SetCellValue(_valores_reporte[grupo].Estadisticos[nombre][std].Media);
                        col++;

                        cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                        cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                        cell.SetCellType(CellType.NUMERIC);
                        cell.SetCellValue(_valores_reporte[grupo].Estadisticos[nombre][std].Desvest);
                        col++;

                        cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                        cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                        cell.SetCellType(CellType.NUMERIC);
                        cell.SetCellValue(_valores_reporte[grupo].Estadisticos[nombre][std].Min);
                        col++;

                        cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                        cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                        cell.SetCellType(CellType.NUMERIC);
                        cell.SetCellValue(_valores_reporte[grupo].Estadisticos[nombre][std].Max);
                        col++;

                        cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                        cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                        cell.SetCellType(CellType.NUMERIC);
                        int numeroFila = contadorRow + 1 + _primera_fila;
                        int N = Convert.ToInt32(_valores_reporte[grupo].Estadisticos[nombre][std].N);
                        string formula1 = "E" + numeroFila;
                        string formula2 = "F" + numeroFila + "/SQRT(" + N + ")* TINV(" + ReportManager.CONFIANZA + "," + (N - 1) + ")";
                        cell.CellFormula = "Max(" + formula1 + "-" + formula2 + ",0)";
                        col++;

                        cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                        cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                        cell.SetCellType(CellType.NUMERIC);
                        cell.CellFormula = "Min(" + formula1 + " + " + formula2 + ",1)";
                        col++;
                        contadorRow++;
                    }
                }            
            }

        }

        #endregion
    }
}
