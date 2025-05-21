FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY *.sln .
COPY Template.API/*.csproj ./Template.API/
COPY Template.Data/*.csproj ./Template.Data/
COPY Template.Domain/*.csproj ./Template.Domain/
COPY Template.Core/*.csproj ./Template.Core/
COPY Template.Common/*.csproj ./Template.Common/

RUN dotnet restore

COPY . .
RUN dotnet publish Template.API/Template.API.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 10000

ENTRYPOINT ["dotnet", "Template.API.dll"]