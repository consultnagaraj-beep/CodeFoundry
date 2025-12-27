using System;
using System.IO;

namespace CodeFoundry.Generator.Services
{
    /// <summary>
    /// Manages Project folder structure, file creation and session logs.
    /// </summary>
    public static class ProjectManager
    {
        public static string CurrentProjectName { get; private set; }
        public static string ProjectRoot { get; private set; }
        public static string SessionLogFile { get; private set; }

        /// <summary>
        /// Create or load an existing project.
        /// Always organized under:  ~/CodeFoundryProjects/{ProjectName}
        /// </summary>
        public static void LoadProject(string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentException("Project name cannot be empty.");

            CurrentProjectName = projectName.Trim();
            string baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                          "CodeFoundryProjects");

            if (!Directory.Exists(baseDir))
                Directory.CreateDirectory(baseDir);

            ProjectRoot = Path.Combine(baseDir, CurrentProjectName);

            if (!Directory.Exists(ProjectRoot))
                Directory.CreateDirectory(ProjectRoot);

            // create subfolders
            CreateSub("Models");
            CreateSub("ViewModels");
            CreateSub("EditGrid");
            CreateSub("InfoGrid");
            CreateSub("DropdownGrid");
            CreateSub("StoredProcedures");
            CreateSub("ValidationSP");
            CreateSub("DeleteValidationSP");
            CreateSub("Metadata");

            // Session log file (timestamp based)
            string sessionId = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            SessionLogFile = Path.Combine(ProjectRoot, $"Session_{sessionId}.log");
            File.WriteAllText(SessionLogFile, $"CodeFoundry Session Started: {DateTime.Now}\n\n");
        }

        private static void CreateSub(string folder)
        {
            string path = Path.Combine(ProjectRoot, folder);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Generic file writer for generator output.
        /// Automatically logs the file creation.
        /// </summary>
        public static void WriteFile(string subFolder, string fileName, string content)
        {
            if (string.IsNullOrWhiteSpace(ProjectRoot))
                throw new InvalidOperationException("Project not loaded.");

            string path = Path.Combine(ProjectRoot, subFolder, fileName);
            File.WriteAllText(path, content);

            File.AppendAllText(SessionLogFile,
                $"[{DateTime.Now}] Created: {subFolder}/{fileName}\n");
        }
    }
}
