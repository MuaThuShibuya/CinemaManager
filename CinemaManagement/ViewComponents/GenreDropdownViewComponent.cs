using Microsoft.AspNetCore.Mvc;

public class GenreDropdownViewComponent : ViewComponent
{
    private readonly AppDbContext _context;

    public GenreDropdownViewComponent(AppDbContext context)
    {
        _context = context;
    }

    public IViewComponentResult Invoke()
    {
        var genres = _context.Genres
            .ToList();

        return View(genres);
    }
}
