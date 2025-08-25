using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MvcMovie.Data;
using MvcMovie.Models;

namespace MvcMovie.Controllers
{
    public class MoviesController : Controller
    {
        private readonly MvcMovieContext _context;
        private readonly ILogger<MoviesController> _logger;

        public MoviesController(MvcMovieContext context, ILogger<MoviesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Movies
        public async Task<IActionResult> Index(string movieGenre, string searchString)
        {
            if (_context.Movie == null)
            {
                _logger.LogError(new NullReferenceException(), "Movie context is null");
                return Problem("Entity set 'MvcMovieContext.Movie' is null.");
            }

            IQueryable<string> genreQuery = from m in _context.Movie
                                            orderby m.Genre
                                            select m.Genre;

            var movies = from m in _context.Movie
                         select m;

            if (!string.IsNullOrEmpty(searchString))
            {
                _logger.LogWarning("Search string provided: {SearchString}", searchString);
                movies = movies.Where(s => s.Title!.ToUpper().Contains(searchString.ToUpper()));
            }

            if (!string.IsNullOrEmpty(movieGenre))
            {
                _logger.LogDebug("Filtering by genre: {Genre}", movieGenre);
                movies = movies.Where(x => x.Genre == movieGenre);
            }

            var movieGenreVM = new MovieGenreViewModel
            {
                Genres = new SelectList(await genreQuery.Distinct().ToListAsync()),
                Movies = await movies.ToListAsync()
            };

            return View(movieGenreVM);
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogError("Details requested with null ID at {Time}", DateTimeOffset.UtcNow);
                return NotFound();
            }

            var movie = await _context.Movie.FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                _logger.LogError("Movie not found for ID {Id} at {Time}", id, DateTimeOffset.UtcNow);
                return NotFound();
            }

            return View(movie);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,ReleaseDate,Genre,Price,Rating")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Created new movie: {Title}", movie.Title);
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogDebug("Edit requested with null ID at {Time}", DateTimeOffset.UtcNow);
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                _logger.LogDebug("Edit requested but movie not found for ID {Id}", id);
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ReleaseDate,Genre,Price,Rating")] Movie movie)
        {
            if (id != movie.Id)
            {
                _logger.LogError("Edit ID mismatch at {Time}", DateTimeOffset.UtcNow);
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                    _logger.LogDebug("Successfully edited movie with ID {Id}", movie.Id);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!MovieExists(movie.Id))
                    {
                        _logger.LogWarning("Edit failed: movie ID {Id} not found", movie.Id);
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Concurrency error while editing movie ID {Id}", movie.Id);
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Delete requested with null ID");
                return NotFound();
            }

            var movie = await _context.Movie.FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                _logger.LogWarning("Delete requested but movie not found for ID {Id}", id);
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.FindAsync(id);
            if (movie != null)
            {
                _logger.LogDebug("Deleting movie with ID {Id}", movie.Id);
                _context.Movie.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
        }
    }
}