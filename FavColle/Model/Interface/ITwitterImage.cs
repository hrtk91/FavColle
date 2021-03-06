﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FavColle.Model.Interface
{
    public interface ITwitterImage
    {
        string Url { get; set; }
        byte[] Data { get; set; }
        Task<ITwitterImage> Download(SizeOpt size);
        Task SaveAsAsync(Uri source, string filepath);
        ImageSource Convert();
        Uri ToUri(SizeOpt size = SizeOpt.Small);
    }
}
