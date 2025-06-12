# Use the official .NET SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY AspireSample.Elastic/*.csproj ./
RUN dotnet restore ./AspireSample.Elastic.csproj

# Copy everything else and build
COPY AspireSample.Elastic/. ./
RUN dotnet publish ./AspireSample.Elastic.csproj -c Release -o /app/publish --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AspireSample.Elastic.dll"]
