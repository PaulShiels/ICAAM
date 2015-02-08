using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarMedia
{
    [Serializable]
    class Playlist
    {
        public string PlaylistName { get; set; }
        public List<Track> PlaylistTracks { get; set; }

        public Playlist(string name, List<Track> tracks)
        {
            this.PlaylistName = name;
            this.PlaylistTracks = tracks;
        }
    }
}
