using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Europa1400.Tools.Animation;
using Xunit;

namespace Europa1400.Tools.Verifier.Tests;

public class MainWindowAnimationTests
{
    [Fact]
    public void RealWorker_SelectExportPlayPauseSeekStopAndRestPose()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            MainWindow? window = null;
            try
            {
                SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());
                var app = new App();
                app.InitializeComponent();
                window = new MainWindow();
                var catalog = AssetBrowser.Load(VerifierPaths.GameDirectory);
                window.CategoryList.ItemsSource = catalog.Categories;
                var category = catalog.Categories.Single(c => c.Format.Category == "Models");
                window.CategoryList.SelectedItem = category;
                var asset = category.Assets.Single(a => a.DisplayPath.Replace('\\', '/')
                    .EndsWith("/Arbeiter_MANN.bgf", StringComparison.OrdinalIgnoreCase));
                window.AssetList.SelectedItem = asset;
                PumpUntil(() => window.PreviewModel.Content != null && window.AnimationCombo.Items.Count > 1);
                var walk = window.AnimationCombo.Items.Cast<object>().Single(option =>
                    option.GetType().GetProperty("Clip")!.GetValue(option) is BafAnimationClip clip &&
                    clip.Name == "gehen_MANN");
                window.AnimationCombo.SelectedItem = walk;
                PumpUntil(() => window.AnimationPlaybackPanel.Visibility == Visibility.Visible && window.PlayButton.IsEnabled);
                var export = (PreviewFile)window.ExportList.SelectedItem;
                Assert.EndsWith("--gehen_MANN.glb", export.CachePath);
                Assert.True(File.Exists(export.CachePath));
                Assert.Equal(new System.Windows.Media.Media3D.Vector3D(0, 1, 0), window.PreviewViewport.ModelUpDirection);

                window.PlayButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                PumpUntil(() => window.AnimationTimeline.Value > .05);
                window.PlayButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.Equal("Play", window.PlayButton.Content);
                window.AnimationTimeline.Value = .1;
                Assert.Equal(.1, window.AnimationTimeline.Value, 5);
                window.InPlaceCheck.IsChecked = false;
                window.InPlaceCheck.IsChecked = true;

                window.LoopCheck.IsChecked = false;
                window.AnimationTimeline.Value = window.AnimationTimeline.Maximum - .01;
                window.PlayButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                PumpUntil(() => Equals(window.PlayButton.Content, "Play"));
                Assert.Equal(window.AnimationTimeline.Maximum, window.AnimationTimeline.Value, 5);
                typeof(MainWindow).GetMethod("StopPlayback", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(window, null);
                Assert.Equal(0, window.AnimationTimeline.Value);

                // Replace a pending selection immediately; stale conversion must not restore playback.
                window.AnimationCombo.SelectedIndex = 1;
                window.AnimationCombo.SelectedIndex = 0;
                PumpUntil(() => window.PreviewModel.Content != null && window.LoadingOverlay.Visibility != Visibility.Visible);
                Assert.False(window.PlayButton.IsEnabled);
                Assert.Equal(Visibility.Collapsed, window.AnimationPlaybackPanel.Visibility);
            }
            catch (Exception ex) { failure = ex; }
            finally { window?.Close(); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure != null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
    }

    private static void PumpUntil(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(30);
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(20) };
        timer.Tick += (_, _) => { if (condition() || DateTime.UtcNow > deadline) frame.Continue = false; };
        timer.Start();
        Dispatcher.PushFrame(frame);
        timer.Stop();
        Assert.True(condition(), "Verifier did not reach the expected UI state.");
    }
}
