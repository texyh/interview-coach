# Use the official .NET 8.0 SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj files and restore dependencies
COPY src/AIPA.Domain/AIPA.Domain.csproj src/AIPA.Domain/
COPY src/AIPA.Application/AIPA.Application.csproj src/AIPA.Application/
COPY src/AIPA.Infrastructure/AIPA.Infrastructure.csproj src/AIPA.Infrastructure/
COPY src/AIPA.WebApi/AIPA.WebApi.csproj src/AIPA.WebApi/
COPY AIPA.sln ./

# Restore NuGet packages
RUN dotnet restore

# Copy the entire source code
COPY . .

# Build the application
RUN dotnet build -c Release --no-restore

# Publish the application
RUN dotnet publish src/AIPA.WebApi/AIPA.WebApi.csproj -c Release -o /app/publish --no-build

# Use the official .NET 8.0 runtime image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Expose the port that the application runs on
EXPOSE 8080

# Set the entry point
ENTRYPOINT ["dotnet", "AIPA.WebApi.dll"]
