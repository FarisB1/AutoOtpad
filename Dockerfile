FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["AutoOtpad.csproj", "."]
RUN dotnet restore "./AutoOtpad.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "AutoOtpad.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AutoOtpad.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AutoOtpad.dll"]
ENV ASPNETCORE_URLS=http://*:80
