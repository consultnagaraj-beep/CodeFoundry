using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeFoundry.Generator.Models;
using CodeFoundry.Generator.Tools;

namespace CariHRMS.DAL.Generators
{
    public static class DbViewGenerator
    {
        // =========================================================
        // PUBLIC ENTRY
        // =========================================================
        public static Dictionary<string, string> GenerateViews(
            TableSchemaDto schema,
            SelectionDto selection)
        {
            if (schema == null) throw new ArgumentNullException(nameof(schema));
            if (selection == null) throw new ArgumentNullException(nameof(selection));

            var files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var gridType in selection.GridTypes)
            {
                var sql = GenerateGridView(schema, selection, gridType);
                var viewName = $"vw_{schema.TableName}_{gridType}";
                var path = $"DAL/Views/{viewName}.sql";

                files[path] = sql;
            }

            return files;
        }

        // =========================================================
        // CORE VIEW GENERATOR
        // =========================================================
        private static string GenerateGridView(
            TableSchemaDto schema,
            SelectionDto selection,
            string gridType)
        {
            var table = schema.TableName;
            var selectedCols = selection.GetSelectedColumns(gridType);
            var fkMap = selection.GetFkSelections(gridType);

            var sb = new StringBuilder();

            sb.AppendLine($"CREATE OR REPLACE VIEW vw_{table}_{gridType} AS");
            sb.AppendLine("SELECT");

            // -----------------------------------------------------
            // BASE ID (MANDATORY)
            // -----------------------------------------------------
            sb.AppendLine("    e.Id,");

            // -----------------------------------------------------
            // NORMAL COLUMNS (approval-aware)
            // -----------------------------------------------------
            foreach (var col in selectedCols.Where(c => !c.Equals("Id", StringComparison.OrdinalIgnoreCase)))
            {
                sb.AppendLine(
                    $"    IFNULL(edited.{col}, e.{col}) AS {col},");
            }

            // -----------------------------------------------------
            // FK RETURN COLUMNS (MULTI-FK SAFE)
            // -----------------------------------------------------
            foreach (var fk in fkMap)
            {
                var fkColumn = fk.Key;
                var fkInfo = schema.ForeignKeys
                    .FirstOrDefault(f => f.ColumnName.Equals(fkColumn, StringComparison.OrdinalIgnoreCase));

                if (fkInfo == null)
                    continue;

                var alias = $"{fkColumn}_{fkInfo.ReferencedTable}";

                foreach (var retCol in fk.Value)
                {
                    sb.AppendLine(
                        $"    {alias}.{retCol} AS {fkColumn}_{retCol},");
                }
            }

            // remove last comma
            TrimLastComma(sb);

            // -----------------------------------------------------
            // FROM + APPROVAL SHADOW LOGIC
            // -----------------------------------------------------
            sb.AppendLine($"FROM {table} e");

            sb.AppendLine(
                $"LEFT JOIN {table} edited ON e.Id = edited.Id " +
                $"AND edited.Status IN ('E','D') " +
                $"AND edited.ApprStatus = 'P'");

            // -----------------------------------------------------
            // FK JOINS (approval-aware)
            // -----------------------------------------------------
            foreach (var fk in fkMap)
            {
                var fkColumn = fk.Key;
                var fkInfo = schema.ForeignKeys
                    .FirstOrDefault(f => f.ColumnName.Equals(fkColumn, StringComparison.OrdinalIgnoreCase));

                if (fkInfo == null)
                    continue;

                var alias = $"{fkColumn}_{fkInfo.ReferencedTable}";

                sb.AppendLine(
                    $"LEFT JOIN {fkInfo.ReferencedTable} {alias} " +
                    $"ON IFNULL(edited.{fkColumn}, e.{fkColumn}) = {alias}.Id");
            }

            // -----------------------------------------------------
            // BASE FILTERS (MANDATORY)
            // -----------------------------------------------------
            sb.AppendLine("WHERE e.Status = 'A'");
            sb.AppendLine("  AND e.ApprStatus = 'A';");

            return sb.ToString();
        }

        // =========================================================
        // HELPERS
        // =========================================================
        private static void TrimLastComma(StringBuilder sb)
        {
            for (int i = sb.Length - 1; i >= 0; i--)
            {
                if (sb[i] == ',')
                {
                    sb.Remove(i, 1);
                    break;
                }
                if (!char.IsWhiteSpace(sb[i]))
                    break;
            }
        }
    }
}
