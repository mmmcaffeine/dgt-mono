using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dgt.CrmMicroservice.WebApi
{
    public class ValidationExceptionActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Nothing to do here...
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (!(context.Exception is ValidationException exception)) return;
            if (!exception.Errors.Any()) return;

            var modelState = new ModelStateDictionary();

            foreach (var error in exception.Errors)
            {
                modelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            context.Result = context.Controller is ControllerBase controller
                ? controller.ValidationProblem(modelState)
                : new BadRequestObjectResult(modelState);
            context.ExceptionHandled = true;
        }
    }
}