# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SkyQuery.ImageService/SkyQuery.ImageService.csproj", "SkyQuery.ImageService/"]
COPY ["SkyQuery.ImageService.Application/SkyQuery.ImageService.Application.csproj", "SkyQuery.ImageService.Application/"]
COPY ["SkyQuery.ImageService.Domain/SkyQuery.ImageService.Domain.csproj", "SkyQuery.ImageService.Domain/"]
COPY ["SkyQuery.ImageService.Infrastructure/SkyQuery.ImageService.Infrastructure.csproj", "SkyQuery.ImageService.Infrastructure/"]
COPY ["SkyQuery.ServiceDefaults/SkyQuery.ServiceDefaults.csproj", "SkyQuery.ServiceDefaults/"]
RUN dotnet restore "./SkyQuery.ImageService/SkyQuery.ImageService.csproj"
COPY . .
WORKDIR "/src/SkyQuery.ImageService"
RUN dotnet build "./SkyQuery.ImageService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SkyQuery.ImageService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SkyQuery.ImageService.dll"]