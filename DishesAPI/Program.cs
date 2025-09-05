using DishesAPI.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using DishesAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add services to the container.  
builder.Services.AddDbContext<DishesDbContext>(o => o.UseSqlite(
    builder.Configuration["ConnectionStrings:DishesDBConnectionString"]));
builder.Services.AddAutoMapper(cfg => { }, typeof(Program).Assembly);

var app = builder.Build();






// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.MapGet("/dishes", async (DishesDbContext dishesDbContext,ClaimsPrincipal claimsPrincipal, IMapper mapper, string ? search, string? sortBy, string? sortOrder, int pageNumber = 1, int pageSize = 5) =>
{
    Console.WriteLine($"user not authenticated? {claimsPrincipal.Identity?.IsAuthenticated}");

    // 1. Base query
    var query = dishesDbContext.Dishes.AsQueryable();

    // 2. Filtering / Searching
    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(d => d.Name.Contains(search));
    }

    // 3. Sorting
    if (!string.IsNullOrWhiteSpace(sortBy))
    {
        switch (sortBy.ToLower())
        {
            case "name":
                query = sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(d => d.Name)
                    : query.OrderBy(d => d.Name);
                break;

            case "id":
                query = sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(d => d.Id)
                    : query.OrderBy(d => d.Id);
                break;

            default:
                query = query.OrderBy(d => d.Name); // fallback
                break;
        }
    }
    else
    {
        query = query.OrderBy(d => d.Name); // default sort
    }

    // 4. Paging
    var totalCount = await query.CountAsync();
    var dishes = await query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    // 5. Return paged response
    var result = new
    {
        TotalCount = totalCount,
        PageNumber = pageNumber,
        PageSize = pageSize,
        Items = mapper.Map<IEnumerable<DishDto>>(dishes)
    };

    return TypedResults.Ok(result);
});

app.MapGet("/dishes/{dishid:guid}", async Task<Results<NotFound, Ok<DishDto>>> (DishesDbContext dishesDbContext,IMapper mapper, Guid dishid) =>
{
    var dishEntity = await dishesDbContext.Dishes.FirstOrDefaultAsync(d => d.Id == dishid);
    if (dishEntity == null)
    {
        return TypedResults.NotFound();
    }
    return TypedResults.Ok(mapper.Map<DishDto>( dishEntity));
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


