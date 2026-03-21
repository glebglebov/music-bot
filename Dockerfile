FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ./src/DiscordMusicBot/*.csproj ./DiscordMusicBot/
RUN dotnet restore ./DiscordMusicBot/DiscordMusicBot.csproj

COPY ./src/DiscordMusicBot ./DiscordMusicBot
WORKDIR /src/DiscordMusicBot

RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "DiscordMusicBot.dll"]
