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
    public class FinderDataGridItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private string _id;
        public string 아이디
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

        private string _type;
        public string 타입
        {
            get { return _type; }
            set
            {
                if (_type == value) return;
                _type = value;
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

        private string _group;
        public string 그룹
        {
            get { return _group; }
            set
            {
                if (_group == value) return;
                _group = value;
                OnPropertyChanged();
            }
        }

        private string _series;
        public string 시리즈
        {
            get { return _series; }
            set
            {
                if (_series == value) return;
                _series = value;
                OnPropertyChanged();
            }
        }

        private string _character;
        public string 캐릭터
        {
            get { return _character; }
            set
            {
                if (_character == value) return;
                _character = value;
                OnPropertyChanged();
            }
        }

        private string _upload_time;
        public string 업로드_시간
        {
            get { return _upload_time; }
            set
            {
                if (_upload_time == value) return;
                _upload_time = value;
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

        private string _down;
        public string 다운
        {
            get { return _down; }
            set
            {
                if (_down == value) return;
                _down = value;
                OnPropertyChanged();
            }
        }
    }

    public class FinderDataGridViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<FinderDataGridItemViewModel> _items;
        public ObservableCollection<FinderDataGridItemViewModel> Items => _items;

        public FinderDataGridViewModel()
        {
            _items = new ObservableCollection<FinderDataGridItemViewModel>();
        }
    }
}
