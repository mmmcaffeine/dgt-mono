using System;
using FluentValidation.Results;

namespace Dgt.CrmMicroservice.WebApi
{
    public class Response
    {
        protected Response() { }
        internal Response(Exception exception) => Exception = exception;
        internal Response(ValidationResult validationResult) => ValidationResult = validationResult;

        public static Response<TData> Success<TData>(TData? data) => new (data);

        // TODO You wouldn't want this getting serialized into the response!
        //      You might want to translate this into e.g. BadRequest, NotImplemented etc,
        //      but in most cases would be InternalServerError
        public Exception? Exception { get; }
        public ValidationResult? ValidationResult { get; }

        public bool Successful => Exception is null && (ValidationResult is null || ValidationResult.IsValid);
    }

    public class Response<TData> : Response
    {
        internal Response(TData? data) => Data = data;
        internal Response(Exception exception) : base(exception) { }
        internal Response(ValidationResult validationResult) : base(validationResult) { }

        public TData? Data { get; }
    }
}