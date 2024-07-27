using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace DataSphereX
{
    public class ExcelEngine
    {
        private void SetCellAppearance(ExcelWorksheet worksheet, int row, int col)
        {
            worksheet.Cells[row, col].Style.Font.Bold = true;
            worksheet.Cells[row, col].Style.Font.Size = 12;
            worksheet.Cells[row, col].Style.Font.Color.SetColor(Color.Black);
            worksheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.AliceBlue);
        }

        /// <summary>
        /// ExcelEngine excel = new ExcelEngine();
        /// List<string> heater = new List<string> { "值1", "值2", "值3" };
        /// List<List<string>> data = new List<List<string>>
        /// {
        ///     new List<string> { "A1", "B1", "C1" },
        ///     new List<string> { "A2", "B2", "C2" },
        ///     new List<string> { "A3", "B3", "C3" }
        /// };
        /// excel.CreateExcel(@"E:\Temp\test.xlsx", "Test", heater, data);
        /// List<List<string>> data;
        /// </summary>
        public void CreateExcel(string filepath, string sheetname, List<string>header = null, List<List<string>> data = null)
        {
            if (!File.Exists(filepath))
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var excel = new ExcelPackage();
                var worksheet = excel.Workbook.Worksheets.Add(sheetname);
                if (header != null)
                {
                    for (int index = 0; index < header.Count; index++)
                    {
                        worksheet.Cells[1, index + 1].Value = header[index];
                        SetCellAppearance(worksheet, 1, index + 1);
                    }
                }
                if (data != null)
                {
                    for (int row = 1; row < data.Count + 1; row++)
                    {
                        for (int col = 0; col < data[row - 1].Count; col++)
                        {
                            worksheet.Cells[row + 1, col + 1].Value = data[row - 1][col];
                            SetCellAppearance(worksheet, row + 1, col + 1);
                        }
                    }

                }
                excel.SaveAs(new FileInfo(filepath));
            }
            else
            {
                Console.WriteLine($"{filepath}檔案已存在，不允許覆蓋!");
            }
        }

        public void ReadExcel<T>(string filepath, object sheetname, string searchmodel, out List<T> data, Tuple<int, string> validation= null)
        {
            data = new List<T>();
            if (File.Exists(filepath))
            {     
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var excelfile = new FileInfo(filepath);
                using (var excel = new ExcelPackage(excelfile))
                {
                    ExcelWorksheet worksheet = null;
                    if (sheetname is string)
                    {
                        worksheet = excel.Workbook.Worksheets[(string)sheetname];
                        if (worksheet == null)
                        {
                            Console.WriteLine($"{(string)sheetname}工作表找不到!");
                            return;
                        }
                    }
                    else if (sheetname is int)
                    {
                        try
                        {
                            worksheet = excel.Workbook.Worksheets[(int)sheetname];
                        }
                        catch
                        {
                            Console.WriteLine($"工作表{(int)sheetname}找不到!");
                            return;
                        }
                    }
                    if (worksheet.Dimension == null)
                    {
                        if (sheetname is string)
                        {
                            Console.WriteLine($"Excel工作表{(string)sheetname}為空!");
                        }
                        else if (sheetname is int)
                        {
                            Console.WriteLine($"Excel工作表{(int)sheetname}為空!");
                        }
                        return;
                    }
                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;
                    if (searchmodel == "all")
                    {
                        for (int row = 1; row <= rowCount; row++)
                        {
                            List<string> row_data = new List<string>();
                            for (int col = 1; col <= colCount; col++)
                            {
                                var value = worksheet.Cells[row, col].Value;
                                //Console.WriteLine($"Cell [{row}, {col}] value: {value}");
                                row_data.Add((string)value);
                            }
                            data.Add((T)(object)row_data);
                        }
                    }
                    else if (searchmodel == "one")
                    {
                        for (int row = 1; row <= rowCount; row++)
                        {
                            var contstr = worksheet.Cells[row, validation.Item1].Value;
                            if (contstr != null)
                            {
                                if (contstr.ToString() == validation.Item2)
                                {
                                    data.Add((T)(object)row);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"{filepath}檔案不存在!");
            }
        }

        public void ModifyExcel(string filepath, int row, int col, string modify)
        {
            if (!File.Exists(filepath))
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var excelfile = new FileInfo(filepath);
                using (var excel = new ExcelPackage(excelfile))
                {
                    var worksheet = excel.Workbook.Worksheets[0];
                    worksheet.Cells[row, col].Value = modify;
                    excel.Save();
                }
            }
            else
            {
                Console.WriteLine($"{filepath}檔案已存在，不允許覆蓋!");
            }
        }

        private bool CheckChartName(ExcelWorksheet worksheet, string chartname)
        {
            foreach (ExcelDrawing drawing in worksheet.Drawings)
            {
                if (drawing.Name == chartname)
                {
                    return true;
                }
            }
            return false;
        }

        private void SetChartAppearance(ExcelChart chart, Rectangle rect)
        {
            chart.SetPosition(rect.X, 0, rect.Y, 0);
            chart.SetSize(rect.Width, rect.Height);
            chart.Title.Text = "Title";
            chart.Legend.Position = eLegendPosition.Right;
            chart.XAxis.MajorGridlines.Fill.Color = Color.LightGray;
            chart.XAxis.MajorUnit = 10;
            chart.XAxis.Title.Text = "X Axis";
            chart.YAxis.MajorGridlines.Fill.Color = Color.LightGray;
            chart.YAxis.MajorUnit = 10;
            chart.YAxis.Title.TextVertical = eTextVerticalType.Vertical270;
            chart.YAxis.Title.Text = "Y Axis";
        }

        public void DrawXYScatter(string filepath, string chartnameGp, Rectangle rect)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var excelfile = new FileInfo(filepath);
            var excel = new ExcelPackage(excelfile);
            var worksheet = excel.Workbook.Worksheets[0];
            if (!CheckChartName(worksheet, chartnameGp))
            {
                ExcelChart chart = worksheet.Drawings.AddChart(chartnameGp, eChartType.XYScatter);
                SetChartAppearance(chart, rect);
                var axis_x = worksheet.Cells["A1:A8"];
                var axis_y = worksheet.Cells["B1:B8"];
                var axis_series = (ExcelScatterChartSerie)chart.Series.Add(axis_y, axis_x);
                axis_series.Marker.Style = eMarkerStyle.Circle;
                axis_series.Fill.Color = Color.AliceBlue;
                axis_series.Header = "Category";
            }
            excel.Save();
        }

    }

}
