﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentValidation;
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
            switch (optionsBuilder.Name)
            {
                case null:
                    throw new NotSupportedException("There is currently no way to discover all named instances of this type of options.");
                case "":
                    optionsBuilder.Services.AddTransient<IStartupFilter, ValidateOptionsStartupFilter<TOptions>>();
                    break;
                default:
                    optionsBuilder.Services.AddTransient<IStartupFilter>(provider =>
                        ActivatorUtilities.CreateInstance<ValidateOptionsStartupFilter<TOptions>>(provider, optionsBuilder.Name));
                    break;
            }

            return optionsBuilder;
        }
        
        public static OptionsBuilder<TOptions> ValidateFluentValidation<TOptions>([NotNull] this OptionsBuilder<TOptions> optionsBuilder)
            where TOptions : AbstractValidator<TOptions>
        {
            return optionsBuilder.ValidateFluentValidation<TOptions, TOptions>();
        }

        // TODO How am I going to support named options?
        public static OptionsBuilder<TOptions> ValidateFluentValidation<TOptions, TValidator>([NotNull] this OptionsBuilder<TOptions> optionsBuilder)
            where TOptions : class
            where TValidator : AbstractValidator<TOptions>
        {
            var alreadyRegistered = optionsBuilder.Services.Any(x => x.ServiceType == typeof(AbstractValidator<TValidator>));

            if (!alreadyRegistered)
            {
                optionsBuilder.Services.AddTransient<AbstractValidator<TOptions>, TValidator>();
                optionsBuilder.Services.AddTransient<IValidateOptions<TOptions>, ValidateOptionsAdapter<TOptions>>();
            }

            return optionsBuilder;
        }

        private class ValidateOptionsAdapter<TOptions> : IValidateOptions<TOptions> where TOptions : class
        {
            private readonly AbstractValidator<TOptions> _validator;

            public ValidateOptionsAdapter(AbstractValidator<TOptions> validator)
            {
                _validator = validator;
            }

            public ValidateOptionsResult Validate(string name, TOptions options)
            {
                var result = _validator.Validate(options);

                return result.IsValid
                    ? ValidateOptionsResult.Success
                    : ValidateOptionsResult.Fail(result.Errors.Select(error => error.ErrorMessage));
            }
        }
    }
}