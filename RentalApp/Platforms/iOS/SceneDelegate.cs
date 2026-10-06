using Foundation;

namespace RentalApp;

/// <summary>
/// UIKit 27 terminates apps that don't use the scene lifecycle, so the app declares a scene
/// delegate in Info.plist (UIApplicationSceneManifest) and MAUI's default one handles it.
/// </summary>
[Register("SceneDelegate")]
public class SceneDelegate : MauiUISceneDelegate
{
}
