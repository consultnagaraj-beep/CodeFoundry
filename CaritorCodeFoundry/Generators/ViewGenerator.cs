using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeFoundry.Generator.Models;
using CodeFoundry.Generator.Tools;

namespace CodeFoundry.Generator.Generators
{
    public class ViewGenerator
    {
        // =====================================================
        // PUBLIC ENTRY – REQUIRED BY GeneratorOrchestrator
        // =====================================================
        public static Dictionary<string, string> Generate(TableSchemaDto schema)
        {
            var files = new Dictionary<string, string>();

            var entityName = ToPascal(
                schema.TableName.Replace("tbl_", ""));

            // Index View
            files[$"Web/Views/{entityName}/Index.cshtml"]
                = GenerateIndexView(entityName);

            // Details Partial View
            files[$"Web/Views/{entityName}/_Details.cshtml"]
                = GenerateDetailsView(entityName);

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
        // EXISTING LOGIC – UNTOUCHED
        // =====================================================
        private static string GenerateIndexView(string entityName)
        {
            var sb = new StringBuilder();

            sb.AppendLine("@{");
            sb.AppendLine($"    ViewBag.Title = \"{entityName}\";");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("<div class=\"container-fluid\">");
            sb.AppendLine("    <div class=\"row\">");
            sb.AppendLine("        <div class=\"col-12\">");
            sb.AppendLine("            <div class=\"infogrid\"");
            sb.AppendLine($"                 data-controller=\"{entityName}\"");
            sb.AppendLine($"                 data-grid=\"{entityName}Grid\">");
            sb.AppendLine("            </div>");
            sb.AppendLine("        </div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("</div>");
            sb.AppendLine();
            sb.AppendLine("@section Scripts {");
            sb.AppendLine("    <script src=\"~/Scripts/infogrid.server.js\"></script>");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static string GenerateDetailsView(string entityName)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"@model CariHRMS.ViewModels.{entityName}DetailsVm");
            sb.AppendLine();
            sb.AppendLine("<div class=\"container-fluid\">");
            sb.AppendLine("    <div class=\"row\">");
            sb.AppendLine("        <div class=\"col-12\">");
            sb.AppendLine("            <table class=\"table table-bordered table-sm mb-0\">");
            sb.AppendLine("                <tbody>");
            sb.AppendLine("                @foreach (var field in Model.Fields)");
            sb.AppendLine("                {");
            sb.AppendLine("                    <tr>");
            sb.AppendLine("                        <th class=\"w-25\">@field.Label</th>");
            sb.AppendLine("                        <td>@field.Value</td>");
            sb.AppendLine("                    </tr>");
            sb.AppendLine("                }");
            sb.AppendLine("                </tbody>");
            sb.AppendLine("            </table>");
            sb.AppendLine("        </div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("</div>");

            return sb.ToString();
        }
    }
}
