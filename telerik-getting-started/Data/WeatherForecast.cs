namespace telerik_getting_started.Data
{
    /// <summary>
    /// Represents a single weather forecast entry.
    /// </summary>
    public class WeatherForecast
    {
        /// <summary>Forecast date.</summary>
        public DateOnly Date { get; set; }

        /// <summary>Temperature in Celsius.</summary>
        public int TemperatureC { get; set; }

        /// <summary>Temperature in Fahrenheit.</summary>
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        /// <summary>Optional descriptive summary.</summary>
        public string? Summary { get; set; }
    }
}
