# WebAPI_Cros_EFCore_Swagger_Configuration
# 中间件middle的原理和使用
app.Use(middleware);  
源码：  

    public class ApplicationBuilder : IApplicationBuilder  
    {  
      //public delegate Task RequestDelegate(HttpContext context);  //RequestDelegate委托，传入一个httpcontext对象，返回一个Task
      private readonly List<Func<RequestDelegate, RequestDelegate>> _components = new();  
      public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
      {
          _components.Add(middleware);
          return this;
      }  

      /// <summary>
        /// Produces a <see cref="RequestDelegate"/> that executes added middlewares.
        /// </summary>
        /// <returns>The <see cref="RequestDelegate"/>.</returns>
        public RequestDelegate Build()
        {
            RequestDelegate app = context =>
            {
                // If we reach the end of the pipeline, but we have an endpoint, then something unexpected has happened.
                // This could happen if user code sets an endpoint, but they forgot to add the UseEndpoint middleware.
                var endpoint = context.GetEndpoint();
                var endpointRequestDelegate = endpoint?.RequestDelegate;
                if (endpointRequestDelegate != null)
                {
                    var message =
                        $"The request reached the end of the pipeline without executing the endpoint: '{endpoint!.DisplayName}'. " +
                        $"Please register the EndpointMiddleware using '{nameof(IApplicationBuilder)}.UseEndpoints(...)' if using " +
                        $"routing.";
                    throw new InvalidOperationException(message);
                }

                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return Task.CompletedTask;
            };

            for (var c = _components.Count - 1; c >= 0; c--)
            {
                app = _components[c](app);   //主要是这一行代码，把加入list的RequestDelegate，然后把最后一个委托当作倒数第二个委托中间件的参数，返回一个RequestDelegate委托作为倒数第三个中间件的参数。next.Invoke(httpcontext)
                //httpcontext作为一连串中间件的参数进行传递
            }

            return app;
        }
    }  

    例如
    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
      {
          Func<RequestDelegate, RequestDelegate> middleware = new Func<RequestDelegate, RequestDelegate>(
              next =>
              {
                  return new RequestDelegate(context => 
                      { 
                          return Task.Run(() =>
                              { 
                                  context.Response.WriteAsync("This is Tom middleWare test begin");
                                  next.Invoke(context);
                                  context.Response.WriteAsync("This is Tom middleWare test end");
                              }
                          ); 
                      }
                  );
              }
          );

          Func<RequestDelegate, RequestDelegate> middleware2 = next => async context =>
            {
                await context.Response.WriteAsync("This is Tom middleWare2 test begin \n");
                await next.Invoke(context);
                await context.Response.WriteAsync("This is Tom middleWare2 test end \n");
            };

          app.Use(middleware);
          app.Use(middleware2);
          app.Use(next => async context =>
            {
                await context.Response.WriteAsync("This is Tom middleWare3 test begin \n");
                await next.Invoke(context);
                await context.Response.WriteAsync("This is Tom middleWare3 test end \n");
            });
          app.Use(next => async context =>
            {
                await context.Response.WriteAsync("This is Tom middleWare4 test begin \n");
                //await next.Invoke(context);    //最后一个加入的中间件，不要调用传入的委托的invoke方法，因为最后一个是默认的委托，会报错
                await context.Response.WriteAsync("This is Tom middleWare4 test end \n");
            });

      }

1.自定义app.Use<Middelware>();     

    app.UseMiddleware<HeaderReadWriteMiddleware>();  

    public class HeaderReadWriteMiddleware
    {
        private readonly RequestDelegate _next;

        public HeaderReadWriteMiddleware(RequestDelegate next)   //必须有构造函数，因为会通过反射调用创建实例
        {
            this._next = next;
        }

        public async Task InvokeAsync(HttpContext context)  //必须有InvokeAsync或Invoke方法，通过反射调用这个方法，查看下面源码
        {
            context.Response.OnStarting(async state => {
                var httpcontext = (HttpContext)state;
                httpcontext.Response.Headers.Add("middleware","Tom");
            }, context);

            context.Response.OnCompleted(async state => {
                var httpcontext = (HttpContext)state;
                //await httpcontext.Response.WriteAsync($"请求结果：{httpcontext.Response.StatusCode}");
                Console.WriteLine($"请求结果：{httpcontext.Response.StatusCode}");
            }, context);

            await this._next.Invoke(context);
        }
    }

    public static class UseMiddlewareExtensions
    {
        internal const string InvokeMethodName = "Invoke";
        internal const string InvokeAsyncMethodName = "InvokeAsync";

                /// <summary>
        /// Adds a middleware type to the application's request pipeline.
        /// </summary>
        /// <typeparam name="TMiddleware">The middleware type.</typeparam>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
        /// <param name="args">The arguments to pass to the middleware type instance's constructor.</param>
        /// <returns>The <see cref="IApplicationBuilder"/> instance.</returns>
        public static IApplicationBuilder UseMiddleware<[DynamicallyAccessedMembers(MiddlewareAccessibility)]TMiddleware>(this IApplicationBuilder app, params object?[] args)
        {
            return app.UseMiddleware(typeof(TMiddleware), args);    //app.Use<TMiddleware>调用的就是这个方法
        }

        /// <summary>
        /// Adds a middleware type to the application's request pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
        /// <param name="middleware">The middleware type.</param>
        /// <param name="args">The arguments to pass to the middleware type instance's constructor.</param>
        /// <returns>The <see cref="IApplicationBuilder"/> instance.</returns>
        public static IApplicationBuilder UseMiddleware(this IApplicationBuilder app, [DynamicallyAccessedMembers(MiddlewareAccessibility)] Type middleware, params object?[] args)
        {
            if (typeof(IMiddleware).IsAssignableFrom(middleware))
            {
                // IMiddleware doesn't support passing args directly since it's
                // activated from the container
                if (args.Length > 0)
                {
                    throw new NotSupportedException(Resources.FormatException_UseMiddlewareExplicitArgumentsNotSupported(typeof(IMiddleware)));
                }

                return UseMiddlewareInterface(app, middleware);
            }

            var applicationServices = app.ApplicationServices;
            return app.Use(next =>                                                //最终调用的还是app.Use方法
            {
                var methods = middleware.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                var invokeMethods = methods.Where(m =>
                    string.Equals(m.Name, InvokeMethodName, StringComparison.Ordinal)        //必须有InvokeAsync或Invoke方法之一，只能有一个
                    || string.Equals(m.Name, InvokeAsyncMethodName, StringComparison.Ordinal)
                    ).ToArray();

                if (invokeMethods.Length > 1)
                {
                    throw new InvalidOperationException(Resources.FormatException_UseMiddleMutlipleInvokes(InvokeMethodName, InvokeAsyncMethodName));
                }

                if (invokeMethods.Length == 0)
                {
                    throw new InvalidOperationException(Resources.FormatException_UseMiddlewareNoInvokeMethod(InvokeMethodName, InvokeAsyncMethodName, middleware));
                }

                var methodInfo = invokeMethods[0];
                if (!typeof(Task).IsAssignableFrom(methodInfo.ReturnType))
                {
                    throw new InvalidOperationException(Resources.FormatException_UseMiddlewareNonTaskReturnType(InvokeMethodName, InvokeAsyncMethodName, nameof(Task)));
                }

                var parameters = methodInfo.GetParameters();
                if (parameters.Length == 0 || parameters[0].ParameterType != typeof(HttpContext))
                {
                    throw new InvalidOperationException(Resources.FormatException_UseMiddlewareNoParameters(InvokeMethodName, InvokeAsyncMethodName, nameof(HttpContext)));
                }

                var ctorArgs = new object[args.Length + 1];
                ctorArgs[0] = next;
                Array.Copy(args, 0, ctorArgs, 1, args.Length);
                var instance = ActivatorUtilities.CreateInstance(app.ApplicationServices, middleware, ctorArgs);  //反射创建实例
                if (parameters.Length == 1)
                {
                    return (RequestDelegate)methodInfo.CreateDelegate(typeof(RequestDelegate), instance);  //返回一个委托
                }

                var factory = Compile<object>(methodInfo, parameters);

                return context =>
                {
                    var serviceProvider = context.RequestServices ?? applicationServices;
                    if (serviceProvider == null)
                    {
                        throw new InvalidOperationException(Resources.FormatException_UseMiddlewareIServiceProviderNotAvailable(nameof(IServiceProvider)));
                    }

                    return factory(instance, context, serviceProvider);
                };
            });
        }

    }

2.自定义app.UseCushomeMiddle(middle);  //这个就相当于app.Use(Middle);  
3.自定义app.UseCustomeMiddle();

    public static class MiddleExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        public static void UseCustomeAPIFilter(this IApplicationBuilder app) 
        {
            Func<RequestDelegate, RequestDelegate> func = next =>
            {
                return new RequestDelegate(context =>
                {
                    if (context.Request.Path.Value.Contains("api"))
                    {
                        return Task.Run(()=> 
                        {
                            context.Response.WriteAsync("This is api check, start \n");
                            context.Response.WriteAsync("This is api check, end \n");
                        });
                    }
                    else
                    {
                        return Task.Run(() =>
                        {
                            next.Invoke(context);
                        });
                    }
                });
            };

            app.Use(func);
        }
    }

    app.UseCustomeAPIFilter();    

4. app.UseMiddleware<StaticPagaMiddleware>("保存路径", true);    
 
        public class StaticPagaMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly string directoryPath;
            private readonly bool deleted;

            public StaticPagaMiddleware(RequestDelegate next, string directoryUrl, bool deleted)
            {
                this._next = next;
                this.directoryPath = directoryUrl;
                this.deleted = deleted;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                if (context.Request.Path.Value!.StartsWith("/weatherforecast"))
                {
                    Console.WriteLine($"path:{context.Request.Path.Value}");

                    #region  context.Response.Body

                    var originalStream = context.Response.Body;
                    using (var copyStream = new MemoryStream()) 
                    {
                        context.Response.Body = copyStream;
                        await _next(context);

                        copyStream.Position = 0;
                        var reader = new StreamReader(copyStream);
                        var content = await reader.ReadToEndAsync();
                        string url = context.Request.Path.Value;

                        this.SaveHtml(url, content);
                        copyStream.Position = 0;
                        await copyStream.CopyToAsync(originalStream);
                        context.Response.Body = originalStream;

                    }

                    #endregion
                }
                else {
                    await _next(context);
                }
            }

            private void SaveHtml(string url, string html) 
            {
                try
                {
                    if (string.IsNullOrEmpty(html)) 
                    {
                        return;
                    }

                    if (!url.EndsWith("weatherforecast")) 
                    {
                        return;
                    }

                    //string directoryPath = "D:\\WangCong\\data";
                    if (Directory.Exists(directoryPath) == false) 
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    var fullPath = Path.Combine(directoryPath, url.Split("/").Last());
                    File.WriteAllText(fullPath, html);
                }
                catch (Exception e) 
                {
                    Console.WriteLine(e.Message);
                }
            }

        }

5. 上面的可以改写为，需要扩展一下方法app.UseStaticPage("保存路径"，true)  //带参数的    
   
        /// <summary>
        /// 中间件扩展类
        /// </summary>
        public static class MiddleExtension
        {
            public static IApplicationBuilder UseStaticPage(this IApplicationBuilder app,string directoryPath, bool isDelete) 
            {
                return app.UseMiddleware<StaticPagaMiddleware>("D:\\WangCong\\data", true);
            }
        }
   

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
	
   
