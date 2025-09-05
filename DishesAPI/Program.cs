using DishesAPI.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using DishesAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add services to the container.  
builder.Services.AddDbContext<DishesDbContext>(o => o.UseSqlite(
    builder.Configuration["ConnectionStrings:DishesDBConnectionString"]));
builder.Services.AddAutoMapper(cfg => { }, typeof(Program).Assembly);

var app = builder.Build();






// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapGet("/dishes", async(DishesDbContext dishesDbContext,ClaimsPrincipal claimsPrincipal,IMapper mapper, string? name) =>
{
    Console.WriteLine($"user not authenticated?{claimsPrincipal.Identity?.IsAuthenticated}");
    return mapper.Map<IEnumerable<DishDto>>(await dishesDbContext.Dishes.Where(d => name ==null || d.Name.Contains(name)).ToListAsync());

});
app.MapGet("/dishes/{dishid:guid}", async (DishesDbContext dishesDbContext,IMapper mapper, Guid dishid) =>
{
    return mapper.Map<DishDto>( await dishesDbContext.Dishes.FirstOrDefaultAsync(d => d.Id == dishid));
});
app.MapGet("/dishes/{dishid}/ingrediants", async (DishesDbContext dishesDbContext, Guid dishid) =>
{
    return (await dishesDbContext.Dishes.Include(d => d.Ingredients).FirstOrDefaultAsync(d=>d.Id==dishid))?.Ingredients;
});

using (var serviceScope = app.Services.GetService<IServiceScopeFactory>().CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<DishesDbContext>();
    context.Database.EnsureDeleted();
    context.Database.Migrate();
}
app.Run();


