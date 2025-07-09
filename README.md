# ProjeX

ProjeX is a web application for managing team and individual projects, implemented on the basis of ASP.NET Core. The functionality allows you to manage projects, add participants, assign roles, and track task completion.

---

## ⚙️ Технології

- ASP.NET Core (MVC)
- Entity Framework Core
- C#
- SQL Server
- Razor Pages

---

## 📌 Основний функціонал

- Реєстрація та вхід користувачів
- Створення та видалення проєктів
- Додавання учасників до проєктів
- Призначення ролей та завдань
- Відображення прогресу виконання

---

## 📂 Структура проєкту
  Controllers/ — логіка обробки запитів (Home, Participant)
  Models/ — класи Participant, NameOfProject, ProjectParticipant
  Views/ — Razor-сторінки для інтерфейсу
  Data/ — ApplicationDbContext.cs, налаштування EF Core

## Гайд для Visual Studio 2022

1. Створіть папку, в цій папці створіть ще одну з такою ж назвою, відкрийте Git Bash в цій папці

2. Клонуйте репозиторій:
   ```bash
   git clone https://github.com/sanUAman/ProjectManage-Website.git
   cd ProjectManage-Website

3. Відкрийте папку як проект в Visual Studio та запустіть проект як файл ProjectManage.csproj

4. Для створення всіх потрібних таблиць введіть таку команду в термінал
   ```bash
   dotnet ef database update

5. Запустіть проект

