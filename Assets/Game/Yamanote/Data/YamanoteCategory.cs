
using System.Collections.Generic;

namespace USEN.Games.Yamanote
{
    public class YamanoteCategory
    {
        public string Name { get; set; }
        public List<YamanoteQuestion> Questions { get; set; } = new();
    }
}