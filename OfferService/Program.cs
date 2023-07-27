using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OfferService.Configuration;
using OfferService.ConfigureServices;
using OfferService.Grpc;
using OfferService.Offers;
using Serilog;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("OfferService.UnitTests")]

var builder = WebApplication.CreateBuilder(args);

SerilogConfiguration.AddSerilog(builder.Configuration);

try
{
    Log.Information("Starting OfferService.");

    // Add services to the container.
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    builder.Services.AddDatabase(connectionString);
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwagger();
    builder.Services.AddScoped<IGrpcIdentityService, GrpcIdentityService>();
    builder.Services.AddTransient<IOfferRepository, OfferRepository>();

    ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

    builder.Host.UseSerilog();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.MigrateDatabase();

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.AddEndpoints();

    app.Run();
} 
catch (Exception e)
{
    Log.Fatal(e, "OfferService failed to start.");
}
finally
{
    Log.CloseAndFlush();
}