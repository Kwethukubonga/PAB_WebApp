namespace PhilisaAbantuBethu.Models;

public class Programme
{

    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Tagline { get; set; } = "";
    public string Description { get; set; } = "";
    public string Overview { get; set; } = "";
    public string ImageId { get; set; } = "";
    public string Accent { get; set; } = "#6B21A8";
    public List<string> Objectives { get; set; } = new();
    public List<string> Activities { get; set; } = new();

}
