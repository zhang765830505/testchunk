using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using System.Text.Json;

var client = new HttpClient();

string inputFile = "C:/uploads/48f789e4362e8940fdfcc984bc2b06ea.jpg"; // 替换为你的视频文件路径
string apiUrl = "http://localhost:59457/uploadfile/api/files/upload-chunk"; // 替换为你的 API 地址
string fileId = "sadlifghasdkljfhaskjdhfk"; // 根据需要生成或指定一个文件 ID
int uploadType = 0;
string contentType = "image/jpg"; // 或根据文件类型进行调整

await ProcessFileAsync(inputFile, apiUrl, fileId, uploadType, contentType);

async Task ProcessFileAsync(string inputFile, string apiUrl, string fileId, int uploadType, string contentType)
{
    FileInfo fileInfo = new FileInfo(inputFile);
    int totalChunks = (int)Math.Ceiling((double)fileInfo.Length / 1_048_575);

    int partNumber = 0;
    foreach (var chunk in ReadFileInChunks(inputFile, 1_048_575))
    {
        await UploadFileAsync(chunk, apiUrl, partNumber, totalChunks, fileId, uploadType, contentType);
        partNumber++;
    }

    Console.WriteLine("Upload completed.");
}

async Task UploadFileAsync(byte[] fileChunk, string apiUrl, int chunkIndex, int totalChunks, string fileId, int uploadType, string contentType)
{
    await Console.Out.WriteLineAsync($"第{chunkIndex+1}片，共{totalChunks}片");
    string base64Data = Convert.ToBase64String(fileChunk);

    var uploadData = new
    {
        base64Chunk = base64Data,
        chunkIndex,
        totalChunks,
        fileId,
        uploadType,
        contentType
    };

    var content = new StringContent(JsonSerializer.Serialize(uploadData), Encoding.UTF8, "application/json");
    var response = await client.PostAsync(apiUrl, content);

    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine($"Error uploading chunk {chunkIndex}: {response.StatusCode}");
    }
    else
    {
        await Console.Out.WriteLineAsync(await response.Content.ReadAsStringAsync());
    }
}

IEnumerable<byte[]> ReadFileInChunks(string filePath, int chunkSize)
{
    using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
    using var binaryReader = new BinaryReader(fileStream);
    while (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
    {
        yield return binaryReader.ReadBytes(chunkSize);
    }
}
