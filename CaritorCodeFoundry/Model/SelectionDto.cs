using System;
using System.Collections.Generic;

namespace CodeFoundry.Generator.Models
{
    /// <summary>
    /// SelectionDto: per-table, per-grid configuration selected by user.
    /// PURE data + defensive helpers.
    /// </summary>
    public class SelectionDto
    {
        public SelectionDto()
        {
            // Core maps
            PerGridColumns = NewListMap();
            PerGridHidden = NewListMap();
            PerGridUnhidable = NewListMap();

            PerGridFkSelections =
                new Dictionary<string, Dictionary<string, List<string>>>(StringComparer.OrdinalIgnoreCase);

            PerGridFieldValidation =
                new Dictionary<string, Dictionary<string, Dictionary<string, string>>>(StringComparer.OrdinalIgnoreCase);

            PerGridFormulaFields =
                new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

            PerGridFieldGroups =
                new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);

            PerGridFieldOrder =
                new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);

            // 🔴 NEW: table-level validation config (Normal only)

            GridTypes = new List<string>();
        }

        // -------------------- Table-level metadata --------------------

        /// <summary>
        /// Logical PascalCase table name approved by user (e.g., MasterTypeData).
        /// Used for Entity / ViewModel / View naming.
        /// </summary>
        public string LogicalTableName { get; set; }

        public string Notes { get; set; }

        /// <summary>
        /// Strongly-typed validation configuration for Normal (entity-level).
        /// </summary>
        public ValidationInfo Validation { get; set; } = new ValidationInfo();

        // -------------------- Core storage --------------------

        public Dictionary<string, List<string>> PerGridColumns { get; }
        public Dictionary<string, List<string>> PerGridHidden { get; }
        public Dictionary<string, List<string>> PerGridUnhidable { get; }
        public Dictionary<string, Dictionary<string, List<string>>> PerGridFkSelections { get; }
        public Dictionary<string, Dictionary<string, string>> PerGridFormulaFields { get; }
        public Dictionary<string, Dictionary<string, Dictionary<string, string>>> PerGridFieldValidation { get; }

        public Dictionary<string, Dictionary<string, int>> PerGridFieldGroups { get; }
        public Dictionary<string, Dictionary<string, int>> PerGridFieldOrder { get; }

        public List<string> GridTypes { get; }

        // -------------------- Read helpers --------------------

        public List<string> GetSelectedColumns(string gridType)
            => GetOrEmpty(PerGridColumns, gridType);

        public List<string> GetHiddenColumns(string gridType)
            => GetOrEmpty(PerGridHidden, gridType);

        public List<string> GetUnhidableColumns(string gridType)
            => GetOrEmpty(PerGridUnhidable, gridType);

        public Dictionary<string, List<string>> GetFkSelections(string gridType)
            => GetOrEmptyMap(PerGridFkSelections, gridType);

        public Dictionary<string, Dictionary<string, string>> GetFieldValidation(string gridType)
            => GetOrEmptyMap(PerGridFieldValidation, gridType);

        public Dictionary<string, string> GetFormulaFields(string gridType)
            => GetOrEmptyMap(PerGridFormulaFields, gridType);

        public Dictionary<string, int> GetFieldGroups(string gridType)
            => GetOrEmptyMap(PerGridFieldGroups, gridType);

        public Dictionary<string, int> GetFieldOrder(string gridType)
            => GetOrEmptyMap(PerGridFieldOrder, gridType);

        // -------------------- Write helpers --------------------
        // Stores display name metadata per grid type
        private readonly Dictionary<string, Dictionary<string, DisplayNameMeta>>
            _displayNames =
                new Dictionary<string, Dictionary<string, DisplayNameMeta>>(
                    StringComparer.OrdinalIgnoreCase);
        // ColumnName -> DataType (lowercase, mysql-style)
        public Dictionary<string, string> ColumnDataTypes { get; }
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        //public string GetDisplayName(string field)
        //{
        //    if (DisplayNames.TryGetValue(field, out var dn))
        //        return dn.DisplayName;

        //    return null;
        //}

        //public IEnumerable<string> GetOrderedFieldsForValidation()
        //{
        //    return Fields
        //        .Where(f => !HiddenFields.Contains(f.Key)) // exclude hidden
        //        .OrderBy(f => f.Value.GroupNo)
        //        .ThenBy(f => f.Value.Order)
        //        .Select(f => f.Key);
        //}
        public IEnumerable<string> GetValidationFieldOrder(string gridType)
        {
            var selected = GetSelectedColumns(gridType);
            var hidden = GetHiddenColumns(gridType);

            // selected already preserves grid order in your design
            foreach (var f in selected)
            {
                if (!hidden.Contains(f))
                    yield return f;
            }
        }

        public Dictionary<string, DisplayNameMeta> GetDisplayNames(string gridType)
        {
            if (!_displayNames.TryGetValue(gridType, out var dict))
            {
                dict = new Dictionary<string, DisplayNameMeta>(StringComparer.OrdinalIgnoreCase);
                _displayNames[gridType] = dict;
            }

            return dict;
        }

        public class DisplayNameMeta
        {
            public string DisplayName { get; set; }
            public bool IsEdited { get; set; }
        }

        public void SetSelectedColumns(string gridType, List<string> columns)
        {
            gridType = Normalize(gridType);
            PerGridColumns[gridType] = columns ?? new List<string>();
            RegisterGrid(gridType);
        }

        public void SetHiddenColumns(string gridType, List<string> columns)
        {
            gridType = Normalize(gridType);
            PerGridHidden[gridType] = columns ?? new List<string>();
            RegisterGrid(gridType);
        }

        public void SetUnhidableColumns(string gridType, List<string> columns)
        {
            gridType = Normalize(gridType);
            PerGridUnhidable[gridType] = columns ?? new List<string>();
            RegisterGrid(gridType);
        }

        public void SetFkSelections(string gridType, Dictionary<string, List<string>> fk)
        {
            gridType = Normalize(gridType);
            PerGridFkSelections[gridType] =
                fk ?? new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            RegisterGrid(gridType);
        }

        public void SetFieldValidation(string gridType, Dictionary<string, Dictionary<string, string>> validation)
        {
            gridType = Normalize(gridType);
            PerGridFieldValidation[gridType] =
                validation ?? new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            RegisterGrid(gridType);
        }

        public void SetFormulaFields(string gridType, Dictionary<string, string> formulas)
        {
            gridType = Normalize(gridType);
            PerGridFormulaFields[gridType] =
                formulas ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            RegisterGrid(gridType);
        }

        public void SetFieldGroups(string gridType, Dictionary<string, int> groups)
        {
            gridType = Normalize(gridType);
            PerGridFieldGroups[gridType] =
                groups ?? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            RegisterGrid(gridType);
        }

        public void SetFieldOrder(string gridType, Dictionary<string, int> order)
        {
            gridType = Normalize(gridType);
            PerGridFieldOrder[gridType] =
                order ?? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            RegisterGrid(gridType);
        }

        // -------------------- Internals --------------------

        private static string Normalize(string gridType)
            => string.IsNullOrWhiteSpace(gridType) ? "EditGrid" : gridType;

        private void RegisterGrid(string gridType)
        {
            gridType = Normalize(gridType);

            if (!GridTypes.Contains(gridType))
                GridTypes.Add(gridType);
        }

        private static List<string> GetOrEmpty(
            Dictionary<string, List<string>> map,
            string key)
        {
            if (map == null)
                return new List<string>();

            key = Normalize(key);

            if (map.TryGetValue(key, out var v) && v != null)
                return v;

            return new List<string>();
        }

        private static Dictionary<string, T> GetOrEmptyMap<T>(
            Dictionary<string, Dictionary<string, T>> map,
            string key)
        {
            if (map == null)
                return new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

            key = Normalize(key);

            if (map.TryGetValue(key, out var v) && v != null)
                return v;

            return new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        }

        private static Dictionary<string, List<string>> NewListMap()
        {
            return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        }
    }
    // =====================================================
    // VALIDATION (NORMAL VIEW ONLY)
    // =====================================================

    public class ValidationInfo
    {
        public Dictionary<string, FieldValidation> Fields
            = new Dictionary<string, FieldValidation>(StringComparer.OrdinalIgnoreCase);
    }

    public class FieldValidation
    {
        public UiValidation Ui { get; set; } = new UiValidation();
        public DbValidation Db { get; set; } = new DbValidation();
    }

    // -------- UI VALIDATION --------
    public class UiValidation
    {
        public RequiredRule Required { get; set; }
        public LengthRule MinLength { get; set; }
        public LengthRule MaxLength { get; set; }
        public AllowedValuesRule AllowedValues { get; set; }

        public string DateRuleType { get; set; }
        public string DateRuleMessage { get; set; }


        // 🔴 ADD ONLY THESE TWO LINES
        public string PatternType { get; set; }      // Email / Mobile / PAN / etc
        public string PatternMessage { get; set; }   // Error message
    }


    // -------- DB VALIDATION --------
    public class DbValidation
    {
        public RequiredRule Required { get; set; }
        public LengthRule MinLength { get; set; }
        public LengthRule MaxLength { get; set; }
        public AllowedValuesRule AllowedValues { get; set; }
        public UniqueRule Unique { get; set; }
    }

    // -------- RULES --------
    public class RequiredRule
    {
        public bool Enabled { get; set; }
        public string Message { get; set; }
    }

    public class LengthRule
    {
        public bool Enabled { get; set; }
        public int Value { get; set; }
        public string Message { get; set; }
    }

    public class AllowedValuesRule
    {
        public bool Enabled { get; set; }
        public string ValuesCsv { get; set; }
        public string Message { get; set; }
    }

    public class UniqueRule
    {
        public bool Enabled { get; set; }
        public string ColumnsCsv { get; set; }
        public string Message { get; set; }
    }

}
