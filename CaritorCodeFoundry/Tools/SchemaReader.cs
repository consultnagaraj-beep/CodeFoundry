using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using MySql.Data.MySqlClient;
using CodeFoundry.Generator.Models;

namespace CodeFoundry.Generator.Tools
{
    /// <summary>
    /// Simple schema reader using MySQL INFORMATION_SCHEMA.
    /// Synchronous, explicit, and easy to read.
    /// </summary>
    public static class SchemaReader
    {
        /// <summary>
        /// Return list of user tables in the current database.
        /// </summary>
        public static List<string> GetTables(string connectionString)
        {
            var list = new List<string>();
            using (var cn = new MySqlConnection(connectionString))
            {
                cn.Open();
                var dbName = cn.Database;

                string sql = @"
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = @schema
  AND TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;";

                using (var cmd = new MySqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@schema", dbName);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                            list.Add(r.GetString(0));
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Returns column metadata for a given table.
        /// </summary>
        public static List<ColumnSchema> GetColumns(string connectionString, string tableName)
        {
            var cols = new List<ColumnSchema>();

            using (var cn = new MySqlConnection(connectionString))
            {
                cn.Open();
                var dbName = cn.Database;

                string sql = @"
SELECT
  COLUMN_NAME,
  DATA_TYPE,
  CHARACTER_MAXIMUM_LENGTH,
  IS_NULLABLE,
  COLUMN_KEY,
  NUMERIC_PRECISION,
  NUMERIC_SCALE,
  EXTRA
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = @schema
  AND TABLE_NAME = @table
ORDER BY ORDINAL_POSITION;";

                using (var cmd = new MySqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@schema", dbName);
                    cmd.Parameters.AddWithValue("@table", tableName);

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            cols.Add(new ColumnSchema
                            {
                                ColumnName = r["COLUMN_NAME"] as string,
                                DataType = r["DATA_TYPE"] as string,
                                MaxLength = r["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value
                                    ? (int?)Convert.ToInt32(r["CHARACTER_MAXIMUM_LENGTH"])
                                    : null,
                                IsNullable = string.Equals(
                                    r["IS_NULLABLE"] as string, "YES",
                                    StringComparison.OrdinalIgnoreCase),
                                IsPrimaryKey = (r["COLUMN_KEY"] as string) == "PRI",
                                NumericPrecision = r["NUMERIC_PRECISION"] != DBNull.Value
                                    ? (int?)Convert.ToInt32(r["NUMERIC_PRECISION"])
                                    : null,
                                NumericScale = r["NUMERIC_SCALE"] != DBNull.Value
                                    ? (int?)Convert.ToInt32(r["NUMERIC_SCALE"])
                                    : null,
                                IsAutoIncrement =
                                    (r["EXTRA"] as string)?.IndexOf(
                                        "auto_increment",
                                        StringComparison.OrdinalIgnoreCase) >= 0
                            });
                        }
                    }
                }
            }

            return cols;
        }

        /// <summary>
        /// Returns primary key column names (in order) for the table.
        /// </summary>
        public static List<string> GetPrimaryKey(string connectionString, string tableName)
        {
            var pk = new List<string>();

            using (var cn = new MySqlConnection(connectionString))
            {
                cn.Open();
                var dbName = cn.Database;

                string sql = @"
SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE TABLE_SCHEMA = @schema
  AND TABLE_NAME = @table
  AND CONSTRAINT_NAME = 'PRIMARY'
ORDER BY ORDINAL_POSITION;";

                using (var cmd = new MySqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@schema", dbName);
                    cmd.Parameters.AddWithValue("@table", tableName);

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                            pk.Add(r.GetString(0));
                    }
                }
            }

            return pk;
        }

        /// <summary>
        /// Return foreign key info for the table.
        /// </summary>
        public static List<ForeignKeySchema> GetForeignKeys(string connectionString, string tableName)
        {
            var fks = new List<ForeignKeySchema>();

            using (var cn = new MySqlConnection(connectionString))
            {
                cn.Open();
                var dbName = cn.Database;

                string sql = @"
SELECT
  k.CONSTRAINT_NAME,
  k.COLUMN_NAME,
  k.REFERENCED_TABLE_NAME,
  k.REFERENCED_COLUMN_NAME
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE k
JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
  ON k.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
 AND k.TABLE_SCHEMA = tc.TABLE_SCHEMA
 AND k.TABLE_NAME = tc.TABLE_NAME
WHERE k.TABLE_SCHEMA = @schema
  AND k.TABLE_NAME = @table
  AND tc.CONSTRAINT_TYPE = 'FOREIGN KEY'
ORDER BY k.CONSTRAINT_NAME, k.ORDINAL_POSITION;";

                using (var cmd = new MySqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@schema", dbName);
                    cmd.Parameters.AddWithValue("@table", tableName);

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            fks.Add(new ForeignKeySchema
                            {
                                ConstraintName = r["CONSTRAINT_NAME"] as string,
                                ColumnName = r["COLUMN_NAME"] as string,
                                ReferencedTable = r["REFERENCED_TABLE_NAME"] as string,
                                ReferencedColumn = r["REFERENCED_COLUMN_NAME"] as string
                            });
                        }
                    }
                }
            }

            return fks;
        }

        /// <summary>
        /// Compose TableSchemaDto with Columns, PKs, and FKs.
        /// FK information is mapped back into ColumnSchema (CRITICAL FIX).
        /// </summary>
        public static TableSchemaDto GetTableSchema(string connectionString, string tableName)
        {
            var cols = GetColumns(connectionString, tableName);
            var pk = GetPrimaryKey(connectionString, tableName);
            var fks = GetForeignKeys(connectionString, tableName);

            // 🔑 FK → COLUMN ENRICHMENT (REQUIRED FOR UI)
            foreach (var fk in fks)
            {
                var col = cols.Find(c =>
                    string.Equals(c.ColumnName, fk.ColumnName, StringComparison.OrdinalIgnoreCase));

                if (col != null)
                {
                    col.IsForeignKey = true;
                    col.ReferencedTable = fk.ReferencedTable;
                    col.ReferencedColumn = fk.ReferencedColumn;
                }
            }

            var dto = new TableSchemaDto
            {
                TableName = tableName,
                Columns = cols,
                PrimaryKey = pk,
                ForeignKeys = fks,
                SchemaHash = ComputeSchemaHash(cols, pk, fks)
            };

            return dto;
        }

        /// <summary>
        /// Compute canonical SHA256 hash of the table schema.
        /// </summary>
        private static string ComputeSchemaHash(
            List<ColumnSchema> cols,
            List<string> pk,
            List<ForeignKeySchema> fks)
        {
            var sb = new StringBuilder();

            foreach (var c in cols)
            {
                sb.Append(c.ColumnName).Append('|')
                  .Append(c.DataType).Append('|')
                  .Append(c.MaxLength).Append('|')
                  .Append(c.IsNullable).Append('|')
                  .Append(c.NumericPrecision).Append('|')
                  .Append(c.NumericScale).Append(';');
            }

            sb.Append("|PK:");
            if (pk != null)
                sb.Append(string.Join(",", pk));

            sb.Append("|FK:");
            if (fks != null)
            {
                foreach (var fk in fks)
                {
                    sb.Append(fk.ConstraintName).Append(':')
                      .Append(fk.ColumnName).Append("->")
                      .Append(fk.ReferencedTable).Append('(')
                      .Append(fk.ReferencedColumn).Append(");");
                }
            }

            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(sb.ToString());
                return BitConverter.ToString(sha.ComputeHash(bytes))
                    .Replace("-", "")
                    .ToUpperInvariant();
            }
        }
    }
}
