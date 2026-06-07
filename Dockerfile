# syntax=docker/dockerfile:1.20

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

COPY --parents src/**/*.csproj .
RUN dotnet restore src/FiapX.Worker/FiapX.Worker.csproj --locked-mode

COPY src src
RUN dotnet publish src/FiapX.Worker/FiapX.Worker.csproj --no-restore -c Release -o /dist

FROM mcr.microsoft.com/dotnet/aspnet:8.0-noble-chiseled AS final

ENV LANG=pt_BR.UTF-8 LANGUAGE=pt_BR:pt LC_ALL=pt_BR.UTF-8
ENV TZ=America/Sao_Paulo

WORKDIR /app
COPY --from=build /dist .

ENTRYPOINT ["dotnet", "FiapX.Worker.dll"]
