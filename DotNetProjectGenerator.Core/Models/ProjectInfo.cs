using System.Collections.Generic;
using System.Linq;

namespace DotNetProjectGenerator.Core.Models
{
    public class ProjectInfo
    {
        public string ProjectName { get; set; }
        public string ProjectPattern { get; set; }
        public string ControllersPath { get; set; }
        public string EntitiesPath { get; set; }
        public List<EntityInfo> Entities { get; set; } = new List<EntityInfo>();

        public IEnumerable<string> GetPossibleControllerLocations()
        {
            var locations = new List<string>();
            if (!string.IsNullOrEmpty(ControllersPath))
                locations.Add(ControllersPath);
            return locations;
        }

        public IEnumerable<string> GetEntityLocations()
        {
            var locations = new List<string>();
            if (!string.IsNullOrEmpty(EntitiesPath))
                locations.Add(EntitiesPath);
            return locations;
        }

        public IEnumerable<string> GetEntitiesInLocation(string location)
        {
            return Entities.Select(e => e.Name);
        }
    }

    public class EntityInfo
    {
        public string Name { get; set; }
        public List<PropertyInfo> Properties { get; set; } = new List<PropertyInfo>();
    }

    public class PropertyInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
} 