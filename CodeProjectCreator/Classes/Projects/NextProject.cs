using CodeProjectCreator.Enums;
using System;
using System.Threading.Tasks;

namespace CodeProjectCreator.Classes.Projects
{
    public class NextProject : CliProject
    {
        public readonly ProjectType ProjectType = ProjectType.Next;
        public const string CliCommand = "create-next-app";
        public override async Task<string> Initialize()
        {
            if (!CanInitialize())
                throw new Exception("Invalid name/path for the next project");

            return await HandleInitialization(CliCommand, ProjectType);
        }
    }
}
