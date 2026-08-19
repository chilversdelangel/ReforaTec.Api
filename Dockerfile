# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["src/ReforaTec.Api/ReforaTec.Api.csproj", "src/ReforaTec.Api/"]
RUN dotnet restore "src/ReforaTec.Api/ReforaTec.Api.csproj"

# Copy full source and publish
COPY . .
WORKDIR "/src/src/ReforaTec.Api"
RUN dotnet publish "ReforaTec.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime — use same SDK image to guarantee ASP.NET Core assembly version match
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS final
WORKDIR /app
COPY --from=build /app/publish .

# Install Kerberos GSSAPI library required by Npgsql for remote PostgreSQL connections
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_HTTP_PORTS=8080
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
EXPOSE 8080

ENTRYPOINT ["dotnet", "ReforaTec.Api.dll"]
