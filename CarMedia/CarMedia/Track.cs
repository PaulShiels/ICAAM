using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarMedia
{
    public class Track : Album
    {
        public int TrackId { get; set; }

        public string TrackName { get; set; }

        public Track(string file, int id)
        {
            this.TrackId = id;
            TagLib.File tagFile = TagLib.File.Create(file);
            this.TrackName = tagFile.Tag.Title;
            this.ArtistName = tagFile.Tag.JoinedPerformers;
            this.AlbumName = tagFile.Tag.Album;
        }
    }
}
