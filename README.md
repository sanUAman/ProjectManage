# ProjeX

ProjeX is a web application for managing team and individual projects, implemented on the basis of ASP.NET Core. The functionality allows you to manage projects, add participants, assign roles, and track task completion.

---

## ⚙️ Technologies

- ASP.NET Core (MVC)
- Entity Framework Core
- C#
- SQL Server
- Razor Pages

---

## 📌 Basic functionality

- User registration and login
- Creating and deleting projects
- Adding participants to projects
- Assigning roles and tasks
- Displaying task progress

---

## 📂 Project structure
- Controllers/ — request processing logic (Home, Participant)
- Models/ — classes Participant, NameOfProject, ProjectParticipant
- Views/ — Razor-pages for the interface
- Data/ — ApplicationDbContext.cs, EF Core settings

---

## 📄 Models examples
- NameOfProject
  ```bash
  public class NameOfProject {
    public int Id { get; set; }
    public string Name { get; set; }
  }
- Participant
  ```bash
  public class Participant {
    public int Id { get; set; }
    public string Nickname { get; set; }
    public string Password { get; set; }
  }
- ProjectParticipant
  ```bash
  public class ProjectParticipant {
    public int Id { get; set; }
    public int ParticipantId { get; set; }
    public string ParticipantName { get; set; }
    public string ParticipantPassword { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; }
    public string Role { get; set; }
    public string Tasks { get; set; }
    public bool Status { get; set; }
  }

---

## Guide for Visual Studio 2022

1. Create a folder, in this folder create another one with the same name, open Git Bash in this folder.

2. Clone the repository:
   ```bash
   git clone https://github.com/sanUAman/ProjectManage-Website.git
   cd ProjectManage-Website

3. Open folder as a project in Visual Studio and run the project as a ProjectManage.csproj file

4. To create all the necessary tables, enter the following command in the terminal
   ```bash
   dotnet ef database update

5. Run the project

