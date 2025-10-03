using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Drawing; // Остаётся для System.Drawing.Image, Bitmap и ImageCodecInfo
using System.Drawing.Imaging; // Добавлено явно для ImageCodecInfo, Encoder, EncoderParameter


namespace ConsolePDFResizer
{
    class PDFCompressHelper
    {
        static bool KeepFolderStructure = true;
        static bool CmdOverwritePDF = true;
        static string CmdOutputFolder = null;

        // Поля информации о документе не используются в логике, но оставлены как статические поля.
        public static string Title = "";
        public static string Author = "";
        public static string Subject = "";
        public static string Keywords = "";

        public static DateTime CreationDate = DateTime.Now;
        public static DateTime ModificationDate = DateTime.Now;
        public static string Creator = "";

        public static bool SetDocInfo = false;

        public static bool CANCELLED = false;

        public static string DefaultPassword = "";

        public static string ErrMultiple = "";

        public static long BytesBefore = 0;
        public static long BytesAfter = 0;

        public static string CompressDecompressMultiplePDF(DataTable dt, string OutputDir, bool compress_images, int images_quality, bool overwrite, bool keep_backup)
        {
            string err = "";

            CANCELLED = false;
            BytesBefore = 0;
            BytesAfter = 0;

            for (int k = 0; k < dt.Rows.Count; k++)
            {
                if (CANCELLED)
                {
                    return err;
                }

                string outfilepath = "";
                string filepath = dt.Rows[k]["fullfilepath"].ToString();
                string password = dt.Rows[k]["userpassword"].ToString();
                string rootfolder = dt.Rows[k]["rootfolder"].ToString();

                if (OutputDir.Trim() == ("Same Folder of PDF Document"))
                {
                    string dirpath = System.IO.Path.GetDirectoryName(filepath);

                    // Оставлена логика для генерации "_compressed.pdf" в той же папке
                    outfilepath = System.IO.Path.Combine(dirpath, System.IO.Path.GetFileNameWithoutExtension(filepath) + "_compressed.pdf");
                }
                else if (OutputDir.StartsWith(("Subfolder") + " : "))
                {
                    int subfolderspos = (("Subfolder") + " : ").Length;
                    string subfolder = OutputDir.Substring(subfolderspos);

                    outfilepath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filepath) + "\\" + subfolder, System.IO.Path.GetFileName(filepath));
                }
                else if (overwrite)
                {
                    outfilepath = filepath;
                }
                else
                {
                    if (rootfolder != string.Empty && KeepFolderStructure)
                    {
                        string dep = System.IO.Path.GetDirectoryName(filepath).Substring(rootfolder.Length);

                        string outdfp = OutputDir + dep;

                        outfilepath = System.IO.Path.Combine(outdfp, System.IO.Path.GetFileName(filepath));

                        if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(outfilepath)))
                        {
                            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outfilepath));
                        }
                    }
                    else
                    {
                        outfilepath = System.IO.Path.Combine(OutputDir, System.IO.Path.GetFileName(filepath));
                    }
                }

                try
                {
                    err += CompressDecompressSinglePDF(filepath, outfilepath, password, compress_images, images_quality, overwrite, keep_backup);
                }
                catch (Exception ex)
                {
                    err += ex.Message + "\r\n";
                }

            }

            return err;
        }

        public static string CompressDecompressMultiplePDFCmd(DataTable dt, string CmdOutputFile, int CmdImageQuality)
        {
            string err = "";
            BytesBefore = 0;
            BytesAfter = 0;
            bool CmdCompressImages = true;

            for (int k = 0; k < dt.Rows.Count; k++)
            {

                Console.WriteLine("[" + DateTime.Now.ToString() + "] Compressing PDF file : " + dt.Rows[k]["fullfilepath"].ToString());

                string outfilepath = "";
                string filepath = dt.Rows[k]["fullfilepath"].ToString();
                string password = dt.Rows[k]["userpassword"].ToString();

                if (CmdOutputFolder == string.Empty)
                {
                    string dirpath = System.IO.Path.GetDirectoryName(filepath);

                    // Предполагаем, что всегда происходит сжатие, т.к. это "ConsolePDFResizer"
                    if (CmdOutputFile == string.Empty)
                    {
                        outfilepath = System.IO.Path.Combine(dirpath, System.IO.Path.GetFileNameWithoutExtension(filepath) + "_compressed.pdf");
                    }
                    else
                    {
                        outfilepath = System.IO.Path.Combine(dirpath, CmdOutputFile.Replace("[FILENAME]", System.IO.Path.GetFileNameWithoutExtension(filepath)).Replace("[EXT]", System.IO.Path.GetExtension(filepath)));
                    }
                }

                if (CmdOverwritePDF)
                {
                    outfilepath = filepath;
                }

                if (CmdOutputFolder != string.Empty)
                {
                    outfilepath = System.IO.Path.Combine(CmdOutputFolder, System.IO.Path.GetFileName(filepath));
                }

                try
                {
                    err += CompressDecompressSinglePDF(filepath, outfilepath, password, CmdCompressImages, CmdImageQuality, CmdOverwritePDF, false);
                }
                catch (Exception ex)
                {
                    err += ex.Message + "\r\n";
                }
            }

            return err;
        }


        public static string CompressDecompressSinglePDF(string InputFile, string OutputFile, string Password, bool CompressImages, int ImageQuality, bool Overwrite, bool KeepBackup)
        {
            string err = "";
            PdfReader reader = null;
            string OutputFile2 = "";

            try
            {
                System.IO.FileInfo fi = new FileInfo(InputFile);

                DateTime dtcreated = fi.CreationTime;
                DateTime dtlastmod = fi.LastWriteTime;

                // Создание выходной директории, если она не существует
                if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(OutputFile)))
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(OutputFile));
                    }
                    catch (Exception exd)
                    {
                        err += ("Error. Could not Create Directory for File") + " : " + InputFile + "\r\n" + exd.Message + "\r\n";
                        return err;
                    }
                }


                try
                {
                    // Попытка открыть PDF-файл, с паролем или без
                    while (true)
                    {
                        using (Stream input = new FileStream(InputFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            try
                            {
                                if (!String.IsNullOrEmpty(Password))
                                {
                                    reader = new PdfReader(input, Encoding.Default.GetBytes(Password));
                                }
                                else
                                {
                                    reader = new PdfReader(input);
                                }

                                break;
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                    }

                    string TmpFile = "";

                    if (Overwrite)
                    {
                        TmpFile = OutputFile;
                        OutputFile = System.IO.Path.GetTempFileName(); // Временный файл для перезаписи
                    }

                    if (CompressImages)
                    {
                        // Применяем сжатие изображений
                        ReduceResolution(reader, (long)ImageQuality);
                    }

                    OutputFile2 = OutputFile + ".temp.pdf";

                    if (CANCELLED)
                    {
                        return err;
                    }

                    // 1. Повторное сжатие / оптимизация с использованием PdfStamper
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        PdfStamper stamper = new PdfStamper(reader, memoryStream, PdfWriter.VERSION_1_5);
                        stamper.Writer.CompressionLevel = 9;
                        stamper.FormFlattening = true;
                        stamper.SetFullCompression();

                        int total = reader.NumberOfPages + 1;
                        for (int i = 1; i < total; i++)
                        {
                            if (CANCELLED)
                            {
                                stamper.Close();
                                reader.Close();
                                return err;
                            }

                            // Этот вызов сам по себе может быть частью оптимизации/копирования
                            reader.SetPageContent(i, reader.GetPageContent(i));
                        }
                        stamper.SetFullCompression();
                        stamper.Close();
                        reader.Close();

                        // Сохраняем результат в промежуточный файл
                        File.WriteAllBytes(OutputFile2, memoryStream.ToArray());
                    }


                    if (CANCELLED)
                    {
                        return err;
                    }

                    // 2. Открытие промежуточного файла
                    using (Stream input = new FileStream(OutputFile2, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        try
                        {
                            if (!String.IsNullOrEmpty(Password))
                            {
                                reader = new PdfReader(input, Encoding.Default.GetBytes(Password));
                            }
                            else
                            {
                                reader = new PdfReader(input);
                            }
                        }
                        catch (Exception exr2)
                        {
                            err += "Error. Could not read PDF File ! " + OutputFile2 + " " + exr2.Message + "\n\n";
                            return err;
                        }
                    }

                    // 3. Финальная оптимизация с использованием PdfSmartCopy
                    using (Stream outputPdfStream = new FileStream(OutputFile, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        Document document = new Document();
                        PdfSmartCopy copy = new PdfSmartCopy(document, outputPdfStream);
                        copy.CompressionLevel = 9;
                        copy.SetFullCompression();
                        try
                        {
                            reader.RemoveUnusedObjects(); // Удаление неиспользуемых объектов
                        }
                        catch { }

                        document.Open();

                        int totalPageCnt = reader.NumberOfPages;

                        for (int t = 1; t <= totalPageCnt; t++)
                        {
                            if (CANCELLED)
                            {
                                reader.Close();
                                copy.Close();
                                document.Close();
                                return err;
                            }
                            copy.AddPage(copy.GetImportedPage(reader, t));
                        }

                        copy.FreeReader(reader);
                        reader.Close();
                        copy.Close();
                        document.Close();
                    }


                    if (CANCELLED)
                    {
                        return err;
                    }

                    // Обработка перезаписи файла (создание/удаление резервной копии)
                    if (TmpFile != string.Empty)
                    {
                        if (KeepBackup)
                        {
                            string bakfilepath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(TmpFile),
                            System.IO.Path.GetFileNameWithoutExtension(TmpFile) + ".bak.pdf");

                            System.IO.File.Move(TmpFile, bakfilepath); // Переименовываем оригинал в бэкап
                        }
                        else
                        {
                            System.IO.File.Delete(TmpFile); // Удаляем оригинал
                        }
                        System.IO.File.Move(OutputFile, TmpFile); // Переименовываем оптимизированный файл во имя оригинала

                        OutputFile = TmpFile;
                    }

                    // Подсчет размера
                    try
                    {
                        if (System.IO.File.Exists(InputFile) && System.IO.File.Exists(OutputFile))
                        {
                            FileInfo fibefore = new FileInfo(InputFile);
                            BytesBefore += fibefore.Length;

                            FileInfo fiafter = new FileInfo(OutputFile);
                            BytesAfter += fiafter.Length;
                        }
                    }
                    catch
                    { }

                    // Восстановление даты создания/изменения
                    FileInfo fi2 = new FileInfo(OutputFile);
                    fi2.CreationTime = dtcreated;
                    fi2.LastWriteTime = dtlastmod;

                    return err;
                }
                catch (Exception exm)
                {
                    err += ("An error occured while protecting PDF File") + " : " + InputFile + "\r\n" + exm.Message + "\r\n";
                    return err;
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }

                    // Удаление промежуточного файла
                    if (System.IO.File.Exists(OutputFile2))
                    {
                        try
                        {
                            System.IO.File.Delete(OutputFile2);
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                err += ("An error occured while compressing PDF File") + " : " + InputFile + "\r\n" + ex.Message + "\r\n";
            }

            return err;
        }

        // Метод сжатия/изменения разрешения изображений с использованием System.Drawing (JPEG-сжатие)
        public static void ReduceResolution(PdfReader reader, long quality)
        {
            int n = reader.XrefSize;
            for (int i = 0; i < n; i++)
            {
                if (CANCELLED)
                {
                    return;
                }

                PdfObject obj = reader.GetPdfObject(i);
                if (obj == null || !obj.IsStream()) { continue; }

                PdfDictionary dict = (PdfDictionary)PdfReader.GetPdfObject(obj);
                PdfName subType = (PdfName)PdfReader.GetPdfObject(
                  dict.Get(PdfName.SUBTYPE)
                );
                if (!PdfName.IMAGE.Equals(subType)) { continue; }

                PRStream stream = (PRStream)obj;
                try
                {
                    PdfImageObject image = new PdfImageObject(stream);
                    System.Drawing.Image img = image.GetDrawingImage();
                    if (img == null) continue;

                    int width = img.Width;
                    int height = img.Height;

                    using (System.Drawing.Bitmap dotnetImg =
                        new System.Drawing.Bitmap(img))
                    {
                        // set codec to jpeg type => jpeg index codec is "1"
                        System.Drawing.Imaging.ImageCodecInfo codec =
                        System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()[1];
                        // set parameters for image quality
                        System.Drawing.Imaging.EncoderParameters eParams =
                          new System.Drawing.Imaging.EncoderParameters(1);
                        eParams.Param[0] =
                          new System.Drawing.Imaging.EncoderParameter(
                             System.Drawing.Imaging.Encoder.Quality, quality
                          );

                        using (MemoryStream msImg = new MemoryStream())
                        {
                            dotnetImg.Save(msImg, codec, eParams);
                            msImg.Position = 0;
                            // Устанавливаем данные, фильтр и метаданные для потока PRStream
                            stream.SetData(msImg.ToArray(), false, PRStream.BEST_COMPRESSION);
                            stream.Put(PdfName.TYPE, PdfName.XOBJECT);
                            stream.Put(PdfName.SUBTYPE, PdfName.IMAGE);
                            stream.Put(PdfName.FILTER, PdfName.DCTDECODE); // DCTDECODE для JPEG
                            stream.Put(PdfName.WIDTH, new PdfNumber(width));
                            stream.Put(PdfName.HEIGHT, new PdfNumber(height));
                            stream.Put(PdfName.BITSPERCOMPONENT, new PdfNumber(8));
                            stream.Put(PdfName.COLORSPACE, PdfName.DEVICERGB);
                        }
                    }
                }
                catch
                {
                    // iTextSharp/System.Drawing не может обработать все типы изображений
                }
                finally
                {
                    // Может помочь очистить память или оптимизировать
                    reader.RemoveUnusedObjects();
                }
            }
        }
    }
}