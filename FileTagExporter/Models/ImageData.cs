using System.Collections.Generic;

namespace FileTagExporter.Models;

public class ImageData 
{
    public ImageData(string location)
    {
        Location = location;
    }

    public string? Location { get; set; }
    
    public string? Title { get; set; }

    public List<string>? Tags { get; set; }

    public string? Status { get; set; }
}
