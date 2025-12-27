using System.Text;

namespace CodeFoundry.Generator.Generators
{
    public static class GridManagerGenerator
    {
        public static string Generate(
            string tableName,
            string gridType)
        {
            var managerName = $"{tableName}{gridType}GridManager";
            var interfaceName = $"I{tableName}{gridType}GridService";
            var viewModelName = $"{tableName}{gridType}GridViewModel";

            var sb = new StringBuilder();

            sb.AppendLine("using CariHRMS.Contracts.Grids;");
            sb.AppendLine("using CariHRMS.DTOs.Common;");
            sb.AppendLine("using CariHRMS.DTOs.ViewModels;");
            sb.AppendLine();
            sb.AppendLine("namespace CariHRMS.BLL.Grids");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {managerName}");
            sb.AppendLine("    {");
            sb.AppendLine($"        private readonly {interfaceName} _service;");
            sb.AppendLine();
            sb.AppendLine($"        public {managerName}({interfaceName} service)");
            sb.AppendLine("        {");
            sb.AppendLine("            _service = service;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine($"        public GridResult<{viewModelName}> GetGrid(GridRequest request)");
            sb.AppendLine("        {");
            sb.AppendLine("            // Business rules go here");
            sb.AppendLine("            // Status, ApprStatus, SessionId filters");
            sb.AppendLine();
            sb.AppendLine("            if (request.PageSize <= 0)");
            sb.AppendLine("                request.PageSize = 20;");
            sb.AppendLine();
            sb.AppendLine("            return _service.GetGridData(request);");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}
