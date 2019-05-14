using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
namespace DNWS
{
    public class TwitterApiPlugin : TwitterPlugin
    {
        private List<User> GetUser()
        {
            using (var context = new TweetContext())
            {
                try
                {
                    List<User> users = context.Users.Where(b => true).Include(b => b.Following).ToList();
                    return users;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public override HTTPResponse GetResponse(HTTPRequest request)
        {
            HTTPResponse response = new HTTPResponse(200);
            string user = request.getRequestByKey("user");
            string password = request.getRequestByKey("password");
            string following = request.getRequestByKey("following");
            string message = request.getRequestByKey("message");
            string[] u = request.Filename.Split("?");
            if(u[0]=="users")
            {
                if(request.Method=="GET")
                {
                    string x = JsonConvert.SerializeObject(GetUser());
                    response.body = Encoding.UTF8.GetBytes(x);
                }
                if(request.Method=="POST")
                {
                    Twitter.AddUser(user, password);
                    response.body = Encoding.UTF8.GetBytes("Susceed");
                }
                if(request.Method=="DELETE")
                {
                    Twitter.DeleteUser(user);
                    response.body = Encoding.UTF8.GetBytes("Susceed");
                }
            }
            if (u[0] == "follow")
            {
                if (request.Method == "GET")
                {
                    string x = JsonConvert.SerializeObject(Twitter.GetfollowUser(user));
                    response.body = Encoding.UTF8.GetBytes(x);
                }
                if (request.Method == "POST")
                {
                    Twitter ntwitter = new Twitter(user);
                    ntwitter.AddFollowing(following);
                    response.body = Encoding.UTF8.GetBytes("Following Susceed");
                }
                /*if (request.Method == "DELETE")
                {
                    Twitter.DeleteUser(following);
                    response.body = Encoding.UTF8.GetBytes("Susceed");
                }*/
            }
            if (u[0] == "tweet")
            {
                Twitter ntweet = new Twitter(user);
                if (request.Method == "GET")
                {
                    string timeline = request.getRequestByKey("timeline");
                    if (timeline == "follow")
                    {
                        string x = JsonConvert.SerializeObject(ntweet.GetFollowingTimeline());
                        response.body = Encoding.UTF8.GetBytes(x);
                    }
                    else
                    {
                        string x = JsonConvert.SerializeObject(ntweet.GetUserTimeline());
                        response.body = Encoding.UTF8.GetBytes(x);
                    }
                }
                if (request.Method == "POST")
                {
                    ntweet.PostTweet(message);
                    response.body = Encoding.UTF8.GetBytes("Post Susceed");
                }
            }

            return response;
        }
    }

}
