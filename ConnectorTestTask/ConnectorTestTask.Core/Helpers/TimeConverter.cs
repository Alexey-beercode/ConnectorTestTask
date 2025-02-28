namespace ConnectorTestTask.Core.Helpers;

public static class TimeConverter
{
    public static string ConvertToTimeframe(int periodInSec)
    {
        return periodInSec switch
        {
            60 => "1m",
            300 => "5m",
            900 => "15m",
            3600 => "1h",
            86400 => "1d",
            _ => throw new ArgumentException("Неподдерживаемый период")
        };
    }
}