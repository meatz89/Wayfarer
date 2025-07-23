using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Wayfarer.Core.Repositories
{
    /// <summary>
    /// Base repository implementation providing common CRUD operations
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public abstract class BaseRepository<T> : IRepository<T> where T : class
    {
        protected readonly IWorldStateAccessor _worldState;
        protected readonly ILogger _logger;

        protected BaseRepository(IWorldStateAccessor worldState, ILogger logger)
        {
            _worldState = worldState ?? throw new ArgumentNullException(nameof(worldState));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get the collection of entities from the world state
        /// </summary>
        protected abstract List<T> GetCollection();

        /// <summary>
        /// Get the unique identifier for an entity
        /// </summary>
        protected abstract string GetEntityId(T entity);

        /// <summary>
        /// Get the entity type name for logging
        /// </summary>
        protected virtual string EntityTypeName => typeof(T).Name;

        public virtual T GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("ID cannot be null or empty", nameof(id));
            }

            var collection = GetCollection();
            var entity = collection?.FirstOrDefault(e => GetEntityId(e) == id);
            
            if (entity == null)
            {
                _logger.LogDebug($"{EntityTypeName} with ID '{id}' not found");
            }

            return entity;
        }

        public virtual IEnumerable<T> GetAll()
        {
            return GetCollection() ?? new List<T>();
        }

        public virtual void Add(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var collection = GetCollection();
            var entityId = GetEntityId(entity);

            if (string.IsNullOrWhiteSpace(entityId))
            {
                throw new InvalidOperationException($"{EntityTypeName} must have a valid ID");
            }

            if (collection.Any(e => GetEntityId(e) == entityId))
            {
                throw new InvalidOperationException($"{EntityTypeName} with ID '{entityId}' already exists");
            }

            collection.Add(entity);
            _logger.LogDebug($"Added {EntityTypeName} with ID '{entityId}'");
        }

        public virtual void Update(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var collection = GetCollection();
            var entityId = GetEntityId(entity);

            if (string.IsNullOrWhiteSpace(entityId))
            {
                throw new InvalidOperationException($"{EntityTypeName} must have a valid ID");
            }

            var existingEntity = collection.FirstOrDefault(e => GetEntityId(e) == entityId);
            if (existingEntity == null)
            {
                throw new InvalidOperationException($"{EntityTypeName} with ID '{entityId}' not found");
            }

            var index = collection.IndexOf(existingEntity);
            collection[index] = entity;
            _logger.LogDebug($"Updated {EntityTypeName} with ID '{entityId}'");
        }

        public virtual bool Remove(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("ID cannot be null or empty", nameof(id));
            }

            var collection = GetCollection();
            var entity = collection?.FirstOrDefault(e => GetEntityId(e) == id);
            
            if (entity != null)
            {
                var removed = collection.Remove(entity);
                if (removed)
                {
                    _logger.LogDebug($"Removed {EntityTypeName} with ID '{id}'");
                }
                return removed;
            }

            _logger.LogDebug($"{EntityTypeName} with ID '{id}' not found for removal");
            return false;
        }

        public virtual bool Exists(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            var collection = GetCollection();
            return collection?.Any(e => GetEntityId(e) == id) ?? false;
        }

        public virtual int Count()
        {
            return GetCollection()?.Count ?? 0;
        }

        /// <summary>
        /// Clear all entities from the collection
        /// </summary>
        protected virtual void ClearAll()
        {
            var collection = GetCollection();
            if (collection != null)
            {
                var count = collection.Count;
                collection.Clear();
                _logger.LogDebug($"Cleared all {count} {EntityTypeName} entities");
            }
        }
    }
}