using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Wayfarer.Core.Repositories;
using Wayfarer.Core.Repositories.Implementation;

namespace Wayfarer.Core.Repositories.Tests
{
    [TestFixture]
    public class BaseRepositoryTests
    {
        private Mock<IWorldStateAccessor> _mockWorldState;
        private Mock<ILogger<TestRepository>> _mockLogger;
        private TestRepository _repository;
        private List<TestEntity> _testCollection;

        private class TestEntity
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        private class TestRepository : BaseRepository<TestEntity>
        {
            private readonly List<TestEntity> _collection;

            public TestRepository(IWorldStateAccessor worldState, ILogger<TestRepository> logger, List<TestEntity> collection) 
                : base(worldState, logger)
            {
                _collection = collection;
            }

            protected override List<TestEntity> GetCollection() => _collection;
            protected override string GetEntityId(TestEntity entity) => entity?.Id;
        }

        [SetUp]
        public void SetUp()
        {
            _mockWorldState = new Mock<IWorldStateAccessor>();
            _mockLogger = new Mock<ILogger<TestRepository>>();
            _testCollection = new List<TestEntity>();
            _repository = new TestRepository(_mockWorldState.Object, _mockLogger.Object, _testCollection);
        }

        [Test]
        public void Add_ValidEntity_AddsToCollection()
        {
            // Arrange
            var entity = new TestEntity { Id = "test1", Name = "Test Entity" };

            // Act
            _repository.Add(entity);

            // Assert
            Assert.That(_testCollection.Count, Is.EqualTo(1));
            Assert.That(_testCollection[0].Id, Is.EqualTo("test1"));
        }

        [Test]
        public void Add_DuplicateId_ThrowsException()
        {
            // Arrange
            var entity1 = new TestEntity { Id = "test1", Name = "Test Entity 1" };
            var entity2 = new TestEntity { Id = "test1", Name = "Test Entity 2" };
            _repository.Add(entity1);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _repository.Add(entity2));
        }

        [Test]
        public void Add_NullEntity_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _repository.Add(null));
        }

        [Test]
        public void GetById_ExistingId_ReturnsEntity()
        {
            // Arrange
            var entity = new TestEntity { Id = "test1", Name = "Test Entity" };
            _testCollection.Add(entity);

            // Act
            var result = _repository.GetById("test1");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Test Entity"));
        }

        [Test]
        public void GetById_NonExistingId_ReturnsNull()
        {
            // Act
            var result = _repository.GetById("non_existent");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Update_ExistingEntity_UpdatesInCollection()
        {
            // Arrange
            var entity = new TestEntity { Id = "test1", Name = "Original Name" };
            _testCollection.Add(entity);
            var updatedEntity = new TestEntity { Id = "test1", Name = "Updated Name" };

            // Act
            _repository.Update(updatedEntity);

            // Assert
            Assert.That(_testCollection[0].Name, Is.EqualTo("Updated Name"));
        }

        [Test]
        public void Update_NonExistingEntity_ThrowsException()
        {
            // Arrange
            var entity = new TestEntity { Id = "test1", Name = "Test Entity" };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _repository.Update(entity));
        }

        [Test]
        public void Remove_ExistingId_RemovesFromCollection()
        {
            // Arrange
            var entity = new TestEntity { Id = "test1", Name = "Test Entity" };
            _testCollection.Add(entity);

            // Act
            var result = _repository.Remove("test1");

            // Assert
            Assert.That(result, Is.True);
            Assert.That(_testCollection.Count, Is.EqualTo(0));
        }

        [Test]
        public void Remove_NonExistingId_ReturnsFalse()
        {
            // Act
            var result = _repository.Remove("non_existent");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void Exists_ExistingId_ReturnsTrue()
        {
            // Arrange
            var entity = new TestEntity { Id = "test1", Name = "Test Entity" };
            _testCollection.Add(entity);

            // Act
            var result = _repository.Exists("test1");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Count_ReturnsCorrectCount()
        {
            // Arrange
            _testCollection.Add(new TestEntity { Id = "test1", Name = "Entity 1" });
            _testCollection.Add(new TestEntity { Id = "test2", Name = "Entity 2" });

            // Act
            var result = _repository.Count();

            // Assert
            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public void GetAll_ReturnsAllEntities()
        {
            // Arrange
            _testCollection.Add(new TestEntity { Id = "test1", Name = "Entity 1" });
            _testCollection.Add(new TestEntity { Id = "test2", Name = "Entity 2" });

            // Act
            var result = _repository.GetAll().ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Any(e => e.Id == "test1"), Is.True);
            Assert.That(result.Any(e => e.Id == "test2"), Is.True);
        }
    }
}