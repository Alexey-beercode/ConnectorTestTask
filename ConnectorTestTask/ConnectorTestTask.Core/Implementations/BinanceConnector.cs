using ConnectorTestTask.Core.Interfaces;
using ConnectorTestTask.Core.Models;

namespace ConnectorTestTask.Core.Implementations
{
    public class BinanceConnector : ITestConnector
    {
        private readonly IBinanceRestClient _restClient;
        private readonly IBinanceWebSocketClient _webSocketClient;

        public event Action<Trade> NewBuyTrade;
        public event Action<Trade> NewSellTrade;
        public event Action<Candle> CandleSeriesProcessing;

        public BinanceConnector(IBinanceRestClient restClient, IBinanceWebSocketClient webSocketClient)
        {
            _restClient = restClient;
            _webSocketClient = webSocketClient;

            _webSocketClient.NewTradeReceived += trade =>
            {
                if (trade.Side == "buy") NewBuyTrade?.Invoke(trade);
                else NewSellTrade?.Invoke(trade);
            };

            _webSocketClient.NewCandleReceived += candle =>
            {
                CandleSeriesProcessing?.Invoke(candle);
            };
        }

        #region REST API

        public async Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount)
        {
            return await _restClient.GetTradesAsync(pair, maxCount);
        }

        public async Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0)
        {
            return await _restClient.GetCandlesAsync(pair, periodInSec, from, to, count);
        }

        public async Task<Dictionary<string, decimal>> CalculatePortfolioValueAsync(Dictionary<string, decimal> portfolio, string targetCurrency)
        {
            // Получаем список доступных валют
            var availableCurrencies = await _restClient.GetAvailableCurrenciesAsync();

            var portfolioValue = new Dictionary<string, decimal>();

            foreach (var (currency, amount) in portfolio)
            {
                if (!availableCurrencies.Contains(currency) || !availableCurrencies.Contains(targetCurrency))
                {
                    Console.WriteLine($"Ошибка: {currency} или {targetCurrency} не поддерживаются.");
                    portfolioValue[currency] = 0;
                    continue;
                }

                decimal rate = await _restClient.GetTickerAsync($"{currency}{targetCurrency}");
                portfolioValue[currency] = rate > 0 ? amount * rate : 0;
            }

            return portfolioValue;
        }
        
        public async Task<HashSet<string>> GetAvailableCurrenciesAsync()
        {
            return await _restClient.GetAvailableCurrenciesAsync();
        }

        
        #endregion

        #region WebSocket API

        public async void SubscribeTrades(string pair, int maxCount = 100)
        {
            await _webSocketClient.ConnectAsync();
            await _webSocketClient.SubscribeTrades(pair);
        }

        public void UnsubscribeTrades(string pair)
        {
            _webSocketClient.Unsubscribe(pair);
        }

        public async void SubscribeCandles(string pair, int periodInSec, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = 0)
        {
            await _webSocketClient.ConnectAsync();
            await _webSocketClient.SubscribeCandles(pair, periodInSec);
        }

        public void UnsubscribeCandles(string pair)
        {
            _webSocketClient.Unsubscribe(pair);
        }

        #endregion
    }
}
