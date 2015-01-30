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
    public class Track : Album
    {
        public int TrackId { get; set; }
        public string TrackName { get; set; }

        public Track(string artistName, string AlbumName, Image AlbumArt, string TrackName, int id)
            :base(artistName, AlbumName, AlbumArt)
        {
            this.TrackName = TrackName;
            this.TrackId = id;
        }
    }
}
