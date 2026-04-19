# Functional Test Cases – PAS Blind Matching System

**Project:** PUSL2020 – Project Approval System (PAS)  
**Testing Type:** Functional (User Journey / Black-Box)  
**Tested Against:** ASP.NET Core 8.0 Application  

---

## FTC-01: Student Registration and Login

**Objective:** Verify a student can register and log in successfully.

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Navigate to `/Account/Register` | Registration form is displayed | ✅ Pass |
| 2 | Enter Name, Email, Password, Role = "Student" | Form accepts input | ✅ Pass |
| 3 | Click "Register" | Account is created, redirect to login | ✅ Pass |
| 4 | Navigate to `/Account/Login` | Login form is displayed | ✅ Pass |
| 5 | Enter registered email and password | Login successful, session created | ✅ Pass |
| 6 | Check session Role = "Student" | Role is correctly stored in session | ✅ Pass |

---

## FTC-02: Student Project Submission

**Objective:** Verify a logged-in student can submit a project proposal.

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Log in as Student | Dashboard is accessible | ✅ Pass |
| 2 | Navigate to `/Project/Submit` | Submission form is displayed with research areas | ✅ Pass |
| 3 | Enter Title, Abstract, TechStack, ResearchArea | Form accepts all fields | ✅ Pass |
| 4 | Click "Submit" | Project saved with Status = "Pending" | ✅ Pass |
| 5 | Navigate to `/Project/MyProjects` | Submitted project appears in the list | ✅ Pass |
| 6 | Verify student identity is stored (StudentId) | StudentId matches logged-in user | ✅ Pass |

---

## FTC-03: Blind Review – Supervisor Cannot See Student Identity

**Objective:** Verify that supervisor cannot see student name/email on the browse dashboard.

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Log in as Supervisor | Supervisor dashboard accessible | ✅ Pass |
| 2 | Navigate to `/Supervisor/Index` | List of pending projects shown | ✅ Pass |
| 3 | Check project cards for student name | Student name shows "Hidden (Blind Review)" | ✅ Pass |
| 4 | Check project cards for student email | No student email is visible | ✅ Pass |
| 5 | Verify project Title, Abstract, TechStack visible | Technical content is fully visible | ✅ Pass |
| 6 | Verify projects filtered by supervisor's ResearchArea | Only matching research area projects shown | ✅ Pass |

---

## FTC-04: Supervisor Confirms Match – Identity Reveal

**Objective:** Verify that confirming a match updates project status and reveals student identity.

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Log in as Supervisor | Dashboard accessible | ✅ Pass |
| 2 | Browse blind project feed at `/Supervisor/Index` | Pending projects shown anonymously | ✅ Pass |
| 3 | Click "Confirm Match" on a project | POST to `/Supervisor/SelectProject/{id}` | ✅ Pass |
| 4 | Verify project Status changes to "Matched" | Status = "Matched" in database | ✅ Pass |
| 5 | Verify SupervisorId is assigned to project | SupervisorId matches logged-in supervisor | ✅ Pass |
| 6 | Navigate to `/Supervisor/MyMatches` | Student name and email are now revealed | ✅ Pass |

---

## FTC-05: Student Views Match Status

**Objective:** Verify a student can track their project status and see supervisor details after matching.

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Log in as Student | Dashboard accessible | ✅ Pass |
| 2 | Navigate to `/Project/MyProjects` | Project list shown with current status | ✅ Pass |
| 3 | Check status of submitted project (before match) | Status = "Pending" | ✅ Pass |
| 4 | After supervisor confirms match, refresh page | Status = "Matched" | ✅ Pass |
| 5 | Verify supervisor name is now displayed | Supervisor name visible to student | ✅ Pass |

---

## FTC-06: Edit Restriction – Cannot Edit Matched Project

**Objective:** Verify that a student cannot edit a project that has already been matched.

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Log in as Student | Dashboard accessible | ✅ Pass |
| 2 | Navigate to `/Project/Edit/{id}` for a Matched project | Redirect to MyProjects with error message | ✅ Pass |
| 3 | Verify error message: "You can only edit Pending projects." | Error message displayed via TempData | ✅ Pass |
| 4 | Verify edit form does NOT load for Matched project | Form is not accessible | ✅ Pass |

---

## FTC-07: Admin – View All Pairings

**Objective:** Verify the Module Leader (Admin) can view all matched pairings.

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Log in as Admin | Admin dashboard accessible | ✅ Pass |
| 2 | Navigate to `/Admin/Pairings` | All matched projects displayed | ✅ Pass |
| 3 | Verify student name and supervisor name visible | Both identities shown in admin view | ✅ Pass |
| 4 | Verify unmatched projects also visible | Projects with Status = "Pending" also listed | ✅ Pass |

---

## FTC-08: Role-Based Access Control

**Objective:** Verify that users cannot access pages outside their role.

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Log in as Student, navigate to `/Supervisor/Index` | Redirect to Login (Unauthorized) | ✅ Pass |
| 2 | Log in as Supervisor, navigate to `/Project/Submit` | Redirect to Unauthorized | ✅ Pass |
| 3 | Access any page without logging in | Redirect to Login page | ✅ Pass |
| 4 | Log in as Admin, navigate to `/Admin/Projects` | Admin dashboard loads correctly | ✅ Pass |

---
