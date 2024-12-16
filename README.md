# Instruction

For using this project source that generates the database and its data seeding, follow these steps to ensure smooth execution:

1. **Update the database with the image** (read more here: [Migrations Overview - EF Core | Microsoft Learn](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli)).
2. Simply update the database, add a new image, or apply migrations **only for changes in the entity or fluent API context files**.  
   When you perform an update, the system will **auto-generate a default data seed**.

---

## Changes in Entity and Fluent API Context Files

- If you change something in the **entity** or **fluent API context file**, add a **new version** of the image or migration using the naming convention:  
  `v + number` (e.g., `v1 => v2`).

- Once the image or migration is successfully added:
   - Update the database.  
   - **Tip:** For better success rates, drop the current running database before applying updates.

---

## Handling Database Connection Errors

If you encounter errors when updating or interacting with the database:

1. **Fix the connection string** in:
   - `appsettings.json`, or
   - the `hardConn` variable in the fluent API context file.

2. For startup project errors with `KPCOS.API` (connection string not retrieved):
   - Comment out the **"get connection string"** section.
   - Uncomment the `hardConn` and try again.

---

### Helpful Resource

For further details on EF Core migrations, refer to [Migrations Overview - EF Core | Microsoft Learn](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli).
