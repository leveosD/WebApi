using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("GetDeviceFromAnywhere", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader().WithMethods(["POST"]);
    });
    
    options.AddPolicy("SendDeviceInfo", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader().WithMethods(["GET"]);
    });
    
    options.AddPolicy("AnswerItself", policy =>
    {
        policy.WithOrigins("http://localhost:5141")
            .AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("GetDeviceFromAnywhere");
app.UseCors("SentDeviceInfo");
app.UseCors("AnswerItself");

List<Device> devices = new List<Device>();
devices.Add(new Device()
{
    Id = "f695ea23-8662-4a57-975a-f5afd26655db",
    Name = "Phone",
    StartTime = "00:00",
    EndTime = "00:01",
    Version = "v00"
});

devices.Add(new Device()
{
    Id = "fuckthis-shit-iamo-otno-t5afd26655db",
    Name = "Desktop",
    StartTime = "00:05",
    EndTime = "01:54",
    Version = "v01"
});

// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}*/

//app.UseHttpsRedirection();

Console.WriteLine(@"\uxxxx");
string idScheme = @"^/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";
app.Map("/devices", Devices);
/*app.Map("/add-device", () =>
{
    Device device = new Device()
    {
        Id = "1",
        Name = "Iphone(Maxim)",
        StartTime = "15:12",
        EndTime = "15:17",
        Version = "v1.0"
    };
    AddDevice(device);
});*/
app.Map("/home", Home);
    
app.Run();

void Home(IApplicationBuilder appBuilder)
{
    appBuilder.Run(async context =>
    {
        var request = context.Request;
        Console.WriteLine(request.Method + " " + request.Path + " " + request.Body + " " + request.Headers);
        await context.Response.WriteAsync("Hello world!");
    });
}

void Devices(IApplicationBuilder appBuilder)
{
    appBuilder.Run(async context =>
    {
        var request = context.Request;
        var response = context.Response;
        var path = request.Path;
        
        Console.WriteLine("Method: " + request.Method);
        Console.WriteLine("Path: " + path);

        if (Regex.IsMatch(path, idScheme) && request.Method == "GET")
        {
            Console.WriteLine("\nGet Device");
            string? id = path.Value?.Split('/')[1];
            Console.WriteLine(id);

            await GetDevice(id, response);
        }
        else if (request.Method == "GET")
        {
            Console.WriteLine("Get All Devices");
            await GetAll(response);
        }
        else if (request.Method == "POST")
            await AddDevice(request);
        else if (request.Method == "DELETE")
        {
            string? id = path.Value?.Split()[0];
            await DeleteDevice(id, response);
        }
        else
        {
            Console.WriteLine("Something went wrong :(");
            Console.WriteLine("ID: " + path.Value?.Split()[0]);
            await response.WriteAsJsonAsync(new { message = "Something went wrong :(" });
        }
    });
}

async Task GetAll(HttpResponse response)
{
    List<ShortDevice> shorts = new List<ShortDevice>();
    foreach (var device in devices)
    {
        shorts.Add(new ShortDevice() { Id = device.Id, Name = device.Name });
    }

    await response.WriteAsJsonAsync(shorts);
}

async Task GetDevice(string? id, HttpResponse response)
{
    Device? device = devices.FirstOrDefault((d => d.Id == id));
    if (device != null)
        await response.WriteAsJsonAsync(device);
    else
    {
        await response.WriteAsJsonAsync(new { message = "Device is not found." });
    }
}

async Task AddDevice(HttpRequest request)
{
    var device = await request.ReadFromJsonAsync<Device>();
    if(device != null)
        devices.Add(device);
}

async Task DeleteDevice(string? id, HttpResponse response)
{
    Device? device = devices.FirstOrDefault((d) => d.Id == id);
    if(device!= null)
        devices.Remove(device);
    await response.WriteAsJsonAsync(device);
}

public class ShortDevice
{
    public string Id { get; set; }
    public string Name { get; set; }
}

public class Device
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public string Version { get; set; }
}