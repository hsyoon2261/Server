FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ServerCore/ServerCore.csproj", "ServerCore/"]
RUN dotnet restore "ServerCore/ServerCore.csproj"
COPY . .
WORKDIR "/src/ServerCore"
RUN dotnet build "ServerCore.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ServerCore.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServerCore.dll"]
