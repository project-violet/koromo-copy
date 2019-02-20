/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy_UX.Domain
{
    public class CustomArtistsRecommendationDataGridItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _rank;
        public string 순위
        {
            get { return _rank; }
            set
            {
                if (_rank == value) return;
                _rank = value;
                OnPropertyChanged();
            }
        }

        private string _artist;
        public string 작가
        {
            get { return _artist; }
            set
            {
                if (_artist == value) return;
                _artist = value;
                OnPropertyChanged();
            }
        }

        private string _articles;
        public string 작품수
        {
            get { return _articles; }
            set
            {
                if (_articles == value) return;
                _articles = value;
                OnPropertyChanged();
            }
        }

        private string _score;
        public string 점수
        {
            get { return _score; }
            set
            {
                if (_score == value) return;
                _score = value;
                OnPropertyChanged();
            }
        }

        private string _tags;
        public string 태그
        {
            get { return _tags; }
            set
            {
                if (_tags == value) return;
                _tags = value;
                OnPropertyChanged();
            }
        }
    }

    public class CustomArtistsRecommendationDataGridViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<CustomArtistsRecommendationDataGridItemViewModel> _items;
        public ObservableCollection<CustomArtistsRecommendationDataGridItemViewModel> Items => _items;

        public CustomArtistsRecommendationDataGridViewModel(IEnumerable<CustomArtistsRecommendationDataGridItemViewModel> collection = null)
        {
            if (collection == null)
                _items = new ObservableCollection<CustomArtistsRecommendationDataGridItemViewModel>();
            else
                _items = new ObservableCollection<CustomArtistsRecommendationDataGridItemViewModel>(collection);
        }
    }
}
