var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.AspireSample_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

var password = builder.AddParameter("password", secret: true);
var elasticsearch = builder.AddElasticsearch("elasticsearch", password)
    .WithDataVolume();

builder.AddProject<Projects.AspireSample_Elastic>("elastic-service")
    .WithHttpHealthCheck("/health")
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
