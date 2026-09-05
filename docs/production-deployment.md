# DenOS production deployment

Use `render.production.yaml` to create a separate Render Blueprint. Do not replace or modify the existing test Blueprint.

## Initial Blueprint values

Use the generated Render URLs until the custom domains are connected:

- `Cors__AllowedOrigins__0`: the HTTPS URL Render assigns to `denos-web`
- `Frontend__BaseUrl`: the same `denos-web` HTTPS URL
- `VITE_API_BASE_URL`: the HTTPS URL Render assigns to `denos-api`
- `BootstrapStaff__Email`: the initial production administrator email
- `BootstrapStaff__Password`: a unique temporary password of at least 12 characters
- `Email__ResendApiKey`: the working Resend API key
- `Email__From`: `DenOS <accounts@thedenfranklin.com>`
- `Stripe__SecretKey`: the Stripe live secret key when live payment testing begins
- `Stripe__WebhookSecret`: the signing secret for the production webhook endpoint

Never copy the test database URL, test Stripe secret, or test webhook secret into production.

## Custom domains

After both services deploy successfully, add these custom domains in Render:

- `app.thedenfranklin.com` on `denos-web`
- `api.thedenfranklin.com` on `denos-api`

Add the DNS records Render provides at the DNS host. After TLS is active, update:

- `Cors__AllowedOrigins__0` to `https://app.thedenfranklin.com`
- `Frontend__BaseUrl` to `https://app.thedenfranklin.com`
- `VITE_API_BASE_URL` to `https://api.thedenfranklin.com`

Redeploy both services after changing these values.

## Stripe live cutover

Create a new live-mode webhook endpoint using the production API URL. Never reuse the sandbox webhook signing secret. Complete one low-value real purchase and refund before onboarding members.

## Production initialization

The API applies database migrations on startup. After the first successful deployment:

1. Sign in with the bootstrap administrator.
2. Add the permanent administrators and reset temporary passwords.
3. Upload the production waiver.
4. Create production products and discount rules.
5. Send a test email from Administrator Access.
6. Complete the production test checklist before accepting member registrations.
