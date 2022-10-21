

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        allOrigin => {
        allOrigin.AllowAnyMethod();
        allOrigin.AllowCredentials();
        allOrigin.AllowAnyHeader();
            allOrigin.WithOrigins("http://localhost", "https://localhost",
                "https://localhost:44361", "http://localhost:5000/", "http://localhost:5000")
                            .WithMethods("POST", "GET", "PUT","DELETE");
        });
});

// services.AddResponseCaching();

builder.Services.AddControllers();

builder.Services.AddRazorPages();

builder.Services.AddSignalR();


var app = builder.Build();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.UseCors(MyAllowSpecificOrigins);

//app.UseCors();

app.UseAuthorization();

app.MapRazorPages();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller}/{action}");

});

app.Run();