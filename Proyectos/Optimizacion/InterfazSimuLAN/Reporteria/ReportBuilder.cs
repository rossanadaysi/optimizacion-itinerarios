using System;
using System.Collections.Generic;
using System.Text;
using NPOI.HSSF.UserModel;
using NPOI.HPSF;
using NPOI.POIFS.FileSystem;
using NPOI.SS.UserModel;
using NPOI.HSSF.Util;
using System.IO;

namespace InterfazSimuLAN.Reportes
{
    /// <summary>
    /// Clase que encapsula la creación de reportes utilizando la librería NPOI
    /// </summary>
    internal class ReportBuilder
    {
        /// <summary>
        /// Definiciones para los estilos dwe celdas soportados en los reportes
        /// </summary>
        public enum EstilosTexto { NumeroEntero, Porcentajes, EncabezadoColumnas, Titulo, NumeroDosDecimales, EncabezadoFila, PorcentajesNegrita, NumeroEnteroNegrita }

        #region CONSTANTS

        //Fuente usada para la creación de reportes
        private const string FUENTE = "Calibri";

        #endregion

        #region ATRIBUTES

        /// <summary>
        /// Estructura que encapsula los estilos definidos para los reportes
        /// </summary>
        private Dictionary<EstilosTexto, CellStyle> _estilos;

        /// <summary>
        /// Nobre del libro de excel
        /// </summary>
        private string _filename;

        /// <summary>
        /// Lista con el número de columnas del reporte
        /// </summary>
        private List<int> _index_col_logo;
        
        /// <summary>
        /// Ruta que almacena el archivo excel
        /// </summary>
        private string _path;
        
        /// <summary>
        /// Arreglo con los nombres de las hojas del libro de excel
        /// </summary>
        private string[] _sheets_names;
        
        /// <summary>
        /// Columna donde inicia la escritura de los valores
        /// </summary>
        public int _primera_columna;

        /// <summary>
        /// Columna donde inicia la escritura de los valores
        /// </summary>
        public int _primera_fila;
        
        /// <summary>
        /// Objeto que guarda un libro de excel.
        /// </summary>
        private HSSFWorkbook _workbook;
        
        #endregion

        #region PROPERTIES

        /// <summary>
        /// Objeto que guarda un libro de excel.
        /// </summary>
        public HSSFWorkbook Workbook
        {
            get { return _workbook; }
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor genérico de reportes excel
        /// </summary>
        /// <param name="path">Ruta del reporte</param>
        /// <param name="filename">Nombre del reporte</param>
        /// <param name="sheetsNames">Nombre de las hojas dentro del reporte</param>
        public ReportBuilder(string path,string filename,string[] sheetsNames)
        {
            this._path = path;
            this._filename = filename;
            this._sheets_names = sheetsNames;
            this._workbook = new HSSFWorkbook();
            for (int i = 0; i < sheetsNames.Length; i++)
            {
                _workbook.CreateSheet(sheetsNames[i]);
                _workbook.GetSheet(sheetsNames[i]).DisplayGridlines = false;               
            }
            this._estilos = new Dictionary<EstilosTexto, CellStyle>();
            this._primera_columna = 1;
            this._primera_fila = 3;
            this._index_col_logo = new List<int>();
            CargarEstilos();
        }

        #endregion

        #region PRIVATE METHODS


        /// <summary>
        /// Carga estilos de celda
        /// </summary>
        private void CargarEstilos()
        {
            _estilos.Add(EstilosTexto.Titulo, EstiloTitulo(this._workbook));
            _estilos.Add(EstilosTexto.NumeroEntero, EstiloCeldaNumeroEntero(this._workbook));
            _estilos.Add(EstilosTexto.NumeroDosDecimales, EstiloCeldaNumeroDosDecimales(this._workbook));
            _estilos.Add(EstilosTexto.EncabezadoColumnas, EstiloEncabezadosColumnas(this._workbook));
            _estilos.Add(EstilosTexto.Porcentajes, EstiloCeldaPorcentaje(this._workbook));
            _estilos.Add(EstilosTexto.EncabezadoFila, EstiloEncabezadosFilas(this._workbook));
            _estilos.Add(EstilosTexto.NumeroEnteroNegrita,EstiloCeldaNumeroEnteroNegrita(this._workbook));
            _estilos.Add(EstilosTexto.PorcentajesNegrita, EstiloCeldaPorcentajeNegrita(this._workbook));
        }

        /// <summary>
        /// Metodo usado para cargar una imagen de encabezado en cada reporte
        /// </summary>
        /// <param name="sheet1">Hoja dondde se insertará la imagen</param>
        /// <param name="cols">Columna donde se insertará la imagen</param>
        private void CargarLogoPrimeraFila(Sheet sheet1, int cols)
        {
            HSSFPatriarch patriarch = (HSSFPatriarch)sheet1.CreateDrawingPatriarch();
            //create the anchor
            HSSFClientAnchor anchor;
            anchor = new HSSFClientAnchor(0, 0, 255, 255, cols, 0, cols, 1);
            anchor.AnchorType = 0;
            //load the picture and get the picture index in the workbook
            HSSFPicture picture = (HSSFPicture)patriarch.CreatePicture(anchor, this.LoadImage(System.Windows.Forms.Application.StartupPath + @"/ClipArt/LAN.bmp", _workbook));//@"C:\Users\Rodolfo\PUC\postgrado\SimuLAN2\Code\InterfazSimuLAN\Reportes\lan.jpg", this.workbook));
            //Reset the image to the original size.
            picture.Resize();
            picture.LineStyle = HSSFPicture.LINESTYLE_NONE;
        }

        /// <summary>
        /// Metodo usado para cargar una imagen de encabezado en cada reporte
        /// </summary>
        /// <param name="sheet1">Hoja dondde se insertará la imagen</param>
        /// <param name="cols">Columna donde se insertará la imagen</param>
        private void CargarLogoUltimaFila(Sheet sheet1, int cols)
        {
            HSSFPatriarch patriarch = (HSSFPatriarch)sheet1.CreateDrawingPatriarch();
            //create the anchor
            HSSFClientAnchor anchor;
            anchor = new HSSFClientAnchor(0, 0, 255, 255, cols, sheet1.LastRowNum + 2, cols, sheet1.LastRowNum + 2);
            anchor.AnchorType = 0;
            //load the picture and get the picture index in the workbook
            HSSFPicture picture = (HSSFPicture)patriarch.CreatePicture(anchor, this.LoadImage(System.Windows.Forms.Application.StartupPath + @"/ClipArt/LAN.bmp", _workbook));//@"C:\Users\Rodolfo\PUC\postgrado\SimuLAN2\Code\InterfazSimuLAN\Reportes\lan.jpg", this.workbook));
            //Reset the image to the original size.
            picture.Resize();
            picture.LineStyle = HSSFPicture.LINESTYLE_NONE;
        }

        /// <summary>
        /// Setea el estilo usado para una celda de estilo número sin decimales
        /// </summary>
        /// <param name="hw">Workbook</param>
        /// <returns></returns>
        private CellStyle EstiloCeldaNumeroDosDecimales(HSSFWorkbook hw)
        {
            CellStyle style = hw.CreateCellStyle();
            style.BorderBottom = CellBorderType.THIN;
            style.BottomBorderColor = HSSFColor.BLACK.index;
            style.BorderLeft = CellBorderType.THIN;
            style.LeftBorderColor = HSSFColor.BLACK.index;
            style.BorderRight = CellBorderType.THIN;
            style.RightBorderColor = HSSFColor.BLACK.index;
            style.BorderTop = CellBorderType.THIN;
            style.TopBorderColor = HSSFColor.BLACK.index;
            style.VerticalAlignment = VerticalAlignment.CENTER;
            style.Alignment = HorizontalAlignment.GENERAL;
            style.DataFormat = HSSFDataFormat.GetBuiltinFormat("0.00");
            Font font = hw.CreateFont();
            SetFuenteCeldasNormal(font);
            style.SetFont(font);
            style.FillBackgroundColor = HSSFColor.WHITE.index;
            return style;
        }

        /// <summary>
        /// Setea el estilo usado para una celda de estilo número sin decimales
        /// </summary>
        /// <param name="hw">Workbook</param>
        /// <returns></returns>
        private CellStyle EstiloCeldaNumeroEntero(HSSFWorkbook hw)
        {
            CellStyle style = hw.CreateCellStyle();
            style.BorderBottom = CellBorderType.THIN;
            style.BottomBorderColor = HSSFColor.BLACK.index;
            style.BorderLeft = CellBorderType.THIN;
            style.LeftBorderColor = HSSFColor.BLACK.index;
            style.BorderRight = CellBorderType.THIN;
            style.RightBorderColor = HSSFColor.BLACK.index;
            style.BorderTop = CellBorderType.THIN;
            style.TopBorderColor = HSSFColor.BLACK.index;
            style.VerticalAlignment = VerticalAlignment.CENTER;
            style.Alignment = HorizontalAlignment.GENERAL;
            style.DataFormat = HSSFDataFormat.GetBuiltinFormat("000");
            Font font = hw.CreateFont();
            SetFuenteCeldasNormal(font);
            style.SetFont(font);
            style.FillBackgroundColor = HSSFColor.WHITE.index;
            return style;
        }

        /// <summary>
        /// Setea el estilo usado para una celda de estilo número sin decimales
        /// </summary>
        /// <param name="hw">Workbook</param>
        /// <returns></returns>
        private CellStyle EstiloCeldaNumeroEnteroNegrita(HSSFWorkbook hw)
        {
            CellStyle style = hw.CreateCellStyle();
            style.BorderBottom = CellBorderType.THIN;
            style.BottomBorderColor = HSSFColor.BLACK.index;
            style.BorderLeft = CellBorderType.THIN;
            style.LeftBorderColor = HSSFColor.BLACK.index;
            style.BorderRight = CellBorderType.THIN;
            style.RightBorderColor = HSSFColor.BLACK.index;
            style.BorderTop = CellBorderType.THIN;
            style.TopBorderColor = HSSFColor.BLACK.index;
            style.VerticalAlignment = VerticalAlignment.CENTER;
            style.Alignment = HorizontalAlignment.GENERAL;
            style.DataFormat = HSSFDataFormat.GetBuiltinFormat("000");
            Font font = hw.CreateFont();
            SetFuenteEncabezadoColumnas(font);
            font.FontHeightInPoints = 10;
            style.SetFont(font);
            style.FillBackgroundColor = HSSFColor.WHITE.index;
            return style;
        }

        /// <summary>
        /// Setea el estilo usado para una celda normal
        /// </summary>
        /// <param name="hw">Workbook</param>
        /// <returns></returns>
        private CellStyle EstiloCeldaPorcentaje(HSSFWorkbook hw)
        {
            CellStyle style = hw.CreateCellStyle();
            style.BorderBottom = CellBorderType.THIN;
            style.BottomBorderColor = HSSFColor.BLACK.index;
            style.BorderLeft = CellBorderType.THIN;
            style.LeftBorderColor = HSSFColor.BLACK.index;
            style.BorderRight = CellBorderType.THIN;
            style.RightBorderColor = HSSFColor.BLACK.index;
            style.BorderTop = CellBorderType.THIN;
            style.TopBorderColor = HSSFColor.BLACK.index;
            style.VerticalAlignment = VerticalAlignment.CENTER;
            style.Alignment = HorizontalAlignment.GENERAL;
            style.DataFormat = HSSFDataFormat.GetBuiltinFormat("0.00%");
            Font font = hw.CreateFont();
            SetFuenteCeldasNormal(font);
            style.SetFont(font);
            style.FillBackgroundColor = HSSFColor.WHITE.index;
            return style;
        }

        /// <summary>
        /// Setea el estilo usado para una celda normal
        /// </summary>
        /// <param name="hw">Workbook</param>
        /// <returns></returns>
        private CellStyle EstiloCeldaPorcentajeNegrita(HSSFWorkbook hw)
        {
            CellStyle style = hw.CreateCellStyle();
            style.BorderBottom = CellBorderType.THIN;
            style.BottomBorderColor = HSSFColor.BLACK.index;
            style.BorderLeft = CellBorderType.THIN;
            style.LeftBorderColor = HSSFColor.BLACK.index;
            style.BorderRight = CellBorderType.THIN;
            style.RightBorderColor = HSSFColor.BLACK.index;
            style.BorderTop = CellBorderType.THIN;
            style.TopBorderColor = HSSFColor.BLACK.index;
            style.VerticalAlignment = VerticalAlignment.CENTER;
            style.Alignment = HorizontalAlignment.CENTER;
            style.DataFormat = HSSFDataFormat.GetBuiltinFormat("0.00%");
            Font font = hw.CreateFont();
            SetFuenteEncabezadoColumnas(font);
            font.FontHeightInPoints = 10;
            style.SetFont(font);
            style.FillBackgroundColor = HSSFColor.WHITE.index;
            return style;
        }

        /// <summary>
        /// Setea el formato de los encabezados de las columnas
        /// </summary>
        /// <param name="hw">Workbook</param>
        /// <returns></returns>
        private CellStyle EstiloEncabezadosColumnas(HSSFWorkbook hw)
        {
            CellStyle style = hw.CreateCellStyle();
            style.BorderBottom = CellBorderType.THIN;
            style.BottomBorderColor = HSSFColor.BLACK.index;
            style.BorderLeft = CellBorderType.THIN;
            style.LeftBorderColor = HSSFColor.BLACK.index;
            style.BorderRight = CellBorderType.THIN;
            style.RightBorderColor = HSSFColor.BLACK.index;
            style.BorderTop = CellBorderType.THIN;
            style.TopBorderColor = HSSFColor.BLACK.index;
            style.VerticalAlignment = VerticalAlignment.CENTER;
            style.Alignment = HorizontalAlignment.CENTER;
            Font font = hw.CreateFont();
            SetFuenteEncabezadoColumnas(font);
            style.SetFont(font);
            style.FillBackgroundColor = HSSFColor.GREY_25_PERCENT.index;
            return style;
        }

        /// <summary>
        /// Setea el formato de los encabezados de las columnas
        /// </summary>
        /// <param name="hw">Workbook</param>
        /// <returns></returns>
        private CellStyle EstiloEncabezadosFilas(HSSFWorkbook hw)
        {
            CellStyle style = hw.CreateCellStyle();
            style.BorderBottom = CellBorderType.THIN;
            style.BottomBorderColor = HSSFColor.BLACK.index;
            style.BorderLeft = CellBorderType.THIN;
            style.LeftBorderColor = HSSFColor.BLACK.index;
            style.BorderRight = CellBorderType.THIN;
            style.RightBorderColor = HSSFColor.BLACK.index;
            style.BorderTop = CellBorderType.THIN;
            style.TopBorderColor = HSSFColor.BLACK.index;
            style.VerticalAlignment = VerticalAlignment.CENTER;
            style.Alignment = HorizontalAlignment.LEFT;
            Font font = hw.CreateFont();
            SetFuenteEncabezadoColumnas(font);
            style.SetFont(font);
            style.FillBackgroundColor = HSSFColor.GREY_25_PERCENT.index;
            return style;
        }

        /// <summary>
        /// Setea el estilo usado para los títulos de cada hoja
        /// </summary>
        /// <param name="hw"></param>
        /// <returns></returns>
        private CellStyle EstiloTitulo(HSSFWorkbook hw)
        {
            CellStyle style = hw.CreateCellStyle();
            style.VerticalAlignment = VerticalAlignment.CENTER;
            style.Alignment = HorizontalAlignment.LEFT;
            Font font = hw.CreateFont();
            SetFuenteTitulos(font);
            style.SetFont(font);
            style.FillBackgroundColor = HSSFColor.WHITE.index;
            return style;
        }

        /// <summary>
        ///Permite obtener un tipo de estilo
        /// </summary>
        /// <param name="estilo">Estilo de celda</param>
        /// <returns></returns>
        internal CellStyle GetEstilo(EstilosTexto estilo)
        {
            return _estilos[estilo];
        }

        #endregion
        
        #region PRIVATE STATIC METHODS

        /// <summary>
        /// Setea la fuente usada en una celda normal
        /// </summary>
        /// <param name="font"></param>
        private static void SetFuenteCeldasNormal(Font font)
        {
            font.Color = HSSFColor.BLACK.index;
            font.IsItalic = false;
            font.Boldweight = (short)FontBoldWeight.NORMAL;
            font.FontHeightInPoints = 10;
            font.FontName = FUENTE;
        }

        /// <summary>
        /// Setea la fuente usada en los encabezados de cada columna
        /// </summary>
        /// <param name="font"></param>
        private static void SetFuenteEncabezadoColumnas(Font font)
        {
            font.Color = HSSFColor.BLACK.index;
            font.IsItalic = false;
            font.Boldweight = (short)FontBoldWeight.BOLD;
            font.FontHeightInPoints = 12;
            font.FontName = FUENTE;
        }

        /// <summary>
        /// Setea la fuente usada en los títulos
        /// </summary>
        /// <param name="font"></param>
        private static void SetFuenteTitulos(Font font)
        {
            font.Color = HSSFColor.DARK_BLUE.index;
            font.IsItalic = false;
            font.Boldweight = (short)FontBoldWeight.BOLD;
            font.FontHeightInPoints = 14;
            font.FontName = FUENTE;
        }

        #endregion
        
        #region PUBLIC METHODS

        /// <summary>
        /// Ajusta las columnas automáticamente
        /// </summary>
        internal void AutoSizeColumns()
        {
            for (int i = 0; i < Workbook.NumberOfSheets; i++)
            {
                Sheet sheet = Workbook.GetSheetAt(i);
                for (int j = 1; j < 40; j++)
                {
                    sheet.AutoSizeColumn(j);
                    sheet.SetColumnWidth(j, sheet.GetColumnWidth(j) + 240);
                }
            }
        }

        /// <summary>
        /// Carga logo de la compañía en cada hoja del reporte
        /// </summary>
        /// <param name="primeraFila">Indica si se cargará el logo en la primera fila de cada hoja</param>
        internal void CargarLogoEnSheets(bool primeraFila)
        {
            for (int i = 0; i < Workbook.NumberOfSheets; i++)
            {
                Sheet sheet = Workbook.GetSheetAt(i);
                if (primeraFila)
                {
                    CargarLogoPrimeraFila(sheet, _index_col_logo[i]);
                }
                else
                {
                    CargarLogoUltimaFila(sheet, 1);
                }
            }
        }

        /// <summary>
        /// Método base para crear un reporte
        /// </summary>
        /// <param name="titulo">Título del reporte: aparecerá en cada hoja</param>
        /// <param name="colHeaders">Texto con los encabezados de cada columna</param>
        public virtual void CrearReporte(string titulo, bool junta_nombres_sheets_con_titulo)
        {            
            for (int nSheet = 0; nSheet < _sheets_names.Length; nSheet++)
            {
                Sheet sheet = _workbook.GetSheet(_sheets_names[nSheet]);
                //Escribe título
                Cell cellTitle = sheet.CreateRow(0).CreateCell(0);
                cellTitle.CellStyle = EstiloTitulo(this._workbook);
                cellTitle.SetCellType(CellType.STRING);
                string titulo_print = "";
                if (junta_nombres_sheets_con_titulo) titulo_print = titulo + _sheets_names[nSheet];
                else titulo_print = titulo;
                cellTitle.SetCellValue(titulo_print);
                sheet.GetRow(0).Height = Convert.ToInt16(cellTitle.CellStyle.GetFont(_workbook).FontHeightInPoints * 40);
            }
        }

        /// <summary>
        /// Método base para crear un sheet base de reporte
        /// </summary>
        /// <param name="titulo">Título del reporte: aparecerá en cada hoja</param>
        /// <param name="colHeaders">Texto con los encabezados de cada columna</param>
        public virtual void CrearSheetReporteGeneral(string nombreSheet, string titulo, string[] colHeaders, int desfaseLogo)
        {
            Sheet sheet = _workbook.GetSheet(nombreSheet);
            _index_col_logo.Add(colHeaders.Length + desfaseLogo);
            sheet.DisplayGridlines = false;
            //Escribe título
            Cell cellTitle = sheet.CreateRow(0).CreateCell(0);
            cellTitle.CellStyle = EstiloTitulo(this._workbook);
            cellTitle.SetCellType(CellType.STRING);
            cellTitle.SetCellValue(titulo);
            sheet.GetRow(0).Height = Convert.ToInt16(cellTitle.CellStyle.GetFont(_workbook).FontHeightInPoints * 40);

            //Escribe encabezados de columnas
            Row row = sheet.CreateRow(2);
            for (int i = 0; i < colHeaders.Length; i++)
            {
                Cell cellHeader = sheet.GetRow(2).CreateCell(i + 1);
                cellHeader.CellStyle = GetEstilo(EstilosTexto.EncabezadoColumnas);
                cellHeader.SetCellType(CellType.STRING);
                cellHeader.SetCellValue(colHeaders[i]);
                int nn = cellHeader.CellStyle.GetFont(_workbook).FontHeightInPoints;
            }
            //CargarLogo(sheet, colHeaders.Length + desfaseLogo);
            
        }

        /// <summary>
        /// Inicializa libro
        /// </summary>
        public void InitializeWorkbook()
        {
            //create a entry of DocumentSummaryInformation
            DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
            dsi.Company = "LAN Airlines";
            _workbook.DocumentSummaryInformation = dsi;

            //create a entry of SummaryInformation
            SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
            si.Subject = "SimuLAN report";
            _workbook.SummaryInformation = si;
        }

        /// <summary>
        /// Método usado para adherir una imagen a un workbook
        /// </summary>
        /// <param name="path">Ruta de la imagen</param>
        /// <param name="wb">Workbook</param>
        /// <returns></returns>
        public int LoadImage(string path, HSSFWorkbook wb)
        {
            FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[file.Length];
            file.Read(buffer, 0, (int)file.Length);
            return wb.AddPicture(buffer, PictureType.JPEG);
        }

        /// <summary>
        /// Escribe encabezados de las columnas
        /// </summary>
        /// <param name="indexSheet">Número de hoja</param>
        /// <param name="colHeaders">Nombres de las columnas</param>
        /// <param name="desfaseLogo">Columnas de desfase de logo de LAN</param>
        public virtual void SetHeadersSheet(int indexSheet, string[] colHeaders, int desfaseLogo)
        {
            Sheet sheet = _workbook.GetSheet(_sheets_names[indexSheet]);
            Row row = sheet.CreateRow(2);
            _index_col_logo.Add(colHeaders.Length + desfaseLogo);
            for (int i = 0; i < colHeaders.Length; i++)
            {
                Cell cellHeader = sheet.GetRow(2).CreateCell(i + 1);
                cellHeader.CellStyle = EstiloEncabezadosColumnas(this._workbook);
                cellHeader.SetCellType(CellType.STRING);
                cellHeader.SetCellValue(colHeaders[i]);
                int nn = cellHeader.CellStyle.GetFont(_workbook).FontHeightInPoints;                
            }
        }


        /// <summary>
        /// Escribe encabezados de las columnas
        /// </summary>
        /// <param name="nameSheet">Nombre de hoja</param>
        /// <param name="colHeaders">Nombres de las columnas</param>
        /// <param name="desfaseLogo">Columnas de desfase de logo de LAN</param>
        public virtual void SetHeadersSheet(string nameSheet, string[] colHeaders, int desfaseLogo)
        {
            Sheet sheet = _workbook.GetSheet(nameSheet);
            Row row = sheet.CreateRow(2);
            _index_col_logo.Add(desfaseLogo);
            for (int i = 0; i < colHeaders.Length; i++)
            {
                Cell cellHeader = sheet.GetRow(2).CreateCell(i + 1);
                cellHeader.CellStyle = GetEstilo(EstilosTexto.EncabezadoColumnas);
                cellHeader.SetCellType(CellType.STRING);
                cellHeader.SetCellValue(colHeaders[i]);
                int nn = cellHeader.CellStyle.GetFont(_workbook).FontHeightInPoints;
            }
        }

        /// <summary>
        /// Setea las columnas de todas las hojas
        /// </summary>
        /// <param name="colHeaders">Nombres de las columnas</param>
        /// <param name="desfaseLogo">Columnas de desfase de logo de LAN</param>
        public virtual void SetSheetsHeaders(string[] colHeaders, int desfaseLogo)
        {
            for (int nSheet = 0; nSheet < _sheets_names.Length; nSheet++)
            {
                SetHeadersSheet(nSheet, colHeaders, desfaseLogo);                
            }
        }

        /// <summary>
        /// Construye el archivo excel
        /// </summary>
        public void WriteToFile()
        {            
            FileStream file = new FileStream(_path + _filename + ".xls", FileMode.Create);
            _workbook.Write(file);
            file.Close();
        }
        
        #endregion
    }
}
