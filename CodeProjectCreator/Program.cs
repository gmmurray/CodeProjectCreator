using CreateCodeProject.Classes;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace CodeProjectCreator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //var dirToCheck = Directory.GetCurrentDirectory() + @"/Persistence/defaultSettings.json";
            //Console.WriteLine(dirToCheck);
            //var exists = File.Exists(dirToCheck);
            //Console.WriteLine(exists);
            //var nada = Console.ReadLine();

            while (true)
            {
                var command = new NewProjectCommand();
                try
                {
                    command.ConfigureProjectType();
                    command.ConfigureOptions();
                    await command.ConfigureRootDirectory();
                    command.ConfigureProjectDirectory();

                    // start over if we had issues creating the directory
                    if (!command.CanCreateDirectory())
                        continue;

                    var newDirInfo = await command.ConfigureFullDirectory();

                    // only continue if the directory was actually created
                    if (newDirInfo.Exists)
                    {
                        if (command.CreateGitRepository)
                        {
                            await command.ConfigureGitRepository();
                        }
                        else if (!command.CreateGitRepository && command.GitRepositoryAlreadyCreated())
                        {
                            command.CleanUpGitRepository();
                        }
                        if (command.OpenProjectAfterCreation)
                        {
                            var startInfo = new ProcessStartInfo
                            {
                                Arguments = newDirInfo.FullName,
                                FileName = "explorer.exe"
                            };
                            Process.Start(startInfo);
                        }
                    }
                    else
                    {
                        Console.WriteLine("The folder could not be created. Starting over...");
                        continue;
                    }

                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    command.RevertChanges();
                    continue;
                }                
            }
        }
    }
}
