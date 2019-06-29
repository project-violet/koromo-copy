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

namespace DCGallery.Domain
{
    public class GalleryDataGridItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _num;
        public string 번호
        {
            get { return _num; }
            set
            {
                if (_num == value) return;
                _num = value;
                OnPropertyChanged();
            }
        }

        private string _class;
        public string 클래스
        {
            get { return _class; }
            set
            {
                if (_class == value) return;
                _class = value;
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

        private string _reply;
        public string 답글
        {
            get { return _reply; }
            set
            {
                if (_reply == value) return;
                _reply = value;
                OnPropertyChanged();
            }
        }

        private string _nick;
        public string 닉네임
        {
            get { return _nick; }
            set
            {
                if (_nick == value) return;
                _nick = value;
                OnPropertyChanged();
            }
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

        private string _rem;
        public string 추천수
        {
            get { return _rem; }
            set
            {
                if (_rem == value) return;
                _rem = value;
                OnPropertyChanged();
            }
        }

        private string _click;
        public string 조회수
        {
            get { return _click; }
            set
            {
                if (_click == value) return;
                _click = value;
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
    }

    public class GalleryDataGridViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<GalleryDataGridItemViewModel> _items;
        public ObservableCollection<GalleryDataGridItemViewModel> Items => _items;

        public GalleryDataGridViewModel(IEnumerable<GalleryDataGridItemViewModel> collection = null)
        {
            if (collection == null)
                _items = new ObservableCollection<GalleryDataGridItemViewModel>();
            else
                _items = new ObservableCollection<GalleryDataGridItemViewModel>(collection);
        }
    }
}
