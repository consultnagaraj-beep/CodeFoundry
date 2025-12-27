using System;
using System.Collections.Generic;
using System.Text;
using CodeFoundry.Generator.Models;
using CodeFoundry.Generator.Tools;

namespace CariHRMS.DTOs.Generators
{
    public static class DtoGenerator
    {
        // =========================================================
        // PUBLIC ENTRY
        // =========================================================
        public static Dictionary<string, string> GenerateDto(
            TableSchemaDto schema)
        {
            if (schema == null)
                throw new ArgumentNullException(nameof(schema));

            var files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var dtoName = schema.TableName + "Dto";
            var sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine();
            sb.AppendLine("namespace CariHRMS.DTOs");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {dtoName}");
            sb.AppendLine("    {");

            foreach (var col in schema.Columns)
            {
                sb.AppendLine(
                    $"        public {MapToCSharp(col)} {col.ColumnName} {{ get; set; }}");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            files[$"DTOs/{dtoName}.cs"] = sb.ToString();

            return files;
        }

        // =========================================================
        // TYPE MAPPING (STRICT + SAFE)
        // =========================================================
        private static string MapToCSharp(ColumnSchema c)
        {
            var dt = (c.DataType ?? "").ToLowerInvariant();
            bool nullable = c.IsNullable;

            string type;

            if (dt.Contains("char") || dt.Contains("text"))
                type = "string";
            else if (dt.Contains("bigint"))
                type = "long";
            else if (dt.Contains("int"))
                type = "int";
            else if (dt.Contains("decimal") || dt.Contains("numeric"))
                type = "decimal";
            else if (dt.Contains("float"))
                type = "float";
            else if (dt.Contains("double"))
                type = "double";
            else if (dt.Contains("date") || dt.Contains("time"))
                type = "DateTime";
            else if (dt.Contains("bit") || dt.Contains("bool"))
                type = "bool";
            else
                type = "string";

            // nullable handling (NO nullable for string)
            if (nullable && type != "string")
                type += "?";

            return type;
        }
    }
}
