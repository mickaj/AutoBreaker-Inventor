using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Inventor;

namespace InvAddIn.Model
{
    public interface IInventorHandler
    {
        void AutoBreak();
    }

    public class InventorHandler : IInventorHandler
    {
        private Inventor.Application invInstance;
        private ISettingsModel settings;
        private IComparer<DrawingView> comparer;

        /// <summary>
        /// Contains allowed types of view orientaton
        /// </summary>
        private static List<ViewOrientationTypeEnum> cams = new List<ViewOrientationTypeEnum>(
            new ViewOrientationTypeEnum[]
            {
                    ViewOrientationTypeEnum.kBackViewOrientation,
                    ViewOrientationTypeEnum.kBottomViewOrientation,
                    ViewOrientationTypeEnum.kFrontViewOrientation,
                    ViewOrientationTypeEnum.kLeftViewOrientation,
                    ViewOrientationTypeEnum.kRightViewOrientation,
                    ViewOrientationTypeEnum.kTopViewOrientation,
                    ViewOrientationTypeEnum.kArbitraryViewOrientation
            });

        /// <summary>
        /// Provides condition to remove all views which are in not correct model orientation
        /// </summary>
        private Predicate<DrawingView> removePredicate = (DrawingView view) => (!cams.Contains(view.Camera.ViewOrientationType));

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="_instance">Current instance of Inventor application</param>
        /// <param name="_settings">Settings model</param>
        public InventorHandler(Inventor.Application _instance, ISettingsModel _settings)
        {
            invInstance = _instance;
            settings = _settings;
            comparer = new DrawingViewSizeComparerHelper();
        }

        /// <summary>
        /// Performs the auto-breaking
        /// </summary>
        public void AutoBreak()
        {
            DrawingDocument drawing = invInstance.ActiveDocument as DrawingDocument;
            DrawingView biggest = getBiggest(drawing.ActiveSheet);
            //if no usable view or view is too small to be broken 
            if ((biggest == null)||(!canBeBroken(biggest,isHorizontal(biggest))))
            {
                MessageBox.Show("There are no usable views!","Cannot apply auto-break");
                return;
            }
            if(biggest.BreakOperations.Count > 0)
            {
                MessageBox.Show("There already is a break operation applied!", "Cannot apply auto-break");
                return;
            }

            Point2d start;
            Point2d end;
            if(isHorizontal(biggest)) { horizontalPoints(biggest, (int)settings.Range, out start, out end); }
            else { verticalPoints(biggest, (int)settings.Range, out start, out end); }

            biggest.BreakOperations.Add(boolBreakOrientationConvert(isHorizontal(biggest)), start, end, intToBreakStyleConverter(settings.Style), 5, settings.Gap, settings.Symbols, true);
        }        
        
        /// <summary>
        /// Checks if a view can be brokens
        /// </summary>
        /// <param name="_view">Drawing view</param>
        /// <param name="_isHorizontal"></param>
        /// <returns>True if the view is big enough to be broken, otherwise false</returns>
        private bool canBeBroken(DrawingView _view, bool _isHorizontal)
        {
            //MessageBox.Show("can be broken?");
            if (_isHorizontal)
            {
                if(_view.Width-settings.Gap >= 5 ) { return true; }
            } else
            {
                if(_view.Height-settings.Gap >= 5) { return true; }
            }
            return false;
        }
        
        
        /// <summary>
        /// Determines whether a view is orientated vertically or horizontally 
        /// </summary>
        /// <param name="_view">Drawing view</param>
        /// <returns>Return ture if vertical, false if horizontal</returns>
        private bool isHorizontal(DrawingView _view)
        {
            if(_view.Width>=_view.Height) { return true; }
            return false;
        }

        /// <summary>
        /// Converts boolean value to BreakOrientationEnum
        /// </summary>
        /// <param name="_isHorizontal">Boolean orientation equivalent</param>
        /// <returns>BreakOrientationEnum.kHorizontal if argument is true</returns>
        private  BreakOrientationEnum boolBreakOrientationConvert(bool _isHorizontal)
        {
            if(!_isHorizontal) { return BreakOrientationEnum.kVerticalBreakOrientation; }
            return BreakOrientationEnum.kHorizontalBreakOrientation;

        }
        
        /// <summary>
        /// Gets biggest view from given drawing sheet
        /// </summary>
        /// <param name="_sheet">Drawing sheet</param>
        /// <returns>Drawing view</returns>
        private DrawingView getBiggest(Sheet _sheet)
        {
            List<DrawingView> views = new List<DrawingView>();
            foreach(DrawingView _view in _sheet.DrawingViews )
            {
                views.Add(_view);
            }
            views.RemoveAll(removePredicate);
            if (views.Count > 0)
            {
                views.Sort(comparer);
                return views[0];
            }
            else return null;
        }

        /// <summary>
        /// Helper method that outs starting and ending points for horizontal break
        /// </summary>
        /// <param name="_view">View to break</param>
        /// <param name="_percentage">Percentage of view that needs to be removed</param>
        /// <param name="_start">Out starting point</param>
        /// <param name="_end">Out ending point</param>
        /// <returns>True if break points can be used for breaking, ie. break ration is between 0.1 and 0.9</returns>
        private bool horizontalPoints(DrawingView _view, int _percentage, out Point2d _start, out Point2d _end)
        {
            //determine break points
            //MessageBox.Show("determining break points...");
            //MessageBox.Show("percentage: "+_percentage);
            double breakRatio = (double)_percentage / 100;
            //MessageBox.Show("ratio: " + breakRatio);

            if (_view.Width * breakRatio < 5) { breakRatio = 5 / _view.Width; }

            //MessageBox.Show("ratio: " + breakRatio);
            double breakWidth = _view.Width * breakRatio;
            double breakStart = _view.Position.X - breakWidth / 2;
            double breakEnd = _view.Position.X + breakWidth / 2;

            _start = invInstance.TransientGeometry.CreatePoint2d(breakStart);
            _end = invInstance.TransientGeometry.CreatePoint2d(breakEnd);
            //MessageBox.Show("Width: " + _view.Width);
            //MessageBox.Show("X centre: " + _view.Position.X);
            //MessageBox.Show("start: " + _start.X);
            //MessageBox.Show("end: " + _end.X);

            if ((breakRatio >= 0.1) && (breakRatio <= 0.9)) { return true; }
            return false;
        }

        /// <summary>
        /// Helper method that outs starting and ending points for vertical break
        /// </summary>
        /// <param name="_view">Wiew to break</param>
        /// <param name="_percentage">Percentage of view that needs to be removed</param>
        /// <param name="_start">Out starting point</param>
        /// <param name="_end">Out ending point</param>
        /// <returns>True if break points can be used for breaking, ie. break ration is between 0.1 and 0.9</returns>
        private bool verticalPoints(DrawingView _view, int _percentage, out Point2d _start, out Point2d _end)
        {
            //determine break points
            //MessageBox.Show("determining break points...");
            //MessageBox.Show("percentage: "+_percentage);
            double breakRatio = (double)_percentage / 100;

            if (_view.Height * breakRatio < 5) { breakRatio = 5 / _view.Height; }

            //MessageBox.Show("ratio: " + breakRatio);
            double breakHeight = _view.Height * breakRatio;
            double breakStart = _view.Position.Y - breakHeight / 2;
            double breakEnd = _view.Position.Y + breakHeight / 2;

            _start = invInstance.TransientGeometry.CreatePoint2d(0, breakStart);
            _end = invInstance.TransientGeometry.CreatePoint2d(0, breakEnd);
            //MessageBox.Show("Width: " + views[current].Width);
            //MessageBox.Show("X centre: " + views[current].Position.X);
            //MessageBox.Show("start: " + startPoint.X);
            //MessageBox.Show("end: " + endPoint.X);

            if ((breakRatio >= 0.1) && (breakRatio <= 0.9)) { return true; }
            return false;
        }

        /// <summary>
        /// Converts integer value to BreakStyleEnum 
        /// </summary>
        /// <param name="_input"></param>
        /// <returns></returns>
        private BreakStyleEnum intToBreakStyleConverter(int _input)
        {
            switch(_input)
            {
                case 0:
                    return BreakStyleEnum.kRectangularBreakStyle;
                case 1:
                    return BreakStyleEnum.kStructuralBreakStyle;
                default:
                    return BreakStyleEnum.kRectangularBreakStyle;
            }
        }

        
        /// <summary>
        /// Provides IComparer implementation for descending area-wise sorting of views
        /// </summary>
        internal class DrawingViewSizeComparerHelper : IComparer<DrawingView>
        {
            /// <summary>
            /// provides IComparer implementation for area-wise sorting of views 
            /// </summary>
            /// <param name="x">view no. one</param>
            /// <param name="y">view no. two</param>
            /// <returns>-1 - view one is bigger; 0 - views are equal size; 1 - view one is smaller</returns>
            public int Compare(DrawingView x, DrawingView y)
            {
                double xArea = x.Height * x.Width;
                double yArea = y.Height * y.Width;
                if (xArea > yArea) { return -1; }
                if (xArea < yArea) { return 1; }
                return 0;
            }
        }
    }
}
