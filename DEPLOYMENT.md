# DenOS Render deployment

The committed `render.yaml` creates a disposable test environment:

- `denos-web-test`: free React static site
- `denos-api-test`: free ASP.NET Core Docker web service
- `denos-test-db`: free PostgreSQL database

Do not use this free stack for real members. The free database expires after 30 days, has no backups, and the free API sleeps when idle.

## Create the test stack

1. Push this branch to the GitHub repository connected to Render.
2. In Render, choose **New > Blueprint** and select the repository.
3. Render reads `render.yaml`. Supply these prompted values without committing them:
   - `Cors__AllowedOrigins__0`: the HTTPS URL Render assigns to `denos-web-test`, with no trailing slash.
   - `Frontend__BaseUrl`: the same frontend HTTPS URL.
   - `VITE_API_BASE_URL`: the HTTPS URL Render assigns to `denos-api-test`, with no trailing slash.
   - `Stripe__SecretKey`: Stripe sandbox key beginning with `sk_test_`.
   - `Stripe__WebhookSecret`: sandbox webhook signing secret beginning with `whsec_`.
   - `BootstrapStaff__Email`: initial staff email.
   - `BootstrapStaff__Password`: a unique temporary password of at least 12 characters.
4. Deploy the Blueprint. The API applies Entity Framework migrations when it starts.
5. If a prompted service URL was unknown during initial creation, update it in that service's **Environment** page and redeploy the affected service.
6. In Stripe test mode, create a webhook endpoint at `https://<denos-api-test-host>/portal/purchases/stripe/webhook` using the event list in the launch workbook.
7. Run the workbook's **Sandbox tests** sheet. Do not enter real family data or live Stripe credentials.

## Custom domain and production

After sandbox tests pass, create a separate paid production Blueprint/resources. Do not upgrade or reuse the test database as the production customer database. Attach `thedenfranklin.com` and `www.thedenfranklin.com` to the frontend and `api.thedenfranklin.com` to the API, then configure the exact DNS records Render displays.

Store live Stripe keys only in the production API's Render environment settings. Configure a separate live Stripe webhook, run the workbook's **Live Stripe tests**, and remove the bootstrap password after the named staff account has been verified.
