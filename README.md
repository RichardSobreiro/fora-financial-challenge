# Fora Financial Coding Challenge

A production-style .NET 10 Minimal API that:

- Imports company financial data from the SEC EDGAR Company Facts API
- Persists company + income history to SQL Server
- Exposes an endpoint that returns each company’s funding eligibility

## Prerequisites

- Docker Desktop
- (Optional) .NET 10 SDK for local development

## Quick start (Docker)

```bash
docker-compose up --build

# Wait for the startup import to run (first run can take a bit)
curl http://localhost:5000/api/admin/import/current

# Retrieve companies (optionally filter by name prefix)
curl "http://localhost:5000/api/companies?startsWith=U"
```

## Reset the database

If you want to wipe all persisted SQL data and re-import from scratch:

```bash
# Stop containers and remove the SQL volume
docker-compose down -v

# Start again (migrations apply automatically, then startup import runs)
docker-compose up --build
```

To re-run an import without deleting the database:

```bash
curl -X POST "http://localhost:5000/api/admin/import"
curl http://localhost:5000/api/admin/import/current
```

## API endpoints

### Companies

**GET** `/api/companies`

Optional query parameter:

- `startsWith`: returns only companies whose name starts with that string

Response format (matches the challenge requirements):

```json
[
  {
    "id": 1,
    "name": "UBER TECHNOLOGIES, INC.",
    "standardFundableAmount": 123.45,
    "specialFundableAmount": 234.56
  }
]
```

### Admin

**POST** `/api/admin/import`

Optional query parameter:

- `force=true` (reserved for future enhancement)

Returns:

- `202 Accepted` with `jobId` + status URL
- `409 Conflict` if an import is already running

**GET** `/api/admin/import/current`

Returns the current or most recent import job status.

**GET** `/api/admin/import/{jobId}`

Returns the status for a specific import job.

### Health

**GET** `/health`

## Business rules (from PDF)

### Standard fundable amount

- Must have income data for **all years 2018–2022**; otherwise **0**
- Must have **positive** income in **both 2021 and 2022**; otherwise **0**
- Use **highest** income between 2018–2022:
  - If highest income \(>= $10B\): **12.33%**
  - If highest income \(< $10B\): **21.51%**

### Special fundable amount

- Starts equal to standard fundable amount
- If company name starts with a vowel: **+15% of standard**
- If 2022 income < 2021 income: **-25% of standard**

## Next Steps

### CI/CD (Azure DevOps + Azure Key Vault)

- Set up Azure DevOps pipeline for automated builds and deployments
- Store connection strings, API keys, and secrets in Azure Key Vault
- Configure pipeline to retrieve secrets from Key Vault at deployment time
- Implement multi-stage pipeline (build → test → deploy) with approval gates

### Infrastructure as Code (Terraform)

- Define Azure App Service and Azure SQL Database resources in Terraform
- Configure App Service with proper scaling, health checks, and deployment slots
- Set up SQL Database with firewall rules, backup retention, and performance tiers
- Version control infrastructure changes and enable automated provisioning

### Integration Testing (TestContainers)

- Replace in-memory test doubles with TestContainers for real database testing
- Spin up SQL Server containers per test run for isolated, realistic integration tests
- Test actual EF Core migrations and queries against real database instances
- Ensure tests are deterministic and can run in CI/CD pipelines

### Message Queue / Job Processing

- Replace in-memory `System.Threading.Channels` queue with Azure Service Bus or Azure Queue Storage
- Implement durable message queuing for import jobs to survive application restarts
- Add dead-letter queue handling for failed jobs
- Enable horizontal scaling of background workers across multiple instances

### Distributed Locking

- Replace SQL-based distributed lock with Azure Blob Lease or Redis distributed locks
- Implement lock renewal mechanism to prevent premature expiration during long-running imports
- Add lock timeout and retry logic for better resilience across distributed instances

### Security & Authorization

- Implement authentication (Azure AD / OAuth2) for admin endpoints
- Enable HTTPS-only communication and security headers (HSTS, CSP)
- Implement API key authentication or rate limiting for public endpoints
- Add input validation and sanitization to prevent injection attacks

### Observability & Monitoring

- Integrate Application Insights for distributed tracing, metrics, and logging
- Add custom metrics for import job duration, success rates, and queue depth
- Set up alerting for failed imports, high error rates, and performance degradation
- Implement structured logging with correlation IDs for request tracing
- Create dashboards for system health, import status, and business metrics

### Performance & Scalability

- Add Redis caching for frequently accessed company data
- Implement database query optimization (indexes, query hints, connection pooling)
- Add response compression and HTTP/2 support
- Configure App Service auto-scaling based on CPU/memory metrics
- Consider read replicas for SQL Database to offload read queries
- Implement pagination for `/api/companies` endpoint to handle large result sets
