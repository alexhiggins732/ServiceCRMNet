FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Crm.Api/Crm.Api.csproj", "Crm.Api/"]
COPY ["src/Crm.Infrastructure/Crm.Infrastructure.csproj", "Crm.Infrastructure/"]
COPY ["src/Crm.Application/Crm.Application.csproj", "Crm.Application/"]
COPY ["src/Crm.Domain/Crm.Domain.csproj", "Crm.Domain/"]
COPY ["src/Crm.Shared/Crm.Shared.csproj", "Crm.Shared/"]
COPY ["src/Crm.AiAssistant/Crm.AiAssistant.csproj", "Crm.AiAssistant/"]
COPY ["src/Crm.Pricing/Crm.Pricing.csproj", "Crm.Pricing/"]
COPY ["src/Crm.Integrations.Meta/Crm.Integrations.Meta.csproj", "Crm.Integrations.Meta/"]
COPY ["src/Crm.Integrations.Twilio/Crm.Integrations.Twilio.csproj", "Crm.Integrations.Twilio/"]
COPY ["src/Directory.Build.props", "./"]

RUN dotnet restore "Crm.Api/Crm.Api.csproj"

COPY src/ .
WORKDIR "/src/Crm.Api"
RUN dotnet build "Crm.Api.csproj" -c Release -o /app/build
RUN dotnet publish "Crm.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Crm.Api.dll"]
