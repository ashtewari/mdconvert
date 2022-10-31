using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

class PdfTools
{   
    public static void MergeFiles(IList<string> files, string outFile)
    {
        using (var merged_pdf = new PdfDocument())
        {
            foreach (var file in files)
            {
                var source_pdf = PdfReader.Open(file, PdfDocumentOpenMode.Import);
    
                for (int i = 0; i < source_pdf.PageCount; i++)
                {
                    merged_pdf.AddPage(source_pdf.Pages[i]);
                }
            }
    
            merged_pdf.Save(outFile);
        }
    }
}