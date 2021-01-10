using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dgt.Options
{
    public class ValidateOptionsStartupFilter<TOptions> : IStartupFilter where TOptions : class
    {
        public Action<IApplicationBuilder> Configure([NotNull] Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                var options = builder.ApplicationServices.GetRequiredService<IOptions<TOptions>>();
                _ = options.Value;

                next(builder);
            };
        }
    }
}