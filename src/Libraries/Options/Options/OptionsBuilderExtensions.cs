using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dgt.Options
{
    public static class OptionsBuilderExtensions
    {
        public static OptionsBuilder<TOptions> ValidateAtStartup<TOptions>([NotNull] this OptionsBuilder<TOptions> optionsBuilder)
            where TOptions : class
        {
            optionsBuilder.Services.AddTransient<IStartupFilter, ValidateOptionsStartupFilter<TOptions>>();

            return optionsBuilder;
        }
    }
}