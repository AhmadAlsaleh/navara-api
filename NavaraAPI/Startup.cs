using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartLifeLtd.IServices;
using NavaraAPI.Models;
using NavaraAPI.Services;
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Classes;
using SmartLifeLtd.Data.DataContexts;

namespace NavaraAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IConfigurationSection DBConf = Configuration.GetSection("DBOnline");
            ConnectionSetting connectionString = new ConnectionSetting(
                DBConf.GetValue<string>("ServerName"),
                DBConf.GetValue<string>("DatabaseName"),
                DBConf.GetValue<string>("Port"),
                DBConf.GetValue<string>("Username"),
                DBConf.GetValue<string>("Password")
            );
            services.AddDbContext<NavaraDbContext>(options => options.UseSqlServer(connectionString.ToString()));
            NavaraDbContext.DEFAULT_CONNECTION_STRING = connectionString.ToString();

            services.AddScoped<IUsersService, UsersService>();
            // AddIdentity adds cookie based authentication
            // Adds scoped classes for things like UserManager, SignInManager, PasswordHashers etc..
            services.AddIdentity<ApplicationUser, IdentityRole>(optoins =>
            {
                //Onyl unique emails
                optoins.User.RequireUniqueEmail = true;
            })
                           // Adds UserStore and RoleStore from this context
                           // That are consumed by the UserManager and RoleManager
                           .AddEntityFrameworkStores<NavaraDbContext>()
                           // Adds a provider that generates unique keys and hashes for things like
                           // forgot password links, phone number verification codes etc...
                           .AddDefaultTokenProviders();

            //Configure IoC values
            IoCCore.AppViewModel.Audience = Configuration["Jwt:Audience"];
            IoCCore.AppViewModel.Issuer = Configuration["Jwt:Issuer"];
            IoCCore.AppViewModel.SecretKey = Configuration["Jwt:SecretKey"];


            //Add the token based authentication
            services.AddAuthentication().AddJwtBearer(options =>
            {
                //Set the parameters
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = IoCCore.AppViewModel.Issuer,
                    ValidAudience = IoCCore.AppViewModel.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                      Encoding.UTF8.GetBytes(IoCCore.AppViewModel.SecretKey))
                };
            }).AddFacebook(options =>
            {
                options.AppId = "298734884211109";
                options.AppSecret = "d01d4721a2113bf6f623981653503ccc";
            }).AddGoogle(options =>
            {
                options.ClientId = "876550715606-pi1lhpd3856q7l63c9ado274950dm676.apps.googleusercontent.com";
                options.ClientSecret = "xDiz4sz_LTqhObJ2E8fRrLxN";
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.Expiration = TimeSpan.FromHours(2.0);
                options.LoginPath = "/Home/SignIn";
                options.AccessDeniedPath = "/Home";
                options.SlidingExpiration = true;
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, NavaraDbContext Context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            Context.Database.Migrate();

        }
    }
}
