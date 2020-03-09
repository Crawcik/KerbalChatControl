using System.Net;
using System.IO;
using System;
using System.Collections.Generic;

namespace WebApi
{
    class Program
    {
        static Youtube youtube;
        static Dictionary<string,string> config = GetConfig(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/kspwebapi/config.txt");
        static void Main(string[] args)
        {
            Initialize();
            HttpListener httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://localhost:8080/");
            httpListener.Start();
            while (true)
            {
                var context = httpListener.GetContext();
                var request = context.Request;
                var responce = context.Response;
                if (request.HttpMethod == "GET")
                {
                    if (request.ContentType == "x-www-form-urlencoded")
                    {
                        Dictionary<string, string> keys = new Dictionary<string, string>();
                        try
                        {
                            string parameters = request.RawUrl.Split(new string[] { "/ksp/?" }, StringSplitOptions.None)[1];
                            foreach(string parameter in parameters.Split('&')){
                                string[] arg = parameter.Split('=');
                                keys.Add(arg[0], arg[1]);
                            }
                            if (keys.ContainsKey("channel") && keys.ContainsKey("token") && keys.ContainsKey("type"))
                            {
                                List<Message> msgs= new List<Message>();
                                List<char> platforms_key = new List<char>(keys["type"].ToCharArray());
                                string[] channelIdAll = keys["channel"].Split(new string[] { ":BREAK:" }, StringSplitOptions.RemoveEmptyEntries);
                                if (platforms_key.Contains('y'))
                                {
                                    int index = platforms_key.IndexOf('y');
                                    string liveChatId;
                                    youtube.TryGetLiveChatId(out liveChatId, channelIdAll[index]);
                                    Message[] messages_yt;
                                    youtube.TryGetChatMessages(liveChatId, out messages_yt);
                                    msgs.AddRange(messages_yt);
                                }
                                string data = string.Join(":BREAK:", msgs);
                                StreamWriter writer = new StreamWriter(responce.OutputStream);
                                writer.Write(data);
                                writer.Close();
                            } else responce.StatusCode = 400;
                        }
                        catch { responce.StatusCode = 400;}

                    }
                    else
                    {
                        responce.StatusCode = 400;
                    }
                }
                else { responce.StatusCode = 405; }
                responce.Close();
            }

        }
        static void Initialize()
        {
            if (config["youtube_On"] == "true") {
                Console.WriteLine("Initializing Youtube");
                string client_id = config["youtube_ClientId"];
                youtube = new Youtube(client_id, config["youtube_ClientSecret"]);
                Console.WriteLine($"Get access code from... \nhttps://accounts.google.com/o/oauth2/auth?client_id={client_id}&redirect_uri=urn:ietf:wg:oauth:2.0:oob&scope=https://www.googleapis.com/auth/youtube.readonly&response_type=code&access_type=offline \n and paste here: ");
                string code = Console.ReadLine();
                if (youtube.TryGetToken(code))
                {
                    youtube.IsConnected = true;
                    youtube.KeepKeyAlive().GetAwaiter();
                    Console.WriteLine("Youtube initialized!");
                }
                else
                {
                    Console.WriteLine("Failed Youtube");
                }
            }

        }

        static Dictionary<string, string> GetConfig(string path)
        {
            Dictionary<string, string> temp = new Dictionary<string, string>();
            string[] lines = System.IO.File.ReadAllLines(path);
            foreach (string line in lines)
            {
                if (!string.IsNullOrEmpty(line) && line.ToCharArray()[0] != '#')
                {
                    string[] parameter = line.Split('=');
                    temp.Add(parameter[0], parameter[1]);
                }
            }
            return temp;
        }
    }
}
