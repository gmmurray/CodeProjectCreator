using CodeProjectCreator.Classes.Projects;
using CodeProjectCreator.Enums;

namespace CodeProjectCreator.Classes
{
    public interface IProjectFactory
    {
        Project GetProject(ProjectType projectType, string name, string rootPath);
    }

    public class ProjectFactory : IProjectFactory
    {
        public Project GetProject(ProjectType projectType, string name, string rootPath)
        {
            return projectType switch
            {
                ProjectType.React => new ReactProject { Name = name, RootPath = rootPath },
                ProjectType.Next => new NextProject { Name = name, RootPath = rootPath },
                _ => new DefaultProject { Name = name, RootPath = rootPath }
            };
        }
    }
}
