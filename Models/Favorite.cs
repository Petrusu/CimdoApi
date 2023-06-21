using System;
using System.Collections.Generic;

namespace CimdoApi.Models;

public partial class Favorite
{
    public int IdUser { get; set; }

    public int IdBook { get; set; }

    public string? Note { get; set; }

    public virtual Book IdBookNavigation { get; set; } = null!;

    public virtual User IdUserNavigation { get; set; } = null!;
}
