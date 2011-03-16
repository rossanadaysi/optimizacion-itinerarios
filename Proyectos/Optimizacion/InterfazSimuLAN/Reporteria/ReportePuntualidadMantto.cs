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
    /// Reporte de puntualidad de mantenimiento
    /// </summary>
    internal class ReportePuntualidadMantto:ReportBuilder
    {
        #region ATRIBUTES

        /// <summary>
        /// Detalles por réplica de las unidades de backup
        /// </summary>
        private Dictionary<int, List<SlotMantenimiento>> _detalles_slots;

        /// <summary>
        /// Diccionario con los estadísticos consolidados de cada slot de backup
        /// </summary>
        private Dictionary<string, InfoReporteMantto> _estadisticos_por_slot_mantto;

        /// <summary>
        /// Encabezados de las columnas
        /// </summary>
        private string[] _headers;

        #endregion

        #region CONSTRUCTOR

        public ReportePuntualidadMantto(string path, string filename, string[] sheetsNames, Dictionary<int, List<SlotMantenimiento>> detalles, string[] headers)
            : base(path, filename, sheetsNames)
        {
            this._headers = headers;
            this._detalles_slots = detalles;
            this._estadisticos_por_slot_mantto = new Dictionary<string,InfoReporteMantto>();
            Dictionary<string, List<SlotMantenimiento>> manttos_por_id = new Dictionary<string, List<SlotMantenimiento>>();
            foreach (int replica in detalles.Keys)
            {
                foreach (SlotMantenimiento mantto in detalles[replica])
                {
                    string id_mantto = mantto.TramoBase.Numero_Global.ToString();
                    if (!manttos_por_id.ContainsKey(id_mantto))
                    {
                        manttos_por_id.Add(id_mantto, new List<SlotMantenimiento>());
                    }
                    manttos_por_id[id_mantto].Add(mantto);
                }
            }

            foreach (string key in manttos_por_id.Keys)
            {
                _estadisticos_por_slot_mantto.Add(key, new InfoReporteMantto(manttos_por_id[key]));
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
            base.CrearReporte(titulo, juntaTitulos);
            base.SetSheetsHeaders(_headers, 0);
            int contadorRow = 0;
            string nombreHoja = "Puntualidad Mantto";
            Sheet sheet = base.Workbook.GetSheet(nombreHoja);  
            foreach (InfoReporteMantto info in _estadisticos_por_slot_mantto.Values)
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
                cell.SetCellValue(info.matricula);
                col++;

                cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                cell.SetCellType(CellType.STRING);
                cell.SetCellValue(info.tiempo_ini_slot_prog);
                col++;

                cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                cell.SetCellType(CellType.STRING);
                cell.SetCellValue(info.tiempo_fin_slot_prog);
                col++;

                cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                cell.SetCellType(CellType.NUMERIC);
                cell.SetCellValue(info.duracion_slot);
                col++;

                cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                cell.SetCellType(CellType.NUMERIC);
                cell.SetCellValue(info.estadisticosCumplimientoSTD0.Media);
                col++;

                cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                cell.SetCellType(CellType.NUMERIC);
                cell.SetCellValue(info.estadisticosCumplimientoSTD0.Desvest);
                col++;

                cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                cell.CellStyle = GetEstilo(EstilosTexto.NumeroDosDecimales);
                cell.SetCellType(CellType.NUMERIC);
                cell.SetCellValue(info.estadisticosMinutosAtraso.Media);
                col++;

                cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                cell.CellStyle = GetEstilo(EstilosTexto.NumeroDosDecimales);
                cell.SetCellType(CellType.NUMERIC);
                cell.SetCellValue(info.estadisticosMinutosAtraso.Desvest);
                col++;                

                contadorRow++;
            }
            CrearReporteDetalles(_detalles_slots);

        }

        #endregion

        #region PRIVATE METHODS

        private void CrearReporteDetalles(Dictionary<int, List<SlotMantenimiento>> detalles_backups)
        {
            Workbook.CreateSheet("Detalles Mantto");
            base.CrearSheetReporteGeneral("Detalles Mantto", "Detalles Mantto", new string[] { "Réplica", "Id_slot", "Estación", "SubFlota", "Matrícula", "Atraso" }, 0);
            Sheet sheet = Workbook.GetSheet("Detalles Mantto");
            int contadorRow = 0;
            foreach (int replica in detalles_backups.Keys)
            {
                foreach (SlotMantenimiento sm in detalles_backups[replica])
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
                    cell.SetCellValue(sm.TramoBase.Numero_Global.ToString());
                    col++;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                    cell.SetCellType(CellType.STRING);
                    cell.SetCellValue(sm.Estacion);
                    col++;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                    cell.SetCellType(CellType.STRING);
                    cell.SetCellValue(sm.TramoBase.Ac_Owner + " " + sm.TramoBase.Numero_Vuelo);
                    col++;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                    cell.SetCellType(CellType.STRING);
                    cell.SetCellValue(sm.TramoBase.Numero_Ac);
                    col++;

                    cell = sheet.GetRow(_primera_fila + contadorRow).CreateCell(col);
                    cell.CellStyle = GetEstilo(EstilosTexto.NumeroEntero);
                    cell.SetCellType(CellType.NUMERIC);
                    cell.SetCellValue(sm.TiempoInicioManttoRst - sm.TiempoInicioManttoPrg);
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
    internal struct InfoReporteMantto
    {
        #region ATRIBUTES

        /// <summary>
        /// Minutos de duración del slot
        /// </summary>
        public int duracion_slot;
        
        /// <summary>
        /// Id de la estación de backup
        /// </summary>
        public string estacion;
        
        /// <summary>
        /// Estructura con los estadísticos del porcentaje de utilización neto (slot transferido)
        /// </summary>
        public EstadisticosGenerales estadisticosCumplimientoSTD0;

        /// <summary>
        /// Estructura con los estadísticos del total de minutos de atraso recuperados
        /// </summary>
        public EstadisticosGenerales estadisticosMinutosAtraso;

        /// <summary>
        /// Id de la unidad de backup
        /// </summary>
        public string id;
        
        /// <summary>
        /// Matrícula del avión asignado
        /// </summary>
        public string matricula;
        
        /// <summary>
        /// Subflota del avión asignado
        /// </summary>
        public string subflota;
        
        /// <summary>
        /// Tiempo de término del slot programado
        /// </summary>
        public string tiempo_fin_slot_prog;

        /// <summary>
        /// Tiempo de inicio del slot programado
        /// </summary>
        public string tiempo_ini_slot_prog;

        /// <summary>
        /// Lista con los detalles de cada réplica
        /// </summary>
        public List<SlotMantenimiento> valores;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Construye la estructura y camputa sus estadísticos generales.
        /// </summary>
        /// <param name="valores">Lista con los detalles de cada réplica</param>
        public InfoReporteMantto(List<SlotMantenimiento> valores)
        {
            SlotMantenimiento slotMantto = valores[0];
            this.valores = valores;
            this.subflota = slotMantto.TramoBase.Ac_Owner + " " + slotMantto.TramoBase.Numero_Vuelo;
            this.matricula = slotMantto.TramoBase.Numero_Ac;
            this.estacion = slotMantto.Estacion;
            this.id = slotMantto.TramoBase.Numero_Global.ToString();
            this.tiempo_ini_slot_prog = slotMantto.TramoBase.Hora_Salida;
            this.tiempo_fin_slot_prog = slotMantto.TramoBase.Hora_Llegada;
            this.duracion_slot = slotMantto.TiempoFinManttoPrg - slotMantto.TiempoInicioManttoPrg;
            List<double> valoresCumplimiento = new List<double>();
            List<double> valoresMinutosAtraso = new List<double>();
            foreach (SlotMantenimiento sm in valores)
            {
                valoresCumplimiento.Add((sm.TiempoInicioManttoPrg == sm.TiempoInicioManttoRst) ? 1 : 0);
                valoresMinutosAtraso.Add(sm.TiempoInicioManttoRst - sm.TiempoInicioManttoPrg);
            }
            this.estadisticosCumplimientoSTD0 = new EstadisticosGenerales(valoresCumplimiento);
            this.estadisticosMinutosAtraso = new EstadisticosGenerales(valoresMinutosAtraso);
        }

        #endregion
    }
}
