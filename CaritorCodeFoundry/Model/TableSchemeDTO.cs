using System.Collections.Generic;

namespace CodeFoundry.Generator.Models
{
    public class TableSchemaDto
    {
        public string TableName { get; set; }
        public List<ColumnSchema> Columns { get; set; } = new List<ColumnSchema>();
        public List<string> PrimaryKey { get; set; } = new List<string>();
        public List<ForeignKeySchema> ForeignKeys { get; set; } = new List<ForeignKeySchema>();
        public string SchemaHash { get; set; }
    }
}
