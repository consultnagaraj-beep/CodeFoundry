using System;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CodeFoundry.Generator.Models.Metadata;
using System.IO;

namespace CodeFoundry.Generator.Helpers
{
    public static class GridHtmlHelpers
    {
        /// <summary>
        /// Renders an infogrid container and inlines metadata JSON (no Ajax).
        /// metaSource: either a pre-built EditGridMeta object OR virtual path to json file.
        /// If metaObject provided, it takes precedence.
        /// </summary>
        public static IHtmlString InfoGrid(this HtmlHelper html, string elementId, string gridId, EditGridMeta metaObject = null, string metaFileVirtualPath = null)
        {
            string metaJson = "[]";

            if (metaObject != null)
            {
                metaJson = metaObject.ToJson();
            }
            else if (!string.IsNullOrEmpty(metaFileVirtualPath))
            {
                var path = HttpContext.Current.Server.MapPath(metaFileVirtualPath);
                if (File.Exists(path))
                    metaJson = File.ReadAllText(path);
            }

            // encode for safe embedding in data attribute and for script tag fallback
            var encoded = HttpUtility.JavaScriptStringEncode(metaJson);

            var sb = new StringBuilder();
            sb.AppendLine($"<div id=\"{HttpUtility.HtmlAttributeEncode(elementId)}\" class=\"infogrid-card\" data-grid-id=\"{HttpUtility.HtmlAttributeEncode(gridId)}\" data-meta=\"{HttpUtility.HtmlAttributeEncode(metaJson)}\" style=\"height:100%;\">");
            sb.AppendLine("  <div class=\"ag-theme-balham\" style=\"height:100%;width:100%;\" id=\"" + HttpUtility.HtmlAttributeEncode(elementId) + "_grid\"></div>");
            sb.AppendLine("</div>");

            // Also inline a <script type='application/json'> block (useful if data-meta attribute length is problematic)
            sb.AppendLine($"<script id=\"{elementId}_meta\" type=\"application/json\">{metaJson}</script>");

            // Return markup
            return new MvcHtmlString(sb.ToString());
        }
    }
}
