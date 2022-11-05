# mdconvert

Converts markdown files in a folder to a single pdf file. 

# Usage 
* Convert markdown files in a folder (recursively) to pdf
 
> cmd> mdconvert.exe "c:\temp\folder-to-convert"

* Convert a markdown file and only (local) linked markdown files (recursively) to pdf

> cmd> mdconvert.exe "c:\temp\root-folder\file-in-root-folder.md"

# Dependencies
Uses pandoc and wkhtmltopdf, install these tools before executing mdconvert.exe
* Install pandoc - https://pandoc.org/installing.html
* Install wkhtmltopdf - https://wkhtmltopdf.org/downloads.html
