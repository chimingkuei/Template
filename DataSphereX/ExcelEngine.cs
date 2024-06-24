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

        /// <summary>
        /// Reading the value of a cell from a specified worksheet in an Excel file.
        /// </summary>
        /// <param name="filepath">
        /// The path to the Excel file.
        /// </param>
        /// <param name="sheetname">
        /// The name or index of the worksheet. This parameter can be either a string (worksheet name) or an int (worksheet index).
        /// </param>
        /// <param name="searchmodel">
        /// The search model to use. "all" retrieves all data from the worksheet, "one" searches for a specific value in the specified column.
        /// </param>
        /// <param name="data">
        /// Output parameter to hold the data read from the Excel sheet.
        /// </param>
        /// <param name="tuple">
        /// A tuple containing the column index and search value for the "one" search model.
        /// </param>
        public void ReadExcel<T>(string filepath, object sheetname, string searchmodel, out List<T> data, Tuple<int, string> tuple = null)
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
                            var contstr = worksheet.Cells[row, tuple.Item1].Value;
                            if (contstr != null)
                            {
                                if (contstr.ToString() == tuple.Item2)
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

        public void ModifyExcel(string filepath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // 路徑到Excel文件
            var fileInfo = new FileInfo(filepath);
            // 使用ExcelPackage讀取Excel文件
            using (var package = new ExcelPackage(fileInfo))
            {
                // 取得第一個工作表
                var worksheet = package.Workbook.Worksheets[0];
                // 修改單元格的值
                worksheet.Cells[1, 1].Value = "Hello, world!";
                // 保存Excel文件
                package.Save();
            }
        }
    }

}
