﻿using System;
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
using SmartLifeLtd.Data.AspUsers;
using SmartLifeLtd.Classes;
using SmartLifeLtd.Data.DataContexts;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Http;
using SmartLifeLtd.Data.Tables.Navara;
using SmartLifeLtd.Services;
using SmartLifeLtd.IServices;
using NavaraAPI.Controllers;
using DinkToPdf;
using DinkToPdf.Contracts;
using NavaraAPI.Classes;

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
            IConfigurationSection DBConf = Configuration.GetSection("DBOnline2");
            IConfigurationSection DBLogConf = Configuration.GetSection("DBLogOnline");
            ConnectionSetting connectionString = new ConnectionSetting(
                DBConf.GetValue<string>("ServerName"),
                DBConf.GetValue<string>("DatabaseName"),
                DBConf.GetValue<string>("Port"),
                DBConf.GetValue<string>("Username"),
                DBConf.GetValue<string>("Password")
            );
            ConnectionSetting logConnectionString = new ConnectionSetting(
                 DBLogConf.GetValue<string>("ServerName"),
                 DBLogConf.GetValue<string>("DatabaseName"),
                 DBLogConf.GetValue<string>("Port"),
                 DBLogConf.GetValue<string>("Username"),
                 DBLogConf.GetValue<string>("Password")
             );
            services.AddDbContext<NavaraDbContext>(options => options.UseSqlServer(connectionString.ToString()));
            //services.AddDbContext<BaseDbContext>(options => options.UseSqlServer(connectionString.ToString()));
            services.AddDbContext<LogDbContext>(options => options.UseSqlServer(logConnectionString.ToString()));
            NavaraDbContext.DEFAULT_CONNECTION_STRING = connectionString.ToString();

            services.AddScoped<IUsersService, UsersService<Account, NavaraDbContext>>();
            // AddIdentity adds cookie based authentication
            // Adds scoped classes for things like UserManager, SignInManager, PasswordHashers etc..
            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(optoins =>
            {
                //Onyl unique emails
                optoins.User.RequireUniqueEmail = false;
                optoins.Password.RequireDigit = false;
                optoins.Password.RequiredLength = 6;
                optoins.Password.RequiredUniqueChars = 0;
                optoins.Password.RequireLowercase = false;
                optoins.Password.RequireNonAlphanumeric = false;
                optoins.Password.RequireUppercase = false;
            })
                           // Adds UserStore and RoleStore from this context
                           // That are consumed by the UserManager and RoleManager
                           .AddEntityFrameworkStores<NavaraDbContext>()
                           // Adds a provider that generates unique keys and hashes for things like
                           // forgot password links, phone number verification codes etc...
                           .AddDefaultTokenProviders();

            //Configure IoC values
            JwtService.Audience = Configuration["Jwt:Audience"];
            JwtService.Issuer = Configuration["Jwt:Issuer"];
            JwtService.SecretKey = Configuration["Jwt:SecretKey"];


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
                    ValidIssuer = JwtService.Issuer,
                    ValidAudience = JwtService.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                      Encoding.UTF8.GetBytes(JwtService.SecretKey))
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
            services.AddSignalR();

            #region PDF Generator
            var context = new CustomAssemblyLoadContext();
            context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.dll"));
            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            #endregion

            #region Configure Email
            EmailService.AppName = "Navara Store";
            EmailService.SenderName = "Navara Store Team";
            EmailService.ConfirmationURL = "http://navarastore.com/Users/ConfirmAccount?token={0}&userid={1}";
            EmailService.ResetURL = "http://navarastore.com/Users/ResetPassword?token={0}&userid={1}";
            EmailService.SenderEmail = "noreply@navarastore.com";
            EmailService.Password = "P@ssw0rd";
            EmailService.SupportEmail = "support@navarastore.com";
            EmailService.SupportPassword = "P@ssw0rd";
            EmailService.ServerMailHost = "mi3-wts6.a2hosting.com";
            EmailService.ServerPort = 25;
            EmailService.SSLNeed = false;
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, NavaraDbContext Context)
        {
            app.UseDirectoryBrowser(new DirectoryBrowserOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot")),
                RequestPath = new PathString()
            });
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();
            app.UseMvc();
            Context.Database.Migrate();
            app.UseSignalR(routes =>
            {
                routes.MapHub<ChatHub>("/chatHub");
            });
            //try
            //{
            //    var project = new Project
            //    {
            //        Name = "Test Project ",
            //        Name2 = "مشروع تجريب",
            //        Description = "this project for test and you dont need to see it so go away ",
            //        Description2 = "هيدا المشروع للتجريب و انت ما بهمك يلا من هون",
            //    };
            //    var Image = new ProjectImage { ImagePath = "Images/Course/batota.jpeg" };
            //    var Image2 = new ProjectImage { ImagePath = "Images/Course/batota.jpeg" };
            //    var Image3 = new ProjectImage { ImagePath = "Images/Course/batota.jpeg" };
            //    project.ProjectImages.Add(Image);
            //    project.ProjectImages.Add(Image2);
            //    project.ProjectImages.Add(Image3);
            //    var itemes = Context.Items.Take(4).ToList();
            //    foreach (var item in itemes)
            //    {
            //        project.ProjectItems.Add(new ProjectItem { ItemID = item.ID, ProjectID = project.ID });
            //    }
            //    Context.Projects.Add(project);
            //    Context.SaveChanges();

            //}
            //catch (Exception ex)
            //{

    
            //}
           
        }
    }
}
