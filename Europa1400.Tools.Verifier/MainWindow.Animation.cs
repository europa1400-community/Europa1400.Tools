using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Europa1400.Tools.Animation;

namespace Europa1400.Tools.Verifier;

public partial class MainWindow
{
    private sealed record AnimationOption(string Label, BafAnimationClip? Clip);
    private string? _animationAssetPath;
    private bool _suppressAnimationChange;
    private bool _updatingTimeline;
    private GltfAnimationPreview? _animationPreview;
    private readonly DispatcherTimer _animationTimer = new(DispatcherPriority.Render)
    {
        Interval = TimeSpan.FromMilliseconds(16)
    };
    private readonly Stopwatch _animationClock = new();
    private double _animationTime;
    private double _lastAnimationTick;

    private async Task<ConverterChoice?> ResolveAnimationAsync(CatalogAsset item,
        ConverterChoice? converter, CancellationToken token)
    {
        var enabled = converter?.Id == "glb";
        AnimationPanel.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
        if (!enabled) return converter;
        if (_animationAssetPath != item.Asset.FilePath)
        {
            _suppressAnimationChange = true;
            AnimationCombo.ItemsSource = null;
            _suppressAnimationChange = false;
            var clips = await Task.Run(() => AnimationExports.Find(item.Asset, token), token);
            token.ThrowIfCancellationRequested();
            _animationAssetPath = item.Asset.FilePath;
            _suppressAnimationChange = true;
            AnimationCombo.ItemsSource = new[] { new AnimationOption("Rest pose", null) }
                .Concat(clips.Select(c => new AnimationOption(c.RelativePath, c))).ToArray();
            AnimationCombo.SelectedIndex = 0;
            AnimationCombo.IsEnabled = clips.Count > 0;
            AnimationCombo.ToolTip = clips.Count > 0
                ? $"{clips.Count} compatible animations. Selecting one exports an animated GLB."
                : "No matching animation family and vertex layout found.";
            _suppressAnimationChange = false;
        }
        return AnimationCombo.SelectedItem is AnimationOption { Clip: { } clip }
            ? AnimationExports.ForClip(clip) : converter;
    }

    private async void AnimationCombo_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_suppressAnimationChange && _selectedAsset != null)
            await LoadPreviewAsync(_selectedAsset, false);
    }

    private void AnimationTimeline_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_updatingTimeline || _animationPreview == null) return;
        SetAnimationTime(e.NewValue);
        _lastAnimationTick = _animationClock.Elapsed.TotalSeconds;
    }

    private void InPlace_OnChanged(object sender, RoutedEventArgs e)
    {
        if (_animationPreview != null) SetAnimationTime(_animationTime);
    }

    private void SetAnimationTime(double time)
    {
        if (_animationPreview == null || _animationPreview.Animations.Count == 0) return;
        var duration = _animationPreview.Duration(0);
        _animationTime = Math.Clamp(time, 0, duration);
        _animationPreview.SetTime(0, (float)_animationTime, InPlaceCheck.IsChecked == true);
        _updatingTimeline = true;
        AnimationTimeline.Value = _animationTime;
        _updatingTimeline = false;
        AnimationTimeText.Text = $"{_animationTime:F2} / {duration:F2} s";
        PreviewViewport.InvalidateVisual();
    }

    private void ToggleAnimation()
    {
        if (_animationTimer.IsEnabled) { PauseAnimation(); return; }
        if (_animationPreview == null) return;
        if (_animationTime >= _animationPreview.Duration(0)) SetAnimationTime(0);
        _animationClock.Restart();
        _lastAnimationTick = 0;
        SetPreviewRenderLoop(true);
        _animationTimer.Start();
        PlayButton.Content = "Pause";
    }

    private void PauseAnimation()
    {
        _animationTimer.Stop();
        _animationClock.Stop();
        SetPreviewRenderLoop(false);
        PlayButton.Content = "Play";
    }

    private void SetPreviewRenderLoop(bool enabled)
    {
        if (enabled)
        {
            PreviewRenderPulse.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, 0.02,
                TimeSpan.FromMilliseconds(500))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            });
            return;
        }

        PreviewRenderPulse.BeginAnimation(UIElement.OpacityProperty, null);
        PreviewRenderPulse.Opacity = 0;
    }

    private void AnimationTick(object? sender, EventArgs e)
    {
        if (_animationPreview == null) { PauseAnimation(); return; }
        var now = _animationClock.Elapsed.TotalSeconds;
        var speed = SpeedCombo.SelectedIndex switch { 0 => 0.25, 1 => 0.5, 3 => 2, _ => 1 };
        var time = _animationTime + (now - _lastAnimationTick) * speed;
        _lastAnimationTick = now;
        var duration = _animationPreview.Duration(0);
        if (LoopCheck.IsChecked == true)
        {
            var (start, end) = _animationPreview.LoopRange(0);
            if (end > start && time > end) time = start + (time - end) % (end - start);
            else if (duration == 0) PauseAnimation();
        }
        else if (time >= duration) { time = duration; PauseAnimation(); }
        SetAnimationTime(time);
    }
}
