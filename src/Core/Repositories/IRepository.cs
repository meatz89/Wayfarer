using System.Collections.Generic;

namespace Wayfarer.Core.Repositories
{
    /// <summary>
    /// Base repository interface for common CRUD operations
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Get an entity by its unique identifier
        /// </summary>
        T GetById(string id);

        /// <summary>
        /// Get all entities
        /// </summary>
        IEnumerable<T> GetAll();

        /// <summary>
        /// Add a new entity
        /// </summary>
        void Add(T entity);

        /// <summary>
        /// Update an existing entity
        /// </summary>
        void Update(T entity);

        /// <summary>
        /// Remove an entity by its identifier
        /// </summary>
        /// <returns>True if removed, false if not found</returns>
        bool Remove(string id);

        /// <summary>
        /// Check if an entity exists by its identifier
        /// </summary>
        bool Exists(string id);

        /// <summary>
        /// Get the count of all entities
        /// </summary>
        int Count();
    }
}