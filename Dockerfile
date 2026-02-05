# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the entire project
COPY . .

# Restore packages from the csproj location
WORKDIR /src/src/zeferini-person-api-dotnet
RUN dotnet restore "zeferini-person-api-dotnet.csproj"

# Build the application
RUN dotnet build "zeferini-person-api-dotnet.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
WORKDIR /src/src/zeferini-person-api-dotnet
RUN dotnet publish "zeferini-person-api-dotnet.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "zeferini-person-api-dotnet.dll"]
