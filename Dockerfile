FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

COPY . ./

RUN dotnet publish WebUI -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
WORKDIR /app
COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "WebUI.dll"]

EXPOSE 5000