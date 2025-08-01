namespace telerik_getting_started.Data
{
    /// <summary>
    /// Defines the contract for providing weather forecasts.
    /// </summary>
    public interface IWeatherForecastService
    {
        /// <summary>
        /// Retrieves a set of weather forecasts starting from the provided date.
        /// </summary>
        /// <param name="startDate">First date for the forecast period.</param>
        /// <returns>Array of <see cref="WeatherForecast"/> objects.</returns>
        Task<WeatherForecast[]> GetForecastAsync(DateOnly startDate);
    }
}
