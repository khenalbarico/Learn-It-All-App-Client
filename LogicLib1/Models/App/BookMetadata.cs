using System.ComponentModel.DataAnnotations;

namespace LogicLib1.Models.App;

public class BookMetadata
{
    [Required] public string   BookUid       { get; set; } = string.Empty;
               public string?  Category      { get; set; }
               public string?  Title         { get; set; }
               public string?  Description   { get; set; }
               public decimal  Price         { get; set; }
               public string?  ImageCoverUrl { get; set; }
}
