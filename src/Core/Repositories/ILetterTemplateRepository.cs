using System.Collections.Generic;

namespace Wayfarer.Core.Repositories
{
    /// <summary>
    /// Repository interface for LetterTemplate entities
    /// </summary>
    public interface ILetterTemplateRepository : IRepository<LetterTemplate>
    {
        /// <summary>
        /// Get templates by token type
        /// </summary>
        IEnumerable<LetterTemplate> GetTemplatesByTokenType(ConnectionType tokenType);

        /// <summary>
        /// Get a random template
        /// </summary>
        LetterTemplate GetRandomTemplate();

        /// <summary>
        /// Get a random template by token type
        /// </summary>
        LetterTemplate GetRandomTemplateByTokenType(ConnectionType tokenType);

        /// <summary>
        /// Get forced shadow templates
        /// </summary>
        IEnumerable<LetterTemplate> GetForcedShadowTemplates();

        /// <summary>
        /// Get forced patron templates
        /// </summary>
        IEnumerable<LetterTemplate> GetForcedPatronTemplates();

        /// <summary>
        /// Get a random forced shadow template
        /// </summary>
        LetterTemplate GetRandomForcedShadowTemplate();

        /// <summary>
        /// Get a random forced patron template
        /// </summary>
        LetterTemplate GetRandomForcedPatronTemplate();
    }
}