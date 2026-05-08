# Entity Framework Core Skill - MusicBar

## Purpose

This skill helps GitHub Copilot generate Entity Framework Core code for the MusicBar ASP.NET Core MVC application.

The application uses:
- ASP.NET Core MVC
- Entity Framework Core 6
- SQL Server
- Repository pattern
- Code-first migrations

---

# Project Architecture

## Main technologies

- ASP.NET Core MVC
- EF Core 6
- SQL Server LocalDB

---

# DbContext

The application uses:

csharp
MusicBarDbContext

# EF Workflow

1. Modify model
2. Create migration
3. Update database
4. Update repositories

# Routing Rules

- /Song/Details/{id}
- /Album/Details/{id}
- /Artist/Details/{id}