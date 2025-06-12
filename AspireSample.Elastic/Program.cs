using Elastic.Clients.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton(sp =>
{
    var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"));
    var client = new ElasticsearchClient(settings);
    return client;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/create-index", async () =>
{
    var elasticClient = app.Services.GetRequiredService<ElasticsearchClient>();

    // Create a new index for WeatherForecast if it doesn't exist
    var indexName = "weatherforecasts";
    var existsResponse = await elasticClient.Indices.ExistsAsync(indexName);
    if (!existsResponse.Exists)
    {
        var createIndexResponse = await elasticClient.Indices.CreateAsync(indexName, ci => ci
            .Mappings(m => m
                .Properties(p => p
                    .Date("date")
                    .IntegerNumber("temperatureC")
                    .Text("summary")
                )
            )
        );
    }

    // Generate and index WeatherForecast documents
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    // Index the documents into Elasticsearch
    foreach (var item in forecast)
    {
        var doc = new
        {
            date = item.Date.ToDateTime(TimeOnly.MinValue),
            temperatureC = item.TemperatureC,
            summary = item.Summary
        };
        await elasticClient.IndexAsync(doc, idx => idx.Index(indexName));
    }

    // Query all documents from the weatherforecasts index
    var searchResponse = await elasticClient.SearchAsync<object>(s => s.Indices(indexName).Query(q => q.MatchAll()));
    return Results.Json(searchResponse.Documents);
})
.WithName("CreateIndex")
.WithSummary("Creates an index for WeatherForecast and indexes sample data.")
.WithDescription("This endpoint creates an index named 'weatherforecasts' if it does not exist, indexes sample WeatherForecast data, and returns all indexed documents.");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
