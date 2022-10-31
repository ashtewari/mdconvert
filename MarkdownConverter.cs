using System.Diagnostics;
using System.Dynamic;
using Microsoft.Toolkit.Parsers.Markdown;

internal class MarkdownConverter 
{
    private string parentFilePath;
    private IList<string> links = new List<string>();
    private IList<string> tmpPdfFiles = new List<string>();

    public MarkdownConverter(string rootFolder)
    {
        if(!string.IsNullOrWhiteSpace(rootFolder))
        {
            parentFilePath = rootFolder;
        }
    }

    int folderCounter = 0;

    internal async Task ProcessFolder(string folderToProcess = default(string))
    {
        var folder = string.IsNullOrWhiteSpace(folderToProcess) ? parentFilePath : folderToProcess;
        var folderInfo = new DirectoryInfo(folder);
        if(!folderInfo.Exists) return;
        
        folderCounter++;
        var tmpPath = Path.GetTempPath();
        var sessionId = System.Guid.NewGuid().ToString();
        var sessionFolder = Path.Combine(tmpPath, sessionId);    
        if(folder == parentFilePath)
        {
            sessionId = System.Guid.NewGuid().ToString();
            sessionFolder = Path.Combine(tmpPath, sessionId);
        }       
        
        var tmpFolder = Path.Combine(sessionFolder, folderCounter.ToString());
        Directory.CreateDirectory(tmpFolder);
        foreach (var file in folderInfo.GetFiles("*.md", SearchOption.TopDirectoryOnly))
        {
            Console.WriteLine($"{file.DirectoryName}, {file.Name}");
            
            // Export files to PDF
            ProcessStartInfo startInfo = new ProcessStartInfo();  
            var outFile = string.Format("{0}.{1}", Path.GetFileNameWithoutExtension(file.FullName), "pdf");  
            var outFileFullName = Path.Combine(tmpFolder, outFile);  
            startInfo.FileName = @"pandoc";
            startInfo.Arguments = $"{file.FullName} -f markdown -t pdf --pdf-engine=wkhtmltopdf -o {outFile}";
            startInfo.WorkingDirectory = tmpFolder;
            startInfo.RedirectStandardOutput = false;
            Process.Start(startInfo).WaitForExit(); 
            
            if(!tmpPdfFiles.Contains(outFileFullName))
            {
                tmpPdfFiles.Add(outFileFullName);            
            }
        }

        foreach (var dirInfo in folderInfo.GetDirectories("*", SearchOption.TopDirectoryOnly))
        {
            await ProcessFolder(dirInfo.FullName);
        }

        if(folder == parentFilePath)
        {
            var mergedFile = Path.Combine(parentFilePath, string.Format("merged.{0}.pdf", System.Guid.NewGuid().ToString()));
            PdfTools.MergeFiles(tmpPdfFiles, mergedFile);

            CleanupTempFiles(tmpPdfFiles); 

            Console.WriteLine($"PDF created : {mergedFile}");           
        }       
    }

    private void ConvertFilesToPdf(IList<string> files)
    {      
        var tmpFolder = Path.Combine(Path.GetTempPath(), System.Guid.NewGuid().ToString());
        Directory.CreateDirectory(tmpFolder);
        IList<string> tmpFiles = new List<string>();
        foreach (var file in files)
        {
            Console.WriteLine($"{file}");
            
            // Export files to PDF
            ProcessStartInfo startInfo = new ProcessStartInfo();  
            var outFile = string.Format("{0}.{1}", Path.GetFileNameWithoutExtension(file), "pdf");  
            var outFileFullName = Path.Combine(tmpFolder, outFile);  
            startInfo.FileName = @"pandoc";
            startInfo.Arguments = $"{file} -f markdown -t pdf --pdf-engine=wkhtmltopdf -o {outFile}";
            startInfo.WorkingDirectory = tmpFolder;
            startInfo.RedirectStandardOutput = false;
            Process.Start(startInfo).WaitForExit(); 
            
            if(!tmpFiles.Contains(outFileFullName))
            {
                tmpFiles.Add(outFileFullName);            
            }
        }        

        var mergedFile = Path.Combine(parentFilePath, string.Format("merged.{0}.pdf", System.Guid.NewGuid().ToString()));
        PdfTools.MergeFiles(tmpFiles, mergedFile);

        CleanupTempFiles(tmpFiles);

        Console.WriteLine($"PDF created : {mergedFile}"); 
    }

    private void CleanupTempFiles(IList<string> files)
    {
            foreach (var tf in files)
            {
                Console.WriteLine($"Deleting :: {tf}");
                File.Delete(tf);
            } 

            foreach (var tf in files)
            {
                var folderToBeDeleted = Path.GetDirectoryName(tf);
                if(folderToBeDeleted != parentFilePath) {
                    var dirInfo = new DirectoryInfo(folderToBeDeleted);
                    if(dirInfo.Exists)
                    {
                        dirInfo.Delete();
                    }
                }
            }                   
    }  

    internal async Task ProcessFile(string markdownFilePath)
    {
        links.Clear();
        var files = await this.GetLinks(markdownFilePath);           
        this.ConvertFilesToPdf(files);
    } 

    private async Task<IList<string>> GetLinks(string markdownFilePath)
    {
        if(!File.Exists(markdownFilePath)) return new List<string>();            

        links.Add(markdownFilePath);
        MarkdownDocument document = new MarkdownDocument();
        document.Parse(await File.ReadAllTextAsync(markdownFilePath));
        foreach (var element in document.Blocks)
        {
            await FindLinks(element);
        }

        return links;
    }

    private async Task FindLinks(dynamic element)
    {
        if(HasProperty(element, "Url"))
        {
            var url = element.Url.ToString();
            if(string.IsNullOrWhiteSpace(url)) return;
            var fi = GetFileInfo(url);
            if(fi.Exists) 
            {
                links.Add(fi.FullName);
            }
            await new MarkdownConverter(parentFilePath).GetLinks(fi.FullName);            
        } 
        if(HasProperty(element, "Cells"))
        {
            foreach (var row in element.Cells)
            {
                FindLinks(row);
            }
        }             
        else if(HasProperty(element, "Rows"))
        {
            foreach (var row in element.Rows)
            {
                FindLinks(row);
            }
        }                                  
        else if(HasProperty(element, "Inlines"))
        {
            foreach (var inline in element.Inlines)
            {
                FindLinks(inline);
            }
        }                      
    }

    private FileInfo GetFileInfo(string stringPath)
    {
        if (stringPath.StartsWith("http:\\") || stringPath.StartsWith("https:\\"))
        {
            return new FileInfo("");
        }
        var absFilePath = System.IO.Path.Combine(parentFilePath, stringPath);
        var fi = new FileInfo(absFilePath);
        Console.WriteLine($">>>>>> {fi.FullName}");
        return fi;
    }    

    private static bool HasProperty(dynamic obj, string name)
    {
        if (obj is ExpandoObject)
        return ((IDictionary<string, object>)obj).ContainsKey(name);

        return obj.GetType().GetProperty(name) != null;
    }         
}