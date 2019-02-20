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
    public class ArtistDataGridItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _index;
        public string 항목
        {
            get { return _index; }
            set
            {
                if (_index == value) return;
                _index = value;
                OnPropertyChanged();
            }
        }

        private int _count;
        public int 카운트
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

    public class ArtistDataGridViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private ObservableCollection<ArtistDataGridItemViewModel> _items;
        public ObservableCollection<ArtistDataGridItemViewModel> Items => _items;

        public ArtistDataGridViewModel(IEnumerable<ArtistDataGridItemViewModel> collection = null)
        {
            if (collection == null)
                _items = new ObservableCollection<ArtistDataGridItemViewModel>();
            else
                _items = new ObservableCollection<ArtistDataGridItemViewModel>(collection);
        }
    }
}
