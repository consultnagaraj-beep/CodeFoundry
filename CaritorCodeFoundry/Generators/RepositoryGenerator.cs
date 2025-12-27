using System;
using System.Collections.Generic;
using System.Text;
using CodeFoundry.Generator.Tools;
using CodeFoundry.Generator.Models;

namespace CodeFoundry.Generator.Generators
{
    /// <summary>
    /// Generates DAL Repository classes (View-based for grids, Table-based for single record)
    /// </summary>
    public static class RepositoryGenerator
    {
        public static Dictionary<string, string> GenerateRepositories(TableSchemaDto schema)
        {
            if (schema == null)
                throw new ArgumentNullException(nameof(schema));

            var files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var entityName = NamingHelper.ToPascalCase(schema.TableName);
            var className = $"{entityName}Repository";

            var sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using MySql.Data.MySqlClient;");
            sb.AppendLine("using CariHRMS.DAL.Core;"); // DbContext
            sb.AppendLine();
            sb.AppendLine("namespace CariHRMS.DAL.Repositories");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {className}");
            sb.AppendLine("    {");
            sb.AppendLine("        private readonly CariHrmsDbContext _db;");
            sb.AppendLine();
            sb.AppendLine($"        public {className}()");
            sb.AppendLine("        {");
            sb.AppendLine("            _db = new CariHrmsDbContext();");
            sb.AppendLine("        }");
            sb.AppendLine();

            // -------- InfoGrid (VIEW) --------
            sb.AppendLine("        public DataTable GetInfoGrid()");
            sb.AppendLine("        {");
            sb.AppendLine($"            return ExecuteView(\"vw_{schema.TableName}_InfoGrid\");");
            sb.AppendLine("        }");
            sb.AppendLine();

            // -------- Get By Id (BASE TABLE – NOT VIEW) --------
            sb.AppendLine("        public DataTable GetById(int id)");
            sb.AppendLine("        {");
            sb.AppendLine($"            var sql = \"SELECT * FROM {schema.TableName} WHERE Id = @Id\";");
            sb.AppendLine();
            sb.AppendLine("            using (var cn = _db.CreateConnection())");
            sb.AppendLine("            using (var cmd = new MySqlCommand(sql, cn))");
            sb.AppendLine("            using (var da = new MySqlDataAdapter(cmd))");
            sb.AppendLine("            {");
            sb.AppendLine("                cmd.Parameters.AddWithValue(\"@Id\", id);");
            sb.AppendLine("                var dt = new DataTable();");
            sb.AppendLine("                da.Fill(dt);");
            sb.AppendLine("                return dt;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();

            // -------- Common View Executor --------
            sb.AppendLine("        private DataTable ExecuteView(string viewName)");
            sb.AppendLine("        {");
            sb.AppendLine("            using (var cn = _db.CreateConnection())");
            sb.AppendLine("            using (var da = new MySqlDataAdapter($\"SELECT * FROM {viewName}\", cn))");
            sb.AppendLine("            {");
            sb.AppendLine("                var dt = new DataTable();");
            sb.AppendLine("                da.Fill(dt);");
            sb.AppendLine("                return dt;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");

            sb.AppendLine("    }");
            sb.AppendLine("}");

            files[$"DAL/Repositories/{className}.cs"] = sb.ToString();
            return files;
        }
    }
}
