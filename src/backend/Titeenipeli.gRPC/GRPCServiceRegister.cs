using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Titeenipeli.gRPC.Services;

namespace Titeenipeli.gRPC;

public static class GRPCServiceRegister
{
    public static void AddGRPCServices(WebApplication app)
    {
        app.UseGrpcWeb();
        app.MapGrpcService<IncrementalMapUpdateService>().EnableGrpcWeb();

        IWebHostEnvironment env = app.Environment;
        if (env.IsDevelopment())
        {
            app.MapGrpcReflectionService();
        }
    }
}
