using System.Collections.Generic;
using System.Text;
using CodeFoundry.Generator.Models;
using CodeFoundry.Generator.Tools;

namespace CodeFoundry.Generator.Generators
{
    public static class GridMetadataGenerator
    {
        // =====================================================
        // PUBLIC ENTRY
        // =====================================================
        public static Dictionary<string, string> Generate(
            string tableName,
            SelectionDto selection)
        {
            var files = new Dictionary<string, string>();

            var entityName = NamingHelper.ToPascalCase(
                tableName.Replace("tbl_", ""));

            foreach (var gridType in selection.GridTypes)
            {
                var className = $"{entityName}{gridType}MetaData";
                var code = GenerateMetaDataClass(
                    className,
                    gridType,
                    selection);

                files[$"DTO/MetaDataModels/{className}.cs"] = code;
            }

            return files;
        }

        // =====================================================
        // METADATA CLASS GENERATOR
        // =====================================================
        private static string GenerateMetaDataClass(
            string className,
            string gridType,
            SelectionDto selection)
        {
            var sb = new StringBuilder();

            var selectedFields = selection.GetSelectedColumns(gridType);

            var hiddenFields = new HashSet<string>(
                selection.GetHiddenColumns(gridType),
                System.StringComparer.OrdinalIgnoreCase);

            var unhidableFields = new HashSet<string>(
                selection.GetUnhidableColumns(gridType),
                System.StringComparer.OrdinalIgnoreCase);

            var validations = selection.GetFieldValidation(gridType);
            var formulas = selection.GetFormulaFields(gridType);

            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using CariHRMS.DTO.MetaDataModels;");
            sb.AppendLine();
            sb.AppendLine("namespace CariHRMS.DTO.MetaDataModels");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {className}");
            sb.AppendLine("    {");
            sb.AppendLine("        public List<FieldMetaData> Fields { get; } =");
            sb.AppendLine("            new List<FieldMetaData>");
            sb.AppendLine("            {");

            foreach (var fieldName in selectedFields)
            {
                Dictionary<string, string> fieldValidation;
                validations.TryGetValue(fieldName, out fieldValidation);

                string formula;
                formulas.TryGetValue(fieldName, out formula);

                int groupNo = GetInt(fieldValidation, "GroupNo", 1);
                int orderNo = GetInt(fieldValidation, "Order", 0);

                bool isVisible =
                    groupNo != 0 &&
                    !hiddenFields.Contains(fieldName);

                sb.AppendLine("                new FieldMetaData");
                sb.AppendLine("                {");
                sb.AppendLine($"                    FieldName   = \"{fieldName}\",");
                sb.AppendLine($"                    DisplayName = \"{NamingHelper.ToDisplayName(fieldName)}\",");
                sb.AppendLine($"                    IsVisible   = {isVisible.ToString().ToLower()},");
                sb.AppendLine($"                    IsUnhidable = {unhidableFields.Contains(fieldName).ToString().ToLower()},");
                sb.AppendLine($"                    Order       = {orderNo},");
                sb.AppendLine($"                    Formula     = {(formula != null ? $"\"{formula}\"" : "null")},");
                sb.AppendLine($"                    Validation  = {GenerateValidationBlock(fieldValidation)}");
                sb.AppendLine("                },");
            }

            sb.AppendLine("            };");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        // =====================================================
        // HELPERS — CLASS LEVEL (C# 7.3 SAFE)
        // =====================================================
        private static int GetInt(
            Dictionary<string, string> dict,
            string key,
            int defaultValue)
        {
            if (dict == null)
                return defaultValue;

            string val;
            if (!dict.TryGetValue(key, out val))
                return defaultValue;

            int parsed;
            return int.TryParse(val, out parsed)
                ? parsed
                : defaultValue;
        }

        private static string GenerateValidationBlock(
            Dictionary<string, string> validation)
        {
            if (validation == null || validation.Count == 0)
                return "null";

            var sb = new StringBuilder();
            sb.Append("new Dictionary<string, string>");
            sb.Append("{ ");

            foreach (var kv in validation)
            {
                // IMPORTANT: Order & GroupNo are NOT validation rules
                if (kv.Key == "Order" || kv.Key == "GroupNo")
                    continue;

                sb.Append($"{{ \"{kv.Key}\", \"{kv.Value}\" }}, ");
            }

            sb.Append("}");

            return sb.ToString();
        }
    }
}
