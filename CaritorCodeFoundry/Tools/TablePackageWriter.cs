using System;
using System.IO;
using System.Collections.Generic;

namespace CodeFoundry.Generator.Tools
{
    public static class TablePackageWriter
    {
        /// <summary>
        /// Ensure standard package structure for a table and write the provided files into appropriate subfolders.
        /// baseOutputFolder: e.g. Path.Combine(generatorRoot,"OutputPackages")
        /// files: dictionary where key = relative path inside package (e.g. "DAL/StoredProcedures/Employee.SPs.sql")
        /// and value = file content.
        /// Returns full package folder path.
        /// </summary>
        public static string WriteTablePackage(string baseOutputFolder, string tableName, Dictionary<string, string> files)
        {
            if (string.IsNullOrWhiteSpace(baseOutputFolder)) throw new ArgumentNullException(nameof(baseOutputFolder));
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentNullException(nameof(tableName));
            if (files == null) throw new ArgumentNullException(nameof(files));

            var root = Path.Combine(baseOutputFolder, tableName);
            EnsureStandardPackageFolders(root);

            foreach (var kv in files)
            {
                // normalize forward/back slashes
                var rel = kv.Key.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
                var target = Path.Combine(root, rel);
                var folder = Path.GetDirectoryName(target);
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                // write only if changed (simple implementation)
                if (!File.Exists(target) || File.ReadAllText(target) != kv.Value)
                {
                    File.WriteAllText(target, kv.Value);
                }
            }

            // write README with simple copy instructions (overwrite each time)
            var readme = Path.Combine(root, "README.txt");
            File.WriteAllText(readme, GenerateReadmeContent(tableName));

            return root;
        }

        private static void EnsureStandardPackageFolders(string root)
        {
            var folders = new[]
            {
                "DAL/Entities",
                "DAL/Repositories",
                "DAL/UnitOfWork",
                "DAL/Services",
                "DAL/StoredProcedures",
                "DTOs/ViewModels",
                "DTOs/MetadataModels",
                "WEB/Controllers",
                "WEB/Views",
                "WEB/Helpers",
                "WEB/Scripts",
                "Common"
            };

            foreach (var f in folders)
            {
                var full = Path.Combine(root, f.Replace('/', Path.DirectorySeparatorChar));
                if (!Directory.Exists(full)) Directory.CreateDirectory(full);
            }
        }

        private static string GenerateReadmeContent(string tableName)
        {
            return
$@"Table package: {tableName}

Folders and suggested project destinations:

  DAL/Entities                -> Caritor.CariHRMS.DAL/Entities
  DAL/Repositories            -> Caritor.CariHRMS.DAL/Repositories
  DAL/UnitOfWork              -> Caritor.CariHRMS.DAL/UnitOfWork
  DAL/Services                -> Caritor.CariHRMS.DAL/Services
  DAL/StoredProcedures        -> Caritor.CariHRMS.DAL/StoredProcedures

  DTOs/ViewModels             -> Caritor.CariHRMS.DTOs.ViewModels
  DTOs/MetadataModels         -> Caritor.CariHRMS.DTOs.MetadataModels    (optional copy)

  WEB/Controllers             -> Caritor.CariHRMS.Web/Controllers
  WEB/Views/{tableName}       -> Caritor.CariHRMS.Web/Views/{tableName}
  WEB/Helpers                 -> Caritor.CariHRMS.Web/Helpers
  WEB/Scripts                 -> Caritor.CariHRMS.Web/Scripts

  Common                      -> Caritor.CariHRMS.Common (attributes, helpers)

Copy entire folder '{tableName}' into your solution root and then
add files to respective projects using VS: Add Existing Item -> choose files.

";
        }
    }
}
