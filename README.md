# UEBS2 Modding Tools v. 0.3Plus

This is my personal edition of the UEBS2 Modding setup. It is the [original 0.3 file](https://steamcommunity.com/workshop/discussions/18446744073709551615/3833172420306495031/?appid=1468720) plus these additions:
* a new "Export + Test in UEBS2" feature that automatically copies the resulting files into your "ModCharacterTest" folder (requires you to enter your GameInstallationPath; replaces existing files there) and launches the game via Steam
* diagnosis and suggestions for about a dozen additional errors in a proper error message popup
* a convenience feature `Exchange MeshRenderers` exchanges all instances of `MeshRenderer` in your prefab and replaces them by `SkinnedMeshRenderer` (saves you some click work)
* a convenience feature `Reset AllMeshes` that fills in all entries for the `allMeshes` entry (saves you some more click work)

As a fallback, you can still use the unchanged "Export Character" function. This, however, does not contain the additional error checks.

## Security Note

The script performs the following actions on your computer
* Copying and overriding files
* Editing prefabs and / or scenes in your Unity instance.

The author of this file is not liable to any damage this script may inflict malfunctioning. Only use this script at your own risk.
