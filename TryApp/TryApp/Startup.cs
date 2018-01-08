using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TryApp.Models;


namespace TryApp
{
    public class Startup
    {
        //  public Startup(IConfiguration configuration)
        public Startup(IHostingEnvironment env)

        {
           // Configuration = configuration;
           var builder = new ConfigurationBuilder()
              .SetBasePath(env.ContentRootPath)
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
              .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //   services.AddDbContext<TryApp.Models.TodoContext>(opt => opt.UseInMemoryDatabase("TodoList"));
            //Data Source=OMARATIA;Initial Catalog=master;Integrated Security=True
         //    services.AddDbContext<TryApp.Models.MusicStoreContext>(opt => opt.UseSqlServer("Password = sasql; Persist Security Info = True; User ID = sasql; Initial Catalog = test; Data Source =."));
         //  services.Add(new ServiceDescriptor(typeof(MusicStoreContext), new MusicStoreContext("Password=sasql;Persist Security Info=True;User ID=sasql;Initial Catalog=test;Data Source=.")));

            services.AddMvc();
            services.Add(new ServiceDescriptor(typeof(MusicStoreContext), new MusicStoreContext(Configuration.GetConnectionString("DefaultConnection"))));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
