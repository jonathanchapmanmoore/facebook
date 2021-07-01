﻿//-----------------------------------------------------------------------
// <copyright file="PhotoViewerControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Control used to display a full photo.
// </summary>
//-----------------------------------------------------------------------

namespace FacebookClient
{
    using System;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using ClientManager.Controls;
    using Contigo;
    using Standard;
    using System.Windows.Interop;

    [
        TemplatePart(Name = "PART_PhotoDisplay", Type = typeof(PhotoDisplayControl)),
        TemplatePart(Name = "PART_PhotoDisplayGrid", Type = typeof(Grid)),
        TemplatePart(Name = "PART_TagTargetElement", Type = typeof(TagTarget)),
        TemplatePart(Name = "PART_PhotoTaggerControl", Type = typeof(PhotoTaggerControl)),
        TemplatePart(Name = "PART_FitPhotoFrame", Type = typeof(Control)),
    ]
    public class PhotoViewerControl : SizeTemplateControl
    {
        private PhotoDisplayControl _photoDisplay;
        private Grid _photoDisplayGrid;
        private TagTarget _tagTarget;
        private PhotoTaggerControl _photoTagger;
        private Control _fitPhotoControl;

        public static readonly DependencyProperty FacebookPhotoProperty = DependencyProperty.Register(
            "FacebookPhoto", 
            typeof(FacebookPhoto), 
            typeof(PhotoViewerControl));

        public FacebookPhoto FacebookPhoto
        {
            get { return (FacebookPhoto)GetValue(FacebookPhotoProperty); }
            set { SetValue(FacebookPhotoProperty, value); }
        }

        public static RoutedCommand SetAsDesktopBackgroundCommand { get; private set; }
        public static RoutedCommand IsMouseOverTagCommand { get; private set; }
        public static RoutedCommand SavePhotoAsCommand { get; private set; }
        public static RoutedCommand SaveAlbumCommand { get; private set; }
        public static RoutedCommand StartSlideShowCommand { get; private set; }

        static PhotoViewerControl()
        {
            SetAsDesktopBackgroundCommand = new RoutedCommand("SetAsDesktopBackground", typeof(PhotoViewerControl));
            IsMouseOverTagCommand = new RoutedCommand("IsMouseOverTag", typeof(PhotoViewerControl));
            SaveAlbumCommand = new RoutedCommand("SaveAlbum", typeof(PhotoViewerControl));
            SavePhotoAsCommand = new RoutedCommand("SavePhotoAs", typeof(PhotoViewerControl));
            StartSlideShowCommand = new RoutedCommand("StartSlideShow", typeof(PhotoViewerControl));
        }
      
        public PhotoViewerControl()
        {
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Print, (sender, e) => _PrintPhoto()));
            CommandBindings.Add(new CommandBinding(SetAsDesktopBackgroundCommand, (sender, e) => _SetAsDesktopBackground()));
            CommandBindings.Add(new CommandBinding(IsMouseOverTagCommand, (sender, e) => _OnIsMouseOverTag(e)));
            CommandBindings.Add(new CommandBinding(SaveAlbumCommand, (sender, e) => _SaveAlbum()));
            CommandBindings.Add(new CommandBinding(SavePhotoAsCommand, (sender, e) => _SavePhoto()));
            CommandBindings.Add(new CommandBinding(StartSlideShowCommand, (sender, e) => ((PhotoViewerControl)sender)._StartSlideShow()));

            KeyDown += _OnKeyDown;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _photoDisplay = this.Template.FindName("PART_PhotoDisplay", this) as PhotoDisplayControl;
            Assert.IsNotNull(_photoDisplay);
            _tagTarget = this.Template.FindName("PART_TagTargetElement", this) as TagTarget;
            Assert.IsNotNull(_tagTarget);
            _photoDisplayGrid = this.Template.FindName("PART_PhotoDisplayGrid", this) as Grid;
            Assert.IsNotNull(_photoDisplayGrid);
            _photoTagger = this.Template.FindName("PART_PhotoTaggerControl", this) as PhotoTaggerControl;
            Assert.IsNotNull(_photoTagger);
            _fitPhotoControl = this.Template.FindName("PART_FitPhotoFrame", this) as Control;
            //Assert.IsNotNull(_fitPhotoControl);

            _photoDisplay.FitControl = _fitPhotoControl;

            //_photoScrollViewer.PreviewMouseWheel += new MouseWheelEventHandler(OnPhotoScrollViewerPreviewMouseWheel);
            _photoDisplay.PhotoStateChanged += new PhotoStateChangedEventHandler(PhotoDisplay_PhotoStateChanged);

            // Focus the control so that we can grab keyboard events.
            this.Focus();
        }

        // Weird that this is public.  It's used as a generic handler for mouse scrolling from the filmstrip panel...
        public static void HandleScrollViewerMouseWheel(ScrollViewer scrollViewer, bool checkExtent, MouseWheelEventArgs e)
        {
            // This ScrollViewer will swallow mousewheel events even when it can't be scrolled down any more.
            // We workaround this by raising a new event in that case.

            if (!e.Handled)
            {
                if (!checkExtent || scrollViewer.ExtentHeight <= scrollViewer.ViewportHeight)
                {
                    e.Handled = true;
                    UIElement parent = (UIElement)scrollViewer.Parent;

                    parent.RaiseEvent(
                        new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                        {
                            RoutedEvent = UIElement.MouseWheelEvent,
                            Source = scrollViewer
                        });
                }
            }
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Delta > 0)
                {
                    PhotoDisplayControl.ZoomPhotoInCommand.Execute(e, _photoDisplay);
                }
                else if (e.Delta < 0)
                {
                    PhotoDisplayControl.ZoomPhotoOutCommand.Execute(e, _photoDisplay);
                }

                e.Handled = true;
            }
            else
            {
                base.OnPreviewMouseWheel(e);
            }
        }

        /// <summary>
        /// Allows the photo to be fit to the current window size using the mouse wheel.
        /// </summary>
        /// <param name="e">Arguments describing the event.</param>
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.MiddleButton == MouseButtonState.Pressed)
                {
                    _photoDisplay.FitToWindow();
                }
            }
        }

        private void _PrintPhoto()
        {
            FacebookPhoto.Image.SaveToFile(FacebookImageDimensions.Big, Path.GetTempFileName(), true, FacebookImageSaveOptions.PreserveOriginal, _PrintFileCallback, null);
        }

        private void _PrintFileCallback(object sender, SaveImageCompletedEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke((SaveImageAsyncCallback)_PrintFileCallback, new[] { sender, e });
                return;
            }

            if (e.Cancelled || e.Error != null)
            {
                MessageBox.Show("Unable to print the image");
                return;
            }

            object path = e.ImagePath;
            WIA.CommonDialog dialog = new WIA.CommonDialogClass();
            dialog.ShowPhotoPrintingWizard(ref path);
        }

        private void _SetAsDesktopBackground()
        {
            FacebookPhoto.Image.SaveToFile(FacebookImageDimensions.Big, Path.GetTempFileName(), true, FacebookImageSaveOptions.PreserveOriginal, _SetAsDesktopBackgroundCallback, null);
        }

        private void _SetAsDesktopBackgroundCallback(object sender, SaveImageCompletedEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke((SaveImageAsyncCallback)_SetAsDesktopBackgroundCallback, sender, e);
                return;
            }

            if (e.Cancelled || e.Error != null)
            {
                MessageBox.Show("Unable to use this photo for the desktop background");
                return;
            }

            string photoLocalPath = e.ImagePath;
            string wallpaperPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Fishbowl\\Wallpaper") + Path.GetExtension(photoLocalPath);

            try
            {
                // Clear the current wallpaper (releases lock on current bitmap)
                NativeMethods.SystemParametersInfo(SPI.SETDESKWALLPAPER, 0, String.Empty, SPIF.UPDATEINIFILE | SPIF.SENDWININICHANGE);

                // Delete the old wallpaper if it exists
                Utility.SafeDeleteFile(wallpaperPath);
                Utility.EnsureDirectory(Path.GetDirectoryName(wallpaperPath));

                File.Copy(photoLocalPath, wallpaperPath);
                NativeMethods.SystemParametersInfo(SPI.SETDESKWALLPAPER, 0, wallpaperPath, SPIF.UPDATEINIFILE | SPIF.SENDWININICHANGE);
            }
            catch (Exception)
            { }
        }

        private void _SavePhoto()
        {
            var photo = FacebookPhoto;
            if (photo == null)
            {
                return;
            }
            string defaultFileName = "Facebook Photo";
            if (photo.Album != null)
            {
                defaultFileName = photo.Album.Title + " (" + (photo.Album.Photos.IndexOf(photo) + 1) + ")";
            }

            string filePath = null;
            if (Utility.IsOSVistaOrNewer)
            {
                IFileSaveDialog pFileSaveDialog = null;
                try
                {
                    pFileSaveDialog = CLSID.CoCreateInstance<IFileSaveDialog>(CLSID.FileSaveDialog);
                    pFileSaveDialog.SetOptions(pFileSaveDialog.GetOptions() | FOS.FORCEFILESYSTEM | FOS.OVERWRITEPROMPT);
                    pFileSaveDialog.SetTitle("Select where to save the photo");
                    pFileSaveDialog.SetOkButtonLabel("Save Photo");
                    var filterspec = new COMDLG_FILTERSPEC { pszName = "Images", pszSpec = "*.jpg;*.png;*.bmp;*.gif" };
                    pFileSaveDialog.SetFileTypes(1, ref filterspec);
                    pFileSaveDialog.SetFileName(defaultFileName);
                    Guid clientId = new Guid(0x8c18c882, 0x482b, 0x459d, 0x9a, 0xc6, 0x6c, 0xc5, 0x39, 0x71, 0xa7, 0xf7);
                    pFileSaveDialog.SetClientGuid(ref clientId);

                    IShellItem pItem = null;
                    try
                    {
                        pItem = ShellUtil.GetShellItemForPath(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
                        pFileSaveDialog.SetDefaultFolder(pItem);
                    }
                    finally
                    {
                        Utility.SafeRelease(ref pItem);
                    }

                    HRESULT hr = pFileSaveDialog.Show(new WindowInteropHelper(Application.Current.MainWindow).Handle);
                    if (hr.Failed)
                    {
                        Assert.AreEqual((HRESULT)Win32Error.ERROR_CANCELLED, hr);
                        return;
                    }

                    pItem = null;
                    try
                    {
                        pItem = pFileSaveDialog.GetResult();
                        filePath = ShellUtil.GetPathFromShellItem(pItem);
                    }
                    finally
                    {
                        Utility.SafeRelease(ref pItem);
                    }
                }
                finally
                {
                    Utility.SafeRelease(ref pFileSaveDialog);
                }
            }
            else
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Image Files|*.jpg;*.png;*.bmp;*.gif",
                    FileName = defaultFileName,
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                };

                if (saveFileDialog.ShowDialog(Application.Current.MainWindow) != true)
                {
                    return;
                }

                filePath = saveFileDialog.FileName;
            }

            FacebookImageSaveOptions fiso = FacebookImageSaveOptions.FindBetterName;

            // We told the file dialog to prompt about overwriting, so if the user specified a location
            // with a file extension and the file already exists, prepare to overwrite.
            // This isn't quite right because the file extension may be different, so we may overwrite a jpg 
            // when it was asked to be a gif, but it's not a likely scenario.
            if (System.IO.File.Exists(filePath))
            {
                fiso = FacebookImageSaveOptions.Overwrite;
            }

            photo.Image.SaveToFile(FacebookImageDimensions.Big, filePath, true, fiso, (sender, e) => { }, null);
        }
        
        /// <summary>
        /// Saves every photo in the currently displayed album to a user-provided location.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments describing the event.</param>
        private void _SaveAlbum()
        {
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Choose where to save the album.",
                ShowNewFolderButton = true,
            };
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FacebookPhoto.Album.SaveToFolder(folderDialog.SelectedPath, (sender, e) => { }, null);
            }
        }

        private void _OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Handled)
            {
                if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                {
                    switch (e.Key)
                    {
                        case Key.OemPlus:
                            PhotoDisplayControl.ZoomPhotoInCommand.Execute(null, _photoDisplay);
                            e.Handled = true;
                            break;
                        case Key.OemMinus:
                            PhotoDisplayControl.ZoomPhotoOutCommand.Execute(null, _photoDisplay);
                            e.Handled = true;
                            break;
                        case Key.D0:
                            PhotoDisplayControl.FitPhotoToWindowCommand.Execute(null, _photoDisplay);
                            e.Handled = true;
                            break;
                        default:
                            break;
                    }
                }
                else if (e.KeyboardDevice.Modifiers == ModifierKeys.None)
                {
                    switch (e.Key)
                    {
                        case Key.Escape:
                            Focus();
                            e.Handled = true;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Place the photo tagger to the bottom left of given point.
        /// </summary>
        /// <param name="photoViewer">PhotoViewerControl which holds the tagger control.</param>
        /// <param name="topRightCorner">Top right corner point of tagger.</param>
        private static void PlaceTaggerControl(PhotoViewerControl photoViewer, Point topRightCorner)
        {
            double widthByTwo = photoViewer._tagTarget.Width / 2;
            double heightByTwo = photoViewer._tagTarget.Height / 2;

            double x = topRightCorner.X - photoViewer._photoTagger.Width - widthByTwo;
            double y = topRightCorner.Y - heightByTwo;

            if (topRightCorner.Y + photoViewer._photoTagger.Height > photoViewer._photoDisplayGrid.ActualHeight)
            {
                y = photoViewer._photoDisplayGrid.ActualHeight - photoViewer._photoTagger.Height - heightByTwo;
            }

            if (topRightCorner.X - widthByTwo < photoViewer._photoTagger.Width)
            {
                x = topRightCorner.X + widthByTwo;
            }

            photoViewer._photoTagger.TransformPoint = new Point(x, y);
        }

        /// <summary>
        /// Tag target scale has changed, reposition.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event args.</param>
        private void PhotoDisplay_PhotoStateChanged(object sender, EventArgs e)
        {
            this._tagTarget.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Hide the tag target, or show it and position it accordingly.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event args.</param>
        private void _OnIsMouseOverTag(ExecutedRoutedEventArgs e)
        {
            if (e.Parameter == null)
            {
                // Hide tag target
                _tagTarget.Visibility = Visibility.Collapsed;
                return;
            }

            // Position tag and show
            var pt = (Point)e.Parameter;

            MatrixTransform mt = (MatrixTransform)_photoDisplay.TransformToVisual(_photoDisplay.PhotoImage);

            _tagTarget.Scale = Math.Sqrt(1 / mt.Matrix.M11);

            double widthByTwo = _tagTarget.Width / 2;
            double heightByTwo = _tagTarget.Height / 2;

            Point transPt = new Point(widthByTwo * mt.Matrix.M11, heightByTwo * mt.Matrix.M22);

            // Convert point from percentage to pixels

            pt = new Point(pt.X * _photoDisplay.PhotoImage.ActualWidth + mt.Matrix.M11,
                           pt.Y * _photoDisplay.PhotoImage.ActualHeight + mt.Matrix.M22);

            double left;
            double top;

            // Ensure tag target element does not go outside of photo boundry
            if (pt.X < transPt.X)
            {
                left = transPt.X;
            }
            else if (pt.X > (_photoDisplay.PhotoImage.ActualWidth - transPt.X))
            {
                left = _photoDisplay.PhotoImage.ActualWidth - transPt.X;
            }
            else
            {
                left = pt.X;
            }

            // Ensure tag target element does not go outside of photo boundry
            if (pt.Y < transPt.Y)
            {
                top = transPt.Y;
            }
            else if (pt.Y > (_photoDisplay.PhotoImage.ActualHeight - transPt.Y))
            {
                top = _photoDisplay.PhotoImage.ActualHeight - transPt.Y;
            }
            else
            {
                top = pt.Y;
            }

            // Set new coordinates
            transPt = _photoDisplay.PhotoImage.TranslatePoint(new Point(left, top), _photoDisplayGrid);
            _tagTarget.TransformPoint = transPt;
            _tagTarget.Visibility = Visibility.Visible;
        }

        private void _StartSlideShow()
        {
            ((FacebookClientApplication)Application.Current).SwitchToSlideShow(FacebookPhoto.Album.Photos, FacebookPhoto);
        }
    }
}