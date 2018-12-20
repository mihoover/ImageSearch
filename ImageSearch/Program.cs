using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

namespace ImageSearch
{
   class Program
   {
      static void Main(string[] args)
      {
         SearchServiceClient serviceClient = CreateSearchServiceClient();
         serviceClient.Indexes.Delete("trips");
         //create index
         Index definition = new Index()
         {
            Name = "trips",
            Fields = FieldBuilder.BuildForType<ImageDTO>(),
         };

         serviceClient.Indexes.Create(definition);


         //get data
         string folderPath = @"D:\Pictures";
         string[] directories = Directory.GetDirectories(folderPath, "Finals", SearchOption.AllDirectories);
         int totalImages = 0;
         int id = 1;
         List<ImageDTO> images = new List<ImageDTO>();
         foreach (string directory in directories)
         {
            string dirName = Path.GetDirectoryName(directory);
            DirectoryInfo directoryInfo = new DirectoryInfo(directory);
            dirName = directoryInfo.Parent.Name;
            string[] files = Directory.GetFiles(directory, "*.jpg");
            Console.WriteLine($"{dirName} - {files.Count()}");
            foreach (string file in files)
            {
               images.Add(new ImageDTO()
               {
                  Id = id++.ToString(),
                  TripName = dirName,
                  DateProcessed = File.GetCreationTime(file),
                  ImageName = Path.GetFileName(file),
               });
            }
            totalImages += files.Count();
         }
         Console.WriteLine($"Total Images: {totalImages}");

         //Prep for push
         List<IndexAction<ImageDTO>> actions = new List<IndexAction<ImageDTO>>(totalImages);
         foreach (ImageDTO image in images.Take(1000))
         {
            actions.Add(IndexAction.Upload(image));
         }
         IndexBatch<ImageDTO> batch = IndexBatch.New(actions);

         //Push to index
         ISearchIndexClient indexClient = serviceClient.Indexes.GetClient("trips");
         try
         {
            indexClient.Documents.Index(batch);
         }
         catch (IndexBatchException e)
         {
            // Sometimes when your Search service is under load, indexing will fail for some of the documents in
            // the batch. Depending on your application, you can take compensating actions like delaying and
            // retrying. For this simple demo, we just log the failed document keys and continue.
            Console.WriteLine(
                "Failed to index some of the documents: {0}",
                string.Join(", ", e.IndexingResults.Where(r => !r.Succeeded).Select(r => r.Key)));
         }

         Console.WriteLine("Waiting for documents to be indexed...");
         Console.ReadLine();
      }

      private static SearchServiceClient CreateSearchServiceClient()
      {
         string searchServiceName = ConfigurationManager.AppSettings["SearchServiceName"];
         string adminApiKey = ConfigurationManager.AppSettings["SearchServiceAdminApiKey"];

         SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));
         return serviceClient;
      }
   }
}
