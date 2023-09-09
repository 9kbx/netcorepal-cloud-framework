using FluentValidation.AspNetCore;
using FluentValidation;
using NetCorePal.Extensions.Domain.Json;
using NetCorePal.Extensions.Primitives;
using NetCorePal.Extensions.DependencyInjection;
using StackExchange.Redis;
using System.Reflection;
using NetCorePal.Web.Infra;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Web.Application.Queries;
using NetCorePal.Extensions.DistributedTransactions.Sagas;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();

builder.Services.AddMvc().AddControllersAsServices().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new EntityIdJsonConverterFactory());
});
var redis = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!);
builder.Services.AddSingleton<IConnectionMultiplexer>(p => redis);


#region ��������

builder.Services.AddSingleton<IClock, SystemClock>();

#endregion


#region �����¼�

//builder.Services.AddTransient<OrderPaidIntegrationEventHandler>();

#endregion

#region ģ����֤��

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

#endregion


#region Mapper Provider

builder.Services.AddMapperPrivider(Assembly.GetExecutingAssembly());

#endregion

builder.Services.AddScoped<OrderQuery>();

#region ������ʩ

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()).AddUnitOfWorkBehaviors());
builder.Services.AddRepositories(typeof(ApplicationDbContext).Assembly);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL"));
    options.LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors();
});
builder.Services.AddUnitOfWork<ApplicationDbContext>();
builder.Services.AddPostgreSqlTransactionHandler();
builder.Services.AddAllCAPEventHanders(typeof(Program));
builder.Services.AddCap(x =>
{
    x.UseEntityFramework<ApplicationDbContext>();
    x.UseRabbitMQ(p => builder.Configuration.GetSection("RabbitMQ").Bind(p));
});
builder.Services.AddSagas<ApplicationDbContext>(typeof(Program)).AddCAPSagaEventPublisher();
#endregion

var app = builder.Build();
app.UseRouting();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/", () => "Hello World!");

app.Run();


public partial class Program
{
}