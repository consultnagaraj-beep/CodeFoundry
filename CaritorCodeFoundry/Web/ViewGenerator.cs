using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeFoundry.Generator.Models;
using CodeFoundry.Generator.Tools;

namespace CariHRMS.Web.Generators
{
    public static class ViewGenerator
    {
        // =====================================================
        // ENTRY
        // =====================================================
        public static Dictionary<string, string> Generate(
            TableSchemaDto schema,
            SelectionDto selection)
        {
            var files = new Dictionary<string, string>();

            var entityName = schema.TableName.Replace("tbl_", ""); // NamingHelper.ToPascalCase(
                //schema.TableName.Replace("tbl_", ""));

            files[$"Web/Views/{entityName}/Index.cshtml"] =
                GenerateIndexView(entityName);

            files[$"Web/Views/{entityName}/_Details.cshtml"] =
                GenerateDetailsView(entityName, schema, selection);

            return files;
        }

        // =====================================================
        // INDEX VIEW (FINAL – InfoGrid Helper)
        // =====================================================
        private static string GenerateIndexView(string entityName)
        {
            var sb = new StringBuilder();

            sb.AppendLine("@model CariHRMS.Web.Models.InfoGridPageVm");
            sb.AppendLine("@{");
            sb.AppendLine($"    ViewBag.Title = \"{entityName}\";");
            sb.AppendLine("    var controllerName = \"" + entityName + "\";");
            sb.AppendLine("    var gridData = Model.Data;");
            sb.AppendLine("    var metadata = Model.Metadata;");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("@Html.InfoGrid(");
            sb.AppendLine("    \"\",");
            sb.AppendLine("    gridData,");
            sb.AppendLine("    metadata,");
            sb.AppendLine("    controllerName,");
            sb.AppendLine("    height: 500");
            sb.AppendLine(")");
            sb.AppendLine();
            sb.AppendLine("@section Scripts {");
            sb.AppendLine("    <script src=\"~/Scripts/infogrid.server.js\"></script>");
            sb.AppendLine("}");

            return sb.ToString();
        }

        // =====================================================
        // DETAILS VIEW (FINAL – STRONGLY TYPED, TAB AWARE)
        // =====================================================
        private static string GenerateDetailsView(
            string entityName,
            TableSchemaDto schema,
            SelectionDto selection)
        {
            var sb = new StringBuilder();

            sb.AppendLine("@using CariHRMS.DTOs.ViewModels");
            sb.AppendLine($"@model {entityName}NormalViewModel");
            sb.AppendLine("@{");
            sb.AppendLine("    var mode = (ViewBag.Mode ?? \"View\").ToString();");
            sb.AppendLine("    var isFormMode = mode == \"Add\" || mode == \"Edit\";");
            sb.AppendLine("}");
            sb.AppendLine();

            // ---------- FORM START ----------
            sb.AppendLine("@using (Html.BeginFormFor(mode))");
            sb.AppendLine("{");

            // =====================================================
            // HIDDEN FIELDS (GroupNo = 0)
            // =====================================================
            var validations = selection.GetFieldValidation("Normal");

            foreach (var col in schema.Columns)
            {
                Dictionary<string, string> meta;
                validations.TryGetValue(col.ColumnName, out meta);

                var groupNo =
                    meta != null && meta.ContainsKey("GroupNo")
                        ? meta["GroupNo"]
                        : "1";

                if (groupNo == "0")
                {
                    sb.AppendLine($"    @Html.HiddenFor(m => m.{NamingHelper.ToPascalCase(col.ColumnName)})");
                }
            }

            sb.AppendLine();

            // =====================================================
            // GROUPING (EXCLUDING GroupNo = 0)
            // =====================================================
            var groups = new Dictionary<int, List<string>>();
            var groupTitles = new Dictionary<int, string>();

            foreach (var field in selection.GetSelectedColumns("Normal"))
            {
                Dictionary<string, string> meta;
                validations.TryGetValue(field, out meta);

                int groupNo = meta != null && meta.ContainsKey("GroupNo")
                    ? int.Parse(meta["GroupNo"])
                    : 1;

                if (groupNo == 0)
                    continue;

                if (!groups.ContainsKey(groupNo))
                {
                    groups[groupNo] = new List<string>();
                    groupTitles[groupNo] =
                        meta != null && meta.ContainsKey("GroupTitle") && !string.IsNullOrWhiteSpace(meta["GroupTitle"])
                            ? meta["GroupTitle"]
                            : $"Group {groupNo}";
                }

                groups[groupNo].Add(field);
            }

            bool useTabs = groups.Count > 1;

            // =====================================================
            // TABS HEADER
            // =====================================================
            if (useTabs)
            {
                sb.AppendLine("    <ul class=\"nav nav-tabs\" role=\"tablist\">");

                int idx = 0;
                foreach (var g in groups.OrderBy(x => x.Key))
                {
                    sb.AppendLine("        <li class=\"nav-item\">");
                    sb.AppendLine(
                        $"            <a class=\"nav-link {(idx == 0 ? "active" : "")}\" data-toggle=\"tab\" href=\"#tab_{g.Key}\">{groupTitles[g.Key]}</a>");
                    sb.AppendLine("        </li>");
                    idx++;
                }

                sb.AppendLine("    </ul>");
                sb.AppendLine();
                sb.AppendLine("    <div class=\"tab-content pt-3\">");
            }

            // =====================================================
            // GROUP CONTENT
            // =====================================================
            int tabIndex = 0;
            foreach (var g in groups.OrderBy(x => x.Key))
            {
                if (useTabs)
                {
                    sb.AppendLine(
                        $"        <div class=\"tab-pane fade {(tabIndex == 0 ? "show active" : "")}\" id=\"tab_{g.Key}\">");
                }

                foreach (var field in g.Value)
                {
                    var prop = field;// NamingHelper.ToPascalCase(field);

                    sb.AppendLine("            <div class=\"form-group\">");
                    sb.AppendLine($"                @Html.LabelFor(m => m.{prop}, new {{ @class = \"control-label\" }})");
                    sb.AppendLine($"                @Html.TextBoxFor(m => m.{prop}, new {{ @class = \"form-control\" }})");
                    sb.AppendLine($"                @Html.ValidationMessageFor(m => m.{prop})");
                    sb.AppendLine("            </div>");
                }

                if (useTabs)
                {
                    sb.AppendLine("        </div>");
                }

                tabIndex++;
            }

            if (useTabs)
            {
                sb.AppendLine("    </div>");
            }

            // =====================================================
            // ACTION BUTTONS (UNCHANGED)
            // =====================================================
            sb.AppendLine("    <div class=\"mt-3 text-right\">");
            sb.AppendLine("        <a class=\"btn btn-secondary\" href=\"#\" onclick=\"$('.modal').modal('hide'); return false;\">Back</a>");

            sb.AppendLine("        @if (mode == \"Add\" || mode == \"Edit\")");
            sb.AppendLine("        {");
            sb.AppendLine("            <button type=\"submit\" class=\"btn btn-success\">Save</button>");
            sb.AppendLine("            <button type=\"button\" class=\"btn btn-secondary\" onclick=\"$('.modal').modal('hide')\">Cancel</button>");
            sb.AppendLine("        }");

            sb.AppendLine("        @if (mode == \"Delete\")");
            sb.AppendLine("        {");
            sb.AppendLine("            <button type=\"submit\" class=\"btn btn-danger\">Delete</button>");
            sb.AppendLine("            <button type=\"button\" class=\"btn btn-secondary\" onclick=\"$('.modal').modal('hide')\">Cancel</button>");
            sb.AppendLine("        }");

            sb.AppendLine("        @if (mode == \"Approval\")");
            sb.AppendLine("        {");
            sb.AppendLine("            <button type=\"submit\" class=\"btn btn-success\">Approve</button>");
            sb.AppendLine("            <button type=\"submit\" class=\"btn btn-warning\">Reject</button>");
            sb.AppendLine("            <button type=\"submit\" class=\"btn btn-info\">Reassign</button>");
            sb.AppendLine("        }");

            sb.AppendLine("        @if (mode == \"Audit\")");
            sb.AppendLine("        {");
            sb.AppendLine("            <button type=\"button\" class=\"btn btn-primary\">Prev</button>");
            sb.AppendLine("            <button type=\"button\" class=\"btn btn-primary\">Next</button>");
            sb.AppendLine("        }");

            sb.AppendLine("    </div>");

            // ---------- FORM END ----------
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}
