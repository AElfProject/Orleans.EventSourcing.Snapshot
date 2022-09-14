FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["samples/Orleans.EventSourcing.API/Orleans.EventSourcing.API.csproj", "Orleans.EventSourcing.API/"]
RUN dotnet restore "samples/Orleans.EventSourcing.API/Orleans.EventSourcing.API.csproj"
COPY . .
WORKDIR "/src/Orleans.EventSourcing.API"
RUN dotnet build "Orleans.EventSourcing.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Orleans.EventSourcing.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Orleans.EventSourcing.API.dll"]
