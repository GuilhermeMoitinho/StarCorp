FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restaura usando so os manifestos primeiro, aproveitando o cache de camadas.
COPY StarCorp.slnx ./
COPY src/StarCorp.WebApi/StarCorp.WebApi.csproj src/StarCorp.WebApi/
COPY src/StarCorp.Business/StarCorp.Business.csproj src/StarCorp.Business/
COPY src/StarCorp.Data/StarCorp.Data.csproj src/StarCorp.Data/
RUN dotnet restore src/StarCorp.WebApi/StarCorp.WebApi.csproj

COPY src/ src/
COPY db/ db/
RUN dotnet publish src/StarCorp.WebApi/StarCorp.WebApi.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080
ENTRYPOINT ["dotnet", "StarCorp.WebApi.dll"]
