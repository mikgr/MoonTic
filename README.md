# MoonTic

Event ticketing: create events, mint and sell tickets, usher check-in. ASP.NET Core (Razor Pages) with DynamoDB and blockchain integration.

## Run locally

```bash
cd backend
dotnet run --project Ticketer.Web
```

See [backend/README.md](backend/README.md) for Docker and deployment.

## Smart contract

Solidity ticket contract (Foundry). **Ticket.sol** — per-event: mint, transfer, check-in/check-out, resale asks (USDC). Tests in `test/`, scripts in `script/`.

```bash
cd smart-contract
forge build
forge test
```

## Structure

**Backend** (in `backend/`)

- **Ticketer.Web** — Web app (events, tickets, organizer, usher)
- **Ticketer.Cli** — Setup and seeding
- **Ticketer.Model** — Domain types
- **Ticketer.Repository** — DynamoDB persistence
- **Ticketer.UseCases** — Business logic (mint, buy, transfer, check-in)

**Smart contract** (in `smart-contract/`)

- **Ticket.sol** — Per-event on-chain logic
