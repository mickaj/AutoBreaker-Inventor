namespace InvAddIn.Model
{
    public interface ISettingsModel
    {
        int Style { get; set; }
        double Gap { get; set; }
        int Symbols { get; set; }
        double Range { get; set; }
    }

    public class SettingsModel : ISettingsModel
    {
        private int style;
        public int Style
        {
            get
            {
                return style;
            }
            //0 - rectangular, 1 - structural
            set
            {
                if ((value == 0) || (value == 1)) { style = value; }
                else { style = 0; }
            }
        }

        public double Gap { get; set; }

        private int symbols;
        public int Symbols
        {
            get { return symbols; }
            //allowed value between 1 and 3 inclusively
            set
            {
                if ((value < 1) && (value > 3)) { symbols = 1; }
                else { symbols = value; }
            }
        }

        private double range;
        public double Range
        {
            get { return range; }
            //allowed value between 10 and 90 inclusively
            set
            {
                if (value<10) { range = 10; }
                else if(value>90) { range = 90; }
                else { range = value; }
            }
        }

        /// <summary>
        /// Default constructor that set initial values; 
        /// </summary>
        public SettingsModel()
        {
            style = 0;
            Gap = 0.6;
            symbols = 1;
            Range = 90;
        }
    }
}
