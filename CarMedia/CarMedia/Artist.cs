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
        public string ArtistShortName { get; set; }
        public ICollection<Album> Album { get; set; }
        public ICollection<Track> Tracks { get; set; }

        public Artist(string artistName)
        {
            //this.ArtistId = artistId;
            this.ArtistName = artistName;
            this.ArtistShortName = this.ArtistName.Length > 23 ? this.ArtistName.Substring(0, 20) + "..." : this.ArtistName;
        }
    }
}
