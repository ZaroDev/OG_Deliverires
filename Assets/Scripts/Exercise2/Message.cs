using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Exercise2
{
    [System.Serializable]
    public class Message
    {
        public string userName;
        public string message;
        public string date;

        public Message(string name, string msg, string dateTime)
        {
            userName = name;
            message = msg;
            date = dateTime;
        }
    }
}