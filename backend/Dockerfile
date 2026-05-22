FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["FootballAcademy/FootballAcademy.csproj", "FootballAcademy/"]
COPY ["DataAccess/DataAccess.csproj", "DataAccess/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["Application/Application.csproj", "Application/"]
RUN dotnet restore "FootballAcademy/FootballAcademy.csproj"
COPY . .
WORKDIR "/src/FootballAcademy"
RUN dotnet build "FootballAcademy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FootballAcademy.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["FootballAcademy/knowledge", "knowledge/"]
ENTRYPOINT ["dotnet", "FootballAcademy.dll"]