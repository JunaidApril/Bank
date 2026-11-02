# Withdrawal Assessment – Problem Analysis & Solutions

## Overview

The original withdrawal implementation had several weaknesses around transaction consistency, concurrency, validation and event publishing.

I reviewed the existing flow and focused on addressing the risks that could affect the accuracy of account balances, the reliability of transactions and the ability to recover from failures.

The following outlines the 12 main problems identified and the changes made to address them.

## Problems Identified and Resolved

### 1. SNS notification was unreachable

**Problem:** The withdrawal logic returned before reaching the SNS publishing code, meaning successful withdrawals would never trigger the notification.

**Solution:** Introduced a transactional outbox. The withdrawal event is saved alongside the database changes and published by a background worker. This ensures the event is not lost simply because the publishing code is never reached.

### 2. No database transaction

**Problem:** The account balance and related operations were not protected by a single transaction. A failure partway through could leave the system in an inconsistent state.

**Solution:** Used the Unit of Work to commit the balance update, withdrawal record, idempotency record and outbox event together. Either all changes succeed or none are committed.

### 3. Race conditions during withdrawals

**Problem:** Two concurrent withdrawal requests could read the same account balance and both attempt to withdraw against it.

**Solution:** Implemented optimistic concurrency using an account version field. Combined with database constraints, this helps prevent concurrent updates from resulting in an invalid account balance.

### 4. Missing amount validation

**Problem:** The original implementation did not prevent zero or negative withdrawal amounts. A negative amount could effectively increase the account balance.

**Solution:** Added request validation and domain-level checks to ensure withdrawal amounts are positive. Monetary precision is also validated to keep amounts consistent with the database's decimal precision.

### 5. No explicit account-not-found handling

**Problem:** A request for an account that does not exist could result in an unhandled database exception rather than a meaningful API response.

**Solution:** Introduced a dedicated account-not-found exception and centralised exception handling so the API can return a consistent and appropriate error response.

### 6. Balance update was not sufficiently protected

**Problem:** The original update did not provide a database-level safeguard against withdrawing more than the available balance.

**Solution:** Added optimistic concurrency handling and a database constraint preventing negative balances. This provides an additional layer of protection beyond application-level validation.

### 7. Manual JSON construction

**Problem:** Building SNS messages manually as strings made the event format fragile and difficult to extend or maintain.

**Solution:** Replaced manual JSON construction with structured event models and `System.Text.Json` serialisation. This makes the event easier to maintain and reduces formatting errors. The updated event structure should remain compatible with any existing consumers.

### 8. Hardcoded infrastructure dependencies

**Problem:** The original withdrawal logic was responsible for creating and configuring the SNS client. This tightly coupled business logic to AWS infrastructure.

**Solution:** Moved infrastructure configuration into dependency injection and external application settings. This keeps the application logic independent of the messaging provider and makes the implementation easier to configure and test.

### 9. SNS publishing blocked the request

**Problem:** Publishing directly to SNS during the withdrawal request meant an external service failure or delay could affect the API response.

**Solution:** Moved event publishing into a background outbox processor. The withdrawal can complete once the database transaction succeeds, without waiting for SNS to respond.

### 10. No retry or recovery mechanism

**Problem:** If SNS publishing failed, there was no reliable mechanism to retry the notification or recover messages that remained unpublished.

**Solution:** Added retry handling with back-off, dead-letter support and replay capability. Processing metrics also provide visibility into failures and message delivery. Since delivery is at least once, consumers should use the event ID to prevent duplicate processing.

### 11. No withdrawal audit trail

**Problem:** Updating only the account balance meant there was no proper record of individual withdrawals or how the balance changed.

**Solution:** Introduced a withdrawal transaction record containing the amount, balance before and after the withdrawal, timestamp and correlation ID. This provides a transaction history that can support investigation and reconciliation.

### 12. Weak API responses and error handling

**Problem:** The original implementation relied on basic string responses and did not provide consistent handling of expected and unexpected errors.

**Solution:** Introduced structured request and response models, dedicated exceptions and centralised Problem Details responses. This gives API consumers clearer feedback and keeps error handling consistent. The current `201 Created` response should be changed to `200 OK` unless a corresponding GET endpoint is implemented for the created withdrawal resource.

## Conclusion

The changes focus on making the withdrawal process more reliable and predictable, particularly when requests are repeated, transactions happen concurrently or external services fail.

Rather than treating the withdrawal as a simple balance update, the implementation considers the full transaction lifecycle: validating the request, protecting the account balance, recording the transaction and ensuring that related events can be delivered and recovered independently.

The intention was to introduce enough structure to address the risks of a financial transaction without adding complexity that does not serve a clear purpose.