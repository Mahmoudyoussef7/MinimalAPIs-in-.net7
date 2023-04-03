using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ToDoDb>(opt => opt.UseInMemoryDatabase("TodoList"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Hello in Minimal APIs");
app.MapGet("/todoitems", async (ToDoDb db) => await db.Todos.ToListAsync());
app.MapGet("/todoitems/complete", async (ToDoDb db) => await db.Todos.Where(i => i.IsComplete).ToListAsync());
app.MapGet("/todoitems/{id}", async (int id, ToDoDb db) => await db.Todos.FindAsync(id)
    is Todo todo ? Results.Ok(todo) : Results.NotFound());
app.MapPost("/todoitems", async (Todo todo, ToDoDb db) =>
{
    await db.Todos.AddAsync(todo);
    await db.SaveChangesAsync();
});
app.MapPut("/todoitems/{id}", async (int id, Todo input, ToDoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();
    todo.Name = input.Name;
    todo.IsComplete = input.IsComplete;
    await db.SaveChangesAsync();
    return Results.NoContent();
});
app.MapDelete("/todoitems/{id}", async (int id, ToDoDb db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok(todo);
    }
    return Results.NotFound();
});

app.Run();

class ToDoDb : DbContext
{
    public ToDoDb(DbContextOptions<ToDoDb> options) : base(options) { }

    public DbSet<Todo> Todos => Set<Todo>();
}

class Todo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
}
