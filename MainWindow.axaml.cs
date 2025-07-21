using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.Styling;
using Avalonia;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeletextSharedResources;

namespace Teletext
{
    public partial class MainWindow : Window
    {

        Service service = new Service();
        private TeletextRenderer renderer;
        private Page currentPage;
        private string currentFontName = "Mullard";
        private bool showBorders = true;
        private List<Page> recoveredPages = new List<Page>();
        private ThemeVariant currentTheme = ThemeVariant.Default;
        private const int ThumbnailWidth = 160;
        private const int ThumbnailHeight = 120;

        public MainWindow()
        {
            InitializeComponent();
            InitializeRenderer();
            InitializeTheme();
            InitializeDragDrop();
            UpdateStatus("Teletext Recovery Editor initialized.");
        }

        private void InitializeDragDrop()
        {
            // Wire up drag and drop events programmatically
            this.AddHandler(DragDrop.DragEnterEvent, MainWindow_DragEnter);
            this.AddHandler(DragDrop.DropEvent, MainWindow_Drop);
        }

        private void InitializeTheme()
        {
            // Set initial theme menu indicators
            SetTheme(ThemeVariant.Default, "System Default");
        }

        private void InitializeRenderer()
        {
            try
            {
                renderer = new TeletextRenderer(showBorders);
                renderer.Font = currentFontName;
                
                // Set DPI for proper scaling
                var scaling = this.RenderScaling;
                renderer.DeviceDPI = (float)(96.0 * scaling);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error initializing renderer: {ex.Message}");
            }
        }

        private void UpdateStatus(string message)
        {
            StatusText.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
        }

        // File Menu Handlers
        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var topLevel = GetTopLevel(this);
                var folders = await topLevel!.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Open Teletext Folder",
                    AllowMultiple = false
                });

                if (folders.Count >= 1)
                {
                    await LoadTeletextFolder(folders[0].Path.LocalPath);
                    UpdateStatus($"Folder opened: {Path.GetFileName(folders[0].Path.LocalPath)}");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error opening folder: {ex.Message}");
            }
        }

        private async void OpenT42Service_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var topLevel = GetTopLevel(this);
                
                var files = await topLevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Open T42 Service File",
                    AllowMultiple = false,
                    FileTypeFilter = new[] 
                    { 
                        new FilePickerFileType("T42 Service Files") { Patterns = new[] { "*.t42" } },
                        FilePickerFileTypes.All
                    }
                });

                if (files.Count >= 1)
                {
                    await LoadT42Service(files[0].Path.LocalPath);
                    UpdateStatus($"T42 Service opened: {Path.GetFileName(files[0].Path.LocalPath)}");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error opening T42 service: {ex.Message}");
            }
        }

        private async void SaveT42Service_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var topLevel = GetTopLevel(this);
                
                var file = await topLevel!.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save T42 Service File",
                    SuggestedFileName = service.Filename ?? "teletext.t42",
                    FileTypeChoices = new[] 
                    { 
                        new FilePickerFileType("T42 Service Files") { Patterns = new[] { "*.t42" } },
                        FilePickerFileTypes.All
                    }
                });

                if (file != null)
                {
                    service.SaveService(file.Path.LocalPath);
                    UpdateStatus($"T42 Service saved: {Path.GetFileName(file.Path.LocalPath)}");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error saving T42 service: {ex.Message}");
            }
            await Task.Delay(1);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Carousel Menu Handlers
        private async void OpenCarousel_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Open Carousel - Feature to be implemented");
            await Task.Delay(1);
        }

        private async void SaveCarousel_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Save Carousel - Feature to be implemented");
            await Task.Delay(1);
        }

        private async void SaveCarouselAs_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Save Carousel As - Feature to be implemented");
            await Task.Delay(1);
        }

        private async void ExportCarousel_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Export Carousel - Feature to be implemented");
            await Task.Delay(1);
        }

        private async void ClearCarousel_Click(object sender, RoutedEventArgs e)
        {
            recoveredPages.Clear();
            RecoveredPagesList.Items.Clear();
            ThumbnailPanel.Children.Clear();
            ClearPageDisplay();
            UpdateStatus("Carousel cleared");
            await Task.Delay(1);
        }

        // Page Menu Handlers
        private async void ExportCurrentPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage == null)
            {
                UpdateStatus("No page to export");
                return;
            }
            UpdateStatus("Export Current Page - Feature to be implemented");
            await Task.Delay(1);
        }

        private async void ExportPNG_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage == null)
            {
                UpdateStatus("No page to export as PNG");
                return;
            }
            UpdateStatus("Export as PNG - Feature to be implemented");
            await Task.Delay(1);
        }

        // Preferences Menu Handlers
        private void FontMullard_Click(object sender, RoutedEventArgs e)
        {
            SetFont("Mullard");
        }

        private void FontETS_Click(object sender, RoutedEventArgs e)
        {
            SetFont("ETS");
        }

        private void FontTiFax_Click(object sender, RoutedEventArgs e)
        {
            SetFont("TiFax");
        }

        private void ShowBorders_Click(object sender, RoutedEventArgs e)
        {
            showBorders = !showBorders;
            ShowBordersMenuItem.Header = showBorders ? "Show _Borders ✓" : "Show _Borders";
            UpdateStatus($"Borders: {(showBorders ? "Enabled" : "Disabled")}");
            
            // Recreate renderer with new border setting, preserving current font
            string currentFont = renderer?.Font ?? currentFontName;
            try
            {
                renderer = new TeletextRenderer(showBorders);
                renderer.Font = currentFont;
                
                // Set DPI for proper scaling
                var scaling = this.RenderScaling;
                renderer.DeviceDPI = (float)(96.0 * scaling);
                
                RefreshDisplay();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error updating renderer borders: {ex.Message}");
            }
        }

        // Theme Menu Handlers
        private void ThemeSystem_Click(object sender, RoutedEventArgs e)
        {
            SetTheme(ThemeVariant.Default, "System Default");
        }

        private void ThemeLight_Click(object sender, RoutedEventArgs e)
        {
            SetTheme(ThemeVariant.Light, "Light");
        }

        private void ThemeDark_Click(object sender, RoutedEventArgs e)
        {
            SetTheme(ThemeVariant.Dark, "Dark");
        }

        private void SetTheme(ThemeVariant theme, string themeName)
        {
            currentTheme = theme;
            
            // Update menu item indicators
            ThemeSystemMenuItem.Header = theme == ThemeVariant.Default ? "_System Default ✓" : "_System Default";
            ThemeLightMenuItem.Header = theme == ThemeVariant.Light ? "_Light ✓" : "_Light";
            ThemeDarkMenuItem.Header = theme == ThemeVariant.Dark ? "_Dark ✓" : "_Dark";
            
            // Apply theme to application
            if (Application.Current != null)
            {
                Application.Current.RequestedThemeVariant = theme;
                UpdateStatus($"Theme set to: {themeName}");
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            // Create a simple about dialog
            var aboutWindow = new Window
            {
                Title = "About Teletext Recovery Editor",
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var stackPanel = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 10
            };

            stackPanel.Children.Add(new TextBlock 
            { 
                Text = "Teletext Recovery Editor", 
                FontSize = 16, 
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            });

            stackPanel.Children.Add(new TextBlock 
            { 
                Text = "Avalonia Version",
                FontSize = 12,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            });

            stackPanel.Children.Add(new TextBlock 
            { 
                Text = "A tool for editing and recovering Teletext pages",
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            });

            var okButton = new Button 
            { 
                Content = "OK", 
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Avalonia.Thickness(0, 20, 0, 0)
            };
            okButton.Click += (s, e) => aboutWindow.Close();

            stackPanel.Children.Add(okButton);
            aboutWindow.Content = stackPanel;
            aboutWindow.ShowDialog(this);
        }

        // Page Control Handlers
        private void LoadPage_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(PageTextBox.Text, out int pageNumber))
            {
                LoadPageByNumber(pageNumber);
            }
            else
            {
                UpdateStatus("Invalid page number");
            }
        }

        private void SavePage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage != null)
            {
                UpdateStatus("Save Page - Feature to be implemented");
            }
            else
            {
                UpdateStatus("No page to save");
            }
        }

        private async void RecoveredPagesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RecoveredPagesList.SelectedItem is ListBoxItem selectedItem)
            {
                if (selectedItem.Tag is Page selectedPage)
                {
                    currentPage = selectedPage;
                    DisplayPage(selectedPage);
                    UpdatePageInfo(selectedPage);
                }
                else if (selectedItem.Tag is string filePath)
                {
                    UpdateStatus($"Loading file: {Path.GetFileName(filePath)}...");
                    
                    // Re-instantiate the service to ensure a clean state for the new file.
                    service = new Service();

                    if (filePath.EndsWith(".t42", StringComparison.OrdinalIgnoreCase))
                    {
                        await LoadT42Service(filePath);
                    }
                    else
                    {
                        // For individual files from folder view, load all pages for thumbnails
                        await LoadPagesForThumbnails(filePath);
                    }
                }
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshDisplay();
            RefreshThumbnails();
            UpdateStatus("Display and thumbnails refreshed");
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ClearPageDisplay();
            currentPage = null;
            UpdateStatus("Display cleared");
        }

        private async Task LoadT42Service(string filePath)
        {
            try
            {
                string status = service.OpenService(filePath);
                
                if (status == "")
                {
                    service.CheckHorizon = 0x200000;

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        recoveredPages.Clear();
                        RecoveredPagesList.Items.Clear();
                    });

                    var carousels = service.ListCarousels();
                    UpdateStatus($"Found {carousels.Count} carousels in T42 service");
                    
                    await LoadAllPagesFromService();
                    
                    this.Title = $"Teletext Recovery Editor : {Path.GetFileName(filePath)}";
                    UpdateStatus($"T42 Service loaded successfully with {recoveredPages.Count} pages");
                }
                else
                {
                    UpdateStatus($"Error loading T42 service: {status}");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error loading T42 service: {ex.Message}");
            }
            await Task.Delay(1);
        }

        private async Task LoadAllPagesFromService()
        {
            try
            {
                int pageCount = 0;
                int maxPages = 5000;
                
                service.Position = 0;
                
                UpdateStatus("Loading pages from T42 service...");
                
                while (service.Revolutions == 0 && pageCount < maxPages)
                {
                    var page = service.GetPage();
                    if (page != null && page.Lines != null && page.Lines.Length > 0)
                    {
                        if (page.Lines.Length > 0 && page.Lines[0].Text != null)
                        {
                            page.Lines[0].Text = "   P" + page.Lines[0].Magazine + page.Lines[0].Page + " " + page.Lines[0].Text;
                        }
                        
                        recoveredPages.Add(page);
                        pageCount++;
                        
                        // Update progress every 50 pages for better responsiveness
                        if (pageCount % 50 == 0)
                        {
                            UpdateStatus($"Loading pages... {pageCount} pages loaded");
                            
                            // Allow UI to update periodically
                            if (pageCount % 200 == 0)
                            {
                                await Task.Delay(1);
                            }
                        }
                    }
                    
                    // Safety check to prevent infinite loops
                    if (service.Position > 100000000) // 100MB safety limit
                    {
                        UpdateStatus($"Reached safety limit, stopping at {pageCount} pages");
                        break;
                    }
                }
                
                await Dispatcher.UIThread.InvokeAsync(() => RefreshRecoveredPagesList());
                UpdateStatus($"Successfully loaded {pageCount} pages from T42 service");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error loading pages from service: {ex.Message}");
            }
            await Task.Delay(1);
        }

        private async Task LoadT42ServiceFolder(string folderPath)
        {
            try
            {
                recoveredPages.Clear();
                RecoveredPagesList.Items.Clear();
                
                var files = Directory.GetFiles(folderPath, "*.t42")
                    .OrderBy(f => f)
                    .ToArray();

                if (files.Length == 0)
                {
                    UpdateStatus("No .t42 files found in folder");
                    return;
                }

                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    var listBoxItem = new ListBoxItem
                    {
                        Content = fileName,
                        Tag = file
                    };
                    RecoveredPagesList.Items.Add(listBoxItem);
                }

                this.Title = $"Teletext Recovery Editor : {Path.GetFileName(folderPath)}";
                UpdateStatus($"Found {files.Length} T42 service files. Select a file to load.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error loading folder: {ex.Message}");
            }
            await Task.Delay(1);
        }

        private async Task LoadTeletextFolder(string folderPath)
        {
            try
            {
                if (!IsValidTeletextFolder(folderPath))
                {
                    UpdateStatus("No valid teletext files found in folder");
                    return;
                }

                recoveredPages.Clear();
                RecoveredPagesList.Items.Clear();
                ThumbnailPanel.Children.Clear(); // Clear thumbnails when switching folders
                
                var files = Directory.GetFiles(folderPath, "*.*")
                    .Where(f => IsValidTeletextFile(f))
                    .OrderBy(f => f)
                    .ToArray();

                UpdateStatus($"Found {files.Length} teletext files in folder");

                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    var fileSize = new FileInfo(file).Length;
                    var displayText = $"{fileName} ({FormatFileSize(fileSize)})";
                    
                    var listBoxItem = new ListBoxItem
                    {
                        Content = displayText,
                        Tag = file
                    };
                    RecoveredPagesList.Items.Add(listBoxItem);
                }

                this.Title = $"Teletext Recovery Editor : {Path.GetFileName(folderPath)}";
                UpdateStatus($"Loaded folder with {files.Length} teletext files. Select files to load thumbnails.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error loading folder: {ex.Message}");
            }
            await Task.Delay(1);
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes == 0)
                return "0 B";

            const int scale = 1024;
            string[] orders = new string[] { "B", "KB", "MB", "GB", "TB" };
            
            int orderIndex = 0;
            double size = bytes;
            
            while (size >= scale && orderIndex < orders.Length - 1)
            {
                size /= scale;
                orderIndex++;
            }
            
            return string.Format("{0:##.##} {1}", size, orders[orderIndex]).TrimStart();
        }

        // Helper Methods
        private bool IsValidTeletextFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension == ".t42" || extension == ".bin" || extension == ".vbi" || extension == ".pes";
        }

        private bool IsValidTeletextFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return false;
                
            var files = Directory.GetFiles(folderPath, "*.*")
                .Where(f => IsValidTeletextFile(f))
                .Take(1); // Just check if at least one valid file exists
                
            return files.Any();
        }

        private void SetFont(string fontName)
        {
            currentFontName = fontName;
            if (renderer != null)
            {
                renderer.Font = fontName;
                
                // Preserve DPI settings when changing font
                var scaling = this.RenderScaling;
                renderer.DeviceDPI = (float)(96.0 * scaling);
                
                RefreshDisplay();
                UpdateStatus($"Font set to: {fontName}");
            }
        }

        private async Task LoadFile(string filePath)
        {
            try
            {
                string status = service.OpenService(filePath);
                if (status == "")
                {
                    service.CheckHorizon = 0x200000;
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        recoveredPages.Clear();
                        RecoveredPagesList.Items.Clear();
                    });

                    // Loop to get all pages
                    int pageCount = 0;
                    int maxPages = 10000; // Safety break
                    service.Position = 0;
                    
                    while(service.Revolutions == 0 && pageCount < maxPages)
                    {
                        var page = service.GetPage();
                        if (page != null && page.Lines != null && page.Lines.Length > 0)
                        {
                            // This is the fix from before, which is still needed.
                            if (page.Lines.Length > 0 && page.Lines[0].Text != null)
                            {
                                page.Lines[0].Text = "   P" + page.Lines[0].Magazine + page.Lines[0].Page + " " + page.Lines[0].Text;
                            }
                            recoveredPages.Add(page);
                            pageCount++;
                        }
                        
                        // Safety check to prevent infinite loops
                        if (service.Position > 100000000) // 100MB safety limit
                        {
                            UpdateStatus($"Reached safety limit, stopping at {pageCount} pages");
                            break;
                        }
                    }

                    await Dispatcher.UIThread.InvokeAsync(() => RefreshRecoveredPagesList());
                    this.Title = "Teletext Recovery Editor : " + Path.GetFileName(filePath);
                    UpdateStatus($"File loaded with {recoveredPages.Count} pages.");
                }
                else
                {
                    UpdateStatus($"Error loading file: {status}");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error loading file: {ex.Message}");
            }
            await Task.Delay(1);
        }

        private async Task LoadSingleFileFromFolder(string filePath)
        {
            try
            {
                string status = service.OpenService(filePath);
                if (status == "")
                {
                    service.CheckHorizon = 0x200000;

                    // Get just the first page from the file for display
                    service.Position = 0;
                    var page = service.GetPage();
                    if (page != null && page.Lines != null && page.Lines.Length > 0)
                    {
                        // Format header text for renderer compatibility
                        if (page.Lines.Length > 0 && page.Lines[0].Text != null)
                        {
                            page.Lines[0].Text = "   P" + page.Lines[0].Magazine + page.Lines[0].Page + " " + page.Lines[0].Text;
                        }
                        
                        currentPage = page;
                        DisplayPage(page);
                        UpdatePageInfo(page);
                        UpdateStatus($"Loaded page from {Path.GetFileName(filePath)}");
                    }
                    else
                    {
                        UpdateStatus($"No valid pages found in {Path.GetFileName(filePath)}");
                    }
                }
                else
                {
                    UpdateStatus($"Error loading file: {status}");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error loading file: {ex.Message}");
            }
            await Task.Delay(1);
        }

        private async Task LoadPagesForThumbnails(string filePath)
        {
            try
            {
                UpdateStatus($"Loading pages for thumbnails from {Path.GetFileName(filePath)}...");
                
                // Clear existing pages and thumbnails on UI thread, but don't clear the file list
                await Dispatcher.UIThread.InvokeAsync(() => {
                    recoveredPages.Clear();
                    ThumbnailPanel.Children.Clear();
                    // Don't clear RecoveredPagesList.Items - keep the folder file list intact
                });
                
                // Create a temporary service for loading all pages
                var tempService = new Service();
                string status = tempService.OpenService(filePath);
                
                if (status == "")
                {
                    tempService.CheckHorizon = 0x200000;
                    tempService.Position = 0;
                    
                    int pagesLoaded = 0;
                    int maxPages = 200; // Limit for performance
                    
                    while (tempService.Revolutions == 0 && pagesLoaded < maxPages)
                    {
                        var page = tempService.GetPage();
                        if (page != null && page.Lines != null && page.Lines.Length > 0)
                        {
                            // Format header text for renderer compatibility
                            if (page.Lines.Length > 0 && page.Lines[0].Text != null)
                            {
                                page.Lines[0].Text = "   P" + page.Lines[0].Magazine + page.Lines[0].Page + " " + page.Lines[0].Text;
                            }
                            recoveredPages.Add(page);
                            pagesLoaded++;
                        }
                        
                        // Safety check to prevent infinite loops
                        if (tempService.Position > 100000000) // 100MB safety limit
                        {
                            UpdateStatus($"Reached safety limit, stopping at {pagesLoaded} pages");
                            break;
                        }
                    }
                    
                    if (pagesLoaded > 0)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() => {
                            // Only refresh thumbnails, don't refresh the recovered pages list
                            // because we want to keep showing the folder file list
                            RefreshThumbnailsOnly();
                        });
                        UpdateStatus($"Loaded {pagesLoaded} pages with thumbnails from {Path.GetFileName(filePath)}");
                    }
                    else
                    {
                        UpdateStatus($"No valid pages found in {Path.GetFileName(filePath)}");
                    }
                }
                else
                {
                    UpdateStatus($"Error loading file for thumbnails: {status}");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error loading pages for thumbnails: {ex.Message}");
            }
            await Task.Delay(1);
        }

        private void LoadPageByNumber(int pageNumber)
        {
            var page = recoveredPages.FirstOrDefault(p => 
                p.Lines?.Length > 0 && GetPageNumber(p) == pageNumber);
            
            if (page != null)
            {
                currentPage = page;
                DisplayPage(page);
                UpdatePageInfo(page);
                UpdateStatus($"Page {pageNumber} loaded");
            }
            else
            {
                UpdateStatus($"Page {pageNumber} not found");
            }
        }

        private void RefreshRecoveredPagesList()
        {
            try
            {
                RecoveredPagesList.Items.Clear();
                foreach (var page in recoveredPages.OrderBy(p => GetPageSortKey(p)))
                {
                    var pageNumber = GetPageNumber(page);
                    var subPageNumber = GetSubPageNumber(page);
                    var magazine = (page.Lines != null && page.Lines.Length > 0) ? page.Lines[0].Magazine : 0;
                    var displayText = $"P{magazine}{pageNumber:00}.{subPageNumber:00}";
                    
                    var listBoxItem = new ListBoxItem
                    {
                        Content = displayText,
                        Tag = page
                    };
                    RecoveredPagesList.Items.Add(listBoxItem);
                }
                
                // Also refresh thumbnails when pages are loaded
                RefreshThumbnails();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error refreshing recovered pages list: {ex.Message}");
            }
        }
        
        private string GetPageSortKey(Page p)
        {
            var magazine = (p.Lines != null && p.Lines.Length > 0) ? p.Lines[0].Magazine : 0;
            var pageNum = GetPageNumber(p);
            var subPageNum = GetSubPageNumber(p);
            return $"{magazine:0}{pageNum:00}{subPageNum:00}";
        }

        private void DisplayPage(Page page)
        {
            try
            {
                PageCanvas.Children.Clear();
                if (renderer != null && page.Lines != null)
                {
                    var layers = renderer.Render(page, false, false);
                    if (layers.Foreground != null)
                    {
                        using (var ms = new MemoryStream())
                        {
                            layers.Foreground.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                            ms.Seek(0, SeekOrigin.Begin);
                            var avaloniaBitmap = new Avalonia.Media.Imaging.Bitmap(ms);
                            
                            // Calculate DPI-aware dimensions
                            var scaling = this.RenderScaling;
                            var displayWidth = avaloniaBitmap.PixelSize.Width / scaling;
                            var displayHeight = avaloniaBitmap.PixelSize.Height / scaling;
                            
                            // Resize canvas to exactly fit the image with DPI scaling
                            PageCanvas.Width = displayWidth;
                            PageCanvas.Height = displayHeight;
                            
                            var image = new Image 
                            { 
                                Source = avaloniaBitmap, 
                                Width = displayWidth, 
                                Height = displayHeight 
                            };
                            PageCanvas.Children.Add(image);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error displaying page: {ex.Message}");
            }
        }

        private void UpdatePageInfo(Page page)
        {
            if (page.Lines?.Length > 0)
            {
                MagazineTextBox.Text = page.Lines[0].Magazine.ToString();
                PageNumberTextBox.Text = GetPageNumber(page).ToString("00");
                SubPageTextBox.Text = GetSubPageNumber(page).ToString("00");
                TimeCodeTextBox.Text = page.Lines[0].TimeCode ?? "";
            }
        }

        private void RefreshDisplay()
        {
            if (currentPage != null)
            {
                DisplayPage(currentPage);
            }
        }

        private void ClearPageDisplay()
        {
            PageCanvas.Children.Clear();
            // Reset canvas to default size when no content
            PageCanvas.Width = 480;
            PageCanvas.Height = 576;
            MagazineTextBox.Text = "";
            PageNumberTextBox.Text = "";
            SubPageTextBox.Text = "";
            TimeCodeTextBox.Text = "";
        }

        private int GetPageNumber(Page page)
        {
            if (page.Lines?.Length > 0 && int.TryParse(page.Lines[0].Page, out int pageNum))
                return pageNum;
            return 0;
        }

        private int GetSubPageNumber(Page page)
        {
            // The SubPage is stored in the page.Subpage property as Int32
            return page.Subpage;
        }

        // Drag and Drop Event Handlers
        private void MainWindow_DragEnter(object sender, DragEventArgs e)
        {
            // Check if the drag data contains files
            if (e.Data.Contains(DataFormats.Files))
            {
                e.DragEffects = DragDropEffects.Copy;
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }
        }

        private async void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.Files))
            {
                var files = e.Data.GetFiles();
                if (files != null)
                {
                    var firstFile = files.FirstOrDefault();
                    if (firstFile != null)
                    {
                        var filePath = firstFile.Path.LocalPath;
                        
                        if (Directory.Exists(filePath))
                        {
                            // Dropped a folder
                            if (IsValidTeletextFolder(filePath))
                            {
                                await LoadTeletextFolder(filePath);
                                UpdateStatus($"Folder dropped and loaded: {Path.GetFileName(filePath)}");
                            }
                            else
                            {
                                UpdateStatus($"Folder contains no valid teletext files: {Path.GetFileName(filePath)}");
                            }
                        }
                        else if (IsValidTeletextFile(filePath))
                        {
                            var extension = Path.GetExtension(filePath).ToLowerInvariant();
                            
                            if (extension == ".t42")
                            {
                                await LoadT42Service(filePath);
                                UpdateStatus($"T42 file dropped and loaded: {Path.GetFileName(filePath)}");
                            }
                            else
                            {
                                await LoadSingleFileFromFolder(filePath);
                                UpdateStatus($"Teletext file dropped and loaded: {Path.GetFileName(filePath)}");
                            }
                        }
                        else
                        {
                            UpdateStatus($"Unsupported file type: {Path.GetExtension(filePath)}. Supported: .t42, .bin, .vbi, .pes");
                        }
                    }
                }
            }
        }

        // Thumbnail Generation Methods
        private Avalonia.Media.Imaging.Bitmap GeneratePageThumbnail(Page page)
        {
            try
            {
                if (renderer != null && page.Lines != null)
                {
                    var layers = renderer.Render(page, false, false);
                    if (layers.Foreground != null)
                    {
                        using (var ms = new MemoryStream())
                        {
                            layers.Foreground.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                            ms.Seek(0, SeekOrigin.Begin);
                            var fullBitmap = new Avalonia.Media.Imaging.Bitmap(ms);
                            
                            // Create thumbnail by resizing
                            return fullBitmap.CreateScaledBitmap(new Avalonia.PixelSize(ThumbnailWidth, ThumbnailHeight));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error generating thumbnail: {ex.Message}");
            }
            return null;
        }

        private void RefreshThumbnailsOnly()
        {
            try
            {
                UpdateStatus($"RefreshThumbnailsOnly called - clearing {ThumbnailPanel.Children.Count} existing thumbnails");
                ThumbnailPanel.Children.Clear();
                
                var pagesToShow = recoveredPages.Take(50).ToList(); // Limit to first 50 pages for performance
                
                UpdateStatus($"RefreshThumbnailsOnly - processing {pagesToShow.Count} pages from {recoveredPages.Count} total");
                
                if (pagesToShow.Count > 0)
                {
                    UpdateStatus($"Generating {pagesToShow.Count} thumbnails...");
                }
                
                // Add a test placeholder to verify the thumbnail panel is working
                if (pagesToShow.Count == 0)
                {
                    var testBorder = new Border
                    {
                        Background = Brushes.Red,
                        Width = 100,
                        Height = 100,
                        Margin = new Avalonia.Thickness(5),
                        Child = new TextBlock 
                        { 
                            Text = "No Pages\nLoaded", 
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                            Foreground = Brushes.White
                        }
                    };
                    ThumbnailPanel.Children.Add(testBorder);
                    UpdateStatus("Added test placeholder to thumbnail panel");
                    return;
                }
                
                int thumbnailCount = 0;
                foreach (var page in pagesToShow)
                {
                    try
                    {
                        var thumbnail = GeneratePageThumbnail(page);
                        if (thumbnail != null)
                        {
                            var pageNumber = GetPageNumber(page);
                            var subPageNumber = GetSubPageNumber(page);
                            var magazine = (page.Lines != null && page.Lines.Length > 0) ? page.Lines[0].Magazine : 0;
                            
                            var thumbnailContainer = new Border
                            {
                                BorderBrush = Brushes.Gray,
                                BorderThickness = new Avalonia.Thickness(1),
                                Margin = new Avalonia.Thickness(2),
                                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
                                Tag = page
                            };
                            
                            var stackPanel = new StackPanel
                            {
                                Orientation = Avalonia.Layout.Orientation.Vertical
                            };
                            
                            var image = new Image
                            {
                                Source = thumbnail,
                                Width = ThumbnailWidth,
                                Height = ThumbnailHeight,
                                Stretch = Avalonia.Media.Stretch.Uniform
                            };
                            
                            var label = new TextBlock
                            {
                                Text = $"P{magazine}{pageNumber:00}.{subPageNumber:00}",
                                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                                FontSize = 10,
                                Margin = new Avalonia.Thickness(2)
                            };
                            
                            stackPanel.Children.Add(image);
                            stackPanel.Children.Add(label);
                            thumbnailContainer.Child = stackPanel;
                            
                            // Add click handler
                            thumbnailContainer.Tapped += (s, e) =>
                            {
                                if (s is Border border && border.Tag is Page selectedPage)
                                {
                                    currentPage = selectedPage;
                                    DisplayPage(selectedPage);
                                    UpdatePageInfo(selectedPage);
                                    
                                    UpdateStatus($"Page selected from thumbnail: P{magazine}{pageNumber:00}.{subPageNumber:00}");
                                }
                            };
                            
                            ThumbnailPanel.Children.Add(thumbnailContainer);
                            thumbnailCount++;
                            UpdateStatus($"Added thumbnail {thumbnailCount}: P{magazine}{pageNumber:00}.{subPageNumber:00}");
                        }
                        else
                        {
                            UpdateStatus($"Failed to generate thumbnail for page {GetPageNumber(page)}");
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus($"Error creating thumbnail: {ex.Message}");
                    }
                }
                
                if (recoveredPages.Count > 50)
                {
                    UpdateStatus($"Generated {thumbnailCount} thumbnails (showing first 50 of {recoveredPages.Count} pages)");
                }
                else if (thumbnailCount > 0)
                {
                    UpdateStatus($"Generated {thumbnailCount} page thumbnails");
                }
                else
                {
                    UpdateStatus("No thumbnails were generated - check for errors above");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error in RefreshThumbnailsOnly: {ex.Message}");
            }
        }

        private void RefreshThumbnails()
        {
            try
            {
                UpdateStatus($"RefreshThumbnails called - clearing {ThumbnailPanel.Children.Count} existing thumbnails");
                ThumbnailPanel.Children.Clear();
                
                var pagesToShow = recoveredPages.Take(50).ToList(); // Limit to first 50 pages for performance
                
                UpdateStatus($"RefreshThumbnails - processing {pagesToShow.Count} pages from {recoveredPages.Count} total");
                
                if (pagesToShow.Count > 0)
                {
                    UpdateStatus($"Generating {pagesToShow.Count} thumbnails...");
                }
                
                // Add a test placeholder to verify the thumbnail panel is working
                if (pagesToShow.Count == 0)
                {
                    var testBorder = new Border
                    {
                        Background = Brushes.Red,
                        Width = 100,
                        Height = 100,
                        Margin = new Avalonia.Thickness(5),
                        Child = new TextBlock 
                        { 
                            Text = "No Pages\nLoaded", 
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                            Foreground = Brushes.White
                        }
                    };
                    ThumbnailPanel.Children.Add(testBorder);
                    UpdateStatus("Added test placeholder to thumbnail panel");
                    return;
                }
                
                int thumbnailCount = 0;
                foreach (var page in pagesToShow)
                {
                    try
                    {
                        var thumbnail = GeneratePageThumbnail(page);
                        if (thumbnail != null)
                        {
                            var pageNumber = GetPageNumber(page);
                            var subPageNumber = GetSubPageNumber(page);
                            var magazine = (page.Lines != null && page.Lines.Length > 0) ? page.Lines[0].Magazine : 0;
                            
                            var thumbnailContainer = new Border
                            {
                                BorderBrush = Brushes.Gray,
                                BorderThickness = new Avalonia.Thickness(1),
                                Margin = new Avalonia.Thickness(2),
                                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
                                Tag = page
                            };
                            
                            var stackPanel = new StackPanel
                            {
                                Orientation = Avalonia.Layout.Orientation.Vertical
                            };
                            
                            var image = new Image
                            {
                                Source = thumbnail,
                                Width = ThumbnailWidth,
                                Height = ThumbnailHeight,
                                Stretch = Avalonia.Media.Stretch.Uniform
                            };
                            
                            var label = new TextBlock
                            {
                                Text = $"P{magazine}{pageNumber:00}.{subPageNumber:00}",
                                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                                FontSize = 10,
                                Margin = new Avalonia.Thickness(2)
                            };
                            
                            stackPanel.Children.Add(image);
                            stackPanel.Children.Add(label);
                            thumbnailContainer.Child = stackPanel;
                            
                            // Add click handler
                            thumbnailContainer.Tapped += (s, e) =>
                            {
                                if (s is Border border && border.Tag is Page selectedPage)
                                {
                                    currentPage = selectedPage;
                                    DisplayPage(selectedPage);
                                    UpdatePageInfo(selectedPage);
                                    
                                    // Update selection in the list
                                    var listItem = RecoveredPagesList.Items.Cast<ListBoxItem>()
                                        .FirstOrDefault(item => item.Tag == selectedPage);
                                    if (listItem != null)
                                    {
                                        RecoveredPagesList.SelectedItem = listItem;
                                    }
                                    
                                    UpdateStatus($"Page selected from thumbnail: P{magazine}{pageNumber:00}.{subPageNumber:00}");
                                }
                            };
                            
                            ThumbnailPanel.Children.Add(thumbnailContainer);
                            thumbnailCount++;
                            UpdateStatus($"Added thumbnail {thumbnailCount}: P{magazine}{pageNumber:00}.{subPageNumber:00}");
                        }
                        else
                        {
                            UpdateStatus($"Failed to generate thumbnail for page {GetPageNumber(page)}");
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus($"Error creating thumbnail: {ex.Message}");
                    }
                }
                
                if (recoveredPages.Count > 50)
                {
                    UpdateStatus($"Generated {thumbnailCount} thumbnails (showing first 50 of {recoveredPages.Count} pages)");
                }
                else if (thumbnailCount > 0)
                {
                    UpdateStatus($"Generated {thumbnailCount} page thumbnails");
                }
                else
                {
                    UpdateStatus("No thumbnails were generated - check for errors above");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error in RefreshThumbnails: {ex.Message}");
            }
        }
    }
}