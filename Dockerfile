
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["global.json", "./"]

COPY ["src/ForaChallenge.Api/ForaChallenge.Api.csproj", "ForaChallenge.Api/"]
COPY ["src/ForaChallenge.Core/ForaChallenge.Core.csproj", "ForaChallenge.Core/"]
COPY ["src/ForaChallenge.Infrastructure/ForaChallenge.Infrastructure.csproj", "ForaChallenge.Infrastructure/"]

RUN dotnet restore "ForaChallenge.Api/ForaChallenge.Api.csproj"

COPY src/ .

WORKDIR "/src/ForaChallenge.Api"
RUN dotnet publish "ForaChallenge.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "ForaChallenge.Api.dll"]

