namespace telerik_getting_started.Data
{
    /// <summary>
    /// Provides random weather forecast data. Implemented as a service so that it can
    /// be injected where needed.
    /// </summary>
    public class WeatherForecastService : IWeatherForecastService
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        /// <inheritdoc />
        public Task<WeatherForecast[]> GetForecastAsync(DateOnly startDate)
        {
            var forecasts = Enumerable.Range(1, 5)
                .Select(index => new WeatherForecast
                {
                    Date = startDate.AddDays(index),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
                .ToArray();

            return Task.FromResult(forecasts);
        }
    }
}
