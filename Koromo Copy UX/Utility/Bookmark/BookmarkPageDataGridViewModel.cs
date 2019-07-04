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

namespace Koromo_Copy_UX.Utility.Bookmark
{
    public class BookmarkPageDataGridItemViewModel : INotifyPropertyChanged
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

        private string _yuhuyng;
        public string 유형
        {
            get { return _yuhuyng; }
            set
            {
                if (_yuhuyng == value) return;
                _yuhuyng = value;
                OnPropertyChanged();
            }
        }

        private string _content;
        public string 내용
        {
            get { return _content; }
            set
            {
                if (_content == value) return;
                _content = value;
                OnPropertyChanged();
            }
        }

        private string _stamp;
        public string 추가된날짜
        {
            get { return _stamp; }
            set
            {
                if (_stamp == value) return;
                _stamp = value;
                OnPropertyChanged();
            }
        }

        private string _guitar;
        public string 기타
        {
            get { return _guitar; }
            set
            {
                if (_guitar == value) return;
                _guitar = value;
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

        private string _size;
        public string 파일크기
        {
            get { return _size; }
            set
            {
                if (_size == value) return;
                _size = value;
                OnPropertyChanged();
            }
        }

        BookmarkItemModel bim;
        public BookmarkItemModel BIM
        {
            get { return bim; }
            set
            {
                if (bim == value) return;
                bim = value;
                OnPropertyChanged();
            }
        }
    }

    public class BookmarkPageDataGridViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<BookmarkPageDataGridItemViewModel> _items;
        public ObservableCollection<BookmarkPageDataGridItemViewModel> Items => _items;

        public BookmarkPageDataGridViewModel(IEnumerable<BookmarkPageDataGridItemViewModel> collection = null)
        {
            if (collection == null)
                _items = new ObservableCollection<BookmarkPageDataGridItemViewModel>();
            else
                _items = new ObservableCollection<BookmarkPageDataGridItemViewModel>(collection);
        }
    }
}
