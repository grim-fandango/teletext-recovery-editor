using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
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
        private Page? currentPage;
        private string currentFontName = "Mullard";
        private bool showBorders = true;
        private List<Page> recoveredPages = new List<Page>();

        public MainWindow()
        {
            InitializeComponent();
            InitializeRenderer();
            UpdateStatus("Teletext Recovery Editor initialized.");
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
            UpdateStatus("Save T42 Service - Feature to be implemented");
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
                        await LoadSingleFileFromFolder(filePath);
                    }
                }
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshDisplay();
            UpdateStatus("Display refreshed");
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
                        
                        if (pageCount % 100 == 0)
                        {
                            UpdateStatus($"Loading pages... {pageCount} loaded");
                        }
                    }
                }
                
                RefreshRecoveredPagesList();
                UpdateStatus($"Loaded {pageCount} pages from T42 service");
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
                recoveredPages.Clear();
                RecoveredPagesList.Items.Clear();
                
                var files = Directory.GetFiles(folderPath, "*.*")
                    .Where(f => f.EndsWith(".bin", StringComparison.OrdinalIgnoreCase) ||
                               f.EndsWith(".vbi", StringComparison.OrdinalIgnoreCase) ||
                               f.EndsWith(".pes", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(f => f)
                    .ToArray();

                if (files.Length == 0)
                {
                    UpdateStatus("No teletext binary files found in folder");
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
                UpdateStatus($"Found {files.Length} binary files. Select a file to load.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error loading folder: {ex.Message}");
            }
            await Task.Delay(1);
        }

        // Helper Methods
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
                    
                    while(service.Position < service.Length && pageCount < maxPages)
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
                        // Break if we are at the end of the stream
                        if (service.Position >= service.Length -1)
                            break;
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

                    // Get just the first page from the file
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
    }
}