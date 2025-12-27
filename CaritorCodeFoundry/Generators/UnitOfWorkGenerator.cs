using System;
using System.Collections.Generic;
using System.Text;
using CodeFoundry.Generator.Tools;
using CodeFoundry.Generator.Models;

namespace CodeFoundry.Generator.Generators
{
    /// <summary>
    /// Generates DAL UnitOfWork class
    /// </summary>
    public static class UnitOfWorkGenerator
    {
        public static Dictionary<string, string> GenerateUnitOfWork(TableSchemaDto schema)
        {
            if (schema == null)
                throw new ArgumentNullException(nameof(schema));

            var files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var sb = new StringBuilder();

            sb.AppendLine("using CariHRMS.DAL.Repositories;");
            sb.AppendLine();
            sb.AppendLine("namespace CariHRMS.DAL");
            sb.AppendLine("{");
            sb.AppendLine("    public class UnitOfWork");
            sb.AppendLine("    {");
            sb.AppendLine("        private readonly string _connectionString;");
            sb.AppendLine();
            sb.AppendLine("        public UnitOfWork(string connectionString)");
            sb.AppendLine("        {");
            sb.AppendLine("            _connectionString = connectionString;");
            sb.AppendLine("        }");
            sb.AppendLine();

            // Repository property
            sb.AppendLine($"        private {schema.TableName}Repository _{schema.TableName.ToLower()}Repo;");
            sb.AppendLine($"        public {schema.TableName}Repository {schema.TableName}");
            sb.AppendLine("        {");
            sb.AppendLine("            get");
            sb.AppendLine("            {");
            sb.AppendLine($"                return _{schema.TableName.ToLower()}Repo ??");
            sb.AppendLine($"                       (_{schema.TableName.ToLower()}Repo = new {schema.TableName}Repository());");
            sb.AppendLine("            }");
            sb.AppendLine("        }");

            sb.AppendLine("    }");
            sb.AppendLine("}");

            files["DAL/UnitOfWork/UnitOfWork.cs"] = sb.ToString();
            return files;
        }
    }
}
