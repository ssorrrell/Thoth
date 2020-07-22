using System;
using System.Collections.Generic;
using System.Text;

namespace Thoth.Models
{
    public enum MenuItemType
    {
        Browse,
        Seed,
        About
    }
    public class HomeMenuItem
    {
        public MenuItemType Id { get; set; }

        public string Title { get; set; }
    }
}
