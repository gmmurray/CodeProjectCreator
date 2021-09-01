using CodeProjectCreator.Enums;
using System;
using System.Threading.Tasks;

namespace CodeProjectCreator.Classes.Projects
{
    public class ReactProject : CliProject
    {
        public readonly ProjectType ProjectType = ProjectType.React;
        public const string CliCommand = "create-react-app";
        public override async Task<string> Initialize()
        {
            if (!CanInitialize())
                throw new Exception($"Invalid name/path for the {ProjectType} project.");

            return await HandleInitialization(CliCommand, ProjectType);
        }        
    }
}
