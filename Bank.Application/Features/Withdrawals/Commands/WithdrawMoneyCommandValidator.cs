using FluentValidation;

namespace Bank.Application.Features.Withdrawals.Commands
{
    public sealed class WithdrawMoneyCommandValidator
        : AbstractValidator<WithdrawMoneyCommand>
    {
        public WithdrawMoneyCommandValidator()
        {
            RuleFor(x => x.AccountId)
                .NotEmpty()
                .WithMessage("Account ID is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage(
                    "Withdrawal amount must be greater than zero.");

            RuleFor(x => x.Amount)
                .PrecisionScale(19, 4, false)
                .WithMessage(
                    "Withdrawal amount supports a maximum of 4 decimal places.");

            RuleFor(x => x.IdempotencyKey)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}
