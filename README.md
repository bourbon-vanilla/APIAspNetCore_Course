# Building Your First API with ASP.NET Core
Starter files and fully functioning finished sample for my Building Your First API with ASP.NET Core course at Pluralsight (https://app.pluralsight.com/library/courses/asp-dotnet-core-api-building-first/table-of-contents)

### Entity Framework
##### Packages needed for migrations:
```
microsoft.entityframeworkcore.tools
```

##### Create Migration
```
Add-Migration CityInfoDBInitialMigration
```
If this is a first initial migration, you have to delete the content of the Up-Method from the generated file `x_CityInfoDBInitialMigration.cs`
##### Update Database after migration creation
```
Update-Database
```
##### Do it in Code
You can do it code, i.e. in the Program class for demo purposes
```csharp
      var host = CreateHostBuilder(args).Build();

      using(var scope = host.Services.CreateScope())
      {
          var dbContext = scope.ServiceProvider.GetService<CityInfoContext>();

          // if for demo purposes - delete existing & create fresh database on sql-server
          dbContext.Database.EnsureDeleted();
          // You
          var createdNow = dbContext.Database.EnsureCreated();
          // or if you dont delete you migrate the existing database, if the db is not the latest
          if (!createdNow)
              dbContext.Database.Migrate(); // The migration will be done only if not latest db schema
      }

      host.Run();
```


### Check if context matches database schema
```
bool isCompatible = context.Database.CompatibleWithModel(true);
```
