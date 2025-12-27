using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using CodeFoundry.Generator.Tools;
using CodeFoundry.Generator.Models;

namespace CodeFoundry.Generator.Generators
{
    /// <summary>
    /// Metadata model for an EditGrid field.
    /// This drives the EditGrid UI and generation of EditGrid/ViewModel metadata.
    /// </summary>
    public class EditGridFieldMeta
    {
        // Basic identity
        public string FieldName { get; set; }
        public string DisplayName { get; set; }

        // Visibility / layout
        public bool IsHidden { get; set; } = false;        // not shown on edit form
        public bool IsReadOnly { get; set; } = false;      // shown but not editable
        public int Order { get; set; }

        // Control / UI
        public string ControlType { get; set; } = "TextBox"; // TextBox, TextArea, Numeric, Date, CheckBox, DropDown
        public bool IsNumeric { get; set; } = false;
        public int? MaxLength { get; set; }                 // for text controls
        public string Placeholder { get; set; }

        // Validation hints (generator will emit DataAnnotations based on these)
        public bool IsRequired { get; set; } = false;
        public int? DecimalPlaces { get; set; }             // for numeric fields (eg: 2)
        public string RegexValidation { get; set; }         // custom regex

        // Dropdown configuration (if ControlType == "DropDown")
        public DropDownMeta DropDown { get; set; }

        // Formula fields (calculated client-side or server-side)
        public FormulaFieldMeta Formula { get; set; }

        // Additional custom metadata bag for future features
        public Dictionary<string, object> Extras { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Dropdown metadata describing source and returned columns.
    /// </summary>
    public class DropDownMeta
    {
        // Source data: server URL or server-side source table name (generator uses SourceTable to create stub)
        public string SourceTable { get; set; }         // e.g. "Items"
        public string Url { get; set; }                 // optional, if grid will fetch via HTTP
        public string ValueField { get; set; }          // e.g. "Id"
        public string TextField { get; set; }           // e.g. "Name"
        public List<string> ReturnColumns { get; set; } // columns to map back to form (e.g. ["Name","UnitName","Price"])
        public string ReturnAs { get; set; } = "object"; // "object" or "csv"
        public string CsvSeparator { get; set; } = ",";
        public string OrderBy { get; set; }
    }

    /// <summary>
    /// Formula field metadata: expression or script to compute value.
    /// Expression language is simple: supports arithmetic using other field names in braces.
    /// Example: "{Qty} * {UnitPrice} - {Discount}"
    /// The generator will only emit the formula as metadata; execution handled by client/server.
    /// </summary>
    public class FormulaFieldMeta
    {
        public string Expression { get; set; }          // formula text
        public string TargetField { get; set; }         // where to put result (optional if formula field is itself the target)
        public string Description { get; set; }
        public bool EvaluateClientSide { get; set; } = true;
        public bool EvaluateServerSide { get; set; } = false;
    }

    /// <summary>
    /// EditGrid metadata generator.
    /// Produces JSON array of EditGridFieldMeta from table schema.
    /// Default values are conservative; UI can allow overrides before final generation.
    /// </summary>
    public static class EditGridMetaGenerator
    {
        /// <summary>
        /// Generate EditGrid metadata JSON.
        /// lockedHidden: columns that must remain hidden (server-enforced)
        /// dropdownHints: optional mapping of columnName -> DropDownMeta to prefill dropdown fields
        /// formulaHints: optional mapping of columnName -> FormulaFieldMeta to prefill formula fields
        /// </summary>
        public static string GenerateJson(TableSchemaDto schema,
            IEnumerable<string> lockedHidden = null,
            Dictionary<string, DropDownMeta> dropdownHints = null,
            Dictionary<string, FormulaFieldMeta> formulaHints = null)
        {
            if (schema == null) throw new ArgumentNullException(nameof(schema));
            var lockSet = new HashSet<string>(lockedHidden ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            dropdownHints = dropdownHints ?? new Dictionary<string, DropDownMeta>(StringComparer.OrdinalIgnoreCase);
            formulaHints = formulaHints ?? new Dictionary<string, FormulaFieldMeta>(StringComparer.OrdinalIgnoreCase);

            var list = new List<EditGridFieldMeta>();
            var cols = schema.Columns ?? Enumerable.Empty<ColumnSchema>();
            int order = 1;

            foreach (var c in cols)
            {
                var meta = new EditGridFieldMeta
                {
                    FieldName = c.ColumnName,
                    DisplayName = ToFriendly(c.ColumnName),
                    IsHidden = lockSet.Contains(c.ColumnName),
                    IsReadOnly = c.IsPrimaryKey && c.IsAutoIncrement, // PK auto identity read-only
                    Order = order++,
                    ControlType = InferControlType(c),
                    IsNumeric = IsNumericType(c.DataType),
                    MaxLength = c.MaxLength,
                    IsRequired = !c.IsNullable && !IsStringType(c.DataType) ? true : !c.IsNullable && IsStringType(c.DataType),
                    DecimalPlaces = c.NumericScale
                };

                // attach dropdown hint if supplied (UI can override)
                if (dropdownHints.TryGetValue(c.ColumnName, out var dd))
                    meta.DropDown = dd;

                // attach formula hint if supplied
                if (formulaHints.TryGetValue(c.ColumnName, out var fm))
                    meta.Formula = fm;

                list.Add(meta);
            }

            // JSON (indented) output
            return JsonConvert.SerializeObject(list.OrderBy(x => x.Order), Formatting.Indented);
        }

        #region Helpers

        private static bool IsNumericType(string dt)
        {
            if (string.IsNullOrEmpty(dt)) return false;
            var d = dt.ToLowerInvariant();
            return d.Contains("int") || d == "decimal" || d == "numeric" || d == "float" || d == "double" || d == "money";
        }

        private static bool IsStringType(string dt)
        {
            if (string.IsNullOrEmpty(dt)) return false;
            var d = dt.ToLowerInvariant();
            return d.Contains("char") || d.Contains("text") || d == "varchar" || d == "nvarchar";
        }

        private static string InferControlType(ColumnSchema c)
        {
            if (c == null) return "TextBox";
            var dt = (c.DataType ?? "").ToLowerInvariant();

            if (IsNumericType(dt)) return "Numeric";
            if (dt.StartsWith("date") || dt.StartsWith("datetime") || dt.StartsWith("timestamp")) return "Date";
            if (dt.Contains("text") || (c.MaxLength.HasValue && c.MaxLength.Value > 200)) return "TextArea";
            if (dt == "bit" || dt == "boolean" || dt == "tinyint(1)") return "CheckBox";

            // default
            return "TextBox";
        }

        private static string ToFriendly(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var noUnderscore = s.Replace('_', ' ');
            var friendly = System.Text.RegularExpressions.Regex.Replace(noUnderscore, "(?<!^)([A-Z])", " $1");
            var parts = friendly.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                               .Select(p => char.ToUpperInvariant(p[0]) + (p.Length > 1 ? p.Substring(1).ToLowerInvariant() : ""));
            return string.Join(" ", parts);
        }

        #endregion
    }
}
