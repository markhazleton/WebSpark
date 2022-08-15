using ControlSpark.Domain.Entities;

namespace ControlSpark.Domain.Models;

public class AuthorItem
{
    public Author Author { get; set; }
    public bool Selected { get; set; }
}
