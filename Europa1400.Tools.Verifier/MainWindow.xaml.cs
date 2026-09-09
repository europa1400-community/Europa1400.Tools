using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Europa1400.Tools.Pipeline;

namespace Europa1400.Tools.Verifier;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<CatalogAsset> _visibleAssets = [];
    private readonly PreviewService _previewService;
    private readonly VerifierCache _cache;
    private CancellationTokenSource? _previewCts;
    private CatalogAsset? _selectedAsset;

    private readonly Dictionary<string, string> _converterByFormat = [];
    private bool _suppressConverterChange;

    public MainWindow()
    {
        InitializeComponent();
        PreviewViewport.RotateGesture = new MouseGesture(MouseAction.LeftClick);
        PreviewViewport.RotateGesture2 = new MouseGesture(MouseAction.LeftClick);
        PreviewViewport.PanGesture = new MouseGesture(MouseAction.RightClick);
        PreviewViewport.PanGesture2 = new MouseGesture(MouseAction.MiddleClick);
        AssetList.ItemsSource = _visibleAssets;
        _cache = new VerifierCache(VerifierPaths.VerifierCacheRoot, VerifierPaths.LibraryFingerprint);
        _previewService = new PreviewService(_cache);
        Loaded += MainWindow_OnLoaded;
        _animationTimer.Tick += AnimationTick;
        Closed += (_, _) =>
        {
            _previewCts?.Cancel();
            _previewCts?.Dispose();
            StopPlayback();
        };
    }

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            VerifierPaths.ConfigurePipelineCache();
            StatusText.Text = $"Discovering assets in {VerifierPaths.GameDirectory}…";
            var gamePath = VerifierPaths.GameDirectory;

            var catalog = await Task.Run(() => AssetBrowser.Load(gamePath));
            CategoryList.ItemsSource = catalog.Categories;

            if (catalog.Categories.Count > 0)
                CategoryList.SelectedIndex = 0;

            var assetCount = catalog.Categories.Sum(c => c.Assets.Count);
            var warningText = catalog.Warnings.Count == 0
                ? ""
                : $"  ({catalog.Warnings.Count} discover warnings)";
            StatusText.Text = $"{assetCount} assets across {catalog.Categories.Count} categories.{warningText}";
        }
        catch (Exception ex)
        {
            StatusText.Text = ex.Message;
            MessageBox.Show(this, ex.Message, "Verifier", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CategoryList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _previewCts?.Cancel();
        _selectedAsset = null;
        _animationAssetPath = null;
        AnimationPanel.Visibility = Visibility.Collapsed;
        StopPlayback();
        HideLoading();
        ClearPreview();
        AssetTitle.Text = "Select an asset";
        AssetMeta.Text = "";
        JsonText.Text = "";
        ExportList.ItemsSource = null;

        if (CategoryList.SelectedItem is CatalogCategory category)
            BindConverters(category);

        RefreshAssetList();
    }

    private void SearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        RefreshAssetList();
    }

    private void RefreshAssetList()
    {
        _visibleAssets.Clear();
        if (CategoryList.SelectedItem is not CatalogCategory category)
            return;

        var query = SearchBox.Text?.Trim() ?? "";
        foreach (var asset in category.Assets)
        {
            if (query.Length == 0 ||
                asset.DisplayPath.Contains(query, StringComparison.OrdinalIgnoreCase))
                _visibleAssets.Add(asset);
        }
    }

    private void BindConverters(CatalogCategory category)
    {
        var converters = category.Format.Converters;

        _suppressConverterChange = true;
        ConverterCombo.ItemsSource = converters;
        ConverterCombo.IsEnabled = converters.Count > 1;
        ConverterPanel.Visibility = converters.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

        if (converters.Count > 0)
        {
            var selectedId = _converterByFormat.GetValueOrDefault(category.Format.Id);
            ConverterCombo.SelectedItem = converters.FirstOrDefault(c => c.Id == selectedId) ?? converters[0];
            _converterByFormat[category.Format.Id] = ((ConverterChoice)ConverterCombo.SelectedItem).Id;
        }
        else
        {
            ConverterCombo.SelectedItem = null;
        }

        _suppressConverterChange = false;
    }

    private ConverterChoice? SelectedConverter => ConverterCombo.SelectedItem as ConverterChoice;

    private async void ConverterCombo_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressConverterChange || SelectedConverter == null || _selectedAsset == null)
            return;

        _converterByFormat[_selectedAsset.Format.Id] = SelectedConverter.Id;
        await LoadPreviewAsync(_selectedAsset, force: false);
    }

    private async void AssetList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AssetList.SelectedItem is CatalogAsset asset)
            await LoadPreviewAsync(asset, force: false);
    }

    private async void ReconvertButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_selectedAsset != null)
            await LoadPreviewAsync(_selectedAsset, force: true);
    }

    private async Task LoadPreviewAsync(CatalogAsset item, bool force)
    {
        _previewCts?.Cancel();
        _previewCts?.Dispose();
        _previewCts = new CancellationTokenSource();
        var token = _previewCts.Token;
        _selectedAsset = item;
        var converter = SelectedConverter;

        StopPlayback();
        AssetTitle.Text = $"{item.Format.Category} / {item.DisplayPath}";
        AssetMeta.Text = item.Asset.FilePath;
        JsonText.Text = "";
        ExportList.ItemsSource = null;
        ClearPreview();

        ShowLoading("Preparing preview…");
        var progress = new Progress<PipelineProgress>(p =>
        {
            if (token.IsCancellationRequested) return;
            PreviewProgress.IsIndeterminate = p.Total <= 0;
            PreviewProgress.Maximum = Math.Max(p.Total, 1);
            PreviewProgress.Value = Math.Clamp(p.Current, 0, Math.Max(p.Total, 0));
            var label = string.IsNullOrEmpty(p.FileName)
                ? $"{p.Step} {p.Current}/{p.Total}"
                : $"{p.Step} {p.Current}/{p.Total}  {p.FileName}";
            PreviewProgressText.Text = label;
            StatusText.Text = label;
        });

        try
        {
            converter = await ResolveAnimationAsync(item, converter, token);
            token.ThrowIfCancellationRequested();
            if (!force && _cache.TryGet(item.Format, item.Asset, converter, out var cached) && cached != null)
            {
                HideLoading();
                ShowPreview(cached);
                return;
            }
            if (force) _cache.Invalidate(item.Format, item.Asset, converter);
            var preview = await Task.Run(
                () => _previewService.LoadAsync(item.Format, converter, item.Asset, progress, token), token);
            token.ThrowIfCancellationRequested();
            HideLoading();
            ShowPreview(preview);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            if (token.IsCancellationRequested) return;
            HideLoading();
            ShowPlaceholder(ex.Message);
            JsonText.Text = ex.ToString();
            StatusText.Text = "Failed.";
        }
    }

    private void ShowPreview(CachedPreview preview)
    {
        JsonText.Text = preview.Json;
        ExportList.ItemsSource = preview.Files;
        var preferred = preview.Files.FirstOrDefault(file => file.Kind == PreviewKind.Model)
                        ?? preview.Files.FirstOrDefault();
        if (preferred != null)
            ExportList.SelectedItem = preferred;
        else
            ShowPlaceholder("Decoded only — no converted exports for this format.");

        StatusText.Text = preview.Files.Count == 0
            ? "Decoded from source (cached as JSON)."
            : $"{preview.Files.Count} export(s). Cache invalidates when the library build or source file changes.";
    }

    private void ShowLoading(string message)
    {
        LoadingOverlay.Visibility = Visibility.Visible;
        PreviewProgress.IsIndeterminate = true;
        PreviewProgress.Value = 0;
        PreviewProgressText.Text = message;
        PreviewPlaceholder.Visibility = Visibility.Collapsed;
    }

    private void HideLoading()
    {
        LoadingOverlay.Visibility = Visibility.Collapsed;
        PreviewProgress.IsIndeterminate = false;
    }

    private void ExportList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ExportList.SelectedItem is PreviewFile file)
            ShowFile(file);
    }

    private void PlayButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_animationPreview?.Animations.Count > 0) { ToggleAnimation(); return; }
        if (ExportList.SelectedItem is not PreviewFile file || file.Kind != PreviewKind.Audio)
            return;

        PreviewMedia.Source = new Uri(file.CachePath);
        PreviewMedia.Play();
    }

    private void StopButton_OnClick(object sender, RoutedEventArgs e)
    {
        StopPlayback();
    }

    private void ShowFile(PreviewFile file)
    {
        StopPlayback();
        ClearPreview();
        PlayButton.IsEnabled = file.Kind == PreviewKind.Audio;

        switch (file.Kind)
        {
            case PreviewKind.Image:
                PreviewImage.Source = LoadBitmap(file.CachePath);
                PreviewImage.Visibility = Visibility.Visible;
                break;
            case PreviewKind.Audio:
                PreviewMedia.Source = new Uri(file.CachePath);
                PreviewMedia.Visibility = Visibility.Visible;
                PreviewPlaceholder.Text = $"{file.RelativePath}  ({file.ByteLength} bytes)  — press Play";
                PreviewPlaceholder.Visibility = Visibility.Visible;
                break;
            case PreviewKind.Model:
                ShowModel(file.CachePath);
                break;
            case PreviewKind.Text:
                PreviewText.Text = File.ReadAllText(file.CachePath);
                PreviewText.Visibility = Visibility.Visible;
                break;
            default:
                PreviewPlaceholder.Text = $"{file.RelativePath}\n{file.ByteLength} bytes";
                PreviewPlaceholder.Visibility = Visibility.Visible;
                break;
        }
    }

    private void ShowModel(string path)
    {
        try
        {
            if (Path.GetExtension(path).Equals(".glb", StringComparison.OrdinalIgnoreCase) ||
                Path.GetExtension(path).Equals(".gltf", StringComparison.OrdinalIgnoreCase))
            {
                _animationPreview = ModelPreviewLoader.LoadAnimated(path);
                PreviewModel.Content = _animationPreview.Model;
                if (_animationPreview.Animations.Count > 0)
                {
                    AnimationPlaybackPanel.Visibility = Visibility.Visible;
                    AnimationTimeline.Maximum = Math.Max(_animationPreview.Duration(0), 0.001);
                    SetAnimationTime(0);
                    PlayButton.IsEnabled = true;
                }
            }
            else PreviewModel.Content = ModelPreviewLoader.Load(path);
            PreviewViewport.Visibility = Visibility.Visible;
            PreviewHint.Visibility = Visibility.Visible;
            ResetViewButton.IsEnabled = true;
            PreviewViewport.Dispatcher.BeginInvoke(() => PreviewViewport.ZoomExtents(0), DispatcherPriority.Loaded);
        }
        catch (Exception ex)
        {
            ShowPlaceholder($"Could not load 3D preview.\n{ex.Message}");
        }
    }

    private void ResetViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (PreviewViewport.Visibility != Visibility.Visible)
            return;

        PreviewViewport.ResetCamera();
        PreviewViewport.ZoomExtents(0.25);
    }

    private static BitmapImage LoadBitmap(string path)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri(path);
        image.EndInit();
        image.Freeze();
        return image;
    }

    private void ClearPreview()
    {
        PauseAnimation();
        _animationPreview = null;
        AnimationPlaybackPanel.Visibility = Visibility.Collapsed;
        PreviewImage.Source = null;
        PreviewImage.Visibility = Visibility.Collapsed;
        PreviewMedia.Visibility = Visibility.Collapsed;
        PreviewText.Visibility = Visibility.Collapsed;
        PreviewText.Text = "";
        PreviewPlaceholder.Visibility = Visibility.Collapsed;
        PreviewViewport.Visibility = Visibility.Collapsed;
        PreviewHint.Visibility = Visibility.Collapsed;
        PreviewModel.Content = null;
        ResetViewButton.IsEnabled = false;
        PlayButton.IsEnabled = false;
    }

    private void ShowPlaceholder(string text)
    {
        ClearPreview();
        PreviewPlaceholder.Text = text;
        PreviewPlaceholder.Visibility = Visibility.Visible;
    }

    private void StopPlayback()
    {
        PauseAnimation();
        SetAnimationTime(0);
        PreviewMedia.Stop();
        PreviewMedia.Source = null;
    }
}
