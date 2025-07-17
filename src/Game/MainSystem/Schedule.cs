public enum Schedule
{
    Always,           // Available all time periods (innkeepers, guards)
    Market_Hours,     // Morning + Afternoon (traders, merchants)
    Workshop_Hours,   // Dawn + Morning + Afternoon (craftsmen)
    Library_Hours,    // Morning + Afternoon (scholars, librarians)
    Business_Hours,   // Morning + Afternoon (caravan masters, officials)
    Morning_Evening,  // Morning + Evening (village elders)
    Morning_Afternoon, // Morning + Afternoon (route planners, miners)
    Afternoon_Evening, // Afternoon + Evening (exotic traders)
    Evening_Only,     // Evening only (tavern keepers, entertainers)
    Morning_Only,     // Morning only (harbor masters)
    Afternoon_Only,   // Afternoon only (diplomatic couriers)
    Evening_Night,    // Evening + Night (information brokers, sea captains)
    Dawn_Only,        // Dawn only (early departing transport, farmers)
    Night_Only        // Night only (guards, special services)
}
