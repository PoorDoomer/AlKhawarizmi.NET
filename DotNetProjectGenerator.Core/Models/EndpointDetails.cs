namespace DotNetProjectGenerator.Core.Models;

public class EndpointDetails
{
    public string HttpMethod { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string ResponseType { get; set; } = string.Empty;
    public string? DtoName { get; set; }
    public bool RequiresValidation { get; set; }
    public bool RequiresAuthorization { get; set; }
    public bool RequiresCaching { get; set; }
} 