FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY invoices.core/invoices.core.csproj invoices.core/
COPY invoices.api/invoices.api.csproj invoices.api/
RUN dotnet restore invoices.api/invoices.api.csproj
COPY . .
RUN dotnet publish invoices.api/invoices.api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "invoices.api.dll"]
