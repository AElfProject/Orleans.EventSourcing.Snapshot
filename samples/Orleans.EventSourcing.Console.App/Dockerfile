FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["samples/Orleans.EventSourcing.Console.App/Orleans.EventSourcing.Console.App.csproj", "Orleans.EventSourcing.Console.App/"]
RUN dotnet restore "samples/Orleans.EventSourcing.Console.App/Orleans.EventSourcing.Console.App.csproj"
COPY . .
WORKDIR "/src/Orleans.EventSourcing.Console.App"
RUN dotnet build "Orleans.EventSourcing.Console.App.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Orleans.EventSourcing.Console.App.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Orleans.EventSourcing.Console.App.dll"]
