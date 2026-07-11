FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY learnflow_service.csproj ./
RUN dotnet restore

COPY . ./

# Publish the application
RUN dotnet publish learnflow_service.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app
COPY --from=build /app/out ./
COPY --from=build /app/.env ./

EXPOSE 8080

ENTRYPOINT ["dotnet", "learnflow_service.dll"]
