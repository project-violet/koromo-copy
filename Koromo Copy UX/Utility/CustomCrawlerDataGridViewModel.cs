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

namespace Koromo_Copy_UX.Utility
{
    public class CustomCrawlerDataGridItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int i;
        public int j;

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

        private string _depth;
        public string 깊이
        {
            get { return _depth; }
            set
            {
                if (_depth == value) return;
                _depth = value;
                OnPropertyChanged();
            }
        }

        private string _name;
        public string 네임
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        private string _specific;
        public string 특정항목
        {
            get { return _specific; }
            set
            {
                if (_specific == value) return;
                _specific = value;
                OnPropertyChanged();
            }
        }
    }

    public class CustomCrawlerDataGridViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<CustomCrawlerDataGridItemViewModel> _items;
        public ObservableCollection<CustomCrawlerDataGridItemViewModel> Items => _items;

        public CustomCrawlerDataGridViewModel(IEnumerable<CustomCrawlerDataGridItemViewModel> collection = null)
        {
            if (collection == null)
                _items = new ObservableCollection<CustomCrawlerDataGridItemViewModel>();
            else
                _items = new ObservableCollection<CustomCrawlerDataGridItemViewModel>(collection);
        }
    }
}
