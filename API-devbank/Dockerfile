FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY API-devbank/API-devbank.csproj API-devbank/
RUN dotnet restore API-devbank/API-devbank.csproj

COPY . .

WORKDIR /src/API-devbank
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "API-devbank.dll"]