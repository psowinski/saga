FROM mcr.microsoft.com/dotnet/core/runtime:3.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
#COPY ["ShoppingSaga/ShoppingSaga.csproj", "ShoppingSaga/"]
COPY . .
RUN dotnet restore "ShoppingSaga/ShoppingSaga.csproj"
WORKDIR "/src/ShoppingSaga"
RUN dotnet build "ShoppingSaga.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ShoppingSaga.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShoppingSaga.dll"]