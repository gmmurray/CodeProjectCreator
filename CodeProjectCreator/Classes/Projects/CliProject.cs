using CodeProjectCreator.Enums;
using System;
using System.IO;
using System.Management.Automation;
using System.Threading.Tasks;

namespace CodeProjectCreator.Classes.Projects
{
    public abstract class CliProject : Project
    {
        public async Task<string> HandleInitialization(string cliCommand, ProjectType projectType)
        {
            Console.WriteLine($"Creating new {projectType} project using {cliCommand}...");

            using var ps = PowerShell.Create();

            try
            {
                var showScriptDetails = false;

                ps.AddScript($@"cd {RootPath}");
                ps.AddScript($@"npx {cliCommand} {Name}");

                var resultTask = ps.InvokeAsync();

                while (resultTask.Status != TaskStatus.RanToCompletion)
                {
                    await PrintLoadingText();
                }

                if (showScriptDetails)
                {
                    foreach (var outputItem in await resultTask)
                    {
                        if (outputItem != null)
                        {
                            Console.WriteLine(outputItem);
                        }
                    }
                }
                else
                {
                    await resultTask;
                }

                var createdPath = Path.Combine(RootPath, Name);

                if (new DirectoryInfo(createdPath).Exists)
                {
                    Console.WriteLine($"\n{projectType} project successfully created");
                    return createdPath;
                }
                else
                    throw new Exception();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception("Error using create-react-app");
            }
        }

        private static async Task PrintLoadingText()
        {
            const string loadingText = "Loading";
            const char loadingChar = '.';

            for (int i = 0; i < 4; i++)
            {
                Console.Write("\r{0}   ", loadingText + new string(loadingChar, i));
                await Task.Delay(300);
            }
        }
    }
}
