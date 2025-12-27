using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CodeFoundry.Generator.Services;

namespace CodeFoundry.Generator.IO
{
    public enum FileOperation
    {
        Added,
        Modified,
        Skipped
    }

    public class FileWriteResult
    {
        public FileOperation Operation { get; set; }
        public string RelativePath { get; set; }
        public string FullPath { get; set; }
        public string PrevChecksum { get; set; }
        public string NewChecksum { get; set; }
        public DateTime Timestamp { get; set; }
        public bool CustomBlocksReinjected { get; set; }
    }

    /// <summary>
    /// FileWriter implements write-if-changed, archive of previous versions, and immediate session logging.
    /// Usage: FileWriter.WriteIfChanged("ViewModels", "EmployeesVm.cs", content);
    /// </summary>
    public static class FileWriter
    {
        /// <summary>
        /// Write the file under ProjectRoot\subFolder\fileName only if content changed.
        /// Archives previous version on modification.
        /// Returns FileWriteResult with details.
        /// </summary>
        public static FileWriteResult WriteIfChanged(string subFolder, string fileName, string content)
        {
            if (string.IsNullOrEmpty(ProjectManager.ProjectRoot))
                throw new InvalidOperationException("Project not loaded. Call ProjectManager.LoadProject(...) first.");

            // Ensure subfolder exists
            string folderPath = Path.Combine(ProjectManager.ProjectRoot, subFolder);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fullPath = Path.Combine(folderPath, fileName);
            var result = new FileWriteResult
            {
                RelativePath = Path.Combine(subFolder, fileName).Replace('\\', '/'),
                FullPath = fullPath,
                Timestamp = DateTime.Now,
                PrevChecksum = null,
                NewChecksum = ComputeChecksum(content),
                CustomBlocksReinjected = false
            };

            // If file does not exist -> add
            if (!File.Exists(fullPath))
            {
                File.WriteAllText(fullPath, content, Encoding.UTF8);
                result.Operation = FileOperation.Added;
                // log
                AppendSessionLog(result, $"Added");
                return result;
            }

            // File exists -> compare checksums
            string existingContent = File.ReadAllText(fullPath, Encoding.UTF8);
            var existingChecksum = ComputeChecksum(existingContent);
            result.PrevChecksum = existingChecksum;

            if (string.Equals(existingChecksum, result.NewChecksum, StringComparison.OrdinalIgnoreCase))
            {
                // identical -> skip
                result.Operation = FileOperation.Skipped;
                AppendSessionLog(result, $"Skipped-Identical");
                return result;
            }

            // different -> archive the old file and write new content
            try
            {
                // ensure archive folder exists
                string archiveFolder = Path.Combine(ProjectManager.ProjectRoot, "Archive");
                if (!Directory.Exists(archiveFolder))
                    Directory.CreateDirectory(archiveFolder);

                // create table-specific archive subfolder for cleanliness
                string tableArchiveSub = Path.Combine(archiveFolder, SanitizeForPath(subFolder));
                if (!Directory.Exists(tableArchiveSub))
                    Directory.CreateDirectory(tableArchiveSub);

                string ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string archiveFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{ts}{Path.GetExtension(fileName)}.bak";
                string archiveFullPath = Path.Combine(tableArchiveSub, archiveFileName);

                // copy old file to archive
                File.Copy(fullPath, archiveFullPath, overwrite: true);

                // write new content
                File.WriteAllText(fullPath, content, Encoding.UTF8);

                result.Operation = FileOperation.Modified;
                AppendSessionLog(result, $"Modified (archived: {Path.Combine("Archive", SanitizeForPath(subFolder), archiveFileName)})");
                return result;
            }
            catch (Exception ex)
            {
                // if archive or write failed, rethrow as it's important to notice
                throw new IOException($"Failed to archive and write file '{fullPath}': {ex.Message}", ex);
            }
        }

        private static string ComputeChecksum(string content)
        {
            if (content == null) content = "";
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(content);
                var hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
            }
        }

        private static void AppendSessionLog(FileWriteResult result, string shortMessage)
        {
            try
            {
                // Build a compact log line
                var line = $"[{result.Timestamp:yyyy-MM-dd HH:mm:ss}] {shortMessage}: {result.RelativePath}";
                if (!string.IsNullOrEmpty(result.PrevChecksum))
                    line += $" (prev:{result.PrevChecksum.Substring(0, 8)} -> new:{(result.NewChecksum ?? "").Substring(0, 8)})";
                line += Environment.NewLine;

                // Append to session log file if present, else fallback to a default log under ProjectRoot
                string sessionLog = ProjectManager.SessionLogFile;
                if (string.IsNullOrEmpty(sessionLog))
                {
                    var defaultLog = Path.Combine(ProjectManager.ProjectRoot, $"Session_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                    File.AppendAllText(defaultLog, line);
                }
                else
                {
                    File.AppendAllText(sessionLog, line);
                }
            }
            catch
            {
                // swallow logging exceptions to avoid breaking generator pipeline;
                // but in real app surface this in UI/log console.
            }
        }

        private static string SanitizeForPath(string s)
        {
            // Replace directory separators and invalid chars to create a safe folder name
            if (string.IsNullOrEmpty(s)) return "misc";
            foreach (var c in Path.GetInvalidFileNameChars())
                s = s.Replace(c, '_');
            s = s.Replace(Path.DirectorySeparatorChar, '_').Replace(Path.AltDirectorySeparatorChar, '_');
            s = s.Trim('_');
            if (string.IsNullOrEmpty(s)) return "misc";
            return s;
        }
    }
}
