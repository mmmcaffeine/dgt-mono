using System;
using System.Linq;
using System.Net;
using Dgt.Extensions.Validation;
using Dgt.MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dgt.CrmMicroservice.WebApi
{
    public static class ResponseExtensions
    {
        private const int InternalServerError = (int) HttpStatusCode.InternalServerError;

        public static ActionResult<TData> CreateActionResult<TData>(
            this Response<TData> response,
            ControllerBase controller,
            Func<Response<TData>, ActionResult<TData>> onSuccess)
        {
            _ = response.WhenNotNull(nameof(response));
            _ = onSuccess.WhenNotNull(nameof(response));

            if (response.Successful)
            {
                // TODO This might throw, which should then be considered as an internal server error
                return onSuccess(response);
            }
            else if (response.ValidationResult is not null && response.ValidationResult.Errors.Any())
            {
                var dictionary = new ModelStateDictionary();

                foreach (var error in response.ValidationResult.Errors)
                {
                    dictionary.AddModelError(error.PropertyName, error.ErrorMessage);
                }

                return controller.ValidationProblem(dictionary);
            }
            else if(response.Exception is not null)
            {
                return controller.StatusCode(InternalServerError, response.Exception.Message);
            }
            else
            {
                // REM This is not supposed to happen! The response indicates it is not successful, but we cannot find any
                //     validation errors, or an exception so WTF actually happened?!
                return controller.StatusCode(InternalServerError);
            }
        }
    }
}