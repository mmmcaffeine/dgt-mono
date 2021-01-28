using System;
using FluentValidation.Results;

namespace Dgt.CrmMicroservice.WebApi
{
    public class Response
    {
        // TODO You wouldn't want this getting serialized into the response!
        //      You might want to translate this into e.g. BadRequest, NotImplemented etc,
        //      but in most cases would be InternalServerError
        public Exception? Exception { get; init; }

        // TODO We could do with finding a better way of dealing with this. Ideally this is not publicly settable
        public ValidationResult? ValidationResult { get; set; }

        public bool Successful => Exception is null && (ValidationResult is null || ValidationResult.IsValid);
    }

    public class Response<TData> : Response
    {
        public TData? Data { get; init; }
    }
}