# CTP Payment Gateway Backend - ASP.NET (VB.NET)

This is an ASP.NET Web API backend written in VB.NET, converted from the original Node.js/Express implementation.

## Project Structure

```
backend-aspnet/
├── CTPBackend.vbproj      # Project file
├── Program.vb              # Application entry point
├── appsettings.json       # Configuration file
├── Controllers/           # API Controllers
│   ├── ContractorsController.vb
│   ├── PaymentsController.vb
│   └── MockPaymentController.vb
├── Models/                # Data Models
│   ├── Contractor.vb
│   ├── Payment.vb
│   ├── ApiResponse.vb
│   └── CtpPayload.vb
└── Services/              # Business Logic Services
    ├── IContractorService.vb
    ├── ContractorService.vb
    ├── IPaymentService.vb
    ├── PaymentService.vb
    ├── ICtpService.vb
    ├── CtpService.vb
    ├── IEmailService.vb
    └── EmailService.vb
```

## Features

- Contractor Registration API
- Payment Initiation with CTP Gateway
- Webhook Handling for Payment Callbacks
- Payment Verification
- Payment Retry Functionality
- Mock Payment Gateway for Testing

## Configuration

1. Update `appsettings.json` with your CTP Gateway credentials:
```json
{
  "CtpSettings": {
    "MerchantId": "YOUR_MERCHANT_ID",
    "ApiKey": "YOUR_API_KEY",
    "ApiSecret": "YOUR_API_SECRET",
    "WebhookSecret": "YOUR_WEBHOOK_SECRET",
    "BaseUrl": "https://api.ctp.com"
  }
}
```

2. Or use environment variables:
- `CTP_MERCHANT_ID`
- `CTP_API_KEY`
- `CTP_API_SECRET`
- `CTP_WEBHOOK_SECRET`
- `CTP_API_BASE_URL`
- `USE_REAL_CTP` (set to "true" for production)
- `APP_URL`
- `PORT`

## Running the Application

1. Navigate to the backend-aspnet directory:
```bash
cd backend-aspnet
```

2. Restore NuGet packages:
```bash
dotnet restore
```

3. Run the application:
```bash
dotnet run
```

The API will be available at `http://localhost:4000` (or the port specified in configuration).

## API Endpoints

- `POST /api/contractors/register` - Register a new contractor
- `GET /api/contractors/{contractorId}` - Get contractor details
- `POST /api/payments/initiate` - Initiate a payment
- `POST /api/payments/webhook` - Webhook endpoint for CTP callbacks
- `GET /api/payments/verify/{orderId}` - Verify payment status
- `POST /api/payments/retry/{orderId}` - Retry a failed payment
- `GET /mock-ctp-payment` - Mock payment gateway for testing

## Requirements

- .NET 8.0 SDK or later
- Visual Studio 2022 or VS Code with VB.NET extensions (optional)

## Notes

- Currently uses in-memory storage (ConcurrentDictionary). Replace with a database for production.
- Email service is a placeholder. Implement actual email sending with SMTP/SendGrid.
- Mock payment mode is enabled by default. Set `USE_REAL_CTP=true` for production.


