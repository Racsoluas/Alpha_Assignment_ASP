///Genererat av chatgpt, för att skapa en design-time DbContext-factory för EF Core migrations.
///Har den ursprungliga koden under.

using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Data.Contexts;
using Microsoft.Extensions.Configuration;

namespace Data.Factories
{
    public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            // Gå från Data/Factories/bin/... till lösningens rot
            var basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Presentation"));

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("SqlConnection");

            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new DataContext(optionsBuilder.Options);
        }
    }
}



//using Microsoft.EntityFrameworkCore.Design;
//using Microsoft.EntityFrameworkCore;
//using Data.Contexts;
//using Microsoft.Extensions.Configuration;

//namespace Data.Factories
//{
//    public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
//    {
//        public DataContext CreateDbContext(string[] args)
//        {
//            var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Presentation", "appsettings.json");

//            var configuration = new ConfigurationBuilder()
//                .SetBasePath(Path.GetDirectoryName(appSettingsPath)!)
//                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//                .Build();

//            var connectionString = configuration.GetConnectionString("SqlConnection");

//            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
//            optionsBuilder.UseSqlServer(connectionString);

//            return new DataContext(optionsBuilder.Options);
//        }
//    }
//}