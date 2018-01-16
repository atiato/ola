using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TryApp.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Jwt";
                options.DefaultChallengeScheme = "Jwt";
            }).AddJwtBearer("Jwt", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    //ValidAudience = "the audience you want to validate",
                    ValidateIssuer = false,
                    //ValidIssuer = "the isser you want to validate",

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("the secret that needs to be at least 16 characeters long for HmacSha256")),

                    ValidateLifetime = true, //validate the expiration and not before values in the token

                    ClockSkew = TimeSpan.FromMinutes(5) //5 minute tolerance for the expiration date
                };
            });
            //   services.AddDbContext<TryApp.Models.TodoContext>(opt => opt.UseInMemoryDatabase("TodoList"));
            //Data Source=OMARATIA;Initial Catalog=master;Integrated Security=True
            //    services.AddDbContext<TryApp.Models.MusicStoreContext>(opt => opt.UseSqlServer("Password = sasql; Persist Security Info = True; User ID = sasql; Initial Catalog = test; Data Source =."));
            //  services.Add(new ServiceDescriptor(typeof(MusicStoreContext), new MusicStoreContext("Password=sasql;Persist Security Info=True;User ID=sasql;Initial Catalog=test;Data Source=.")));
            
            services.AddMvc();
            services.Add(new ServiceDescriptor(typeof(StoreContext), new StoreContext(Configuration.GetConnectionString("DefaultConnection"))));
            services.AddDistributedRedisCache(option =>
            {
                option.Configuration = "172.30.167.64:6379,password=redis";
                
             //   option.InstanceName = "master";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
