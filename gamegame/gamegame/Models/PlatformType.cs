using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gamegame.Models
{
    public enum PlatformType
    {
        Normal,   // Обычная платформа - нельзя цепляться
        Wall      // Стена - можно цепляться и прыгать от неё
    }
}
