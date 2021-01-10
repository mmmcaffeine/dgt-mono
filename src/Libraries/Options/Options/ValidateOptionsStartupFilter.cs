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
        private readonly string _name;

        public ValidateOptionsStartupFilter()
            : this(string.Empty)
        {
        }

        public ValidateOptionsStartupFilter(string name)
        {
            _name = name;
        }

        public Action<IApplicationBuilder> Configure([NotNull] Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                switch (_name)
                {
                    case null:
                        throw new NotSupportedException("There is currently no way to discover all named instances of this type of options.");
                    case "":
                        var options = builder.ApplicationServices.GetRequiredService<IOptions<TOptions>>();
                        _ = options.Value;
                        break;
                    default:
                        using (var scope = builder.ApplicationServices.CreateScope())
                        {
                            var namedOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<TOptions>>();
                            _ = namedOptions.Get(_name);
                        }

                        break;
                }

                next(builder);
            };
        }
    }
}