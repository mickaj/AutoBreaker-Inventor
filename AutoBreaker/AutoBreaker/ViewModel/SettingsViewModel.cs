using System.ComponentModel;
using InvAddIn.Model;
using System.Windows.Input;

namespace InvAddIn.ViewModel
{
    public interface ISettingsViewModel
    {
        int Style { get; set; }
        string Gap { get; set; }
        int Symbols { get; set; }
        int Range { get; set; }
        ICommand SaveCmd { get; }

        void SaveToModel();
    }

    public class SettingsViewModel : ISettingsViewModel, INotifyPropertyChanged
    {
        private ISettingsModel model;

        private int style; //0 - rectangular, 1 - structural
        public int Style
        {
            get { return style; }
            set
            {
                style = value;
                OnPropertyChanged("Style");
            }
        }

        private string gap;
        public string Gap
        {
            get { return gap; }
            set
            {
                gap = value;
                OnPropertyChanged("Gap");
            }
        }

        private int symbols;
        public int Symbols
        {
            get { return symbols; }
            set
            {
                symbols = value;
                OnPropertyChanged("Symbols");
            }
        }

        private double range;
        public int Range
        {
            get { return (int)range; }
            set
            {
                range = (double)value;
                OnPropertyChanged("Range");
            }
        }

        private ICommand saveCmd;
        public ICommand SaveCmd
        {
            get
            {
                if(saveCmd == null) { saveCmd = new SaveCommand(this); }
                return saveCmd;
            }
        }


        /// <summary>
        /// Default constructor that acquires model instance and retrieves values from it
        /// </summary>
        /// <param name="_model">Settings model</param>
        public SettingsViewModel(ISettingsModel _model)
        {
            model = _model;
            style = _model.Style;
            gap = _model.Gap.ToString();
            symbols = _model.Symbols;
            range = _model.Range;
        }

        /// <summary>
        /// Saves current settings to model
        /// </summary>
        public void SaveToModel()
        {
            model.Gap = double.Parse(gap);
            model.Range = range;
            model.Style = style;
            model.Symbols = symbols;
        }
        
        #region INotifyProperyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
                
        private void OnPropertyChanged(params string[] propertiesChanged)
        {
            if (PropertyChanged != null)
            {
                foreach (string property in propertiesChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));
                }
            }
        }
        #endregion
    }
}
