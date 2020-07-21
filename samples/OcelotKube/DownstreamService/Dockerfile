FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["DownstreamService/DownstreamService.csproj", "DownstreamService/"]
RUN dotnet restore "DownstreamService/DownstreamService.csproj"
COPY . .
WORKDIR "/src/DownstreamService"
RUN dotnet build "DownstreamService.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "DownstreamService.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "DownstreamService.dll"]
