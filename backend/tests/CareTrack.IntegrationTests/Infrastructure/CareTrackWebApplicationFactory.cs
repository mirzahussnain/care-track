// using CareTrack.Infrastructure.Persistance;
// using Microsoft.AspNetCore.Hosting;
// using Microsoft.AspNetCore.Mvc.Testing;
// using Microsoft.Data.Sqlite;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Infrastructure;
// using Microsoft.Extensions.DependencyInjection;

// namespace CareTrack.IntegrationTests.Infrastructure;

// public class CareTrackWebApplicationFactory
//     : WebApplicationFactory<Program>
// {
//   private SqliteConnection? _connection;

//   protected override void ConfigureWebHost(
//       IWebHostBuilder builder)
//   {
//     builder.ConfigureServices(services =>
//     {
//       // Remove the production SQL Server configuration.
//       var dbContextDescriptor =
//               services.SingleOrDefault(
//                   service =>
//                       service.ServiceType ==
//                       typeof(
//                           IDbContextOptionsConfiguration<
//                               CareTrackDbContext>));

//       if (dbContextDescriptor is not null)
//       {
//         services.Remove(dbContextDescriptor);
//       }

//       // Create one open SQLite in-memory connection.
//       _connection = new SqliteConnection(
//               "Data Source=:memory:");

//       _connection.Open();

//       // Register CareTrackDbContext using SQLite.
//       services.AddDbContext<CareTrackDbContext>(
//               options =>
//               {
//                 options.UseSqlite(_connection);
//               });

//       var serviceProvider =
//               services.BuildServiceProvider();

//       using var scope =
//               serviceProvider.CreateScope();

//       var dbContext =
//               scope.ServiceProvider
//                   .GetRequiredService<CareTrackDbContext>();

//       dbContext.Database.EnsureCreated();
//     });
//   }

//   protected override void Dispose(bool disposing)
//   {
//     if (disposing)
//     {
//       _connection?.Dispose();
//     }

//     base.Dispose(disposing);
//   }
// }