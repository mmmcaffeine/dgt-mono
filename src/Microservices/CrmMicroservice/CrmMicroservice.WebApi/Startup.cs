using Dgt.Caching;
using Dgt.CrmMicroservice.Domain;
using Dgt.CrmMicroservice.Infrastructure.Caching;
using Dgt.CrmMicroservice.Infrastructure.FileBased;
using Dgt.CrmMicroservice.WebApi.Operations.Contacts;
using Dgt.CrmMicroservice.WebApi.PipelineBehaviors;
using Dgt.Options;
using FluentValidation;
using MediatR;
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
            services.AddOptions<FileBasedRepositoryOptions>(FileBasedRepositoryOptions.ContactRepository)
                .BindConfiguration($"{FileBasedRepositoryOptions.Repositories}:{FileBasedRepositoryOptions.ContactRepository}")
                .ValidateFluentValidation()
                .ValidateAtStartup();
            services.AddOptions<FileBasedRepositoryOptions>(FileBasedRepositoryOptions.BranchRepository)
                .BindConfiguration($"{FileBasedRepositoryOptions.Repositories}:{FileBasedRepositoryOptions.BranchRepository}")
                .ValidateFluentValidation()
                .ValidateAtStartup();

            // ENHANCE Some method on IServiceCollection where we can bind to a type, but with a decorator on it
            services.AddTransient<FileBasedContactRepository>();
            services.AddTransient<IContactRepository>(provider =>
                ActivatorUtilities.CreateInstance<CachingContactRepositoryDecorator>(provider, provider.GetRequiredService<FileBasedContactRepository>()));
            services.AddTransient<IBranchRepository, FileBasedBranchRepository>();
            services.AddTransient<ITypedCache, TypedCache>();

            services.AddMediatR(GetType());
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CatchUnhandledExceptionsPipelineBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidateRequestPipelineBehavior<,>));
            services.AddTransient<IValidator<CreateContactCommand.Request>, CreateContactCommand.RequestValidator>();
            services.AddTransient<IValidator<GetContactsByNameQuery.Request>, GetContactsByNameQuery.RequestValidator>();

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = _configuration.GetConnectionString("Redis");
                options.InstanceName = "crm:";
            });

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