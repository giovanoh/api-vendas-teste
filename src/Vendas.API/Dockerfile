FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
EXPOSE 5000 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Vendas.API/Vendas.API.csproj", "Vendas.API/"]
RUN dotnet restore "Vendas.API/Vendas.API.csproj"
COPY . .
RUN dotnet publish "Vendas.API/Vendas.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Vendas.API.dll"]