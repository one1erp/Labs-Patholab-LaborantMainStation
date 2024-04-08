using System.ComponentModel;
using System.Windows.Controls;

namespace LaborantMainStation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public class GridRow : INotifyPropertyChanged
    {
        private string _stationName;
        private string _stationCode;
        private string _laborantId;
        private string _laborantWorkerNumber;
        private int _rowNember;
        private string _laborantName; 
        private string _sessionStart;
        private decimal _laborantWorkId;
        private bool _saveNeeded;
        
        public void Clean()
        {
            LaborantId = "";
            LaborantWorkerNumber = "";
            LaborantName = "";
            SessionStart = "";
            LaborantWorkId = 0;
            SaveNeeded = false;
        }
        public string StationName
        {
            get { return _stationName; }
            set { _stationName = value; OnPropertyChanged("StationName"); }
        }

        public string StationCode
        {
            get { return _stationCode; }
            set { _stationCode = value; OnPropertyChanged("StationCode"); }
        }

        public string LaborantId
        {
            get { return _laborantId; }
            set { _laborantId = value; OnPropertyChanged("LaborantId"); }
        }
        public string LaborantWorkerNumber
        {
            get { return _laborantWorkerNumber; }
            set { _laborantWorkerNumber = value;
                SaveNeeded = true;
                OnPropertyChanged("LaborantWorkerNumber"); 
            }
        }
       
        public int RowNember
        {
            get { return _rowNember; }
            set { _rowNember = value; OnPropertyChanged("RowNember"); }
        }

        public string LaborantName
        {
            get { return _laborantName; }
            set { _laborantName = value; OnPropertyChanged("LaborantName"); }
        }
  
        public string SessionStart
        {
            get { return _sessionStart; }
            set { _sessionStart = value; OnPropertyChanged("SessionStart"); }
        }
          public decimal LaborantWorkId
        {
            get { return _laborantWorkId; }
            set { _laborantWorkId = value; OnPropertyChanged("LaborantWorkId");}
        }

        public bool SaveNeeded
        {
            get { return _saveNeeded; }
            set { _saveNeeded = value; OnPropertyChanged("SaveNeeded"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }



    }
}