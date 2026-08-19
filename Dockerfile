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

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ReforaTec.Api.dll"]
