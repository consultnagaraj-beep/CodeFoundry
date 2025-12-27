using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeFoundry.Generator.Tools;
using CodeFoundry.Generator.Models;

namespace CodeFoundry.Generator.Generators
{
    /// <summary>
    /// Generates DAL Entity classes (1:1 mapping with DB table)
    /// No business logic, no UI logic.
    /// </summary>
    public static class EntityGenerator
    {
        public static Dictionary<string, string> GenerateEntities(TableSchemaDto schema)
        {
            if (schema == null)
                throw new ArgumentNullException(nameof(schema));

            var files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var sb = new StringBuilder();

            var className = schema.TableName + "Entity";

            sb.AppendLine("using System;");
            sb.AppendLine();
            sb.AppendLine("namespace CariHRMS.DAL.Entities");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {className}");
            sb.AppendLine("    {");

            foreach (var column in schema.Columns)
            {
                sb.AppendLine($"        public {MapCSharpType(column)} {column.ColumnName} {{ get; set; }}");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            var relativePath = $"DAL/Entities/{className}.cs";
            files[relativePath] = sb.ToString();

            return files;
        }

        // ---------------- HELPERS ----------------

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
            else if (dt == "float")
                type = "float";
            else if (dt == "double")
                type = "double";
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
    }
}
