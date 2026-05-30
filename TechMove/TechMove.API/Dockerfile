FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the API project files
COPY TechMove.API/*.csproj .
RUN dotnet restore

# Copy the rest of the API project
COPY TechMove.API/. .

# Build and publish
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TechMove.API.dll"]