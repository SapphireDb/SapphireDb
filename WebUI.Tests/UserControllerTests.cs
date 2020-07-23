using NUnit.Framework;
using Moq;
using WebUI.Controllers;
using Microsoft.EntityFrameworkCore;
using WebUI.Data;
using SapphireDb;
using System.Collections.Generic;
using WebUI.Data.Models;
using System.Linq;
using DeepEqual.Syntax;

namespace WebUI.Tests
{
    internal class UserControllerTests
    {
        private RealtimeContext _testRealtimeContext;
        private UserController _userController;

        [SetUp]
        public void Setup()
        {
            var dbName = "RealtimeDb";
            
            var realtimeOptions = new DbContextOptionsBuilder<RealtimeContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            var realtimeContext = new RealtimeContext(realtimeOptions);
            _userController = new UserController(realtimeContext);

            var testOptions = new DbContextOptionsBuilder<RealtimeContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            _testRealtimeContext = new RealtimeContext(testOptions);
        }

        [Test]
        public void Get_GivenValidData_ReturnsValidData()
        {
            // Arrange.
            IEnumerable<User> users = new List<User>
            {
                new User { Id = 1, FirstName = "f1", LastName = "l1", Username = "u1" },
                new User { Id = 2, FirstName = "f2", LastName = "l2", Username = "u2" },
                new User { Id = 3, FirstName = "f3", LastName = "l3", Username = "u3" },
                new User { Id = 4, FirstName = "f4", LastName = "l4", Username = "u4" }
            };
            _testRealtimeContext.Users.AddRange(users);
            _testRealtimeContext.SaveChanges();

            // Act.
            List<User> usersFromController = _userController.Get();

            // Assert.
            usersFromController.ShouldDeepEqual(users);
        }

        [Test]
        public void Post_CanAddData()
        {
            // Arrange.
            var newUser = new User { Id = 1, FirstName = "f1", LastName = "l1", Username = "u1" };

            // Act.
            _userController.Post(newUser);

            // Assert.
            User userFromDb = _testRealtimeContext.Users.FirstOrDefault(user => user.Id == newUser.Id);
            userFromDb.ShouldDeepEqual(newUser);

            Test testFromDb = _testRealtimeContext.Tests.FirstOrDefault(test => test.Content == newUser.Username);
            Assert.IsNotNull(testFromDb);
            Assert.AreEqual(newUser.Username, testFromDb.Content);
        }

        [Test]
        public void Put_CanUpdateData()
        {
            // Arrange.
            var newUser = new User { Id = 1, FirstName = "f1", LastName = "l1", Username = "u1" };
            _testRealtimeContext.Users.Add(newUser);
            _testRealtimeContext.SaveChanges();
            var existingUser = _testRealtimeContext.Users.FirstOrDefault(user => user.Id == newUser.Id);
            existingUser.FirstName = "changed first name";

            // Act.
            _userController.Put(existingUser);

            // Assert.
            var existingUserFromDb = _testRealtimeContext.Users.FirstOrDefault(user => user.Id == newUser.Id);
            existingUserFromDb.ShouldDeepEqual(newUser);
            existingUserFromDb.ShouldDeepEqual(existingUser);
        }

        [Test]
        public void Delete_CanRemoveData()
        {
            // Arrange.
            var newUser = new User { Id = 1, FirstName = "f1", LastName = "l1", Username = "u1" };
            _testRealtimeContext.Users.Add(newUser);
            _testRealtimeContext.SaveChanges();

            // Act.
            _userController.Delete(newUser);

            // Assert.
            var existingUserFromDb = _testRealtimeContext.Users.FirstOrDefault(user => user.Id == newUser.Id);
            Assert.IsNull(existingUserFromDb);
        }

        [TearDown]
        public void Cleanup()
        {
            _testRealtimeContext.Database.EnsureDeleted();
        }
    }
}