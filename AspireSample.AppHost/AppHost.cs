using AspireSample.AppHost;
using Json.Patch;

var builder = DistributedApplication.CreateBuilder(args);
var password = builder.AddParameter("password", "default-password", secret: true);

var elasticsearch = builder.AddElasticsearch("elasticsearch", password);
    // .RunElasticWithHttpsDevCertificate(port: 9200);
    


var apiService = builder.AddProject<Projects.AspireSample_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");




builder.AddProject<Projects.AspireSample_Elastic>("elastic-service")
    .WithExternalHttpEndpoints()
    // .WithHttpHealthCheck("/health")
    .WithReference(elasticsearch)
    .WaitFor(elasticsearch);

builder.AddProject<Projects.AspireSample_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();

// Example: reference elasticsearch from your elasticservice if needed
//var elasticService = builder.AddProject<Projects.AspireSample_Elastic>("elasticservice").WithReference(elasticsearch);
