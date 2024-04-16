using System;
using System.Collections.Generic;

namespace prjTravel.Models;

public partial class Role
{
    public string Rid { get; set; } = null!;

    public string Rname { get; set; } = null!;

    public int Rstatus { get; set; }
}
