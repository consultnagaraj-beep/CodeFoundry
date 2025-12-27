using System.Text;

namespace CodeFoundry.Generator.Generators
{
    public static class GridInterfaceGenerator
    {
        public static string Generate(
            string tableName,
            string gridType,
            string viewModelNamespace = "CariHRMS.DTOs.ViewModels")
        {
            var interfaceName = $"I{tableName}{gridType}GridService";
            var viewModelName = $"{tableName}{gridType}GridViewModel";

            var sb = new StringBuilder();

            sb.AppendLine("using CariHRMS.DTOs.Common;");
            sb.AppendLine($"using {viewModelNamespace};");
            sb.AppendLine();
            sb.AppendLine("namespace CariHRMS.Contracts.Grids");
            sb.AppendLine("{");
            sb.AppendLine($"    public interface {interfaceName}");
            sb.AppendLine("    {");
            sb.AppendLine($"        GridResult<{viewModelName}> GetGridData(GridRequest request);");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}
