using System.Threading.Tasks;

namespace CodeProjectCreator.Classes.Projects
{
    public abstract class Project
    {
        public string Name { get; set; }
        public string RootPath { get; set; }

        public abstract Task<string> Initialize();

        public bool CanInitialize()
        {
            return !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(RootPath);
        }
    }
}
