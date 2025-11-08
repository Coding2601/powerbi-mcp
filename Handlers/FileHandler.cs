using System.IO.Compression;

namespace PowerBI_MCP.Handlers
{
    public class FileHandler
    {
        public bool CheckFileExists(string filePath)
        {
            if (filePath == null || filePath == "")
                return false;
            return File.Exists(filePath);
        }

        public void ExtractZipFile(string powerBiFilePath, string extractionPath)
        {
            if (!File.Exists(powerBiFilePath))
                throw new FileNotFoundException("File not found", powerBiFilePath);

            Directory.CreateDirectory(extractionPath);

            using (FileStream fs = new FileStream(powerBiFilePath, FileMode.Open, FileAccess.Read))
            using (ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Read))
            {
                archive.ExtractToDirectory(extractionPath, overwriteFiles: true);
            }
        }
    }
}