using FluentValidation;
using Mapster;
using OrderService.Configuration;
using OrderService.Data;
using OrderService.Grpc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new NullReferenceException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDatabase(connectionString);
builder.Services.AddScoped<IGrpcIdentityService, GrpcIdentityService>();
builder.Services.ConfigureRabbitMQ(builder.Configuration);

ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
TypeAdapterConfig.GlobalSettings.Default.NameMatchingStrategy(NameMatchingStrategy.IgnoreCase);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MigrateDatabase();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>()
        ?? throw new InvalidOperationException("Couldn't seed database, dbcontext is null.");
    SeedDatabase.SeedDb(dbContext);
}

app.AddEndpoints();

app.Run();
