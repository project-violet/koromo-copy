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
    public class DownloadDataGridItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _id;
        public string 인덱스
        {
            get { return _id; }
            set
            {
                if (_id == value) return;
                _id = value;
                OnPropertyChanged();
            }
        }

        private string _title;
        public string 제목
        {
            get { return _title; }
            set
            {
                if (_title == value) return;
                _title = value;
                OnPropertyChanged();
            }
        }

        private string _path;
        public string 경로
        {
            get { return _path; }
            set
            {
                if (_path == value) return;
                _path = value;
                OnPropertyChanged();
            }
        }
    }

    public class DownloadDataGridViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<DownloadDataGridItemViewModel> _items;
        public ObservableCollection<DownloadDataGridItemViewModel> Items => _items;

        public DownloadDataGridViewModel()
        {
            _items = new ObservableCollection<DownloadDataGridItemViewModel>();
        }
    }
}
