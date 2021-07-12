using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebApiDemo.Model
{
  public class TestContext : DbContext
  {
    public TestContext()
    {

    }
    /// <summary>
    /// constructor for Coding first
    /// </summary>
    /// <param name="options"></param>
    public TestContext(DbContextOptions<TestContext> options) : base(options) { 
    
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
      optionsBuilder.UseSqlServer(@"Server=.;Database=;User ID=;Password=;");
    }
    public DbSet<LocationInfo> locationInfos { get; set; }
    public DbSet<Car> cars { get; set; }
  }
}
