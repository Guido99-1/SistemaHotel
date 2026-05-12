using ClosedXML.Excel;
using Microsoft.JSInterop;
using SistemaHotel.Shared;

namespace SistemaHotel.Client.Servicios.Implementacion
{
    /// <summary>
    /// Servicio centralizado para generar reportes Excel con plantilla unificada.
    /// Aplica branding consistente del hotel a todos los reportes.
    /// </summary>
    public class ReporteExcelService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly ConfiguracionHotelDTO _config;

        public ReporteExcelService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _config = new ConfiguracionHotelDTO();
        }

        /// <summary>
        /// Genera un reporte Excel con plantilla unificada (branding del hotel).
        /// </summary>
        /// <param name="titulo">Título del reporte</param>
        /// <param name="subtitulo">Información adicional (filtros, rango, etc.)</param>
        /// <param name="encabezados">Lista de encabezados de columnas</param>
        /// <param name="datos">Datos a mostrar (matriz de objetos)</param>
        /// <param name="anchosColumnas">Anchos de cada columna</param>
        /// <param name="columnasMoneda">Índices (0-based) de columnas que son moneda</param>
        /// <param name="usuarioGenerador">Nombre del usuario que generó el reporte</param>
        /// <param name="nombreArchivo">Nombre del archivo (sin extensión)</param>
        /// <param name="incluirTotales">Si se debe incluir fila de totales</param>
        public async Task GenerarReporte(
            string titulo,
            string subtitulo,
            List<string> encabezados,
            List<object[]> datos,
            double[] anchosColumnas,
            int[] columnasMoneda,
            string usuarioGenerador,
            string nombreArchivo,
            bool incluirTotales = true)
        {
            using var libro = new XLWorkbook();
            var hoja = libro.Worksheets.Add(titulo.Length > 30 ? titulo.Substring(0, 30) : titulo);

            int totalCols = encabezados.Count;

            // ═══════════════════════════════════════════
            // 1. ENCABEZADO DEL HOTEL (branding)
            // ═══════════════════════════════════════════
            hoja.Range(1, 1, 1, totalCols).Merge();
            hoja.Cell(1, 1).Value = _config.NombreHotel;
            hoja.Cell(1, 1).Style.Font.Bold = true;
            hoja.Cell(1, 1).Style.Font.FontSize = 18;
            hoja.Cell(1, 1).Style.Font.FontColor = XLColor.White;
            hoja.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(_config.ColorPrimario);
            hoja.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            hoja.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            hoja.Row(1).Height = 35;

            // Subtítulo del hotel (datos de contacto)
            hoja.Range(2, 1, 2, totalCols).Merge();
            hoja.Cell(2, 1).Value = $"RUC: {_config.Ruc}  |  {_config.Direccion}  |  Tel: {_config.Telefono}";
            hoja.Cell(2, 1).Style.Font.FontSize = 9;
            hoja.Cell(2, 1).Style.Font.Italic = true;
            hoja.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            hoja.Cell(2, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E3F2FD");
            hoja.Row(2).Height = 18;

            // ═══════════════════════════════════════════
            // 2. TÍTULO DEL REPORTE
            // ═══════════════════════════════════════════
            hoja.Range(4, 1, 4, totalCols).Merge();
            hoja.Cell(4, 1).Value = titulo.ToUpper();
            hoja.Cell(4, 1).Style.Font.Bold = true;
            hoja.Cell(4, 1).Style.Font.FontSize = 14;
            hoja.Cell(4, 1).Style.Font.FontColor = XLColor.FromHtml(_config.ColorPrimario);
            hoja.Cell(4, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            hoja.Row(4).Height = 22;

            // Subtítulo (filtros aplicados)
            if (!string.IsNullOrEmpty(subtitulo))
            {
                hoja.Range(5, 1, 5, totalCols).Merge();
                hoja.Cell(5, 1).Value = subtitulo;
                hoja.Cell(5, 1).Style.Font.FontSize = 10;
                hoja.Cell(5, 1).Style.Font.Italic = true;
                hoja.Cell(5, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                hoja.Row(5).Height = 16;
            }

            // Metadata
            hoja.Range(6, 1, 6, totalCols).Merge();
            hoja.Cell(6, 1).Value = $"Generado por: {usuarioGenerador}  |  Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}  |  Total registros: {datos.Count}";
            hoja.Cell(6, 1).Style.Font.FontSize = 9;
            hoja.Cell(6, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            hoja.Row(6).Height = 14;

            // ═══════════════════════════════════════════
            // 3. ENCABEZADOS DE COLUMNAS
            // ═══════════════════════════════════════════
            int filaHeader = 8;
            for (int i = 0; i < encabezados.Count; i++)
            {
                hoja.Cell(filaHeader, i + 1).Value = encabezados[i];
            }

            var rangoHeader = hoja.Range(filaHeader, 1, filaHeader, totalCols);
            rangoHeader.Style.Font.Bold = true;
            rangoHeader.Style.Font.FontSize = 11;
            rangoHeader.Style.Font.FontColor = XLColor.White;
            rangoHeader.Style.Fill.BackgroundColor = XLColor.FromHtml(_config.ColorPrimario);
            rangoHeader.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rangoHeader.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            rangoHeader.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            rangoHeader.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            hoja.Row(filaHeader).Height = 22;

            // ═══════════════════════════════════════════
            // 4. DATOS
            // ═══════════════════════════════════════════
            int filaDataInicio = filaHeader + 1;
            int fila = filaDataInicio;

            foreach (var row in datos)
            {
                for (int c = 0; c < row.Length; c++)
                {
                    var cellValue = row[c];
                    if (cellValue == null)
                    {
                        hoja.Cell(fila, c + 1).Value = "";
                    }
                    else if (cellValue is decimal d)
                    {
                        hoja.Cell(fila, c + 1).Value = d;
                    }
                    else if (cellValue is double db)
                    {
                        hoja.Cell(fila, c + 1).Value = db;
                    }
                    else if (cellValue is int i)
                    {
                        hoja.Cell(fila, c + 1).Value = i;
                    }
                    else if (cellValue is DateTime dt)
                    {
                        hoja.Cell(fila, c + 1).Value = dt;
                    }
                    else
                    {
                        hoja.Cell(fila, c + 1).Value = cellValue.ToString();
                    }
                }

                // Filas alternadas (estilo zebra)
                if ((fila - filaDataInicio) % 2 == 1)
                {
                    hoja.Range(fila, 1, fila, totalCols).Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F5F5");
                }

                fila++;
            }

            int filaDataFin = fila - 1;

            // Bordes en datos
            if (datos.Any())
            {
                var rangoDatos = hoja.Range(filaDataInicio, 1, filaDataFin, totalCols);
                rangoDatos.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                rangoDatos.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                rangoDatos.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                // Permitir que el texto se ajuste en celdas largas (como métodos MIXTO)
                rangoDatos.Style.Alignment.WrapText = true;
            }

            // Formato de moneda
            foreach (int colIdx in columnasMoneda)
            {
                hoja.Column(colIdx + 1).Style.NumberFormat.Format = "$#,##0.00";
                hoja.Column(colIdx + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            }

            // ═══════════════════════════════════════════
            // 5. TOTALES (sólo para columnas de moneda)
            // ═══════════════════════════════════════════
            if (incluirTotales && columnasMoneda.Any() && datos.Any())
            {
                int filaTotal = filaDataFin + 2;

                hoja.Cell(filaTotal, 1).Value = "TOTALES:";
                hoja.Range(filaTotal, 1, filaTotal, columnasMoneda.Min()).Merge();
                hoja.Cell(filaTotal, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                foreach (int colIdx in columnasMoneda)
                {
                    string colLetra = GetColumnLetter(colIdx + 1);
                    hoja.Cell(filaTotal, colIdx + 1).FormulaA1 = $"SUM({colLetra}{filaDataInicio}:{colLetra}{filaDataFin})";
                    hoja.Cell(filaTotal, colIdx + 1).Style.NumberFormat.Format = "$#,##0.00";
                }

                var rangoTotales = hoja.Range(filaTotal, 1, filaTotal, totalCols);
                rangoTotales.Style.Font.Bold = true;
                rangoTotales.Style.Font.FontSize = 11;
                rangoTotales.Style.Fill.BackgroundColor = XLColor.FromHtml(_config.ColorTotales);
                rangoTotales.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                rangoTotales.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            // ═══════════════════════════════════════════
            // 6. ANCHOS DE COLUMNAS
            // ═══════════════════════════════════════════
            for (int i = 0; i < anchosColumnas.Length && i < totalCols; i++)
            {
                hoja.Column(i + 1).Width = anchosColumnas[i];
            }

            // ═══════════════════════════════════════════
            // 7. FILTROS Y CONGELAR PANEL
            // ═══════════════════════════════════════════
            if (datos.Any())
            {
                hoja.Range(filaHeader, 1, filaDataFin, totalCols).SetAutoFilter();
            }
            hoja.SheetView.FreezeRows(filaHeader);

            // ═══════════════════════════════════════════
            // 8. PIE DE PÁGINA
            // ═══════════════════════════════════════════
            int filaFooter = filaDataFin + (incluirTotales && columnasMoneda.Any() ? 4 : 2);
            hoja.Range(filaFooter, 1, filaFooter, totalCols).Merge();
            hoja.Cell(filaFooter, 1).Value = $"{_config.NombreHotel}  |  {_config.Email}  |  {_config.Website}";
            hoja.Cell(filaFooter, 1).Style.Font.FontSize = 9;
            hoja.Cell(filaFooter, 1).Style.Font.Italic = true;
            hoja.Cell(filaFooter, 1).Style.Font.FontColor = XLColor.Gray;
            hoja.Cell(filaFooter, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // ═══════════════════════════════════════════
            // 9. DESCARGAR
            // ═══════════════════════════════════════════
            using var memoria = new MemoryStream();
            libro.SaveAs(memoria);

            var nombreFinal = $"{nombreArchivo}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

            await _jsRuntime.InvokeAsync<object>(
                "DescargarExcel",
                nombreFinal,
                Convert.ToBase64String(memoria.ToArray())
            );
        }

        /// <summary>
        /// Convierte número de columna a letra (1=A, 2=B, ..., 27=AA)
        /// </summary>
        private static string GetColumnLetter(int columnNumber)
        {
            string columnLetter = "";
            while (columnNumber > 0)
            {
                int modulo = (columnNumber - 1) % 26;
                columnLetter = Convert.ToChar(65 + modulo) + columnLetter;
                columnNumber = (columnNumber - modulo) / 26;
            }
            return columnLetter;
        }
    }
}
