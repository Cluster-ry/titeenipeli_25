using Titeenipeli.Grpc.Services;

namespace Titeenipeli.Grpc;

public static class GrpcServiceRegister
{
    public static void AddSingletonGrpcServices(IServiceCollection services)
    {
        services.AddSingleton<IIncrementalMapUpdateCoreService, IncrementalMapUpdateCoreService>();
        services.AddSingleton<IMiscGameStateUpdateCoreService, MiscGameStateUpdateCoreService>();
    }

    public static void AddGrpcServices(WebApplication app)
    {
        app.UseGrpcWeb();
        app.MapGrpcService<StateUpdateService>().EnableGrpcWeb();

        var env = app.Environment;
        if (env.IsDevelopment())
        {
            app.MapGrpcReflectionService();
        }
    }
}