# LegacyOS MVP Acceptance Test

Run against a disposable PostgreSQL database and Stripe test mode. Never use production data or real cards.

## Configuration

Create the gitignored `backend/LegacyOS.Api/appsettings.Development.json` from the example and configure:

- `ConnectionStrings:LegacyOS`
- `BootstrapStaff:Email`
- `BootstrapStaff:Password`
- `Frontend:BaseUrl`
- `Stripe:SecretKey` using an `sk_test_` key
- `Stripe:WebhookSecret` using the endpoint's `whsec_` secret
- `Cors:AllowedOrigins:0` as `http://localhost:5173`

## Automated checks

From the repository root:

```bash
dotnet build backend/LegacyOS.Api/LegacyOS.Api.csproj --no-restore
dotnet ef migrations has-pending-model-changes --project backend/LegacyOS.Api/LegacyOS.Api.csproj --no-build --no-color
dotnet run --project backend/LegacyOS.Api.Tests/LegacyOS.Api.Tests.csproj --no-restore

cd frontend
npm ci
npm run build
./node_modules/.bin/eslint src/features/auth src/pages/Portal src/pages/UsaWrestling src/api/http.ts src/App.tsx src/routes.tsx src/components/navigation/Sidebar.tsx
cd ..
```

Expected: both builds succeed, EF reports no pending model changes, every security check passes, and ESLint reports no errors for changed frontend files.

## Start locally

API terminal:

```bash
dotnet run --project backend/LegacyOS.Api/LegacyOS.Api.csproj --launch-profile http
```

Frontend terminal:

```bash
cd frontend
npm run dev
```

Verify `http://localhost:5021/health` reports that the API and database are connected, then open `http://localhost:5173/portal/login`.

## Stripe webhook forwarding

```bash
stripe listen \
  --events checkout.session.completed,checkout.session.async_payment_succeeded,checkout.session.async_payment_failed,checkout.session.expired \
  --forward-to http://localhost:5021/stripe/webhook
```

Put the CLI's `whsec_` value in development configuration and restart the API.

## Staff and account tests

1. Sign in with the bootstrap Staff account.
2. Create a Guardian invitation through `POST /staff/guardian-invitations`.
3. Register the parent with the invitation, exact Guardian email, and a 12+ character password.
4. Confirm the parent sees only their Family and Athletes.
5. Confirm invitation reuse, a mismatched email, and a short password all fail.
6. Confirm a Customer cannot access `/families` or `/families/{id}`.

## USA Wrestling tests

1. Attempt membership checkout before submitting a USA Wrestling number; checkout must fail.
2. Submit a valid-looking number for the athlete; status must become `Pending`.
3. Confirm short values, unsupported punctuation, duplicate numbers, and another family's athlete are rejected.
4. As Staff, open `/usa-wrestling-verifications` and mark the record Current with an expiration date.
5. Change the number as the parent; status and audit fields must reset to Pending.

## Membership subscription test

1. Select an athlete and membership plan.
2. Confirm Stripe Checkout displays the server-side LegacyOS price and monthly recurrence.
3. Complete payment using Stripe test card `4242 4242 4242 4242`, any future expiration, and any CVC.
4. Confirm the signed webhook is received.
5. Confirm the Purchase Order becomes Completed and exactly one Enrollment becomes active.
6. Replay the webhook; no duplicate Enrollment may be created.
7. Invalid or stale webhook signatures must return 400.

## Product test

1. Buy an active Product.
2. Confirm Stripe displays a one-time payment rather than a subscription.
3. Complete payment and confirm the Purchase Order becomes Completed.
4. Confirm no membership Enrollment is created.

## Failure tests

Use Stripe decline card `4000 0000 0000 0002`.

- The payment must fail.
- The Purchase Order must not become Completed.
- The Enrollment must remain inactive.

Start another checkout and expire it through Stripe CLI or Dashboard.

- The signed expiration webhook must change the order to Expired.
- The Enrollment must remain inactive.

## Launch gate

Do not use live Stripe keys or migrate the existing production database until:

- All automated and manual checks pass.
- Cross-family requests consistently return 403.
- Invalid webhook signatures return 400.
- Successful test payment activates exactly one Enrollment.
- Failed and expired payments never activate Enrollment.
- Database backup and restore have been tested.
- Production HTTPS, exact CORS origins, backups, monitoring, and logging are configured.
- The bootstrap Staff password has been removed from ongoing runtime configuration.
