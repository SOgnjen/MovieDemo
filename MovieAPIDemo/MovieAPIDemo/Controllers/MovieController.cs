using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieAPIDemo.Data;
using MovieAPIDemo.Entities;
using MovieAPIDemo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MovieAPIDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly MovieDbContext _context;
        private readonly IMapper _mapper;

        public MovieController(MovieDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Get(int pageIndex = 0, int pageSize = 10)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                var movieCount = _context.Movie.Count();
                var movieList = _mapper.Map<List<MovieListViewModel>> (_context.Movie.Include(x => x.Actors).Skip(pageIndex * pageSize).Take(pageSize).ToList());

                response.Status = true;
                response.Message = "Success";
                response.Data = new {Movies = movieList, Count = movieCount};

                return Ok(response);
            }
            catch (System.Exception)
            {
                response.Status = false;
                response.Message = "Something went wrong";
                
                return BadRequest(response);
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetMovieById(int id)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                var movie = _context.Movie.Include(x => x.Actors).Where(x => x.Id == id).FirstOrDefault();

                if(movie == null)
                {
                    response.Status = false;
                    response.Message = "Record Not Exist.";

                    return BadRequest(response);
                }

                var movieData = _mapper.Map<MovieDetailsViewModel>(movie);

                response.Status = true;
                response.Message = "Success";
                response.Data = movieData;

                return Ok(response);
            }
            catch (System.Exception)
            {
                response.Status = false;
                response.Message = "Something went wrong";

                return BadRequest(response);
            }
        }

        [HttpPost]
        public IActionResult Post(CreateMovieViewModel model) 
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                if(ModelState.IsValid)
                {
                    var actors = _context.Person.Where(x => model.Actors.Contains(x.Id)).ToList();

                    if(actors.Count() != model.Actors.Count())
                    {
                        response.Status = false;
                        response.Message = "Invalid Actor assigned";

                        return BadRequest(response);
                    }

                    var postedModel = _mapper.Map<Movie>(model);
                    postedModel.Actors = actors;

                    _context.Movie.Add(postedModel);
                    _context.SaveChanges();

                    var responseData = _mapper.Map<MovieDetailsViewModel>(postedModel);

                    response.Status = true;
                    response.Message = "Created successfully";
                    response.Data = responseData;

                    return Ok(response);
                }
                else
                {
                    response.Status = false;
                    response.Message = "Validation failed.";
                    response.Data = ModelState;

                    return BadRequest(response);
                }
            }
            catch (System.Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong.";

                return BadRequest(response);
            }
        }

        [HttpPut]
        public IActionResult Put(CreateMovieViewModel model)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                if (ModelState.IsValid)
                {
                    if (model.Id <= 0)
                    {
                        response.Status = false;
                        response.Message = "Invalid Movie Record";

                        return BadRequest(response);
                    }

                    var actors = _context.Person.Where(x => model.Actors.Contains(x.Id)).ToList();

                    if (actors.Count() != model.Actors.Count())
                    {

                        response.Status = false;
                        response.Message = "Invalid Actor assigned";

                        return BadRequest(response);
                    }

                    var movieDetails = _context.Movie.Include(x => x.Actors).Where(x => x.Id == model.Id).FirstOrDefault();

                    if (movieDetails == null)
                    {
                        response.Status = false;
                        response.Message = "Invalid Movie Record";

                        return BadRequest(response);
                    }

                    movieDetails.CoverImage = model.CoverImage;
                    movieDetails.Description = model.Description;
                    movieDetails.Language = model.Language;
                    movieDetails.ReleaseDate = model.ReleaseDate;
                    movieDetails.Title = model.Title;

                    // Find removed actors

                    var removedActors = movieDetails.Actors.Where(x => !model.Actors.Contains(x.Id)).ToList();

                    foreach (var actor in removedActors)
                    {
                        movieDetails.Actors.Remove(actor);
                    }

                    // Find added actors
                    var addedActors = actors.Except(movieDetails.Actors).ToList();

                    foreach (var actor in addedActors)
                    {
                        movieDetails.Actors.Add(actor);
                    }

                    _context.SaveChanges();

                    var responseData = new MovieDetailsViewModel
                    {
                        Id = movieDetails.Id,
                        Title = movieDetails.Title,
                        Actors = movieDetails.Actors.Select(y => new ActorViewModel
                        {
                            Id = y.Id,
                            Name = y.Name,
                            DateOfBirth = y.DateOfBirth
                        }).ToList(),
                        CoverImage = movieDetails.CoverImage,
                        Language = movieDetails.Language,
                        ReleaseDate = movieDetails.ReleaseDate,
                        Description = movieDetails.Description
                    };

                    response.Status = true;
                    response.Message = "Updated successfully";
                    response.Data = responseData;

                    return Ok(response);
                }
                else
                {
                    response.Status = false;
                    response.Message = "Validation failed.";
                    response.Data = ModelState;

                    return BadRequest(response);
                }
            }
            catch (System.Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong.";

                return BadRequest(response);
            }
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                var movie = _context.Movie.Where(x => x.Id == id).FirstOrDefault();

                if(movie == null)
                {
                    response.Status = false;
                    response.Message = "Invalid Movie Record.";

                    return BadRequest(response);
                }

                _context.Movie.Remove(movie);
                _context.SaveChanges();

                response.Status = true;
                response.Message = "Deleted successfully.";
                
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong";

                return BadRequest(response);
            }
        }

        [HttpPost]
        [Route("upload-movie-poster")]
        public async Task<IActionResult> UploadMoviePoster(IFormFile imageFile)
        {
            try
            {
                var filename = ContentDispositionHeaderValue.Parse(imageFile.ContentDisposition).FileName.TrimStart('\"').TrimEnd('\"');
                string newPath = @"C:\to-delete";

                if(!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }

                string[] allowedImageExtensions = new string[] { ".jpg", ".jpeg", ".png" };

                if(!allowedImageExtensions.Contains(Path.GetExtension(filename)))
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Status = false,
                        Message = "Only .jpg, .jpeg and .png type files are allowed"
                    });
                }

                string newFileName = Guid.NewGuid() + Path.GetExtension(filename);
                string fullFilePath = Path.Combine(newPath, newFileName);

                using(var stream = new FileStream(fullFilePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                return Ok(new { ProfileImage = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/StaticFiles/{newFileName}" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new BaseResponseModel
                {
                    Status = false,
                    Message = "Error Occured"
                });
            }
        }
    }
}
