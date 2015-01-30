﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;

namespace CarMedia
{
    public class Album : Artist
    {
        public int AlbumId { get; set; }
        public string AlbumName { get; set; }
        public Image AlbumArt { get; set;}

        public Album(string artistName, string AlbumName, Image AlbumArt)
            :base(artistName)
        {
            this.AlbumName = AlbumName;
            this.AlbumArt = AlbumArt;
        }
        
    }
}
