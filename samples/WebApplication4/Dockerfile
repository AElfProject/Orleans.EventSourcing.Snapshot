FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["samples/WebApplication4/WebApplication4.csproj", "WebApplication4/"]
RUN dotnet restore "samples/WebApplication4/WebApplication4.csproj"
COPY . .
WORKDIR "/src/WebApplication4"
RUN dotnet build "WebApplication4.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebApplication4.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApplication4.dll"]
