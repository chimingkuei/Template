using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
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
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            if (!File.Exists(filepath))
            {
                var excelFile = new ExcelPackage();
                var worksheet = excelFile.Workbook.Worksheets.Add(sheetname);
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
                excelFile.SaveAs(new FileInfo(filepath));
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
                var fileInfo = new FileInfo(filepath);
                using (var package = new ExcelPackage(fileInfo))
                {
                    ExcelWorksheet worksheet = null;
                    if (sheetname is string)
                    {
                        worksheet = package.Workbook.Worksheets[(string)sheetname];
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
                            worksheet = package.Workbook.Worksheets[(int)sheetname];
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
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var fileInfo = new FileInfo(filepath);
            using (var package = new ExcelPackage(fileInfo))
            {
                var worksheet = package.Workbook.Worksheets[0];
                worksheet.Cells[row, col].Value = modify;
                package.Save();
            }
        }
    }

}
