using Titeenipeli.Grpc.Services;

namespace Titeenipeli.Grpc;

public static class GrpcServiceRegister
{
    public static void AddSingletonGRPCServices(IServiceCollection services)
    {
        services.AddSingleton<IIncrementalMapUpdateCoreService, IncrementalMapUpdateCoreService>();
    }

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
