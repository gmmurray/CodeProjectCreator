using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Management.Automation;
using System.Threading.Tasks;
using CodeProjectCreator.Enums;
using System.Linq;
using CodeProjectCreator.Classes;

namespace CreateCodeProject.Classes
{
    public class NewProjectCommand
    {
        private readonly string _settingsPath = Directory.GetCurrentDirectory() +  @"/Persistence/defaultSettings.json";

        private static readonly ProjectType[] _excludedGitProjectTypes = new ProjectType[] { ProjectType.React };

        public string RootDirectory { get; set; } = string.Empty;

        public string ProjectDirectory { get; set; } = string.Empty;

        public string FullDirectoryPath { get; set; }

        public bool OpenProjectAfterCreation { get; set; }

        public bool CreateGitRepository { get; set; }

        public ProjectType ProjectType { get; set; } = ProjectType.Empty;

        #region Configuration

        public void ConfigureProjectType()
        {
            Console.WriteLine("What type of project do you want to create? Press the corresponding key");
            while (true)
            {
                foreach (var option in Enum.GetValues(typeof(ProjectType)))
                {
                    Console.WriteLine("{0,-5} {1,1} {2,1}", option, "-", (int)option);
                }
                if (int.TryParse(Console.ReadLine(), out var selection) && Enum.IsDefined(typeof(ProjectType), selection))
                {
                    ProjectType = (ProjectType)selection;
                    break;
                }
                else
                {
                    Console.WriteLine("Select a valid project type");
                }
            }
        }

        public void ConfigureOptions()
        {
            Console.WriteLine("Initialize git repository? y/n");
            CreateGitRepository = GetYesConsoleResponse();
            Console.WriteLine("Open project after creation? y/n");
            OpenProjectAfterCreation = GetYesConsoleResponse();
        }

        public async Task ConfigureRootDirectory()
        {
            var persistedSettings = await JsonHelpers.ReadAndDeserializeJson<DefaultSettings>(_settingsPath);
            while (true)
            {
                var useDefaultDir = false;
                if (!string.IsNullOrEmpty(persistedSettings.RootDirectory))
                {
                    Console.WriteLine($"Use the default root directory? ({persistedSettings.RootDirectory}) y/n");

                    useDefaultDir = GetYesConsoleResponse();
                }

                if (useDefaultDir)
                {
                    RootDirectory = persistedSettings.RootDirectory;
                }
                else
                {
                    Console.WriteLine("Select your desired root directory.");
                    var dirDialog = new CommonOpenFileDialog
                    {
                        InitialDirectory = @"C:\Users",
                        IsFolderPicker = true
                    };
                    if (dirDialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        Console.WriteLine($"Using {dirDialog.FileName} as root directory.");
                        RootDirectory = dirDialog.FileName;
                        Console.WriteLine("Would you like to save this as your default root directory? y/n");
                        var saveAsDefault = GetYesConsoleResponse();
                        if (saveAsDefault)
                        {
                            var updatedSettings = new DefaultSettings { RootDirectory = RootDirectory };
                            await JsonHelpers.WriteAndSerializeJson(_settingsPath, updatedSettings);
                            Console.WriteLine($"Successfully saved {RootDirectory} as your default root directory.");
                        }
                    }
                }

                if (string.Equals(RootDirectory, string.Empty))
                    Console.WriteLine("Root directory is required to continue.");
                else
                    break;

            }
        }

        public void ConfigureProjectDirectory()
        {
            while (true)
            {
                Console.WriteLine("What is the name of the project?");
                ProjectDirectory = Console.ReadLine();

                if (string.Equals(ProjectDirectory, string.Empty))
                {
                    Console.WriteLine("Project name is required to continue.");
                }
                else if (new DirectoryInfo(Path.Combine(RootDirectory, ProjectDirectory)).Exists)
                {
                    Console.WriteLine("That project name is already taken.");
                }
                else
                {
                    break;
                }                    
            }
        }

        public async Task<DirectoryInfo> ConfigureFullDirectory()
        {
            var projectFactory = new ProjectFactory();
            var project = projectFactory.GetProject(ProjectType, ProjectDirectory, RootDirectory);

            FullDirectoryPath = await project.Initialize();

            return new DirectoryInfo(FullDirectoryPath);
        }

        public async Task ConfigureGitRepository()
        {
            if (GitRepositoryAlreadyCreated())
                return;

            using var ps = PowerShell.Create();

            Console.WriteLine("Creating git repository...");
            try
            {
                ps.AddScript($@"cd {FullDirectoryPath}");
                ps.AddScript(@"git init");
                ps.AddScript(@"git commit -m 'initial commit'");

                await ps.InvokeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Unable to initialize git repository, git may not be installed");
            }
        }

        #endregion

        #region helpers

        // Returns true if none of the directory components are empty strings
        public bool CanCreateDirectory()
        {
            return !string.Equals(RootDirectory, string.Empty) && !string.Equals(ProjectDirectory, string.Empty);
        }

        // Returns true if the user has opted to create a git repo AND this project doesn't create one itself
        public bool GitRepositoryAlreadyCreated()
        {
            return _excludedGitProjectTypes.Any(t => t == ProjectType);
        }

        // Deletes git repo if the project startup process created one
        public void CleanUpGitRepository()
        {
            var gitDir = new DirectoryInfo(Path.Combine(FullDirectoryPath, ".git"));
            if (gitDir.Exists)
            {
                SetFileAttributesToNormal(gitDir);
                gitDir.Delete(true);
            }

            var gitIgnoreFile = new FileInfo(Path.Combine(FullDirectoryPath, ".gitignore"));
            if (gitIgnoreFile.Exists)
            {
                gitIgnoreFile.Attributes = FileAttributes.Normal;
                gitIgnoreFile.Delete();
            }
        }

        // Deletes the entire directory and its contents
        public void RevertChanges()
        {
            Console.WriteLine("Reverting project creation...");
            var dir = new DirectoryInfo(FullDirectoryPath);
            if (dir.Exists)
            {
                SetFileAttributesToNormal(dir);
                dir.Delete(true);
            }
        }

        // Returns true if the console response is 'y'
        private static bool GetYesConsoleResponse()
        {
            return string.Equals(Console.ReadLine().ToLower(), "y");
        }

        private static void SetFileAttributesToNormal(DirectoryInfo dir)
        {
            foreach (var subDir in dir.GetDirectories())
                SetFileAttributesToNormal(subDir);

            foreach (var file in dir.GetFiles())
                file.Attributes = FileAttributes.Normal;
        }

        #endregion
    }
}
