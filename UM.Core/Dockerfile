
# Build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["UM.Core.csproj", "./"]

RUN dotnet restore "UM.Core.csproj"

COPY . .

RUN dotnet publish "UM.Core.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false



# Runtime

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .


EXPOSE 8080

ENTRYPOINT ["dotnet", "UM.Core.dll"]