using System.ComponentModel.DataAnnotations;

namespace LogicLib1.Models.Api;

public class ApiRequest
{
    [Required] public string       AuthToken { get; set; } = string.Empty;
    [Required] public string       AuthUid   { get; set; } = string.Empty;
               public object?      Payload   { get; set; }
               public ApiFunctions Function  { get; set; }
}
