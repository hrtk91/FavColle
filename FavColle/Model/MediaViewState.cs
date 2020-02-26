using System;
using System.Collections.Generic;
using System.Linq;

namespace FavColle.Model
{
    class MediaViewState
    {
        protected int TweetId { get; set; }
        protected Uri Selected { get; set; }
        protected IList<Uri> MediaUris { get; set; }

        public MediaViewState(Uri selected, IEnumerable<Uri> mediaUris)
        {
            Selected = selected;
            MediaUris = mediaUris.ToList();
        }

        public bool HasPrevious()
            => !(MediaUris.First() == Selected);
        public bool HasNext()
            => !(MediaUris.Last() == Selected);

        public Uri Previous()
        {
            var idx = MediaUris.IndexOf(Selected);
            if (idx < 1) throw new InvalidOperationException();

            var prevIdx = idx;

            Selected = MediaUris.Take(prevIdx).Last();
            return Selected;
        }

        public Uri Next()
        {
            var idx = MediaUris.IndexOf(Selected);
            if (idx == -1) throw new InvalidOperationException();

            var nextIdx = idx + 2;

            Selected = MediaUris.Take(nextIdx).Last();
            return Selected;
        }
    }
}
