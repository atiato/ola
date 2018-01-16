using System;
namespace TryApp.Models
{
    public class TodoItem
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
        public bool IsComplete { get; set; }
    }
}