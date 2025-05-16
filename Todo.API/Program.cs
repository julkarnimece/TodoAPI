using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Todo.API.InMemoryDataStore;
using Todo.API.Models;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, MyAppJsonContext.Default);
});


//builder.Services.ConfigureHttpJsonOptions(options =>
//{
//    options.SerializerOptions.ReferenceHandler = ReferenceHandler.Preserve; 
    
//});

builder.Services.AddSingleton<Todo.API.InMemoryDataStore.TodoStore>();


var app = builder.Build();


app.Use(async (context, next) =>
{
    await next();
    Console.WriteLine($"{context.Request.Method} {context.Request.Path} responded {context.Response.StatusCode}");
});

app.MapGet("/todos", (TodoStore store) =>
{
    return Results.Ok(store.GetAll());
});

app.MapGet("/todos/{id:guid}", (TodoStore store, Guid id) =>
{
    var todo = store.Get(id);
    return todo is not null ? Results.Ok(todo) : Results.NotFound();
});

app.MapPost("/todos", async (TodoStore store, HttpContext ctx) =>
{
    var data = await ctx.Request.ReadFromJsonAsync<CreateTodoDto>();
    if (data is null || string.IsNullOrWhiteSpace(data.Text)) return Results.BadRequest();

    var todo = store.Create(data.Text);
    return Results.Created($"/todos/{todo.Id}", todo);
});

app.MapPut("/todos/{id:guid}", async (TodoStore store, Guid id, HttpContext ctx) =>
{
    var update = await ctx.Request.ReadFromJsonAsync<UpdateTodoDto>();
    if (update is null) return Results.BadRequest();

    var updated = store.Update(id, update.Text, update.IsComplete);
    return updated ? Results.NoContent() : Results.NotFound();
});

app.MapDelete("/todos/{id:guid}", (TodoStore store, Guid id) =>
{
    return store.Delete(id) ? Results.NoContent() : Results.NotFound();
});


app.Run();

record CreateTodoDto(string Text);
record UpdateTodoDto(string Text, bool IsComplete);


[JsonSerializable(typeof(IEnumerable<TodoItem>))]
[JsonSerializable(typeof(TodoItem))]
public partial class MyAppJsonContext : JsonSerializerContext 
{
    public MyAppJsonContext(JsonSerializerOptions? options) : base(options)
    {
    }
}
