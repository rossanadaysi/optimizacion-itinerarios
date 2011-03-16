using System;
using System.Collections.Generic;
using System.Text;
using NPOI.HSSF.UserModel;
using NPOI.HPSF;
using NPOI.POIFS.FileSystem;
using NPOI.SS.UserModel;
using NPOI.HSSF.Util;
using SimuLAN.Utils;

namespace InterfazSimuLAN.Reportes
{
    /// <summary>
    /// Reporte de utilización de turnos de backup
    /// </summary>
    internal class ReporteUtilizacionTurnos: ReportBuilder
    {
        #region ATRIBUTES

        /// <summary>
        /// Información por grupo de flota de la utilización de los turnos absoluta y relativa.
        /// </summary>
        public Dictionary<string, InfoReporteTurnos> _estadisticos_turnos;

        /// <summary>
        /// Encabezados de las columnas
        /// </summary>
        private string[] _headers;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Construye una instancia del reporte general de puntualidad
        /// </summary>
        /// <param name="path">Ruta donde se guardará el reporte</param>
        /// <param name="filename">Nombre del reporte</param>
        /// <param name="sheetsNames">Nombre de las hojas del reporte</param>
        /// <param name="estadisticos_turnos">Diccionario con los valores a imprimir</param>
        public ReporteUtilizacionTurnos(string path, string filename, string[] sheetsNames, Dictionary<string, InfoReporteTurnos> estadisticos_turnos, string[] headers)   
            : base(path, filename, sheetsNames)
        {
            this._headers = headers;
            this._primera_columna = 1;
            this._primera_fila = 3;
            this._estadisticos_turnos = estadisticos_turnos;
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
            base.SetSheetsHeaders(_headers, 0);
            foreach (string grupo in _estadisticos_turnos.Keys)
            {
                Sheet sheet = base.Workbook.GetSheet("Grupo " + grupo);
                int contador_filas = 0;
                foreach (DateTime fecha in _estadisticos_turnos[grupo].estadisticosPorcentajeUtilizacionManana.Keys)
                {
                    Cell cell = sheet.CreateRow(_primera_fila + contador_filas).CreateCell(_primera_columna);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                    cell.SetCellType(CellType.STRING);
                    cell.SetCellValue(fecha.ToShortDateString());

                    cell = sheet.GetRow(_primera_fila + contador_filas).CreateCell(_primera_columna + 1);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroDosDecimales);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(_estadisticos_turnos[grupo].estadisticosPromedioUtilizacionManana[fecha].Media);

                    cell = sheet.GetRow(_primera_fila + contador_filas).CreateCell(_primera_columna + 2);
                    cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(_estadisticos_turnos[grupo].estadisticosPorcentajeUtilizacionManana[fecha].Media);

                    cell = sheet.GetRow(_primera_fila + contador_filas).CreateCell(_primera_columna + 3);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroDosDecimales);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(_estadisticos_turnos[grupo].estadisticosPromedioUtilizacionTarde[fecha].Media);

                    cell = sheet.GetRow(_primera_fila + contador_filas).CreateCell(_primera_columna + 4);
                    cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(_estadisticos_turnos[grupo].estadisticosPorcentajeUtilizacionTarde[fecha].Media);

                    contador_filas++;
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Estructura utilizada para consolidar la información de las unidades de backup
    /// </summary>
    internal struct InfoReporteTurnos
    {
        #region ATRIBUTES

        /// <summary>
        /// Id del grupo flota
        /// </summary>
        public string id;

        /// <summary>
        /// Estructura con los estadísticos del promedio de utilización de los turnos de mañana
        /// </summary>
        public Dictionary<DateTime,EstadisticosGenerales> estadisticosPromedioUtilizacionManana;

        /// <summary>
        /// Estructura con los estadísticos del porcentaje de utilización de los turnos de mañana
        /// </summary>
        public Dictionary<DateTime,EstadisticosGenerales> estadisticosPorcentajeUtilizacionManana;

        /// <summary>
        /// Estructura con los estadísticos del promedio de utilización de los turnos de tarde
        /// </summary>
        public Dictionary<DateTime, EstadisticosGenerales> estadisticosPromedioUtilizacionTarde;

        /// <summary>
        /// Estructura con los estadísticos del porcentaje de utilización de los turnos de tarde
        /// </summary>
        public Dictionary<DateTime, EstadisticosGenerales> estadisticosPorcentajeUtilizacionTarde;

        /// <summary>
        /// Lista con los detalles de cada réplica por fecha para turnos de mañana y tarde.
        /// </summary>
        public List<Dictionary<DateTime,int[]>> valores_utilizacion;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Construye la estructura y camputa sus estadísticos generales.
        /// </summary>
        /// <param name="valores">Lista con los detalles de cada réplica</param>
        public InfoReporteTurnos(string id, List<Dictionary<DateTime,int[]>> valores, double[] capacidad_turnos)
        {
            this.valores_utilizacion = valores;
            this.id = id;
            this.estadisticosPromedioUtilizacionManana = new Dictionary<DateTime, EstadisticosGenerales>();
            this.estadisticosPorcentajeUtilizacionManana = new Dictionary<DateTime, EstadisticosGenerales>();
            this.estadisticosPromedioUtilizacionTarde = new Dictionary<DateTime, EstadisticosGenerales>();
            this.estadisticosPorcentajeUtilizacionTarde = new Dictionary<DateTime, EstadisticosGenerales>();
            Dictionary<DateTime, List<double>> valoresPorcentajeUtilizacionManana = new Dictionary<DateTime, List<double>>();
            Dictionary<DateTime, List<double>> valoresPromedioUtilizacionManana = new Dictionary<DateTime, List<double>>();
            Dictionary<DateTime, List<double>> valoresPorcentajeUtilizacionTarde = new Dictionary<DateTime, List<double>>();
            Dictionary<DateTime, List<double>> valoresPromedioUtilizacionTarde = new Dictionary<DateTime, List<double>>();
            foreach (Dictionary<DateTime, int[]> bu in valores)
            {
                foreach (DateTime dt in bu.Keys)
                {
                    if (!valoresPorcentajeUtilizacionManana.ContainsKey(dt))
                    {
                        valoresPorcentajeUtilizacionManana.Add(dt, new List<double>());
                        valoresPromedioUtilizacionManana.Add(dt, new List<double>());
                        valoresPorcentajeUtilizacionTarde.Add(dt, new List<double>());
                        valoresPromedioUtilizacionTarde.Add(dt, new List<double>());    
                    }
                    valoresPorcentajeUtilizacionManana[dt].Add(bu[dt][0]);
                    valoresPromedioUtilizacionManana[dt].Add(bu[dt][0] / capacidad_turnos[0]);
                    valoresPorcentajeUtilizacionTarde[dt].Add(bu[dt][1]);
                    valoresPromedioUtilizacionTarde[dt].Add(bu[dt][1] / capacidad_turnos[1]);
                }
            }
            foreach (DateTime dt in valoresPromedioUtilizacionManana.Keys)
            {
                estadisticosPorcentajeUtilizacionManana.Add(dt, new EstadisticosGenerales(valoresPorcentajeUtilizacionManana[dt]));
                estadisticosPromedioUtilizacionManana.Add(dt, new EstadisticosGenerales(valoresPromedioUtilizacionManana[dt]));
                estadisticosPorcentajeUtilizacionTarde.Add(dt, new EstadisticosGenerales(valoresPorcentajeUtilizacionTarde[dt]));
                estadisticosPromedioUtilizacionTarde.Add(dt, new EstadisticosGenerales(valoresPromedioUtilizacionTarde[dt]));
            }
        }

        #endregion
    }
}
