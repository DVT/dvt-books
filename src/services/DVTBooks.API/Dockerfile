FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY src/services/DVTBooks.API/DVTBooks.API.csproj src/services/DVTBooks.API/
RUN dotnet restore "src/services/DVTBooks.API/DVTBooks.API.csproj"
COPY . .
WORKDIR "/src/src/services/DVTBooks.API"
RUN dotnet build "DVTBooks.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DVTBooks.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DVTBooks.API.dll"]
