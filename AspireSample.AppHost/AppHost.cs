var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.AspireSample_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

// var elasticsearch = builder.AddElasticsearch("elastic-client")
//     .WithExternalHttpEndpoints()
//     .WithHttpHealthCheck("/health");

builder.AddProject<Projects.AspireSample_Elastic>("elastic-service")
    .WithHttpHealthCheck("/health");
    // .WithReference(elasticsearch);
    // .WaitFor(elasticsearch);

builder.AddProject<Projects.AspireSample_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();

// Example: reference elasticsearch from your elasticservice if needed
//var elasticService = builder.AddProject<Projects.AspireSample_Elastic>("elasticservice").WithReference(elasticsearch);
