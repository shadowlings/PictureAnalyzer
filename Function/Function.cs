using System;
using System.IO;
using PictureAnalyzer.Service;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PictureAnalyzer.Function
{
    public class Function
    {
        private readonly ILogger _logger;
        private AnalysisService _analysisService;
        private AzureBlobService _azureBlobService;
        private IConfiguration _configuration;

        public Function(ILoggerFactory loggerFactory, AnalysisService analysisService, AzureBlobService azureBlobService, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<Function>();

            _analysisService = analysisService;
            _azureBlobService = azureBlobService;
            _configuration = configuration;
        }

        /*

        Analysis Triggers
        - Analyze folder contents
        - Analyze single file
        - 

        */

        //[Function("Function1")]
        //public void Run([BlobTrigger("images/{name}", Connection = "AzureStorageConnectionString")] string myBlob, string name)
        //{
        //    _logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n Data: {myBlob}");
        //}

        [Function("FolderItemUpload")]
        public void Run([BlobTrigger("images/{name}", Connection = "AzureStorageConnectionString")] string myBlob, string name)
        {
            try
            {
                var sasUrl = _azureBlobService.GetViewSasForBlob(name).Result;
                _analysisService.AnalyzeImageUrl(name, sasUrl.Sas.ToString()).Wait();
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error: {name}: Ex Message: {ex.Message}: Inner Exception: {ex.InnerException?.InnerException?.Message}");
            }

            _logger.LogInformation($"C# Blob trigger function Processed blob: {name}");// \n Data: {myBlob}");
            //log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {uploadedBlob.Length} Bytes");
        }



        // [FunctionName("AnalyzeTrigger")]
        // public static void Run(
        //     [SqlTrigger("[dbo].[ToDo]", ConnectionStringSetting = "SqlConnectionString")]
        //     IReadOnlyList<SqlChange<FolderItem>> changes,
        //     ILogger logger)
        // {
        //     // foreach (SqlChange<ToDoItem> change in changes)
        //     // {
        //     //     ToDoItem toDoItem = change.Item;
        //     //     logger.LogInformation($"Change operation: {change.Operation}");
        //     //     logger.LogInformation($"Id: {toDoItem.Id}, Title: {toDoItem.title}, Url: {toDoItem.url}, Completed: {toDoItem.completed}");
        //     // }
        // }
    }
}
