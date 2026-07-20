FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["OCRWeb.slnx", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["NuGet.config", "./"]
COPY ["src/Backend/OCRWeb.API/OCRWeb.API.csproj", "src/Backend/OCRWeb.API/"]
COPY ["src/Backend/OCRWeb.Document/OCRWeb.Document.csproj", "src/Backend/OCRWeb.Document/"]
COPY ["src/Backend/OCRWeb.Document.Api/OCRWeb.Document.Api.csproj", "src/Backend/OCRWeb.Document.Api/"]
COPY ["src/Backend/OCRWeb.Document.Contract/OCRWeb.Document.Contract.csproj", "src/Backend/OCRWeb.Document.Contract/"]
COPY ["src/Backend/OCRWeb.OCR/OCRWeb.OCR.csproj", "src/Backend/OCRWeb.OCR/"]
COPY ["src/Backend/OCRWeb.OCR.Contract/OCRWeb.OCR.Contract.csproj", "src/Backend/OCRWeb.OCR.Contract/"]
COPY ["src/Backend/OCRWeb.Pdf/OCRWeb.Pdf.csproj", "src/Backend/OCRWeb.Pdf/"]
COPY ["src/Backend/OCRWeb.Pdf.Contract/OCRWeb.Pdf.Contract.csproj", "src/Backend/OCRWeb.Pdf.Contract/"]
COPY ["src/Backend/OCRWeb.ProjectManagement/OCRWeb.ProjectManagement.csproj", "src/Backend/OCRWeb.ProjectManagement/"]
COPY ["src/Backend/OCRWeb.ProjectManagement.Contract/OCRWeb.ProjectManagement.Contract.csproj", "src/Backend/OCRWeb.ProjectManagement.Contract/"]
RUN dotnet restore "src/Backend/OCRWeb.API/OCRWeb.API.csproj"

COPY . .
RUN dotnet publish "src/Backend/OCRWeb.API/OCRWeb.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OCRWeb.API.dll"]
