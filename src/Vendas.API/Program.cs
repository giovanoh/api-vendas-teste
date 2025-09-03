using System.Reflection;

using DotNetEnv;

using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using MySqlConnector;

using Vendas.API.Infrastructure.Contexts;

Env.Load();

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

builder.Services.AddControllers();
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

var dbConfig = new MySqlConnectionStringBuilder()
{
    Server = Environment.GetEnvironmentVariable("MYSQL_SERVER"),
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
