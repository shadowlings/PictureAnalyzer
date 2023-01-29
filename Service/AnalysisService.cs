using PictureAnalyzer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace PictureAnalyzer.Service
{
    public class AnalysisService
    {
        private DBContext _db;
        private AzureBlobService _azureBlobService;
        private IConfiguration _configuration;

        public AnalysisService(DBContext dbContext, AzureBlobService azureBlobService, IConfiguration configuration)
        {
            _db = dbContext;
            _azureBlobService = azureBlobService;
            _configuration = configuration;
        }

        async public Task AnalyzeImageUrl(string blobId, string imageUrl)
        {
            var folderItem = await _db.FolderItems
                                    .Include(x => x.ImageRatings)
                                    .FirstOrDefaultAsync(x => x.BlobId == blobId);

            if (folderItem != null)
            {
                string endpoint = _configuration["VisionEndPoint"];
                string subscriptionKey = _configuration["VisionSubscriptionKey"];

                ComputerVisionClient client =
                  new ComputerVisionClient(new ApiKeyServiceClientCredentials(subscriptionKey))
                  { Endpoint = endpoint };


                Console.WriteLine("----------------------------------------------------------");
                Console.WriteLine("ANALYZE IMAGE - URL");
                Console.WriteLine();

                // Creating a list that defines the features to be extracted from the image. 

                List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
                {
                    //VisualFeatureTypes.ImageType,
                    //VisualFeatureTypes.Faces,
                    VisualFeatureTypes.Adult,
                    //VisualFeatureTypes.Categories,
                    //VisualFeatureTypes.Color,
                    VisualFeatureTypes.Tags,
                    //VisualFeatureTypes.Description,
                    //VisualFeatureTypes.Objects,
                    //VisualFeatureTypes.Brands
                };

                Console.WriteLine($"Analyzing the image {Path.GetFileName(imageUrl)}...");
                Console.WriteLine();
                // Analyze the URL image 
                ImageAnalysis results = await client.AnalyzeImageAsync(imageUrl, visualFeatures: features);

            

                // Image tags and their confidence score
                Console.WriteLine("Tags:");
                foreach (var item in results.Tags)
                {
                    Console.WriteLine($"{item.Name} {item.Confidence}");
                    if (folderItem != null)
                    {
                        PictureAnalyzer.Data.ImageTag tag = new PictureAnalyzer.Data.ImageTag()
                        {
                            FolderItemId = folderItem.Id,
                            Tag = item.Name,
                            Confidence = item.Confidence
                        };
                        _db.ImageTags.Add(tag);
                    }
                }

                //Console.WriteLine("ImageType:");
                //foreach (var item in results.Categories)
                //{
                //    Console.WriteLine($"{item.Name} {item.Score}");
                //}
                //Console.WriteLine();

                if (folderItem != null)
                {
                    PictureAnalyzer.Data.ImageRatings ratings = new PictureAnalyzer.Data.ImageRatings()
                    {
                        FolderItemId = folderItem.Id,
                        IsAdultContent = results.Adult.IsAdultContent,
                        IsRacyContent = results.Adult.IsRacyContent,
                        IsGoryContent = results.Adult.IsGoryContent,
                        AdultScore = results.Adult.AdultScore,
                        GoreScore = results.Adult.GoreScore,
                        RacyScore = results.Adult.RacyScore
                    };
                    _db.ImageRatings.Add(ratings);
                }
                await _db.SaveChangesAsync();


                Console.WriteLine($"Adult: {results.Adult.IsAdultContent} {results.Adult.AdultScore}");
                Console.WriteLine($"Racy: {results.Adult.IsRacyContent} {results.Adult.RacyScore}");
                Console.WriteLine($"Gore: {results.Adult.IsGoryContent} {results.Adult.GoreScore}");
                Console.WriteLine();


                //Console.WriteLine("Description - Captions:");
                //foreach (var item in results.Description.Captions)
                //{
                //    Console.WriteLine($"{item.Text} {item.Confidence}");
                //}
                //Console.WriteLine();

                //Console.WriteLine("Description - Tags:");
                //foreach (var item in results.Description.Tags)
                //{
                //    Console.WriteLine($"{item}");
                //}
                //Console.WriteLine();
            }
        }



    }
}
