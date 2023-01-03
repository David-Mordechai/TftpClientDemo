using System.Text;
using Tftp.Net;

AutoResetEvent autoResetEvent = new(false);

const string fileName = "example.txt";

WriteFile(fileName, "testing 2");
var fileContent = ReadFile(fileName);
Console.WriteLine(fileContent);

void WriteFile(string file, string content)
{
    var byteArray = Encoding.ASCII.GetBytes(content);
    var writeStream = new MemoryStream(byteArray);
    try
    {
        var client = new TftpClient("tftp_server");

        var transfer = client.Upload(file);

        transfer.OnProgress += TransferOnProgress;
        transfer.OnFinished += TransferOnFinished;
        transfer.OnError += TransferOnError;
        transfer.RetryCount = 1;
        transfer.RetryTimeout = TimeSpan.FromSeconds(1);

        transfer.Start(writeStream);

        autoResetEvent.WaitOne();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }

    void TransferOnProgress(ITftpTransfer t, TftpTransferProgress progress)
    {
        Console.WriteLine("Transfer running. Progress: " + progress);
    }

    void TransferOnFinished(ITftpTransfer t)
    {
        Console.WriteLine("Transfer succeeded.");
        writeStream.Dispose();
        autoResetEvent.Set();
    }

    void TransferOnError(ITftpTransfer t, TftpTransferError error)
    {
        Console.WriteLine("Transfer failed: " + error);
        writeStream.Dispose();
        autoResetEvent.Set();
    }
}

string ReadFile(string file)
{
    var content = string.Empty;
    Stream readStream = new MemoryStream();
    try
    {
        var client = new TftpClient("tftp_server");

        var transfer = client.Download(file);

        transfer.OnProgress += TransferOnProgress;
        transfer.OnFinished += TransferOnFinished;
        transfer.OnError += TransferOnError;
        transfer.RetryCount = 1;
        transfer.RetryTimeout = TimeSpan.FromSeconds(1);
        transfer.TransferMode = TftpTransferMode.octet;
        
        transfer.Start(readStream);
        
        autoResetEvent.WaitOne();
       
        return content;
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }

    void TransferOnProgress(ITftpTransfer t, TftpTransferProgress progress)
    {
        Console.WriteLine("Transfer running. Progress: " + progress);
    }

    void TransferOnFinished(ITftpTransfer t)
    {
        Console.WriteLine("Transfer succeeded.");
        readStream.Position = 0;
        var reader = new StreamReader(readStream);
        var result = reader.ReadToEnd();
        content = result;
        readStream.Dispose();
        autoResetEvent.Set();
    }

    void TransferOnError(ITftpTransfer t, TftpTransferError error)
    {
        Console.WriteLine("Transfer failed: " + error);
        readStream.Dispose();
        autoResetEvent.Set();
    }
}
