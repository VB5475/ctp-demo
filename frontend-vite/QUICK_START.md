# Quick Start Guide

## Installation & Setup

1. **Navigate to the frontend-vite directory:**
   ```bash
   cd frontend-vite
   ```

2. **Install dependencies:**
   ```bash
   npm install
   ```

3. **Start the development server:**
   ```bash
   npm run dev
   ```

The app will automatically open at `http://localhost:3000`

## What Changed?

### Before (Single HTML File)
- All React code in one `<script type="text/babel">` tag
- CSS embedded in `<style>` tag
- No build process
- Used CDN scripts (React, ReactDOM, Babel)

### After (Vite React Project)
- ✅ Proper project structure with separate files
- ✅ CSS in dedicated stylesheet (`src/styles/index.css`)
- ✅ Components split into separate files:
  - `src/components/RegistrationForm.jsx`
  - `src/components/PaymentCallback.jsx`
- ✅ API service utilities (`src/services/api.js`)
- ✅ Fast Vite dev server with hot module replacement
- ✅ Production-ready build system

## Project Structure

```
frontend-vite/
├── src/
│   ├── components/          # React components
│   │   ├── RegistrationForm.jsx
│   │   └── PaymentCallback.jsx
│   ├── services/            # API utilities
│   │   └── api.js
│   ├── styles/              # CSS files
│   │   └── index.css
│   ├── App.jsx              # Main app component
│   └── main.jsx             # Entry point
├── index.html               # HTML template
├── package.json             # Dependencies
├── vite.config.js          # Vite config
└── README.md               # Documentation
```

## Available Scripts

- `npm run dev` - Start development server (port 3000)
- `npm run build` - Build for production
- `npm run preview` - Preview production build

## Environment Variables

Create `.env` file (optional):
```env
VITE_API_BASE_URL=http://localhost:4000/api
```

## Features Preserved

✅ All original functionality maintained:
- Contractor registration form
- Payment initiation
- Payment callback handling
- Debug logging
- Responsive design
- All styling preserved

## Next Steps

1. Make sure your backend is running on `http://localhost:4000`
2. Run `npm install` in the `frontend-vite` directory
3. Run `npm run dev` to start the frontend
4. Open `http://localhost:3000` in your browser

The app should work exactly the same as before, but now with a proper React project structure!


