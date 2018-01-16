using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TryApp.Models;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

namespace TryApp.Controllers
{
    [Authorize]
    [Route("api/rest")]
    public class TodoController : Controller
    {
        //      private readonly TodoContext _context;
        /*     [HttpGet]
           public IEnumerable<TodoItem> GetAll()
            {
                return _context.TodoItems.ToList();
            }

            [HttpGet("{id}", Name = "GetTodo")]
            public IActionResult GetById(long id)
            {
                var item = _context.TodoItems.FirstOrDefault(t => t.Id == id);
                if (item == null)
                {
                    return NotFound();
                }
                return new ObjectResult(item);
            }
            */
        
        [HttpPost]
        public IActionResult Create([FromBody] TodoItem item)
        {
            if (item == null)
            {
                return BadRequest();
            }

            PlayItem test = new PlayItem();
            test.Id = 0;
            test.game = "PS4";
            test.Name = "OmarAtia";
            test.IsComplete = false;
            //    MusicStoreContext trial= new MusicStoreContext("server=localhost;Password=sasql;Persist Security Info=True;User ID=sa;Initial Catalog=test;Data Source=.");


       /*     StoreContext context = HttpContext.RequestServices.GetService(typeof(TryApp.Models.StoreContext)) as StoreContext;

           string Item= context.GetAllAlbums().FirstOrDefault().Item.Trim();
            string Name= context.GetAllAlbums().FirstOrDefault().Name.Trim();

            Store trial = new Store();
            trial.Item = Item;
            trial.Name = Name;
            trial.Id = context.GetAllAlbums().FirstOrDefault().Id;
            trial.NoOfDays = context.GetAllAlbums().FirstOrDefault().NoOfDays.Trim();*/


            return new ObjectResult(test);


          //  MusicStoreContext trial= new MusicStoreContext();

         //   Album raq = trial.GetAllAlbums().FirstOrDefault();
          //  _context.TodoItems.Add(item);

            //   _context.TodoItems.Add(FirstName);
            //   _context.TodoItems.Add(LastName);

            // _context.SaveChanges();

            //  return CreatedAtRoute("GetTodo", new { id = item.Id }, item);
            //  return new ObjectResult(test);
          //  return new ObjectResult(raq);

        }





        /*  [HttpPut("{id}")]
          public IActionResult Update(long id, [FromBody] TodoItem item)
          {
              if (item == null || item.Id != id)
              {
                  return BadRequest();
              }

              var todo = _context.TodoItems.FirstOrDefault(t => t.Id == id);
              if (todo == null)
              {
                  return NotFound();
              }

              todo.IsComplete = item.IsComplete;
              todo.Name = item.Name;

              _context.TodoItems.Update(todo);
              _context.SaveChanges();
              return new NoContentResult();
          }

          [HttpDelete("{id}")]
  public IActionResult Delete(long id)
  {
      var todo = _context.TodoItems.FirstOrDefault(t => t.Id == id);
      if (todo == null)
      {
          return NotFound();
      }

      _context.TodoItems.Remove(todo);
      _context.SaveChanges();
      return new NoContentResult();
  }
          public TodoController(TodoContext context)
          {
              _context = context;

              if (_context.TodoItems.Count() == 0)
              {
                  _context.TodoItems.Add(new TodoItem { Name = "Item1" });
                  _context.SaveChanges();
              }
          }*/
    }
}