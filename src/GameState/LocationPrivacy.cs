/// <summary>
/// Privacy level and social exposure of location
/// Orthogonal categorical dimension for entity resolution
/// Determines who can witness events and social stakes of actions
/// </summary>
public enum LocationPrivacy
{
    /// <summary>
    /// Open to all, many witnesses, public scrutiny
    /// Actions observed by crowd, high social stakes
    /// Example: Market square, main street, public forum, town hall
    /// </summary>
    Public,

    /// <summary>
    /// Restricted but not private - common shared spaces
    /// Some witnesses present, moderate social stakes
    /// Example: Inn common room, guild hall, meeting chamber, tavern
    /// </summary>
    SemiPublic,

    /// <summary>
    /// Enclosed, intimate, isolated from observation
    /// Few or no witnesses, low social stakes, personal space
    /// Example: Private room, secluded grove, locked chamber, hidden alcove
    /// </summary>
    Private
}
