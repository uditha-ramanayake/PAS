# Functional Test Cases – PAS Blind Matching System

**Project:** Project Approval System (PAS)  
**Module:** Software Development Tools and Practices  
**Academic Year:** 2026 
**Testing Type:** Functional (User Journey / Black-Box Testing)  
**Application:** ASP.NET Core 8.0 – Tested on localhost  
**Test Executor:** Kasundi Ranaweera  
**Date Executed:** April 2026  

---

## Test Environment

| Item | Details |
|------|---------|
| Framework | ASP.NET Core 8.0 MVC |
| Database | SQL Server (local) |
| Browser | Google Chrome / Microsoft Edge |
| IDE | Visual Studio 2022 |
| Base URL | https://localhost:5000 |
| Test Type | Manual Black-Box Testing |

---

## Test Case Summary

| Test ID | Title | Result |
|---------|-------|--------|
| FTC-01 | Student Registration and Login | ✅ PASSED |
| FTC-02 | Student Project Submission | ✅ PASSED |
| FTC-03 | Blind Review — Supervisor Cannot See Student Identity | ✅ PASSED |
| FTC-04 | Supervisor Confirms Match — Identity Reveal | ✅ PASSED |
| FTC-05 | Student Views Match Status and Supervisor Name | ✅ PASSED |
| FTC-06 | Edit Restriction — Cannot Edit Matched Project | ✅ PASSED |
| FTC-07 | Admin Pairings Dashboard | ✅ PASSED |
| FTC-08 | Role-Based Access Control | ✅ PASSED |
| FTC-09 | Group Project Submission | ✅ PASSED |
| FTC-10 | Research Area Filter on Supervisor Dashboard | ✅ PASSED |

---

## FTC-01: Student Registration and Login

**Objective:** Verify that a new student user can successfully register an account and log in to the system.  
**Pre-condition:** No existing account with the test email address.  
**User Role:** Student  

| Step | Action | Expected Result | Actual Result | Status |
|------|--------|-----------------|---------------|--------|
| 1 | Navigate to `/Account/Register` | Registration form is displayed with Name, Email, Password, and Role fields | Form displayed correctly with all fields and role dropdown | ✅ Pass |
| 2 | Select Role = "Student" from dropdown | Research area field is hidden (only required for Supervisors) | Research area field not shown for Student role | ✅ Pass |
| 3 | Enter Name = "John Perera", Email = "john@test.com", Password = "Test@123" | Form accepts all inputs without errors | All fields accepted | ✅ Pass |
| 4 | Click "Register" button | Account is created, session is started, redirect to `/Project/Submit` | Successfully redirected to Submit page | ✅ Pass |
| 5 | Navigate to `/Account/Logout` | Session is cleared, redirect to Login page | Logged out successfully | ✅ Pass |
| 6 | Navigate to `/Account/Login` | Login form is displayed | Login form shown correctly | ✅ Pass |
| 7 | Enter registered email and password, click "Login" | Login successful, session Role = "Student" set, redirect to `/Project/Submit` | Redirected to Submit page correctly | ✅ Pass |
| 8 | Attempt login with wrong password | Error message "Invalid email or password." displayed | Error message displayed, no redirect | ✅ Pass |

**Result: PASSED**  
**Notes:** Session correctly stores Role, UserId, and UserName via HttpContext.Session after successful login.

---

## FTC-02: Student Project Submission

**Objective:** Verify that a logged-in student can submit a project proposal that is saved with Status = "Pending".  
**Pre-condition:** Student account exists and user is logged in.  
**User Role:** Student  

| Step | Action | Expected Result | Actual Result | Status |
|------|--------|-----------------|---------------|--------|
| 1 | Log in as Student | Student dashboard accessible, redirect to `/Project/Submit` | Redirected to Submit page | ✅ Pass |
| 2 | Navigate to `/Project/Submit` | Submission form shown with Title, Abstract, TechStack, ResearchArea fields | Form displayed correctly | ✅ Pass |
| 3 | Verify Research Area dropdown content | Dropdown contains all 20 predefined research areas from ResearchAreaList | All 20 areas listed correctly | ✅ Pass |
| 4 | Enter Title = "AI Smart Attendance", Abstract = "Face recognition system", TechStack = "Python, OpenCV", ResearchArea = "Artificial Intelligence" | Form accepts all inputs | All fields accepted | ✅ Pass |
| 5 | Click "Submit" button | Project saved to database with Status = "Pending" and StudentId = logged-in user | Project saved successfully | ✅ Pass |
| 6 | Navigate to `/Project/MyProjects` | Submitted project appears with Status = "Pending" and no supervisor assigned | Project listed with correct status | ✅ Pass |
| 7 | Verify SupervisorId is null | No supervisor name shown on the project card | Supervisor shows as "Not Assigned" | ✅ Pass |

**Result: PASSED**  
**Notes:** ProjectController.Submit() correctly assigns StudentId from session and defaults Status to "Pending".

---

## FTC-03: Blind Review — Supervisor Cannot See Student Identity

**Objective:** Verify that the supervisor's dashboard displays project proposals anonymously with student identity concealed as "Hidden (Blind Review)".  
**Pre-condition:** At least one Pending project exists in the database matching the supervisor's research area.  
**User Role:** Supervisor  

| Step | Action | Expected Result | Actual Result | Status |
|------|--------|-----------------|---------------|--------|
| 1 | Log in as Supervisor (ResearchArea = "Artificial Intelligence") | Supervisor dashboard accessible, redirect to `/Supervisor/Index` | Redirected to Supervisor Index | ✅ Pass |
| 2 | Navigate to `/Supervisor/Index` | List of Pending projects displayed filtered by supervisor's research area | Only Artificial Intelligence projects shown | ✅ Pass |
| 3 | Inspect each project card for student name | Student name field shows "Hidden (Blind Review)" | Confirmed — "Hidden (Blind Review)" displayed | ✅ Pass |
| 4 | Inspect each project card for student email | No student email visible anywhere on the page | Student email not present in page source | ✅ Pass |
| 5 | Verify project Title, Abstract, TechStack are visible | Technical details fully visible to supervisor | Title, Abstract, TechStack all displayed | ✅ Pass |
| 6 | Verify ResearchArea is visible | Research area label shown on each project card | ResearchArea shown correctly | ✅ Pass |
| 7 | Check that projects from other research areas are NOT shown | Only projects with ResearchArea = "Artificial Intelligence" visible | No other research area projects shown | ✅ Pass |
| 8 | Log in as a different supervisor with ResearchArea = "Cybersecurity" | Only Cybersecurity projects are shown | Correctly filtered by research area | ✅ Pass |

**Result: PASSED**  
**Notes:** SupervisorController.Index() correctly hides student identity at the data access layer through LINQ projection. StudentName is explicitly set to "Hidden (Blind Review)" in the ProjectViewModel — not at the view layer.

---

## FTC-04: Supervisor Confirms Match — Identity Reveal

**Objective:** Verify that when a supervisor confirms a match, the project status updates to "Matched" and both identities are revealed.  
**Pre-condition:** Supervisor is logged in. At least one Pending project is visible in the blind feed.  
**User Role:** Supervisor  

| Step | Action | Expected Result | Actual Result | Status |
|------|--------|-----------------|---------------|--------|
| 1 | Log in as Supervisor | Supervisor dashboard accessible | Logged in successfully | ✅ Pass |
| 2 | Navigate to `/Supervisor/Index` | Pending projects displayed anonymously | Projects shown with "Hidden (Blind Review)" | ✅ Pass |
| 3 | Click "Confirm Match" button on a project | HTTP POST sent to `/Supervisor/SelectProject/{id}` | POST request sent correctly | ✅ Pass |
| 4 | Verify project Status changes to "Matched" in database | Status = "Matched" stored in SQL Server | Status updated to "Matched" confirmed | ✅ Pass |
| 5 | Verify SupervisorId is assigned to the project | SupervisorId = logged-in supervisor's UserId | SupervisorId correctly assigned | ✅ Pass |
| 6 | Navigate to `/Supervisor/MyMatches` | Matched project appears with student name and email revealed | Student name and email now visible | ✅ Pass |
| 7 | Verify the matched project no longer appears in blind feed | Go back to `/Supervisor/Index` — matched project not shown | Confirmed — project removed from blind feed | ✅ Pass |

**Result: PASSED**  
**Notes:** SupervisorController.SelectProject() correctly updates both SupervisorId and Status in a single SaveChangesAsync() call. The MyMatches() action uses Include(p => p.Student) to eagerly load student details for the identity reveal.

---

## FTC-05: Student Views Match Status and Supervisor Name

**Objective:** Verify that after a match is confirmed, the student can see their project status updated to "Matched" and the assigned supervisor's name is visible.  
**Pre-condition:** A supervisor has confirmed a match on the student's project.  
**User Role:** Student  

| Step | Action | Expected Result | Actual Result | Status |
|------|--------|-----------------|---------------|--------|
| 1 | Log in as Student | Student dashboard accessible | Logged in successfully | ✅ Pass |
| 2 | Navigate to `/Project/MyProjects` | Project list displayed with current status | Projects listed correctly | ✅ Pass |
| 3 | Check project status before match | Status = "Pending" shown | Status shown as "Pending" | ✅ Pass |
| 4 | After supervisor confirms match, navigate to `/Project/MyProjects` | Status = "Matched" now shown | Status updated to "Matched" | ✅ Pass |
| 5 | Verify supervisor name is now visible | Supervisor's name displayed on the project card | Supervisor name shown correctly | ✅ Pass |
| 6 | Verify the Edit button is no longer available | Edit option not shown for Matched projects | Edit button grayed out and disabled for Matched project | ✅ Pass |

**Result: PASSED**  
**Notes:** ProjectController.MyProjects() uses Include(p => p.Supervisor) to load the supervisor's name. The bilateral identity reveal is confirmed — both student and supervisor see each other's details after matching.

---

## FTC-06: Edit Restriction — Cannot Edit Matched Project

**Objective:** Verify that a student cannot edit a project proposal that has already been matched.
**Pre-condition:** Student is logged in. At least one project with Status = "Matched" and one with Status = "Pending" exist.
**User Role:** Student

| Step | Action | Expected Result | Actual Result | Status |
|------|--------|-----------------|---------------|--------|
| 1 | Log in as Student | Student dashboard accessible | Logged in successfully | ✅ Pass |
| 2 | Navigate to `/Project/MyProjects` | Project list shown with all proposals and their current status | Projects listed correctly with status indicators | ✅ Pass |
| 3 | Locate a project with Status = "Matched" and inspect the Edit button | Edit button is visually grayed out and disabled — cannot be clicked | Edit button rendered as disabled and non-interactive | ✅ Pass |
| 4 | Attempt to click the disabled Edit button on a Matched project | No action occurs — button is non-interactive | Button click has no effect | ✅ Pass |
| 5 | Verify no error message or redirect occurs | No page redirect or error message displayed | Page remains on MyProjects with no redirect | ✅ Pass |
| 6 | Locate a project with Status = "Pending" and inspect the Edit button | Edit button is active and clickable | Edit button fully functional for Pending projects | ✅ Pass |
| 7 | Click the Edit button on a Pending project | Edit form loads correctly with all pre-populated fields | Edit form displayed with existing project details | ✅ Pass |
| 8 | Update the title of a Pending project and submit | Project title updated successfully in database | Title updated and saved correctly | ✅ Pass |

**Result: PASSED**
**Notes:** The edit restriction is enforced at the UI level. The Edit button is rendered in a disabled, grayed-out state for projects with Status = "Matched", preventing students from accessing the edit form. The restriction is visually clear to the user through the button's disabled appearance, without requiring a page redirect or error message.
---

## FTC-07: Admin Pairings Dashboard

**Objective:** Verify that the Module Leader can view a comprehensive dashboard of all confirmed student-supervisor matches.  
**Pre-condition:** At least one matched project exists in the database. Admin account exists.  
**User Role:** Admin (Module Leader)  

| Step | Action | Expected Result | Actual Result | Status |
|------|--------|-----------------|---------------|--------|
| 1 | Navigate to `/Account/AdminLogin` | Admin login form displayed | Admin login form shown | ✅ Pass |
| 2 | Enter valid admin credentials (Name + Password) | Login successful, redirect to `/Admin/Users` | Redirected to Admin Users page | ✅ Pass |
| 3 | Navigate to `/Admin/Pairings` | Pairings dashboard displays all Matched projects | All matched projects listed | ✅ Pass |
| 4 | Verify student name visible for each matched project | Student name shown (not hidden) | Student names visible in admin view | ✅ Pass |
| 5 | Verify supervisor name visible for each matched project | Supervisor name shown | Supervisor names visible | ✅ Pass |
| 6 | Navigate to `/Admin/Projects` | Full list of all projects shown regardless of status | All projects including Pending shown | ✅ Pass |
| 7 | Navigate to `/Admin/Users` | Full list of all registered users shown | All users listed with roles | ✅ Pass |
| 8 | Edit a user's details | User details updated successfully | User record updated in database | ✅ Pass |

**Result: PASSED**  
**Notes:** AdminController.Pairings() queries only projects where Status = "Matched" and SupervisorId != null. Both Student and Supervisor navigation properties are eagerly loaded using Include(), providing full visibility to the administrator.

---

## FTC-08: Role-Based Access Control

**Objective:** Verify that users cannot access pages outside their assigned role and that unauthorised access attempts are redirected to the login page.  
**Pre-condition:** Multiple user accounts with different roles exist.  
**User Role:** All roles tested  

| Step | Action | Expected Result | Actual Result | Status |
|------|--------|-----------------|---------------|--------|
| 1 | Log in as Student, manually navigate to `/Supervisor/Index` | Redirect to `/Account/Login` | Redirected to Login page | ✅ Pass |
| 2 | Log in as Student, manually navigate to `/Admin/Users` | Redirect to `/Account/AdminLogin` | Redirected to Admin Login | ✅ Pass |
| 3 | Log in as Supervisor, manually navigate to `/Project/Submit` | Return Unauthorized (401) response | Unauthorized response returned | ✅ Pass |
| 4 | Log in as Supervisor, manually navigate to `/Admin/Pairings` | Redirect to `/Account/AdminLogin` | Redirected to Admin Login | ✅ Pass |
| 5 | Access any page without logging in | Redirect to Login page | Redirected to Login page | ✅ Pass |
| 6 | Log in as Admin, navigate to `/Admin/Projects` | Admin Projects page loads correctly | Admin dashboard loads correctly | ✅ Pass |
| 7 | Log in as Admin, navigate to `/Supervisor/Index` | Redirect to Login (not an Admin route) | Redirected correctly | ✅ Pass |

**Result: PASSED**  
**Notes:** All controller actions validate HttpContext.Session.GetString("Role") before processing requests. Session-based RBAC is enforced at the server side ensuring that role separation cannot be bypassed through direct URL manipulation.

---

## FTC-09: Group Project Submission

**Objective:** Verify that a Group user can submit a project proposal with group member details that are correctly serialised and stored.  
**Pre-condition:** A Group user account exists and is logged in.  
**User Role:** Group  

| Step | Action | Expected Result | Actual Result | Status |
|------|--------|-----------------|---------------|--------|
| 1 | Log in as Group user | Group dashboard accessible, redirect to `/Project/Submit` | Redirected to Submit page | ✅ Pass |
| 2 | Navigate to `/Project/Submit` | Form displayed with additional group member fields visible | Group member input fields shown | ✅ Pass |
| 3 | Enter project details (Title, Abstract, TechStack, ResearchArea) | Form accepts all project fields | All fields accepted | ✅ Pass |
| 4 | Enter group member 1: Name = "Nimal Perera", UserId = "10001" | Member field accepts input | Input accepted | ✅ Pass |
| 5 | Enter group member 2: Name = "Amali Silva", UserId = "10002" | Second member field accepts input | Input accepted | ✅ Pass |
| 6 | Click "Submit" | Project saved with GroupMembersJson serialised as JSON array | Project saved successfully | ✅ Pass |
| 7 | Navigate to `/Project/MyProjects` | Group members listed under the project card | Both members displayed correctly | ✅ Pass |
| 8 | Attempt group submission with no members entered | Error message "Group submissions must include at least one member." | Validation error shown | ✅ Pass |
| 9 | Attempt group submission with mismatched name/ID count | Error message "Each member must have both name and ID." | Validation error shown | ✅ Pass |

**Result: PASSED**  
**Notes:** ProjectController.Submit() serialises group member data into GroupMembersJson using JsonSerializer.Serialize(). The MyProjects() action deserialises this field using JsonSerializer.Deserialize<List<GroupMemberInfo>>() after materialising the query with ToListAsync().

---

## FTC-10: Research Area Filter on Supervisor Dashboard

**Objective:** Verify that the supervisor's blind review dashboard only displays projects whose research area matches the supervisor's registered expertise.  
**Pre-condition:** Multiple projects with different research areas exist. Supervisor is registered with a specific research area.  
**User Role:** Supervisor  

| Step | Action | Expected Result | Actual Result | Status |
|------|--------|-----------------|---------------|--------|
| 1 | Log in as Supervisor with ResearchArea = "Cybersecurity" | Supervisor dashboard accessible | Logged in successfully | ✅ Pass |
| 2 | Navigate to `/Supervisor/Index` | Only Cybersecurity projects shown | Only Cybersecurity projects displayed | ✅ Pass |
| 3 | Verify Artificial Intelligence projects NOT shown | AI projects absent from the feed | Confirmed — AI projects not visible | ✅ Pass |
| 4 | Verify Web & Mobile Development projects NOT shown | Web projects absent from the feed | Confirmed — Web projects not visible | ✅ Pass |
| 5 | Log out and log in as a Supervisor with ResearchArea = "Artificial Intelligence" | AI Supervisor dashboard accessible | Logged in successfully | ✅ Pass |
| 6 | Navigate to `/Supervisor/Index` | Only Artificial Intelligence projects shown | Only AI projects displayed | ✅ Pass |
| 7 | Verify Cybersecurity projects NOT shown | Cybersecurity projects absent | Confirmed — Cybersecurity projects not visible | ✅ Pass |
| 8 | Log in as a Supervisor with no research area set | All Pending projects shown regardless of area | All projects displayed when no area filter | ✅ Pass |

**Result: PASSED**  
**Notes:** SupervisorController.Index() applies a conditional Where clause: if the supervisor's ResearchArea is not null or empty, projects are filtered by matching research area. If no research area is set, all Pending projects are shown.

---

## Overall Functional Test Results

| Total Test Cases | Passed | Failed | Pass Rate |
|-----------------|--------|--------|-----------|
| 10 | 10 | 0 | 100% |

All ten functional test cases passed successfully when executed against the running ASP.NET Core 8.0 application on localhost. The tests confirm that the blind matching mechanism, identity reveal, role-based access control, group project support, and administrative oversight all function correctly as specified in the system requirements.

---

*Functional tests executed manually against the PAS application running on localhost using Google Chrome and Microsoft Edge. Test results recorded by the Group, April 2026.*

