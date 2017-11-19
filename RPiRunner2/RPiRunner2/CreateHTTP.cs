using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiRunner2
{
    class CreateHTTP
    {
        public static string Code200_Ok(string content)
        {
            string response = "";
            response += "HTTP/1.0 200 OK\r\n";
            response += "Server: TAU_IoT_Workshop\r\n";
            response += "Content-Type: text/html\r\n";
            response += "Content-Length: " + content.Length.ToString() + "\r\n";
            response += "\r\n";
            response += content;
            return response;
        }
        public static string Code204_NoContent()
        {
            string response = "";
            response += "HTTP/1.1 204 No Content\r\n";
            response += "Server: TAU_IoT_Workshop\r\n";
            response += "Content-Length: 0\r\n";
            response += "\r\n\r\n";
            return response;
        }
        public static string Code301_MovedPermanently(string url)
        {
            string response = "";
            response += "HTTP/1.1 301 Moved Permanently\r\n";
            response += "Location: " + url + "\r\n\r\n\r\n";
            return response;
        }
        public static string Code302_MovedTemporarily(string url)
        {
            string response = "";
            response += "HTTP/1.1 302 Moved Temporarily\r\n";
            response += "Location: " + url + "\r\n\r\n\r\n";
            return response;
        }
        public static string Code400_BadRequest()
        {
            string response = "HTTP/1.1 400 Bad Request\r\n";
            response += "Date: " + DateTime.Now.ToString() + "\r\n";
            response += "Server: TAU_IoT_Workshop\r\n";
            response += "Content-Length: 0\r\n";
            response += "Connection: Closed\r\n\r\n\r\n";
            return response;
        }
        public static string Code404_NotFound()
        {
            string response = "HTTP/1.1 404 Not Found\r\n";
            response += "Date: " + DateTime.Now.ToString() + "\r\n";
            response += "Server: TAU_IoT_Workshop\r\n";
            response += "Content-Length: 0\r\n";
            response += "Connection: Closed\r\n\r\n\r\n";
            return response;
        }
        //before popping the login window
        public static string Code401_Unauthorized()
        {
            return "HTTP/1.1 401 Unauthorized\r\n" +
                    "Server: TAU_IoT_Workshop\r\n" +
                    "Content-Length: 0\r\n" +
                    "WWW-Authenticate: Basic realm=\"logtorpi\"\r\n\r\n\r\n";
        }
    }
}
