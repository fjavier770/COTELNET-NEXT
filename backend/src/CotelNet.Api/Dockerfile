FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Packages.props /src/Directory.Packages.props
COPY backend/ /src/backend/
RUN dotnet restore /src/backend/CotelNet.slnx
RUN dotnet publish /src/backend/src/CotelNet.Api/CotelNet.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "CotelNet.Api.dll"]

