FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["./pandora.admin.webapi.csproj", "pandora.admin.webapi/"]
RUN dotnet restore "pandora.admin.webapi/pandora.admin.webapi.csproj"
COPY ["./**", "pandora.admin.webapi/"]

WORKDIR "/src/pandora.admin.webapi"
RUN dotnet build "pandora.admin.webapi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "pandora.admin.webapi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "pandora.admin.webapi.dll"]
