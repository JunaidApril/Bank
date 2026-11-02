using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Bank.Application.Features.Withdrawals.Idempotency
{
    public static class WithdrawalRequestHasher
    {
        private const string FingerprintVersion = "v1";

        public static string Create(
            Guid accountId,
            decimal amount)
        {
            if (accountId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Account ID cannot be empty.",
                    nameof(accountId));
            }

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(amount));

            if (decimal.Round(amount, 4) != amount)
                throw new ArgumentException(
                    "Amount supports a maximum of 4 decimal places.",
                    nameof(amount));

            var canonicalAmount = amount.ToString(
                "0.0000",
                CultureInfo.InvariantCulture);

            var payload = string.Join(
                "|",
                FingerprintVersion,
                "WITHDRAWAL",
                accountId.ToString("D"),
                canonicalAmount);

            var hash = SHA256.HashData(
                Encoding.UTF8.GetBytes(payload));

            return Convert.ToHexString(hash);
        }
    }
}
