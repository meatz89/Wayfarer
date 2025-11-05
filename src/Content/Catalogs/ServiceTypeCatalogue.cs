using Wayfarer.GameState;

namespace Wayfarer.Content.Catalogues;

public static class ServiceTypeCatalogue
{
    private static readonly List<ServiceType> _allServiceTypes = new List<ServiceType>
    {
        new LodgingService(),
        new TrainingService(),
        new NegotiationService(),
        new ConfrontationService(),
        new InvestigationService(),
        new SocialManeuveringService(),
        new CrisisService(),
        new GenericService()
    };

    public static ServiceType GetByIdOrThrow(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            ServiceType generic = _allServiceTypes.FirstOrDefault(s => s.Id.Equals("generic", StringComparison.OrdinalIgnoreCase));
            if (generic == null)
                throw new InvalidDataException("Generic service type not found in catalogue");
            return generic;
        }

        ServiceType found = _allServiceTypes.FirstOrDefault(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

        if (found == null)
        {
            string validIds = string.Join(", ", _allServiceTypes.Select(s => s.Id));
            throw new InvalidDataException($"Unknown serviceType: '{id}'. Valid values: {validIds}");
        }

        return found;
    }
}
