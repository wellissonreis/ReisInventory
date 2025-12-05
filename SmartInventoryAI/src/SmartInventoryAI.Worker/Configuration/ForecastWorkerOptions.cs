namespace SmartInventoryAI.Worker.Configuration;

public class ForecastWorkerOptions
{
    public const string SectionName = "ForecastWorker";

    public int IntervalMinutes { get; set; } = 5;

    public int ForecastDays { get; set; } = 7;

    public int HistoryDays { get; set; } = 30;

    public bool GeneratePurchaseSuggestions { get; set; } = true;
}
