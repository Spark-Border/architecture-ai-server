FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ArchitectureAI.Api/ArchitectureAI.Api.csproj", "ArchitectureAI.Api/"]
RUN dotnet restore "ArchitectureAI.Api/ArchitectureAI.Api.csproj"
COPY . .
WORKDIR "/src/ArchitectureAI.Api"
RUN dotnet build "./ArchitectureAI.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ArchitectureAI.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ArchitectureAI.Api.dll"]
