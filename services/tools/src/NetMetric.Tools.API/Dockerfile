# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props Directory.Build.targets Directory.Packages.props ./
COPY .nuget/NuGet.Config ./.nuget/NuGet.Config
COPY packages/dotnet/src ./packages/dotnet/src
COPY services/tools/src ./services/tools/src

RUN dotnet restore services/tools/src/NetMetric.Tools.API/NetMetric.Tools.API.csproj --force-evaluate
RUN dotnet publish services/tools/src/NetMetric.Tools.API/NetMetric.Tools.API.csproj \
    -c Release \
    --no-restore \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

USER $APP_UID
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "NetMetric.Tools.API.dll"]
