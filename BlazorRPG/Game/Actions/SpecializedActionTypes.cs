public enum SpecializedActionTypes
{
    // Generated systematically from context combinations
    // LABOR + Industrial + Public + Organized = "LoadCargo"
    LoadCargo,
    // LABOR + Industrial + Technical + Complex = "OperateMill"  
    OperateMill,
    // LABOR + Social + Public + Service = "ServeDrinks"
    ServeDrinks,

    // GATHER specializations 
    CollectMaterials,        // Industrial + Organized + Legal
    MarketBrowse,           // Commercial + Public + Legal 
    ForageMushrooms,         // Nature + Wild + Solo

    // TRADE specializations
    NegotiateContract,       // Commercial + Private + Official
    MarketBarter,           // Commercial + Public + Informal
    SellGoods,              // Commercial + Public + Legal

    // MINGLE specializations  
    TavernGossip,           // Social + Public + Casual
    CourtlyConverse,        // Social + Formal + Organized
    MarketChatter,          // Commercial + Public + Casual

    // INVESTIGATE specializations
    PatrolWatch,            // Industrial + Official + Active
    StudyDocuments,         // Commercial + Private + Technical  
    TrackAnimals            // Nature + Wild + Technical
}