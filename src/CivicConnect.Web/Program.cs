using CivicConnect;
var builder = WebApplication.CreateBuilder(args);
if (!builder.Environment.IsDevelopment())
    throw new InvalidOperationException("This slice requires Development. Integrate persistent storage and identity provisioning before deployment.");
builder.Services.AddCivicConnect();
var app = builder.Build();
app.UseCivicConnect();
app.Run();
