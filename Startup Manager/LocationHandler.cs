using System;

namespace Startup_Manager
{
    class Location
    {
        private string m_location;
        public Location(string location)
        {
            if (location[0] == '"')
            {
                m_location = location.Split('"')[1];
            }
            else
            {
                m_location = location.Split(' ')[0];
            }
        }
        public string GetLocation()
        {
            return m_location;
        }
    }
}
