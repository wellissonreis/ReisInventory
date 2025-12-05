namespace SmartInventoryAI.Worker.Configuration;

public class ForecastWorkerOptions
{
    public const string SectionName = "ForecastWorker";

    /// <summary>
    /// Intervalo entre execuções do worker em minutos.
    /// </summary>
    public int IntervalMinutes { get; set; } = 5;

    /// <summary>
    /// Número de dias para gerar previsões.
    /// </summary>
    public int ForecastDays { get; set; } = 7;

    /// <summary>
    /// Número de dias de histórico para analisar.
    /// </summary>
    public int HistoryDays { get; set; } = 30;

    /// <summary>
    /// Se deve gerar sugestões de compra automaticamente.
    /// </summary>
    public bool GeneratePurchaseSuggestions { get; set; } = true;
}
