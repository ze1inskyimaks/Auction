FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Auction.API/Auction.API.csproj", "Auction.API/"]
COPY ["Auction.BL/Auction.BL.csproj", "Auction.BL/"]
COPY ["Auction.Data/Auction.Data.csproj", "Auction.Data/"]
RUN dotnet restore "Auction.API/Auction.API.csproj"
COPY . .
WORKDIR "/src/Auction.API"
RUN dotnet build "Auction.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Auction.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auction.API.dll"]
