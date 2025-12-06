# How to Start the ASP.NET Server

## Quick Start

1. **Navigate to the backend-aspnet directory:**
   ```bash
   cd backend-aspnet
   ```

2. **Restore NuGet packages (first time only):**
   ```bash
   dotnet restore
   ```

3. **Run the server:**
   ```bash
   dotnet run
   ```

   Or for development with hot reload:
   ```bash
   dotnet watch run
   ```

## Server Details

- **Default Port:** `http://localhost:4000`
- **Port can be changed via:**
  - Environment variable: `PORT=5000`
  - Or in `appsettings.json`: `"Port": "5000"`

## Environment Variables (Optional)

You can set these before running:
```bash
# Windows (PowerShell)
$env:PORT="4000"
$env:USE_REAL_CTP="false"
$env:APP_URL="http://localhost:4000"
$env:CTP_MERCHANT_ID="your_merchant_id"
$env:CTP_API_KEY="your_api_key"
$env:CTP_API_SECRET="your_api_secret"
$env:CTP_WEBHOOK_SECRET="your_webhook_secret"

# Then run
dotnet run
```

```bash
# Windows (CMD)
set PORT=4000
set USE_REAL_CTP=false
set APP_URL=http://localhost:4000
dotnet run
```

```bash
# Linux/Mac
export PORT=4000
export USE_REAL_CTP=false
export APP_URL=http://localhost:4000
dotnet run
```

## Verification

Once the server starts, you should see:
```
🚀 CTP Backend running on http://localhost:4000
📝 Environment: DEVELOPMENT (Mock CTP)
```

## API Endpoints

- `POST http://localhost:4000/api/contractors/register` - Register contractor
- `GET http://localhost:4000/api/contractors/{id}` - Get contractor
- `POST http://localhost:4000/api/payments/initiate` - Initiate payment
- `POST http://localhost:4000/api/payments/webhook` - Webhook endpoint
- `GET http://localhost:4000/api/payments/verify/{orderId}` - Verify payment
- `POST http://localhost:4000/api/payments/retry/{orderId}` - Retry payment
- `GET http://localhost:4000/mock-ctp-payment` - Mock payment page

## Stop the Server

Press `Ctrl+C` in the terminal to stop the server.


