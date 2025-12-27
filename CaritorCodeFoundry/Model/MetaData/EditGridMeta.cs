using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CodeFoundry.Generator.Models.Metadata
{
    public class EditGridColumnMeta
    {
        public string FieldName { get; set; }
        public string DisplayName { get; set; }
        public bool Hidden { get; set; }
        public bool IsRequired { get; set; }
        public bool IsNumeric { get; set; }
        public int? MaxLength { get; set; }
        public string ControlType { get; set; }
        public int Order { get; set; }
        public bool IsDropDown { get; set; }
        public string DropDownGridId { get; set; }
        public string DropDownValueField { get; set; }
        public string DropDownTextField { get; set; }
        public string DropDownReturnFields { get; set; }
        public bool IsFormula { get; set; }
        public string FormulaExpression { get; set; }
        public string FormulaOutputField { get; set; }
    }

    public class EditGridMeta
    {
        public string GridId { get; set; }
        public List<EditGridColumnMeta> Columns { get; set; } = new List<EditGridColumnMeta>();

        /// <summary>
        /// Serialize metadata to JSON with safe settings for inlining into HTML.
        /// Use this when generating files or in Razor helper for inline metadata.
        /// </summary>
        public string ToJson()
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };
            return JsonConvert.SerializeObject(this, settings);
        }
    }
}
