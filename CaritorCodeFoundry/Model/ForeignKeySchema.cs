namespace CodeFoundry.Generator.Models
{
    public class ForeignKeySchema
    {
        public string ConstraintName { get; set; }
        public string ColumnName { get; set; }
        public string ReferencedTable { get; set; }
        public string ReferencedColumn { get; set; }
    }
}
