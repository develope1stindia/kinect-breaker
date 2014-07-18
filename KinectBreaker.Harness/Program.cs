using System;
using Microsoft.AspNet.SignalR.Client;

namespace KinectBreaker.Harness
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var connection = new HubConnection("http://localhost:53059/");
            var hub = connection.CreateHubProxy("GameHub");
            
            connection.Start().Wait();

            if (args[0] == "/kinect")
            {
                //kinect specific code goes here
            }
            else
            {
                bool die = false;

                while (!die)
                {
                    switch (Console.ReadKey().Key)
                    {
                        case ConsoleKey.Z:
                            hub.Invoke("Move", -1);
                            break;
                        case ConsoleKey.X:
                            hub.Invoke("Move", 1);
                            break;
                        case ConsoleKey.Escape:
                            die = true;
                            break;
                    }
                }
            }
        }
    }
}