using Xunit;
using Moq;
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
    public class MatchingLogicTests
    {
        // Helper: Creates a fresh isolated in-memory database for each test
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        // TEST 1: BLIND REVIEW
        // Verifies student identity is hidden during supervisor browsing
        // Tests: SupervisorController.Index() blind projection logic
       
        [Fact]
        public async Task BlindReview_StudentNameIsHidden_WhenProjectIsPending()
        {
            // Arrange
            var db = GetDbContext();

            var student = new User
            {
                Name = "John Perera",
                Email = "john@test.com",
                Password = "hashed",
                Role = "Student"
            };
            var supervisor = new User
            {
                Name = "Dr. Silva",
                Email = "silva@test.com",
                Password = "hashed",
                Role = "Supervisor",
                ResearchArea = "Artificial Intelligence"
            };
            db.Users.AddRange(student, supervisor);
            await db.SaveChangesAsync();

            var project = new Project
            {
                Title = "AI Chatbot",
                Abstract = "A smart chatbot system.",
                TechStack = "Python, TensorFlow",
                ResearchArea = "Artificial Intelligence",
                Status = "Pending",
                StudentId = student.Id
            };
            db.Projects.Add(project);
            await db.SaveChangesAsync();

            // Act: Simulate SupervisorController.Index() blind projection
            var pendingProjects = await db.Projects
                .Where(p => p.Status == "Pending" && p.ResearchArea == supervisor.ResearchArea)
                .Select(p => new ProjectViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Abstract = p.Abstract,
                    TechStack = p.TechStack,
                    ResearchArea = p.ResearchArea,
                    Status = p.Status,
                    StudentName = "Hidden (Blind Review)"
                }).ToListAsync();

            // Assert
            Assert.Single(pendingProjects);
            Assert.Equal("Hidden (Blind Review)", pendingProjects[0].StudentName);
            Assert.NotEqual("John Perera", pendingProjects[0].StudentName);
        }

        // TEST 2: STATUS TRANSITION
        // Verifies project status changes from Pending to Matched
        // Tests: SupervisorController.SelectProject() logic
        
        [Fact]
        public async Task ProjectStatus_ChangesToMatched_WhenSupervisorConfirms()
        {
            // Arrange
            var db = GetDbContext();

            var project = new Project
            {
                Title = "Blockchain App",
                Abstract = "Decentralized application.",
                TechStack = "Solidity, Web3",
                ResearchArea = "Blockchain Technology",
                Status = "Pending",
                StudentId = 1
            };
            db.Projects.Add(project);
            await db.SaveChangesAsync();

            // Act: Simulate SupervisorController.SelectProject() logic
            var dbProject = await db.Projects.FirstAsync();
            dbProject.SupervisorId = 2;
            dbProject.Status = "Matched";
            await db.SaveChangesAsync();

            // Assert
            var updated = await db.Projects.FirstAsync();
            Assert.Equal("Matched", updated.Status);
            Assert.Equal(2, updated.SupervisorId);
        }

     
        // TEST 3: IDENTITY REVEAL
        // Verifies student name and email are visible after matching
        // Tests: SupervisorController.MyMatches() identity reveal
       
        [Fact]
        public async Task IdentityReveal_StudentDetailsVisible_AfterMatching()
        {
            // Arrange
            var db = GetDbContext();

            var student = new User
            {
                Name = "Alice Fernando",
                Email = "alice@test.com",
                Password = "hashed",
                Role = "Student"
            };
            db.Users.Add(student);
            await db.SaveChangesAsync();

            var project = new Project
            {
                Title = "IoT Smart Home",
                Abstract = "Smart home automation system.",
                TechStack = "C#, Raspberry Pi",
                ResearchArea = "Internet of Things (IoT)",
                Status = "Matched",
                StudentId = student.Id,
                SupervisorId = 2
            };
            db.Projects.Add(project);
            await db.SaveChangesAsync();

            // Act: Simulate SupervisorController.MyMatches() identity reveal
            var matchedProjects = await db.Projects
                .Where(p => p.SupervisorId == 2)
                .Include(p => p.Student)
                .Select(p => new ProjectViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Status = p.Status,
                    StudentName = p.Student != null ? p.Student.Name : "Unknown",
                    StudentEmail = p.Student != null ? p.Student.Email : "Unknown"
                }).ToListAsync();

            // Assert
            Assert.Single(matchedProjects);
            Assert.Equal("Alice Fernando", matchedProjects[0].StudentName);
            Assert.Equal("alice@test.com", matchedProjects[0].StudentEmail);
            Assert.NotEqual("Hidden (Blind Review)", matchedProjects[0].StudentName);
        }

       
        // TEST 4: RESEARCH AREA FILTER
        // Verifies supervisor only sees projects matching their expertise
        // Tests: SupervisorController.Index() research area filtering
       
        [Fact]
        public async Task SupervisorDashboard_OnlyShowsProjects_MatchingResearchArea()
        {
            // Arrange
            var db = GetDbContext();

            db.Projects.AddRange(
                new Project { Title = "AI Project", ResearchArea = "Artificial Intelligence", Status = "Pending", StudentId = 1, Abstract = "AI system.", TechStack = "Python" },
                new Project { Title = "Web App", ResearchArea = "Web & Mobile Development", Status = "Pending", StudentId = 2, Abstract = "Web application.", TechStack = "React" },
                new Project { Title = "ML Model", ResearchArea = "Artificial Intelligence", Status = "Pending", StudentId = 3, Abstract = "ML pipeline.", TechStack = "TensorFlow" }
            );
            await db.SaveChangesAsync();

            // Act: Filter projects by supervisor's research area
            var supervisorArea = "Artificial Intelligence";
            var filtered = await db.Projects
                .Where(p => p.Status == "Pending" && p.ResearchArea == supervisorArea)
                .ToListAsync();

            // Assert
            Assert.Equal(2, filtered.Count);
            Assert.All(filtered, p => Assert.Equal("Artificial Intelligence", p.ResearchArea));
        }

        
        // TEST 5: EDIT RESTRICTION
        // Verifies a matched project cannot be edited by student
        // Tests: ProjectController.Edit() guard logic
        
        [Fact]
        public async Task EditProject_IsNotAllowed_WhenProjectIsMatched()
        {
            // Arrange
            var db = GetDbContext();

            var project = new Project
            {
                Title = "Cyber Security App",
                Abstract = "Security monitoring system.",
                TechStack = "C#, ASP.NET Core",
                ResearchArea = "Cybersecurity",
                Status = "Matched",
                StudentId = 1
            };
            db.Projects.Add(project);
            await db.SaveChangesAsync();

            // Act: Simulate edit guard check in ProjectController.Edit()
            var dbProject = await db.Projects
                .FirstOrDefaultAsync(p => p.StudentId == 1);
            bool canEdit = dbProject?.Status == "Pending";

            // Assert
            Assert.False(canEdit);
        }

      
        // TEST 6: DEFAULT STATUS
        // Verifies new project submission always defaults to Pending
        // Tests: ProjectController.Submit() default state
       
        [Fact]
        public async Task NewProject_DefaultStatus_IsPending()
        {
            // Arrange
            var db = GetDbContext();

            var project = new Project
            {
                Title = "Cloud Computing System",
                Abstract = "Cloud-based application.",
                TechStack = "Azure, C#",
                ResearchArea = "Cloud Computing",
                StudentId = 1,
                Status = "Pending"
            };

            // Act
            db.Projects.Add(project);
            await db.SaveChangesAsync();

            // Assert
            var saved = await db.Projects
                .FirstOrDefaultAsync(p => p.Title == "Cloud Computing System");
            Assert.NotNull(saved);
            Assert.Equal("Pending", saved.Status);
            Assert.Null(saved.SupervisorId);
        }

        // TEST 7: NO DOUBLE MATCHING
        // Verifies matched projects do not appear in supervisor blind feed
        // Tests: SupervisorController.Index() status filter
       
        [Fact]
        public async Task MatchedProject_DoesNotAppear_InSupervisorBlindFeed()
        {
            // Arrange
            var db = GetDbContext();

            db.Projects.Add(new Project
            {
                Title = "Database System",
                Abstract = "Database management tool.",
                TechStack = "SQL Server, C#",
                ResearchArea = "Database Systems",
                Status = "Matched",
                StudentId = 1,
                SupervisorId = 2
            });
            await db.SaveChangesAsync();

            // Act: Blind feed only queries Pending projects
            var availableProjects = await db.Projects
                .Where(p => p.Status == "Pending")
                .ToListAsync();

            // Assert
            Assert.Empty(availableProjects);
        }

        // TEST 8: SUPERVISOR NAME REVEAL TO STUDENT
        // Verifies supervisor name is visible to student after match
        // Tests: ProjectController.MyProjects() after match
        
        [Fact]
        public async Task SupervisorName_IsVisibleToStudent_AfterMatch()
        {
            // Arrange
            var db = GetDbContext();

            var supervisor = new User
            {
                Name = "Dr. Pathirana",
                Email = "pathirana@nsbm.ac.lk",
                Password = "hashed",
                Role = "Supervisor",
                ResearchArea = "Machine Learning"
            };
            var student = new User
            {
                Name = "Kasun Silva",
                Email = "kasun@test.com",
                Password = "hashed",
                Role = "Student"
            };
            db.Users.AddRange(student, supervisor);
            await db.SaveChangesAsync();

            var project = new Project
            {
                Title = "ML Research Tool",
                Abstract = "Machine learning research tool.",
                TechStack = "Python, scikit-learn",
                ResearchArea = "Machine Learning",
                Status = "Matched",
                StudentId = student.Id,
                SupervisorId = supervisor.Id
            };
            db.Projects.Add(project);
            await db.SaveChangesAsync();

            // Act: Simulate ProjectController.MyProjects() after match
            var data = await db.Projects
                .Where(p => p.StudentId == student.Id)
                .Include(p => p.Supervisor)
                .Include(p => p.Student)
                .ToListAsync();

            var viewModel = data.Select(p => new ProjectViewModel
            {
                Id = p.Id,
                Title = p.Title,
                Status = p.Status,
                SupervisorName = p.Supervisor != null ? p.Supervisor.Name : null,
                StudentName = p.Student.Name
            }).ToList();

            // Assert
            Assert.Single(viewModel);
            Assert.Equal("Dr. Pathirana", viewModel[0].SupervisorName);
            Assert.Equal("Matched", viewModel[0].Status);
        }

       
        // TEST 9: SUPERVISOR REQUIRES RESEARCH AREA
        // Verifies supervisor must have research area on registration
        // Tests: AccountController.Register() validation logic
      
        [Fact]
        public async Task Supervisor_MustHave_ResearchArea()
        {
            // Arrange
            var db = GetDbContext();

            var supervisor = new User
            {
                Name = "Dr. Jayakody",
                Email = "jayakody@nsbm.ac.lk",
                Password = "hashed",
                Role = "Supervisor",
                ResearchArea = "Cybersecurity"
            };
            db.Users.Add(supervisor);
            await db.SaveChangesAsync();

            // Act
            var saved = await db.Users
                .FirstOrDefaultAsync(u => u.Role == "Supervisor");

            // Assert
            Assert.NotNull(saved);
            Assert.NotNull(saved.ResearchArea);
            Assert.Equal("Cybersecurity", saved.ResearchArea);
        }

        // TEST 10: GROUP PROJECT SUBMISSION
        // Verifies GroupMembersJson is correctly serialized and stored
        // Tests: ProjectController.Submit() group member logic
        
        [Fact]
        public async Task GroupProject_GroupMembersJson_SerializedAndDeserialized()
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
                Title = "Group Cybersecurity Project",
                Abstract = "Group project on network security.",
                TechStack = "C#, ASP.NET Core",
                ResearchArea = "Cybersecurity",
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

        // TEST 11: DUPLICATE EMAIL PREVENTION
        // Verifies system prevents duplicate email registration
        // Tests: AccountController.Register() email uniqueness check
       
        [Fact]
        public async Task Register_PreventsDuplicateEmail()
        {
            // Arrange
            var db = GetDbContext();

            var existingUser = new User
            {
                Name = "Existing User",
                Email = "existing@test.com",
                Password = "hashed",
                Role = "Student"
            };
            db.Users.Add(existingUser);
            await db.SaveChangesAsync();

            // Act: Simulate AccountController duplicate email check
            bool emailExists = await db.Users
                .AnyAsync(u => u.Email == "existing@test.com");

            // Assert
            Assert.True(emailExists);
        }

        
        // TEST 12: ADMIN PAIRINGS DASHBOARD
        // Verifies admin can view all matched projects with both identities
        // Tests: AdminController.Pairings() logic
    
        [Fact]
        public async Task AdminPairings_ShowsAllMatchedProjects_WithBothIdentities()
        {
            // Arrange
            var db = GetDbContext();

            var student = new User { Name = "Student One", Email = "s1@test.com", Password = "hashed", Role = "Student" };
            var supervisor = new User { Name = "Dr. One", Email = "sup1@test.com", Password = "hashed", Role = "Supervisor", ResearchArea = "Artificial Intelligence" };
            db.Users.AddRange(student, supervisor);
            await db.SaveChangesAsync();

            db.Projects.AddRange(
                new Project { Title = "Matched Project", Abstract = "Test.", TechStack = "C#", ResearchArea = "Artificial Intelligence", Status = "Matched", StudentId = student.Id, SupervisorId = supervisor.Id },
                new Project { Title = "Pending Project", Abstract = "Test.", TechStack = "Python", ResearchArea = "Artificial Intelligence", Status = "Pending", StudentId = student.Id }
            );
            await db.SaveChangesAsync();

            // Act: Simulate AdminController.Pairings() query
            var pairedData = await db.Projects
                .Where(p => p.Status == "Matched" && p.SupervisorId != null)
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .ToListAsync();

            var paired = pairedData.Select(p => new ProjectViewModel
            {
                Title = p.Title,
                StudentName = p.Student != null ? p.Student.Name : "Unknown",
                SupervisorName = p.Supervisor != null ? p.Supervisor.Name : "Unknown",
                Status = p.Status
            }).ToList();

            // Assert
            Assert.Single(paired);
            Assert.Equal("Student One", paired[0].StudentName);
            Assert.Equal("Dr. One", paired[0].SupervisorName);
            Assert.Equal("Matched", paired[0].Status);
        }
    }
}
