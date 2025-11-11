# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SkyQuery.AppGateway/SkyQuery.AppGateway.csproj", "SkyQuery.AppGateway/"]
COPY ["SkyQuery.AppGateway.Application/SkyQuery.AppGateway.Application.csproj", "SkyQuery.AppGateway.Application/"]
COPY ["SkyQuery.AppGateway.Domain/SkyQuery.AppGateway.Domain.csproj", "SkyQuery.AppGateway.Domain/"]
COPY ["SkyQuery.AppGateway.Infrastructure/SkyQuery.AppGateway.Infrastructure.csproj", "SkyQuery.AppGateway.Infrastructure/"]
COPY ["SkyQuery.ServiceDefaults/SkyQuery.ServiceDefaults.csproj", "SkyQuery.ServiceDefaults/"]
RUN dotnet restore "./SkyQuery.AppGateway/SkyQuery.AppGateway.csproj"
COPY . .
WORKDIR "/src/SkyQuery.AppGateway"
RUN dotnet build "./SkyQuery.AppGateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SkyQuery.AppGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SkyQuery.AppGateway.dll"]