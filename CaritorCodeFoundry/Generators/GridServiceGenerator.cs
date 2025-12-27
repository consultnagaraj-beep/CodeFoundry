using System;
using System.Collections.Generic;
using System.Text;

namespace CodeFoundry.Generator.Generators
{
    /// <summary>
    /// Generates a GENERIC grid service for CariHRMS.
    /// - View-based (NO stored procedures)
    /// - Supports AG-Grid server-side parameters
    /// - Reusable for ALL tables & grids
    /// </summary>
    public static class GridServiceGenerator
    {
        public static Dictionary<string, string> Generate()
        {
            var sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using MySql.Data.MySqlClient;");
            sb.AppendLine();
            sb.AppendLine("namespace CariHRMS.DAL.Services");
            sb.AppendLine("{");
            sb.AppendLine("    public class GridService");
            sb.AppendLine("    {");

            // ---------------------------------------------------------
            // CORE GRID DATA METHOD (AG-GRID READY)
            // ---------------------------------------------------------
            sb.AppendLine("        public DataTable GetGridData(");
            sb.AppendLine("            string connectionString,");
            sb.AppendLine("            string viewName,");
            sb.AppendLine("            int startRow,");
            sb.AppendLine("            int endRow,");
            sb.AppendLine("            string whereClause = null,");
            sb.AppendLine("            string orderBy = null)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (string.IsNullOrWhiteSpace(viewName))");
            sb.AppendLine("                throw new ArgumentException(\"View name is required\", nameof(viewName));");
            sb.AppendLine();
            sb.AppendLine("            var sql = new StringBuilder();");
            sb.AppendLine("            sql.Append(\"SELECT * FROM \").Append(viewName);");
            sb.AppendLine();
            sb.AppendLine("            if (!string.IsNullOrWhiteSpace(whereClause))");
            sb.AppendLine("                sql.Append(\" WHERE \").Append(whereClause);");
            sb.AppendLine();
            sb.AppendLine("            if (!string.IsNullOrWhiteSpace(orderBy))");
            sb.AppendLine("                sql.Append(\" ORDER BY \").Append(orderBy);");
            sb.AppendLine();
            sb.AppendLine("            if (endRow > startRow)");
            sb.AppendLine("                sql.Append(\" LIMIT \").Append(startRow).Append(\",\").Append(endRow - startRow);");
            sb.AppendLine();
            sb.AppendLine("            using (var cn = new MySqlConnection(connectionString))");
            sb.AppendLine("            using (var cmd = new MySqlCommand(sql.ToString(), cn))");
            sb.AppendLine("            using (var da = new MySqlDataAdapter(cmd))");
            sb.AppendLine("            {");
            sb.AppendLine("                var dt = new DataTable();");
            sb.AppendLine("                cn.Open();");
            sb.AppendLine("                da.Fill(dt);");
            sb.AppendLine("                return dt;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");

            // ---------------------------------------------------------
            // TOTAL COUNT (FOR AG-GRID pagination)
            // ---------------------------------------------------------
            sb.AppendLine();
            sb.AppendLine("        public int GetGridCount(");
            sb.AppendLine("            string connectionString,");
            sb.AppendLine("            string viewName,");
            sb.AppendLine("            string whereClause = null)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (string.IsNullOrWhiteSpace(viewName))");
            sb.AppendLine("                throw new ArgumentException(\"View name is required\", nameof(viewName));");
            sb.AppendLine();
            sb.AppendLine("            var sql = new StringBuilder();");
            sb.AppendLine("            sql.Append(\"SELECT COUNT(1) FROM \").Append(viewName);");
            sb.AppendLine();
            sb.AppendLine("            if (!string.IsNullOrWhiteSpace(whereClause))");
            sb.AppendLine("                sql.Append(\" WHERE \").Append(whereClause);");
            sb.AppendLine();
            sb.AppendLine("            using (var cn = new MySqlConnection(connectionString))");
            sb.AppendLine("            using (var cmd = new MySqlCommand(sql.ToString(), cn))");
            sb.AppendLine("            {");
            sb.AppendLine("                cn.Open();");
            sb.AppendLine("                return Convert.ToInt32(cmd.ExecuteScalar());");
            sb.AppendLine("            }");
            sb.AppendLine("        }");

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return new Dictionary<string, string>
            {
                { "DAL/Services/GridService.cs", sb.ToString() }
            };
        }
    }
}
