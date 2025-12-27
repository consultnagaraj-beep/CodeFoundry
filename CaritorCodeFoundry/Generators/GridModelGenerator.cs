using CodeFoundry.Generator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CariHRMS.Web.Generators
{
    public static class GridModelGenerator
    {
        public static Dictionary<string, string> Generate(
            string tableName,
            SelectionDto selection)
        {
            var files = new Dictionary<string, string>();

            // =====================================================
            // BASE GRID COLUMN (GENERATED ONCE)
            // =====================================================
            files["Web/GridModels/GridColumn.cs"] = GenerateGridColumnBase();

            // =====================================================
            // PER-TABLE GRID MODELS (EXISTING FLOW)
            // =====================================================
            var entityName = ToPascal(
                tableName.Replace("tbl_", ""));

            files[$"Web/GridModels/{entityName}GridModel.cs"]
                = GenerateTableGridModel(entityName, selection);

            return files;
        }
        private static string ToPascal(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var parts = s.Split(new[] { '_', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var res = string.Join("", parts.Select(p => char.ToUpperInvariant(p[0]) + (p.Length > 1 ? p.Substring(1) : "")));
            if (char.IsDigit(res[0])) res = "_" + res;
            return res;
        }
        // =====================================================
        // BASE MODEL (NO LOGIC)
        // =====================================================
        private static string GenerateGridColumnBase()
        {
            var sb = new StringBuilder();

            sb.AppendLine("namespace CariHRMS.Web.GridModels");
            sb.AppendLine("{");
            sb.AppendLine("    public class GridColumn");
            sb.AppendLine("    {");
            sb.AppendLine("        public string Field { get; set; }");
            sb.AppendLine("        public string HeaderName { get; set; }");
            sb.AppendLine("        public bool Hidden { get; set; }");
            sb.AppendLine("        public bool LockVisibility { get; set; }");
            sb.AppendLine("        public int? Width { get; set; }");
            sb.AppendLine("        public bool Sortable { get; set; }");
            sb.AppendLine("        public bool Filter { get; set; }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        // =====================================================
        // EXISTING TABLE GRID MODEL (LOGIC UNCHANGED)
        // =====================================================
        private static string GenerateTableGridModel(
            string entityName,
            SelectionDto selection)
        {
            var sb = new StringBuilder();

            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using CariHRMS.Web.GridModels;");
            sb.AppendLine();
            sb.AppendLine($"public static class {entityName}GridModel");
            sb.AppendLine("{");

            sb.AppendLine("    // ---------------- EditGrid ----------------");
            sb.AppendLine("    public static readonly List<GridColumn> EditGridColumns =");
            sb.AppendLine("        new List<GridColumn>");
            sb.AppendLine("        {");
            sb.AppendLine("        };");
            sb.AppendLine();

            sb.AppendLine("    // ---------------- DropDownGrid ----------------");
            sb.AppendLine("    public static readonly List<GridColumn> DropDownGridColumns =");
            sb.AppendLine("        new List<GridColumn>");
            sb.AppendLine("        {");
            sb.AppendLine("        };");

            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}
