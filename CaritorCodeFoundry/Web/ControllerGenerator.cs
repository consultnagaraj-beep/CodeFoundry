using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CodeFoundry.Generator.Models;

namespace CariHRMS.Web.Generators
{
    public static class ControllerGenerator
    {
        public static Dictionary<string, string> Generate(TableSchemaDto schema)
        {
            var files = new Dictionary<string, string>();

            var entityName = CodeFoundry.Generator.Tools.NamingHelper
                .ToPascalCase(schema.TableName);

            files[$"Web/Controllers/{entityName}Controller.cs"]
                = GenerateControllerCode(entityName, schema);

            return files;
        }

        private static string GenerateControllerCode(
            string entityName,
            TableSchemaDto schema)
        {
            var sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Web.Mvc;");
            sb.AppendLine("using CariHRMS.DAL.Repositories;");
            sb.AppendLine("using CariHRMS.DTO.MetaDataModels;");
            sb.AppendLine("using CariHRMS.DTOs.ViewModels;");
            sb.AppendLine("using CariHRMS.Web.Models;");
            sb.AppendLine();
            sb.AppendLine("namespace CariHRMS.Web.Controllers");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {entityName}Controller : Controller");
            sb.AppendLine("    {");
            sb.AppendLine($"        private readonly {entityName}Repository _repo;");
            sb.AppendLine();
            sb.AppendLine($"        public {entityName}Controller()");
            sb.AppendLine("        {");
            sb.AppendLine($"            _repo = new {entityName}Repository();");
            sb.AppendLine("        }");
            sb.AppendLine();

            // =====================================================
            // INDEX — DO NOT TOUCH
            // =====================================================
            sb.AppendLine("        public ActionResult Index()");
            sb.AppendLine("        {");
            sb.AppendLine($"            var metadata = new {entityName}InfoGridMetaData();");
            sb.AppendLine();
            sb.AppendLine("            var data = _repo.GetInfoGrid();");
            sb.AppendLine();
            sb.AppendLine("            var vm = new InfoGridPageVm");
            sb.AppendLine("            {");
            sb.AppendLine("                Metadata = metadata,");
            sb.AppendLine("                Data = data");
            sb.AppendLine("            };");
            sb.AppendLine();
            sb.AppendLine("            return View(vm);");
            sb.AppendLine("        }");
            sb.AppendLine();

            // =====================================================
            // DETAILS — STRONGLY TYPED NORMAL VIEWMODEL
            // =====================================================
            sb.AppendLine("        [HttpGet]");
            sb.AppendLine("        public PartialViewResult Details(int id, string mode = \"View\")");
            sb.AppendLine("        {");
            sb.AppendLine("            var dt = _repo.GetById(id);");
            sb.AppendLine();
            sb.AppendLine($"            var vm = new {entityName}NormalViewModel();");
            sb.AppendLine();
            sb.AppendLine("            if (dt.Rows.Count > 0)");
            sb.AppendLine("            {");
            sb.AppendLine("                var row = dt.Rows[0];");

            foreach (var col in schema.Columns)
            {
                var prop = col.ColumnName;// CodeFoundry.Generator.Tools.NamingHelper
                    //.ToPascalCase(col.ColumnName);

                sb.AppendLine(
                    $"                if (dt.Columns.Contains(\"{col.ColumnName}\"))");
                sb.AppendLine(
                    $"                    vm.{prop} = row[\"{col.ColumnName}\"] == DBNull.Value ? default : ({GetClrType(col)})row[\"{col.ColumnName}\"];");
            }

            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            ViewBag.Mode = mode;");
            sb.AppendLine("            return PartialView(\"_Details\", vm);");
            sb.AppendLine("        }");

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static string GetClrType(ColumnSchema c)
        {
            var dt = (c.DataType ?? "").ToLowerInvariant();

            if (dt.Contains("char") || dt.Contains("text"))
                return "string";
            if (dt.Contains("int"))
                return "int";
            if (dt == "bigint")
                return "long";
            if (dt == "decimal" || dt == "numeric")
                return "decimal";
            if (dt.Contains("date") || dt.Contains("time"))
                return "DateTime";
            if (dt == "bit" || dt == "boolean")
                return "bool";

            return "string";
        }
    }
}
