

class Program
    {
        static void Main(string[] args)
        {
            // 1. Start at a specified root folder
            // 2. Make an ordered list of (md) files in the root folder
            // 3. Convert each file to PDF
            // 4. Create list of links in each file
            // 5. Select links resolving to a path in the root folder
            // 6. Repeat steps 2 to 4 for file at each link recursively upto the configured depth
            // 7. Merge all pdf files into one file
            if(args.Length > 0)
            {
                if(args.Length > 0)
                {
                    string inputPath = args[0];

                    FileAttributes attr = File.GetAttributes(inputPath);
                    if (attr.HasFlag(FileAttributes.Directory))
                    {
                        new Converter().ConvertFolder(inputPath).Wait();
                    }
                    else
                    {
                        new Converter().ConvertFile(inputPath).Wait();
                    }
                    return;    
                }
                else
                {
                    Console.Write("No folder or file specified. Exiting ..");
                }
            }
        }
    }

    class Converter
    {
        public async Task ConvertFolder(string folder) {
            Console.WriteLine($"Converting Folder {folder} ..");
            var di = new DirectoryInfo(folder);
            await new MarkdownConverter(di.FullName).ProcessFolder();
        }  

        public async Task ConvertFile(string file) {
            Console.WriteLine($"Converting File {file} ..");
            var fi = new FileInfo(file);
            await new MarkdownConverter(fi.DirectoryName).ProcessFile(fi.FullName);
        }         
    }