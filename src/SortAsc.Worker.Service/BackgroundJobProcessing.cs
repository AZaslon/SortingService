using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SortAsc.Worker.Service
{
    public class BackgroundJobProcessing: BackgroundService
    {
        private readonly ILogger<BackgroundJobProcessing> _logger;

        public BackgroundJobProcessing(IOptions<ServiceOptions> options,
            ILogger<BackgroundJobProcessing> logger)
        {
            _logger = logger;
        }
        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                try
                {
                    throw new NotImplementedException();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Fail during job processing. There will be another attempt to process requested work.", ex);
                    await Task.Delay(3000, stoppingToken).ConfigureAwait(false);
                }
            }
        }
    }

    public class ServiceOptions
    {
    }
}
