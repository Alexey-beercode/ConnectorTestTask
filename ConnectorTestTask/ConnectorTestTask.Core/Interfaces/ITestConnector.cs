using ConnectorTestTask.Core.Models;

namespace ConnectorTestTask.Core.Interfaces
{
    public interface ITestConnector
    {
        #region REST API

        Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount);
        Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0);
        //Метод для получения списка доступных валют, нужен для того, чтобы можно было предложить пользователю выбрать валюты для расчета портфеля
        Task<HashSet<string>> GetAvailableCurrenciesAsync();
        //Основной метод для подсчета суммы портфеля для любых валют 
        Task<Dictionary<string, decimal>> CalculatePortfolioValueAsync(Dictionary<string, decimal> portfolio, string targetCurrency);

        #endregion

        #region WebSocket API

        event Action<Trade> NewBuyTrade;
        event Action<Trade> NewSellTrade;
        void SubscribeTrades(string pair, int maxCount = 100);
        void UnsubscribeTrades(string pair);

        event Action<Candle> CandleSeriesProcessing;
        void SubscribeCandles(string pair, int periodInSec, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = 0);
        void UnsubscribeCandles(string pair);

        #endregion
    }
}