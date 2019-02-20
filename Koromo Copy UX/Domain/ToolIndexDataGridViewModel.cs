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
    public class ToolIndexDataGridItemViewModel : INotifyPropertyChanged
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

        private string _eng;
        public string 영어
        {
            get { return _eng; }
            set
            {
                if (_eng == value) return;
                _eng = value;
                OnPropertyChanged();
            }
        }

        private string _korean;
        public string 한국어
        {
            get { return _korean; }
            set
            {
                if (_korean == value) return;
                _korean = value;
                OnPropertyChanged();
            }
        }

        private string _count;
        public string 카운트
        {
            get { return _count; }
            set
            {
                if (_count == value) return;
                _count = value;
                OnPropertyChanged();
            }
        }
    }

    public class ToolIndexDataGridViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<ToolIndexDataGridItemViewModel> _items;
        public ObservableCollection<ToolIndexDataGridItemViewModel> Items => _items;

        public ToolIndexDataGridViewModel()
        {
            _items = new ObservableCollection<ToolIndexDataGridItemViewModel>();
        }
    }
}
