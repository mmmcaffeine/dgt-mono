using System;
using System.Reflection;
using FluentValidation.Results;

namespace Dgt.MediatR
{
    public class Response
    {
        private const BindingFlags ConstructorBindingFlags = BindingFlags.CreateInstance |
                                                             BindingFlags.Instance |
                                                             BindingFlags.Public |
                                                             BindingFlags.NonPublic;
        internal Response() { }
        internal Response(Exception exception) => Exception = exception;
        internal Response(ValidationResult validationResult) => ValidationResult = validationResult;

        public static Response<TData> Success<TData>(TData? data) => new (data);
        public static Response<TData> Empty<TData>() => new(default(TData?));

        internal static TResponse Failure<TResponse>(object failureData)
        {
            var parameterTypes = new[] {failureData.GetType()};
            var ctor = typeof(TResponse).GetConstructor(ConstructorBindingFlags, null, parameterTypes, null)!;

            return (TResponse) ctor.Invoke(new[] {failureData});
        }

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