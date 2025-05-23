FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 4044
EXPOSE 5054

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["online-store/OnlineStore/OnlineStore.csproj", "OnlineStore/"]
#COPY ["Contracts/Contracts.csproj", "Contracts/"]
RUN dotnet restore "./OnlineStore/./OnlineStore.csproj"
WORKDIR "/src/OnlineStore"
COPY "online-store/OnlineStore/" .
RUN dotnet build "./OnlineStore.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OnlineStore.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

#RUN mkdir -p /app/certificates
#COPY RequisitionService/certificates/aspnetapp.pfx /app/certificates
#RUN mkdir -p /app/keyring

ENTRYPOINT ["dotnet", "OnlineStore.dll"]