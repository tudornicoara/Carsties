using System.Net;
using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient<AuctionsSvcHttpClient>().AddPolicyHandler(GetPolicy());
builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));
    
    x.UsingRabbitMq((context, cfg) =>
    {
        // cfg.UseMessageRetry(r =>
        // {
        //     r.Handle<RabbitMqConnectionException>();
        //     r.Interval(5, TimeSpan.FromSeconds(10));
        // });
        //
        // cfg.Host(builder.Configuration["RabbitMq:Host"], "/", host =>
        // {
        //     host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
        //     host.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        // });
        
        cfg.ReceiveEndpoint("search-auction-created", e =>
        {
            e.UseMessageRetry(r => r.Interval(5, 5));
            
            e.ConfigureConsumer<AuctionCreatedConsumer>(context);
        });
        
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

async void InitDbCall()
{
    try
    {
        await DbInitializer.InitDb(app);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}

app.Lifetime.ApplicationStarted.Register(InitDbCall);

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetPolicy()
    => HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));