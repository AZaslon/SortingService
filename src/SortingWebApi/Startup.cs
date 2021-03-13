using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JobsWebApiService.Commands;
using Microsoft.Extensions.Caching.Redis;
using SortingWebApi.Commands;
using SortingWebApi.Common;
using SortingWebApi.JobsScheduler;

namespace JobsWebApiService
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
