using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodeFoundry.Generator.Models;
using CodeFoundry.Generator.Tools;

namespace CodeFoundry.Generator.Generators
{
    public static class InfoGridViewModelBuilder
    {
        public static string BuildInfoGridViewModel(
            TableSchemaDto schema,
            DataGridView dgv)
        {
            var sb = new StringBuilder();

            string className =
                NamingHelper.ToPascalCase(schema.TableName) + "InfoGridVm";

            sb.AppendLine("public class " + className);
            sb.AppendLine("{");

            var rows = dgv.Rows
                .Cast<DataGridViewRow>()
                .Where(r => !r.IsNewRow)
                .Where(r => r.Cells["Use"].Value is bool b && b)
                .ToList();

            foreach (var row in rows)
            {
                string field = row.Cells["FieldName"].Value.ToString();

                // -------------------------------------------------
                // SKIP FK PARENT FIELD IF FK CHILD FIELDS EXIST
                // -------------------------------------------------
                if (!field.Contains("."))
                {
                    bool hasFkChildren =
                        rows.Any(r =>
                        {
                            var f = r.Cells["FieldName"].Value?.ToString();
                            return f != null &&
                                   f.StartsWith(field + ".", StringComparison.OrdinalIgnoreCase);
                        });

                    if (hasFkChildren)
                        continue;
                }

                string propertyName;
                string propertyType;

                // -----------------------------
                // FK CHILD FIELD (CityId.Code)
                // -----------------------------
                if (field.Contains("."))
                {
                    var parts = field.Split('.');
                    string parent = parts[0];
                    string child = parts[1];

                    var fk = schema.ForeignKeys.First(x =>
                        string.Equals(x.ColumnName, parent, StringComparison.OrdinalIgnoreCase));

                    // ✅ Property name MUST match View SQL alias
                    // Example: City.Code → CityCode
                    propertyName =
                        NamingHelper.ToPascalCase(fk.ReferencedTable) +
                        NamingHelper.ToPascalCase(child);

                    propertyType = InferClrType(
                        schema,
                        fk.ReferencedTable,
                        child);
                }
                // -----------------------------
                // BASE FIELD (non-FK)
                // -----------------------------
                else
                {
                    propertyName = NamingHelper.ToPascalCase(field);
                    propertyType = InferClrType(schema, schema.TableName, field);
                }

                sb.AppendLine($"    public {propertyType} {propertyName} {{ get; set; }}");
            }


            sb.AppendLine("}");

            return sb.ToString();
        }

        private static string InferClrType(
            TableSchemaDto schema,
            string tableName,
            string columnName)
        {
            var col = schema.Columns.FirstOrDefault(c =>
                string.Equals(c.ColumnName, columnName, StringComparison.OrdinalIgnoreCase));

            if (col == null)
                return "string";

            return TypeMapper.MapToClr(col.DataType, col.IsNullable);
        }
    }
}
