﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;

namespace CarMedia
{
    public class Album
    {
        public int AlbumId { get; set; }
        public string AlbumName { get; set; }
        public string AlbumShortName { get; set; }
        public Image AlbumArt { get; set;}
        public Artist Artist { get; set; }
        public ICollection<Track> Tracks { get; set; }

        public Album(string artistName, string AlbumName, Image AlbumArt)
        {
            this.AlbumName = AlbumName;
            this.AlbumShortName = this.AlbumName.Length > 22 ? this.AlbumName.Substring(0, 19) + "..." : this.AlbumName;
            this.AlbumArt = AlbumArt;
            Artist = new Artist(artistName);
        }        
    }
}
