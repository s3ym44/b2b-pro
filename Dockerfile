# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all project files
COPY . .

# Restore dependencies
RUN dotnet restore "B2BProcurement.sln"

# Build and publish - PROJE DOSYASINI BELİRT
RUN dotnet publish "B2BProcurement.Web/B2BProcurement.Web.csproj" -c Release -o /app/publish --no-restore

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

# Railway PORT environment variable
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "B2BProcurement.Web.dll"]
