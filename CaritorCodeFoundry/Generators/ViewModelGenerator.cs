using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CodeFoundry.Generator.Models;
using CodeFoundry.Generator.Tools;

namespace CodeFoundry.Generator.Generators
{
    /// <summary>
    /// Generates ViewModel classes per table per grid type
    /// </summary>
    public static class ViewModelGenerator
    {
        // =====================================================
        // PUBLIC API – THIS IS WHAT MainForm CALLS
        // =====================================================
        public static Dictionary<string, string> GenerateViewModelFiles(
            TableSchemaDto schema,
            string gridType,
            IEnumerable<string> selectedColumns,
            IDictionary<string, IEnumerable<string>> fkSelections = null)
        {
            var files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var source = GenerateViewModelForGrid(
                schema,
                gridType,
                selectedColumns,
                fkSelections,
                "CariHRMS.DTOs.ViewModels"
            );

            var fileName = schema.TableName + gridType + "ViewModel.cs";
            var relPath = Path.Combine("DTOs", "ViewModels", fileName)
                                .Replace('\\', '/');

            files[relPath] = source;
            return files;
        }

        // =====================================================
        // CORE GENERATOR
        // =====================================================
        public static string GenerateViewModelForGrid(
            TableSchemaDto schema,
            string gridType,
            IEnumerable<string> selectedColumns,
            IDictionary<string, IEnumerable<string>> fkSelections,
            string namespaceName)
        {
            if (schema == null)
                throw new ArgumentNullException(nameof(schema));

            var selectedSet = new HashSet<string>(
                selectedColumns ?? new List<string>(),
                StringComparer.OrdinalIgnoreCase
            );

            var sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using System.ComponentModel.DataAnnotations;");
            sb.AppendLine();
            sb.AppendLine("namespace " + namespaceName);
            sb.AppendLine("{");
            sb.AppendLine("    public class " + schema.TableName + gridType + "ViewModel");
            sb.AppendLine("    {");

            foreach (var col in schema.Columns.Where(c => selectedSet.Contains(c.ColumnName)))
            {
                EmitProperty(sb, col);
            }

            // FK RETURN COLUMNS (flat properties)
            if (fkSelections != null)
            {
                foreach (var fk in fkSelections)
                {
                    foreach (var rc in fk.Value)
                    {
                        sb.AppendLine("        // FK return column from " + fk.Key);
                        sb.AppendLine("        public string " + SafePropertyName(rc) + " { get; set; }");
                        sb.AppendLine();
                    }
                }
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        // =====================================================
        // HELPERS
        // =====================================================
        private static void EmitProperty(StringBuilder sb, ColumnSchema c)
        {
            var type = MapCSharpType(c);
            var prop = SafePropertyName(c.ColumnName);

            if (!c.IsNullable && type != "string")
                sb.AppendLine("        [Required]");

            if (IsStringType(c.DataType) && c.MaxLength.HasValue)
                sb.AppendLine("        [StringLength(" + c.MaxLength.Value + ")]");

            sb.AppendLine("        public " + type + " " + prop + " { get; set; }");
            sb.AppendLine();
        }

        private static string MapCSharpType(ColumnSchema c)
        {
            var dt = (c.DataType ?? "").ToLowerInvariant();
            string type;

            if (dt.Contains("char") || dt.Contains("text"))
                type = "string";
            else if (dt.Contains("int"))
                type = "int";
            else if (dt == "bigint")
                type = "long";
            else if (dt == "decimal" || dt == "numeric")
                type = "decimal";
            else if (dt.Contains("date") || dt.Contains("time"))
                type = "DateTime";
            else if (dt == "bit" || dt == "boolean")
                type = "bool";
            else
                type = "string";

            if (c.IsNullable && type != "string")
                type += "?";

            return type;
        }

        private static bool IsStringType(string dt)
        {
            if (string.IsNullOrEmpty(dt)) return false;
            dt = dt.ToLowerInvariant();
            return dt.Contains("char") || dt.Contains("text");
        }

        private static string SafePropertyName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Field";

            var parts = name.Split(new[] { '_', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var s = string.Concat(parts.Select(p => char.ToUpper(p[0]) + p.Substring(1)));

            if (!char.IsLetter(s[0]) && s[0] != '_')
                s = "_" + s;

            return s;
        }
    }
}
