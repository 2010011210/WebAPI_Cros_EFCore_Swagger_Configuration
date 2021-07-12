# WebAPI_Cros_EFCore_Swagger_Configuration
# 1.cross domain
 ConfigureServices class  
 services.AddCors(options => options.AddPolicy("AllowAll",policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));    
 Configure class  
 app.UseCors("AllowAll");  
# 2.Swagger
  ConfigureService class
   //swagger UI
  services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "ThisIsTitle", Version = "ThisIsVersion" });
    });
  Configure class
   app.UseSwagger();
      app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json","v1testTittle");
      });
# 3.EFCore
  ## import the useful package,then create a class that inherit DbContext
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
  ## in the action
    using (var content = new TestContext())
        {
          var log = logging;
          var a = content.cars;
          var name = Guid.NewGuid().ToString();
          content.cars.Add(new Car() { carName = name ,createTime = DateTime.Now });
          content.SaveChanges();
        }
   ## Coding first
   Tools =>Nuget package manager =>Package manager console
   First ,input "Add-Migration FirstMigration", then input "Update-Database"
# 4.Configuration
In the controller
    private IConfiguration _configuration;
    public WeatherForecastController(IConfiguration configuration)
    {
      _configuration = configuration;
    }
    we can get value from the appsettings.json file
    var config = _configuration.GetSection("ConnectionString");
    var configObj = _configuration.GetSection("logging1").Get<Logging>();
# 5.log4
 add a configuration file log4Config.xml
 <!--the location of log file-->
 			<file value="./LogTom/log2021" />
			<appendToFile value="true" />
			<!--按照何种方式产生多个日志文件(日期[Date],文件大小[Size],混合[Composite])-->
			<rollingStyle value="Composite" />
			<RollingStyle value="Date" />
			<staticLogFileName value="false" />
			<datePattern value="yyyy.MM.dd'.log'" />
			<!--最多产生的日志文件数，超过则只保留最新的n个。设定值value="－1"为不限文件数-->
			<maxSizeRollBackups value="10" />
      <maximumFileSize value="1MB" />
  in the startup class
    public static ILoggerRepository LogRepository { get; set; }  
	
    public Startup(IConfiguration configuration)  
    {  
	
      Configuration = configuration;  
      LogRepository = LogManager.CreateRepository("NETCoreRepository");   //仓库的名字可以在配置文件中配置，也可以直接写死
      XmlConfigurator.Configure(LogRepository, new FileInfo("log4Config.xml"));  //读取配置文件
    }  
	
  in the controller  
	
      //type 是指你当前使用log的对象的类，例如需要在HomeController中使用，参数就为typeof(HomeController)
    private readonly ILog _log = LogManager.GetLogger(Startup.LogRepository.Name, typeof(WeatherForecastController));  
	
    _log.Info("Information data");  
	
    _log.Error(DateTime.Now.ToString() + " error test");  
	
   
