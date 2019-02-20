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
    public class CustomArtistsRecommendationBookmarkDataGridItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _index;
        public string 인덱스
        {
            get { return _index; }
            set
            {
                if (_index == value) return;
                _index = value;
                OnPropertyChanged();
            }
        }

        private string _name;
        public string 이름
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        private string _date;
        public string 날짜
        {
            get { return _date; }
            set
            {
                if (_date == value) return;
                _date = value;
                OnPropertyChanged();
            }
        }

        private string _tag;
        public string 태그
        {
            get { return _tag; }
            set
            {
                if (_tag == value) return;
                _tag = value;
                OnPropertyChanged();
            }
        }
    }

    public class CustomArtistsRecommendationBookmarkDataGridViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<CustomArtistsRecommendationBookmarkDataGridItemViewModel> _items;
        public ObservableCollection<CustomArtistsRecommendationBookmarkDataGridItemViewModel> Items => _items;

        public CustomArtistsRecommendationBookmarkDataGridViewModel()
        {
            _items = new ObservableCollection<CustomArtistsRecommendationBookmarkDataGridItemViewModel>();
        }
    }
}
