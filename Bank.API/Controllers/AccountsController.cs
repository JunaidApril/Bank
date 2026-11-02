using Bank.API.Contracts.Requests;
using Bank.API.Contracts.Responses;
using Bank.Application.Features.Withdrawals.Commands;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Bank.API.Controllers
{
    [ApiController]
    [Route("api/v1/accounts")]
    public sealed class AccountsController(
        WithdrawMoneyCommandHandler withdrawHandler)
        : ControllerBase
    {
        [HttpPost("{accountId:guid}/withdrawals")]
        [ProducesResponseType(
            typeof(WithdrawResponse),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(
            StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Withdraw(
            Guid accountId,
            [FromBody] WithdrawRequest request,
            [FromHeader(Name = "Idempotency-Key")]
            [Required]
            string idempotencyKey,
            CancellationToken cancellationToken)
        {
            var command = new WithdrawMoneyCommand(
                accountId,
                request.Amount,
                idempotencyKey);

            var result = await withdrawHandler.Handle(
                command,
                cancellationToken);

            var response = new WithdrawResponse(
                result.WithdrawalId,
                result.AccountId,
                result.Amount,
                result.RemainingBalance);

            return Ok(response);
        }
    }
}
