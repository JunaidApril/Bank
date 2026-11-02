using Bank.Application.Exceptions;
using Bank.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Bank.API.Middleware
{
    public sealed class GlobalExceptionHandler(
     IProblemDetailsService problemDetailsService,
     ILogger<GlobalExceptionHandler> logger)
     : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (status, title) = exception switch
            {
                ValidationException =>
                    (StatusCodes.Status400BadRequest,
                     "Validation failed"),

                AccountNotFoundException =>
                    (StatusCodes.Status404NotFound,
                     "Account not found"),

                InsufficientFundsException =>
                    (StatusCodes.Status422UnprocessableEntity,
                     "Insufficient funds"),

                InvalidWithdrawalException =>
                    (StatusCodes.Status400BadRequest,
                     "Invalid withdrawal"),

                ConcurrencyConflictException =>
                    (StatusCodes.Status409Conflict,
                     "Account modification conflict"),

                IdempotencyKeyReuseException =>
                    (StatusCodes.Status409Conflict,
                     "Idempotency key conflict"),

                _ =>
                    (StatusCodes.Status500InternalServerError,
                     "An unexpected error occurred")
            };

            if (status >= 500)
            {
                logger.LogError(
                    exception,
                    "Unhandled exception processing request.");
            }
            else
            {
                logger.LogWarning(
                    "Request failed with status {StatusCode}: {Error}",
                    status,
                    exception.Message);
            }

            context.Response.StatusCode = status;

            var problemDetails = new ProblemDetails
            {
                Status = status,
                Title = title
            };

            if (exception is ValidationException validationException)
            {
                problemDetails.Extensions["errors"] =
                    validationException.Errors
                        .GroupBy(x => x.PropertyName)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Select(e => e.ErrorMessage)
                                .ToArray());
            }

            return await problemDetailsService.TryWriteAsync(
                new ProblemDetailsContext
                {
                    HttpContext = context,
                    ProblemDetails = problemDetails
                });
        }
    }
}
