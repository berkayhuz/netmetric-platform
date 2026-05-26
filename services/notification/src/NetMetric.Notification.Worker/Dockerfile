# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props Directory.Build.targets Directory.Packages.props NetMetric.slnx ./
COPY .nuget/NuGet.Config ./.nuget/NuGet.Config
COPY packages/dotnet/src ./packages/dotnet/src
COPY services/notification/src ./services/notification/src

RUN dotnet restore services/notification/src/NetMetric.Notification.Worker/NetMetric.Notification.Worker.csproj --force-evaluate
RUN dotnet publish services/notification/src/NetMetric.Notification.Worker/NetMetric.Notification.Worker.csproj -c Release --no-restore -o /app/publish -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
ENV DOTNET_EnableDiagnostics=0
COPY --from=build /app/publish ./
USER $APP_UID
ENTRYPOINT ["dotnet", "NetMetric.Notification.Worker.dll"]
