FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Crm.Web/Crm.Web.csproj", "Crm.Web/"]
COPY ["src/Crm.Shared/Crm.Shared.csproj", "Crm.Shared/"]
COPY ["src/Crm.Domain/Crm.Domain.csproj", "Crm.Domain/"]
COPY ["src/Directory.Build.props", "./"]

RUN dotnet restore "Crm.Web/Crm.Web.csproj"

COPY src/Crm.Web/ Crm.Web/
COPY src/Crm.Shared/ Crm.Shared/
COPY src/Crm.Domain/ Crm.Domain/
WORKDIR "/src/Crm.Web"
RUN dotnet build "Crm.Web.csproj" -c Release -o /app/build
RUN dotnet publish "Crm.Web.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Crm.Web.dll"]
