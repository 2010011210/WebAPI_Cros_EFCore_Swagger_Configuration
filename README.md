# WebAPI_Cros_EFCore_Swagger_Configuration
# IOC 和 AutoFac
  注册服务  
  services.AddTransient<ITestServiceA, ServiceA>();  

  使用服务  

    private readonly ITestServiceA _iTestServiceA;  

    public IOCController(ITestServiceA testServiceA)   
    {  
        this._iTestServiceA = testServiceA;  
    }  

    public string Index()   
    {  
        this._iTestServiceA.Show();  
        return "Ok";  
    }  

  使用AutoFac替换自带的IOC容器   
  
    public static IHostBuilder CreateHostBuilder(string[] args) =>  
      Host.CreateDefaultBuilder(args)  
          .UseServiceProviderFactory(new AutofacServiceProviderFactory())  //替换IOC容器  
          .ConfigureContainer<ContainerBuilder>((context, conainerBuilder)=>   //注册  
          {  
              conainerBuilder.RegisterType<ServiceA>().As<ITestServiceA>();  
          })  
          .ConfigureWebHostDefaults(webBuilder =>  
          {  
            webBuilder.UseStartup<Startup>();  
          });  

# 特性 Attribute
  声明特性  

      public class ColumnNameAttribute : Attribute    
        {    
            public ColumnNameAttribute()   
            {  
            }  

        public ColumnNameAttribute(string des)  
        {   
            this.Description = des;  
        }  
  
        public ColumnNameAttribute(string des, string name)  
        {  
            this.Description = des;  
            this.Name = name;  
        }  
  
  
        public string Description { get; set; }  
        public string Name { get; set; }  
  
        public string GetDescription()   
        {  
            return Description;  
        }  
  
        public string GetName()  
        {  
            return Name;  
        }  
    }  
    标记特性
    //[ColumnName("汽车名称", "CarName")]  
    //[ColumnName("汽车名称")]  
    [ColumnName(Name = "CarName",Description = "汽车名称")]  
    public string carName { get; set; }  

    使用特性

      var attributeList = type.GetCustomAttributes(typeof(CustomAttribute),true);  
      foreach (CustomAttribute item in attributeList)   
      {  
          var name = item.Name;  
          res = name;  
      }  

# AOP
 
需要安装Nuget   
Autofac  
Autofac.Extras.DynamicProxy  
Autofac.Extensions.DependencyInjection;  
  
1.Castle动态代理  
  1.1 生成拦截类   

    public class CustomInterceptor : StandardInterceptor
    {
        protected override void PreProceed(IInvocation invocation)
        {
            Console.WriteLine($"当前调用方法：{invocation.Method.Name}");
        }

        protected override void PerformProceed(IInvocation invocation)
        {
            invocation.Proceed();
        }

        protected override void PostProceed(IInvocation invocation)
        {
            Console.WriteLine($"已经调用方法：{invocation.Method.Name}");
        }
    }

  1.2  

    ProxyGenerator generator = new ProxyGenerator();  
    CustomInterceptor interceptor = new CustomInterceptor();
    ITestServiceA serviceA = new ServiceA();

    var iTestServiceA = generator.CreateInterfaceProxyWithTarget<ITestServiceA>(serviceA, interceptor);

    iTestServiceA.Show();

2.AutoFac基于Castle  
2.1 生成拦截的方法  

    public class CustomerMonitorInterceptor : IInterceptor  
    {   
      /// <summary>  
      /// 拦截--invocation.Proceed() 等同于原方法的调用
      /// </summary>
      /// <param name="invocation"></param>  
      public void Intercept(IInvocation invocation)  
      {  
          var method = invocation.Method;  
          var argument = invocation.Arguments;  

          Console.WriteLine($"method:{method}--argument:{argument}");  

          invocation.Proceed();  
      }
    }


2.2 注册IOC  
 conainerBuilder.RegisterType<ServiceA>().As<ITestServiceA>().EnableInterfaceInterceptors();  

 conainerBuilder.RegisterType<CustomerMonitorInterceptor>();  

2.3 在接口上使用继承该接口 都会被拦截     

    [Intercept(typeof(CustomerMonitorInterceptor))]  
    public interface ITestServiceA
    {
        void Show();

        void Show(int i, string name);

    }

 3.Filter  

  3.1 声明特性    

    public class LogActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string url = context.HttpContext.Request.Path.Value;
            string argument = JsonConvert.SerializeObject(context.ActionArguments);

            string controllerName = context.Controller.GetType().FullName;
            string actionName = context.ActionDescriptor.DisplayName;

            Console.WriteLine($"url--{url},---argument:{argument}");

        }
    }

    特性可以标记在controller类上
    //[LogActionFilterAttribute]
    public class AOPController : ControllerBase
    {
        [HttpGet]
        [Route("Get")]
        public string Get(int i) 
        {
            if ( i == 500) 
            {
                throw new Exception("AOP Index Exception");
            }

            return $"{typeof(AOPController)}_Get";
        }
    }

    也可以在注册的时候直接默认加在所有的controller上  
    services.AddControllersWithViews( options => {
        options.Filters.Add<LogActionFilterAttribute>();
        options.Filters.Add<CustomExceptionFilterAttribute>();
    }).AddControllersAsServices(); 


4. 中间件实现AOP  
   
   4.1 创建中间件     

    public class HeaderReadWriteMiddleware  
      {  

        private readonly RequestDelegate _next;    

        public HeaderReadWriteMiddleware(RequestDelegate next) 
        {
            this._next = next;
        }

        public async Task InvokeAsync(HttpContext context) 
        {
            context.Response.OnStarting(async state => {
                var httpcontext = (HttpContext)state;
                httpcontext.Response.Headers.Add("middleware","Tom");
            }, context);

            context.Response.OnCompleted(async state => {
                var httpcontext = (HttpContext)state;
                Console.WriteLine($"请求结果：{httpcontext.Response.StatusCode}");
            }, context);

            await this._next.Invoke(context);
        }  


      }  

      4.2 注册中间件

        app.UseMiddleware<HeaderReadWriteMiddleware>();

   



# 缓存cache
 Dictionary线程不安全，多线程需要加锁，lock。ConcurrentDictionary是线程安全的，  
 //lock (LockObject)  //Diction多线程可能会又问题，要加锁。或者用ConcurrentDiction,这个是线程安全的。  
//{    
//    shopCache[key] = val;    
//}   

 shopCache[key] = new KeyValuePair<DateTime, object>(DateTime.Now.AddSeconds(seconds), val);   
# 委托
  Linq to object where语句 使用的就是委托
  List<User> users = new List<User>() { };  
  users.Add(new User() { id=1,firstName ="Tom"});  
  users.Add(new User() { id=2,firstName ="Jack"});  
  users.Add(new User() { id=3,firstName ="Eric"});  
  var list = users.TomWhere(s => s.id > 1);  
 public static List<User> TomWhere(this List<User> lists,Func<User, bool> func)   
  {    
      List<User> users = new List<User>();  
      foreach (var user in lists)   
      {  
          Thread.Sleep(2000);  
          Console.WriteLine($"Before:userid:{user.id},username:{user.firstName}");  
          if (func.Invoke(user))   
          {  
              users.Add(user);  
              Console.WriteLine($"After:userid:{user.id},username:{user.firstName}");  
          }  
      }  
  
      return users;  
  }  

  其实linq还是用了迭代器，遍历list结果的时候，取一个比较一个，每隔2秒执行一次，不会像上面那样一次对比全部取出

  public static IEnumerable<User> TomEnumeatorWhere(this List<User> lists, Func<User, bool> func)  
  {  
      List<User> users = new List<User>();  
      foreach (var user in lists)  
      {  
          Thread.Sleep(2000);  
          Console.WriteLine($"Before:userid:{user.id},username:{user.firstName}");  
          if (func.Invoke(user))  
          {  
              yield return user;  
              Console.WriteLine($"After:userid:{user.id},username:{user.firstName}");  
          }  
      }  
  }  

  var list = users.TomEnumeatorWhere(s => { 
        return s.id > 1;  
    });  

    foreach (var user in list)  
    {  
        Console.WriteLine($"{user.id}:{user.firstName}");  
    }  
# 1.cross domain
 ConfigureServices class  2022-07-17
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
	
    Add other config file  
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) => {
              var en = hostingContext.HostingEnvironment;
              config.AddJsonFile(path:"privateConfig.json",  
	             optional: true,
	             reloadOnChange: true  
	      );
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
              webBuilder.UseStartup<Startup>();
            });
	
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
	
   
