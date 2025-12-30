using System;

namespace CodeFoundry.Generator.Tools
{
    public static class TypeMapper
    {
        public static string MapToClr(string dbType, bool isNullable)
        {
            if (string.IsNullOrWhiteSpace(dbType))
                return "string";

            string type = dbType.ToLowerInvariant();

            string clrType;

            switch (type)
            {
                case "int":
                case "integer":
                case "mediumint":
                    clrType = "int";
                    break;

                case "bigint":
                    clrType = "long";
                    break;

                case "smallint":
                case "tinyint":
                    clrType = "short";
                    break;

                case "bit":
                case "boolean":
                case "bool":
                    clrType = "bool";
                    break;

                case "decimal":
                case "numeric":
                case "money":
                    clrType = "decimal";
                    break;

                case "float":
                    clrType = "decimal";
                    break;

                case "double":
                case "real":
                    clrType = "decimal";
                    break;

                case "date":
                case "datetime":
                case "timestamp":
                    clrType = "DateTime";
                    break;

                case "time":
                    clrType = "TimeSpan";
                    break;

                case "char":
                case "varchar":
                case "nvarchar":
                case "text":
                case "longtext":
                case "mediumtext":
                    clrType = "string";
                    break;

                case "blob":
                case "longblob":
                case "mediumblob":
                case "varbinary":
                case "binary":
                    clrType = "byte[]";
                    break;

                default:
                    clrType = "string";
                    break;
            }

            // Nullable handling (ONLY for value types)
            if (isNullable && IsValueType(clrType))
                return clrType + "?";

            return clrType;
        }

        private static bool IsValueType(string clrType)
        {
            switch (clrType)
            {
                case "int":
                case "long":
                case "short":
                case "bool":
                case "decimal":
                case "float":
                case "double":
                case "DateTime":
                case "TimeSpan":
                    return true;

                default:
                    return false;
            }
        }
    }
}
