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
    public sealed class PdfReportService
    {
        private const double MarginPt = 36;
        private const double PageWidthPt = 595.28;
        private const double PageHeightPt = 841.89;
        private const double FooterAreaPt = 28;
        private const double UsableWidthPt = PageWidthPt - 2 * MarginPt;

        private const double RowMinHeight = 16;
        private const double DescLineHeight = 11;
        private const int MaxDescLines = 3;

        private const int MaxWrappedLinesPerCell = 20;

        private static readonly object _fontInitLock = new();
        private static bool _fontResolverRegistered;

        public PdfReportService()
        {
            EnsureFontResolverRegistered();
        }

        public Task<byte[]> BuildReportAsync(
            IEnumerable<WorkLogResponseDto> worklogs,
            DateOnly periodFrom,
            DateOnly periodTo,
            string? generatedBy,
            bool groupByEmployee)
        {
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
                var projectName = w.ProjectName ?? string.Empty;
                var typeName = w.TypeName ?? string.Empty;
                var statusName = w.StatusName ?? string.Empty;

                const double cellPadding = 6;
                var projectLines = WrapText(gfx, projectName, fontRow, colWidths[1] - cellPadding);
                var typeLines = WrapText(gfx, typeName, fontRow, colWidths[2] - cellPadding);
                var statusLines = WrapText(gfx, statusName, fontRow, colWidths[3] - cellPadding);
                var descLines = WrapText(gfx, desc, fontRow, colWidths[4] - cellPadding);

                if (descLines.Count > MaxDescLines)
                {
                    var lastIdx = MaxDescLines - 1;
                    var lastLine = descLines[lastIdx];
                    if (lastLine.EndsWith("-")) lastLine = lastLine.Substring(0, lastLine.Length - 1);
                    descLines = descLines.Take(MaxDescLines).ToList();
                    descLines[lastIdx] = lastLine + "…";
                }

                int maxLines = Math.Max(1,
                    Math.Max(projectLines.Count,
                    Math.Max(typeLines.Count,
                    Math.Max(statusLines.Count, descLines.Count))));
                double blockHeight = Math.Max(RowMinHeight, maxLines * DescLineHeight + 4);

                EnsureRoom(blockHeight + 1);

                gfx.DrawString(w.Date.ToString("dd/MM/yyyy"), fontRow, XBrushes.Black,
                    new XRect(x + 3, y + 2, colWidths[0] - cellPadding, RowMinHeight), XStringFormats.TopLeft);

                double cx = x + colWidths[0];
                for (int i = 0; i < projectLines.Count; i++)
                {
                    gfx.DrawString(projectLines[i], fontRow, XBrushes.Black,
                        new XRect(cx + 3, y + 2 + i * DescLineHeight, colWidths[1] - cellPadding, DescLineHeight),
                        XStringFormats.TopLeft);
                }
                cx += colWidths[1];

                for (int i = 0; i < typeLines.Count; i++)
                {
                    gfx.DrawString(typeLines[i], fontRow, XBrushes.Black,
                        new XRect(cx + 3, y + 2 + i * DescLineHeight, colWidths[2] - cellPadding, DescLineHeight),
                        XStringFormats.TopLeft);
                }
                cx += colWidths[2];

                for (int i = 0; i < statusLines.Count; i++)
                {
                    gfx.DrawString(statusLines[i], fontRow, XBrushes.Black,
                        new XRect(cx + 3, y + 2 + i * DescLineHeight, colWidths[3] - cellPadding, DescLineHeight),
                        XStringFormats.TopLeft);
                }
                cx += colWidths[3];

                for (int i = 0; i < descLines.Count; i++)
                {
                    gfx.DrawString(descLines[i], fontRow, XBrushes.Black,
                        new XRect(cx + 3, y + 2 + i * DescLineHeight, colWidths[4] - cellPadding, DescLineHeight),
                        XStringFormats.TopLeft);
                }

                double oreLeft = x + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4];
                gfx.DrawString(w.HoursCounter.ToString("0.##", CultureInfo.InvariantCulture), fontRow, XBrushes.Black,
                    new XRect(oreLeft + 3, y + 2, colWidths[5] - cellPadding, RowMinHeight), XStringFormats.TopRight);

                y += blockHeight;
                gfx.DrawLine(XPens.WhiteSmoke, x, y, x + totalColScaled, y);
            }

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

        private static List<string> WrapText(XGraphics gfx, string text, XFont font, double maxWidthPt)
        {
            var lines = new List<string>();
            if (string.IsNullOrEmpty(text))
            {
                lines.Add(string.Empty);
                return lines;
            }
            if (maxWidthPt <= 0)
            {
                lines.Add(text);
                return lines;
            }

            string[] paragraphs = text.Replace("\r\n", "\n").Split('\n');
            foreach (var paragraph in paragraphs)
            {
                if (lines.Count >= MaxWrappedLinesPerCell) break;

                if (paragraph.Length == 0)
                {
                    lines.Add(string.Empty);
                    continue;
                }

                string[] words = paragraph.Split(' ');
                var current = string.Empty;

                foreach (var word in words)
                {
                    if (lines.Count >= MaxWrappedLinesPerCell) break;

                    if (current.Length == 0)
                    {
                        if (MeasureWidth(gfx, word, font, maxWidthPt) > maxWidthPt)
                        {
                            var rest = word;
                            while (MeasureWidth(gfx, rest, font, maxWidthPt) > maxWidthPt && rest.Length > 0)
                            {
                                int safeSplit = FindSplitIndex(gfx, rest, font, maxWidthPt);
                                if (safeSplit <= 0) safeSplit = 1;
                                if (safeSplit >= rest.Length)
                                {
                                    lines.Add(rest);
                                    rest = string.Empty;
                                    break;
                                }
                                string piece = rest.Substring(0, safeSplit) + "-";
                                lines.Add(piece);
                                rest = rest.Substring(safeSplit);
                            }
                            current = rest;
                        }
                        else
                        {
                            current = word;
                        }
                        continue;
                    }

                    string candidate = current + " " + word;
                    if (MeasureWidth(gfx, candidate, font, maxWidthPt) <= maxWidthPt)
                    {
                        current = candidate;
                    }
                    else
                    {
                        if (MeasureWidth(gfx, word, font, maxWidthPt) > maxWidthPt)
                        {
                            lines.Add(current);
                            current = string.Empty;
                            var rest = word;
                            while (MeasureWidth(gfx, rest, font, maxWidthPt) > maxWidthPt && rest.Length > 0)
                            {
                                int safeSplit = FindSplitIndex(gfx, rest, font, maxWidthPt);
                                if (safeSplit <= 0) safeSplit = 1;
                                if (safeSplit >= rest.Length)
                                {
                                    lines.Add(rest);
                                    rest = string.Empty;
                                    break;
                                }
                                string piece = rest.Substring(0, safeSplit) + "-";
                                lines.Add(piece);
                                rest = rest.Substring(safeSplit);
                            }
                            current = rest;
                        }
                        else
                        {
                            lines.Add(current);
                            current = word;
                        }
                    }
                }

                if (current.Length > 0 && lines.Count < MaxWrappedLinesPerCell)
                    lines.Add(current);
            }

            return lines;
        }

        private static double MeasureWidth(XGraphics gfx, string text, XFont font, double maxWidthPt)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            if (gfx == null || font == null) return EstimateWidth(text, maxWidthPt);

            try
            {
                var size = gfx.MeasureString(text, font);
                double w = size.Width;
                if (w <= 0 || double.IsNaN(w) || double.IsInfinity(w))
                    return EstimateWidth(text, maxWidthPt);
                return w;
            }
            catch
            {
                return EstimateWidth(text, maxWidthPt);
            }
        }

        private static double EstimateWidth(string text, double maxWidthPt)
        {
            const double avgCharWidth = 4.5;
            double w = text.Length * avgCharWidth;
            if (w <= 0 || double.IsNaN(w) || double.IsInfinity(w))
                return maxWidthPt > 0 ? maxWidthPt : 1;
            return w;
        }

        private static int FindSplitIndex(XGraphics gfx, string text, XFont font, double maxWidthPt)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            int lo = 1, hi = text.Length, best = 1;
            int safety = 0;
            while (lo <= hi && safety++ < 64)
            {
                int mid = (lo + hi) / 2;
                if (MeasureWidth(gfx, text.Substring(0, mid), font, maxWidthPt) <= maxWidthPt)
                {
                    best = mid;
                    lo = mid + 1;
                }
                else
                {
                    hi = mid - 1;
                }
            }
            return best;
        }

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
                }
                _fontResolverRegistered = true;
            }
        }
    }
}
