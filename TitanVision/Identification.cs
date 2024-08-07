﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing.Common;
using ZXing;
using ZXing.QrCode.Internal;
using Tesseract;

namespace TitanVision
{
    public class Identification : SharpVision
    {
        ZXing.IBarcodeReader reader;
        TesseractEngine ocr;

        public Bitmap CreateBarCode(string content, string filepath)
        {
            BarcodeWriter barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Height = 100,
                    Width = 200,
                    PureBarcode = true,
                    Margin = 0
                }
            };
            var barCode = barcodeWriter.Write(content);
            barCode.Save(filepath, System.Drawing.Imaging.ImageFormat.Png);
            return barCode;
        }

        public Bitmap CreateQRCode(string content, string filepath)
        {
            ZXing.BarcodeWriter writer = new ZXing.BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new ZXing.QrCode.QrCodeEncodingOptions
                {
                    Height = 250,  
                    Width = 250,    
                    CharacterSet = "UTF-8",
                    ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.M
                }
            };
            var bm = writer.Write(content);
            bm.Save(filepath, System.Drawing.Imaging.ImageFormat.Png);
            return bm;
        }

        /// <summary>
        /// The filepath parameter can be a one-dimensional or two-dimensional code.
        /// </summary>
        public void ReadCode(string filepath, out string result)
        {
            result = null;
            if (File.Exists(filepath))
            {
                reader = new ZXing.BarcodeReader();
                FileStream fs = new FileStream(filepath, FileMode.Open);
                Byte[] data = new Byte[fs.Length];
                fs.Read(data, 0, data.Length);
                fs.Close();
                MemoryStream ms = new MemoryStream(data);
                var bitmap = (Bitmap)Image.FromStream(ms);
                result = reader.Decode(bitmap)?.ToString()??"解碼失敗!";
            }
            else
            {
                Console.WriteLine($"{filepath}檔案不存在!");
            }
        }

        public void OCR(string filepath, string language, out string result)
        {
            result = null;
            if (File.Exists(filepath))
            {
                ocr = new TesseractEngine("./tessdata", language);
                Bitmap bit = new Bitmap(Image.FromFile(filepath));
                Page page = ocr.Process(bit);
                result = page.GetText();
                page.Dispose();
            }
            else
            {
                Console.WriteLine($"{filepath}檔案不存在!");
            }
           
        }
    }
}
