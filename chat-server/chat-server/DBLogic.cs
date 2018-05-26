using System;
using System.Collections.Generic; // Dictionary

namespace chat_server.Models
{
    public interface DBInterface {
        bool IsUserExist(string username);
        int GetIdFromUsername(string username);
        void AddUserWithPassword(string username, string password);
        string GetToken(int id, string password);
        Dictionary<string, int> GetUsersDict();
        List<Message> GetMessagesList(int user_id, int friend_id);
        void SendMessage(Message new_message);
        int ValidateToken(string token);
        int GetIdFromToken(string token);
        Dictionary<int, string> GetUsernamesDict();
    }

    public class Message
    {
        public string text;
        public int from_id;
        public int to_id;
        public DateTime sendTime;

        public Message(string new_text, int new_from_id, int new_to_id) {
            sendTime = DateTime.Now;
            text = new_text;
            from_id = new_from_id;
            to_id = new_to_id;
        }
    }

    public class DB : DBInterface {
        // ----------- Public methods -----------

        // Проверяет, существует ли пользователь по имени пользователя
        // True - существует, ...
        public bool IsUserExist(string username)
        {
            return users.ContainsKey(username);
        }

        // Возвращает ID пользователя по имени пользователя
        public int GetIdFromUsername(string username) {
            return users[username];
        }

        // Возвращает ID по токену
        // -1, если токен не валидный
        public int GetIdFromToken(string token)
        {
            if (!tokens.ContainsKey(token))
            {
                return -1;
            }
            return tokens[token];
        }

        // Добавляет нового пользователя в базу
        public void AddUserWithPassword(string username, string password)
        {
            // Generate unique id
            int new_key = 0;
            for (int err_count = 0; err_count < 10000; err_count++)
            {
                new_key = random.Next(0, 2147483647);
                if (!passwords.ContainsKey(new_key))
                {
                    break;
                }
            }
            users.Add(username, new_key);
            usernames.Add(new_key, username);
            passwords.Add(new_key, password);
            messages.Add(new_key, new Dictionary<int, List<Message>>());
            return;
        }

        // Создаёт и возвращает новый токен при авторизации (по паре имя пользователя - пароль)
        public string GetToken(int id, string password)
        {
            if (passwords[id] != password)
            {
                return "ERROR_TOKEN";
            }
            string new_token = "";
            for (int err_count = 0; err_count < 10000; err_count++)
            {
                new_token = generateRandomToken();
                if (!tokens.ContainsKey(new_token))
                {
                    break;
                }
            }
            tokens.Add(new_token, id);
            return new_token;
        }

        // Возвращает словарь <имя_пользователя ID>
        public Dictionary<string, int> GetUsersDict()
        {
            return users;
        }

        // Возвращает отсортированный по времени отправки список сообщений
        // между user_id (я) и friend_id (собеседник)
        public List<Message> GetMessagesList(int user_id, int friend_id) {
            List<Message> response = new List<Message>();

            // Если собеседник - это я, то достаточно вернуть лишь часть
            // "мне" от "него"
            if (user_id == friend_id) {
                // Мне (я - user_id) - от него
                if (messages.ContainsKey(user_id))
                {
                    if (messages[user_id].ContainsKey(friend_id))
                    {
                        response.AddRange(messages[user_id][friend_id]);
                    }
                }
                response.Sort(delegate (Message m1, Message m2) {
                    return m1.sendTime.CompareTo(m2.sendTime);
                });
                return response;
            }

            // В ином случае собирает в список сообщения "мне" от "него"
            // и "ему" от "меня"

            // Мне (я - user_id) - от него
            if (messages.ContainsKey(user_id))
            {
                if (messages[user_id].ContainsKey(friend_id))
                {
                    response.AddRange(messages[user_id][friend_id]);
                }
            }
            // От меня - ему
            if (messages.ContainsKey(friend_id))
            {
                if (messages[friend_id].ContainsKey(user_id))
                {
                    response.AddRange(messages[friend_id][user_id]);
                }
            }
            // Сортировка списка сообщений по времени
            response.Sort(delegate (Message m1, Message m2) {
                return m1.sendTime.CompareTo(m2.sendTime);
                }
            );
            return response;
        }

        // Добавляет новое сообщение в базу
        public void SendMessage(Message new_message)
        {
            
            if (!messages[new_message.to_id].ContainsKey(new_message.from_id))
            {
                messages[new_message.to_id].Add(new_message.from_id, new List<Message>());
            }
            messages[new_message.to_id][new_message.from_id].Add(new_message);
        }

        // Валидирует токен
        // -1 если токен невалидный, 0 если валидный
        public int ValidateToken(string token)
        {
            if (tokens.ContainsKey(token))
            {
                return 0;
            } else 
            {
                return -1;
            }
        }

        // Возвращает словарь <ID имя_пользователя>
        public Dictionary<int, string> GetUsernamesDict()
        {
            return usernames;
        }

        // Основной конструктор
        public DB()
        {
            messages = new Dictionary<int, Dictionary<int, List<Message>>>();
        }

        // ---------------- Fields --------------

        // Словарь <имя_пользователя, ID>
        private Dictionary<string, int> users = new Dictionary<string, int>();
        // Словарь <ID, имя_пользователя>
        private Dictionary<int, string> usernames = new Dictionary<int, string>();

        // Словарь <ID, пароль>
        private Dictionary<int, string> passwords = new Dictionary<int, string>();

        // Словарь <токен, ID>
        private Dictionary<string, int> tokens = new Dictionary<string, int>();

        // Словарь со следующей структурой доступа:
        // messages[для_кого][от_кого] ---> Список сообщений
        private Dictionary<int, Dictionary<int,List<Message>>> messages;

        // Объект рандомизатора
        private static Random random = new Random((int)DateTime.Now.Ticks);

        // Длина ключа
        private const int tokenLen = 64;

        // ----------- Private methods -----------

        // Генерирует рандомный токен
        private string generateRandomToken()
        {
            string new_token = "";
            string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            for (int i = 0; i < tokenLen; i++)
            {
                new_token += alphabet[random.Next(0, alphabet.Length)];
            }
            return new_token;
        }
    }
}
