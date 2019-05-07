using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations.Schema;

namespace DNWS
{
    class TwitterApiPlugin : TwitterPlugin
    {

        private User AllUsers()//all user
        {
            using (var context = new TweetContext())
            {
                try
                {
                    List<User> users = context.Users.Where(b => true).Include(b => b.Following).ToList();
                    return users[0];
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public List<Following> FollowingAll(string name) //all following
        {
            using (var context = new TweetContext())
            {
                try
                {
                    List<User> followings = context.Users.Where(b => b.Name.Equals(name)).Include(b => b.Following).ToList();
                    return followings[0].Following;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }


        public virtual HTTPResponse GetResponse(HTTPRequest request)
        {
            HTTPResponse response = new HTTPResponse(200);
            string user = request.getRequestByKey("user");
            string password = request.getRequestByKey("password");
            string following = request.getRequestByKey("follow");
            string message = request.getRequestByKey("message");
            string[] path = request.Filename.Split("?");

            if (path[0] == "users")
            {
                if (request.Method == "GET")
                {
                    string json = JsonConvert.SerializeObject(AllUsers());
                    response.body = Encoding.UTF8.GetBytes(json);
                }
                else if (request.Method == "POST")
                {
                    try
                    {
                        Twitter.AddUser(user, password);
                        response.body = Encoding.UTF8.GetBytes("OK(200)");
                    }
                    catch (Exception)
                    {
                        response.status = 403;
                        response.body = Encoding.UTF8.GetBytes("User already exists(403)");
                    }
                }
                else if (request.Method == "DELETE")
                {
                    try
                    {
                        Twitter twitter = new Twitter(user);
                        twitter.DeleteUser(user);
                        response.body = Encoding.UTF8.GetBytes("OK(200)");
                    }
                    catch (Exception)
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("User not exists(404)");
                    }
                }
            }
            else if (path[0] == "following")
            {
                if (request.Method == "GET")
                {
                    string json = JsonConvert.SerializeObject(FollowingAll(user));
                    response.body = Encoding.UTF8.GetBytes(json);
                }
                else if (request.Method == "POST")
                {
                    if (Twitter.UserCheck(following))
                    {
                        Twitter twitter = new Twitter(user);
                        twitter.AddFollowing(following);
                        response.body = Encoding.UTF8.GetBytes("OK(200)");
                    }
                    else
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("User not exists(404)");
                    }
                }
                else if (request.Method == "DELETE")
                {
                    try
                    {
                        Twitter twitter = new Twitter(user);
                        twitter.RemoveFollowing(following);
                        response.body = Encoding.UTF8.GetBytes("OK(200)");
                    }
                    catch (Exception)
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("User not exists(404)");
                    }
                }
            }
            else if (path[0] == "tweets")
            {
                if (request.Method == "GET")
                {
                    try
                    {
                        string timeline = request.getRequestByKey("timeline");
                        if (timeline == "following")
                        {
                            Twitter twitter = new Twitter(user);
                            string json = JsonConvert.SerializeObject(twitter.GetFollowingTimeline());
                            response.body = Encoding.UTF8.GetBytes(json);
                        }
                        else
                        {
                            Twitter twitter = new Twitter(user);
                            string json = JsonConvert.SerializeObject(twitter.GetUserTimeline());
                            response.body = Encoding.UTF8.GetBytes(json);
                        }
                    }
                    catch (Exception)
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("User not found(404)");
                    }
                }
                else if (request.Method == "POST")
                {
                    try
                    {
                        Twitter twitter = new Twitter(user);
                        twitter.PostTweet(message);
                        response.body = Encoding.UTF8.GetBytes("OK(200)");
                    }
                    catch (Exception)
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("User not found(404)");
                    }
                }
            }
            response.type = "application/json";
            return response;
        }
    }
}

