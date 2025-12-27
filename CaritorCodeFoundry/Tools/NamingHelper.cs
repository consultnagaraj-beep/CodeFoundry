using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeFoundry.Generator.Tools
{
    /// <summary>
    /// Central naming utilities for generators.
    /// DO NOT put business logic here.
    /// </summary>
    public static class NamingHelper
    {
        // =====================================================
        // TABLE / ENTITY NAMING
        // =====================================================

        /// <summary>
        /// Converts DB names like:
        /// TBL_HR_CASTE_MASTER -> CasteMaster
        /// caste_master -> CasteMaster
        /// </summary>
        public static string ToPascalCase(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            var parts = SplitName(name);

            var sb = new StringBuilder();
            foreach (var part in parts)
            {
                sb.Append(Capitalize(part));
            }
            return char.ToUpperInvariant(sb.ToString()[0]) + sb.ToString().Substring(1);
            //return sb.ToString();
        }

        /// <summary>
        /// Converts DB names to camelCase:
        /// caste_name -> casteName
        /// </summary>
        public static string ToCamelCase(string name)
        {
            var pascal = ToPascalCase(name);
            if (string.IsNullOrEmpty(pascal))
                return pascal;

            return char.ToLowerInvariant(pascal[0]) + pascal.Substring(1);
        }

        // =====================================================
        // DISPLAY NAME (FOR UI / METADATA)
        // =====================================================

        /// <summary>
        /// Converts field names into user-friendly labels:
        /// CasteName -> Caste Name
        /// caste_name -> Caste Name
        /// DOJ -> DOJ
        /// </summary>
        public static string ToDisplayName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            // Preserve ALL CAPS words (DOB, DOJ, PAN, etc.)
            if (name.All(char.IsUpper))
                return name;

            // Handle snake_case first
            if (name.Contains("_"))
            {
                return string.Join(" ",
                    name.Split('_')
                        .Where(p => p.Length > 0)
                        .Select(Capitalize));
            }

            // Handle PascalCase / camelCase
            return Regex.Replace(
                name,
                "(?<=[a-z])([A-Z])",
                " $1",
                RegexOptions.Compiled
            );
        }

        // =====================================================
        // INTERNAL HELPERS
        // =====================================================

        private static string[] SplitName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Array.Empty<string>();

            // normalize once
            var normalized = name.ToLowerInvariant();

            normalized = normalized.Replace("tbl_", "");
            normalized = normalized.Replace("tbl", "");

            return normalized
                .Split(new[] { '_', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }


        private static string Capitalize(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value.ToLowerInvariant());
        }
    }
}
