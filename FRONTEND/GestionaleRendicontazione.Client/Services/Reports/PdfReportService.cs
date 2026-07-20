using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Snippets.Font;

namespace GestionaleRendicontazione.Client.Services.Reports
{
    /// <summary>
    /// Genera un PDF di riepilogo dei worklog completamente lato client.
    /// Usa il package core di PDFsharp (no GDI/WPF) per restare compatibile con
    /// Blazor WebAssembly. Il font resolver è inizialializzato al primo utilizzo.
    /// </summary>
    public sealed class PdfReportService
    {
        // Layout A4 portrait in punti tipografici (1 pt = 1/72 inch).
        private const double MarginPt = 36;            // ~12.7 mm
        private const double PageWidthPt = 595.28;
        private const double PageHeightPt = 841.89;
        private const double FooterAreaPt = 28;
        private const double UsableWidthPt = PageWidthPt - 2 * MarginPt;

        private const double RowMinHeight = 16;
        private const double DescLineHeight = 11;
        private const int MaxDescLines = 3;

        private static readonly object _fontInitLock = new();
        private static bool _fontResolverRegistered;

        public PdfReportService()
        {
            EnsureFontResolverRegistered();
        }

        /// <summary>
        /// Costruisce un PDF in memoria a partire dalla lista di worklog fornita.
        /// </summary>
        /// <param name="worklogs">Worklog da includere nel report (filtri già applicati).</param>
        /// <param name="periodFrom">Data inizio periodo.</param>
        /// <param name="periodTo">Data fine periodo.</param>
        /// <param name="generatedBy">Nome utente che ha generato il report (per l'intestazione).</param>
        /// <param name="groupByEmployee">Se true raggruppa per dipendente (utile lato admin).</param>
        public Task<byte[]> BuildReportAsync(
            IEnumerable<WorkLogResponseDto> worklogs,
            DateOnly periodFrom,
            DateOnly periodTo,
            string? generatedBy,
            bool groupByEmployee)
        {
            // PDFsharp è sincrono: lo wrappiamo in Task.Run per non bloccare il thread
            // della UI di Blazor, che è single-threaded.
            return Task.Run(() => BuildReportCore(
                worklogs ?? Array.Empty<WorkLogResponseDto>(),
                periodFrom,
                periodTo,
                generatedBy,
                groupByEmployee));
        }

        private static byte[] BuildReportCore(
            IEnumerable<WorkLogResponseDto> worklogs,
            DateOnly periodFrom,
            DateOnly periodTo,
            string? generatedBy,
            bool groupByEmployee)
        {
            var list = worklogs.ToList();

            using var document = new PdfDocument();
            document.Info.Title = $"Report Worklog {periodFrom:dd/MM/yyyy} - {periodTo:dd/MM/yyyy}";
            document.Info.Author = "Gestionale Rendicontazione";
            document.Info.Subject = "Report worklog";
            document.Info.Creator = "Gestionale Rendicontazione";

            // Definizione colonne (proporzioni in punti)
            var columns = new (string Title, double Width)[]
            {
                ("Data",         70),
                ("Progetto",     90),
                ("Tipo",         70),
                ("Stato",        60),
                ("Descrizione", 165),
                ("Ore",          40),
            };
            double totalColWidth = columns.Sum(c => c.Width);
            double scale = UsableWidthPt / totalColWidth;
            var colWidths = columns.Select(c => c.Width * scale).ToArray();
            double totalColScaled = colWidths.Sum();

            var fontTitle = new XFont("Arial", 18, XFontStyleEx.Bold);
            var fontSub = new XFont("Arial", 10, XFontStyleEx.Regular);
            var fontSection = new XFont("Arial", 12, XFontStyleEx.Bold);
            var fontHeader = new XFont("Arial", 9, XFontStyleEx.Bold);
            var fontRow = new XFont("Arial", 9, XFontStyleEx.Regular);
            var fontPageNum = new XFont("Arial", 8, XFontStyleEx.Regular);

            // ---- Stato disegno (mutabile) ----
            PdfPage page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;
            XGraphics gfx = XGraphics.FromPdfPage(page);
            double x = MarginPt;
            double y = MarginPt;

            void StartNewPage()
            {
                gfx.Dispose();
                page = document.AddPage();
                page.Size = PdfSharp.PageSize.A4;
                gfx = XGraphics.FromPdfPage(page);
                y = MarginPt;
            }

            void EnsureRoom(double needed)
            {
                if (y + needed > PageHeightPt - MarginPt - FooterAreaPt)
                    StartNewPage();
            }

            void DrawColumnHeader()
            {
                EnsureRoom(22);
                double cx = x;
                gfx.DrawRectangle(XBrushes.DimGray, x, y, totalColScaled, 18);
                for (int i = 0; i < columns.Length; i++)
                {
                    gfx.DrawString(columns[i].Title, fontHeader, XBrushes.White,
                        new XRect(cx + 3, y + 3, colWidths[i] - 6, 14), XStringFormats.TopLeft);
                    cx += colWidths[i];
                }
                y += 20;
            }

            void DrawRow(WorkLogResponseDto w)
            {
                var desc = string.IsNullOrWhiteSpace(w.Description) ? "-" : w.Description!;
                int approxCharsPerLine = Math.Max(20, (int)(colWidths[4] / 4.2));
                int lines = Math.Min(MaxDescLines, Math.Max(1, (desc.Length + approxCharsPerLine - 1) / approxCharsPerLine));
                double blockHeight = Math.Max(RowMinHeight, lines * DescLineHeight + 4);

                EnsureRoom(blockHeight + 1);

                // Data
                gfx.DrawString(w.Date.ToString("dd/MM/yyyy"), fontRow, XBrushes.Black,
                    new XRect(x + 3, y + 2, colWidths[0] - 6, RowMinHeight), XStringFormats.TopLeft);
                double cx = x + colWidths[0];
                // Progetto
                gfx.DrawString(Truncate(w.ProjectName, 22), fontRow, XBrushes.Black,
                    new XRect(cx + 3, y + 2, colWidths[1] - 6, RowMinHeight), XStringFormats.TopLeft);
                cx += colWidths[1];
                // Tipo
                gfx.DrawString(Truncate(w.TypeName, 18), fontRow, XBrushes.Black,
                    new XRect(cx + 3, y + 2, colWidths[2] - 6, RowMinHeight), XStringFormats.TopLeft);
                cx += colWidths[2];
                // Stato
                gfx.DrawString(Truncate(w.StatusName, 16), fontRow, XBrushes.Black,
                    new XRect(cx + 3, y + 2, colWidths[3] - 6, RowMinHeight), XStringFormats.TopLeft);
                cx += colWidths[3];
                // Descrizione (multilinea)
                var descRect = new XRect(cx + 3, y + 2, colWidths[4] - 6, lines * DescLineHeight);
                gfx.DrawString(desc, fontRow, XBrushes.Black, descRect, XStringFormats.TopLeft);
                // Ore
                double oreLeft = x + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4];
                gfx.DrawString(w.HoursCounter.ToString("0.##", CultureInfo.InvariantCulture), fontRow, XBrushes.Black,
                    new XRect(oreLeft + 3, y + 2, colWidths[5] - 6, RowMinHeight), XStringFormats.TopRight);

                y += blockHeight;
                gfx.DrawLine(XPens.WhiteSmoke, x, y, x + totalColScaled, y);
            }

            // ---- Intestazione documento (solo prima pagina) ---------------------
            gfx.DrawString("Report Worklog", fontTitle, XBrushes.Black,
                new XRect(x, y, UsableWidthPt, 22), XStringFormats.TopLeft);
            y += 24;

            gfx.DrawString(
                $"Periodo: {periodFrom:dd/MM/yyyy} - {periodTo:dd/MM/yyyy}",
                fontSub, XBrushes.Black,
                new XRect(x, y, UsableWidthPt, 14), XStringFormats.TopLeft);
            y += 14;

            gfx.DrawString(
                $"Generato il {DateTime.Now:dd/MM/yyyy HH:mm}" +
                (string.IsNullOrWhiteSpace(generatedBy) ? string.Empty : $" da {generatedBy}"),
                fontSub, XBrushes.Gray,
                new XRect(x, y, UsableWidthPt, 14), XStringFormats.TopLeft);
            y += 18;

            var totalHours = list.Sum(w => (double)w.HoursCounter);
            gfx.DrawString(
                $"Totale voci: {list.Count}    Totale ore: {totalHours.ToString("0.##", CultureInfo.InvariantCulture)}",
                fontSection, XBrushes.Black,
                new XRect(x, y, UsableWidthPt, 16), XStringFormats.TopLeft);
            y += 22;

            gfx.DrawLine(XPens.LightGray, x, y, x + UsableWidthPt, y);
            y += 8;

            // ---- Corpo ----------------------------------------------------------
            if (list.Count == 0)
            {
                EnsureRoom(20);
                gfx.DrawString("Nessun worklog presente nel periodo selezionato.",
                    fontRow, XBrushes.DarkSlateGray,
                    new XRect(x, y + 6, UsableWidthPt, 16), XStringFormats.TopLeft);
            }
            else if (groupByEmployee)
            {
                var byEmployee = list
                    .GroupBy(w => string.IsNullOrWhiteSpace(w.EmployeeName) ? "(Senza dipendente)" : w.EmployeeName!)
                    .OrderBy(g => g.Key, StringComparer.CurrentCultureIgnoreCase);

                foreach (var group in byEmployee)
                {
                    EnsureRoom(40);
                    gfx.DrawString($"Dipendente: {group.Key}", fontSection, XBrushes.Black,
                        new XRect(x, y, UsableWidthPt, 16), XStringFormats.TopLeft);
                    y += 18;

                    DrawColumnHeader();
                    foreach (var w in group.OrderBy(w => w.Date).ThenBy(w => w.ProjectName))
                        DrawRow(w);
                    y += 4;

                    var empHours = group.Sum(w => (double)w.HoursCounter);
                    EnsureRoom(20);
                    gfx.DrawString(
                        $"Totale {group.Key}: {empHours.ToString("0.##", CultureInfo.InvariantCulture)} ore",
                        fontHeader, XBrushes.Black,
                        new XRect(x, y, UsableWidthPt, 14), XStringFormats.TopLeft);
                    y += 18;

                    EnsureRoom(10);
                    gfx.DrawLine(XPens.LightGray, x, y, x + UsableWidthPt, y);
                    y += 8;
                }
            }
            else
            {
                DrawColumnHeader();
                foreach (var w in list.OrderBy(w => w.Date).ThenBy(w => w.ProjectName))
                    DrawRow(w);
            }

            gfx.Dispose();
            AddPageNumbers(document, fontPageNum);

            using var stream = new MemoryStream();
            document.Save(stream, false);
            return stream.ToArray();
        }

        private static string Truncate(string? s, int max) =>
            string.IsNullOrEmpty(s) ? string.Empty :
            s.Length <= max ? s : s.Substring(0, max - 1) + "…";

        private static void AddPageNumbers(PdfDocument document, XFont font)
        {
            int n = document.PageCount;
            for (int i = 0; i < n; i++)
            {
                var page = document.Pages[i];
                using var g = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);
                g.DrawString($"Pagina {i + 1} di {n}", font, XBrushes.Gray,
                    new XRect(MarginPt, PageHeightPt - MarginPt + 8, UsableWidthPt, 14),
                    XStringFormats.TopRight);
            }
        }

        /// <summary>
        /// In Blazor WebAssembly PDFsharp non trova i font di sistema: occorre
        /// registrare un IFontResolver che fornisca un font embedded. Il package
        /// PdfSharp.WPFonts (dipendenza) porta con sé 6 TTF Segoe WP, e il
        /// FailsafeFontResolver incluso li usa come fallback per qualunque
        /// famiglia richiesta. Va impostato una sola volta, prima di creare
        /// qualunque XFont.
        /// </summary>
        private static void EnsureFontResolverRegistered()
        {
            if (_fontResolverRegistered) return;
            lock (_fontInitLock)
            {
                if (_fontResolverRegistered) return;
                try
                {
                    if (GlobalFontSettings.FontResolver is null)
                    {
                        GlobalFontSettings.FontResolver = new FailsafeFontResolver();
                    }
                }
                catch
                {
                    // GlobalFontSettings può lanciare se già inizializzato altrove.
                }
                _fontResolverRegistered = true;
            }
        }
    }
}
