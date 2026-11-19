FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY src/Greenhouse.Dictionaries.Domain/Greenhouse.Dictionaries.Domain.csproj src/Greenhouse.Dictionaries.Domain/
COPY src/Greenhouse.Dictionaries.Application/Greenhouse.Dictionaries.Application.csproj src/Greenhouse.Dictionaries.Application/
COPY src/Greenhouse.Dictionaries.Infrastructure/Greenhouse.Dictionaries.Infrastructure.csproj src/Greenhouse.Dictionaries.Infrastructure/
COPY src/Greenhouse.Dictionaries.Presentation/Greenhouse.Dictionaries.Presentation.csproj src/Greenhouse.Dictionaries.Presentation/
COPY src/Greenhouse.Dictionaries.Api/Greenhouse.Dictionaries.Api.csproj src/Greenhouse.Dictionaries.Api/
RUN dotnet restore src/Greenhouse.Dictionaries.Api/Greenhouse.Dictionaries.Api.csproj
COPY src/ src/
RUN dotnet publish src/Greenhouse.Dictionaries.Api/Greenhouse.Dictionaries.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080 8081
ENTRYPOINT ["dotnet", "Greenhouse.Dictionaries.Api.dll"]
