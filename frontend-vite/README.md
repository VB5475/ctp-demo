# CTP Payment Gateway - Frontend

A React frontend application for contractor registration with CTP payment gateway integration, built with Vite.

## Project Structure

```
frontend-vite/
├── src/
│   ├── components/
│   │   ├── RegistrationForm.jsx    # Registration form component
│   │   └── PaymentCallback.jsx    # Payment callback handler
│   ├── services/
│   │   └── api.js                  # API service utilities
│   ├── styles/
│   │   └── index.css               # Global styles
│   ├── App.jsx                      # Main app component
│   └── main.jsx                     # Entry point
├── index.html                       # HTML template
├── package.json                     # Dependencies
├── vite.config.js                   # Vite configuration
└── README.md                        # This file
```

## Features

- **Contractor Registration Form**: Multi-step form for contractor details
- **Payment Integration**: CTP payment gateway integration
- **Payment Callback Handler**: Verifies and displays payment status
- **Debug Logging**: Real-time system logs for debugging
- **Responsive Design**: Mobile-friendly UI

## Getting Started

### Prerequisites

- Node.js 16+ and npm/yarn/pnpm

### Installation

1. Install dependencies:
```bash
npm install
```

2. Start development server:
```bash
npm run dev
```

The app will be available at `http://localhost:3000`

### Build for Production

```bash
npm run build
```

The built files will be in the `dist` directory.

### Preview Production Build

```bash
npm run preview
```

## Environment Variables

Create a `.env` file in the root directory:

```env
VITE_API_BASE_URL=http://localhost:4000/api
```

If not set, it defaults to `http://localhost:4000/api`

## API Endpoints

The frontend communicates with the backend API:

- `POST /api/contractors/register` - Register contractor
- `GET /api/contractors/{id}` - Get contractor details
- `POST /api/payments/initiate` - Initiate payment
- `GET /api/payments/verify/{orderId}` - Verify payment status
- `POST /api/payments/retry/{orderId}` - Retry failed payment

## Components

### RegistrationForm
Handles the contractor registration process:
- Step 1: Collect contractor details
- Step 2: Initiate payment

### PaymentCallback
Handles payment verification after redirect:
- Checks payment status
- Displays success/failure messages
- Allows payment retry

## Development

The project uses:
- **React 18** - UI library
- **Vite** - Build tool and dev server
- **Modern ES6+** - JavaScript features

## Notes

- Make sure the backend API is running on `http://localhost:4000`
- The payment gateway redirects back to `/payment/callback?orderId=...&status=...`
- Debug logs are displayed in the registration form for troubleshooting


