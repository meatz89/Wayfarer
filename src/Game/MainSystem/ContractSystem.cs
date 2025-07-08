using System.Text;

public class ContractSystem
{
    public string FormatActiveContracts(List<Contract> contracts)
    {
        StringBuilder sb = new StringBuilder();

        if (contracts == null || !contracts.Any())
            return "None";

        foreach (Contract? contract in contracts)
        {
            sb.AppendLine($"- {contract.Id}: {contract.Description} (at {contract.DestinationLocation})");
        }

        return sb.ToString();
    }

}