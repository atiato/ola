using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TryApp.Models
{
    public class PlayItem
    {

        public long Id { get; set; }
        public string Name { get; set; }

        public string game { get; set; }

        public bool IsComplete { get; set; }
    }
}
