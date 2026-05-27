# =========================================================================
# Multi-stage Dockerfile for ByteBrigade Pharmacy API on Render (or any Docker
# host). Uses the official .NET 10 SDK to build, and the .NET 10 runtime image
# to serve. The container listens on $PORT (Render sets this).
# =========================================================================

# ---- Build stage --------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore first for layer caching
COPY HackathonBackend.csproj ./
RUN dotnet restore HackathonBackend.csproj

# Copy the rest and publish a release build
COPY . ./
RUN dotnet publish HackathonBackend.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ---- Runtime stage ------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Render injects a PORT env var. Bind Kestrel to it.
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production

# Create wwwroot folders so first request doesn't have to.
RUN mkdir -p /app/wwwroot/prescriptions /app/wwwroot/emails

COPY --from=build /app/publish ./

# Render's free tier uses an ephemeral filesystem — prescriptions uploaded at
# runtime survive only while the container is alive. For real persistence,
# mount a Render Disk at /app/wwwroot/prescriptions.

EXPOSE 8080

ENTRYPOINT ["dotnet", "HackathonBackend.dll"]
