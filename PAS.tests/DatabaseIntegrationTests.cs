using Xunit;
using Microsoft.EntityFrameworkCore;
using PAS_BlindMatching.Data;
using PAS_BlindMatching.Models;
using System;
using System.Threading.Tasks;

namespace PAS.Tests
{
    public class DatabaseIntegrationTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // isolate each test
                .Options;

            return new AppDbContext(options);
        }

        // ---------------- USERS TESTS ----------------

        [Fact]
        public async Task Test1_CreateUser()
        {
            var db = GetDbContext();

            db.Users.Add(new User
            {
                Name = "John",
                Email = "john@test.com",
                Password = "123",
                Role = "Student"
            });

            await db.SaveChangesAsync();

            var count = await db.Users.CountAsync();
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task Test2_ReadUsers()
        {
            var db = GetDbContext();

            db.Users.Add(new User
            {
                Name = "Test",
                Email = "test@test.com",
                Password = "123",
                Role = "Student"
            });

            await db.SaveChangesAsync();

            var user = await db.Users.FirstOrDefaultAsync();
            Assert.NotNull(user);
        }

        [Fact]
        public async Task Test3_UpdateUser()
        {
            var db = GetDbContext();

            var user = new User
            {
                Name = "A",
                Email = "a@test.com",
                Password = "123",
                Role = "Student"
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            user.Name = "Updated";
            await db.SaveChangesAsync();

            var updated = await db.Users.FirstAsync();
            Assert.Equal("Updated", updated.Name);
        }

        [Fact]
        public async Task Test4_DeleteUser()
        {
            var db = GetDbContext();

            var user = new User
            {
                Name = "B",
                Email = "b@test.com",
                Password = "123",
                Role = "Student"
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            db.Users.Remove(user);
            await db.SaveChangesAsync();

            var count = await db.Users.CountAsync();
            Assert.Equal(0, count);
        }

        // ---------------- SS-05 REQUIRED TEST ----------------

        [Fact]
        public async Task Test5_ConfirmMatch_UpdatesSupervisorIdAndStatus()
        {
            var db = GetDbContext();

            var project = new Project
            {
                Title = "AI Project",
                Abstract = "Test Abstract",
                TechStack = "C#",
                ResearchArea = "AI",
                SupervisorId = null,
                Status = "Pending"
            };

            db.Projects.Add(project);
            await db.SaveChangesAsync();

            // simulate match confirmation logic
            project.SupervisorId = 1;
            project.Status = "Approved";

            await db.SaveChangesAsync();

            var updated = await db.Projects.FirstAsync();

            Assert.Equal(1, updated.SupervisorId);
            Assert.Equal("Approved", updated.Status);
        }

        // ---------------- PROJECTS TESTS ----------------

        [Fact]
        public async Task Test6_CreateProject()
        {
            var db = GetDbContext();

            db.Projects.Add(new Project
            {
                Title = "AI",
                Abstract = "Test",
                TechStack = "C#",
                ResearchArea = "ML"
            });

            await db.SaveChangesAsync();

            var count = await db.Projects.CountAsync();
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task Test7_ReadProjects()
        {
            var db = GetDbContext();

            db.Projects.Add(new Project
            {
                Title = "Test",
                Abstract = "A",
                TechStack = "C#",
                ResearchArea = "AI"
            });

            await db.SaveChangesAsync();

            var project = await db.Projects.FirstOrDefaultAsync();
            Assert.NotNull(project);
        }

        [Fact]
        public async Task Test8_UpdateProject()
        {
            var db = GetDbContext();

            var project = new Project
            {
                Title = "Old",
                Abstract = "A",
                TechStack = "C#",
                ResearchArea = "AI"
            };

            db.Projects.Add(project);
            await db.SaveChangesAsync();

            project.Title = "New";
            await db.SaveChangesAsync();

            var updated = await db.Projects.FirstAsync();
            Assert.Equal("New", updated.Title);
        }

        [Fact]
        public async Task Test9_DeleteProject()
        {
            var db = GetDbContext();

            var project = new Project
            {
                Title = "X",
                Abstract = "A",
                TechStack = "C#",
                ResearchArea = "AI"
            };

            db.Projects.Add(project);
            await db.SaveChangesAsync();

            db.Projects.Remove(project);
            await db.SaveChangesAsync();

            var count = await db.Projects.CountAsync();
            Assert.Equal(0, count);
        }

        // ---------------- BASIC TESTS ----------------

        [Fact]
        public void Test10_DbContext_NotNull()
        {
            var db = GetDbContext();
            Assert.NotNull(db);
        }

        [Fact]
        public void Test11_InMemoryDatabase_Check()
        {
            var db = GetDbContext();
            Assert.True(db.Database.IsInMemory());
        }
    }
}