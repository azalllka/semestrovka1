using System.ComponentModel.DataAnnotations.Schema;


public class Movie
{   
    public int Id { get; set; }
    public string Title { get; set; }
    public float KinopoiskRating { get; set; }
    public int KinopoiskVotes { get; set; }
    public string Lists { get; set; }
    public string ReleaseDate { get; set; }
    public string Country { get; set; }
    public string Director { get; set; }
    public string Genre { get; set; }
    public string Quality { get; set; }
    public string AgeRating { get; set; }
    public string Duration { get; set; }
    public string Series { get; set; }
    public string Description { get; set; }
    public string PosterUrl { get; set; }
    public string TrailerUrl { get; set; }
    
}