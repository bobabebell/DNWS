using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;//dotnet add package Newtonsoft.Json --version 12.0.2
//ref https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-2.0&tabs=visual-studio
namespace DNWS
{
    public class TwitterAPI : TwitterPlugin
    {
        //get user list
        private List<User> GetListUser(){
            // ref TwitterPlugin.cs
            using (var context = new TweetContext()){
                try{
                    List<User> users = context.Users.Where(b => true).Include(b => b.Following).ToList();// using true for all user in db
                    return users;
                }catch(Exception ex){
                    Console.WriteLine(ex.ToString());
                    return null;
                }
            }
        }
        //get following list
        private List<User> GetListFollowing(string user){
            using (var context = new TweetContext()){
                try{
                    List<User> users = context.Users.Where(u => u.Name.Equals(user)).Include(u => u.Following).ToList();
                    return users;
                }catch(Exception ex){
                    Console.WriteLine(ex.ToString());
                    return null;
                }
            }
        }
        public HTTPResponse PostProcessing(HTTPResponse response)
        {
            throw new NotImplementedException();
        }
        public void PreProcessing(HTTPRequest request)
        {
            throw new NotImplementedException();
        }
        public override HTTPResponse GetResponse(HTTPRequest request)
        {
            HTTPResponse response = null;
            StringBuilder sb = new StringBuilder();
            string[] path = request.Filename.Split("?");
            if(path[0].ToLower() == "user"){
                if(request.Status == 200){
                    if(request.Method.ToLower() == "get"){//GET
                        try{
                            response = new HTTPResponse(200);
                            response.type = "json";
                            response.body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(GetListUser())); //get list user
                        }catch(Exception){
                            response = new HTTPResponse(400);
                        }
                    }else if(request.Method.ToLower() == "post"){//POST
                        try{
                            Twitter.AddUser(request.getRequestByKey("user").ToLower(),request.getRequestByKey("password").ToLower());//add user
                            sb.Append("<html><head><title>TwitterAPI</title></head><body>");
                            sb.Append("Added User : " + request.getRequestByKey("user").ToLower() + " Complete!");
                            sb.Append("</body></html");
                            response = new HTTPResponse(200);
                            response.body = Encoding.UTF8.GetBytes(sb.ToString());
                        }catch(Exception){
                            response = new HTTPResponse(400);
                        }

                    }else if(request.Method.ToLower() == "delete"){//DELETE
                        try{
                            Twitter t = new Twitter(request.getRequestByKey("user"));
                            t.DeleteUser(request.getRequestByKey("user"));
                            sb.Append("<html><head><title>TwitterAPI</title></head><body>");
                            sb.Append("Delete User : " + request.getRequestByKey("user").ToLower() + " Complete!");
                            sb.Append("</body></html");
                            response = new HTTPResponse(200);
                            response.body = Encoding.UTF8.GetBytes(sb.ToString());
                        }catch(Exception){
                            response = new HTTPResponse(400);
                        }
                    }else{//OTHER ELSE
                        response = new HTTPResponse(400);
                    }
                }else{
                    response = new HTTPResponse(400);
                }
            }else if(path[0].ToLower() == "following"){
                if(request.Status == 200){
                    if(request.Method.ToLower() == "get"){
                        try{
                            response = new HTTPResponse(200);
                            response.type = "json";
                            response.body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(GetListFollowing(request.getRequestByKey("user")))); //get following user                            
                        }catch(Exception){
                            response = new HTTPResponse(400);
                        }
                    }else if(request.Method.ToLower() == "post"){
                        try
                        {
                            Twitter t = new Twitter(request.getRequestByKey("user"));
                            t.AddFollowing(request.getRequestByKey("follow"));
                            sb.Append("<html><head><title>TwitterAPI</title></head><body>");
                            sb.Append("User : " + request.getRequestByKey("user").ToLower() + " Start Following : " + request.getRequestByKey("follow") + " Complete!");
                            sb.Append("</body></html");
                            response = new HTTPResponse(200);
                            response.body = Encoding.UTF8.GetBytes(sb.ToString());
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            response = new HTTPResponse(400);
                        }
                    }else if(request.Method.ToLower() == "delete"){
                        try
                        {
                            Twitter t = new Twitter(request.getRequestByKey("user"));
                            t.RemoveFollowing(request.getRequestByKey("follow"));
                            sb.Append("<html><head><title>TwitterAPI</title></head><body>");
                            sb.Append("User : " + request.getRequestByKey("user").ToLower() + " Unfollowing : " + request.getRequestByKey("follow") + " Complete!");
                            sb.Append("</body></html");
                            response = new HTTPResponse(200);
                            response.body = Encoding.UTF8.GetBytes(sb.ToString());

                        }
                        catch (Exception)
                        {
                            response = new HTTPResponse(400);
                        }
                    }else{
                        response = new HTTPResponse(400);
                    }   
                }else{
                    response = new HTTPResponse(400);
                }
            }else if(path[0].ToLower() == "tweet"){
                if(request.Status == 200)
                {
                    if(request.Method.ToLower() == "get")
                    {
                        if (request.getRequestByKey("getlist").ToLower() == "user")
                        {
                            try
                            {
                                Twitter t = new Twitter(request.getRequestByKey("user"));
                                response = new HTTPResponse(200);
                                response.type = "json";
                                response.body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(t.GetUserTimeline()));
                            }
                            catch (Exception)
                            {
                                response = new HTTPResponse(400);
                            }
                        }
                        else if (request.getRequestByKey("getlist").ToLower() == "following")
                        {
                            try
                            {
                                Twitter t = new Twitter(request.getRequestByKey("user"));
                                response = new HTTPResponse(200);
                                response.type = "json";
                                response.body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(t.GetFollowingTimeline()));
                            }
                            catch (Exception ex)
                            {
                                Console.Write(ex.ToString());
                                response = new HTTPResponse(400);
                            }
                        }
                        else
                        {
                            response = new HTTPResponse(400);
                        }
                    }
                    else if(request.Method.ToLower() == "post")
                    {
                        try
                        {
                            Twitter t = new Twitter(request.getRequestByKey("user"));
                            t.PostTweet(request.getRequestByKey("tweet"));
                            sb.Append("<html><head><title>TwitterAPI</title></head><body>");
                            sb.Append("User : " + request.getRequestByKey("user").ToLower() + " Tweet : " + request.getRequestByKey("tweet"));
                            sb.Append("</body></html");
                            response = new HTTPResponse(200);
                            response.body = Encoding.UTF8.GetBytes(sb.ToString());
                        }
                        catch (Exception)
                        {
                            response = new HTTPResponse(400);
                        }
                    }
                    else
                    {
                        response = new HTTPResponse(400);
                    }
                }
                else
                {
                    response = new HTTPResponse(400);
                }
            }
            else{
                sb.Append("<html><head><title>TwitterAPI</title></head><body>");
                sb.Append("<h1>TwitterAPI</h1>");
                sb.Append("</body></html");
                response = new HTTPResponse(200);
                response.body = Encoding.UTF8.GetBytes(sb.ToString());
            }
            return response;
        }
    }
}
            