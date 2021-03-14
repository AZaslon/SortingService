using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SortingWebApi.Commands;
using SortingWebApi.Common;
using SortingWebApi.Controllers;
using SortingWebApi.JobsIterator;
using SortingWebApi.JobsQueueListener;
using SortingWebApi.JobsScheduler;
using SortingWebApi.Model;
using KafkaJobsQueueOptions = SortingWebApi.JobsScheduler.KafkaJobsQueueOptions;

namespace SortingWebApi
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

            services.AddSingleton<IScheduleJobCommandHandler, ScheduleJobCommandHandler>();
            services.AddSingleton<IJobsQueue, KafkaJobsQueue>();
            
            services.AddOptions<KafkaJobsQueueOptions>()
                .Bind(Configuration.GetSection(KafkaJobsQueueOptions.Key))
                .ValidateDataAnnotations();

            services.AddTransient<IMessageConverter, MessageConverter>();

            services.AddTransient<IPendingJobIdsIterator, KafkaPendingJobIdsIterator>();
            
            services.AddTransient<IQuery<JobsStatisticsRequest, JobDescriptor>, JobsStatisticsQuery>();

            services.AddControllers();
           
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
