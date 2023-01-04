using System.Text;
using Tftp.Net;

AutoResetEvent autoResetEvent = new(false);

const string fileName = "example.txt";

WriteFile(fileName, "testing 2");
var fileContent = ReadFile(fileName);
Console.WriteLine(fileContent);

void WriteFile(string file, string content)
{
    try
    {
        var byteArray = Encoding.ASCII.GetBytes(content);
        using var stream = new MemoryStream(byteArray);

        var client = new TftpClient("tftp_server");

        var transfer = client.Upload(file);
        transfer.RetryCount = 3;
        transfer.RetryTimeout = TimeSpan.FromSeconds(1);

        transfer.OnProgress += (transfer, progress) =>
        {
            Console.WriteLine("Transfer running. Progress: " + progress);
        };

        transfer.OnFinished += (transfer) =>
        {
            Console.WriteLine("Transfer succeeded.");;
            autoResetEvent.Set();
        };

        transfer.OnError += (transfer, error) =>
        {
            Console.WriteLine("Transfer failed: " + error);
            autoResetEvent.Set();
        };
       
        transfer.Start(stream);

        autoResetEvent.WaitOne();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
}

string ReadFile(string file)
{
    try
    {
        var content = string.Empty;
        using Stream stream = new MemoryStream();
        var client = new TftpClient("tftp_server");

        var transfer = client.Download(file);
        transfer.RetryCount = 3;
        transfer.RetryTimeout = TimeSpan.FromSeconds(1);
        transfer.TransferMode = TftpTransferMode.octet;

        transfer.OnProgress += (transfer, progress) =>
        {
            Console.WriteLine("Transfer running. Progress: " + progress);
        };

        transfer.OnFinished += (transfer) =>
        {
            Console.WriteLine("Transfer succeeded.");
            stream.Position = 0;
            var reader = new StreamReader(stream);
            content = reader.ReadToEnd();
            autoResetEvent.Set();
        };

        transfer.OnError += (transfer, error) =>
        {
            Console.WriteLine("Transfer failed: " + error);
            autoResetEvent.Set();
        };
        
        transfer.Start(stream);
        
        autoResetEvent.WaitOne();
       
        return content;
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
}
