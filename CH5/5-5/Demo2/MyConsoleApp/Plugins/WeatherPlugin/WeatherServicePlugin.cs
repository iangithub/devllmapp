using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace MyConsoleApp.Plugins
{
    public class WeatherServicePlugin
    {
        private List<WeatherForecastData> cityTemperature = new List<WeatherForecastData>
                {
                    new WeatherForecastData { City = "高雄", Temperature = 25},
                    new WeatherForecastData { City = "台北", Temperature = 19},
                    new WeatherForecastData { City = "台中", Temperature = 21}
                };

        [KernelFunction, Description("Query Temperature by city name")]
        public int? GetTemperature([Description("The name of the city")] string cityName)
        {
            return cityTemperature.FirstOrDefault(c => c.City == cityName)?.Temperature;
        }
    }

    public class WeatherForecastData
    {
        public string City { get; set; }
        public int Temperature { get; set; }
    }
}
