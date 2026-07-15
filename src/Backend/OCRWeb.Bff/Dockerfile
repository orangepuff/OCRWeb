FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["OCRWeb.slnx", "./"]
COPY ["src/Backend/OCRWeb.Bff/OCRWeb.Bff.csproj", "src/Backend/OCRWeb.Bff/"]
COPY ["src/Backend/OCRWeb.Contract/OCRWeb.Contract.csproj", "src/Backend/OCRWeb.Contract/"]
RUN dotnet restore "src/Backend/OCRWeb.Bff/OCRWeb.Bff.csproj"

COPY . .
RUN dotnet publish "src/Backend/OCRWeb.Bff/OCRWeb.Bff.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OCRWeb.Bff.dll"]
