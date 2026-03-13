FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["backend/Ticketer.Web/Ticketer.Web.csproj", "Ticketer.Web/"]
COPY ["backend/Ticketer.Model/Ticketer.Model.csproj", "Ticketer.Model/"]
COPY ["backend/Ticketer.Repository/Ticketer.Repository.csproj", "Ticketer.Repository/"]
COPY ["backend/Ticketer.UseCases/Ticketer.UseCases.csproj", "Ticketer.UseCases/"]
RUN dotnet restore "Ticketer.Web/Ticketer.Web.csproj"
COPY backend/ .
COPY ["smart-contract/out/Ticket.sol/Ticket.json", "/smart-contract/out/Ticket.sol/Ticket.json"]
WORKDIR "/src/Ticketer.Web"
RUN dotnet build "./Ticketer.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Ticketer.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ticketer.Web.dll"]
