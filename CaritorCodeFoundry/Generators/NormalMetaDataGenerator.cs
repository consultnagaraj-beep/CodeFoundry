using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeFoundry.Generator.Models;
using CodeFoundry.Generator.Tools;

namespace CodeFoundry.Generator.Generators
{
    /// <summary>
    /// Generates NormalMetaData (Details / Add / Edit / Approval / Audit)
    /// Grouped by GroupNo and ordered by Order.
    /// </summary>
    public static class NormalMetaDataGenerator
    {
        // =====================================================
        // PUBLIC ENTRY
        // =====================================================
        public static Dictionary<string, string> Generate(
            string tableName,
            SelectionDto selection)
        {
            var files = new Dictionary<string, string>();

            // Only NORMAL grid participates in Details screens
            if (!selection.GridTypes.Contains("Normal"))
                return files;

            var entityName = NamingHelper.ToPascalCase(
                tableName.Replace("tbl_", ""));

            var className = $"{entityName}NormalMetaData";

            var code = GenerateClass(
                className,
                selection);

            files[$"DTO/MetaDataModels/{className}.cs"] = code;

            return files;
        }

        // =====================================================
        // CLASS GENERATION
        // =====================================================
        private static string GenerateClass(
            string className,
            SelectionDto selection)
        {
            var sb = new StringBuilder();

            var selectedFields = selection.GetSelectedColumns("Normal");
            var validations = selection.GetFieldValidation("Normal");

            // ---------------- GROUPING ----------------
            var groups = new Dictionary<int, GroupEntry>();

            foreach (var field in selectedFields)
            {
                Dictionary<string, string> meta;
                validations.TryGetValue(field, out meta);

                int groupNo = GetInt(meta, "GroupNo", 1);
                int orderNo = GetInt(meta, "Order", 0);

                var groupTitle =
                    meta != null && meta.ContainsKey("GroupTitle")
                        ? meta["GroupTitle"]
                        : null;

                if (!groups.ContainsKey(groupNo))
                {
                    groups[groupNo] = new GroupEntry
                    {
                        GroupNo = groupNo,
                        GroupTitle = string.IsNullOrWhiteSpace(groupTitle)
                            ? $"Group {groupNo}"
                            : groupTitle,
                        Fields = new List<FieldEntry>()
                    };
                }

                groups[groupNo].Fields.Add(new FieldEntry
                {
                    FieldName = field,
                    Order = orderNo,
                    Validation = meta
                });
            }

            // =====================================================
            // FILE HEADER
            // =====================================================
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using CariHRMS.DTO.MetaDataModels;");
            sb.AppendLine();
            sb.AppendLine("namespace CariHRMS.DTO.MetaDataModels");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {className} : NormalMetaData");
            sb.AppendLine("    {");
            sb.AppendLine($"        public {className}()");
            sb.AppendLine("        {");

            // =====================================================
            // GROUP EMISSION
            // =====================================================
            foreach (var grp in groups.OrderBy(g => g.Key))
            {
                sb.AppendLine("            Groups.Add(new FieldGroupMetaData");
                sb.AppendLine("            {");
                sb.AppendLine($"                GroupNo = {grp.Value.GroupNo},");
                sb.AppendLine($"                GroupName = \"{grp.Value.GroupTitle}\",");
                sb.AppendLine("                Fields =");
                sb.AppendLine("                {");

                foreach (var f in grp.Value.Fields.OrderBy(x => x.Order))
                {
                    sb.AppendLine("                    new FieldMetaData");
                    sb.AppendLine("                    {");
                    sb.AppendLine($"                        FieldName   = \"{f.FieldName}\",");
                    sb.AppendLine($"                        DisplayName = \"{NamingHelper.ToDisplayName(f.FieldName)}\",");
                    sb.AppendLine($"                        Order       = {f.Order},");
                    sb.AppendLine($"                        ControlType = \"TextBox\",");
                    sb.AppendLine($"                        Validation  = {GenerateValidationBlock(f.Validation)}");
                    sb.AppendLine("                    },");
                }

                sb.AppendLine("                }");
                sb.AppendLine("            });");
            }

            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        // =====================================================
        // HELPERS (C# 7.3 SAFE)
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
                // IMPORTANT: Order, GroupNo, GroupTitle are NOT validation rules
                if (kv.Key == "Order" || kv.Key == "GroupNo" || kv.Key == "GroupTitle")
                    continue;

                sb.Append($"{{ \"{kv.Key}\", \"{kv.Value}\" }}, ");
            }

            sb.Append("}");

            return sb.ToString();
        }

        // =====================================================
        // INTERNAL DTOs
        // =====================================================
        private class GroupEntry
        {
            public int GroupNo;
            public string GroupTitle;
            public List<FieldEntry> Fields;
        }

        private class FieldEntry
        {
            public string FieldName;
            public int Order;
            public Dictionary<string, string> Validation;
        }
    }
}
