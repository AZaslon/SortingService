using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SortAsc.Worker.Service.JobsQueue;
using SortAsc.Worker.Service.ProcessingLogic;
using SortingWebApi.Common;

namespace SortAsc.Worker.Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IRetryPolicyFactory, RetryPolicyFactory>();

            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = Configuration.GetSection("Redis")["ConnectionString"];
                options.InstanceName = Configuration.GetSection("Redis")["Instance"];

            });
            services.AddOptions<KafkaJobsQueueOptions>()
                .Bind(Configuration.GetSection(KafkaJobsQueueOptions.Key))
                .ValidateDataAnnotations();

            services.AddSingleton<IJobProcessingLogic, SortAscProcessingLogic>();
            services.AddSingleton<IMessageConverter, MessageConverter>();
            services.AddTransient<IJobsListener, KafkaJobsListener>();

           
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }
    }
}
