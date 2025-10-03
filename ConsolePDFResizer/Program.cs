using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsolePDFResizer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int ImageQuality=25;
            var sourceFile = @"D:\__\а1\+\IMG_0001.pdf";
            var destinationFile = @"D:\__\а1\+\IMG_00021.pdf";
          //  Console.Write("IQ2 " + ImageQuality.ToString());

            //#if !DEBUG
            // Проверяем количество аргументов
            if (args.Length != 3)
            {
                Console.WriteLine("Использование: ConsolePDFResizer.exe <исходный_файл> <целевой_файл> <число>");
                Console.WriteLine("Пример: FileCopyWithNumber.exe input.pdf  output.pdf 25");
                return;
            }


             sourceFile = args[0];
             destinationFile = args[1];
            // Проверяем существование исходного файла
            if (!File.Exists(sourceFile))
            {
                Console.WriteLine($"Ошибка: файл '{sourceFile}' не найден!");
                //  return;
            }

            // Пытаемся преобразовать третий аргумент в число
            if (!int.TryParse(args[2], out ImageQuality))
            {
                Console.WriteLine("ImageQuality по умолчанию 25");
             //   return;
            }

            Console.Write( "IQ " + ImageQuality.ToString() );

            
//#endif


            try
            {
                  
                    var wer = PDFCompressHelper.CompressDecompressSinglePDF(sourceFile, destinationFile, null, true, ImageQuality, false, false);

                if (wer == null)  wer="";
                if (wer.Length >1)
                    Console.WriteLine(wer);
                else
                    Console.WriteLine($" : файл '{destinationFile}' Создан корверсия Q= {ImageQuality}");

            }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
       

        }
    }
}
