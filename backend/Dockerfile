FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ./AkisaAi.Api.csproj ./
RUN dotnet restore ./AkisaAi.Api.csproj
COPY . ./
RUN dotnet publish ./AkisaAi.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AkisaAi.Api.dll"]
