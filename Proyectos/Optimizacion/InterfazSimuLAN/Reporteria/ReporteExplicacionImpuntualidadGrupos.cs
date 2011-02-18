using System;
using System.Collections.Generic;
using System.Text;
using NPOI.HSSF.UserModel;
using NPOI.HPSF;
using NPOI.POIFS.FileSystem;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using SimuLAN.Clases;

namespace InterfazSimuLAN.Reportes
{
    /// <summary>
    /// Reporte de explicación de la impuntualidad por grupos
    /// </summary>
    internal class ReporteExplicacionImpuntualidadGrupos: ReportBuilder
    {
        #region ATRIBUTES

        /// <summary>
        /// Diccionario con los valores a ser impresos en el reporte (string nombreHoja, estructura con los valores a imprimir)
        /// </summary>
        private Dictionary<GruposReporte, ExplicacionImpuntualidadAgrupada> _valores_reporte;//int nombre hoja reporte, explicación de impuntualidad
        
        /// <summary>
        /// Lista con los estándares de puntualidad estimados
        /// </summary>
        private List<int> _stds;

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
        public ReporteExplicacionImpuntualidadGrupos(string path, string filename, string[] sheetsNames, Dictionary<GruposReporte, ExplicacionImpuntualidadAgrupada> valoresReporte, List<int> stds)
            : base(path, filename, sheetsNames)
        {
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
        
            //Crea primera columna
            string[] columna1 = new string[Enum.GetValues(typeof(TipoDisrupcion)).Length + 1];
            int contador2 = 0;
            foreach (TipoDisrupcion tipo in Enum.GetValues(typeof(TipoDisrupcion)))
            {
                if (tipo != TipoDisrupcion.ADELANTO)
                {
                    columna1[contador2] = tipo.ToString();
                    contador2++;
                }
            }
            columna1[columna1.Length - 2] = "TOTAL";
            columna1[columna1.Length - 1] = "LEGS";


            //Setea nombres de columnas de cada hoja
            foreach (GruposReporte grupo in _valores_reporte.Keys)
            {
                //Máximo de columnas es 256
                if (_valores_reporte[grupo].Estadisticos.Count < 256)
                {
                    foreach (int std in _stds)
                    {
                        string nombreHoja = grupo.ToString() + " - STD" + std;
                        string[] cols = new string[_valores_reporte[grupo].Estadisticos.Count + 1];
                        int contador = 0;
                        cols[contador] = "Causa de atraso";

                        foreach (string nombre in _valores_reporte[grupo].Estadisticos.Keys)
                        {
                            contador++;
                            cols[contador] = nombre;
                        }
                        int col = _primera_columna;
                        base.SetHeadersSheet(nombreHoja, cols, 10);
                        Sheet sheet = base.Workbook.GetSheet(nombreHoja);
                        for (int i = 0; i < columna1.Length; i++)
                        {
                            Cell cell = sheet.CreateRow(_primera_fila + i).CreateCell(col);
                            cell.CellStyle = GetEstilo(EstilosTexto.EncabezadoFila);
                            cell.SetCellType(CellType.STRING);
                            cell.SetCellValue(columna1[i]);
                        }
                        col++;

                        foreach (string nombre in _valores_reporte[grupo].Estadisticos.Keys)
                        {
                            int contadorRow = _primera_fila;
                            Cell cell;
                            double suma = 0;
                            foreach (TipoDisrupcion tipo in _valores_reporte[grupo].Estadisticos[nombre][std].Keys)
                            {
                                cell = sheet.GetRow(contadorRow).CreateCell(col);
                                cell.CellStyle = GetEstilo(EstilosTexto.Porcentajes);
                                cell.SetCellType(CellType.NUMERIC);
                                double media = _valores_reporte[grupo].Estadisticos[nombre][std][tipo].Media;
                                suma += media;
                                cell.SetCellValue(media);
                                contadorRow++;
                            }

                            cell = sheet.GetRow(contadorRow).CreateCell(col);
                            cell.CellStyle = GetEstilo(EstilosTexto.PorcentajesNegrita);

                            cell.SetCellType(CellType.NUMERIC);
                            cell.SetCellValue(suma);
                            contadorRow++;

                            cell = sheet.GetRow(contadorRow).CreateCell(col);
                            cell.CellStyle = GetEstilo(EstilosTexto.NumeroEnteroNegrita);
                            cell.SetCellType(CellType.NUMERIC);
                            cell.SetCellValue(_valores_reporte[grupo].ContadorTotalesPorGrupo[nombre]);
                            col++;
                        }
                    }
                }
            }
        }
        
        #endregion

    }
}
