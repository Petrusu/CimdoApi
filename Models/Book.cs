using System;
using System.Collections.Generic;

namespace CimdoApi.Models;

public partial class Book
{
    public int IdBook { get; set; }

    public string? Title { get; set; }

    public int? Author { get; set; }

    public string? Description { get; set; }

    public virtual Author? AuthorNavigation { get; set; }

    public virtual ICollection<BooksGener> BooksGeners { get; set; } = new List<BooksGener>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}
