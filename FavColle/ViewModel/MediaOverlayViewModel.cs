using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using FavColle.Model;
using FavColle.Model.Interface;
using Microsoft.Win32;

namespace FavColle.ViewModel
{
    class MediaOverlayViewModel : ViewModelBase
    {
        private ITweet Tweet;
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
        public DelegateCommand CloseCommand { get; set; }

        public MediaOverlayViewModel(ITwitterImage selected, IEnumerable<ITwitterImage> medias, ITweet tweet)
        {
            Tweet = tweet;
            State = new MediaViewState(selected, medias);

            Selected = State.Selected.ToUri(SizeOpt.Orig);

            PreviousCommand = new DelegateCommand(Previous);
            NextCommand = new DelegateCommand(Next);
            MediaPushedCommand = new DelegateCommand(MediaPushed);
            CloseCommand = new DelegateCommand(Close);
        }

        public void Previous(object obj)
        {
            if (State.HasPrevious())
            {
                Selected = State.Previous().ToUri(SizeOpt.Orig);
            }
        }

        public void Next(object obj)
        {
            if (State.HasNext())
            {
                Selected = State.Next().ToUri(SizeOpt.Orig);
            }
        }

        public async void MediaPushed(object obj)
        {
            var filename = Path.GetFileName(State.Selected.Url);

            var dialog = new SaveFileDialog();
            dialog.InitialDirectory = Path.GetFullPath(".\\");
            dialog.FileName = $"{Tweet.User.Name}_{filename}";
            dialog.Filter = "*.jpg|JPG,*.jpeg|JPEG,*.png|PNG";

            if (dialog.ShowDialog() == false) return;
            
            await State.Selected.SaveAsAsync(Selected, dialog.FileName);
        }

        public void Close(object obj)
        {
            if (!(obj is Canvas viewer)) return;
            if (!(viewer.Parent is Panel panel)) return;
            
            panel.Children.Remove(viewer);
        }
    }
}
