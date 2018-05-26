using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace chat_server
{
    public class ApiController : Controller
    {
        // ------------ DB Injection --------------
        private readonly Models.DBInterface database;
        // Magic happens here (Injection)
        public ApiController(Models.DBInterface existingDatabase)
        {
            database = existingDatabase;
        }

        // ---------------  API -------------------
        // GET api/get_token/username={username}&password={password}
        // Получает пару username + password
        // Если пользователь username не был зарегистрирован, регистрирует его
        // и выдает новый токен
        // Если был зарегистрирован, то: если пароль корректный, возвращает новый токен
        //                               если нет, то возвращает строку ERROR_TOKEN


        /// <summary>
        /// Checks <paramref name="username"/>-<paramref name="password"/> and returns token
        /// </summary>
        /// <param name="username">Username from Input form</param>
        /// <param name="password">Password from Input form</param>
        /// <remarks>
        /// If user exist, checks password. If password is correct, returns new token
        /// Otherwise returns "ERROR_TOKEN"
        /// If user do not exist, registers new user with usrname and password given
        /// </remarks>
        /// <returns>New Generated token OR "ERROR_TOKEN"</returns>
        /// <response code="200">Returns the newly created token if password is correct or it is new user OR ERROR_TOKEN if password was incorrect</response>
        [Route("api/get_token/username={username}&password={password}")]
        [HttpGet]
        [EnableCors("MyPolicy")]
        public string GetToken(string username, string password)
        {
            int id;
            string token;
            if (database.IsUserExist(username))
            {
                // if user exists - check pass
                id = database.GetIdFromUsername(username);
                token = database.GetToken(id, password);
            }
            else
            {
                // if user does not exist - register
                database.AddUserWithPassword(username, password);
                id = database.GetIdFromUsername(username);
                token = database.GetToken(id, password);
            }

            return token;
        }
        // GET api/send_message/for_id={forId}&message={message}
        // Принимает два параметра: ID получателя сообщения и текст сообщения
        // Возвращает -1 в случае, если токен из cookie невалидный, и
        // 0 в ином случае
        //[DisableCors]

        /*
        [Route("api/send_message/for_id={forId}&message={message}")]
        [HttpGet]
        [EnableCors("MyPolicy")]
        public int SendMessage(int forId, string message)
        {
            string cookieToken = HttpContext.Request.Cookies["token"];
            if (database.ValidateToken(cookieToken) == -1)
            {
                return -1;
            }
            else
            {
                int fromId = database.GetIdFromToken(cookieToken);
                Models.Message messageToSend = new Models.Message(message, fromId, forId);
                database.SendMessage(messageToSend);
                return 0;
            }
        }
        */

        /// <summary>
        /// Sends message from <paramref name="forId"/> with text <paramref name="message"/> and returns result code
        /// </summary>
        /// <param name="forId">Message receiver ID</param>
        /// <param name="message">Message text</param>
        /// <param name="cookieToken">Senders token from cookie</param>
        /// <produces>text/plain</produces>
        /// <returns>-1 or 0</returns>
        /// <response code="200">0 if message was sended; -1 if message was not sended</response>
        [Route("api/send_message/for_id={forId}/message={message}/cookie={cookieToken}")]
        [HttpGet]
        [EnableCors("MyPolicy")]
        public IActionResult SendMessageWithToken(int forId, string message, string cookieToken)
        {
            //string cookieToken = HttpContext.Request.Cookies["token"];
            if (database.ValidateToken(cookieToken) == -1)
            {
                return View("EmptyMessageLayout");
            }
            else
            {
                int fromId = database.GetIdFromToken(cookieToken);
                Models.Message messageToSend = new Models.Message(message, fromId, forId);
                database.SendMessage(messageToSend);
                return View("EmptyMessageLayout");
            }
        }
        // GET: /chat/MessageLayout/{id пользователя с которым открыт чат}
        // Получает id пользователя с которым открыт чат
        // Валидирует токен из cookie. Если Токен невалидный, редиректит в /
        // Если токен валидный, то:
        // Если user еще не выбран (id = -1, логика в JS)
        // то возвращает EmptyMessageLayout.cshtml
        // Если выбран, о возвращает MessageLayout.cshtml, пробрасывая туда
        // messages, usernames, id собеседника
        // MessageLayout.cshtml - внутреность контейнера (div) со списком сообщений
        // Вставляется в контейнер с помощью JS
        // [DisableCors]
        /*
        [Route("/chat/MessageLayout/{id}")]
        [HttpGet]
        [EnableCors("MyPolicy")]
        public IActionResult MessageLayout(int id)
        {
            string cookieToken = HttpContext.Request.Cookies["token"];
            if (database.ValidateToken(cookieToken) == -1)
            {
                return Redirect("/");
            }
            if (id == -1)
            {
                return View("EmptyMessageLayout");
            }
            int my_id = database.GetIdFromToken(cookieToken);
            List<Models.Message> messageList = database.GetMessagesList(my_id, id);
            ViewData["UsernamesDict"] = database.GetUsernamesDict();
            ViewData["MessageList"] = database.GetMessagesList(my_id, id);
            ViewData["CurrentFriendId"] = id;
            return View("MessageLayout");
        }
        */
        /// <summary>
        /// DEPRECATED! CURRENT RAZOR VERSION IS NOT WORKING IN DOCKER! Returns html container with messages from user <paramref name="id"/>
        /// </summary>
        /// <param name="id">ID of user (receiver) or -1 if user is not selected</param>
        /// <param name="cookieToken">Senders token from cookie</param>
        /// <remarks>
        /// Creates view from file MessageLayot if <paramref name="id"/>d != -1
        /// Or EmptyMessageLayout if <paramref name="id"/> = -1
        /// Can return 403 if token is not valid
        /// </remarks>
        /// <returns>HTML container with messages from user <paramref name="id"/></returns>
        /// <response code="200">HTML container with messages from user <paramref name="id"/></response>
        /// <response code="403">Cookie was not valid</response>
        /*
        [Route("/chat/MessageLayoutOld/{id}/cookie={cookie_token}")]
        [HttpGet]
        [EnableCors("MyPolicy")]
        public IActionResult MessageLayoutCookieOld(int id, string cookie_token)
        {
            string cookieToken = cookie_token;
            if (database.ValidateToken(cookieToken) == -1)
            {
                return Forbid();
            }
            if (id == -1)
            {
                return View("EmptyMessageLayout");
            }
            int my_id = database.GetIdFromToken(cookieToken);
            List<Models.Message> messageList = database.GetMessagesList(my_id, id);
            ViewData["UsernamesDict"] = database.GetUsernamesDict();
            ViewData["MessageList"] = database.GetMessagesList(my_id, id);
            ViewData["CurrentFriendId"] = id;
            return View("MessageLayout");
        }
        */
        // GET: /chat/UserLayout
        // Валидирует токен из cookie. Если Токен невалидный, редиректит в /
        // Если токен валидный, то отдаёт UserLayout.cshtml, пробрасывая туда
        // users
        // UserLayout.cshtml - внутреность контейнера (div) со списком пользователей
        // Вставляется в контейнер с помощью JS
        //[DisableCors]

        /// <summary>
        /// DEPRECATED! CURRENT RAZOR VERSION IS NOT WORKING IN DOCKER! Returns html container with registred users list
        /// </summary>
        /// <returns>HTML container with registred users list</returns>
        /// <response code="200">HTML container with messages from user <paramref name="id"/></response>
        /*
        [Route("chat/UserLayoutOld")]
        [HttpGet]
        [EnableCors("MyPolicy")]
        public IActionResult UserLayout()
        {
            //string cookieToken = HttpContext.Request.Cookies["token"];
            //if (database.ValidateToken(cookieToken) == -1)
            //{
            //    return Redirect("/");
            //}
            //int my_id = database.GetIdFromToken(cookieToken);
            ViewData["UsersDict"] = database.GetUsersDict();
            return View("UserLayout");
        }
        */
        /// <summary>
        /// Returns html container with registred users list WITHOUT USING RAZOR
        /// </summary>
        /// <returns>HTML container with registred users list</returns>
        /// <response code="200">HTML container with messages from user <paramref name="id"/></response>
        [Route("chat/UserLayout")]
        [HttpGet]
        [EnableCors("MyPolicy")]
        public string UserLayoutNew()
        {
            string s = "<div class=usersPanel><ul class=usersPanelList>";
            foreach (KeyValuePair<string, int> rec in database.GetUsersDict()) {
                s += "<li><a onclick='selectChat(" + rec.Value + ")' class=userLink'>" + rec.Key + "</a></li>";
            }
            s += "</ul></div >";
            //ViewData["UsersDict"] = database.GetUsersDict();
            return s;
        }

        /// <summary>
        /// Returns html container with messages from user <paramref name="id"/>
        /// </summary>
        /// <param name="id">ID of user (receiver) or -1 if user is not selected</param>
        /// <param name="cookieToken">Senders token from cookie</param>
        /// <remarks>
        /// Creates view from file MessageLayot if <paramref name="id"/>d != -1
        /// Or EmptyMessageLayout if <paramref name="id"/> = -1
        /// Can return 403 if token is not valid
        /// </remarks>
        /// <returns>HTML container with messages from user <paramref name="id"/></returns>
        /// <response code="200">HTML container with messages from user <paramref name="id"/></response>
        /// <response code="403">Cookie was not valid</response>
        [Route("/chat/MessageLayout/{id}/cookie={cookie_token}")]
        [HttpGet]
        [EnableCors("MyPolicy")]
        public string MessageLayoutCookie(int id, string cookie_token)
        {
            string cookieToken = cookie_token;
            if (database.ValidateToken(cookieToken) == -1)
            {
                return "ERROR";
            }
            if (id == -1)
            {
                string empty_s = "<div class='emptyMessageList'><span class='emptyMessageListSpan'>";
                empty_s += "Please, select user you want to chat with</span></div>";
                return empty_s;
            }
            int my_id = database.GetIdFromToken(cookieToken);
            List<Models.Message> messageList = database.GetMessagesList(my_id, id);
            Dictionary<int, string> UsernamesDict = database.GetUsernamesDict();

            string s = "<div class='messagesPanelContainer'>";
            s += "<div class='friendUsername'><span class='friendUsernameSpan'>Chat with <b>";
            s += UsernamesDict[id];
            s += "</b></span></div><div class='messageListContainer' id='messageListContainer'>";
            foreach (chat_server.Models.Message message in messageList) {
                s += "<div class='messageContainer'><b class='userStamp'>";
                s += UsernamesDict[message.from_id];
                s += "</b><div class='messageText'>";
                s += message.text;
                s += "</div><i class='dateTimeStamp'>";
                s += message.sendTime.ToString();
                s += "</i></div>";
            }
            s += "</ div ></ div >";
            return s;
        }
    }
}
