using System.Collections.Generic;
using CariHRMS.DAL.Generators;
using CariHRMS.DTOs.Generators;
using CariHRMS.Web.Generators;
using CodeFoundry.Generator.Models;
using CodeFoundry.Generator.Tools;

namespace CodeFoundry.Generator.Generators
{
    public static class GeneratorOrchestrator
    {
        // =========================================================
        // CORE PIPELINE
        // =========================================================
        private static Dictionary<string, string> GenerateAllInternal(
            string connection,
            string table,
            SelectionDto selection)
        {
            var schema = SchemaReader.GetTableSchema(connection, table);
            var files = new Dictionary<string, string>();

            // 1. DB Views
            Merge(files, DbViewGenerator.GenerateViews(schema, selection));

            // 2. DTO (single)
            Merge(files, DtoGenerator.GenerateDto(schema));

            // 3. ViewModels (PER GRID)
            foreach (var grid in selection.GridTypes)
            {
                var selectedCols = selection.GetSelectedColumns(grid);
                var fkMap = selection.GetFkSelections(grid);

                IDictionary<string, IEnumerable<string>> fkForVm = null;
                if (fkMap != null && fkMap.Count > 0)
                {
                    fkForVm = new Dictionary<string, IEnumerable<string>>();
                    foreach (var kv in fkMap)
                        fkForVm[kv.Key] = kv.Value;
                }

                Merge(
                    files,
                    ViewModelGenerator.GenerateViewModelFiles(
                        schema,
                        grid,
                        selectedCols,
                        fkForVm
                    )
                );
            }

            // 4. MetadataModels (C# ONLY – NO JSON)
            Merge(files, GridMetadataGenerator.Generate(schema.TableName, selection));

            // 5. Grid Models (WEB)
            Merge(files, GridModelGenerator.Generate(schema.TableName, selection));

            // 6. Grid Service (DAL)
            Merge(files, GridServiceGenerator.Generate());

            // 7. Stored Procedures
            Merge(files, StoredProcGenerator.GenerateSpFiles(schema));

            // 8. Entity Generator
            Merge(files, EntityGenerator.GenerateEntities(schema));

            // 9. Repository Generator
            Merge(files, RepositoryGenerator.GenerateRepositories(schema));

            // 10. UnitOfWork Generator
            Merge(files, UnitOfWorkGenerator.GenerateUnitOfWork(schema));

            // =====================================================
            // 11. MVC Controller
            // =====================================================
            Merge(files, ControllerGenerator.Generate(schema));

            // =====================================================
            // 12. MVC Views (Index + Details)
            // =====================================================
            Merge(files, ViewGenerator.Generate(schema, selection));

            return files;
        }

        // =========================================================
        // PUBLIC ENTRY (UI BUTTON CALLS THIS)
        // =========================================================
        public static string GenerateFullPackage(
            string connectionString,
            string tableName,
            string baseOutputFolder,
            SelectionDto selection,
            bool includeEntity = false)
        {
            var files = GenerateAllInternal(
                connectionString,
                tableName,
                selection
            );

            return TablePackageWriter.WriteTablePackage(
                baseOutputFolder,
                tableName,
                files
            );
        }

        // =========================================================
        // HELPERS
        // =========================================================
        private static void Merge(
            Dictionary<string, string> dest,
            Dictionary<string, string> src)
        {
            if (src == null) return;
            foreach (var kv in src)
                dest[kv.Key] = kv.Value;
        }
    }
}
