FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY global.json Directory.Build.props Directory.Build.targets NuGet.config ./
COPY .editorconfig ./
COPY src/GtMotive.Estimate.Microservice.Host/*.csproj src/GtMotive.Estimate.Microservice.Host/
COPY src/GtMotive.Estimate.Microservice.Api/*.csproj src/GtMotive.Estimate.Microservice.Api/
COPY src/GtMotive.Estimate.Microservice.ApplicationCore/*.csproj src/GtMotive.Estimate.Microservice.ApplicationCore/
COPY src/GtMotive.Estimate.Microservice.Domain/*.csproj src/GtMotive.Estimate.Microservice.Domain/
COPY src/GtMotive.Estimate.Microservice.Infrastructure/*.csproj src/GtMotive.Estimate.Microservice.Infrastructure/
RUN dotnet restore src/GtMotive.Estimate.Microservice.Host/GtMotive.Estimate.Microservice.Host.csproj
COPY src/ src/
RUN dotnet publish src/GtMotive.Estimate.Microservice.Host/GtMotive.Estimate.Microservice.Host.csproj -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "GtMotive.Estimate.Microservice.Host.dll"]
