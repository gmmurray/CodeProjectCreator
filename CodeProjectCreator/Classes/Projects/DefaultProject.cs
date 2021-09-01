using System;
using System.IO;
using System.Threading.Tasks;

namespace CodeProjectCreator.Classes.Projects
{
    public class DefaultProject : Project
    {
        public override Task<string> Initialize()
        {
            if (!CanInitialize())
                throw new Exception("Invalid name/path for the empty project");

            Console.WriteLine($"Creating new folder named {Name}...");

            var newPath = Path.Combine(RootPath, Name);
            var result = Directory.CreateDirectory(newPath).FullName;

            Console.WriteLine("Project successfully created!");
            return Task.FromResult(result);
        }
    }
}
