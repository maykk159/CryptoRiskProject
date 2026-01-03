// Using a reliable CDN for crypto icons
const getIconUrl = (symbol: string) => `https://cdn.jsdelivr.net/gh/atomiclabs/cryptocurrency-icons@1a63530be6e374711a8554f31b17e4cb92c25fa5/128/color/${symbol.toLowerCase()}.png`;

export const ASSETS = [
    { id: 'bitcoin', name: 'Bitcoin', ticker: 'BTC', symbol: 'btc', icon: getIconUrl('btc') },
    { id: 'ethereum', name: 'Ethereum', ticker: 'ETH', symbol: 'eth', icon: getIconUrl('eth') },
    { id: 'tether', name: 'Tether', ticker: 'USDT', symbol: 'usdt', icon: getIconUrl('usdt') },
    { id: 'ripple', name: 'Ripple', ticker: 'XRP', symbol: 'xrp', icon: getIconUrl('xrp') },
    { id: 'binancecoin', name: 'BNB', ticker: 'BNB', symbol: 'bnb', icon: getIconUrl('bnb') },
    { id: 'solana', name: 'Solana', ticker: 'SOL', symbol: 'sol', icon: getIconUrl('sol') },
    { id: 'usd-coin', name: 'USDC', ticker: 'USDC', symbol: 'usdc', icon: getIconUrl('usdc') },
    { id: 'tron', name: 'TRON', ticker: 'TRX', symbol: 'trx', icon: getIconUrl('trx') },
    { id: 'dogecoin', name: 'Dogecoin', ticker: 'DOGE', symbol: 'doge', icon: getIconUrl('doge') },
    { id: 'cardano', name: 'Cardano', ticker: 'ADA', symbol: 'ada', icon: getIconUrl('ada') },
    { id: 'avalanche-2', name: 'Avalanche', ticker: 'AVAX', symbol: 'avax', icon: getIconUrl('avax') },
    { id: 'chainlink', name: 'Chainlink', ticker: 'LINK', symbol: 'link', icon: getIconUrl('link') },
    { id: 'shiba-inu', name: 'Shiba Inu', ticker: 'SHIB', symbol: 'shib', icon: getIconUrl('shib') },
    { id: 'bitcoin-cash', name: 'Bitcoin Cash', ticker: 'BCH', symbol: 'bch', icon: getIconUrl('bch') },
    { id: 'stellar', name: 'Stellar', ticker: 'XLM', symbol: 'xlm', icon: getIconUrl('xlm') },
    { id: 'polkadot', name: 'Polkadot', ticker: 'DOT', symbol: 'dot', icon: getIconUrl('dot') },
    { id: 'litecoin', name: 'Litecoin', ticker: 'LTC', symbol: 'ltc', icon: getIconUrl('ltc') },
    { id: 'uniswap', name: 'Uniswap', ticker: 'UNI', symbol: 'uni', icon: getIconUrl('uni') },
    { id: 'wrapped-bitcoin', name: 'Wrapped Bitcoin', ticker: 'WBTC', symbol: 'wbtc', icon: getIconUrl('wbtc') },
    { id: 'dai', name: 'Dai', ticker: 'DAI', symbol: 'dai', icon: getIconUrl('dai') },
];
