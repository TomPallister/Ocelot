FROM microsoft/dotnet:2.1-sdk
ARG BUILD_CONFIGURATION=Debug
ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
EXPOSE 80

WORKDIR /src
COPY ["DownstreamService/DownstreamService.csproj", "DownstreamService/"]

RUN dotnet restore "DownstreamService/DownstreamService.csproj"
COPY . .
WORKDIR "/src/DownstreamService"
RUN dotnet build --no-restore "DownstreamService.csproj" -c $BUILD_CONFIGURATION

ENTRYPOINT ["dotnet", "run", "--no-build", "--no-launch-profile", "-c", "$BUILD_CONFIGURATION", "--"]