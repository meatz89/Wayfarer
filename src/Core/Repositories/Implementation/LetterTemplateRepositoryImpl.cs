using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Wayfarer.Core.Repositories.Implementation
{
    /// <summary>
    /// Concrete implementation of ILetterTemplateRepository
    /// </summary>
    public class LetterTemplateRepositoryImpl : BaseRepository<LetterTemplate>, ILetterTemplateRepository
    {
        private readonly Random _random = new Random();

        public LetterTemplateRepositoryImpl(IWorldStateAccessor worldState, ILogger<LetterTemplateRepositoryImpl> logger) 
            : base(worldState, logger)
        {
        }

        protected override List<LetterTemplate> GetCollection()
        {
            return _worldState.LetterTemplates;
        }

        protected override string GetEntityId(LetterTemplate entity)
        {
            return entity?.Id;
        }

        protected override string EntityTypeName => "LetterTemplate";

        public IEnumerable<LetterTemplate> GetTemplatesByTokenType(ConnectionType tokenType)
        {
            return GetAll().Where(t => t.TokenType == tokenType);
        }

        public LetterTemplate GetRandomTemplate()
        {
            var templates = GetAll().ToList();
            if (!templates.Any())
            {
                return null;
            }

            return templates[_random.Next(templates.Count)];
        }

        public LetterTemplate GetRandomTemplateByTokenType(ConnectionType tokenType)
        {
            var templates = GetTemplatesByTokenType(tokenType).ToList();
            if (!templates.Any())
            {
                return null;
            }

            return templates[_random.Next(templates.Count)];
        }

        public IEnumerable<LetterTemplate> GetForcedShadowTemplates()
        {
            return GetAll().Where(t => t.Id.StartsWith("forced_shadow_") && t.TokenType == ConnectionType.Shadow);
        }

        public IEnumerable<LetterTemplate> GetForcedPatronTemplates()
        {
            return GetAll().Where(t => t.Id.StartsWith("forced_patron_") && t.TokenType == ConnectionType.Noble);
        }

        public LetterTemplate GetRandomForcedShadowTemplate()
        {
            var templates = GetForcedShadowTemplates().ToList();
            if (!templates.Any())
            {
                return null;
            }

            return templates[_random.Next(templates.Count)];
        }

        public LetterTemplate GetRandomForcedPatronTemplate()
        {
            var templates = GetForcedPatronTemplates().ToList();
            if (!templates.Any())
            {
                return null;
            }

            return templates[_random.Next(templates.Count)];
        }
    }
}