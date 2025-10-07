public static class LocationParser
{
    /// <summary>
    /// Convert a LocationDTO to a Location domain model
    /// </summary>
    public static Location ConvertDTOToLocation(LocationDTO dto)
    {
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidOperationException("Location DTO missing required 'Id' field");
        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidOperationException($"Location {dto.Id} missing required 'Name' field");
        if (string.IsNullOrEmpty(dto.LocationType))
            throw new InvalidOperationException($"Location {dto.Id} missing required 'LocationType' field");

        if (!Enum.TryParse(dto.LocationType, out LocationTypes locationType))
            throw new InvalidOperationException($"Location {dto.Id} has invalid LocationType: '{dto.LocationType}'");

        // Parse investigation profile
        InvestigationDiscipline investigationProfile = InvestigationDiscipline.Research;
        if (!string.IsNullOrEmpty(dto.InvestigationProfile))
        {
            if (!System.Enum.TryParse<InvestigationDiscipline>(dto.InvestigationProfile, out investigationProfile))
            {
                investigationProfile = InvestigationDiscipline.Research;
            }
        }

        Location location = new Location(dto.Id, dto.Name)
        {
            Description = dto.Description ?? string.Empty, // Description is optional
            District = dto.DistrictId ?? string.Empty, // Populate District from DistrictId
            Tier = dto.Tier,
            DomainTags = dto.DomainTags ?? new List<string>(), // Empty list is valid for no tags
            LocationType = locationType,
            LocationTypeString = dto.LocationType,
            IsStartingLocation = dto.IsStartingLocation,
            InvestigationProfile = investigationProfile
        };

        // Parse environmental properties
        if (dto.EnvironmentalProperties != null)
        {
            location.MorningProperties = dto.EnvironmentalProperties.Morning ?? new List<string>(); // Empty list valid
            location.AfternoonProperties = dto.EnvironmentalProperties.Afternoon ?? new List<string>(); // Empty list valid
            location.EveningProperties = dto.EnvironmentalProperties.Evening ?? new List<string>(); // Empty list valid
            location.NightProperties = dto.EnvironmentalProperties.Night ?? new List<string>(); // Empty list valid
        }

        // Parse available professions by time
        if (dto.AvailableProfessionsByTime != null)
        {
            foreach (KeyValuePair<string, List<string>> kvp in dto.AvailableProfessionsByTime)
            {
                if (EnumParser.TryParse<TimeBlocks>(kvp.Key, out TimeBlocks timeBlock))
                {
                    List<Professions> professions = new List<Professions>();
                    foreach (string professionStr in kvp.Value)
                    {
                        if (EnumParser.TryParse<Professions>(professionStr, out Professions profession))
                        {
                            professions.Add(profession);
                        }
                    }
                    location.AvailableProfessionsByTime[timeBlock] = professions;
                }
            }
        }

        // Parse access requirement
        if (dto.AccessRequirement != null)
        {
            location.AccessRequirement = AccessRequirementParser.ConvertDTOToAccessRequirement(dto.AccessRequirement);
        }

        // Parse available work actions
        if (dto.AvailableWork != null)
        {
            foreach (WorkActionDTO workDto in dto.AvailableWork)
            {
                WorkAction workAction = new WorkAction
                {
                    Id = workDto.Id,
                    Name = workDto.Name,
                    Description = workDto.Description,
                    Type = Enum.TryParse<WorkType>(workDto.Type, out WorkType workType)
                        ? workType : WorkType.Standard,
                    BaseCoins = workDto.BaseCoins,
                    LocationId = workDto.LocationId,
                    SpotId = workDto.SpotId,
                    RequiredTokens = workDto.RequiredTokens,
                    RequiredTokenType = workDto.RequiredTokenType != null &&
                        Enum.TryParse<ConnectionType>(workDto.RequiredTokenType, out ConnectionType tokenType)
                        ? tokenType : null,
                    RequiredPermit = workDto.RequiredPermit,
                    HungerReduction = workDto.HungerReduction,
                    HealthRestore = workDto.HealthRestore,
                    GrantedItem = workDto.GrantedItem
                };
                location.AvailableWork.Add(workAction);
            }
        }

        return location;
    }

    private static List<string> GetStringArrayFromProperty(JsonElement element, string propertyName)
    {
        List<string> results = new List<string>();

        if (element.TryGetProperty(propertyName, out JsonElement arrayElement) &&
            arrayElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in arrayElement.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    string value = item.GetString();
                    if (string.IsNullOrWhiteSpace(value))
                        throw new InvalidOperationException($"Array property '{propertyName}' contains empty string in Location JSON");
                    results.Add(value);
                }
            }
        }

        return results;
    }

}
