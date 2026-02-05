# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY B2BProcurement.sln .
COPY B2BProcurement/B2BProcurement.Web.csproj B2BProcurement/
COPY B2BProcurement.Core/B2BProcurement.Core.csproj B2BProcurement.Core/
COPY B2BProcurement.Data/B2BProcurement.Data.csproj B2BProcurement.Data/
COPY B2BProcurement.Business/B2BProcurement.Business.csproj B2BProcurement.Business/

# Restore dependencies
RUN dotnet restore

# Copy all source code
COPY . .

# Build and publish
WORKDIR /src/B2BProcurement
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install SQLite (for production SQLite usage)
RUN apt-get update && apt-get install -y sqlite3 && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=build /app/publish .

# Create data directory for SQLite
RUN mkdir -p /app/data

# Set environment variables
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Expose port (Railway will override with PORT env var)
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:${PORT:-8080}/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "B2BProcurement.Web.dll"]
