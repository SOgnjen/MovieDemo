using System.ComponentModel.DataAnnotations;
using System;

namespace MovieAPIDemo.Models
{
    public class ActorViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}
