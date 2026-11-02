using Bank.Application.Exceptions;
using Bank.Application.Features.Withdrawals.DTOs;
using Bank.Application.Features.Withdrawals.Idempotency;
using Bank.Application.Interfaces;
using Bank.Domain.Entities;
using Bank.Domain.Events;
using FluentValidation;
using System.Diagnostics;
using System.Text.Json;

namespace Bank.Application.Features.Withdrawals.Commands
{
    public sealed class WithdrawMoneyCommandHandler(
        IValidator<WithdrawMoneyCommand> validator,
        IAccountRepository accountRepository,
        IWithdrawalRepository withdrawalRepository,
        IOutboxRepository outboxRepository,
        IIdempotencyRepository idempotencyRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        public async Task<WithdrawalResult> Handle(
            WithdrawMoneyCommand command,
            CancellationToken cancellationToken)
        {
            //Validate command.
            await validator.ValidateAndThrowAsync(command, cancellationToken);

            //Generate request fingerprint.
            var requestHash = WithdrawalRequestHasher.Create(
                command.AccountId,
                command.Amount);

            //Begin transaction.
            await using var transaction =
                await unitOfWork.BeginTransactionAsync(
                    cancellationToken);

            //Check existing idempotency record.
            var existingRequest =
                await idempotencyRepository.GetAsync(
                    command.AccountId,
                    command.IdempotencyKey,
                    cancellationToken);

            if (existingRequest is not null)
            {
                //Verify request payload.
                if (!string.Equals(
                    existingRequest.RequestHash,
                    requestHash,
                    StringComparison.Ordinal))
                {
                    throw new IdempotencyKeyReuseException();
                }

                //Return original withdrawal.
                await transaction.CommitAsync(
                    cancellationToken);

                return existingRequest.Result;
            }

            //Retrieve bank account.
            var account = await accountRepository.GetByIdAsync(
                command.AccountId,
                cancellationToken);

            if (account is null)
                throw new AccountNotFoundException(
                    command.AccountId);

            //Capture balance before withdrawal.
            var balanceBefore = account.Balance;

            //Execute domain operation.
            var withdrawalEvent = account.Withdraw(
                command.Amount, timeProvider.GetUtcNow());

            var balanceAfter = account.Balance;

            //Get correlation ID.
            var correlationId =
                Activity.Current?.TraceId.ToString()
                ?? Guid.NewGuid().ToString();

            var occurredAt = withdrawalEvent.OccurredAt;

            //Create permanent financial record.
            var withdrawal = WithdrawalTransaction.Create(
                withdrawalEvent.EventId,
                account.Id,
                withdrawalEvent.Amount,
                balanceBefore,
                balanceAfter,
                occurredAt,
                correlationId);

            //Register financial transaction.
            await withdrawalRepository.AddAsync(
                withdrawal,
                cancellationToken);

            //Register idempotency record.
            await idempotencyRepository.AddAsync(
                account.Id,
                command.IdempotencyKey,
                requestHash,
                withdrawal.Id,
                timeProvider.GetUtcNow(),
                cancellationToken);

            //Create outbox event.
            var payload = JsonSerializer.Serialize(
                withdrawalEvent);

            await outboxRepository.AddAsync(
                withdrawalEvent.EventId,
                nameof(WithdrawalCompletedEvent),
                payload,
                occurredAt,
                correlationId,
                cancellationToken);

            //Save all changes.
            await unitOfWork.SaveChangesAsync(
                cancellationToken);

            //Commit atomic transaction.
            await transaction.CommitAsync(
                cancellationToken);

            //Return result.
            return new WithdrawalResult(
                withdrawal.Id,
                withdrawal.AccountId,
                withdrawal.Amount,
                withdrawal.BalanceAfter);
        }
    }

}
