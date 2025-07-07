using System.Text;

public class ContractSystem
{
    public string FormatActiveOpportunities(List<Contract> contracts)
    {
        StringBuilder sb = new StringBuilder();

        if (contracts == null || !contracts.Any())
            return "None";

        foreach (Contract? contract in contracts.Where(o =>
        {
            return o.Status == "Available";
        }))
        {
            sb.AppendLine($"- {contract.Name}: {contract.Description} (at {contract.Location})");
        }

        return sb.ToString();
    }

}