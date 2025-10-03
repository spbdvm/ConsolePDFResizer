Отлично\! Я внесу это уточнение в раздел "Основные возможности" и "Основные настройки" на обоих языках.

Вот обновленный файл **README.md**:

-----

# ConsolePDFResizer 📄💾

**RU:** Простая и эффективная консольная утилита для сжатия PDF-файлов с использованием **iTextSharp**.

**EN:** A simple and efficient console utility for compressing PDF files using **iTextSharp**.

-----

## ✨ Основные возможности / Key Features

  * **RU:** **Сжатие PDF:** Снижение размера файла за счет оптимизации внутренних ресурсов документа.
  * **EN:** **PDF Compression:** Reduces file size by optimizing the document's internal resources.
  * **RU:** **Оптимизация изображений (JPEG):** Сжатие встроенных растровых изображений до заданного уровня качества. **Для сильного сжатия** используйте параметры качества в диапазоне **10-20**.
  * **EN:** **Image Optimization (JPEG):** Compresses embedded raster images to a specified quality level. **For aggressive compression**, use quality parameters in the **10-20** range.
  * **RU:** **Полное сжатие iTextSharp:** Использование `PdfStamper.SetFullCompression()` для максимального уменьшения размера.
  * **EN:** **Full iTextSharp Compression:** Utilizes `PdfStamper.SetFullCompression()` for maximum size reduction.
  * **RU:** **Режим перезаписи:** Возможность перезаписать исходный файл с автоматическим созданием резервной копии (`.bak.pdf`).
  * **EN:** **Overwrite Mode:** Option to overwrite the original file with an automatic backup creation (`.bak.pdf`).

-----

## ⚙️ Использование / Usage

**RU:** Поскольку это консольное приложение, основной режим работы — передача параметров через командную строку.

**EN:** As a console application, the primary mode of operation is passing parameters via the command line.

### Основные настройки / Core Settings

| Параметр (RU) | Parameter (EN) | Описание (RU) | Description (EN) | Значения |
| :--- | :--- | :--- | :--- | :--- |
| `CmdOverwritePDF` | `CmdOverwritePDF` | Перезаписать исходный файл. | Overwrite the source file. | `true`/`false` |
| `CmdOutputFolder` | `CmdOutputFolder` | Указать другую выходную папку. | Specify an alternate output folder. | Путь к папке / Folder Path |
| `CmdImageQuality` | `CmdImageQuality` | Качество JPEG-сжатия для встроенных изображений (**0-100**). **10-20** для максимального сжатия. | JPEG compression quality for embedded images (**0-100**). **10-20** for maximum compression. | **10-100** (По умолчанию: 80) |

### Пример вызова / Example Invocation

**RU:** В реальном консольном приложении вы должны реализовать парсинг аргументов командной строки, которые затем используются для вызова основного метода: `PDFCompressHelper.CompressDecompressMultiplePDFCmd`.

**EN:** In a practical console application, you should implement command-line argument parsing, which is then used to call the main method: `PDFCompressHelper.CompressDecompressMultiplePDFCmd`.

```bash
# Пример: сжать файл с агрессивным качеством 15% и сохранить в папку 'Output'
# Example: compress a file with aggressive quality 15% and save to 'Output' folder
ConsolePDFResizer.exe C:\path\to\large_file.pdf --quality 15 --output C:\Output
```

-----

## 🛠️ Сборка и зависимости / Build and Dependencies

Проект разработан на C\# и использует библиотеку **iTextSharp (версии 5.x)** для работы с PDF.

The project is developed in C\# and uses the **iTextSharp (version 5.x)** library for PDF manipulation.

*(... остальная часть секции "Сборка и зависимости" без изменений ...)*

-----

## 💡 Принцип работы / Working Principle

**RU:** Программа использует многопроходный подход для достижения максимального сжатия:

1.  **Сжатие изображений (`ReduceResolution`):** Извлекает и перекодирует встроенные изображения с использованием **JPEG-кодека** и заданного уровня `quality`, заменяя оригинальный поток на сжатый.
2.  **Оптимизация (`PdfStamper`):** Применяет `PdfStamper` с максимальным уровнем сжатия (`CompressionLevel = 9`) и функцией **`SetFullCompression()`**.
3.  **Финальная сборка (`PdfSmartCopy`):** Удаление неиспользуемых объектов (`RemoveUnusedObjects()`) и окончательная структурация документа для дополнительного выигрыша в размере.

**EN:** The program uses a multi-pass approach to achieve maximum compression:

1.  **Image Compression (`ReduceResolution`):** Extracts and re-encodes embedded images using the **JPEG codec** and the specified `quality` level, replacing the original stream with the compressed one.
2.  **Optimization (`PdfStamper`):** Applies `PdfStamper` with maximum compression (`CompressionLevel = 9`) and the **`SetFullCompression()`** function.
3.  **Final Assembly (`PdfSmartCopy`):** Removes unused objects (`RemoveUnusedObjects()`) and finalizes the document structure for an additional size reduction benefit.

-----

## ✍️ Лицензия / License

**RU:** Этот проект распространяется под лицензией AGPL 

**EN:** This project is licensed under the AGPL  License.
