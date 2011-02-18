using System;
using System.Collections.Generic;
using System.Text;
using SimuLAN.Clases;
using NPOI.HSSF.UserModel;
using NPOI.HPSF;
using NPOI.POIFS.FileSystem;
using NPOI.SS.UserModel;
using NPOI.HSSF.Util;
using SimuLAN.Clases.Recovery;

namespace InterfazSimuLAN.Reportes
{
    /// <summary>
    /// Reporte de detalles de recovery
    /// </summary>
    internal class ReporteRecovery: ReportBuilder
    {
        #region ATRIBUTES

        private Dictionary<int, List<Swap>> _lista_swaps;

        /// <summary>
        /// Encabezados de las columnas
        /// </summary>
        private string[] _headers;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Construye una instancia del reporte de detalles de recvery
        /// </summary>
        /// <param name="path">Ruta donde se guardará el reporte</param>
        /// <param name="filename">Nombre del reporte</param>
        /// <param name="sheetsNames">Nombre de las hojas del reporte</param>
        /// <param name="valoresReporte">Diccionario con los valores a imprimir</param>
        public ReporteRecovery(string path, string filename, string[] sheetsNames, Dictionary<int, List<Swap>> valoresReporte, string[] headers)        
            : base(path, filename, sheetsNames)
        {
            this._headers = headers;
            this._lista_swaps = valoresReporte;
            this._primera_columna = 1;
            this._primera_fila = 3;
            this._headers = headers;
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Escribe los valores en el reporte
        /// </summary>
        /// <param name="titulo">Título de cada hoja del reporte</param>
        /// <param name="colHeaders">Encabezados de las columnas</param>
        public override void CrearReporte(string titulo, bool junta_nombres_sheets_con_titulo)
        {
            base.CrearReporte(titulo, junta_nombres_sheets_con_titulo);
            base.SetSheetsHeaders(_headers, -15);
            int contadorRow = 0;
            string nombreHoja = "Reporte Detalles Recovery";
            Sheet sheet = base.Workbook.GetSheet(nombreHoja);
            foreach (int replica in _lista_swaps.Keys)
            {
                foreach (Swap s in _lista_swaps[replica])
                {
                    int col = _primera_columna;
                    Cell cell = sheet.CreateRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(contadorRow + 1);
                    col++;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(replica);
                    col++;
                    
                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                    cell.SetCellType(CellType.STRING);
                    cell.SetCellValue(s.TramoIniEmisor.DtIniProg.ToShortDateString());
                    col++;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(s.IdAvionEmisor);
                    col++;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(s.IdAvionReceptor);
                    col++;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(s.TramoIniEmisor.FlotaProgramada);
                    col++;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(s.TramoIniReceptor.FlotaProgramada);
                    col++;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(s.TramoIniEmisor.GetAvion(s.IdAvionEmisor).SubFlota);
                    col++;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(s.TramoIniReceptor.GetAvion(s.IdAvionReceptor).SubFlota);
                    col++;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                    cell.SetCellType(CellType.STRING);
                    string origen = s.TramoIniEmisor.TramoBase.Origen;
                    string destino =  s.TramoFinEmisor.TramoBase.Destino;
                    string rotacion = (origen == destino) ? origen : (origen + "-" + destino);
                    cell.SetCellValue(rotacion);
                    col++;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(s.NumTramosEmisor);
                    col++;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(s.NumTramosReceptor);
                    col++;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(s.MinutosAtrasoReaccionarioInicial);
                    col++;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(s.MinutosGanancia);
                    col++;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(s.MinutosPerdida);
                    col++;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(s.MinutosGananciaNeta);
                    col++;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(s.NumTramosDisminuyenAtraso);
                    col++;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(s.NumTramosAumentanAtraso);
                    col++;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(s.UsaBackup);
                    col++;

                    contadorRow++;
                }
            }
        }

        #endregion

    }
}
