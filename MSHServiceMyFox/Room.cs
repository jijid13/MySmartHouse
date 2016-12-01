using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSHService
{
    class Room
    {
        private int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        private string title;

        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        private double temperature;

        public double Temperature
        {
            get { return temperature; }
            set { temperature = value; }
        }

        public Room(int pid, string ptitle, double ptemperature = 0)
        {
            this.id = pid;
            this.title = ptitle;
            this.temperature = ptemperature;
        }
    }
}
