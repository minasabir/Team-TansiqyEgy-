# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files first (for better layer caching)
COPY TansiqyV1.DAL/TansiqyV1.DAL.csproj TansiqyV1.DAL/
COPY TansiqyV1.BLL/TansiqyV1.BLL.csproj TansiqyV1.BLL/
COPY TansiqyV1.API/TansiqyV1.API.csproj TansiqyV1.API/

# Restore dependencies
RUN dotnet restore TansiqyV1.API/TansiqyV1.API.csproj

# Copy everything else and build
COPY . .
RUN dotnet build TansiqyV1.API/TansiqyV1.API.csproj -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish TansiqyV1.API/TansiqyV1.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080

# Copy published output
COPY --from=publish /app/publish .

# Run as non-root user for security
USER $APP_UID

ENTRYPOINT ["dotnet", "TansiqyV1.API.dll"]
