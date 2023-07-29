using IdentityService.Configuration;
using IdentityService.Grpc;
using Serilog;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("IdentityService.UnitTests")]
// for Moq to use internals
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

var builder = WebApplication.CreateBuilder(args);

SerilogConfiguration.ConfigureSerilog(builder.Configuration);

try
{
    Log.Information("Starting IdentityService.");

    // Add services to the container.
    builder.Services.ConfigureDatabase(builder.Configuration);
    builder.Services.ConfigureIdentity(builder.Configuration);
    builder.Services.ConfigureSwagger();
    builder.Services.ConfigureRabbitMQ(builder.Configuration);

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddGrpc();

    builder.Host.UseSerilog();

    var app = builder.Build();

    if (app!.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
        app.UseSwagger();
        app.UseSwaggerUI();
        // for production apply migrations manually!
        app.MigrateDatabase();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapGrpcService<GrpcAuthService>();
    app.AddEndpoints();

    app.Run();
}
catch (Exception e)
{
    Log.Fatal(e, "IdentityService failed to start.");
}
finally
{
    Log.CloseAndFlush();
}
