FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["RabbitMQMessagingConsoleApp/RabbitMQMessagingConsoleApp.csproj", "RabbitMQMessagingConsoleApp/"]
RUN dotnet restore "RabbitMQMessagingConsoleApp/RabbitMQMessagingConsoleApp.csproj"

COPY . .
WORKDIR "/src/RabbitMQMessagingConsoleApp"
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

RUN mkdir -p /app

COPY RabbitMQMessagingConsoleApp/appsettings.json /app/

ENTRYPOINT ["dotnet", "RabbitMQMessagingConsoleApp.dll"]
