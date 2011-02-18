using System;
using System.Collections.Generic;
using System.Text;
using NPOI.HSSF.UserModel;
using NPOI.HPSF;
using NPOI.POIFS.FileSystem;
using NPOI.SS.UserModel;
using NPOI.HSSF.Util;
using SimuLAN.Clases;

namespace InterfazSimuLAN.Reportes
{
    /// <summary>
    /// Reporte de explicación de la impuntualidad general.
    /// </summary>
    internal class ReporteExplicacionImpuntualidadGeneral: ReportBuilder
    {
        #region ATRIBUTES

        /// <summary>
        /// Diccionario con los valores a imprimir en el reporte. key1: replica, key2: estándar, 
        /// key3: tipo disrupcion, valor: porcentaje de la impuntualidad explicada por la disrupción.
        /// </summary>
        private Dictionary<int, Dictionary<int, Dictionary<TipoDisrupcion, double>>> _valores_reporte;

        private string[] _headers;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Construye una instancia del reporte general de puntualidad
        /// </summary>
        /// <param name="path">Ruta donde se guardará el reporte</param>
        /// <param name="filename">Nombre del reporte</param>
        /// <param name="sheetsNames">Nombre de las hojas del reporte</param>
        /// <param name="valoresReporte">Diccionario con los valores a imprimir</param>
        public ReporteExplicacionImpuntualidadGeneral(string path, string filename, string[] sheetsNames, Dictionary<int, Dictionary<int,Dictionary<TipoDisrupcion, double>>> valoresReporte, string[] headers)
            : base(path, filename, sheetsNames)
        {
            this._headers = headers;
            this._valores_reporte = valoresReporte;
            this._primera_columna = 1;
            this._primera_fila = 3;
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
            int contadorReplica = 0;
            foreach (int replica in _valores_reporte.Keys)
            {
                foreach (int estandar in _valores_reporte[replica].Keys)
                {
                    Sheet sheet = base.Workbook.GetSheet("STD" + estandar);
                    int col = 0;
                    Cell cell = sheet.CreateRow(_primera_fila + contadorReplica).CreateCell(_primera_columna);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(replica + 1);
                    foreach (TipoDisrupcion tipo in _valores_reporte[replica][estandar].Keys)
                    {
                        col++;
                        cell = sheet.GetRow(_primera_fila + contadorReplica).CreateCell(_primera_columna + col);
                        cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                        cell.SetCellType(CellType.NUMERIC);
                        cell.SetCellValue(_valores_reporte[replica][estandar][tipo]);
                    }
                }
                contadorReplica++;
            }
        }

        #endregion

    }
}
