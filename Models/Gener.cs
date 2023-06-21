using System;
using System.Collections.Generic;

namespace CimdoApi.Models;

public partial class Gener
{
    public int IdGener { get; set; }

    public string? Gener1 { get; set; }

    public virtual ICollection<BooksGener> BooksGeners { get; set; } = new List<BooksGener>();

    public virtual ICollection<UsersGener> UsersGeners { get; set; } = new List<UsersGener>();
}
