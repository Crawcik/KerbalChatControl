using System.Net;
using System.IO;
using System;
using System.Text;
using System.Collections.Generic;

namespace WebApi
{
    class Program
    {
        static Youtube youtube;
        static string[] config = File.ReadAllLines( Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/kspwebapi/config.js");
        static void Main(string[] args)
        {
            Initialize(args[0]);
            HttpListener httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://192.168.1.82:8080/ksp/");
            httpListener.Start();
            while (true)
            {
                var context = httpListener.GetContext();
                Console.WriteLine("GET CALL");
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
                        }
                        catch { responce.StatusCode = 400; break; }
                        if (!(keys.ContainsKey("channelId") && keys.ContainsKey("token")))
                        {
                            responce.StatusCode = 400;
                            break;
                        }
                        Stream x = responce.OutputStream;
                        byte[] test = Encoding.UTF8.GetBytes("TEST222");
                        x.Write(test, 0, test.Length);
                        x.Close();

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
        static void Initialize(string authorization_code)
        {
            youtube = new Youtube(config[0], config[1]);
            if (youtube.TryGetToken(authorization_code))
            {
                youtube.KeepKeyAlive().GetAwaiter();
            }

        }
    }
}
