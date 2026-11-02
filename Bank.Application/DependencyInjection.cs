using Bank.Application.Features.Withdrawals.Commands;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Bank.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {
            // Register command handlers
            services.AddScoped<WithdrawMoneyCommandHandler>();

            // Register FluentValidation validators
            services.AddValidatorsFromAssembly(
                typeof(DependencyInjection).Assembly);

            return services;
        }
    }
}
