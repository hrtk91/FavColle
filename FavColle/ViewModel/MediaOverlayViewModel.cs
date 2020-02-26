using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FavColle.Model;

namespace FavColle.ViewModel
{
    class MediaOverlayViewModel : ViewModelBase
    {
        private long TweetId;
        private Uri _selected;
        public Uri Selected
        {
            get => _selected;
            set { _selected = value; RaisePropertyChanged(); }
        }

        public MediaViewState State { get; set; }

        public DelegateCommand PreviousCommand { get; set; }
        public DelegateCommand NextCommand { get; set; }
        public DelegateCommand MediaPushedCommand { get; set; }

        public MediaOverlayViewModel(Uri selected, IEnumerable<Uri> medias, long tweetId)
        {
            TweetId = tweetId;
            Selected = selected;

            State = new MediaViewState(selected, medias);

            PreviousCommand = new DelegateCommand(Previous);
            NextCommand = new DelegateCommand(Next);
            MediaPushedCommand = new DelegateCommand(MediaPushed);
        }

        public void Previous(object obj)
        {
            if (State.HasPrevious())
            {
                Selected = State.Previous();
            }
        }

        public void Next(object obj)
        {
            if (State.HasNext())
            {
                Selected = State.Next();
            }
        }

        public async void MediaPushed(object obj)
        {
            if (MessageBox.Show("画像を保存しますか？", "画像保存確認", MessageBoxButton.YesNo) == MessageBoxResult.No) return;
            MessageBox.Show("未対応");
            return;

            var directory = Path.Combine("./Favorites/", TweetId.ToString());
            if (Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }
            var filename = Path.Combine(Selected.Segments.Last(), Selected.Query.Substring(1).Split('&').Last());
            var filepath = Path.Combine(directory, filename);

            if (File.Exists(filepath) == true) return;

            await (new TweetImage(Selected.AbsoluteUri)).SaveAsAsync(directory, filename);
        }
    }
}
