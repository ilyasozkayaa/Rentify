# Rentify

Rentify is a rental marketplace with a Vue web client and a .NET API. The current application supports account registration and sign-in, natural-language rental search, listing publication and admin review, listing images, and rental requests with a user's rental history.

## Solution layout

The solution is [`Rentify.slnx`](Rentify.slnx):

| Project | Responsibility |
| --- | --- |
| [`RentifyApi`](RentifyApi/) | ASP.NET Core API, HTTP controllers, authentication, API documentation, and middleware. |
| [`RentifyApplication`](RentifyApplication/) | Application commands and queries, validation, use cases, and service/repository contracts. |
| [`RentifyDomain`](RentifyDomain/) | Core entities and domain enums. |
| [`RentifyInfrastructure`](RentifyInfrastructure/) | PostgreSQL persistence and migrations, repositories, Redis, MinIO image storage, and background/outbox processing. |
| [`RentifyUnitTests`](RentifyUnitTests/) | xUnit tests for application handlers, validation, and observability. |
| [`RentifyWeb`](RentifyWeb/) | Vue 3, TypeScript, and Vite frontend. See the [frontend README](RentifyWeb/README.md) for its current API flows. |

## Prerequisites

- .NET 10 SDK
- Node.js 20.19 or newer and npm
- Docker with Docker Compose v2
- An OpenAI API key for natural-language search
- The .NET Entity Framework CLI (`dotnet-ef`) for database migrations

## Run locally

Run the following commands from the repository root.

### 1. Start local dependencies

```sh
docker compose up -d
```

The Compose file starts PostgreSQL, Redis, Elasticsearch, Kibana, MinIO and its bucket/CORS initializer, and pgAdmin. The local service ports are PostgreSQL `5432`, Redis `6379`, Elasticsearch `9200`, Kibana `5601`, MinIO API `9000`, MinIO console `9001`, and pgAdmin `5050`. These Compose settings are for local development; do not reuse development credentials in a deployed environment.

### 2. Configure API secrets

The API reads configuration through .NET configuration providers. Use User Secrets for local-only values rather than putting credentials in tracked settings files. From the repository root, set the values required by your local environment:

```sh
dotnet user-secrets set "Jwt:Key" "<random-key-at-least-32-bytes>" --project RentifyApi
dotnet user-secrets set "OpenAI:ApiKey" "<your-openai-api-key>" --project RentifyApi
dotnet user-secrets set "ObjectStorage:Bucket" "rentify-images" --project RentifyApi
dotnet user-secrets set "ConnectionStrings:PostgreConnection" "<local-postgresql-connection-string>" --project RentifyApi
```

Replace each angle-bracketed placeholder with a value for your local environment. `Jwt:Key` must be at least 32 bytes. The development settings provide the local MinIO endpoint and credentials; override them with User Secrets or environment variables if your local setup differs. `ConnectionStrings:Redis` and `ElasticSearch:Uri` default to the Compose service addresses and can also be overridden.

### 3. Apply database migrations

If `dotnet-ef` is not installed, install the EF Core 10 tool:

```sh
dotnet tool install --global dotnet-ef --version 10.0.11
```

Then apply the migrations:

```sh
dotnet ef database update --project RentifyInfrastructure --startup-project RentifyApi
```

### 4. Start the API

```sh
dotnet run --project RentifyApi --launch-profile "Rentify Development"
```

The development profile serves HTTPS at `https://localhost:7246` and HTTP at `http://localhost:5277`. In Development, the API exposes OpenAPI and Scalar at `https://localhost:7246/scalar`.

### 5. Start the web client

In a second terminal from the repository root:

```sh
cd RentifyWeb
npm ci
npm run dev
```

Vite proxies `/api` to `https://localhost:7246` by default. To override the frontend environment locally, copy `.env.example` to `.env.local` and set `VITE_API_PROXY_TARGET`. `VITE_API_BASE_URL` is normally left empty for local development so requests use the Vite proxy. Do not put credentials in frontend environment variables.

## Configuration reference

| Setting | Purpose |
| --- | --- |
| `ConnectionStrings:PostgreConnection` | PostgreSQL connection used by EF Core. |
| `ConnectionStrings:Redis` | Redis connection used for cache and idempotency storage. |
| `Jwt:Key` | Signing key for API tokens; must be at least 32 bytes. |
| `OpenAI:ApiKey` | API key used by natural-language search. |
| `OpenAI:Model` | Search model name; configured in `RentifyApi/appsettings.json`. |
| `ObjectStorage:Endpoint`, `AccessKey`, `SecretKey`, `Bucket` | MinIO endpoint and bucket configuration for listing images. |
| `ElasticSearch:Uri` | Elasticsearch endpoint used for API log output. |
| `VITE_API_BASE_URL` | Optional API origin for a separately hosted frontend. Leave empty to use the Vite proxy locally. |
| `VITE_API_PROXY_TARGET` | Local API address used by the Vite `/api` proxy. |

The backend settings are in [`RentifyApi/appsettings.json`](RentifyApi/appsettings.json) and [`RentifyApi/appsettings.Development.json`](RentifyApi/appsettings.Development.json). The frontend example is [`RentifyWeb/.env.example`](RentifyWeb/.env.example). Keep real credentials, tokens, and production secrets outside the repository.

The API does not configure CORS. The local Vite proxy avoids a browser cross-origin request during development. A separately hosted frontend requires the API to allow its origin.

## Current API flows

The implemented frontend flows include:

- `POST /api/Auth/register` and `POST /api/Auth/login`
- `POST /api/Search/search` for authenticated rental search
- `GET /api/Search/pending`, `PUT /api/RentalListings/activate`, and `PUT /api/RentalListings/reject` for admin listing review
- `POST /api/RentalListings/image-uploads` and `POST /api/RentalListings` for image upload and listing publication
- `GET /api/RentalListings/{id}` for active listing details
- `POST /api/Rents` and `GET /api/Rents` for rental requests and the authenticated user's rental history

See [`RentifyWeb/README.md`](RentifyWeb/README.md) for additional behavior notes, including authorization and booking constraints.

## Checks

From the repository root, run backend tests with:

```sh
dotnet test Rentify.slnx
```

From `RentifyWeb`, run frontend checks:

```sh
npm test
npm run typecheck
npm run lint
npm run build
```

## Stop local services

From the repository root:

```sh
docker compose down
```

This stops the containers while keeping the named data volumes. Do not add `--volumes` unless you intend to delete local database and storage data.
