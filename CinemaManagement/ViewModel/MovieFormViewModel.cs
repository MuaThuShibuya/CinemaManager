using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CinemaManagement.Models;
using Microsoft.AspNetCore.Http;

namespace CinemaManagement.ViewModel
{
    public class MovieFormViewModel : IValidatableObject
    {
        public Movie Movie { get; set; }

        // ===== UPLOAD FILE =====
        public IFormFile? PosterFile { get; set; }   // Poster 3:4
        public IFormFile? BannerFile { get; set; }  // Banner 16:9

        // ===== GENRES =====
        public List<int> SelectedGenres { get; set; } = new();
        public List<Genre> Genres { get; set; } = new();
        public DateTime? StartTime { get; set; }

        // ===== VALIDATION =====
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Poster 3:4: cho phép URL hoặc File hoặc null
            if (string.IsNullOrWhiteSpace(Movie?.PosterUrl) && PosterFile == null)
            {
                yield return new ValidationResult(
                    "Poster (3:4) cần URL hoặc upload file.",
                    new[] { nameof(Movie.PosterUrl), nameof(PosterFile) }
                );
            }

            // Banner 16:9: cho phép URL hoặc File hoặc null
            if (string.IsNullOrWhiteSpace(Movie?.BannerUrl) && BannerFile == null)
            {
                yield return new ValidationResult(
                    "Banner (16:9) cần URL hoặc upload file.",
                    new[] { nameof(Movie.BannerUrl), nameof(BannerFile) }
                );
            }
        }
    }
}
