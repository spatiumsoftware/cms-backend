#Dockerfile For .NET app written in DDD 

# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the solution file and restore dependencies
COPY SpatiumCMS.sln .
COPY Spatium-CMS/Spatium-CMS.csproj ./Spatium-CMS/
COPY DbMigrations/Migrations.csproj ./DbMigrations/
COPY Infrastructure/Infrastructure.csproj ./Infrastructure/
COPY Utilities/Utilities.csproj ./Utilities/
COPY CMS-Migration-API/CMS-Migration-API.csproj ./CMS-Migration-API/
COPY Domain/Domain.csproj ./Domain/

RUN dotnet restore

# Copy the remaining source code and build the application
COPY . .
RUN dotnet publish -c Release -o out

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "Spatium-CMS.dll"]
