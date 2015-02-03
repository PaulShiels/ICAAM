using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;

namespace CarMedia
{
    public class Track
    {
        public int TrackId { get; set; }
        public string TrackName { get; set; }
        public Album Album { get; set; }
        public Artist Artist { get; set; }
        public string JoinedArtists { get; set; }

        public Track(Artist artist, string joinedArtists, Album album, string TrackName, int id)
        {
            this.TrackName = TrackName;
            this.JoinedArtists = joinedArtists;
            this.TrackId = id;
            Album = album;
            Artist = artist;
        }
    }
}
