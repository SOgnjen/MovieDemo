using AutoMapper;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MovieAPIDemo.Entities;
using MovieAPIDemo.Models;

namespace MovieAPIDemo
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Movie, MovieListViewModel>();
            CreateMap<Movie, MovieDetailsViewModel>();
            CreateMap<MovieListViewModel, Movie>();
            CreateMap<CreateMovieViewModel, Movie>().ForMember(x => x.Actors, y => y.Ignore());

            CreateMap<Person, ActorViewModel>();
            CreateMap<Person, ActorDetailsViewModel>();
            CreateMap<ActorViewModel, Person>();
        }
    }
}
