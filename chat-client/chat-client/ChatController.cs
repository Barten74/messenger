using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860


namespace chatclient
{
    public class ChatController : Controller
    {
        [HttpGet("/")]
        public IActionResult Auth()
        {
            return Redirect("Index.html");
        }
        [HttpGet("/chat/")]
        public IActionResult Chat()
        {
            //string cookieToken = HttpContext.Request.Cookies["token"];
            //if (database.ValidateToken(cookieToken) == -1)
            //{
            //    return Redirect("/");
            //}
            return Redirect("/chat/Index.html");
        }
    }
}
