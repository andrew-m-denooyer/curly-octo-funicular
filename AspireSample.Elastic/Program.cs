using Elastic.Clients.Elasticsearch;
using AspireSample.Elastic;

var builder = WebApplication.CreateBuilder(args);


// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add services to the container.
builder.Services.AddProblemDetails();


// builder.AddElasticsearchClient("elasticsearch", (settings) =>
// {
//     settings.Endpoint = new Uri("http://localhost:27011");
// });


var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

app.MapGet("/create-index", async () =>
{
    var elasticclient = app.Services.GetRequiredService<ElasticsearchClient>();
    ElasticClient client = new(elasticclient);
    // Create a new index for WeatherForecast if it doesn't exist
    var indexName = "weatherforecasts";

    await client.CreateIndexAsync(indexName);

    // Generate and index WeatherForecast documents
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    await client.IndexWeatherForecastAsync(indexName, forecast);

    // Query all documents from the weatherforecasts index
    var searchResponse = await client.SearchWeatherForecastAsync(indexName, 10);
    return Results.Json(searchResponse);
})
.WithName("CreateIndex")
.WithSummary("Creates an index for WeatherForecast and indexes sample data.")
.WithDescription("This endpoint creates an index named 'weatherforecasts' if it does not exist, indexes sample WeatherForecast data, and returns all indexed documents.");


app.MapDefaultEndpoints();

app.Run();
