using Dgt.CrmMicroservice.Domain;
using Dgt.CrmMicroservice.Infrastructure.FileBased;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Dgt.CrmMicroservice.WebApi
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            const string contactsPath = @".\Data\contacts.json";
            const string branchesPath = @".\Data\branches.json";
            const int delay = 500;

            services.AddTransient<IContactRepository>(provider => ActivatorUtilities.CreateInstance<FileBasedContactRepository>(provider, contactsPath, delay));
            services.AddTransient<IBranchRepository>(provider => ActivatorUtilities.CreateInstance<FileBasedBranchRepository>(provider, branchesPath, delay));

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "CrmMicroservice.WebApi", Version = "v1"});
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CrmMicroservice.WebApi v1"));
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}