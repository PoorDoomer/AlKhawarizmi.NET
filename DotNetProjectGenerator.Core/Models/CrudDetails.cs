using System.Collections.Generic;

namespace DotNetProjectGenerator.Core.Models
{
    public class CrudDetails
    {
        public string EntityName { get; set; } = string.Empty;
        public string Pattern { get; set; } = string.Empty;
        public List<EntityProperty> Properties { get; set; } = new();
        public CrudOptions Options { get; set; } = new();
    }

    public class EntityProperty
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public string? DefaultValue { get; set; }
        public bool IsKey { get; set; }
        public string? ForeignKey { get; set; }
        public string? NavigationProperty { get; set; }
        public string? Description { get; set; }
        public List<string> Attributes { get; set; } = new();
        public Dictionary<string, string> ValueObjectProperties { get; set; } = new();
        public bool IsValueObject { get; set; }
    }

    public class CrudOptions
    {
        public bool IncludeSwaggerDocs { get; set; }
        public bool ImplementSoftDelete { get; set; }
        public bool IncludeAuditing { get; set; }
        public bool AddPagination { get; set; }
        public bool ImplementValidation { get; set; }
        public bool GenerateTests { get; set; }
        public bool UseAsyncMethods { get; set; } = true;
        public bool AddLogging { get; set; } = true;
        public bool ImplementCaching { get; set; }
        public string? CustomNamespace { get; set; }
    }
} 