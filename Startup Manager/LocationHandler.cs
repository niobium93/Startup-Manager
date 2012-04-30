using System;
using System.IO;

namespace Startup_Manager
{
    class LocationHandler
    {
        public string Location { get; private set; }
        public LocationHandler(string location)
        {
            if (File.Exists(location))
            {
                Location = Path.GetFullPath(location);
            }
            else if(File.Exists(location.Split('.')[0] + '.' + location.Split('.')[1].Split(' ')[0]))
            {
                Location = Path.GetFullPath(location.Split('.')[0] + '.' + location.Split('.')[1].Split(' ')[0]);
            }
            else if (location[0] == '"')
            {
                Location = location.Split('"')[1];
            }
            else
            {
                Location = location.Split(' ')[0];
            }

            if (Path.GetDirectoryName(Location) == "")
            {
                string tmp;
                string path = Environment.GetEnvironmentVariable("Path");
                foreach (string loc in path.Split(';'))
                {
                    tmp = Path.Combine(loc, Location);
                    if (File.Exists(tmp))
                    {
                        Location = tmp;
                        break;
                    }
                }
            }
        }
    }
} 
