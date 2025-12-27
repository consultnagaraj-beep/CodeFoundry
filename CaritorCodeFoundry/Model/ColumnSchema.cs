using System;

namespace CodeFoundry.Generator.Models
{
    /// <summary>
    /// Represents a single database column.
    /// Pure schema data — no UI or generator logic.
    /// </summary>
    public class ColumnSchema
    {
        // ---------------- BASIC COLUMN INFO ----------------

        public string ColumnName { get; set; }

        public string DataType { get; set; }

        public int? MaxLength { get; set; }

        public bool IsNullable { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool IsAutoIncrement { get; set; }

        public int? NumericPrecision { get; set; }

        public int? NumericScale { get; set; }

        // ---------------- FOREIGN KEY INFO ----------------
        // (Populated by SchemaReader.GetTableSchema)

        /// <summary>
        /// True if this column participates in a foreign key.
        /// </summary>
        public bool IsForeignKey { get; set; }

        /// <summary>
        /// Referenced table name (if FK).
        /// </summary>
        public string ReferencedTable { get; set; }

        /// <summary>
        /// Referenced column name (if FK).
        /// </summary>
        public string ReferencedColumn { get; set; }
    }
}
