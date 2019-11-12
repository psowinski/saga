FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["MyPay/MyPay.csproj", "MyPay/"]
RUN dotnet restore "MyPay/MyPay.csproj"
COPY . .
WORKDIR "/src/MyPay"
RUN dotnet build "MyPay.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyPay.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyPay.dll"]