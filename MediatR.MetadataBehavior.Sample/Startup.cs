using Microsoft.Extensions.DependencyInjection;
using MediatR;
using MediatR.MetadataBehavior.Extensions;
using System.Reflection;

namespace MediatR.MetadataBehavior.Sample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // 註冊 MediatR  
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // 註冊 Metadata 行為  
            services.AddMediatRMetadataBehavior(Assembly.GetExecutingAssembly());
        }
    }
}
