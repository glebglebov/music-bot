FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ./src/VpopulazMusicBot/*.csproj ./VpopulazMusicBot/
RUN dotnet restore ./VpopulazMusicBot/VpopulazMusicBot.csproj

COPY ./src/VpopulazMusicBot ./VpopulazMusicBot
WORKDIR /src/VpopulazMusicBot

RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "VpopulazMusicBot.dll"]
