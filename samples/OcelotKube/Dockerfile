FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["ApiGateway/ApiGateway.csproj", "ApiGateway/"]
COPY ["../../src/Ocelot.Provider.Polly/Ocelot.Provider.Polly.csproj", "../../src/Ocelot.Provider.Polly/"]
COPY ["../../src/Ocelot/Ocelot.csproj", "../../src/Ocelot/"]
COPY ["../../src/Ocelot.Provider.Kubernetes/Ocelot.Provider.Kubernetes.csproj", "../../src/Ocelot.Provider.Kubernetes/"]
RUN dotnet restore "ApiGateway/ApiGateway.csproj"
COPY . .
WORKDIR "/src/ApiGateway"
RUN dotnet build "ApiGateway.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "ApiGateway.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
