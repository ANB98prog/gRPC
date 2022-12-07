using GrpcChatServer.Services;

namespace GrpcChatServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddGrpc();

            builder.Services.AddSingleton<ChatRoom>();

            var app = builder.Build();

            RegisterGrpsServices(app);


            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
        }

        /// <summary>
        /// Registers gRPC services
        /// </summary>
        /// <param name="app">WebApp configuration</param>
        /// <returns>Configuration</returns>
        private static WebApplication RegisterGrpsServices(WebApplication app)
        {
            app.MapGrpcService<ChatService>();

            return app;
        }
    }
}