using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MovieAPIDemo.Data;
using MovieAPIDemo.Entities;
using MovieAPIDemo.Models;
using System.Linq;

namespace MovieAPIDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        private readonly MovieDbContext _context;

        public PersonController(MovieDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get(int pageIndex = 0, int pageSize = 10)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                var actorCount = _context.Person.Count();
                var actorList = _context.Person.Skip(pageIndex * pageSize).Take(pageSize).Select(x => new ActorViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    DateOfBirth = x.DateOfBirth
                }).ToList();

                response.Status = true;
                response.Message = "Success";
                response.Data = new { Person = actorList, Count = actorCount };

                return Ok(response);
            }
            catch(System.Exception ex)
            {
                response.Status =false;
                response.Message = "Something went wrong";

                return BadRequest(response);
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetPersonById(int id)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                var person = _context.Person.Where(x => x.Id == id).FirstOrDefault();

                if(person == null)
                {
                    response.Status = false;
                    response.Message = "Record Not Exist";

                    return BadRequest(response);
                }

                var personData = new ActorDetailsViewModel
                {
                    Id = person.Id,
                    DateOfBirth = person.DateOfBirth,
                    Name = person.Name,
                    Movies = _context.Movie.Where(x => x.Actors.Contains(person)).Select(x => x.Title).ToArray()
                };

                response.Status = true;
                response.Message = "Success";
                response.Data = personData;

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
        public IActionResult Post(ActorViewModel model) 
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                if (ModelState.IsValid)
                {
                    var postedModel = new Person
                    {
                        Name = model.Name,
                        DateOfBirth = model.DateOfBirth
                    };

                    _context.Person.Add(postedModel);
                    _context.SaveChanges();

                    model.Id = postedModel.Id;

                    response.Status = true;
                    response.Message = "Success";
                    response.Data = model;

                    return Ok(response);
                }
                else
                {
                    response.Status = false;
                    response.Message = "Validation failed";

                    return BadRequest(response);
                }
            }
            catch (System.Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong";

                return BadRequest(response);
            }
        }
    }
}
