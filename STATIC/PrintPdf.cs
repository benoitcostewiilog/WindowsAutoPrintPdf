using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsAutoPrintPdf.STATIC
{
    class  PrintPdf
    {
        public static bool PrintPDF(
        string printer,
        string paperName,
        string filename,
        int copies)
    {
        try
        {
            // Create the printer settings for our printer
            var printerSettings = new PrinterSettings
            {
                PrinterName = printer,
                Copies = (short)copies,
            };

            // Create our page settings for the paper size selected
            var pageSettings = new PageSettings(printerSettings)
            {
                Margins = new Margins(0, 0, 0, 0),
            };
                /*
            foreach (PaperSize paperSize in printerSettings.PaperSizes)
            {
                if (paperSize.PaperName == paperName)
                {
                    pageSettings.PaperSize = paperSize;
                    break;
                }
            }
                */
                //Set pageSettings.PaperSize 
                pageSettings.PaperSize = new PaperSize(paperName, 10, 20);

            // Now print the PDF document
            using (var document = PdfDocument.Load(filename))
            {
                using (var printDocument = document.CreatePrintDocument())
                {
                    printDocument.PrinterSettings = printerSettings;
                    printDocument.DefaultPageSettings = pageSettings;
                    printDocument.PrintController = new StandardPrintController();
                    printDocument.Print();
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}
}
