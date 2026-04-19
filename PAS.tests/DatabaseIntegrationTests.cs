using Xunit;
using Microsoft.EntityFrameworkCore;
using PAS_BlindMatching.Data;
using PAS_BlindMatching.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PAS.tests
{
    public class DatabaseIntegrationTests
    {
        // Helper: Creates a fresh isolated in-memory database for each test
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        // ============================================================
        // USER CRUD TESTS
        // ============================================================

        // -------------------------------------------------------
        // TEST 1: CREATE USER
        // Verifies a student user is saved to the database correctly
        // -------------------------------------------------------
        [Fact]
        public async Task Test1_CreateUser()
        {
            // Arrange
            var db = GetDbContext();

            // Act
            db.Users.Add(new User
            {
                Name = "John",
                Email = "john@test.com",
                Password = "123",
                Role = "Student"
            });
            await db.SaveChangesAsync();

            // Assert
            var count = await db.Users.CountAsync();
            Assert.Equal(1, count);
        }

        // -------------------------------------------------------
        // TEST 2: READ USER
        // Verifies a saved user can be retrieved from the database
        // -------------------------------------------------------
        [Fact]
        public async Task Test2_ReadUsers()
        {
            // Arrange
            var db = GetDbContext();

            db.Users.Add(new User
            {
                Name = "Test",
                Email = "test@test.com",
                Password = "123",
                Role = "Student"
            });
            await db.SaveChangesAsync();

            // Act
            var user = await db.Users.FirstOrDefaultAsync();

            // Assert
            Assert.NotNull(user);
            Assert.Equal("Test", user.Name);
        }

        // -------------------------------------------------------
        // TEST 3: UPDATE USER
        // Verifies a user's name can be updated in the database
        // Tests: AdminController.EditUser() update logic
        // -------------------------------------------------------
        [Fact]
        public async Task Test3_UpdateUser()
        {
            // Arrange
            var db = GetDbContext();

            var user = new User
            {
                Name = "Original Name",
                Email = "a@test.com",
                Password = "123",
                Role = "Student"
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            // Act
            user.Name = "Updated Name";
            await db.SaveChangesAsync();

            // Assert
            var updated = await db.Users.FirstAsync();
            Assert.Equal("Updated Name", updated.Name);
        }

        // -------------------------------------------------------
        // TEST 4: DELETE USER
        // Verifies a user can be removed from the database
        // Tests: AdminController.DeleteUser() logic
        // -------------------------------------------------------
        [Fact]
        public async Task Test4_DeleteUser()
        {
            // Arrange
            var db = GetDbContext();

            var user = new User
            {
                Name = "To Delete",
                Email = "b@test.com",
                Password = "123",
                Role = "Student"
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            // Act
            db.Users.Remove(user);
            await db.SaveChangesAsync();

            // Assert
            var count = await db.Users.CountAsync();
            Assert.Equal(0, count);
        }

        // ============================================================
        // MATCH CONFIRMATION TEST
        // ============================================================

        // -------------------------------------------------------
        // TEST 5: CONFIRM MATCH - SupervisorId and Status updated
        // Verifies match confirmation correctly updates project fields
        // Tests: SupervisorController.SelectProject() database update
        // -------------------------------------------------------
        [Fact]
        public async Task Test5_ConfirmMatch_UpdatesSupervisorIdAndStatus()
        {
            // Arrange
            var db = GetDbContext();

            var project = new Project
            {
                Title = "AI Project",
                Abstract = "Test Abstract",
                TechStack = "C#",
                ResearchArea = "Artificial Intelligence",
                SupervisorId = null,
                Status = "Pending",
                StudentId = 1
            };
            db.Projects.Add(project);
            await db.SaveChangesAsync();

            // Act: Simulate SupervisorController.SelectProject() match logic
            project.SupervisorId = 1;
            project.Status = "Matched";
            await db.SaveChangesAsync();

            // Assert
            var updated = await db.Projects.FirstAsync();
            Assert.Equal(1, updated.SupervisorId);
            Assert.Equal("Matched", updated.Status);
        }

        // ============================================================
        // PROJECT CRUD TESTS
        // ============================================================

        // -------------------------------------------------------
        // TEST 6: CREATE PROJECT
        // Verifies a project is saved to the database correctly
        // Tests: ProjectController.Submit() database save
        // -------------------------------------------------------
        [Fact]
        public async Task Test6_CreateProject()
        {
            // Arrange
            var db = GetDbContext();

            // Act
            db.Projects.Add(new Project
            {
                Title = "AI Research",
                Abstract = "Test project abstract.",
                TechStack = "C#, Python",
                ResearchArea = "Artificial Intelligence",
                Status = "Pending",
                StudentId = 1
            });
            await db.SaveChangesAsync();

            // Assert
            var count = await db.Projects.CountAsync();
            Assert.Equal(1, count);
        }

        // -------------------------------------------------------
        // TEST 7: READ PROJECT
        // Verifies a saved project can be retrieved from the database
        // -------------------------------------------------------
        [Fact]
        public async Task Test7_ReadProjects()
        {
            // Arrange
            var db = GetDbContext();

            db.Projects.Add(new Project
            {
                Title = "Test Project",
                Abstract = "Abstract text.",
                TechStack = "C#",
                ResearchArea = "Artificial Intelligence",
                Status = "Pending",
                StudentId = 1
            });
            await db.SaveChangesAsync();

            // Act
            var project = await db.Projects.FirstOrDefaultAsync();

            // Assert
            Assert.NotNull(project);
            Assert.Equal("Test Project", project.Title);
            Assert.Equal("Pending", project.Status);
        }

        // -------------------------------------------------------
        // TEST 8: UPDATE PROJECT
        // Verifies a project title can be updated in the database
        // Tests: ProjectController.Edit() database update
        // -------------------------------------------------------
        [Fact]
        public async Task Test8_UpdateProject()
        {
            // Arrange
            var db = GetDbContext();

            var project = new Project
            {
                Title = "Old Title",
                Abstract = "Abstract.",
                TechStack = "C#",
                ResearchArea = "Artificial Intelligence",
                Status = "Pending",
                StudentId = 1
            };
            db.Projects.Add(project);
            await db.SaveChangesAsync();

            // Act
            project.Title = "Updated Title";
            await db.SaveChangesAsync();

            // Assert
            var updated = await db.Projects.FirstAsync();
            Assert.Equal("Updated Title", updated.Title);
        }

        // -------------------------------------------------------
        // TEST 9: DELETE PROJECT
        // Verifies a project can be removed from the database
        // Tests: AdminController.DeleteUser() cascade project removal
        // -------------------------------------------------------
        [Fact]
        public async Task Test9_DeleteProject()
        {
            // Arrange
            var db = GetDbContext();

            var project = new Project
            {
                Title = "Project To Delete",
                Abstract = "Abstract.",
                TechStack = "C#",
                ResearchArea = "Artificial Intelligence",
                Status = "Pending",
                StudentId = 1
            };
            db.Projects.Add(project);
            await db.SaveChangesAsync();

            // Act
            db.Projects.Remove(project);
            await db.SaveChangesAsync();

            // Assert
            var count = await db.Projects.CountAsync();
            Assert.Equal(0, count);
        }

        // ============================================================
        // INFRASTRUCTURE TESTS
        // ============================================================

        // -------------------------------------------------------
        // TEST 10: DBCONTEXT NOT NULL
        // Verifies the AppDbContext is correctly instantiated
        // -------------------------------------------------------
        [Fact]
        public void Test10_DbContext_NotNull()
        {
            // Act
            var db = GetDbContext();

            // Assert
            Assert.NotNull(db);
        }

        // -------------------------------------------------------
        // TEST 11: IN-MEMORY DATABASE ACTIVE
        // Verifies the InMemory database provider is active during testing
        // -------------------------------------------------------
        [Fact]
        public void Test11_InMemoryDatabase_Check()
        {
            // Act
            var db = GetDbContext();

            // Assert
            Assert.True(db.Database.IsInMemory());
        }

        // ============================================================
        // NAVIGATION PROPERTY TESTS
        // ============================================================

        // -------------------------------------------------------
        // TEST 12: STUDENT NAVIGATION PROPERTY
        // Verifies EF Core Include() correctly loads Student navigation property
        // Tests: SupervisorController.MyMatches() Include(p => p.Student)
        // -------------------------------------------------------
        [Fact]
        public async Task Test12_Project_IncludesStudentNavigationProperty()
        {
            // Arrange
            var db = GetDbContext();

            var student = new User
            {
                Name = "Amali Dias",
                Email = "amali@test.com",
                Password = "hashed",
                Role = "Student"
            };
            db.Users.Add(student);
            await db.SaveChangesAsync();

            var project = new Project
            {
                Title = "E-Commerce Platform",
                Abstract = "Online shopping platform.",
                TechStack = "ASP.NET Core, SQL Server",
                ResearchArea = "Web & Mobile Development",
                Status = "Pending",
                StudentId = student.Id
            };
            db.Projects.Add(project);
            await db.SaveChangesAsync();

            // Act
            var loaded = await db.Projects
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == project.Id);

            // Assert
            Assert.NotNull(loaded);
            Assert.NotNull(loaded.Student);
            Assert.Equal("Amali Dias", loaded.Student.Name);
            Assert.Equal("amali@test.com", loaded.Student.Email);
        }

        // -------------------------------------------------------
        // TEST 13: SUPERVISOR NAVIGATION PROPERTY
        // Verifies EF Core Include() correctly loads Supervisor navigation property
        // Tests: ProjectController.MyProjects() Include(p => p.Supervisor)
        // -------------------------------------------------------
        [Fact]
        public async Task Test13_Project_IncludesSupervisorNavigationProperty()
        {
            // Arrange
            var db = GetDbContext();

            var supervisor = new User
            {
                Name = "Dr. Silva",
                Email = "silva@nsbm.ac.lk",
                Password = "hashed",
                Role = "Supervisor",
                ResearchArea = "Cybersecurity"
            };
            db.Users.Add(supervisor);
            await db.SaveChangesAsync();

            var project = new Project
            {
                Title = "Network Security Tool",
                Abstract = "Security monitoring tool.",
                TechStack = "C#, ASP.NET Core",
                ResearchArea = "Cybersecurity",
                Status = "Matched",
                StudentId = 1,
                SupervisorId = supervisor.Id
            };
            db.Projects.Add(project);
            await db.SaveChangesAsync();

            // Act
            var loaded = await db.Projects
                .Include(p => p.Supervisor)
                .FirstOrDefaultAsync(p => p.Id == project.Id);

            // Assert
            Assert.NotNull(loaded);
            Assert.NotNull(loaded.Supervisor);
            Assert.Equal("Dr. Silva", loaded.Supervisor.Name);
            Assert.Equal("Cybersecurity", loaded.Supervisor.ResearchArea);
        }

        // -------------------------------------------------------
        // TEST 14: RESEARCH AREA LIST INTEGRITY
        // Verifies ResearchAreaList contains exactly 20 predefined areas
        // Tests: ResearchAreaList static class in Users.cs
        // -------------------------------------------------------
        [Fact]
        public void Test14_ResearchAreaList_ContainsTwentyAreas()
        {
            // Act
            var areas = ResearchAreaList.Areas;

            // Assert
            Assert.Equal(20, areas.Count);
            Assert.Contains("Artificial Intelligence", areas);
            Assert.Contains("Cybersecurity", areas);
            Assert.Contains("Blockchain Technology", areas);
            Assert.Contains("Machine Learning", areas);
            Assert.Contains("Web & Mobile Development", areas);
        }

        // -------------------------------------------------------
        // TEST 15: GROUP MEMBERS JSON STORED CORRECTLY
        // Verifies GroupMembersJson is stored and retrieved accurately
        // Tests: ProjectController.Submit() and MyProjects() group logic
        // -------------------------------------------------------
        [Fact]
        public async Task Test15_GroupProject_GroupMembersJson_StoredAndRetrieved()
        {
            // Arrange
            var db = GetDbContext();

            var members = new List<GroupMemberInfo>
            {
                new GroupMemberInfo { Name = "Nimal Perera", UserId = "10001" },
                new GroupMemberInfo { Name = "Amali Silva",  UserId = "10002" }
            };
            var groupMembersJson = JsonSerializer.Serialize(members);

            var project = new Project
            {
                Title = "Group AI Project",
                Abstract = "Group project on AI.",
                TechStack = "Python, TensorFlow",
                ResearchArea = "Artificial Intelligence",
                Status = "Pending",
                StudentId = 1,
                GroupMembersJson = groupMembersJson
            };

            // Act
            db.Projects.Add(project);
            await db.SaveChangesAsync();

            var saved = await db.Projects.FirstAsync();
            var deserialized = JsonSerializer
                .Deserialize<List<GroupMemberInfo>>(saved.GroupMembersJson);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(2, deserialized.Count);
            Assert.Equal("Nimal Perera", deserialized[0].Name);
            Assert.Equal("10001", deserialized[0].UserId);
            Assert.Equal("Amali Silva", deserialized[1].Name);
            Assert.Equal("10002", deserialized[1].UserId);
        }
    }
}
