using System;
using System.Collections.Generic;
using System.Linq;
using FavColle.Model.Interface;

namespace FavColle.Model
{
    class MediaViewState
    {
        public ITwitterImage Selected { get; protected set; }
        protected IList<ITwitterImage> Medias { get; set; }

        public MediaViewState(ITwitterImage selected, IEnumerable<ITwitterImage> mediaUris)
        {
            Medias = mediaUris.ToList();
            Selected = Medias.First(m => m.Url == selected.Url);
        }

        public bool HasPrevious()
            => !(Medias.First() == Selected);
        public bool HasNext()
            => !(Medias.Last() == Selected);

        public ITwitterImage Previous()
        {
            var idx = Medias.IndexOf(Selected);
            if (idx < 1) throw new InvalidOperationException();

            var prevIdx = idx;

            Selected = Medias.Take(prevIdx).Last();
            return Selected;
        }

        public ITwitterImage Next()
        {
            var idx = Medias.IndexOf(Selected);
            if (idx == -1) throw new InvalidOperationException();

            var nextIdx = idx + 2;

            Selected = Medias.Take(nextIdx).Last();
            return Selected;
        }
    }
}
