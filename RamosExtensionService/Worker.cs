using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ThermoArgonautViewerLibrary;

namespace RamosExtensionService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var omni = new Argonaut();
            var recipes = omni.GetRecipeNameList();
            var cfg = omni.GetRaMOSConfiguration();
            
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                var recipe = omni.GetRecipe(recipes.Items[0]);
                _logger.LogInformation(recipe.Name);
                await Task.Delay(1000, stoppingToken);
            }

        }
    }
}
