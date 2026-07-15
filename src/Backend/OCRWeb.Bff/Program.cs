var builder = WebApplication.CreateBuilder(args);

// TODO: register the typed HttpClient(s) that forward to OCRWeb.API,
//       authentication boundary, and anti-corruption mapping for the frontend.

var app = builder.Build();

// TODO: map BFF endpoints that orchestrate backend command/query calls for Angular.

app.Run();
