# Instructions

This project uses Entity Framework Core (EF Core) by scaffolding from an existing database. For any changes, modify the database, then scaffold again and remove the previous scaffold version.

## Prerequisites

Ensure your local environment or computer has the following:

1. **Dotnet CLI Tools** from [EF Core tools reference (.NET CLI) - EF Core | Microsoft Learn](https://learn.microsoft.com/en-us/ef/core/cli/dotnet), make sure the version is at least 8.
2. An **existing database** (this project uses PostgreSQL). Ensure your database is running.
3. Basic knowledge of [Reverse Engineering - EF Core | Microsoft Learn](https://learn.microsoft.com/en-us/ef/core/managing-schemas/scaffolding).

## Local Connection

For local use, you can use the following connection string:

```
Host=localhost;Database=kpcos;Username=postgre;Password=12345;Trust Server Certificate=True;
```

## Scaffolding Instructionss

To scaffold the database, use the following command in your terminal, command prompt, or PowerShell. Ensure you are in the `DataAccessLayer` directory:

```
dotnet ef dbcontext scaffold "Host=localhost;Database=kpcos;Username=postgre;Password=12345;Trust Server Certificate=True;" "Npgsql.EntityFrameworkCore.PostgreSQL" --output-dir ./
```

> **Note:** You can customize the output directory for scaffolding entities. For more details, refer to [Reverse Engineering - EF Core | Microsoft Learn](https://learn.microsoft.com/en-us/ef/core/managing-schemas/scaffolding).

## Post-Scaffolding Modifications

After scaffolding, you will need to make manual adjustments:

1. **Delete Old Entities:** Remove the old version of the entity files in the `Entities` folder.
2. **Move New Entities:** Transfer the newly generated entity files (which are in the `DataAccessLayer` directory but not in `Entities`) to the `Entities` folder.
3. **Adjust the DbContext:** Perform the same steps for the DbContext file.
4. **Update Namespaces:** Ensure all namespaces are updated correctly to align with your project structure.

By following these steps, you can keep your project in sync with database changes.

