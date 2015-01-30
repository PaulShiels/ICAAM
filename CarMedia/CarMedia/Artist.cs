using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarMedia
{
    public class Artist
    {
        //public int ArtistId { get; set; }
        public string ArtistName { get; set; }

        public Artist(string artistName)
        {
            //this.ArtistId = artistId;
            this.ArtistName = artistName;
        }
    }
}
