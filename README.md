# Bank Withdrawal Service

## Overview

This project demonstrates an improved implementation of a basic bank
account withdrawal operation.

The core functionality remains unchanged:

1.  Receive a withdrawal request.
2.  Validate the withdrawal amount.
3.  Check that the account has sufficient funds.
4.  Deduct the amount from the account balance.
5.  Record the transaction.
6.  Publish a withdrawal event to notify downstream systems.

The implementation focuses on improving maintainability, reliability,
consistency and separation of responsibilities, while keeping the
underlying business capability intact.

## Architecture and Design Patterns

A Clean Architecture implementation using layered separation, dependency inversion 
and dependency injection, with the domain and application layers independent
of infrastructure technologies

The solution is organized into four main projects:

-   **Bank.API** -- Handles HTTP requests, responses and API-level
    concerns.
-   **Bank.Application** -- Contains application workflows and
    coordinates business operations.
-   **Bank.Domain** -- Contains core business entities, rules and domain
    events.
-   **Bank.Infrastructure** -- Handles database access, external
    integrations and background processing.

The main design patterns used are:

-   Domain-Driven Design (DDD)
-   Repository Pattern
-   Unit of Work Pattern
-   Dependency Injection
-   CQRS (Command-based request handling)
-   Transactional Outbox Pattern
-   Idempotency Pattern

## Why These Choices Were Made

### 1. Domain-Driven Design

The withdrawal operation is a core business capability, so the business
rules should live within the domain rather than being spread across
controllers or database operations.

The `BankAccount` entity is responsible for enforcing the withdrawal
rules, including:

-   Ensuring the withdrawal amount is greater than zero.
-   Ensuring sufficient funds are available.
-   Updating the account balance.

This keeps business rules in one place and prevents other parts of the
application from directly modifying account balances without following
the required rules.

It also keeps the domain independent of infrastructure concerns such as
Entity Framework Core and AWS SNS.

### 2. Separation of Concerns

The original implementation handled HTTP requests, business logic,
database operations and event publishing within a single controller.

These responsibilities have been separated into dedicated layers.

Each layer has a specific responsibility, making the code easier to
maintain, understand and extend.

For example, changing the messaging provider or database implementation
should not require changes to the core withdrawal business logic.

This also makes individual components easier to work with and test
independently.

### 3. Database Transaction Management

A withdrawal involves multiple database changes that must remain
consistent.

The account balance update, withdrawal transaction record, idempotency
record and outbox event are therefore saved within a single database
transaction.

This ensures that either all changes are committed or none are.

Without this approach, it would be possible to update an account balance
successfully while failing to create the corresponding transaction or
notification record.

Optimistic concurrency control is also used to help prevent conflicting
updates when multiple withdrawal requests are processed against the same
account.

### 4. Idempotency

In a distributed environment, clients may retry requests because of
network interruptions or timeouts, even when the original request was
successfully processed.

Without protection, this could result in the same withdrawal being
processed more than once.

Idempotency keys are used to identify previously processed requests.

When a request is repeated with the same key and the same request
details, the original result is returned rather than processing another
withdrawal.

A request fingerprint is also used to prevent the same idempotency key
from being reused with different withdrawal details.

This helps prevent accidental duplicate financial transactions.

### 5. Transactional Outbox Pattern

In the original implementation, the database update and AWS SNS
publishing were performed as part of the same request flow.

This introduces a potential consistency problem if the withdrawal
succeeds but SNS is unavailable.

The Transactional Outbox Pattern addresses this by storing the event in
the database as part of the same transaction as the withdrawal.

A background worker then processes pending events and publishes them to
AWS SNS independently.

The worker also handles retries and failed messages.

This means that a temporary messaging failure does not result in a
successfully processed withdrawal losing its notification.

The implementation provides at-least-once message delivery, meaning
downstream consumers should use the event ID to handle potential
duplicate notifications.

### 6. Auditability

Financial transactions need to be traceable beyond simply knowing the
current account balance.

A separate withdrawal transaction record is created for each successful
withdrawal.

This captures information such as:

-   Transaction ID.
-   Account ID.
-   Withdrawal amount.
-   Balance before the withdrawal.
-   Balance after the withdrawal.
-   Transaction status.
-   Timestamp.
-   Correlation ID.

This provides a historical record of account activity and makes it
easier to investigate discrepancies and trace individual transactions.

The account balance represents the current state, while the transaction
records provide the history of how that state was reached.

### 7. Observability and Fault Tolerance

The implementation includes additional mechanisms to make the
application easier to monitor and more resilient to temporary failures.

These include:

-   Structured logging for troubleshooting.
-   Correlation IDs for tracing requests across components.
-   Retry policies for temporary messaging failures.
-   Exponential backoff and jitter to avoid excessive retry attempts.
-   Dead-letter handling for messages that repeatedly fail.
-   Message replay functionality for recovering failed events.
-   Outbox metrics for monitoring event processing.
-   Configurable message retention.

The background processing approach also prevents external messaging
delays from directly affecting the withdrawal request.

## Summary

The main objective of the changes was not to introduce complexity for
its own sake, but to address some of the limitations of the original
implementation.

The withdrawal functionality remains the same, but the implementation
now provides stronger transaction consistency, protection against
duplicate requests, reliable event publishing, improved auditability and
better operational visibility.

The resulting structure also makes it easier to extend the application
as additional banking functionality is introduced.
