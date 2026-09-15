# Research: Document Upload and Management

**Feature**: [spec.md](spec.md)  
**Date**: 2026-09-15

## Decision 1: Use an asynchronous scan worker in production

- **Decision**: Publish a versioned scan request to Azure Queue Storage after a private upload and metadata commit. Process it with an Azure Function using a Queue Storage trigger.
- **Rationale**: Virus scanning can be slow or dependent on external infrastructure. Queue-triggered processing keeps the upload request responsive, supports retry and poison-message handling, and allows the scanner to scale independently.
- **Alternatives considered**: Synchronous scanning inside the Blazor request was rejected because it increases upload latency and couples user experience to scanner availability. A hosted worker polling the database was rejected because Queue Storage provides durable delivery and simpler burst handling for this production path.

## Decision 2: Keep files unavailable until a clean result

- **Decision**: New production documents start in `Pending` state and cannot be previewed or downloaded. Only an idempotent clean scan result changes them to available.
- **Rationale**: This is the secure default and prevents a race where users access a file before scanning finishes.
- **Alternatives considered**: Making files available while scanning was rejected because it violates the requirement that files be scanned before access.

## Decision 3: Use a transactional boundary plus recovery for queue publication

- **Decision**: Save private file and pending metadata before enqueueing. Use an outbox or reconciliation record so a queue publication failure leaves a retryable pending document rather than an accessible unscanned document.
- **Rationale**: A database transaction cannot atomically commit a filesystem/blob write and Queue Storage message. Explicit pending state and retryable publication make the failure visible and recoverable.
- **Alternatives considered**: Enqueueing before metadata persistence risks messages that cannot be resolved. Marking the document available before queue success is unsafe.

## Decision 4: Separate offline training validation from production malware scanning

- **Decision**: The training runtime uses deterministic file type, size, and extension validation and remains free of Azure dependencies. Production uses the same scan-state and interface boundary with an Azure Function and configured scanner adapter.
- **Rationale**: This satisfies the accepted clarification and keeps local ARM64/offline setup simple while preserving a credible production deployment design.
- **Alternatives considered**: Requiring Azure services locally would violate offline constraints. Calling extension checks malware scanning would misrepresent the training implementation.

## Decision 5: Make queue messages safe and idempotent

- **Decision**: Queue messages carry document ID, opaque storage identifier, content type, size, schema version, and correlation ID; they exclude file bytes and physical paths. The worker records a single terminal outcome for each document/version.
- **Rationale**: Small messages are more reliable and reduce data exposure. Idempotency is required because Queue Storage may deliver a message more than once.
- **Alternatives considered**: Embedding file content was rejected due to message size, cost, and sensitive-data exposure. Assuming exactly-once delivery was rejected because queue processing is at-least-once.
