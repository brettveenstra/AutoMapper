using System;
using System.Data.Entity;
using System.Linq;
using AutoMapper.QueryableExtensions;
using AutoMapper.UnitTests;
using Should;
using Xunit;

namespace AutoMapper.IntegrationTests.Net4
{
  public class ProjectToSupportAfterMap : AutoMapperSpecBase
  {
    private ConcreteTypeA[] _destinations;

    public class ConcreteTypeA
    {
      public string FirstName { get; set; }
      public int ID { get; set; }
      public string Name { get; set; }
    }

    public class DbEntityA
    {
      public int ID { get; set; }
      public string Name { get; set; }
    }

    public class DatabaseInitializer : CreateDatabaseIfNotExists<Context>
    {
      protected override void Seed(Context context)
      {
        context.EntityA.AddRange(new[]
        {
          new DbEntityA {ID = 1, Name = "Alain Brito"},
          new DbEntityA {ID = 2, Name = "Jimmy Bogard"},
          new DbEntityA {ID = 3, Name = "Bill Gates"}
        });
        base.Seed(context);
      }
    }

    public class Context : DbContext
    {
      public Context()
      {
        Database.SetInitializer(new DatabaseInitializer());
      }

      public DbSet<DbEntityA> EntityA { get; set; }
    }

    protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
    {
      cfg.CreateMap<DbEntityA, ConcreteTypeA>()
        .AfterMap((d, c) => { c.FirstName = c.Name.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)[0]; });
    });

    [Fact]
    public void Should_project_and_execute_AfterMap()
    {
      using (var context = new Context())
      {
        _destinations = context.EntityA.ProjectTo<ConcreteTypeA>(Configuration).ToArray();
      }
      _destinations.Length.ShouldEqual(3);
      _destinations[2].Name.ShouldEqual("Bill Gates");
      _destinations[2].FirstName.ShouldEqual("Bill");
      _destinations[1].FirstName.ShouldEqual("Jimmy");
      _destinations[0].FirstName.ShouldEqual("Alain");
    }
  }
}