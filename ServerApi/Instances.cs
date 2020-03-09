using System;
using System.Text;

namespace WebApi
{
    public struct Message
    {
        public string id;
        public string text;
        public override string ToString() => Convert.ToBase64String(Encoding.UTF8.GetBytes(id)) + ":SPLIT:" + Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
    }
}