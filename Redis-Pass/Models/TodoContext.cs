using System;
using Microsoft.EntityFrameworkCore;

namespace TryApp.Models
{
    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions<TodoContext> options)
            : base(options)
        {
        }

        public DbSet<TodoItem> TodoItems { get; set; }
       // public DbSet<PlayItem> PlayItems { get; set; }

    }
}