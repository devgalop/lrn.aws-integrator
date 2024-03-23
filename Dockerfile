FROM mcr.microsoft.com/dotnet/aspnet:8.0.2-alpine3.18-amd64 AS base
WORKDIR /app

#build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0.201-alpine3.18-amd64 AS build
WORKDIR /src
COPY ["lrn.devgalop.awsintegrator.Core", "./lrn.devgalop.awsintegrator.Core"]
COPY ["lrn.devgalop.awsintegrator.Infrastructure", "./lrn.devgalop.awsintegrator.Infrastructure"]
COPY ["lrn.devgalop.awsintegrator.Webapi", "./lrn.devgalop.awsintegrator.Webapi"]

COPY . .
WORKDIR "/src/."

RUN dotnet build "lrn.devgalop.awsintegrator.Webapi/lrn.devgalop.awsintegrator.Webapi.csproj" -c Release -o /app/build

#publish stage
FROM build AS publish
RUN dotnet publish "lrn.devgalop.awsintegrator.Webapi/lrn.devgalop.awsintegrator.Webapi.csproj" -c Release -o /app/publish

#exec stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "lrn.devgalop.awsintegrator.Webapi.dll"]