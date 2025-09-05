using System.Reflection;
using System.Text.Json.Serialization;

using DotNetEnv;

using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using MySqlConnector;

using Vendas.API.Domain.Repositories;
using Vendas.API.Domain.Services;
using Vendas.API.Infrastructure.Contexts;
using Vendas.API.Infrastructure.Factories;
using Vendas.API.Infrastructure.Middlewares;
using Vendas.API.Infrastructure.Repositories;
using Vendas.API.Infrastructure.Services;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Vendas API",
        Version = "1.0.0",
        Description = "API de testes",
        Contact = new OpenApiContact
        {
            Name = "Giovano Hendges",
            Url = new Uri("https://github.com/giovanoh")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

builder.Services.AddControllers(options =>
    {
        options.Conventions.Add(new RouteTokenTransformerConvention(
            new LowercaseParameterTransformer()
        ));
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = InvalidModelStateResponseFactory.Create;
    });
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

var dbConfig = new MySqlConnectionStringBuilder()
{
    Server = Environment.GetEnvironmentVariable("MYSQL_HOST"),
    Database = Environment.GetEnvironmentVariable("MYSQL_DATABASE"),
    UserID = Environment.GetEnvironmentVariable("MYSQL_USER"),
    Password = Environment.GetEnvironmentVariable("MYSQL_PASSWORD"),
    Port = Convert.ToUInt32(Environment.GetEnvironmentVariable("MYSQL_PORT")),
    CharacterSet = Environment.GetEnvironmentVariable("MYSQL_CHARSET")
};

builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseMySql(
        dbConfig.ConnectionString,
        new MySqlServerVersion(new Version(5, 5, 62))
    ));

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

WebApplication app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}
app.MapControllers();
app.Run();
