using System.Text;

public class OpportunitySystem
{
    public string FormatActiveOpportunities(List<Opportunity> opportunities)
    {
        StringBuilder sb = new StringBuilder();

        if (opportunities == null || !opportunities.Any())
            return "None";

        foreach (Opportunity? opportunity in opportunities.Where(o => o.Status == "Available"))
        {
            sb.AppendLine($"- {opportunity.Name}: {opportunity.Description} (at {opportunity.Location})");
        }

        return sb.ToString();
    }

}