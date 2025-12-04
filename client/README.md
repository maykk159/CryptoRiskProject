# ⚛️ Frontend - Crypto Risk Analysis

React + TypeScript + Vite application for cryptocurrency risk visualization.

## 📋 Requirements

- **Node.js 18.18.0** or higher
- **npm 9.8.1** or higher
- Modern web browser (Chrome 90+, Firefox 88+, Safari 14+, Edge 90+)

## 🔍 Version Information

```json
{
  "node": "18.18.0+",
  "npm": "9.8.1+",
  "react": "19.2.0",
  "typescript": "5.9.3",
  "vite": "7.2.4",
  "tailwindcss": "3.4.18"
}
```

### Dependencies

**Production:**
```json
{
  "axios": "^1.13.2",
  "clsx": "^2.1.1",
  "react": "^19.2.0",
  "react-dom": "^19.2.0",
  "recharts": "^3.5.1",
  "tailwind-merge": "^3.4.0"
}
```

**Development:**
```json
{
  "@vitejs/plugin-react": "^5.1.1",
  "autoprefixer": "^10.4.22",
  "postcss": "^8.5.6",
  "tailwindcss": "^3.4.18",
  "typescript": "~5.9.3",
  "vite": "^7.2.4"
}
```

## 🚀 Installation

### 1. Install Dependencies
```powershell
cd client
npm install
```

### 2. Run Development Server
```powershell
npm run dev
```

Frontend will start on: **http://localhost:5173**

### 3. Build for Production
```powershell
npm run build
```

Output will be in `dist/` folder.

## 📁 Project Structure

```
client/
├── src/
│   ├── components/
│   │   ├── Dashboard.tsx       # Main dashboard component
│   │   ├── AssetSelector.tsx   # Crypto selector dropdown
│   │   ├── RiskCard.tsx        # Risk score display
│   │   └── PriceChart.tsx      # Price chart (Recharts)
│   │
│   ├── services/
│   │   └── api.ts              # Backend API integration
│   │
│   ├── types/
│   │   └── index.ts            # TypeScript type definitions
│   │
│   ├── App.tsx                 # Root component
│   ├── main.tsx                # Application entry point
│   └── index.css               # Global styles (Tailwind)
│
├── public/                     # Static assets
├── index.html                  # HTML template
├── vite.config.ts              # Vite configuration
├── tailwind.config.js          # Tailwind CSS configuration
├── postcss.config.js           # PostCSS configuration
└── tsconfig.json               # TypeScript configuration
```

## 🎨 Features

### Components

1. **Dashboard** (165 lines)
   - Main layout and state management
   - Asset selection
   - Data fetching with loading/error states
   - Risk score display

2. **AssetSelector** (40 lines)
   - Dropdown for cryptocurrency selection
   - 7 supported assets

3. **RiskCard** (60 lines)
   - Visual risk score display
   - Color-coded risk levels
   - Progress bars for individual metrics

4. **PriceChart** (52 lines)
   - 30-day price history chart
   - Interactive tooltips
   - Responsive design

### Styling

- **Tailwind CSS 3** - Utility-first CSS
- **Dark Theme** - Professional dark mode design
- **Responsive** - Mobile, tablet, desktop layouts
- **Gradients** - Modern visual effects

## 🔧 Configuration

### API Endpoint
Located in `src/services/api.ts`:
```typescript
const API_URL = 'http://localhost:5058/api';
```

Change if backend runs on different port.

### Tailwind Configuration
`tailwind.config.js`:
```javascript
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {},
  },
  plugins: [],
}
```

### Vite Configuration
`vite.config.ts`:
```typescript
export default defineConfig({
  plugins: [react()],
})
```

## 🧪 Available Scripts

```powershell
# Development server with hot reload
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Lint code
npm run lint
```

## 🎯 Supported Cryptocurrencies

1. **Bitcoin (BTC)** - `bitcoin`
2. **Ethereum (ETH)** - `ethereum`
3. **Solana (SOL)** - `solana`
4. **Ripple (XRP)** - `ripple`
5. **Cardano (ADA)** - `cardano`
6. **BNB (BNB)** - `binancecoin`
7. **Dogecoin (DOGE)** - `dogecoin`

## 🐛 Troubleshooting

### Port Already in Use
```powershell
# Vite will automatically try port 5174, 5175, etc.
# Or specify port manually:
npm run dev -- --port 3000
```

### Backend Connection Error
Check:
1. Backend is running on `http://localhost:5058`
2. CORS is enabled in backend
3. No firewall blocking localhost

### Dependencies Installation Failed
```powershell
# Clear cache and reinstall
rm -rf node_modules package-lock.json
npm install
```

### Build Errors
```powershell
# Check TypeScript errors
npx tsc --noEmit

# Clear Vite cache
rm -rf node_modules/.vite
npm run dev
```

## 📊 Code Statistics

- **Total Lines**: 330
- **Components**: 317 lines
- **Services**: 15 lines
- **Types**: 15 lines
- **Bundle Size**: ~150 KB (gzipped)

## 🔧 Development

### Hot Module Replacement (HMR)
Vite provides instant HMR - changes appear immediately without full reload.

### TypeScript
Strict mode enabled. All components are fully typed.

### ESLint
Configured for React + TypeScript best practices.

## 🎨 Design System

### Colors
```css
/* Risk Levels */
Low Risk: green-400
Medium Risk: yellow-400
High Risk: red-400

/* UI */
Background: gray-900
Cards: gray-800
Borders: gray-700
Text: white, gray-300, gray-400
```

### Gradient
```css
bg-gradient-to-r from-blue-400 to-purple-500
```

## 📱 Browser Support

- ✅ Chrome 90+
- ✅ Firefox 88+
- ✅ Safari 14+
- ✅ Edge 90+

## 🚀 Deployment

### Build for Production
```powershell
npm run build
```

### Deploy
Upload `dist/` folder to:
- **Netlify**: Drag & drop
- **Vercel**: `vercel deploy`
- **GitHub Pages**: `npm install gh-pages && npm run deploy`

### Environment Variables
Create `.env` file:
```bash
VITE_API_URL=https://your-backend-api.com/api
```

Update `api.ts`:
```typescript
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5058/api';
```

## 📝 License

MIT License - see main [LICENSE](../LICENSE) file

## 🔗 Related

- [Backend README](../CryptoRiskAnalysis.API/README.md)
- [Main README](../README.md)
- [Vite Documentation](https://vitejs.dev/)
- [React Documentation](https://react.dev/)
- [Tailwind CSS](https://tailwindcss.com/)
