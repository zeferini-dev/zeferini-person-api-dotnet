# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY zeferini-person-api-dotnet.csproj ./
RUN dotnet restore

# Copy source code and build
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview
WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Expose port
EXPOSE 3000

# Set environment variables
ENV ASPNETCORE_URLS=http://+:3000
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Start the application
ENTRYPOINT ["dotnet", "zeferini-person-api-dotnet.dll"]
