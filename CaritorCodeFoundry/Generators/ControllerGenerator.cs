using System;
using System.Linq;
using System.Text;
using CodeFoundry.Generator.Models;
using CodeFoundry.Generator.Tools;

namespace CodeFoundry.Generator.Generators
{
    /// <summary>
    /// Generates MVC Controller for a given table
    /// </summary>
    public static class ControllerGenerator
    {
        // =====================================================
        // PUBLIC API – CALLED FROM MainForm
        // =====================================================
        public static string Generate(
            TableSchemaDto table,
            string moduleName)
        {
            var className = ToPascal(table.TableName.Replace("tbl_", ""));
            var controllerName = className + "Controller";

            var sb = new StringBuilder();

            AppendUsings(sb);
            AppendNamespaceStart(sb);
            AppendClass(sb, controllerName, className, moduleName);
            AppendNamespaceEnd(sb);

            return sb.ToString();
        }

        // =====================================================
        // SECTIONS
        // =====================================================
        private static void AppendUsings(StringBuilder sb)
        {
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Web.Mvc;");
            sb.AppendLine("using CariHRMS.Services;");
            sb.AppendLine("using CariHRMS.ViewModels;");
            sb.AppendLine();
        }

        private static void AppendNamespaceStart(StringBuilder sb)
        {
            sb.AppendLine("namespace CariHRMS.Web.Controllers");
            sb.AppendLine("{");
        }

        private static void AppendNamespaceEnd(StringBuilder sb)
        {
            sb.AppendLine("}");
        }

        private static void AppendClass(
            StringBuilder sb,
            string controllerName,
            string entityName,
            string moduleName)
        {
            sb.AppendLine($"    public class {controllerName} : Controller");
            sb.AppendLine("    {");
            sb.AppendLine($"        private readonly I{entityName}Service _service;");
            sb.AppendLine();
            sb.AppendLine($"        public {controllerName}()");
            sb.AppendLine("        {");
            sb.AppendLine($"            _service = new {entityName}Service();");
            sb.AppendLine("        }");
            sb.AppendLine();

            AppendIndex(sb);
            AppendGrid(sb, entityName);
            AppendCreate(sb, entityName);
            AppendUpdate(sb, entityName);
            AppendDelete(sb);

            sb.AppendLine("    }");
        }

        // =====================================================
        // ACTIONS
        // =====================================================
        private static void AppendIndex(StringBuilder sb)
        {
            sb.AppendLine("        // ===============================");
            sb.AppendLine("        // PAGE LOAD");
            sb.AppendLine("        // ===============================");
            sb.AppendLine("        public ActionResult Index()");
            sb.AppendLine("        {");
            sb.AppendLine("            return View();");
            sb.AppendLine("        }");
            sb.AppendLine();
        }
        private static string ToPascal(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var parts = s.Split(new[] { '_', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var res = string.Join("", parts.Select(p => char.ToUpperInvariant(p[0]) + (p.Length > 1 ? p.Substring(1) : "")));
            if (char.IsDigit(res[0])) res = "_" + res;
            return res;
        }
        private static void AppendGrid(StringBuilder sb, string entityName)
        {
            sb.AppendLine("        // ===============================");
            sb.AppendLine("        // GRID DATA (InfoGrid / AG-Grid)");
            sb.AppendLine("        // ===============================");
            sb.AppendLine("        [HttpPost]");
            sb.AppendLine("        public JsonResult GetGridData(GridRequestVm request)");
            sb.AppendLine("        {");
            sb.AppendLine("            var result = _service.GetGrid(request);");
            sb.AppendLine("            return Json(result, JsonRequestBehavior.AllowGet);");
            sb.AppendLine("        }");
            sb.AppendLine();
        }

        private static void AppendCreate(StringBuilder sb, string entityName)
        {
            sb.AppendLine("        // ===============================");
            sb.AppendLine("        // CREATE");
            sb.AppendLine("        // ===============================");
            sb.AppendLine("        [HttpPost]");
            sb.AppendLine($"        public JsonResult Create({entityName}EditVm model)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (!ModelState.IsValid)");
            sb.AppendLine("                return Json(new { success = false, message = \"Invalid data\" });");
            sb.AppendLine();
            sb.AppendLine("            _service.Insert(model);");
            sb.AppendLine("            return Json(new { success = true });");
            sb.AppendLine("        }");
            sb.AppendLine();
        }

        private static void AppendUpdate(StringBuilder sb, string entityName)
        {
            sb.AppendLine("        // ===============================");
            sb.AppendLine("        // UPDATE");
            sb.AppendLine("        // ===============================");
            sb.AppendLine("        [HttpPost]");
            sb.AppendLine($"        public JsonResult Update({entityName}EditVm model)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (!ModelState.IsValid)");
            sb.AppendLine("                return Json(new { success = false, message = \"Invalid data\" });");
            sb.AppendLine();
            sb.AppendLine("            _service.Update(model);");
            sb.AppendLine("            return Json(new { success = true });");
            sb.AppendLine("        }");
            sb.AppendLine();
        }

        private static void AppendDelete(StringBuilder sb)
        {
            sb.AppendLine("        // ===============================");
            sb.AppendLine("        // DELETE");
            sb.AppendLine("        // ===============================");
            sb.AppendLine("        [HttpPost]");
            sb.AppendLine("        public JsonResult Delete(int id)");
            sb.AppendLine("        {");
            sb.AppendLine("            _service.Delete(id);");
            sb.AppendLine("            return Json(new { success = true });");
            sb.AppendLine("        }");
            sb.AppendLine();
        }
    }
}
