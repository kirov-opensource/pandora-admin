FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Pandora.Admin.WebAPI/Pandora.Admin.WebAPI.csproj", "Pandora.Admin.WebAPI/"]
RUN dotnet restore "Pandora.Admin.WebAPI/Pandora.Admin.WebAPI.csproj"
COPY . .
WORKDIR "/src/Pandora.Admin.WebAPI"
RUN dotnet build "Pandora.Admin.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pandora.Admin.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pandora.Admin.WebAPI.dll"]
