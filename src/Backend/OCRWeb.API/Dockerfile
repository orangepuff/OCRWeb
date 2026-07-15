FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["OCRWeb.slnx", "./"]
COPY ["src/Backend/OCRWeb.API/OCRWeb.API.csproj", "src/Backend/OCRWeb.API/"]
COPY ["src/Backend/OCRWeb.Contract/OCRWeb.Contract.csproj", "src/Backend/OCRWeb.Contract/"]
COPY ["src/Backend/OCRWeb.DocumentProcessing/OCRWeb.DocumentProcessing.csproj", "src/Backend/OCRWeb.DocumentProcessing/"]
COPY ["src/Backend/OCRWeb.ProjectManagement/OCRWeb.ProjectManagement.csproj", "src/Backend/OCRWeb.ProjectManagement/"]
COPY ["src/Backend/OCRWeb.Shared/OCRWeb.Shared.csproj", "src/Backend/OCRWeb.Shared/"]
RUN dotnet restore "src/Backend/OCRWeb.API/OCRWeb.API.csproj"

COPY . .
RUN dotnet publish "src/Backend/OCRWeb.API/OCRWeb.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OCRWeb.API.dll"]
