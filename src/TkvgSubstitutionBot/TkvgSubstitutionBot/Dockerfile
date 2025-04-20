
# Use the official .NET SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set the working directory
WORKDIR /src
# Copy the rest of the application files
COPY . .

WORKDIR /src/TkvgSubstitutionBot
# Build the application
RUN dotnet publish -c Release -o /src/out


FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# Set the working directory
WORKDIR /app

# Copy the built application from the build stage
COPY --from=build /src/out .

# Set the entry point for the container
ENTRYPOINT ["dotnet", "TkvgSubstitutionBot.dll"]
