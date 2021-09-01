using System.Collections.Generic;

namespace CheckListBot
{
    public class CheckList
    {
        public List<Items> Lists { get; set; } = new List<Items>();
    }
    public class Items
    {
        public string Title { get; set; }
        //public bool Activities { get; set; }
    }
}
