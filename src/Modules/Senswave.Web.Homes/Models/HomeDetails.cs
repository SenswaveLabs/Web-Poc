namespace Senswave.Web.Homes.Models;

public class HomeDetails 
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsOwner { get; set; }
    public DataSource? DataSource { get; set; }
    public Location? Location { get; set; }
    public List<Room> Rooms { get; set; } = [];
}

public class DataSource
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}

public class Location
{
    public double Longitude { get; set; }
    public double Latitude { get; set; }
}

public class Room
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
