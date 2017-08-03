using System.Drawing;
using System.Windows.Forms;
using Inventor;

namespace AutoBreaker
{
    public partial class StandardAddInServer : ApplicationAddInServer
    {
        internal class AxHostConverter : AxHost
        {
            private AxHostConverter()
                : base("")
            {
            }

            public static stdole.IPictureDisp ImageToPictureDisp(Image image)
            {
                return (stdole.IPictureDisp)GetIPictureDispFromPicture(image);
            }

            public static Image PictureDispToImage(stdole.IPictureDisp pictureDisp)
            {
                return GetPictureFromIPicture(pictureDisp);
            }
        }
    }
}
