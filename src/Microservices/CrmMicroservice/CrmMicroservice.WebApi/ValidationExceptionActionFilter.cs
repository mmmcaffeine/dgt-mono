using System;
using System.Collections.Generic;
using System.Linq;
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
            if (context.Exception is null)
            {
                return;
            }

            var validationExceptions = new List<Exception>();
            var modelState = new ModelStateDictionary();

            // TODO This is brittle, particularly with respect to the pattern matching on the message
            if (context.Exception is InvalidOperationException invalidOperationException)
            {
                validationExceptions.Add(invalidOperationException);
            }
            else if (context.Exception is AggregateException {Message: "The request failed validation."} aggregateException)
            {
                validationExceptions.AddRange(aggregateException.InnerExceptions);
            }

            if (!validationExceptions.Any())
            {
                return;
            }

            validationExceptions.ForEach(ex => modelState.AddModelError("Request", ex.Message));

            context.Result = context.Controller is ControllerBase controller
                ? controller.ValidationProblem(modelState)
                : new BadRequestObjectResult(modelState);
            context.ExceptionHandled = true;
        }
    }
}