# Dockerfile para aplicação .NET 8 Web API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5149 7228

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ApiCentral.csproj", "."]
RUN dotnet restore "ApiCentral.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "ApiCentral.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ApiCentral.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiCentral.dll"]
