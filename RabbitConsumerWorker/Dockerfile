FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["RabbitConsumerWorker/RabbitConsumerWorker.csproj", "RabbitConsumerWorker/"]
RUN dotnet restore "RabbitConsumerWorker/RabbitConsumerWorker.csproj"

COPY . .
WORKDIR "/src/RabbitConsumerWorker"
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

RUN mkdir -p /app

COPY RabbitConsumerWorker/appsettings.json /app/

ENTRYPOINT ["dotnet", "RabbitConsumerWorker.dll"]