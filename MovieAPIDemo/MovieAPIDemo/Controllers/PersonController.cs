﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieAPIDemo.Data;
using MovieAPIDemo.Entities;
using MovieAPIDemo.Models;
using System.Collections.Generic;
using System.Linq;

namespace MovieAPIDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        private readonly MovieDbContext _context;
        private readonly IMapper _mapper;

        public PersonController(MovieDbContext context, IMapper mapper)
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
                var actorCount = _context.Person.Count();
                var actorList = _mapper.Map<List<ActorViewModel>>(_context.Person.Skip(pageIndex * pageSize).Take(pageSize).ToList());

                response.Status = true;
                response.Message = "Success";
                response.Data = new { Person = actorList, Count = actorCount };

                return Ok(response);
            }
            catch (System.Exception ex)
            {
                response.Status = false;
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

                if (person == null)
                {
                    response.Status = false;
                    response.Message = "Record Not Exist";

                    return BadRequest(response);
                }

                var personData = _mapper.Map<ActorDetailsViewModel>(person);
                personData.Movies = _context.Movie.Where(x => x.Actors.Contains(person)).Select(x => x.Title).ToArray();

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

        [HttpGet]
        [Route("Search/{searchText}")]
        public IActionResult Get(string searchText)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                var searchedPerson = _context.Person.Where(x => x.Name.Contains(searchText)).Select(x => new
                {
                    x.Id,
                    x.Name
                }).ToList();

                response.Status = true;
                response.Message = "Success";
                response.Data = searchedPerson;

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

        [HttpPut]
        public IActionResult Put(ActorViewModel model)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                if (ModelState.IsValid)
                {
                    var postedModel = _mapper.Map<Person>(model);

                    if (model.Id <= 0)
                    {
                        response.Status = false;
                        response.Message = "Invalid Person Record";

                        return BadRequest(response);
                    }

                    var personDetails = _context.Person.Where(x => x.Id == model.Id).AsNoTracking().FirstOrDefault();
                    if (personDetails == null)
                    {
                        response.Status = false;
                        response.Message = "Invalid person record";

                        return BadRequest(response);
                    }

                    _context.Person.Update(postedModel);
                    _context.SaveChanges();

                    response.Status = true;
                    response.Message = "Updated successfully";
                    response.Data = postedModel;

                    return Ok(response);
                }
                else
                {
                    response.Status = false;
                    response.Message = "Model not valid";
                    response.Data = ModelState;

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

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            BaseResponseModel response = new BaseResponseModel();

            try
            {
                var person = _context.Person.Where(x => x.Id == id).FirstOrDefault();
                if(person == null)
                {
                    response.Status = false;
                    response.Message = "Invalid Person Record";

                    return BadRequest(response);
                }

                _context.Person.Remove(person);
                _context.SaveChanges();

                response.Status = true;
                response.Message = "Deleted successfully";

                return Ok(response);
            }
            catch (System.Exception)
            {
                response.Status = false;
                response.Message = "Something went wrong";

                return BadRequest(response);
            }
        }
    }
}
