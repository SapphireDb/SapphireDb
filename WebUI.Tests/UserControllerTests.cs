using NUnit.Framework;
using Moq;
using WebUI.Controllers;
using Microsoft.EntityFrameworkCore;
using WebUI.Data;
using SapphireDb;
using System.Collections.Generic;
using WebUI.Data.Models;
using System.Linq;

namespace WebUI.Tests
{
    internal class UserControllerTests
    {
        private Mock<ISapphireDatabaseNotifier> _sapphireDatabaseNotifierMock;
        private RealtimeContext _testRealtimeContext;
        private UserController _userController;

        [SetUp]
        public void Setup()
        {
            _sapphireDatabaseNotifierMock = new Mock<ISapphireDatabaseNotifier>();
            
            var dbName = "RealtimeDb";
            
            var realtimeOptions = new DbContextOptionsBuilder<RealtimeContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            var realtimeContext = new RealtimeContext(realtimeOptions, _sapphireDatabaseNotifierMock.Object);
            _userController = new UserController(realtimeContext);

            var testOptions = new DbContextOptionsBuilder<RealtimeContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            _testRealtimeContext = new RealtimeContext(testOptions, _sapphireDatabaseNotifierMock.Object);
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
            Assert.IsNotNull(usersFromController);
            Assert.AreEqual(4, usersFromController.Count);
            
            Assert.AreEqual(1, usersFromController[0].Id);
            Assert.AreEqual("f1", usersFromController[0].FirstName);
            Assert.AreEqual("l1", usersFromController[0].LastName);
            Assert.AreEqual("u1", usersFromController[0].Username);

            Assert.AreEqual(2, usersFromController[1].Id);
            Assert.AreEqual("f2", usersFromController[1].FirstName);
            Assert.AreEqual("l2", usersFromController[1].LastName);
            Assert.AreEqual("u2", usersFromController[1].Username);

            Assert.AreEqual(3, usersFromController[2].Id);
            Assert.AreEqual("f3", usersFromController[2].FirstName);
            Assert.AreEqual("l3", usersFromController[2].LastName);
            Assert.AreEqual("u3", usersFromController[2].Username);

            Assert.AreEqual(4, usersFromController[3].Id);
            Assert.AreEqual("f4", usersFromController[3].FirstName);
            Assert.AreEqual("l4", usersFromController[3].LastName);
            Assert.AreEqual("u4", usersFromController[3].Username);
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
            Assert.IsNotNull(userFromDb);
            Assert.AreEqual(newUser.Id, userFromDb.Id);
            Assert.AreEqual(newUser.FirstName, userFromDb.FirstName);
            Assert.AreEqual(newUser.LastName, userFromDb.LastName);
            Assert.AreEqual(newUser.Username, userFromDb.Username);

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
            Assert.IsNotNull(existingUserFromDb);
            Assert.AreEqual(newUser.Id, existingUserFromDb.Id);
            Assert.AreEqual(existingUser.FirstName, existingUserFromDb.FirstName);
            Assert.AreEqual(newUser.LastName, existingUserFromDb.LastName);
            Assert.AreEqual(newUser.Username, existingUserFromDb.Username);
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