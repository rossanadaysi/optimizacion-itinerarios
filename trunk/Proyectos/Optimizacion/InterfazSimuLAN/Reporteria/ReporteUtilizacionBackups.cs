using System;
using System.Collections.Generic;
using System.Text;
using SimuLAN.Clases;
using NPOI.HSSF.UserModel;
using NPOI.HPSF;
using NPOI.POIFS.FileSystem;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using SimuLAN.Clases.Recovery;

namespace InterfazSimuLAN.Reportes
{
    /// <summary>
    /// Reporte de utilización de aviones de backup
    /// </summary>
    internal class ReporteUtilizacionBackups: ReportBuilder
    {
        #region ATRIBUTES

        /// <summary>
        /// Detalles por réplica de las unidades de backup
        /// </summary>
        private Dictionary<int, List<UnidadBackup>> _detalles_backups;

        /// <summary>
        /// Diccionario con los estadísticos consolidados de cada slot de backup
        /// </summary>
        private Dictionary<string, InfoReporteUtilizacion> _estadisticos_por_slot_backup;

        /// <summary>
        /// Encabezados de las columnas
        /// </summary>
        private string[] _headers;

        #endregion

        #region CONSTRUCTOR

        public ReporteUtilizacionBackups(string path, string filename, string[] sheetsNames, Dictionary<int, List<UnidadBackup>> detalles, string[] headers)   
            : base(path, filename, sheetsNames)
        {
            this._headers = headers;
            this._detalles_backups = detalles;
            this._estadisticos_por_slot_backup = new Dictionary<string, InfoReporteUtilizacion>();
            Dictionary<string, List<UnidadBackup>> backups_por_id = new Dictionary<string, List<UnidadBackup>>();
            foreach (int replica in detalles.Keys)
            {
                foreach (UnidadBackup bu in detalles[replica])
                {
                    if (!backups_por_id.ContainsKey(bu.Id))
                    {
                        backups_por_id.Add(bu.Id, new List<UnidadBackup>());
                    }
                    backups_por_id[bu.Id].Add(bu);
                }
            }

            foreach (string key in backups_por_id.Keys)
            {
                _estadisticos_por_slot_backup.Add(key, new InfoReporteUtilizacion(backups_por_id[key]));
            }
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
            base.CrearReporte(titulo,juntaTitulos);
            base.SetSheetsHeaders(_headers, -11);
            int contadorRow = 0;
            string nombreHoja = "Reporte Utilización";
            Sheet sheet = base.Workbook.GetSheet(nombreHoja);  
            foreach (InfoReporteUtilizacion info in _estadisticos_por_slot_backup.Values)
            {              
                int col = _primera_columna;
                Cell cell = sheet.CreateRow(_primera_fila + contadorRow).CreateCell(col);
                cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                cell.SetCellType(CellType.NUMERIC);
                cell.SetCellValue(contadorRow + 1);
                col++;

                cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                cell.SetCellType(CellType.STRING);
                cell.SetCellValue(info.id);
                col++;

                cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                cell.SetCellType(CellType.STRING);
                cell.SetCellValue(info.estacion);
                col++;

                cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                cell.SetCellType(CellType.STRING);
                cell.SetCellValue(info.subflota);
                col++;

                cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                cell.SetCellType(CellType.STRING);
                cell.SetCellValue(info.tiempo_ini_slot);
                col++;

                cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                cell.SetCellType(CellType.STRING);
                cell.SetCellValue(info.tiempo_fin_slot);
                col++;

                cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                cell.SetCellType(CellType.NUMERIC);
                cell.SetCellValue(info.duracion_slot);
                col++;

                cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                cell.SetCellType(CellType.NUMERIC);
                cell.SetCellValue(info.estadisticosPorcentajeUtilizacionNeta.Media);
                col++;

                cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                cell.SetCellType(CellType.NUMERIC);
                cell.SetCellValue(info.estadisticosPorcentajeUtilizacionNeta.Desvest);
                col++;

                cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                cell.CellStyle = GetEstilo(EstilosTexto.NumeroDosDecimales);
                cell.SetCellType(CellType.NUMERIC);
                cell.SetCellValue(info.estadisticosMinutosRecuperados.Media);
                col++;

                cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                cell.CellStyle = GetEstilo(EstilosTexto.NumeroDosDecimales);
                cell.SetCellType(CellType.NUMERIC);
                cell.SetCellValue(info.estadisticosMinutosRecuperados.Desvest);
                col++;

                cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                cell.CellStyle = GetEstilo(EstilosTexto.NumeroDosDecimales);
                cell.SetCellType(CellType.NUMERIC);
                cell.SetCellValue(info.estadisticosTramosRecuperados.Media);
                col++;

                cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                cell.CellStyle = GetEstilo(EstilosTexto.NumeroDosDecimales);
                cell.SetCellType(CellType.NUMERIC);
                cell.SetCellValue(info.estadisticosTramosRecuperados.Desvest);
                col++;

                contadorRow++;
            }
            CrearReporteDetalles(_detalles_backups);

        }

        #endregion

        #region PRIVATE METHODS

        private void CrearReporteDetalles(Dictionary<int, List<UnidadBackup>> detalles_backups)
        {
            Workbook.CreateSheet("Detalles Backups");
            base.CrearSheetReporteGeneral("Detalles Backups", "Detalles Backups", new string[] {"Réplica", "Id_slot", "Base", "SubFlota", "Utilización_Neta", "Minutos Recuperados", "Tramos Recuperados" }, 0);
            Sheet sheet = Workbook.GetSheet("Detalles Backups");
            int contadorRow = 0;
            foreach (int replica in detalles_backups.Keys)
            {
                foreach (UnidadBackup bu in detalles_backups[replica])
                {
                    int col = _primera_columna;
                    Cell cell = sheet.CreateRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(replica);
                    col++;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                    cell.SetCellType(CellType.STRING);
                    cell.SetCellValue(bu.Id);
                    col++;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                    cell.SetCellType(CellType.STRING);
                    cell.SetCellValue(bu.Estacion);
                    col++;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                    cell.SetCellType(CellType.STRING);
                    cell.SetCellValue(bu.TramoBase.Ac_Owner + " " + bu.TramoBase.Numero_Vuelo);
                    col++;

                    EstadisticosUtilizacion estadisticos = bu.Estadisticos;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                    cell.SetCellType(CellType.STRING);
                    cell.SetCellValue(estadisticos.porcentajeUtilizacionNeta);
                    col++;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                    cell.SetCellType(CellType.STRING);
                    cell.SetCellValue(estadisticos.minutosRecuperados);
                    col++;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(estadisticos.tramosRecuperados);
                    col++;                    

                    contadorRow++;
                
                }
            }

        }

        #endregion
    }

    /// <summary>
    /// Estructura utilizada para consolidar la información de las unidades de backup
    /// </summary>
    internal struct InfoReporteUtilizacion
    {
        #region ATRIBUTES

        /// <summary>
        /// Id de la unidad de backup
        /// </summary>
        public string id;

        /// <summary>
        /// Id de la estación de backup
        /// </summary>
        public string estacion;

        /// <summary>
        /// Subflota del avión asignado
        /// </summary>
        public string subflota;

        /// <summary>
        /// Tiempo de inicio del slot
        /// </summary>
        public string tiempo_ini_slot;

        /// <summary>
        /// Tiempo de término del slot
        /// </summary>
        public string tiempo_fin_slot;

        /// <summary>
        /// Minutos de duración del slot
        /// </summary>
        public int duracion_slot;

        /// <summary>
        /// Estructura con los estadísticos del porcentaje de utilización neto (slot transferido)
        /// </summary>
        public EstadisticosGenerales estadisticosPorcentajeUtilizacionNeta;

        /// <summary>
        /// Estructura con los estadísticos del total de minutos de atraso recuperados
        /// </summary>
        public EstadisticosGenerales estadisticosMinutosRecuperados;

        /// <summary>
        /// Estructura con los estadísticos del total de tramos recuperados
        /// </summary>
        public EstadisticosGenerales estadisticosTramosRecuperados;

        /// <summary>
        /// Estructura con los estadísticos del porcentaje de utilización del slot (slot estático)
        /// </summary>
        public EstadisticosGenerales estadisticosUtilizacionSlot;

        /// <summary>
        /// Lista con los detalles de cada réplica
        /// </summary>
        public List<UnidadBackup> valores;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Construye la estructura y camputa sus estadísticos generales.
        /// </summary>
        /// <param name="valores">Lista con los detalles de cada réplica</param>
        public InfoReporteUtilizacion(List<UnidadBackup> valores)
        {
            UnidadBackup unidadBackup = valores[0];
            this.valores = valores;
            this.subflota = unidadBackup.TramoBase.Ac_Owner + " " + unidadBackup.TramoBase.Numero_Vuelo;
            this.estacion = unidadBackup.Estacion;
            this.id = unidadBackup.Id;
            this.tiempo_ini_slot = unidadBackup.TramoBase.Hora_Salida;
            this.tiempo_fin_slot = unidadBackup.TramoBase.Hora_Llegada;
            this.duracion_slot = unidadBackup.TiempoFinPrg - unidadBackup.TiempoIniPrg;
            List<double> valoresUtilizacionNeta = new List<double>();
            List<double> valoresTramosRecuperados = new List<double>();
            List<double> valoresMinutosRecuperados = new List<double>();
            List<double> valoresUtilizacionSlotSinTA = new List<double>();
            List<double> valoresUtilizacionSlotConTA = new List<double>();
            foreach (UnidadBackup bu in valores)
            {
                EstadisticosUtilizacion estadisticos_replica = bu.Estadisticos;
                valoresUtilizacionNeta.Add(estadisticos_replica.porcentajeUtilizacionNeta);
                valoresTramosRecuperados.Add(estadisticos_replica.tramosRecuperados);
                valoresMinutosRecuperados.Add(estadisticos_replica.minutosRecuperados);
                valoresUtilizacionSlotSinTA.Add(estadisticos_replica.porcentajeUtilizacionSlotSinTA);
                valoresUtilizacionSlotConTA.Add(estadisticos_replica.porcentajeUtilizacionSlotConTA);
            }
            this.estadisticosMinutosRecuperados = new EstadisticosGenerales(valoresMinutosRecuperados);
            this.estadisticosPorcentajeUtilizacionNeta = new EstadisticosGenerales(valoresUtilizacionNeta);
            this.estadisticosTramosRecuperados = new EstadisticosGenerales(valoresTramosRecuperados);
            this.estadisticosUtilizacionSlot = new EstadisticosGenerales(valoresUtilizacionSlotConTA);
        }

        #endregion
    }
}
