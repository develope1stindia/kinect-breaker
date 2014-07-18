using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Kinect;

namespace KinectBreaker.Harness
{
    internal class Program
    {
        private static double _threshhold = 0.05;
        private static KinectSensor _kinect;
        private static Subject<float> _subject = new Subject<float>();

        private static void Main(string[] args)
        {
            var connection = new HubConnection("http://localhost:53059/");
            IHubProxy hub = connection.CreateHubProxy("GameHub");

            connection.Start().Wait();
            _subject
                .Buffer(TimeSpan.FromMilliseconds(50))
                .Subscribe(
                    positionList =>
                    {
                        if (positionList.Any())
                        {
                            float average = positionList.Average();
                            if (average > _threshhold)
                            {
                                Console.WriteLine("Right: {0}", average);
                                hub.Invoke("Move", 1);
                                return;
                            }

                            if (average < - (_threshhold))
                            {
                                Console.WriteLine("Left: {0}", average);
                                hub.Invoke("Move", -1);
                                return;
                            }


                            Console.WriteLine("Middle: {0}", average);
                        }
                    }
                );

            bool die = false;

            if (args.Any(a => a.Equals("/kinect")))
            {
                //kinect specific code goes here
                StartKinect();
            }

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
                        Stop();
                        break;
                }
            }
        }

        private static void StartKinect()
        {
            _kinect = KinectSensor.KinectSensors.FirstOrDefault();
            if (_kinect != null)
            {
                _kinect.SkeletonStream.Enable(); 
                
                // Depth in near range enabled
                _kinect.DepthStream.Range = DepthRange.Near;

                // enable returning skeletons while depth is in Near Range
                _kinect.SkeletonStream.EnableTrackingInNearRange = true;

                // Sit the fuck down
                _kinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;

                _kinect.SkeletonFrameReady += SkeletonFramesReady;

                _kinect.Start();
            }
        }

        private static void SkeletonFramesReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
            {
                if (skeletonFrameData != null)
                {
                    var allSkeletons = new Skeleton[skeletonFrameData.SkeletonArrayLength];

                    skeletonFrameData.CopySkeletonDataTo(allSkeletons);

                    float hand = allSkeletons.Where(s => s.TrackingState == SkeletonTrackingState.Tracked)
                        .SelectMany(s => s.Joints.Where(j => j.JointType == JointType.HandRight))
                        .Select(h => h.Position.X).FirstOrDefault();

                    _subject.OnNext(hand);
                }
            }
        }

        private static void Stop()
        {
            _kinect.Stop();
        }
    }
}