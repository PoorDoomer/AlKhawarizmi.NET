using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace DotNetProjectGenerator.Core.Models
{
    public class ProjectStructure
    {
        public string ProjectRoot { get; set; } = string.Empty;
        public List<string> ControllerFiles { get; set; } = new();
        public List<string> ServiceFiles { get; set; } = new();
        public List<string> RepositoryFiles { get; set; } = new();
        public List<string> CqrsFiles { get; set; } = new();
        public List<string> EntityFiles { get; set; } = new();
        public List<string> Patterns { get; set; } = new();

        public IEnumerable<string> GetPossibleControllerLocations()
        {
            var locations = new List<string>();
            if (ControllerFiles.Any())
            {
                locations.AddRange(ControllerFiles.Select(Path.GetDirectoryName)
                    .Where(d => !string.IsNullOrEmpty(d))
                    .Distinct());
            }
            return locations;
        }

        public IEnumerable<string> GetEntityLocations()
        {
            var locations = new List<string>();
            if (EntityFiles.Any())
            {
                locations.AddRange(EntityFiles.Select(Path.GetDirectoryName)
                    .Where(d => !string.IsNullOrEmpty(d))
                    .Distinct());
            }
            return locations;
        }
    }
} 