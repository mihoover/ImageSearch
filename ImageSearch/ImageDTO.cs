using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

namespace ImageSearch
{
   [SerializePropertyNamesAsCamelCase]
   internal partial class ImageDTO
   {
      [Key]
      public string Id { get; set; }

      [IsSearchable]
      public string TripName { get; set; }
      [IsSortable]
      public string ImageName { get; set; }
      [IsFilterable, IsSortable]
      public DateTimeOffset DateProcessed { get; set; }
   }
}
