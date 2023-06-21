using System;
using System.Collections.Generic;

namespace CimdoApi.Models;

public partial class Author
{
    public int IdAuthor { get; set; }

    public string? Author1 { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
