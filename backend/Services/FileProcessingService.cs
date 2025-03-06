using System;
using System.IO;
using TikaOnDotNet.TextExtraction;

public class FileProcessingService
{
    public string ExtractTextFromFile(Stream fileStream)
    {
        string tempFilePath = Path.GetTempFileName(); // Cria um arquivo temporário

        try
        {
            // Salva o conteúdo do stream no arquivo temporário
            using (var fileStreamOutput = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
            {
                fileStream.CopyTo(fileStreamOutput);
            }

            var textExtractor = new TextExtractor();
            var result = textExtractor.Extract(tempFilePath); // Agora passamos o caminho do arquivo

            return result.Text;
        }
        finally
        {
            // Remove o arquivo temporário após o processamento
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }
}
