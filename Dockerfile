
# Stage 1: Build and publish the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy the web project file and restore dependencies
COPY src/JourneyTracker.Web/JourneyTracker.Web.csproj src/JourneyTracker.Web/

RUN dotnet restore src/JourneyTracker.Web/JourneyTracker.Web.csproj

# Copy the remaining source files
COPY . .

# Publish the application
RUN dotnet publish src/JourneyTracker.Web/JourneyTracker.Web.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false


# Stage 2: Smaller production runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

# Store the source commit SHA in the image
ARG COMMIT_SHA=unknown
ENV COMMIT_SHA=${COMMIT_SHA}
LABEL org.opencontainers.image.revision="${COMMIT_SHA}"

# Configure application port
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "JourneyTracker.Web.dll"]
