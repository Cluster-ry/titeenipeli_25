using Titeenipeli.Grpc.Services;

namespace Titeenipeli.Grpc;

public static class GrpcServiceRegister
{
    public static void AddSingletonGrpcServices(IServiceCollection services)
    {
        services.AddSingleton<IIncrementalMapUpdateCoreService, IncrementalMapUpdateCoreService>();
    }

    public static void AddGrpcServices(WebApplication app)
    {
        app.UseGrpcWeb();
        app.MapGrpcService<IncrementalMapUpdateService>().EnableGrpcWeb();

        var env = app.Environment;
        if (env.IsDevelopment())
        {
            app.MapGrpcReflectionService();
        }
    }
}