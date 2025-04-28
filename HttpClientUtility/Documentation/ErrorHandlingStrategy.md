# HttpClientUtility Error Handling & Logging Strategy

## Overview

This document describes the standardized error handling and logging strategies implemented throughout the HttpClientUtility library. Following these patterns consistently ensures proper error traceability, meaningful error messages, and appropriate handling of different failure scenarios.

## Core Principles

1. **Correlation IDs**: Every request is assigned a unique correlation ID that flows through all components for end-to-end traceability.
2. **Context Enrichment**: Exceptions and logs are enriched with contextual information to aid in troubleshooting.
3. **Appropriate Log Levels**: Different types of errors are logged at appropriate levels (Warning, Error, Critical) based on severity.
4. **Sanitized Logging**: Sensitive information is automatically redacted from logs.
5. **Standardized Error Responses**: Errors are mapped to appropriate HTTP status codes with descriptive messages.
6. **Request/Response Metrics**: Timing information is captured for performance analysis.

## Logging Standards

### Log Levels

The library follows these log level standards:

- **Trace**: Protocol-level details (rarely used)
- **Debug**: Detailed flow information, request/response payloads (in debug mode)
- **Information**: Normal operational events (requests starting/completing successfully)
- **Warning**:
  - 4xx client errors
  - Transient failures that are handled by retry logic
  - Canceled operations
  - Cache misses or timeouts
- **Error**:
  - 5xx server errors
  - Network failures
  - Configuration issues
  - Data corruption
- **Critical**:
  - Unrecoverable system failures
  - Out of memory conditions
  - Unexpected errors that prevent further operation

### Standard Log Format

Log entries should include:

1. Timestamp (added by logging provider)
2. Log level
3. Source context (class/method)
4. Correlation ID
5. Operation name
6. Message with structured property placeholders
7. Exception details (if applicable)

Example:

```
[2025-04-28 09:15:30.123] [Information] [HttpClientService] HTTP GET request to https://api.example.com/users completed with status 200 in 123ms [CorrelationId: a1b2c3d4]
```

## Error Handling Strategies

### HTTP Client Errors

The library implements these HTTP client error handling strategies:

1. **Network Errors (HttpRequestException)**:
   - Logged as errors with full context (URL, method, correlation ID)
   - Mapped to `ServiceUnavailable` (503) if no status code provided
   - Request context preserved for traceability

2. **Timeouts (TaskCanceledException / OperationCanceledException)**:
   - Logged as warnings with timing information
   - Mapped to `RequestTimeout` (408)
   - Added to error list with details

3. **Validation Errors (ArgumentException)**:
   - Logged as errors with parameter information
   - Mapped to `BadRequest` (400)
   - Descriptive error messages for client debugging

4. **Server Errors (5xx responses)**:
   - Logged as errors with response details
   - Original status code preserved
   - Response content included for debugging

5. **Client Errors (4xx responses)**:
   - Logged as warnings with response details
   - Original status code preserved
   - Error message includes response reason

### Resilience Patterns

The library uses the following resilience patterns:

1. **Retry Logic**:
   - Automatic retry for transient failures
   - Exponential backoff between attempts
   - Configurable max retry count
   - Each retry logged with attempt count and timing

2. **Circuit Breaker**:
   - Prevents cascading failures when services are unavailable
   - Logs circuit state changes (open, closed, half-open)
   - Threshold and duration configurable
   - Status tracked for monitoring

3. **Caching**:
   - Fallback to cached responses when available
   - Cache errors handled separately from HTTP errors
   - Cache hits/misses logged
   - Cache errors don't prevent operation if original request succeeds

### Fire-and-Forget Operations

For asynchronous fire-and-forget operations:

1. **Task Tracking**:
   - All tasks assigned correlation IDs for tracing
   - Start/completion logged
   - Timing captured for performance analysis

2. **Error Containment**:
   - Exceptions never propagate outside the continuation
   - All errors logged with full context
   - Multiple inner exceptions handled individually

3. **Cancellation Support**:
   - Clean cancellation pathway
   - Cancellations logged but not treated as errors
   - Resources properly disposed

## Exception Enrichment

When exceptions occur, they are enriched with:

1. Original exception preserved as InnerException
2. Context about the operation (method, URL, parameters)
3. Correlation ID for traceability
4. Timing information
5. Component-specific details (cache state, retry count, etc.)

## Implementation Details

### Core Components

1. **LoggingUtility**:
   - Standardized logging methods
   - URL sanitization to prevent sensitive data exposure
   - Log level determination based on context
   - Structured logging helpers

2. **ErrorHandlingUtility**:
   - Exception enrichment
   - Status code mapping
   - Error response creation
   - Context preservation

3. **HttpRequestResultBase**:
   - Correlation ID generation and tracking
   - Request context dictionary
   - Error list management
   - Timing capture

### Using the Error Handling Utilities

When implementing new functionality in this library:

1. **For synchronous operations**:

   ```csharp
   try 
   {
       // Operation code
   }
   catch (Exception ex)
   {
       // Use standard error handling
       ErrorHandlingUtility.LogException(ex, _logger, "Operation Name", correlationId);
       throw ErrorHandlingUtility.EnrichException(ex, "Context message", contextData);
   }
   ```

2. **For asynchronous operations**:

   ```csharp
   return await ErrorHandlingUtility.ExecuteWithErrorHandlingAsync(
       async () => {
           // Async operation
           return result;
       },
       _logger,
       "Operation Name",
       correlationId,
       contextData
   ).ConfigureAwait(false);
   ```

3. **For fire-and-forget operations**:

   ```csharp
   _fireAndForgetUtility.SafeFireAndForget(
       asyncTask, 
       "Operation Name",
       cancellationToken);
   ```

## Best Practices

1. **Always pass the CancellationToken**: Flow cancellation tokens through all async methods.
2. **Use ConfigureAwait(false)**: Prevent deadlocks in UI contexts.
3. **Include correlation IDs**: Include in error messages and logs for traceability.
4. **Sanitize sensitive data**: Use `LoggingUtility.SanitizeUrl()` for URLs with query parameters.
5. **Add context to exceptions**: Provide meaningful context in exception messages.
6. **Use appropriate log levels**: Follow the log level guidelines.
7. **Include timing information**: Log request durations for performance analysis.
8. **Document error handling strategies**: Comment non-obvious error handling approaches.

## Error Response Format

Standard error responses include:

1. HTTP status code appropriate to the error
2. Error message describing the issue
3. Correlation ID for support reference
4. Timestamp of the error

Example:

```json
{
  "error": "Unable to retrieve user data",
  "status": 503,
  "correlationId": "a1b2c3d4",
  "timestamp": "2025-04-28T09:15:30.123Z",
  "details": "Service temporarily unavailable"
}
```

## Troubleshooting Guide

1. **Request tracing**: Use the correlation ID to trace the request through logs.
2. **Performance issues**: Check elapsed time metrics in logs.
3. **Intermittent failures**: Look for circuit breaker and retry patterns.
4. **Configuration issues**: Check for argument exceptions in startup logs.
5. **Memory leaks**: Monitor resource usage metrics.

## Conclusion

By following these standardized logging and error handling patterns, the library provides consistent, traceable, and meaningful error information throughout all operations. This approach simplifies debugging, improves reliability, and enhances the overall user experience when working with the library.
