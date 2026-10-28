# Rentify Web

Standalone Vue 3, TypeScript, and Vite frontend for the Rentify API.

## Requirements and commands

Use Node.js 20.19 or newer and npm.

```sh
npm install
npm run dev
npm run typecheck
npm run lint
npm test
npm run build
```

`npm run dev` proxies `/api` to the API's local HTTPS launch URL (`https://localhost:7246`). The proxy accepts the local development certificate. Set `VITE_API_PROXY_TARGET` in `.env.local` to use another local API address, such as `http://localhost:5277` when debugging without an HTTPS development certificate.

Copy `.env.example` to `.env` for local configuration. Keep `VITE_API_BASE_URL` empty to use the Vite proxy. For a deployed frontend, set it to the API origin, such as `https://api.example.com`, and configure that API to allow the frontend origin through CORS. Do not put credentials in Vite environment variables.

## Implemented API flows

- `POST /api/Auth/login` and `POST /api/Auth/register`
- `POST /api/Search/search` (requires authentication; backend pages at 1000 results)
- `GET /api/Search/pending` and `PUT /api/RentalListings/activate` (Admin role)
- `GET /api/RentalListings/{id}` for active listing details and gallery images
- `POST /api/RentalListings/image-uploads` and `POST /api/RentalListings`
- `PUT /api/RentalListings/reject` (Admin role; requires a rejection reason)
- `POST /api/Rents` (authenticated, idempotent) and `GET /api/Rents` (the authenticated user's paginated rental history)

The access token is kept in one local-storage entry and removed when the API returns `401`. The JWT's `role` claim controls admin navigation; the API still enforces authorization.

Public detail reads only return active listings; pending and rejected listings remain available only in the admin review flow. Rental requests can be submitted for an active listing, but there is no separate availability quote endpoint. The booking endpoint checks current price and date conflicts server-side when the rental is confirmed.

The rejection reason is stored in `rentable_products.RejectionReason`. Apply the new Infrastructure migration before starting the updated API:

```sh
dotnet ef database update --project RentifyInfrastructure --startup-project RentifyApi
```

The API does not configure CORS. The local Vite proxy avoids CORS during development; a separately hosted production frontend needs an API CORS policy for its origin. The MinIO setup already allows browser `PUT` uploads.
