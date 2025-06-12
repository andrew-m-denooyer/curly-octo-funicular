using System;
using Elastic.Clients.Elasticsearch;

namespace AspireSample.Elastic;

public class ElasticClient(ElasticsearchClient client)
{
    public async Task CreateIndexAsync(string indexName)
    {
        // Check if the index already exists
        var existsResponse = await client.Indices.ExistsAsync(indexName);
        if (!existsResponse.Exists)
        {
            // Create the index with mappings
            var createIndexResponse = await client.Indices.CreateAsync(indexName, ci => ci
                .Mappings(m => m
                    .Properties(p => p
                        .Date("date")
                        .IntegerNumber("temperatureC")
                        .Text("summary")
                    )
                )
            );
        }
    }

    

    public async Task IndexWeatherForecastAsync(string indexName, WeatherForecast[] forecasts)
    {
        // Index the documents into Elasticsearch
        foreach (var item in forecasts)
        {
            var doc = new
            {
                date = item.Date.ToDateTime(TimeOnly.MinValue),
                temperatureC = item.TemperatureC,
                summary = item.Summary
            };
            await client.IndexAsync(doc, idx => idx.Index(indexName));
        }
    }

    public async Task<IEnumerable<object>> SearchWeatherForecastAsync(string indexName, int maxItems = 10)
    {
        var searchResponse = await client.SearchAsync<object>(s => s.Indices(indexName).Query(q => q.MatchAll()));
        return searchResponse.Documents;
    }
}
